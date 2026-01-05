namespace ConcertJournal.ViewModels
{
    public partial class PerformerViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public string CountText => $"{Count} concerts";
    }
}
