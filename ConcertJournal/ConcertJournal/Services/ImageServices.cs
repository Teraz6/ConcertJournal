using ConcertJournal.ServiceInterface;

namespace ConcertJournal.Services
{
    public class ImageServices : IImageService
    {
        public async Task<IEnumerable<string>> PickImagesAsync(string title)
        {
            try
            {
                var results = await FilePicker.PickMultipleAsync(new PickOptions
                {
                    PickerTitle = title,
                    FileTypes = FilePickerFileType.Images
                });

                if (results == null)
                    return Enumerable.Empty<string>();

                return results.Select(f => f!.FullPath);
            }
            catch (Exception)
            {
                // In a professional app, log the error to a service like AppCenter here
                return [];
            }
        }
    }
}
