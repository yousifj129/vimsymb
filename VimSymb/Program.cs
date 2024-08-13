// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");
Expression expr = new Expression("((x*y)^y)");
Console.WriteLine(expr.PrintExpression());  // Output: ((x+y)^y)