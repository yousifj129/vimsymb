Expression expr1 = new Expression();
expr1.ParseExpression("5*x + 2*x");
Console.WriteLine("5x + 2x = " + expr1.Simplify().PrintExpression());

Expression expr2 = new Expression();
expr2.ParseExpression("5*x * 5*x");
Console.WriteLine("5x * 2x = " + expr2.Simplify().PrintExpression());
