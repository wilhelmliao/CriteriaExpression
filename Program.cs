using System;
using System.Diagnostics;
using System.IO;

namespace CriteriaExpression
{
    class Program
    {
        static void Main(string[] args)
        {
            DemoExpr1();
            string expression = "(foo(3) == bar) ? 'Got It!!!'";


            object result = Interpreter.Evaluate(expression, 
            delegate(TokenType type, string name, Token[] argv)
            {
                switch(type)
                {
                    case TokenType.Variable:
                        if (name == "bar")
                        {
                            return 15;
                        }
                        break;

                    case TokenType.Function:
                        if (name == "foo")
                        {
                            return argv[0] * 5;
                        }
                        break;
                }
                return Token.Null;
            });

            Console.WriteLine(result);
            Console.WriteLine();
        }

        static void DemoExpr1()
        {
            string expression = "!(10 >= 15 || 6 + 3 * 11 - 4 == 12 + -5) ? 'Got It!!!'";

            Stopwatch watch = new Stopwatch();

            watch.Start();
            object result = null;
            int i = 0;
            for (; i < 10000; i++)
            {
                result = Interpreter.Evaluate(expression, Resolver.Default);
                switch(i)
                {
                    case 1000:
                    case 5000:
                        Console.WriteLine("run {0,-8} {1,5}ms", i, watch.ElapsedMilliseconds);
                        break;
                }
                //Console.WriteLine(tree.Root.Token.Lexeme);
            }
            watch.Stop();
            Console.WriteLine("run {0,-8} {1,5}ms", i, watch.ElapsedMilliseconds);

            Console.WriteLine(result.ToString());
            Console.WriteLine();
        }
        static void DemoEvalToken()
        {
            // Token[] tokens = {
            //     Token.Create(Token.EXPRESSION_TOKEN, null),
            //     Token.Create(Token.VARIABLE_TOKEN, "A"),
            //     Token.Create(Token.OPERATION_TOKEN, "=="),
            //     Token.Create(Token.VARIABLE_TOKEN, "B"),
            //     Token.Create(Token.OPERATION_TOKEN, "||"),
            //     Token.Create(Token.VARIABLE_TOKEN, "C"),
            //     Token.Create(Token.OPERATION_TOKEN, "+"),
            //     Token.Create(Token.VARIABLE_TOKEN, "D"),
            //     Token.Create(Token.OPERATION_TOKEN, "*"),
            //     Token.Create(Token.VARIABLE_TOKEN, "E"),
            //     Token.Create(Token.OPERATION_TOKEN, "-"),
            //     Token.Create(Token.VARIABLE_TOKEN, "F"),
            //     Token.Create(Token.OPERATION_TOKEN, ">="),
            //     Token.Create(Token.VARIABLE_TOKEN, "G"),
            //     Token.Create(Token.OPERATION_TOKEN, "+"),
            //     Token.Create(Token.PREFIX_TOKEN, "-"),
            //     Token.Create(Token.VARIABLE_TOKEN, "H"),
            //     Token.Create(Token.END_EXPRESSION_TOKEN, null),
            //     Token.Create(Token.OPERATION_TOKEN, "?"),
            //     Token.Create(Token.VARIABLE_TOKEN, "I"),
            // };

            Token[] tokens = {
                Token.CreatePrefix("!"),
                Token.Create(TokenType.Expression, null),
                (Token)10,
                Token.CreateOperator(">="),
                (Token)15,
                Token.CreateOperator("||"),
                (Token)6,
                Token.CreateOperator("+"),
                (Token)3,
                Token.CreateOperator("*"),
                (Token)11,
                Token.CreateOperator("-"),
                (Token)4,
                Token.CreateOperator("=="),
                (Token)12,
                Token.CreateOperator("+"),
                Token.CreatePrefix("-"),
                (Token)5,
                Token.Create(TokenType.EndExpression, null),
                Token.CreateOperator("?"),
                (Token)"Got It!!!",
            };

            // tokens = new Token[] {
            //     12,
            //     Token.Create(Token.OPERATION_TOKEN, "+"),
            //     Token.Create(Token.PREFIX_TOKEN, "-"),
            //     5,
            // };

            Stopwatch watch = new Stopwatch();

            watch.Start();
            ParseTree tree = null;
            int i = 0;
            for (; i < 1; i++)
            {
                tree = new ParseTree();
                foreach (Token token in tokens)
                {
                    if (!tree.Append(token))
                    {
                        Console.WriteLine(token.Lexeme);
                        break;
                    }
                    //Console.WriteLine(token);
                }
                switch(i)
                {
                    case 1000:
                    case 5000:
                        Console.WriteLine("run {0,-8} {1,5}ms", i, watch.ElapsedMilliseconds);
                        break;
                }
                //Console.WriteLine(tree.Root.Token.Lexeme);
            }
            watch.Stop();
            Console.WriteLine("run {0,-8} {1,5}ms", i, watch.ElapsedMilliseconds);

            Console.WriteLine(ParseTree.Evaluate(tree).ToString());
            Console.WriteLine();
        }
    }
}