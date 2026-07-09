using Microsoft.EntityFrameworkCore;
using Torath.Entities;

namespace Torath
{
    public class TorathDbContext : DbContext
    {
        public TorathDbContext(DbContextOptions<TorathDbContext> options) : base(options)
        {
        }

        // 1. Register ALL the tables (DbSets)
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<ResearchPaper> ResearchPapers { get; set; }
        public DbSet<Magazine> Magazines { get; set; }
        public DbSet<MagazineIssue> MagazineIssues { get; set; }
        public DbSet<Newspaper> Newspapers { get; set; }
        public DbSet<NewspaperIssue> NewspaperIssues { get; set; }
        public DbSet<Article> Articles { get; set; }

        // 2. Configure the database design rules
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Force Table-Per-Type (TPT) for the main content types so they get their own clean tables
            // instead of mixing together in one massive BaseContent table.
            modelBuilder.Entity<Book>().ToTable("Books");
            modelBuilder.Entity<ResearchPaper>().ToTable("ResearchPapers");
            modelBuilder.Entity<Magazine>().ToTable("Magazines");
            modelBuilder.Entity<Newspaper>().ToTable("Newspapers");

            // Configure the complex Article relationships (It belongs to either a Magazine Issue OR a Newspaper Issue)
            // 'HasOne' means the Article has one Issue. 'WithMany' means that Issue has many Articles.
            modelBuilder.Entity<Article>()
                .HasOne(a => a.MagazineIssue)
                .WithMany(mi => mi.Articles)
                .HasForeignKey(a => a.MagazineIssueId)
                .OnDelete(DeleteBehavior.NoAction); // If you delete an issue, it deletes the articles inside it.

            modelBuilder.Entity<Article>()
                .HasOne(a => a.NewspaperIssue)
                .WithMany(ni => ni.Articles)
                .HasForeignKey(a => a.NewspaperIssueId)
                .OnDelete(DeleteBehavior.NoAction);


            // --- Seed Data for Testing ---
            // This automatically inserts default categories into the database.
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Technology", Description = "Tech books and articles." },
                new Category { Id = 2, Name = "History", Description = "Historical documents." },
                new Category { Id = 3, Name = "Science", Description = "Scientific research and journals." }
            );


            // Optional: Seed a single Book for immediate testing in Module 3
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    Title = "Clean Architecture",
                    Description = "A Craftsman's Guide to Software Structure",
                    Language = "English",
                    PublicationDate = new DateTime(2017, 9, 10),
                    Publisher = "Prentice Hall",
                    CategoryId = 1, // Links to the "Technology" category we just seeded
                    ISBN = "978-0134494166",
                    Authors = "Robert C. Martin",
                    NumberOfPages = 432,
                    Edition = "1st"
                }
            );

        }

    }
}