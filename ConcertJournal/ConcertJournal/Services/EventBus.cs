namespace ConcertJournal.Services
{
    public static class EventBus
    {
        public static event Action? ConcertCreated;

        public static void OnConcertCreated() => ConcertCreated?.Invoke();
    }
}
