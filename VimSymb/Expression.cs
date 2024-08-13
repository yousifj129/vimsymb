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
                if (node.Left.Value == "0") return node.Right;
                if (node.Right.Value == "0") return node.Left;
                if (double.TryParse(node.Left.Value, out double leftVal) && 
                    double.TryParse(node.Right.Value, out double rightVal))
                    return new ExpressionNode((leftVal + rightVal).ToString());
                break;
            case "*":
                if (node.Left.Value == "0" || node.Right.Value == "0")
                    return new ExpressionNode("0");
                if (node.Left.Value == "1") return node.Right;
                if (node.Right.Value == "1") return node.Left;
                if (double.TryParse(node.Left.Value, out leftVal) && 
                    double.TryParse(node.Right.Value, out rightVal))
                    return new ExpressionNode((leftVal * rightVal).ToString());
                break;
            case "^":
                if (node.Right.Value == "1") return node.Left;
                if (node.Right.Value == "0") return new ExpressionNode("1");
                if (node.Left.Value == "0") return new ExpressionNode("0");
                if (double.TryParse(node.Left.Value, out leftVal) && 
                    double.TryParse(node.Right.Value, out rightVal))
                    return new ExpressionNode(Math.Pow(leftVal, rightVal).ToString());
                break;
        }

        return node;
    }

    private void CollectTerms(ExpressionNode node, Dictionary<string, double> terms, ref double constant, double sign)
    {
        if (node == null) return;

        switch (node.Value)
        {
            case "+":
                CollectTerms(node.Left, terms, ref constant, sign);
                CollectTerms(node.Right, terms, ref constant, sign);
                break;
            case "-":
                CollectTerms(node.Left, terms, ref constant, sign);
                CollectTerms(node.Right, terms, ref constant, -sign);
                break;
            case "*":
                if (node.Left != null && node.Right != null)
                {
                    if (double.TryParse(node.Left.Value, out double leftCoeff))
                    {
                        AddTerm(terms, node.Right.Value, leftCoeff * sign);
                    }
                    else if (double.TryParse(node.Right.Value, out double rightCoeff))
                    {
                        AddTerm(terms, node.Left.Value, rightCoeff * sign);
                    }
                    else
                    {
                        AddTerm(terms, $"({node.Left.Value}*{node.Right.Value})", sign);
                    }
                }
                else
                {
                    AddTerm(terms, node.Value, sign);
                }
                break;
            default:
                if (double.TryParse(node.Value, out double number))
                {
                    constant += sign * number;
                }
                else
                {
                    AddTerm(terms, node.Value, sign);
                }
                break;
        }
    }

    private void AddTerm(Dictionary<string, double> terms, string key, double value)
    {
        if (!terms.ContainsKey(key))
            terms[key] = 0;
        terms[key] += value;
    }
    public Expression Derivative(string variable)
    {
        Expression result = new Expression();
        result.Root = DerivativeNode(this.Root, variable);
        return result.Simplify();
    }

    private ExpressionNode DerivativeNode(ExpressionNode node, string variable)
    {
        if (node == null) return null;

        switch (node.Value)
        {
            case "+":
            case "-":
                return new ExpressionNode(node.Value,
                    DerivativeNode(node.Left, variable),
                    DerivativeNode(node.Right, variable));

            case "*":
                if (node.Left.Value == variable && !IsVariable(node.Right.Value))
                    return node.Right.Clone();
                else if (node.Right.Value == variable && !IsVariable(node.Left.Value))
                    return node.Left.Clone();
                else
                    // Product rule: d(u*v)/dx = u'v + uv'
                    return new ExpressionNode("+",
                        new ExpressionNode("*", DerivativeNode(node.Left, variable), node.Right.Clone()),
                        new ExpressionNode("*", node.Left.Clone(), DerivativeNode(node.Right, variable)));

            case "^":
                if (node.Left.Value == variable && double.TryParse(node.Right.Value, out double power))
                {
                    // Power rule: d(x^n)/dx = n * x^(n-1)
                    return new ExpressionNode("*",
                        new ExpressionNode(power.ToString()),
                        new ExpressionNode("^",
                            new ExpressionNode(variable),
                            new ExpressionNode((power - 1).ToString())
                        )
                    );
                }
                break;

            default:
                if (node.Value == variable)
                    return new ExpressionNode("1");
                else if (double.TryParse(node.Value, out _) || node.Value != variable)
                    return new ExpressionNode("0");
                break;
        }

        throw new NotImplementedException($"Derivative not implemented for: {PrintExpressionRecursive(node)}");
    }

    private bool IsVariable(string value)
    {
        return !double.TryParse(value, out _);
    }

}