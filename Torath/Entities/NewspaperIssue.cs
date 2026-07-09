namespace Torath.Entities
{
    public class NewspaperIssue
    {
        public int Id { get; set; }

       // Required fields for a newspaper issue [cite: 71-72].
        public string IssueNumber { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; }

        // --- Foreign Key (Belongs to ONE Newspaper) ---
        public int NewspaperId { get; set; }
        public Newspaper Newspaper { get; set; } = null!;

        // --- Navigation Property (Has MANY Articles) ---
        public ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}