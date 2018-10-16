using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CriteriaExpression
{
    public class Interpreter : IResolver
    {
        private IResolver _resolver;

        public Interpreter() : this(Resolver.Default)
        {
        }
        public Interpreter(IResolver resolver)
        {
            if (resolver == null)
                throw new ArgumentNullException("resolver");

            _resolver = resolver;
        }

        public object Evaluate(string expression)
        {
            return Evaluate(expression, this);
        }

        #region IResolver members

        Token IResolver.HandleVariable(string lexeme)
        {
            if(this._resolver != null)
            {
                return _resolver.HandleVariable(lexeme);
            }
            return Token.Null;
        }
        Token IResolver.HandleFunction(string lexeme, Token[] args)
        {
            if (this._resolver != null)
            {
                return _resolver.HandleFunction(lexeme, args);
            }
            return Token.Null;
        }

        #endregion

        static private ParseTree Parse(string expression, IResolver resolver)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            ParseTree tokenTree = new ParseTree();
            StringBuilder literal = new StringBuilder();
            CharReader reader = new CharReader(expression);
            Token last = Token.Null;

            while (reader.Next())
            {
                Debug.Assert(literal.Length == 0);
                char c = reader.Read();

                switch (c)
                {
                    case '(':
                        {
                            switch (last.Type)
                            {
                                case TokenType.Prefix:
                                case TokenType.Operation:
                                case TokenType.Expression:
                                case TokenType.None:
                                    last = Token.Create(TokenType.Expression, null);
                                    tokenTree.Append(last);
                                    break;
                                default:
                                    throw ThrowSyntaxError(reader.Position);
                            }
                        }
                        break;

                    case ')':
                        {
                            switch (last.Type)
                            {
                                case TokenType.Boolean:
                                case TokenType.Number:
                                case TokenType.String:
                                case TokenType.Variable:
                                case TokenType.Function:
                                case TokenType.EndExpression:
                                    last = Token.Create(TokenType.EndExpression, null);
                                    tokenTree.Append(last);
                                    break;
                                default:
                                    throw ThrowSyntaxError(reader.Position);
                            }
                        }
                        break;

                    case '?':
                        switch (last.Type)
                        {
                            case TokenType.Boolean:
                            case TokenType.Number:
                            case TokenType.String:
                            case TokenType.Variable:
                            case TokenType.Function:
                            case TokenType.EndExpression:
                                last = Token.CreateOperator(c.ToString());
                                tokenTree.Append(last);
                                break;

                            default:
                                throw ThrowSyntaxError(reader.Position);
                        }
                        break;

                    case '|':
                    case '&':
                        switch (last.Type)
                        {
                            case TokenType.Boolean:
                            case TokenType.Number:
                            case TokenType.String:
                            case TokenType.Variable:
                            case TokenType.Function:
                            case TokenType.Expression:
                                literal.Append(c);
                                if (!reader.Next())
                                    throw ThrowSyntaxError(reader.Position);
                                if (reader.Current != c)
                                    throw ThrowSyntaxError(reader.Position);

                                literal.Append(reader.Read());

                                last = Token.CreateOperator(literal.ToString());
                                tokenTree.Append(last);
                                literal.Length = 0;
                                break;

                            default:
                                throw ThrowSyntaxError(reader.Position);
                        }
                        break;

                    case '!':
                        switch (last.Type)
                        {
                            case TokenType.Operation:
                            case TokenType.Expression:
                            case TokenType.None:
                                // prefix
                                last = Token.CreatePrefix(c.ToString());
                                tokenTree.Append(last);
                                break;

                            case TokenType.Boolean:
                            case TokenType.Number:
                            case TokenType.String:
                            case TokenType.Variable:
                            case TokenType.Function:
                            case TokenType.EndExpression:
                                // operator
                                literal.Append(c);
                                if (!reader.Next())
                                    throw ThrowSyntaxError(reader.Position);
                                if (reader.Current != '=')
                                    throw ThrowSyntaxError(reader.Position);

                                literal.Append(reader.Read());

                                last = Token.CreateOperator(literal.ToString());
                                tokenTree.Append(last);
                                literal.Length = 0;
                                break;

                            default:
                                throw ThrowSyntaxError(reader.Position);
                        }
                        break;

                    case '-':
                        switch (last.Type)
                        {
                            case TokenType.Operation:
                            case TokenType.Expression:
                            case TokenType.None:
                                // prefix
                                last = Token.CreatePrefix(c.ToString());
                                tokenTree.Append(last);
                                break;

                            case TokenType.Boolean:
                            case TokenType.Number:
                            case TokenType.String:
                            case TokenType.Variable:
                            case TokenType.Function:
                            case TokenType.EndExpression:
                                // operator
                                last = Token.CreateOperator(c.ToString());
                                tokenTree.Append(last);
                                break;
                            default:
                                throw ThrowSyntaxError(reader.Position);
                        }
                        break;

                    case '+':
                    case '*':
                    case '/':
                    case '%':
                        switch (last.Type)
                        {
                            case TokenType.Boolean:
                            case TokenType.Number:
                            case TokenType.String:
                            case TokenType.Variable:
                            case TokenType.Function:
                            case TokenType.EndExpression:
                                last = Token.CreateOperator(c.ToString());
                                tokenTree.Append(last);
                                break;

                            default:
                                throw ThrowSyntaxError(reader.Position);
                        }
                        break;

                    case '>':
                    case '<':
                        switch (last.Type)
                        {
                            case TokenType.Boolean:
                            case TokenType.Number:
                            case TokenType.String:
                            case TokenType.Variable:
                            case TokenType.Function:
                            case TokenType.EndExpression:
                                literal.Append(c);
                                if (!reader.Next())
                                    throw ThrowSyntaxError(reader.Position);
                                if (reader.Current == '=')
                                {
                                    literal.Append(reader.Read());
                                }
                                last = Token.CreateOperator(literal.ToString());
                                tokenTree.Append(last);
                                literal.Length = 0;
                                break;

                            default:
                                throw ThrowSyntaxError(reader.Position);
                        }
                        break;

                    case '=':
                        switch (last.Type)
                        {
                            case TokenType.Boolean:
                            case TokenType.Number:
                            case TokenType.String:
                            case TokenType.Variable:
                            case TokenType.Function:
                            case TokenType.EndExpression:
                                literal.Append(c);
                                if (!reader.Next())
                                    throw ThrowSyntaxError(reader.Position);
                                if (reader.Current != '=')
                                    throw ThrowSyntaxError(reader.Position);

                                literal.Append(reader.Read());

                                last = Token.CreateOperator(literal.ToString());
                                tokenTree.Append(last);
                                literal.Length = 0;
                                break;

                            default:
                                throw ThrowSyntaxError(reader.Position);
                        }
                        break;

                    case '\'':
                        switch (last.Type)
                        {
                            case TokenType.Operation:
                            case TokenType.Expression:
                            case TokenType.None:
                                last = ExpandString(reader, literal);
                                tokenTree.Append(last);
                                literal.Length = 0;
                                break;
                            default:
                                throw ThrowSyntaxError(reader.Position);
                        }
                        break;

                    default:
                        if (char.IsLetter(c))
                        {
                            switch (last.Type)
                            {
                                case TokenType.Prefix:
                                case TokenType.Operation:
                                case TokenType.Expression:
                                case TokenType.None:
                                    literal.Append(c);
                                    last = ExpandVariable(reader, literal, resolver);
                                    tokenTree.Append(last);
                                    literal.Length = 0;
                                    break;
                                default:
                                    throw ThrowSyntaxError(reader.Position);
                            }
                        }
                        else if (char.IsDigit(c))
                        {
                            switch (last.Type)
                            {
                                case TokenType.Prefix:
                                case TokenType.Operation:
                                case TokenType.Expression:
                                case TokenType.None:
                                    literal.Append(c);
                                    last = ExpandNumber(reader, literal);
                                    tokenTree.Append(last);
                                    literal.Length = 0;
                                    break;
                                default:
                                    throw ThrowSyntaxError(reader.Position);
                            }
                        }
                        else
                        {
                            // it is whitespace or newline, to do nothing...
                        }
                        break;
                }
            }
            return tokenTree;
        }

        static private Token ExpandVariable(CharReader reader, StringBuilder literal, IResolver resolver)
        {
            char c;
            while (reader.Next())
            {
                c = reader.Current;
                if (char.IsLetterOrDigit(c))
                {
                    literal.Append(c);
                }
                else
                {
                    if (literal.ToString() == "true")
                    {
                        return true;
                    }
                    else if (literal.ToString() == "false")
                    {
                        return false;
                    }
                    else if (char.IsWhiteSpace(c))
                    {
                        continue;
                    }
                    else if (c == '(')
                    {
                        string name = literal.ToString();
                        literal.Length = 0;
                        reader.Read(); // skip the char
                        Token[] args = ExpandParameters(reader, literal, resolver);
                        Token result = resolver.HandleFunction(name, args);
                        return result;
                    }
                    else
                    {
                        Token result = resolver.HandleVariable(literal.ToString());
                        return result;
                    }
                }
                reader.Read();
            }
            return Token.Null;
        }

        static private Token[] ExpandParameters(CharReader reader, StringBuilder literal, IResolver resolver)
        {
            List<Token> tokens = new List<Token>();
            while (reader.Next())
            {
                char c = reader.Current;
                switch (c)
                {
                    case ',':
                        if (literal.Length == 0)
                            throw ThrowSyntaxError(reader.Position);

                        literal.Length = 0;
                        break;

                    case ')':
                        literal.Length = 0;
                        return tokens.ToArray();

                    case '\'':
                        reader.Read(); //skip the char
                        tokens.Add(ExpandString(reader, literal));
                        continue;

                    default:
                        if (char.IsLetter(c))
                        {
                            tokens.Add(ExpandVariable(reader, literal, resolver));
                            continue;
                        }
                        else if (char.IsDigit(c))
                        {
                            tokens.Add(ExpandNumber(reader, literal));
                            continue;
                        }
                        else
                        {
                            // it is whitespace or newline, to do nothing...
                        }
                        break;
                }
                literal.Length = 0;
                reader.Read();
            }
            throw ThrowSyntaxError(reader.Position);
        }

        static private Token ExpandNumber(CharReader reader, StringBuilder literal)
        {
            char c;
            while (reader.Next())
            {
                c = reader.Current;
                if (char.IsDigit(c))
                {
                    literal.Append(c);
                }
                else
                {
                    break;
                }
                reader.Read();
            }
            return int.Parse(literal.ToString());
        }

        static private Token ExpandString(CharReader reader, StringBuilder literal)
        {
            bool expandEscapeChar = false;
            char c;

            while (reader.Next())
            {
                c = reader.Read();
                if (expandEscapeChar)
                {
                    literal.Append(c);
                }
                else if (c == '\\')
                {
                    expandEscapeChar = true;
                }
                else if (c == '\'')
                {
                    break;
                }
                else
                {
                    literal.Append(c);
                }
            }
            return literal.ToString();
        }

        static private Exception ThrowSyntaxError(int position)
        {
            return new ArgumentException(string.Format("Syntax error at {0}", position));
        }

        static public object Evaluate(string expression, ResolveFunc func)
        {
            IResolver resolver = Resolver.CreateResolver(func);
            return Evaluate(expression, resolver);
        }
        static public object Evaluate(string expression, IResolver resolver)
        {
            if (resolver == null)
                throw new ArgumentNullException("resolver");

            if (string.IsNullOrEmpty(expression))
                return null;

            ParseTree tree = Parse(expression, resolver);
            Token result = ParseTree.Evaluate(tree);
            switch (result.Type)
            {
                case TokenType.Boolean:
                    return result.Boolean;

                case TokenType.Number:
                    return result.Int;

                case TokenType.String:
                    return result.String;
            }
            return null;
        }

        class CharReader
        {
            string _data;
            int _position;

            public int Position
            {
                get { return _position; }
            }
            public char Current
            {
                get
                {
                    if (_position < _data.Length)
                    {
                        return _data[_position];
                    }
                    return '\0';
                }
            }

            public CharReader(string str)
            {
                this._data = str;
                this._position = 0;
            }

            public bool Next()
            {
                return _position < _data.Length;
            }
            public char Read()
            {
                if (_position < _data.Length)
                {
                    return _data[_position++];
                }
                return '\0';
            }
        }
    }
}
