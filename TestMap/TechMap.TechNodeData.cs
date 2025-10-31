namespace TestMap;

public partial class TechMap
{
    public record TechNodeData(string Title)
    {
        public static readonly TechNodeData Empty = new(string.Empty);
        public string Title { get; } = Title;
    }
}