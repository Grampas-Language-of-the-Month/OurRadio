using Microsoft.EntityFrameworkCore;
using OurRadio.Models;

namespace OurRadio.Data
{
    public class RadioService
    {
        private readonly AppDbContext _context;
        private readonly AuthGuard _authGuard;

        public RadioService(AppDbContext context, AuthGuard authGuard)
        {
            _context = context;
            _authGuard = authGuard;
        }

        public async Task<List<Radio>> GetAllRadiosAsync()
        {
            return await _context.Radios.ToListAsync();
        }

        public async Task<Radio?> GetRadioByIdAsync(int id)
        {
            return await _context.Radios.FindAsync(id);
        }

        public async Task AddRadioAsync(Radio radio)
        {
            await _authGuard.EnsureAuthenticatedAsync();
            _context.Radios.Add(radio);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRadioAsync(Radio radio)
        {
            await _authGuard.EnsureAuthenticatedAsync();
            _context.Radios.Update(radio);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRadioAsync(int id)
        {
            await _authGuard.EnsureAuthenticatedAsync();
            var radio = await _context.Radios.FindAsync(id);
            if (radio != null)
            {
                _context.Radios.Remove(radio);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Song>> GetSongsForRadioAsync(int radioId)
        {
            return await _context.RadioSongs
                .Where(rs => rs.RadioId == radioId)
                .Include(rs => rs.Song)
                .Select(rs => rs.Song)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddSongToRadioAsync(int radioId, int songId)
        {
            await _authGuard.EnsureAuthenticatedAsync();
            var exists = await _context.RadioSongs
                .AnyAsync(rs => rs.RadioId == radioId && rs.SongId == songId);

            if (exists)
            {
                throw new InvalidOperationException($"Song {songId} is already added to the radio {radioId}.");
            }

            var radioSong = new RadioSong
            {
                RadioId = radioId,
                SongId = songId
            };
            _context.RadioSongs.Add(radioSong);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsSongInRadioAsync(int radioId, int songId)
        {
            return await _context.RadioSongs
                .AsNoTracking()
                .AnyAsync(rs => rs.RadioId == radioId && rs.SongId == songId);
        }
    }
}