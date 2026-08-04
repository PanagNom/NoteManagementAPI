using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NoteManagementAPI.Models;

namespace NoteManagementAPI.Infrastructure
{
    public class NoteDbContext : IdentityDbContext<ApplicationUser>
    {
        public NoteDbContext(DbContextOptions<NoteDbContext> options) : base(options)
        {
        }

        public DbSet<Note> Notes { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Note>()
                .HasOne(note => note.Owner)
                .WithMany(user => user.Notes)
                .HasForeignKey(note => note.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Tag>()
                .HasOne(tag => tag.Owner)
                .WithMany(user => user.Tags)
                .HasForeignKey(tag => tag.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Tag>()
                .HasIndex(tag => new { tag.OwnerUserId, tag.Name })
                .IsUnique();
        }
    }
}
