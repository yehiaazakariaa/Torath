namespace Torath.Entities
{
    // The colon ":" means "inherits from". Book gets all properties from BaseContent.
    public class Book : BaseContent
    {
        // These are the specific properties that ONLY belong to a Book.
        public string ISBN { get; set; } = string.Empty;
        public string Authors { get; set; } = string.Empty;
        public int NumberOfPages { get; set; }
        public string Edition { get; set; } = string.Empty;
    }
}