namespace OurRadio.Models
{
    public class RadioSong
    {
        public int RadioId { get; set; }
        public Radio Radio { get; set; } = null!;

        public int SongId { get; set; }
        public Song Song { get; set; } = null!;
    }
}