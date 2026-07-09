namespace Torath.Entities
{
    // 'abstract' means we can never create a direct instance of "BaseContent".
    // We can only create specific things that inherit from it, like a "Book" or "Article".
    public abstract class BaseContent
    {
        // Entity Framework (EF) automatically recognizes "Id" as the Primary Key for the database table.
        public int Id { get; set; }

        // We initialize strings to string.Empty to avoid pesky Null Reference Exceptions later.
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;

        // DateTime stores the exact date and time. 
        public DateTime PublicationDate { get; set; }
        public string Publisher { get; set; } = string.Empty;

        // We set a default value of the current exact time when the record is created.
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // The '?' means this property is nullable. An item might not have been updated yet.
        public DateTime? UpdatedDate { get; set; }

        // --- Relational Mapping (Foreign Keys) ---
        // Every piece of content must belong to one Category. 
        // This integer stores the actual ID of the category (e.g., Category ID 5).
        public int CategoryId { get; set; }

        // This is a "Navigation Property". It allows our C# code to easily pull all category 
        // details (like the Category Name) when we query a Book, without writing complex SQL JOINs manually.
        // "null!" tells the compiler to trust us that this won't be null when EF loads it.
        public Category Category { get; set; } = null!;
    }
}