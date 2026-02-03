using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using OurRadio.Models;

namespace OurRadio.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Song> Songs { get; set; }
        public DbSet<Radio> Radios { get; set; }
        public DbSet<RadioSong> RadioSongs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<RadioSong>()
                .HasKey(rs => new { rs.RadioId, rs.SongId });

            modelBuilder.Entity<RadioSong>()
                .HasOne(rs => rs.Radio)
                .WithMany(r => r.RadioSongs)
                .HasForeignKey(rs => rs.RadioId);

            modelBuilder.Entity<RadioSong>()
                .HasOne(rs => rs.Song)
                .WithMany(s => s.RadioSongs)
                .HasForeignKey(rs => rs.SongId);
        }
    }
}