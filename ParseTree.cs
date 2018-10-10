using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CriteriaExpression
{
    class ParseTree
    {
        static private SortedList<TokenType, int> TokenPriorityTable;
        static private SortedList<string, int> OperatorPriorityTable;

        private Stack<Operand> _expressionNodes;
        private Operand _root;
        private Operand _last;

        public Operand Root
        {
            get
            {
                if (_root._parent != null)
                {
                    Operand parent = _root._parent;
                    while (parent._parent != null)
                    {
                        parent = parent._parent;
                    }
                    _root = parent;
                }
                return _root;
            }
        }
        internal Operand Last
        {
            get { return _last; }
        }

        static ParseTree()
        {
            TokenPriorityTable = new SortedList<TokenType, int>();
            TokenPriorityTable.Add(TokenType.Boolean   , 0);
            TokenPriorityTable.Add(TokenType.Number    , 0);
            TokenPriorityTable.Add(TokenType.String    , 0);
            TokenPriorityTable.Add(TokenType.Variable  , 0);
            TokenPriorityTable.Add(TokenType.Function  , 0);
            TokenPriorityTable.Add(TokenType.Expression, 0);

            TokenPriorityTable.Add(TokenType.Prefix    , 0x11);
            TokenPriorityTable.Add(TokenType.Operation , 0x12);

            OperatorPriorityTable = new SortedList<string, int>();
            OperatorPriorityTable.Add("?" , 0);
            OperatorPriorityTable.Add("||", 1);
            OperatorPriorityTable.Add("&&", 1);

            OperatorPriorityTable.Add("==", 0x11);
            OperatorPriorityTable.Add("!=", 0x11);
            OperatorPriorityTable.Add(">=", 0x11);
            OperatorPriorityTable.Add(">" , 0x11);
            OperatorPriorityTable.Add("<=", 0x11);
            OperatorPriorityTable.Add("<" , 0x11);

            OperatorPriorityTable.Add("+", 0x21);
            OperatorPriorityTable.Add("-", 0x21);
            OperatorPriorityTable.Add("*", 0x22);
            OperatorPriorityTable.Add("/", 0x22);
            OperatorPriorityTable.Add("%", 0x22);
        }

        internal ParseTree()
        {
            _expressionNodes = new Stack<Operand>();
        }

        public bool Append(Token token)
        {
            if ((token.Type == TokenType.None ) ||
                (token.Type == TokenType.Function) ||
                (token.Type == TokenType.Variable))
                return false;

            Operand newNode = null;
            if (_last == null)
            {
                newNode = new Operand(token, null);
                _last = newNode;
                _root = newNode;
            }
            else
            {
                if (token.Type == TokenType.EndExpression)
                {
                    Operand expressionNode;
                    bool found = _expressionNodes.TryPop(out expressionNode);
                    if (!found)
                    {
                        return false;
                    }
                    _last = expressionNode;
                    return true;
                }

                Operand root = this.Root;
                Operand prev = null;
                Operand node = _last;
                int comparison;
                while (node != null)
                {
                    comparison = CompareTokenPriority(node, token);
                    if (node == root && comparison <= 0)
                    {
                        newNode = new Operand(token, null);
                        newNode._left = node;
                        node._parent = newNode;

                        _root = newNode;
                        _last = newNode;
                        break;
                    }

                    if (comparison == 0)
                    {
                        Operand parent = node._parent;
                        Debug.Assert(parent != null);
                        newNode = new Operand(token, parent);

                        if (node.IsLeft)
                        {
                            if (parent != null)
                            {
                                if (!CanAppendLeft(parent, token))
                                    return false;

                                parent._left = newNode;
                            }
                            newNode._left = node;
                            node._parent = newNode;
                        }
                        else
                        {
                            if (parent != null)
                            {
                                if (!CanAppendRight(parent, token))
                                    return false;

                                parent._right = newNode;
                            }
                            newNode._left = node;
                            node._parent = newNode;
                        }
                        _last = newNode;
                        break;
                    }
                    else if (comparison > 0)
                    {
                        Operand parent = node;
                        newNode = new Operand(token, parent);
                        switch(parent.Token.Type)
                        {
                            case TokenType.Expression:
                            case TokenType.Function:
                                if (_expressionNodes.Count > 0 && _expressionNodes.Peek() == parent)
                                {
                                    if (!CanAppendLeft(parent, token))
                                        return false;

                                    Operand child = parent._left;
                                    parent._left = newNode;
                                    if (child != null)
                                    {
                                        newNode._left = child;
                                        child._parent = newNode;
                                    }
                                }
                                else
                                {
                                    if (!CanAppendRight(parent, token))
                                        return false;

                                    Debug.Assert(parent._right == null);
                                    parent._right = newNode;
                                }
                                break;

                            default:
                                {
                                    if (prev != null)
                                    {
                                        Operand child = prev;
                                        if (prev.IsLeft)
                                        {
                                            if (!CanAppendLeft(parent, token))
                                                return false;

                                            parent._left = newNode;
                                            newNode._left = prev;
                                            prev._parent = newNode;
                                        }
                                        else
                                        {
                                            if (!CanAppendRight(parent, token))
                                                return false;

                                            parent._right = newNode;
                                            newNode._left = prev;
                                            prev._parent = newNode;
                                        }
                                    }
                                    else
                                    {
                                        if (!CanAppendRight(parent, token))
                                            return false;

                                        Debug.Assert(parent._right == null);
                                        parent._right = newNode;
                                    }
                                }
                                break;
                        }
                        _last = newNode;
                        break;
                    }

                    prev = node;
                    node = node._parent;
                }
            }

            if (newNode != null)
            {
                switch(newNode.Token.Type)
                {
                    case TokenType.Expression:
                    case TokenType.Function:
                        _expressionNodes.Push(newNode);
                        break;
                }
                return true;
            }
            return false;
        }

        private bool CanAppendRight(Operand node, Token token)
        {
            switch(node.Token.Type)
            {
                case TokenType.Boolean:
                case TokenType.Number:
                case TokenType.String:
                case TokenType.Variable:
                case TokenType.Function:
                    if (node.Parent != null)
                    {
                        return (node.Parent.Token.Type == TokenType.Function);
                    }
                    return false;

                case TokenType.Prefix:
                    switch(token.Type)
                    {
                        case TokenType.Boolean:
                        case TokenType.Number:
                        case TokenType.String:
                        case TokenType.Variable:
                        case TokenType.Function:
                        case TokenType.Expression:
                        return true;
                    }
                    return false;

                case TokenType.Expression:
                case TokenType.Operation:
                    return true;

                default:
                    throw new InvalidOperationException();
            }
        }
        private bool CanAppendLeft(Operand node, Token token)
        {
            switch(node.Token.Type)
            {
                case TokenType.Boolean:
                case TokenType.Number:
                case TokenType.String:
                case TokenType.Variable:
                case TokenType.Prefix:
                    return false;

                case TokenType.Function:
                case TokenType.Expression:
                case TokenType.Operation:
                    return true;

                default:
                    throw new InvalidOperationException();
            }
        }
        private int CompareTokenPriority(Operand node, Token y)
        {
            if ((_expressionNodes.Count > 0) && (_expressionNodes.Peek() == node))
            {
                return 1;
            }
            Token x = node.Token;
            int comparison = Comparer<int>.Default.Compare(
                TokenPriorityTable.GetValueOrDefault(x.Type, 0),
                TokenPriorityTable.GetValueOrDefault(y.Type, 0));
            if (comparison == 0)
            {
                if ((x.Type == TokenType.Operation) && (y.Type == TokenType.Operation))
                {
                    return -CompareOperatorPriority(x.Lexeme, y.Lexeme);
                }
            }
            return comparison;
        }
        private int CompareOperatorPriority(string x, string y)
        {
            return
            Comparer<int>.Default.Compare(
                OperatorPriorityTable.GetValueOrDefault(x, 0),
                OperatorPriorityTable.GetValueOrDefault(y, 0));
        }

        static internal Token Evaluate(ParseTree tree)
        {
            try
            {
                return tree.Root.Solve();
            }
            catch(NullReferenceException)
            {
                throw new InvalidCastException();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
