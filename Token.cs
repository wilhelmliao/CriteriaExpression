using System;
using System.Runtime.InteropServices;

namespace CriteriaExpression
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Token
    {
        static private Token NullToken = new Token();
        static public Token Null
        {
            get { return NullToken; }
        }

        private TokenType _type;
        private int _int;
        private string _lexeme;

        public TokenType Type
        {
            get { return _type; }
        }
        public int Int
        {
            get
            {
                switch (_type)
                {
                    case TokenType.Number:
                        return _int;

                    default:
                        throw new InvalidCastException();
                }
            }
        }
        public bool Boolean
        {
            get
            {
                switch (_type)
                {
                    case TokenType.Boolean:
                        return _int == 1;

                    default:
                        throw new InvalidCastException();
                }
            }
        }
        public string String
        {
            get
            {
                switch (_type)
                {
                    case TokenType.String:
                        return _lexeme;

                    default:
                        throw new InvalidCastException();
                }
            }
        }
        public string Lexeme
        {
            get { return _lexeme; }
        }

        public override bool Equals(object obj)
        {
            if (obj is Token)
            {
                return Equals((Token)obj);
            }
            return false;
        }
        public bool Equals(Token token)
        {
            return this == token;
        }
        public override int GetHashCode()
        {
            switch(_type)
            {
                case TokenType.Boolean:
                    return Boolean.GetHashCode();

                case TokenType.Number:
                    return Int.GetHashCode();

                case TokenType.String:
                    return String.GetHashCode();
            }
            return (int)_type;
        }
        public override string ToString()
        {
            switch (_type)
            {
                case TokenType.Expression:
                    return "(";

                case TokenType.EndExpression:
                    return ")";

                case TokenType.Number:
                    return _int.ToString();

                default:
                    return _lexeme;
            }
        }

        static internal Token Create(TokenType type, string lexeme)
        {
            Token token = new Token();
            token._type = type;
            token._lexeme = lexeme;
            return token;
        }
        static internal Token CreatePrefix(string lexeme)
        {
            if (lexeme == null)
                throw new ArgumentNullException("lexeme");

            switch(lexeme)
            {
                case "!":
                case "-":
                    break;

                default:
                    throw new ArgumentOutOfRangeException("lexeme", "Unknown prefix '" + lexeme + "'");
            }
            Token token = new Token();
            token._type = TokenType.Prefix;
            token._lexeme = lexeme;
            return token;
        }
        static internal Token CreateOperator(string lexeme)
        {
            if (lexeme == null)
                throw new ArgumentNullException("lexeme");

            switch(lexeme)
            {
                case "?":
                case "||":
                case "&&":

                case "==":
                case "!=":
                case ">=":
                case "<=":
                case ">":
                case "<":

                case "+":
                case "-":
                case "*":
                case "/":
                case "%":
                    break;

                default:
                    throw new ArgumentOutOfRangeException("lexeme", "Unknown operator '" + lexeme + "'");
            }
            Token token = new Token();
            token._type = TokenType.Operation;
            token._lexeme = lexeme;
            return token;
        }
        static public bool IsNull(Token token)
        {
            return token._type == TokenType.None;
        }
        static public implicit operator Token(string value)
        {
            Token token = new Token();
            token._type = TokenType.String;
            token._lexeme = value;
            return token;
        }
        static public implicit operator Token(int value)
        {
            Token token = new Token();
            token._type = TokenType.Number;
            token._int = value;
            return token;
        }
        static public implicit operator Token(bool value)
        {
            Token token = new Token();
            token._type = TokenType.Boolean;
            token._int = value ? 1 : 0;
            return token;
        }
        static public implicit operator bool(Token value)
        {
            if (value._type == TokenType.Boolean)
            {
                return value.Boolean;
            }
            throw new InvalidCastException();
        }
        static public implicit operator int(Token value)
        {
            if (value._type == TokenType.Number)
            {
                return value.Int;
            }
            throw new InvalidCastException();
        }
        static public implicit operator string(Token value)
        {
            if (value._type == TokenType.String)
            {
                return value.String;
            }
            throw new InvalidCastException();
        }
        static public bool operator true(Token x)
        {
            if (x._type == TokenType.Boolean)
            {
                return x._int == 1;
            }
            throw new InvalidCastException();
        }
        static public bool operator false(Token x)
        {
            if (x._type == TokenType.Boolean)
            {
                return x._int == 0;
            }
            throw new InvalidCastException();
        }
        static public bool operator ==(Token x, Token y)
        {
            if (x._type == y._type)
            {
                switch (x._type)
                {
                    case TokenType.Boolean:
                    case TokenType.Number:
                        return x._int == y._int;

                    case TokenType.String:
                        return x._lexeme == y._lexeme;
                }
            }
            throw new InvalidOperationException();
        }
        static public bool operator !=(Token x, Token y)
        {
            if (x._type == y._type)
            {
                switch (x._type)
                {
                    case TokenType.Boolean:
                    case TokenType.Number:
                        return x._int != y._int;

                    case TokenType.String:
                        return x._lexeme != y._lexeme;
                }
            }
            throw new InvalidOperationException();
        }
        static public bool operator >=(Token x, Token y)
        {
            if (x._type == TokenType.Number && y._type == TokenType.Number)
            {
                return x._int >= y._int;
            }
            throw new InvalidOperationException();
        }
        static public bool operator <=(Token x, Token y)
        {
            if (x._type == TokenType.Number && y._type == TokenType.Number)
            {
                return x._int <= y._int;
            }
            throw new InvalidOperationException();
        }
        static public bool operator >(Token x, Token y)
        {
            if (x._type == TokenType.Number && y._type == TokenType.Number)
            {
                return x._int > y._int;
            }
            throw new InvalidOperationException();
        }
        static public bool operator <(Token x, Token y)
        {
            if (x._type == TokenType.Number && y._type == TokenType.Number)
            {
                return x._int < y._int;
            }
            throw new InvalidOperationException();
        }
        static public int operator +(Token x, Token y)
        {
            if (x._type == TokenType.Number && y._type == TokenType.Number)
            {
                return x._int + y._int;
            }
            throw new InvalidOperationException();
        }
        static public int operator -(Token x, Token y)
        {
            if (x._type == TokenType.Number && y._type == TokenType.Number)
            {
                return x._int - y._int;
            }
            throw new InvalidOperationException();
        }
        static public int operator *(Token x, Token y)
        {
            if (x._type == TokenType.Number && y._type == TokenType.Number)
            {
                return x._int * y._int;
            }
            throw new InvalidOperationException();
        }
        static public int operator /(Token x, Token y)
        {
            if (x._type == TokenType.Number && y._type == TokenType.Number)
            {
                return x._int / y._int;
            }
            throw new InvalidOperationException();
        }
        static public int operator %(Token x, Token y)
        {
            if (x._type == TokenType.Number && y._type == TokenType.Number)
            {
                return x._int % y._int;
            }
            throw new InvalidOperationException();
        }
    }
}