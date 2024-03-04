using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace RecipeAPI.DAL
{
    public class RecipeContext : DbContext
    {
        public RecipeContext(DbContextOptions<RecipeContext> options)
            : base(options)
        {
        }
        public DbSet<Recipe> Recipes { get; set; } = null!;
        public DbSet<Instruction> Instructions { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
    }
}
