namespace Torath.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // --- Navigation Property (The "Many" side) ---
        // A single Category can contain MANY pieces of content. 
        // ICollection is an interface that allows us to hold a list of those connected items.
        // We initialize it to an empty list so it is never null when we try to add items to it.
        public ICollection<BaseContent> Contents { get; set; } = new List<BaseContent>();
    }
}