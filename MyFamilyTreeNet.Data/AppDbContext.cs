using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Family> Families { get; set; } = null!;
        public DbSet<FamilyMember> FamilyMembers { get; set; } = null!;
        public DbSet<Relationship> Relationships { get; set; } = null!;
        public DbSet<Photo> Photos { get; set; } = null!;
        public DbSet<Story> Stories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.MiddleName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<Family>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.HasOne(e => e.CreatedBy)
                      .WithMany(u => u.CreatedFamilies)
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<FamilyMember>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.MiddleName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.PlaceOfBirth).HasMaxLength(100);
                entity.Property(e => e.PlaceOfDeath).HasMaxLength(100);
                entity.Property(e => e.Biography).HasMaxLength(2000);

                entity.HasOne(e => e.Family)
                      .WithMany(f => f.FamilyMembers)
                      .HasForeignKey(e => e.FamilyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AddedBy)
                      .WithMany()
                      .HasForeignKey(e => e.AddedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Relationship>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.PrimaryMember)
                      .WithMany(m => m.RelationshipsAsPrimary)
                      .HasForeignKey(e => e.PrimaryMemberId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RelatedMember)
                      .WithMany(m => m.RelationshipsAsRelated)
                      .HasForeignKey(e => e.RelatedMemberId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedBy)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Ensure no self-relationships
                entity.ToTable(t => t.HasCheckConstraint("CK_Relationship_NoSelfReference",
                    "[PrimaryMemberId] <> [RelatedMemberId]"));
            });

            modelBuilder.Entity<Photo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ImageUrl).IsRequired();
                entity.Property(e => e.Location).HasMaxLength(100);

                entity.HasOne(e => e.Family)
                      .WithMany(f => f.Photos)
                      .HasForeignKey(e => e.FamilyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.UploadedBy)
                      .WithMany()
                      .HasForeignKey(e => e.UploadedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Story>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Content).IsRequired();

                entity.HasOne(e => e.Family)
                      .WithMany(f => f.Stories)
                      .HasForeignKey(e => e.FamilyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Author)
                      .WithMany(u => u.Stories)
                      .HasForeignKey(e => e.AuthorUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });



        }
    }
}