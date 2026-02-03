namespace OurRadio.Models
{
    public class Radio
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public List<RadioSong> RadioSongs { get; set; } = new List<RadioSong>();
    }
}