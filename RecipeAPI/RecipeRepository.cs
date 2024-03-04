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
        //take the ingredients from the passed in recipe check to see if they already exist 
        //if they do the attach them to the existing ingredients
        foreach(Ingredient ingredient in recipe.Ingredients)
        {
            var ingd = _context.Ingredients.AsNoTracking().Where(i => i.IngredientName == ingredient.IngredientName).FirstOrDefault();
            //look through ingredients and if they already exist make it clear they exist when we attach them to the recipe
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
        
        //get the recipe from the database
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
            // If the db recipe already has an ingredient that the updated recipe has keep going. No need to remove.
            if (recipe.Ingredients.Any(x => x.IngredientName.ToLower() == item.IngredientName.ToLower()))
            {
                continue;
            }

            // if the updated recipe doesn't include an ingredient on the existing db recipe, we need to remove it from the db recipe
            dbRecipe.Ingredients.Remove(item);
        }
        foreach (Ingredient item in recipe.Ingredients)
        {
            // If the db recipe already has an ingredient that the updated recipe has keep going. No need to add.
            if (dbRecipe.Ingredients.Any(x => x.IngredientName.ToLower() == item.IngredientName.ToLower()))
            {
                continue;
            }
            // if the updated recipe doesn't include an ingredient on the existing db recipe, we need to add it to the db recipe
            dbRecipe.Ingredients.Add(item);
        }

        dbRecipe.Name = recipe.Name;
        dbRecipe.Source = recipe.Source;
        dbRecipe.DateAdded = recipe.DateAdded;
        dbRecipe.Instructions = recipe.Instructions;
    }
    public async Task DeleteRecipe(int id)
    {
        //get the recipe to delete from the database
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
