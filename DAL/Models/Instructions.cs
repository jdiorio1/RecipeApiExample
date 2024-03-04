namespace RecipeAPI.DAL
{
    public class Instruction
    {
        public int InstructionId { get; set; }
        public int InstructionNumber { get; set; }
        public string InstructionDescription { get; set; }
        public int RecipeId { get; set; }
    }
}
