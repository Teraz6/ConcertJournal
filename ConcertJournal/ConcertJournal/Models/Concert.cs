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
        public string? Notes { get; set; }
        public string? MediaPaths { get; set; }
    }
}
