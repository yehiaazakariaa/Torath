namespace Torath.DTOs
{
    // This defines the expected JSON format when creating or updating a Newspaper Issue
    public class NewspaperIssueWriteDto
    {
        public string IssueNumber { get; set; } = string.Empty; // e.g., "Edition 540"
        public DateTime PublicationDate { get; set; }           // The date this specific issue was published

        // CRITICAL: This is the Foreign Key that attaches this issue to an existing Newspaper in the database
        public int NewspaperId { get; set; }
    }
}
