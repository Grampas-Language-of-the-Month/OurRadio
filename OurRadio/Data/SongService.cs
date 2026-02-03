using Microsoft.EntityFrameworkCore;
using OurRadio.Models;

namespace OurRadio.Data
{
    public class SongService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<SongService> _logger;

        public SongService(AppDbContext context, IWebHostEnvironment environment, ILogger<SongService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<List<Song>> GetAllSongsAsync()
        {
            return await _context.Songs.ToListAsync();
        }

        public async Task<Song?> GetSongByIdAsync(int id)
        {
            return await _context.Songs.FindAsync(id);
        }

        public async Task AddSongAsync(Song song)
        {
            _context.Songs.Add(song);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSongAsync(Song song)
        {
            _context.Songs.Update(song);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSongAsync(int id)
        {
            var song = await _context.Songs.FindAsync(id);

            // delete song file from storage if needed
            if (song != null && !string.IsNullOrEmpty(song.Filename))
            {
                var path = Path.Combine(_environment.ContentRootPath,
                    _environment.EnvironmentName, "unsafe_uploads",
                    song.Filename);

                if (File.Exists(path))
                {
                    File.Delete(path);
                    _logger.LogInformation($"Deleted song file: {path}");
                }
            }

            if (song != null)
            {
                _context.Songs.Remove(song);
                await _context.SaveChangesAsync();
            }
        }
    }
}