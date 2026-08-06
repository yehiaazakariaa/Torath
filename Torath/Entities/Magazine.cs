using Torath.Entities;

public class Magazine : BaseContent

{

    // A unique identifier for magazines (similar to a Book's ISBN)

    public string ISSN { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }
    public string? PdfFileUrl { get; set; }

    // --- Relational Navigation ---

    // The requirements state: "A magazine can have multiple issues"[cite: 66].

    // This list holds all the specific monthly/weekly issues belonging to this magazine series.

    public ICollection<MagazineIssue> Issues { get; set; } = new List<MagazineIssue>();
}