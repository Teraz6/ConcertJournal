using SQLite;

namespace ConcertJournal.Models
{
    public class Concert
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? BandName { get; set; }
        public string? Venue { get; set; }
        public string? Location { get; set; }
        public DateTime? Date { get; set; }
        public string? Notes { get; set; }

        //For future update
        //public string? PhotoPath { get; set; }
        //public string? VideoPath { get; set; }

    }
}
