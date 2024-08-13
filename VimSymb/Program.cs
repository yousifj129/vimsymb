Expression expr1 = new Expression();
expr1.ParseExpression("x^2 + 2*x + 1");
Expression derivative1 = expr1.Derivative("x");
Console.WriteLine("d/dx(x^2 + 2x + 1) = " + derivative1.PrintExpression());
