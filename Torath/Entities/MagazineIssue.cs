namespace Torath.Entities
{
    public class MagazineIssue
    {
        public int Id { get; set; }

// These fields are specifically required for a magazine issue .
        public string IssueNumber { get; set; } = string.Empty;
        public string VolumeNumber { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; }

        // --- Foreign Key (Belongs to ONE Magazine) ---
        public int MagazineId { get; set; }
        public Magazine Magazine { get; set; } = null!;
        public double Rating { get; set; } = 0;
        public int ViewCount { get; set; } = 0;

        // --- Navigation Property (Has MANY Articles) ---
        // "An issue can contain multiple articles"[cite: 67].
        public ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}