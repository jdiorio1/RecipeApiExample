using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using RecipeAPI.Controllers;
using NSubstitute.ReturnsExtensions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using RecipeAPI.DAL;
using FluentAssertions;
using RecipeAPI;

namespace RecipeApiTest
{
    public class RecipesControllerTests
    {
        [Fact]
        public async Task GetRecipe_WithExistingId_ReturnRecipe()
        {
            var mockRecipeRepository = Substitute.For<IRecipeRepository>();
            Recipe expectedRecipe = new Recipe()
            {
                DateAdded = DateTime.UtcNow,
                Name = "Kiki Buffalo Cauliflower Salad",
                Source = "Plantiful Lean Cookbook",
                RecipeId = 15,
                Instructions = new List<Instruction>()
                {
                    new Instruction()
                    {
                        InstructionNumber = 1,
                        InstructionDescription = "Steam russet potatoes."
                    },
                    new Instruction()
                    {
                        InstructionNumber = 2,
                        InstructionDescription = "Cut up peppers and onions."
                    }
                },
                Ingredients = new List<Ingredient>() 
                { 
                    new Ingredient() { IngredientName = "1 head Cauliflower" },
                    new Ingredient() {IngredientName = "7 Tbsp Frank's Mild Buffalo Sauce" }, 
                    new Ingredient() {IngredientName = "1/4 tsp salt"} 
                }
            };
            List<string> ingredientNames = expectedRecipe.Ingredients.Select(i => i.IngredientName).ToList();
            var instructions = expectedRecipe.Instructions.ToDictionary(i => i.InstructionNumber, i => i.InstructionDescription);
            RecipeDTO expectedRecipeDTO = new RecipeDTO()
            {
                Name = expectedRecipe.Name,
                Source = expectedRecipe.Source,
                Instructions = instructions,
                Ingredients = ingredientNames
            };

            mockRecipeRepository.GetRecipeById(15).Returns(expectedRecipe);

            RecipesController recipeController = new RecipesController(mockRecipeRepository);

            ActionResult<RecipeDTO> result = await recipeController.GetRecipe(15);

            Assert.IsAssignableFrom<ActionResult<RecipeDTO>>(result);

            result.Value.Should().BeEquivalentTo(expectedRecipeDTO);

            await mockRecipeRepository.Received().GetRecipeById(15);
        }
        [Fact]
        public async Task GetRecipe_WhenRecipesIsNull_ReturnNotFound404()
        {
            IRecipeRepository mockRepository = Substitute.For<IRecipeRepository>();

            mockRepository.GetRecipeById(2).ReturnsNull();

            RecipesController recipeController = new RecipesController(mockRepository);

            var result = await recipeController.GetRecipe(2);

            Assert.IsType<NotFoundResult>(result.Result);

            await mockRepository.Received().GetRecipeById(2);
        }
        [Fact]
        public async Task PostRecipe_WithAllInputs_ReturnRecipe()
        {
            IRecipeRepository mockRepository = Substitute.For<IRecipeRepository>();
            Recipe expectedRecipe = new Recipe()
            {
                DateAdded = DateTime.UtcNow,
                Name = "Kiki Buffalo Cauliflower Salad",
                Source =
                "Plantiful Lean Cookbook",
                RecipeId = 15,
                Instructions = new List<Instruction>() { new Instruction() {InstructionNumber = 1,
                InstructionDescription = "Steam russet potatoes."}, new Instruction() {InstructionNumber = 2, InstructionDescription = "Cut up peppers and onions."} },
                Ingredients = new List<Ingredient>() { new Ingredient() { IngredientName = "1 head Cauliflower" },
                    new Ingredient() {IngredientName = "7 Tbsp Frank's Mild Buffalo Sauce" }, new Ingredient() {IngredientName = "1/4 tsp salt"} }
            };
            mockRepository.AddRecipe(expectedRecipe).Returns(expectedRecipe);
            RecipesController recipeController = new RecipesController(mockRepository);

            ActionResult<Recipe> result = await recipeController.PostRecipe(expectedRecipe);

            Assert.IsAssignableFrom<ActionResult<Recipe>>(result);
            Assert.Equal(expectedRecipe, result.Value);
            await mockRepository.Received().AddRecipe(expectedRecipe);
            await mockRepository.Received().Save();
        }
        [Fact]
        public async Task PutRecipe_UpdateExistingRecipe_Returnstatus204()
        {
            IRecipeRepository mockRepository = Substitute.For<IRecipeRepository>();

            Recipe modifiedRecipe = new Recipe()
            {
                DateAdded = DateTime.UtcNow,
                Name = "Kiki Buffalo Wing Cauliflower Salad",
                Source = "Plantiful Lean Cookbook",
                RecipeId = 15,
                Instructions = new List<Instruction>() { new Instruction() {InstructionNumber = 1,
                InstructionDescription = "Steam 2 russet potatoes."}, new Instruction() {InstructionNumber = 2, InstructionDescription = "Cut up your pepper and onion."} },
                Ingredients = new List<Ingredient>() { new Ingredient() { IngredientName = "1 head Cauliflower" },
                    new Ingredient() {IngredientName = "7 Tbsp Frank's Mild Buffalo Sauce" }, new Ingredient() {IngredientName = "3/4c Panko breadcrumbs"} }
            };
            mockRepository.GetRecipeById(modifiedRecipe.RecipeId).Returns(modifiedRecipe);
            mockRepository.UpdateRecipe(modifiedRecipe).Returns(Task.CompletedTask);

            RecipesController recipeController = new RecipesController(mockRepository);

            var result = await recipeController.PutRecipe(modifiedRecipe);

            Assert.IsType<NoContentResult>(result);
            await mockRepository.Received().UpdateRecipe(modifiedRecipe);
            await mockRepository.Received().GetRecipeById(modifiedRecipe.RecipeId);
            await mockRepository.Received().Save();
        }
        [Fact]
        public async Task PutRecipe_PassInvalidRecipeId_ReturnNotFound()
        {
            IRecipeRepository mockRepository = Substitute.For<IRecipeRepository>();
            Recipe testRecipe = new Recipe();
            mockRepository.GetRecipeById(testRecipe.RecipeId).ReturnsNull();
            RecipesController recipeController = new RecipesController(mockRepository);

            var result = await recipeController.PutRecipe(testRecipe);

            Assert.IsType<NotFoundResult>(result);
            await mockRepository.Received().GetRecipeById(testRecipe.RecipeId);
        }
        [Fact]
        public async Task DeleteRecipe_WithExistingId_ReturnNoContext()
        {
            IRecipeRepository mockRepository = Substitute.For<IRecipeRepository>();
            Recipe testRecipe = new Recipe()
            {
                RecipeId = 22,
                Name = "Greeek yogurt bowl",
                Ingredients = new List<Ingredient>() { new Ingredient() { IngredientName = "2% greek yogurt" },
                new Ingredient() {IngredientName = "1/4 cup pomegranate seeds"}},
                Instructions = new List<Instruction>() { new Instruction() { InstructionNumber = 1,
                InstructionDescription = "add 1 cup greek yogurt to bowl" }, new Instruction() { InstructionNumber = 2,
                    InstructionDescription = "top with 1/4 cup pomegranate seeds" } }
            };
            mockRepository.GetRecipeById(testRecipe.RecipeId).Returns(testRecipe);
            mockRepository.DeleteRecipe(testRecipe.RecipeId).Returns(Task.CompletedTask);
            RecipesController recipeController = new RecipesController(mockRepository);

            var result = await recipeController.DeleteRecipe(testRecipe.RecipeId);

            Assert.IsType<NoContentResult>(result);
            await mockRepository.Received().GetRecipeById(testRecipe.RecipeId);
            await mockRepository.Received().DeleteRecipe(testRecipe.RecipeId);
            await mockRepository.Received().Save();
        }
        [Fact]
        public async Task DeleteRecipe_PassInvalidRecipeId_ReturnNotFound()
        {
            IRecipeRepository mockRepository = Substitute.For<IRecipeRepository>();
            Recipe testRecipe = new Recipe();
            mockRepository.GetRecipeById(testRecipe.RecipeId).ReturnsNull();
            RecipesController recipeController = new RecipesController(mockRepository);

            var result = await recipeController.DeleteRecipe(testRecipe.RecipeId);

            Assert.IsType<NotFoundResult>(result);
            await mockRepository.Received().GetRecipeById(testRecipe.RecipeId);
        }
    }
}