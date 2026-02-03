using Microsoft.EntityFrameworkCore;
using OurRadio.Models;

namespace OurRadio.Data
{
    public class RadioService
    {
        private readonly AppDbContext _context;

        public RadioService(AppDbContext context)
        {
            _context = context;
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
            _context.Radios.Add(radio);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRadioAsync(Radio radio)
        {
            _context.Radios.Update(radio);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRadioAsync(int id)
        {
            var radio = await _context.Radios.FindAsync(id);
            if (radio != null)
            {
                _context.Radios.Remove(radio);
                await _context.SaveChangesAsync();
            }
        }
    }
}