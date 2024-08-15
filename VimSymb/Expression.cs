using System;
using System.Collections.Generic;
using System.Linq;

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
    public ExpressionNode Clone()
    {
        return new ExpressionNode(Value, 
            Left?.Clone(), 
            Right?.Clone());
    }
}

class Expression
{
    public ExpressionNode Root { get; private set; }
    public Expression(){
        Root = null;
    }
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

    public Expression Add(Expression other)
    {
        Expression result = new Expression();
        result.Root = new ExpressionNode("+", this.Root.Clone(), other.Root.Clone());
        return result.Simplify();
    }

    public Expression Subtract(Expression other)
    {
        Expression result = new Expression();
        result.Root = new ExpressionNode("-", this.Root.Clone(), other.Root.Clone());
        return result.Simplify();
    }

    public Expression Multiply(Expression other)
    {
        Expression result = new Expression();
        result.Root = new ExpressionNode("*", this.Root.Clone(), other.Root.Clone());
        return result.Simplify();
    }

    public Expression Divide(Expression other)
    {
        Expression result = new Expression();
        result.Root = new ExpressionNode("/", this.Root.Clone(), other.Root.Clone());
        return result.Simplify();
    }

    public Expression Simplify()
    {
        Expression result = new Expression();
        result.Root = SimplifyNode(this.Root);
        return result;
    }

    private ExpressionNode SimplifyNode(ExpressionNode node)
    {
        if (node == null) return null;

        // Recursively simplify child nodes
        node.Left = SimplifyNode(node.Left);
        node.Right = SimplifyNode(node.Right);

        switch (node.Value)
        {
            case "+":
            case "-":
                return SimplifyAdditionSubtraction(node);
            case "*":
                return SimplifyMultiplication(node);
            case "^":
                return SimplifyPower(node);
        }

        return node;
    }

    private ExpressionNode SimplifyAdditionSubtraction(ExpressionNode node)
    {
        var terms = new Dictionary<string, double>();
        CollectTerms(node, terms, 1);
        return ReconstructFromTerms(terms);
    }

    private ExpressionNode SimplifyMultiplication(ExpressionNode node)
    {
        var factors = new Dictionary<string, double>();
        CollectFactors(node, factors);
        return ReconstructFromFactors(factors);
    }

    private void CollectTerms(ExpressionNode node, Dictionary<string, double> terms, double coefficient)
    {
        if (node == null) return;

        switch (node.Value)
        {
            case "+":
                CollectTerms(node.Left, terms, coefficient);
                CollectTerms(node.Right, terms, coefficient);
                break;
            case "-":
                CollectTerms(node.Left, terms, coefficient);
                CollectTerms(node.Right, terms, -coefficient);
                break;
            case "*":
                if (double.TryParse(node.Left.Value, out double leftCoeff))
                {
                    AddOrUpdateTerm(terms, node.Right.Value, leftCoeff * coefficient);
                }
                else if (double.TryParse(node.Right.Value, out double rightCoeff))
                {
                    AddOrUpdateTerm(terms, node.Left.Value, rightCoeff * coefficient);
                }
                else
                {
                    AddOrUpdateTerm(terms, $"{node.Left.Value}*{node.Right.Value}", coefficient);
                }
                break;
            default:
                if (double.TryParse(node.Value, out double numericValue))
                    AddOrUpdateTerm(terms, "constant", numericValue * coefficient);
                else
                    AddOrUpdateTerm(terms, node.Value, coefficient);
                break;
        }
    }

    private void CollectFactors(ExpressionNode node, Dictionary<string, double> factors, double coefficient = 1)
    {
        if (node == null) return;

        if (node.Value == "*")
        {
            CollectFactors(node.Left, factors);
            CollectFactors(node.Right, factors);
        }
        else if (double.TryParse(node.Value, out double numericValue))
        {
            AddOrUpdateTerm(factors, "constant", numericValue * coefficient);
        }
        else
        {
            AddOrUpdateTerm(factors, node.Value, 1 * coefficient);
        }
    }

    private void AddOrUpdateTerm(Dictionary<string, double> terms, string key, double value)
    {
        if (terms.ContainsKey(key))
            terms[key] += value;
        else
            terms[key] = value;
    }

    private ExpressionNode ReconstructFromTerms(Dictionary<string, double> terms)
    {
        List<ExpressionNode> termNodes = new List<ExpressionNode>();

        foreach (var term in terms)
        {
            if (Math.Abs(term.Value) > 1e-10) // Ignore terms with near-zero coefficients
            {
                if (term.Key == "constant")
                {
                    termNodes.Add(new ExpressionNode(term.Value.ToString()));
                }
                else if (Math.Abs(term.Value - 1) < 1e-10)
                {
                    termNodes.Add(new ExpressionNode(term.Key));
                }
                else if (Math.Abs(term.Value + 1) < 1e-10)
                {
                    termNodes.Add(new ExpressionNode("-", new ExpressionNode(term.Key)));
                }
                else
                {
                    termNodes.Add(new ExpressionNode("*",
                        new ExpressionNode(term.Value.ToString()),
                        new ExpressionNode(term.Key)));
                }
            }
        }

        return CombineNodes(termNodes, "+");
    }

    private ExpressionNode ReconstructFromFactors(Dictionary<string, double> factors)
    {
        List<ExpressionNode> factorNodes = new List<ExpressionNode>();
        double constantFactor = 1;

        foreach (var factor in factors)
        {
            if (Math.Abs(factor.Value) > 1e-10) // Ignore factors with near-zero exponents
            {
                if (factor.Key == "constant")
                {
                    constantFactor *= factor.Value;
                }
                else if (Math.Abs(factor.Value - 1) < 1e-10)
                {
                    factorNodes.Add(new ExpressionNode(factor.Key));
                }
                else
                {
                    factorNodes.Add(new ExpressionNode("^",
                        new ExpressionNode(factor.Key),
                        new ExpressionNode(factor.Value.ToString())));
                }
            }
        }

        if (Math.Abs(constantFactor - 1) > 1e-10)
        {
            factorNodes.Insert(0, new ExpressionNode(constantFactor.ToString()));
        }

        return CombineNodes(factorNodes, "*");
    }

    private ExpressionNode CombineNodes(List<ExpressionNode> nodes, string operation)
    {
        if (nodes.Count == 0)
            return new ExpressionNode(operation == "*" ? "1" : "0");
        if (nodes.Count == 1)
            return nodes[0];

        ExpressionNode result = nodes[0];
        for (int i = 1; i < nodes.Count; i++)
        {
            result = new ExpressionNode(operation, result, nodes[i]);
        }

        return result;
    }
    private ExpressionNode SimplifyPower(ExpressionNode node)
    {
        if (node.Right.Value == "1") return node.Left;
        if (node.Right.Value == "0") return new ExpressionNode("1");
        if (node.Left.Value == "0") return new ExpressionNode("0");
        if (node.Left.Value == "1") return new ExpressionNode("1");

        if (double.TryParse(node.Left.Value, out double leftVal) && 
            double.TryParse(node.Right.Value, out double rightVal))
            return new ExpressionNode(Math.Pow(leftVal, rightVal).ToString());

        return node; // Keep symbolic powers as is
    }

}