namespace ConcertJournal.ServiceInterface
{
    public interface IImageService
    {
        Task<IEnumerable<string>> PickImagesAsync(string title = "Select Images");
    }
}
