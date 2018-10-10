namespace CriteriaExpression
{
    public enum TokenType
    {
        None = 0,
        Boolean = 1,
        Number = 2,
        String = 3,
        Variable = 4,
        Function = 5,
        Prefix = 6,
        Operation = 7,
        Expression = 0x40,
        EndExpression = 0x80,
    }
}