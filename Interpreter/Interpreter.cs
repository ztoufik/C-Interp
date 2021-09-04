using System.Collections.Generic;
using Interpreter.Tokenize;
using Interpreter.AST;
using Interpreter.Error;

namespace Interpreter {

    public class Parser{
        
        private LinkedList<Token> _tokens;
        private Expr _ast;

        public LinkedList<Token> Tokens{
            get {return _tokens;}
            set {_tokens=value;}
        }

        public Expr AST{
            get {return _ast;}
        }

        public Parser(){ }

        public Parser(LinkedList<Token> tokens){
            this._tokens=tokens;
        }

        public void Parse(){
            _ast=Exprs(_tokens);
        }

        private Expr Exprs(LinkedList<Token> tokens){
            if (tokens.Count==0){
                throw new ParserError("empty tokens stream");
            }

            var FactorTokens=new LinkedList<Token>();
            while(tokens.Count>0){
                Token token=tokens.Last.Value;
                tokens.RemoveLast();
                switch(token.type){
                    case TokensType.Add:
                        return new AddExpr(
                                Exprs(tokens),
                                Factor(FactorTokens));
                    case TokensType.Sub:
                        return new SubExpr(
                                Exprs(tokens),
                                Factor(FactorTokens));
                    case TokensType.Eof:continue;
                    default: FactorTokens.AddFirst(token);break;
                }
            }
            return Factor(FactorTokens);
        }

        private Expr Factor(LinkedList<Token> tokens){
            if (tokens.Count==0){
                throw new ParserError("empty tokens stream");
            }

            var TermTokens=new LinkedList<Token>();
            while(tokens.Count>0){
                Token token=tokens.Last.Value;
                tokens.RemoveLast();
                switch(token.type){
                    case TokensType.Mul:
                        return new MulExpr(
                                Exprs(tokens),
                                Term(TermTokens));
                    case TokensType.Div:
                        return new DivExpr(
                                Exprs(tokens),
                                Term(TermTokens));
                    case TokensType.Eof:continue;
                    default:TermTokens.AddFirst(token);break;
                }
            }
            return Term(TermTokens);
        }

        private Expr Term(LinkedList<Token> tokens){
            if (tokens.Count==0){
                throw new ParserError("empty tokens stream");
            }

            if (tokens.Count>1){
                throw new ParserError("More than one Number token");
            }
            return new Number(tokens.First.Value.Value);
        }

    }
}
