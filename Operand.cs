using System;

namespace CriteriaExpression
{
    class Operand
    {
        private Token _value;
        internal Operand _parent;
        internal Operand _left;
        internal Operand _right;

        public Token Token
        {
            get { return _value; }
        }
        public Operand Parent
        {
            get { return _parent; }
        }
        public Operand Left
        {
            get { return _left; }
        }
        public Operand Right
        {
            get { return _right; }
        }
        internal bool IsLeft
        {
            get
            {
                if (_parent != null)
                {
                    return _parent._left == this;
                }
                return false;
            }
        }

        internal Operand(Token value, Operand parent)
        {
            _parent = parent;
            _value = value;
        }

        public Token Solve()
        {
            return Solve(this);
        }

        static private Token Solve(Operand op)
        {
            if (op == null)
                throw new InvalidProgramException("syntax error");

            switch (op.Token.Type)
            {
                case TokenType.Boolean:
                    return op.Token;

                case TokenType.Number:
                    return op.Token;

                case TokenType.String:
                    return op.Token;

                case TokenType.Prefix:
                    if (op.Right != null)
                    {
                        Token value = Solve(op.Right);

                        switch (op.Token.Lexeme)
                        {
                            case "!":
                                return !value;

                            case "-":
                                return -value;

                            default:
                                throw new InvalidProgramException("unknown prefix '" + op.Token.Lexeme + "'");
                        }
                    }
                    throw new InvalidProgramException("syntax error");

                case TokenType.Operation:
                    if ((op.Left != null) && (op.Right != null))
                    {
                        Token left = Solve(op.Left);
                        Token right = Solve(op.Right);

                        switch (op.Token.Lexeme)
                        {
                            case "?":
                                if (left)
                                {
                                    return right;
                                }
                                return false;

                            case "||":
                                if (left)
                                {
                                    return true;
                                }
                                return right;

                            case "&&":
                                if (!left)
                                {
                                    return false;
                                }
                                return right;

                            case "==":
                                return left.Equals(right);

                            case "!=":
                                return !left.Equals(right);

                            case ">=":
                                return left >= right;

                            case ">":
                                return left > right;

                            case "<=":
                                return left <= right;

                            case "<":
                                return left < right;

                            case "+":
                                return left + right;

                            case "-":
                                return left - right;

                            case "*":
                                return left * right;

                            case "/":
                                return left / right;

                            case "%":
                                return left % right;

                            default:
                                throw new InvalidProgramException("unknown operator '" + op.Token.Lexeme + "'");
                        }
                    }
                    throw new InvalidProgramException("syntax error");

                case TokenType.Expression:
                    return Solve(op.Left);
            }
            return Token.Null;
        }
    }
}