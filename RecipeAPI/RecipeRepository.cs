using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeAPI.DAL;

public class RecipeRepository : IRecipeRepository
{
    private readonly RecipeContext _context;
    public RecipeRepository(RecipeContext context) => _context = context;
    public async Task<Recipe?> GetRecipeById(int id)
    {
        var recipe = await _context.Recipes
            .Include(r => r.Instructions)
            .Include(r => r.Ingredients)
            .AsNoTracking()
            .FirstOrDefaultAsync(r=>r.RecipeId == id);

        return recipe;
    }
    public async Task<Recipe> AddRecipe(Recipe recipe)
    {
        foreach(Ingredient ingredient in recipe.Ingredients)
        {
            var ingd = _context.Ingredients.AsNoTracking().Where(i => i.IngredientName == ingredient.IngredientName).FirstOrDefault();
            if (ingd != null)
            {
                ingredient.IngredientId = ingd.IngredientId;
                _context.Ingredients.Attach(ingredient);
            }
        }
        await _context.Recipes.AddAsync(recipe);
        return recipe;
    }
    public async Task UpdateRecipe(Recipe recipe)
    {
        Recipe? dbRecipe = await _context.Recipes.Where(r => r.RecipeId == recipe.RecipeId)
            .Include(r => r.Instructions)
            .Include(r => r.Ingredients)
            .SingleOrDefaultAsync();

        if (dbRecipe == null)
        {
            return;
        }
        foreach (Ingredient item in dbRecipe.Ingredients)
        {
            if (recipe.Ingredients.Any(x => x.IngredientName.ToLower() == item.IngredientName.ToLower()))
            {
                continue;
            }

            dbRecipe.Ingredients.Remove(item);
        }
        foreach (Ingredient item in recipe.Ingredients)
        {
            if (dbRecipe.Ingredients.Any(x => x.IngredientName.ToLower() == item.IngredientName.ToLower()))
            {
                continue;
            }
            dbRecipe.Ingredients.Add(item);
        }

        dbRecipe.Name = recipe.Name;
        dbRecipe.Source = recipe.Source;
        dbRecipe.DateAdded = recipe.DateAdded;
        dbRecipe.Instructions = recipe.Instructions;
    }
    public async Task DeleteRecipe(int id)
    {
        Recipe? dbRecipe = await _context.Recipes.FindAsync(id);
        if (dbRecipe != null)
        {
            _context.Recipes.Remove(dbRecipe);
        }
    }
    public async Task Save()
    {
        await _context.SaveChangesAsync();
    }
}
