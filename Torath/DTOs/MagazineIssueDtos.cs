using System;

namespace Torath.DTOs
{
    // This defines the exact JSON payload the user must send in POST and PUT requests
    public class MagazineIssueWriteDto
    {
        public string IssueNumber { get; set; } = string.Empty;
        public string VolumeNumber { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; }

        // CRITICAL: This links the issue to its parent Magazine
        public int MagazineId { get; set; }
    }

    
   
}