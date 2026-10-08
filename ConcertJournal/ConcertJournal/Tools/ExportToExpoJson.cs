using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SQLite;
using ConcertJournal.Models;

namespace ConcertJournal.Tools
{
    // Simple exporter to create an Expo-compatible JSON export from the existing ConcertJournal DB.
    // Usage: call Exporter.ExportAsync() from a debug command or a small UI button. Defaults:
    // - DB: Path.Combine(FileSystem.AppDataDirectory, "concertjournal.db")
    // - OutDir: FileSystem.AppDataDirectory
    // - Delimiter: ','
    public static class Exporter
    {
        public static async Task<string> ExportAsync(string? sourceDbPath = null, string? outDir = null, char delimiter = ',')
        {
            sourceDbPath ??= Path.Combine(FileSystem.AppDataDirectory, "concertjournal.db");
            outDir ??= FileSystem.AppDataDirectory;

            if (!File.Exists(sourceDbPath))
                throw new FileNotFoundException("Source database not found.", sourceDbPath);

            // Use synchronous connection from sqlite-net
            using var conn = new SQLiteConnection(sourceDbPath);

            // Read concerts
            var concerts = conn.Table<Concert>().ToList();

            // Prepare mappings
            var eventMap = new Dictionary<int, string>(); // old int id -> new guid
            var performerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // name -> guid
            var venueMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // name -> guid
            var mediaMap = new Dictionary<string, string>(); // composite key -> guid

            var eventsOut = new List<EventDto>();
            var performersOut = new List<PerformerDto>();
            var venuesOut = new List<VenueDto>();
            var eventPerformersOut = new List<EventPerformerDto>();
            var eventMediaOut = new List<EventMediaDto>();

            foreach (var c in concerts)
            {
                var newEventId = Guid.NewGuid().ToString();
                eventMap[c.Id] = newEventId;

                string? venueId = null;
                if (!string.IsNullOrWhiteSpace(c.Venue))
                {
                    var vname = c.Venue!.Trim();
                    if (!venueMap.TryGetValue(vname, out var vid))
                    {
                        vid = Guid.NewGuid().ToString();
                        venueMap[vname] = vid;
                        venuesOut.Add(new VenueDto
                        {
                            Id = vid,
                            OldKey = vname,
                            UserId = null,
                            Name = vname,
                            City = c.City,
                            Country = c.Country,
                            CountryCode = null,
                            CreatedAt = null
                        });
                    }
                    venueId = venueMap[vname];
                }

                // Event
                var evt = new EventDto
                {
                    Id = newEventId,
                    OldId = c.Id,
                    UserId = null,
                    ParentId = null,
                    VenueId = venueId,
                    EventType = "concert",
                    EventTitle = c.EventTitle,
                    Date = c.Date?.ToString("o"),
                    EndDate = null,
                    Rating = c.Rating,
                    Notes = c.Notes,
                    Headliner = null,
                    IsSynced = 0,
                    CreatedAt = null,
                    Price = null
                };

                eventsOut.Add(evt);

                // Performers
                if (!string.IsNullOrWhiteSpace(c.Performers))
                {
                    var parts = c.Performers!.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    foreach (var p in parts)
                    {
                        if (!performerMap.TryGetValue(p, out var pid))
                        {
                            pid = Guid.NewGuid().ToString();
                            performerMap[p] = pid;
                            performersOut.Add(new PerformerDto
                            {
                                Id = pid,
                                OldKey = p,
                                UserId = null,
                                Name = p,
                                CreatedAt = null
                            });
                        }

                        eventPerformersOut.Add(new EventPerformerDto
                        {
                            EventId = newEventId,
                            PerformerId = performerMap[p]
                        });
                    }
                }

                // Media paths
                if (!string.IsNullOrWhiteSpace(c.MediaPaths))
                {
                    var mparts = c.MediaPaths!.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct()
                        .ToList();

                    foreach (var m in mparts)
                    {
                        var key = $"{c.Id}|{m}";
                        if (!mediaMap.TryGetValue(key, out var mid))
                        {
                            mid = Guid.NewGuid().ToString();
                            mediaMap[key] = mid;
                            eventMediaOut.Add(new EventMediaDto
                            {
                                Id = mid,
                                EventId = newEventId,
                                FilePath = m,
                                MediaType = InferMediaTypeFromPath(m),
                                CreatedAt = null
                            });
                        }
                    }
                }
            }

            // Build mapping object
            var mapping = new Dictionary<string, object>
            {
                ["events"] = eventMap.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
                ["performers"] = performerMap.ToDictionary(kv => kv.Key, kv => kv.Value),
                ["venues"] = venueMap.ToDictionary(kv => kv.Key, kv => kv.Value),
                ["media"] = mediaMap.ToDictionary(kv => kv.Key, kv => kv.Value)
            };

            var metadata = new
            {
                exportVersion = "1.0",
                exportedAt = DateTime.UtcNow.ToString("o"),
                sourceDbPath = sourceDbPath,
                delimiter = delimiter.ToString()
            };

            var exportObj = new
            {
                metadata,
                users = new List<object>(), // source app has no users table; importer will set user_id
                events = eventsOut,
                performers = performersOut,
                genres = new List<object>(),
                venues = venuesOut,
                event_media = eventMediaOut,
                event_performers = eventPerformersOut,
                performer_genres = new List<object>(),
                user_achievements = new List<object>()
            };

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var exportFile = Path.Combine(outDir, $"export-{timestamp}.json");
            var mappingFile = Path.Combine(outDir, $"id-mapping-{timestamp}.json");

            var opts = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            // Write files
            await File.WriteAllTextAsync(exportFile, JsonSerializer.Serialize(exportObj, opts));
            await File.WriteAllTextAsync(mappingFile, JsonSerializer.Serialize(mapping, opts));

            return exportFile;
        }

        private static string InferMediaTypeFromPath(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant() ?? string.Empty;
            return ext switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "image",
                ".mp4" or ".mov" or ".m4v" or ".avi" => "video",
                ".mp3" or ".wav" or ".m4a" or ".aac" => "audio",
                _ => "file"
            };
        }

        // DTOs with explicit json property names matching target schema
        private class EventDto
        {
            [JsonPropertyName("id")] public string Id { get; set; } = null!;
            [JsonPropertyName("oldId")] public int OldId { get; set; }
            [JsonPropertyName("user_id")] public string? UserId { get; set; }
            [JsonPropertyName("parent_id")] public string? ParentId { get; set; }
            [JsonPropertyName("venue_id")] public string? VenueId { get; set; }
            [JsonPropertyName("eventType")] public string? EventType { get; set; }
            [JsonPropertyName("eventTitle")] public string? EventTitle { get; set; }
            [JsonPropertyName("date")] public string? Date { get; set; }
            [JsonPropertyName("endDate")] public string? EndDate { get; set; }
            [JsonPropertyName("rating")] public double Rating { get; set; }
            [JsonPropertyName("notes")] public string? Notes { get; set; }
            [JsonPropertyName("headliner")] public string? Headliner { get; set; }
            [JsonPropertyName("is_synced")] public int IsSynced { get; set; }
            [JsonPropertyName("createdAt")] public string? CreatedAt { get; set; }
            [JsonPropertyName("price")] public double? Price { get; set; }
        }

        private class PerformerDto
        {
            [JsonPropertyName("id")] public string Id { get; set; } = null!;
            [JsonPropertyName("oldKey")] public string? OldKey { get; set; }
            [JsonPropertyName("user_id")] public string? UserId { get; set; }
            [JsonPropertyName("name")] public string? Name { get; set; }
            [JsonPropertyName("createdAt")] public string? CreatedAt { get; set; }
        }

        private class VenueDto
        {
            [JsonPropertyName("id")] public string Id { get; set; } = null!;
            [JsonPropertyName("oldKey")] public string? OldKey { get; set; }
            [JsonPropertyName("user_id")] public string? UserId { get; set; }
            [JsonPropertyName("name")] public string? Name { get; set; }
            [JsonPropertyName("city")] public string? City { get; set; }
            [JsonPropertyName("country")] public string? Country { get; set; }
            [JsonPropertyName("countryCode")] public string? CountryCode { get; set; }
            [JsonPropertyName("createdAt")] public string? CreatedAt { get; set; }
        }

        private class EventMediaDto
        {
            [JsonPropertyName("id")] public string Id { get; set; } = null!;
            [JsonPropertyName("event_id")] public string EventId { get; set; } = null!;
            [JsonPropertyName("filePath")] public string? FilePath { get; set; }
            [JsonPropertyName("mediaType")] public string? MediaType { get; set; }
            [JsonPropertyName("createdAt")] public string? CreatedAt { get; set; }
        }

        private class EventPerformerDto
        {
            [JsonPropertyName("event_id")] public string EventId { get; set; } = null!;
            [JsonPropertyName("performer_id")] public string PerformerId { get; set; } = null!;
        }
    }
}
