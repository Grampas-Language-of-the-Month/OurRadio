namespace OurRadio.Models
{
    public class Song
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }

        public List<RadioSong> RadioSongs { get; set; } = new();
    }
}