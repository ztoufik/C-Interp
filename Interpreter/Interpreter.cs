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
            _ast=ParseAdd(_tokens);
        }

        private Expr ParseAdd(LinkedList<Token> tokens){

            Expr AddtvNode=ParseMul(tokens);

            var token=tokens.First.Value;

            while((token.type==TokensType.Add) || (token.type==TokensType.Sub)){
                tokens.RemoveFirst();
                if(token.type==TokensType.Add){
                        AddtvNode= new AddExpr(AddtvNode,ParseMul(tokens));
                }
                else {
                        AddtvNode= new SubExpr(AddtvNode,ParseMul(tokens));
                }
             token=tokens.First.Value;
            }
            return AddtvNode;
        }

        private Expr ParseMul(LinkedList<Token> tokens){

            Expr MultplNode=ParseParenth(tokens);

            var token=tokens.First.Value;

            while((token.type==TokensType.Mul) || (token.type==TokensType.Div)){
                tokens.RemoveFirst();
                if(token.type==TokensType.Mul){
                        MultplNode= new MulExpr(MultplNode,ParseParenth(tokens));
                }
                else {
                        MultplNode= new DivExpr(MultplNode,ParseParenth(tokens));
                }
                token=tokens.First.Value;
            }
            return MultplNode;
        }

        private Expr ParseParenth(LinkedList<Token> tokens){

            var node=tokens.First.Value;

            if(node.type==TokensType.LP){
                int LPN=1;
                tokens.RemoveFirst();
                var tokensstream=new LinkedList<Token>();

                node=tokens.First.Value;
                while(LPN>0){
                    if(node.type==TokensType.LP){
                        LPN++;
                    }

                    if(node.type==TokensType.RP){
                        LPN--;
                    }
                    tokensstream.AddLast(node);
                    tokens.RemoveFirst();
                    if(tokens.Count<=0){
                        throw new ParserError("missing )");
                    }
                    node=tokens.First.Value;
                }
                tokensstream.AddLast(new Token(TokensType.Eof,null));
                return ParseAdd(tokensstream);
            }

            return ParseUnOpr(tokens);
        }

        private Expr ParseUnOpr(LinkedList<Token> tokens){

            var node=tokens.First.Value;
            switch(node.type){
                case TokensType.Add:
                    tokens.RemoveFirst();
                    return new PlusExpr(ParseTerm(tokens));
                case TokensType.Sub:
                    tokens.RemoveFirst();
                    return new MinusExpr(ParseTerm(tokens));
                default:return ParseTerm(tokens);
            }
        }

        private Expr ParseTerm(LinkedList<Token> tokens){

            if (tokens.Count==0){
                throw new ParserError("empty tokens stream");
            }

            var node=tokens.First.Value;
            tokens.RemoveFirst();
            switch(node.type){
                case TokensType.Number:return new Number(node.Value);
                default:throw new ParserError("expected Number token");
            }
        }

    }

}
