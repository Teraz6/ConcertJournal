using System.Text.Json.Serialization;

namespace ConcertJournal.Models
{
    public class UpdateInfo
    {
        [JsonPropertyName("version")]
        public required string Version { get; set; }

        [JsonPropertyName("downloadUrl")]
        public required string DownloadUrl { get; set; }
    }
}
