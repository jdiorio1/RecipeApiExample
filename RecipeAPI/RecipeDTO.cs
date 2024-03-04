namespace RecipeAPI
{
    public class RecipeDTO
    {
            public string Name { get; set; }
            public string? Source { get; set; }
            public Dictionary<int, string> Instructions { get; set; }
            public List<string> Ingredients { get; set; }
    }
}
