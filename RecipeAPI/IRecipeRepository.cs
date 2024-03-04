using Microsoft.AspNetCore.Mvc;
using RecipeAPI.DAL;

public interface IRecipeRepository
{
    Task<Recipe?> GetRecipeById(int id);
    Task<Recipe> AddRecipe(Recipe recipe);
    Task UpdateRecipe(Recipe recipe);
    Task DeleteRecipe(int id);
    Task Save();
}
