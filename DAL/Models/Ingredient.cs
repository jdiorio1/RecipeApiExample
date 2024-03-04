namespace RecipeAPI.DAL
{
    public class Ingredient
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; }
        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    }
}
