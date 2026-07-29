using System.ComponentModel.DataAnnotations;

namespace Torath.DTOs
{
    // --- MAGAZINE DTOs ---
    public class MagazineDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;

        // Corrected to match the common properties requirement
        public DateTime PublicationDate { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ISSN { get; set; } = string.Empty;
    }

    public class MagazineWriteDto
    {
        [Required] public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required] public string Language { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;

        // Corrected to match the common properties requirement
        public DateTime PublicationDate { get; set; }

        [Required] public int CategoryId { get; set; }
        public string ISSN { get; set; } = string.Empty;
    }


    // --- ISSUE DTO FOR THE NESTED ENDPOINT ---
    public class MagazineIssueDto
    {
        public int Id { get; set; }
        public string IssueNumber { get; set; } = string.Empty;
        public string VolumeNumber { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; }
        public int MagazineId { get; set; }
    }
}