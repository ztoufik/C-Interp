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
                    case TokensType.RP: do{
                                            FactorTokens.AddFirst(token);
                                            if(tokens.Count<=0)
                                                throw new ParserError("missing (");
                                            token=tokens.Last.Value;
                                            tokens.RemoveLast();
                                }while(token.type!=TokensType.LP);
                                            FactorTokens.AddFirst(token);
                                            break;
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

            var TermsTokens=new LinkedList<Token>();
            while(tokens.Count>0){
                Token token=tokens.Last.Value;
                tokens.RemoveLast();
                switch(token.type){
                    case TokensType.Mul:
                        return new MulExpr(
                                Exprs(tokens),
                                Terms(TermsTokens));
                    case TokensType.Div:
                        return new DivExpr(
                                Exprs(tokens),
                                Terms(TermsTokens));
                    case TokensType.RP: do{
                                            TermsTokens.AddFirst(token);
                                            if(tokens.Count<=0)
                                                throw new ParserError("missing (");
                                            token=tokens.Last.Value;
                                            tokens.RemoveLast();
                                }while(token.type!=TokensType.LP);
                                            TermsTokens.AddFirst(token);
                                            break;
                    default:TermsTokens.AddFirst(token);break;
                }
            }
            return Terms(TermsTokens);
        }

        private Expr Terms(LinkedList<Token> tokens){
            if (tokens.Count==0){
                throw new ParserError("empty tokens stream");
            }

            var last=tokens.Last.Value;
            var first=tokens.First.Value;
            if(last.type==TokensType.RP){
                if(first.type!=TokensType.LP){
                    throw new ParserError("missing (");
                }
                tokens.RemoveLast();
                tokens.RemoveFirst();
                return Exprs(tokens);
            }
            else {
                if(first.type==TokensType.LP){
                    throw new ParserError("missing )");
                }
                return Term(tokens);
            }
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
