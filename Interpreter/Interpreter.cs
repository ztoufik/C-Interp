//using System;
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

            Expr MultplNode=ParseTerm(tokens);


            var token=tokens.First.Value;

            while((token.type==TokensType.Mul) || (token.type==TokensType.Div)){
                tokens.RemoveFirst();
                if(token.type==TokensType.Mul){
                        MultplNode= new MulExpr(MultplNode,ParseTerm(tokens));
                }
                else {
                        MultplNode= new DivExpr(MultplNode,ParseTerm(tokens));
                }
                token=tokens.First.Value;
            }
            return MultplNode;
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

            //Console.WriteLine("before proc");
            //foreach(var tk in tokens){
            //    Console.WriteLine(tk);
            //}
            
            //Console.WriteLine("after proc");
            //foreach(var tk in tokens){
            //    Console.WriteLine(tk);
            //}
