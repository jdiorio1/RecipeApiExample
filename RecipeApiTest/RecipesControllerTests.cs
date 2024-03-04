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
            //Arrange
            //need recipe repository instance
            var mockRecipeRepository = Substitute.For<IRecipeRepository>();
            //create a recipe to test
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
            //Convert expected recipe into DTO object so we can ensure the right result is being returned
            List<string> ingredientNames = expectedRecipe.Ingredients.Select(i => i.IngredientName).ToList();
            var instructions = expectedRecipe.Instructions.ToDictionary(i => i.InstructionNumber, i => i.InstructionDescription);
            RecipeDTO expectedRecipeDTO = new RecipeDTO()
            {
                Name = expectedRecipe.Name,
                Source = expectedRecipe.Source,
                Instructions = instructions,
                Ingredients = ingredientNames
            };

            //when recipeRepository.GetRecipebyId gets called call a fake version and returns the full recipe
            mockRecipeRepository.GetRecipeById(15).Returns(expectedRecipe);

            //create new instance of controller
            RecipesController recipeController = new RecipesController(mockRecipeRepository);

            //Act
            //just calls getrecipe and returns the DTO object to the user
            ActionResult<RecipeDTO> result = await recipeController.GetRecipe(15);

            //Assert
            //result (what we got back) is the expected type of ActionResult of type recipe
            Assert.IsAssignableFrom<ActionResult<RecipeDTO>>(result);

            //confirm that the recipe that we got back is the expectedRecipe
            result.Value.Should().BeEquivalentTo(expectedRecipeDTO);

            //we also need to assert/make sure that our mocks were called with expected value
            //this says we're confirming that the mock repository got a method call for GetRecipe by ID with ID 7
            await mockRecipeRepository.Received().GetRecipeById(15);
        }
        [Fact]
        public async Task GetRecipe_WhenRecipesIsNull_ReturnNotFound404()
        {
            //Arrange
            //need a repository instance
            IRecipeRepository mockRepository = Substitute.For<IRecipeRepository>();

            //when recipeRepository.GetRecipebyId gets called call a fake version and expect a null value to be returned from DB
            mockRepository.GetRecipeById(2).ReturnsNull();

            //create instance of controller
            RecipesController recipeController = new RecipesController(mockRepository);

            //Act
            //just calls getrecipe and passes in any random Id using instance of controller created above
            var result = await recipeController.GetRecipe(2);

            //Assert
            //The result that NotFound() returns (what we got back) is the expected type of NotFoundResult
            Assert.IsType<NotFoundResult>(result.Result);

            //this says we're confirming that the mock repository got a method call (the substitute Received a call) for GetRecipe by ID with ID 2
            await mockRepository.Received().GetRecipeById(2);
        }
        [Fact]
        public async Task PostRecipe_WithAllInputs_ReturnRecipe()
        {
            //Arrange
            //need a repository instance
            IRecipeRepository mockRepository = Substitute.For<IRecipeRepository>();
            //create a recipe to test
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
            //create a mock version of AddRecipe function (from our repository layer) to test
            mockRepository.AddRecipe(expectedRecipe).Returns(expectedRecipe);
            //create a new instance of controller that we use to call mock version of AddRecipe
            RecipesController recipeController = new RecipesController(mockRepository);

            //Act
            //call AddRecipe with instance of controller created above
            ActionResult<Recipe> result = await recipeController.PostRecipe(expectedRecipe);

            //Assert
            //result (what we got back) is the expected type of ActionResult of type recipe
            Assert.IsAssignableFrom<ActionResult<Recipe>>(result);
            //make sure we got back the recipe we passed in
            Assert.Equal(expectedRecipe, result.Value);
            //make sure our mock received a call to the expected method (make sure we called the function from the right place)
            await mockRepository.Received().AddRecipe(expectedRecipe);
            await mockRepository.Received().Save();
        }
        [Fact]
        public async Task PutRecipe_UpdateExistingRecipe_Returnstatus204()
        {
            //Arrange
            //create mock repository instance
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
            //create a mock version of GetRecipe & UpdateRecipe functions (from our repository layer) to test
            //because PutRecipe in our controller first checks to see if a recipe is there
            mockRepository.GetRecipeById(modifiedRecipe.RecipeId).Returns(modifiedRecipe);
            mockRepository.UpdateRecipe(modifiedRecipe).Returns(Task.CompletedTask);

            //create a new instance of controller that we use to call mock version of Repository functions
            RecipesController recipeController = new RecipesController(mockRepository);

            //Act
            //call Controller function PutRecipe. We mock the repository functions above that the controller method calls
            //but in Act we only test the controller method and it uses the mock versions of the repository methods
            //that the controller uses
            var result = await recipeController.PutRecipe(modifiedRecipe);

            //Assert
            //result (what we got back) is the expected type of ActionResult of type recipe
            Assert.IsType<NoContentResult>(result);
            //this says we're confirming that the mock repository got a method call (the substitute Received a call) for PutRecipe
            await mockRepository.Received().UpdateRecipe(modifiedRecipe);
            await mockRepository.Received().GetRecipeById(modifiedRecipe.RecipeId);
            await mockRepository.Received().Save();
        }
        [Fact]
        public async Task PutRecipe_PassInvalidRecipeId_ReturnNotFound()
        {
            //Arrange
            //create mock repository
            IRecipeRepository mockRepository = Substitute.For<IRecipeRepository>();
            Recipe testRecipe = new Recipe();
            //create mock version of GetReipe which checks to see if passed in recipe is valid
            mockRepository.GetRecipeById(testRecipe.RecipeId).ReturnsNull();
            //create instance of controller with mock repository
            RecipesController recipeController = new RecipesController(mockRepository);

            //Act
            var result = await recipeController.PutRecipe(testRecipe);

            //Assert
            //confirm that we got a not found result back
            Assert.IsType<NotFoundResult>(result);
            //Assert that we called mock version of GetRecipeById (this is the mock repository method)
            await mockRepository.Received().GetRecipeById(testRecipe.RecipeId);
        }
        [Fact]
        public async Task DeleteRecipe_WithExistingId_ReturnNoContext()
        {
            //Arrange
            IRecipeRepository mockRepository = Substitute.For<IRecipeRepository>();
            //create recipe with Id to use for testing
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
            //create mock version of GetRecipe and DeleteRecipe that gets called from the controller
            mockRepository.GetRecipeById(testRecipe.RecipeId).Returns(testRecipe);
            mockRepository.DeleteRecipe(testRecipe.RecipeId).Returns(Task.CompletedTask);
            //create controller
            RecipesController recipeController = new RecipesController(mockRepository);

            //Act
            var result = await recipeController.DeleteRecipe(testRecipe.RecipeId);

            //Assert
            Assert.IsType<NoContentResult>(result);
            //make sure we successfully called our methods
            await mockRepository.Received().GetRecipeById(testRecipe.RecipeId);
            await mockRepository.Received().DeleteRecipe(testRecipe.RecipeId);
            await mockRepository.Received().Save();
        }
        [Fact]
        public async Task DeleteRecipe_PassInvalidRecipeId_ReturnNotFound()
        {
            //Arrange
            //Create mock repository
            IRecipeRepository mockRepository = Substitute.For<IRecipeRepository>();
            //create test recipe
            Recipe testRecipe = new Recipe();
            //create mock version of Get Recipe Repository method
            mockRepository.GetRecipeById(testRecipe.RecipeId).ReturnsNull();
            //create controller instance
            RecipesController recipeController = new RecipesController(mockRepository);

            //Act
            var result = await recipeController.DeleteRecipe(testRecipe.RecipeId);

            //Assert
            Assert.IsType<NotFoundResult>(result);
            await mockRepository.Received().GetRecipeById(testRecipe.RecipeId);
        }
    }
}