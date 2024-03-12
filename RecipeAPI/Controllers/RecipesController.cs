using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeAPI.DAL;

namespace RecipeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeRepository _repository;
        public RecipesController(IRecipeRepository repository)
        {
            _repository = repository;
        }

        // GET: api/Recipes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeDTO>> GetRecipe(int id)
        {
            var recipe = await _repository.GetRecipeById(id);
            if (recipe == null)
            {
                return NotFound();
            }
            return RecipeToDTO(recipe);
        }


        // POST: api/Recipes
        [HttpPost]
        public async Task<ActionResult<Recipe>> PostRecipe(Recipe recipe)
        {

            var result = await _repository.AddRecipe(recipe);
            await _repository.Save();
            return result;
        }
        // PUT: api/Recipes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecipe(Recipe recipe)
        {
            if (await _repository.GetRecipeById(recipe.RecipeId) == null)
            {
                return NotFound();
            }

            await _repository.UpdateRecipe(recipe);
            await _repository.Save();

            return NoContent();
        }
        // DELETE: api/Recipes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            if (await _repository.GetRecipeById(id) == null)
            {
                return NotFound();
            }

            await _repository.DeleteRecipe(id);
            await _repository.Save();

            return NoContent();
        }

        private static RecipeDTO RecipeToDTO(Recipe recipe)
        {
            List<string> ingredientNames = recipe.Ingredients.Select(i => i.IngredientName).ToList();
            var instructions = recipe.Instructions.ToDictionary(i => i.InstructionNumber, i => i.InstructionDescription);
            return new RecipeDTO
            {
                Name = recipe.Name,
                Source = recipe.Source,
                Instructions = instructions,
                Ingredients = ingredientNames
            };
        }
    }
}
