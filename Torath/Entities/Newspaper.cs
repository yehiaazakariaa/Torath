using System.ComponentModel.DataAnnotations.Schema;

namespace Torath.Entities
{
    public class Newspaper : BaseContent
    {
        // Newspapers usually don't have an ISSN, so we just establish the relationship.
        // "A newspaper issue can contain multiple articles"[cite: 73].

        public string Frequency { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public string? PdfFilePath { get; set; }
        public ICollection<NewspaperIssue> Issues { get; set; } = new List<NewspaperIssue>();
    }
}