
Expression expr2 = new Expression();
expr2.ParseExpression("5*x * 5*x");
Console.WriteLine("5x * 2x = " + expr2.Simplify().PrintExpression());
