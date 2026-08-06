using System;

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

    // This is the Read DTO returned to the frontend
    public class NewspaperIssueDto
    {
        public int Id { get; set; }
        public string IssueNumber { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; }
        public int NewspaperId { get; set; }

        // New Analytics Properties
        public double Rating { get; set; }
        public int ViewCount { get; set; }
    }
}