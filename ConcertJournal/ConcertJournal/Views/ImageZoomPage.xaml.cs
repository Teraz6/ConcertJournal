namespace ConcertJournal.Views;

public partial class ImageZoomPage : ContentPage
{
    public ImageZoomPage(string imagePath)
    {
        InitializeComponent();
        ZoomedImage.Source = imagePath;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}