using SQLite;

namespace ConcertJournal.Models
{
    public class Concert
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? EventTitle { get; set; }
        public string? Performers { get; set; }
        public string? Venue { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public DateTime? Date { get; set; }
        public double Rating { get; set; }
        public string? Notes { get; set; }
        public string? MediaPaths { get; set; }

        public string Location => $"{City}, {Country}";


        // Display performers with truncation if more than 3
        public string DisplayPerformers
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Performers))
                    return string.Empty;

                var list = Performers
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToList();

                if (list.Count <= 3)
                    return string.Join(", ", list);

                string firstThree = string.Join(", ", list.Take(3));
                int remaining = list.Count - 3;

                return $"{firstThree} + {remaining} more";
            }
        }
    }
}
