namespace ConcertJournal.Services
{
    public static class EventBus
    {
        public static event Action? ConcertCreated;
        public static event Action? ConcertUpdated;

        public static void OnConcertCreated() => ConcertCreated?.Invoke();
        public static void OnConcertUpdated() => ConcertUpdated?.Invoke();
    }
}
