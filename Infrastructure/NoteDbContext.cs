using Microsoft.EntityFrameworkCore;
using NoteManagementAPI.Models;

namespace NoteManagementAPI.Infrastructure
{
    public class NoteDbContext : DbContext
    {
        public NoteDbContext(DbContextOptions<NoteDbContext> options): base(options)
        {

        }

        public DbSet<Note> Notes { get; set; }
    }
}
