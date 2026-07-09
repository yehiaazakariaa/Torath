namespace Torath.Entities
{
    // Inherits Id, Title, CategoryId, etc., from BaseContent
    public class ResearchPaper : BaseContent
    {
        public string Authors { get; set; } = string.Empty;
        public string Abstract { get; set; } = string.Empty;
        public string Keywords { get; set; } = string.Empty;

        // Notice this is an integer, while BaseContent has a full PublicationDate DateTime. 
        // We keep both to satisfy the specific requirement for Research Papers.
        public int PublicationYear { get; set; }
        public string JournalOrConferenceName { get; set; } = string.Empty;
        public string DOI { get; set; } = string.Empty; // Digital Object Identifier
    }
}