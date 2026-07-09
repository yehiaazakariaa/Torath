namespace Torath.Entities
{
    public class Newspaper : BaseContent
    {
        // Newspapers usually don't have an ISSN, so we just establish the relationship.
       // "A newspaper issue can contain multiple articles"[cite: 73].
        public ICollection<NewspaperIssue> Issues { get; set; } = new List<NewspaperIssue>();
    }
}