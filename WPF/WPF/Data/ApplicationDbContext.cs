using System.Data.Entity;
using WPF.Models;

namespace WPF.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("name=DefaultConnection")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Image> Images { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Favorite>()
                .HasKey(f => new { f.UserID, f.FavItemID });

            modelBuilder.Entity<Favorite>()
                .HasRequired(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Favorite>()
                .HasRequired(f => f.FavItem)
                .WithMany(i => i.Favorites)
                .HasForeignKey(f => f.FavItemID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Review>()
                .HasRequired(r => r.User)
                .WithMany(u => u.ReviewsReceived)
                .HasForeignKey(r => r.UserID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Report>()
                .HasRequired(r => r.Reporter)
                .WithMany(u => u.ReportsSent)
                .HasForeignKey(r => r.ReporterID)
                .WillCascadeOnDelete(false); 

            modelBuilder.Entity<Report>()
                .HasRequired(r => r.ReportedUser)
                .WithMany(u => u.ReportsReceived)
                .HasForeignKey(r => r.ReportedID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Report>()
                .HasOptional(r => r.Item)
                .WithMany()
                .HasForeignKey(r => r.ItemID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Report>()
                .HasOptional(r => r.Resolver)
                .WithMany()
                .HasForeignKey(r => r.ResolverID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Image>()
                .HasRequired(img => img.Item)
                .WithMany(i => i.Images)
                .HasForeignKey(img => img.ItemID)
                .WillCascadeOnDelete(true);
        }
    }
}

