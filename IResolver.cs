using System;

namespace CriteriaExpression
{
    public interface IResolver
    {
        Token HandleVariable(string lexeme);
        Token HandleFunction(string lexeme, Token[] args);
    }

    public delegate Token ResolveFunc(TokenType type, string lexeme, Token[] args);


    public abstract class Resolver : IResolver
    {
        static private Resolver DefaultResolverInstance = null;
        static public Resolver Default
        {
            get
            {
                System.Threading.Interlocked.CompareExchange<Resolver>(
                    ref DefaultResolverInstance,
                    new _DefaultResolver(),
                    null);
                return DefaultResolverInstance;
            }
        }
        public abstract Token HandleVariable(string lexeme);
        public abstract Token HandleFunction(string lexeme, Token[] args);


        static internal Resolver CreateResolver(ResolveFunc func)
        {
            return new ResolveFuncWrapper(func);
        }

        internal class _DefaultResolver : Resolver, IResolver
        {
            internal _DefaultResolver()
            {
                // do nothing ...
            }

            public override Token HandleVariable(string lexeme)
            {
                return Token.Null;
            }
            public override Token HandleFunction(string lexeme, Token[] args)
            {
                return Token.Null;
            }
        }
    }

    class ResolveFuncWrapper : Resolver, IResolver
    {
        private ResolveFunc _resolve; 

        internal ResolveFuncWrapper(ResolveFunc func)
        {
            if (func == null)
                throw new ArgumentNullException("func");

            _resolve = func;
        }

        public override Token HandleVariable(string lexeme)
        {
            return _resolve.Invoke(TokenType.Variable, lexeme, null);
        }
        public override Token HandleFunction(string lexeme, Token[] args)
        {
            return _resolve.Invoke(TokenType.Function, lexeme, args);
        }

    }
}