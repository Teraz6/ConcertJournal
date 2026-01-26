using System.Text.Json.Serialization;

namespace ConcertJournal.Models
{
    public class AudioDbInfo
    {
        public record AudioDbResponse(List<ArtistInfo> Artists);

        public record ArtistInfo(
            [property: JsonPropertyName("idArtist")] string IdArtist,
            [property: JsonPropertyName("strArtist")] string StrArtist,
            [property: JsonPropertyName("strBiographyEN")] string StrBiographyEN,
            [property: JsonPropertyName("strArstistFanart")] string StrArtistFanart, //Background image
            [property: JsonPropertyName("strArtistThumb")] string StrArtistThumb,  //Square profile pic
            [property: JsonPropertyName("strArtistBanner")] string StrArtistBanner //Banner image
        );
    }
}
