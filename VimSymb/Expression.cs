class ExpressionNode
{
    public string Value { get; set; }
    public ExpressionNode Left { get; set; }
    public ExpressionNode Right { get; set; }

    public ExpressionNode(string value, ExpressionNode left = null, ExpressionNode right = null)
    {
        Value = value;
        Left = left;
        Right = right;
    }
}

class Expression
{
    public ExpressionNode Root { get; private set; }

    public Expression(string eq) {
        Root = new ExpressionNode(eq);
        this.ParseExpression(eq);
     }

    public void ParseExpression(string expression)
    {
        Root = ParseExpressionRecursive(expression);
    }

    private ExpressionNode ParseExpressionRecursive(string expression)
    {
        expression = expression.Trim();

        // Remove outermost parentheses if present
        if (expression.StartsWith("(") && expression.EndsWith(")"))
        {
            expression = expression.Substring(1, expression.Length - 2);
        }

        // Find the main operator
        int parenthesesCount = 0;
        int operatorIndex = -1;
        for (int i = expression.Length - 1; i >= 0; i--)
        {
            if (expression[i] == ')')
                parenthesesCount++;
            else if (expression[i] == '(')
                parenthesesCount--;
            else if (parenthesesCount == 0 && IsOperator(expression[i]))
            {
                operatorIndex = i;
                break;
            }
        }

        // If no operator found, it's a single variable or number
        if (operatorIndex == -1)
        {
            return new ExpressionNode(expression);
        }

        string op = expression[operatorIndex].ToString();
        string left = expression.Substring(0, operatorIndex);
        string right = expression.Substring(operatorIndex + 1);

        return new ExpressionNode(op, ParseExpressionRecursive(left), ParseExpressionRecursive(right));
    }

    private bool IsOperator(char c)
    {
        return c == '+' || c == '-' || c == '*' || c == '/' || c == '^';
    }

    public string PrintExpression()
    {
        return PrintExpressionRecursive(Root);
    }

    private string PrintExpressionRecursive(ExpressionNode node)
    {
        if (node == null)
            return "";

        if (node.Left == null && node.Right == null)
            return node.Value;

        string leftExpr = PrintExpressionRecursive(node.Left);
        string rightExpr = PrintExpressionRecursive(node.Right);

        return $"({leftExpr}{node.Value}{rightExpr})";
    }
}