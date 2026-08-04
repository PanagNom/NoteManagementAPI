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
        internal DbSet<RefreshToken> RefreshTokens { get; set; }

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

            builder.Entity<RefreshToken>()
                .HasOne(token => token.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RefreshToken>()
                .HasIndex(token => token.TokenHash)
                .IsUnique();

            builder.Entity<RefreshToken>()
                .HasIndex(token => new { token.UserId, token.FamilyId });

            builder.Entity<RefreshToken>()
                .Property(token => token.RowVersion)
                .IsRowVersion();
        }
    }
}
