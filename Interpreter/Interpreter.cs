using System;
using System.Collections.Generic;
using System.Linq;
using Interpreter.Tokenize;

namespace Interpreter {
    abstract public class Expr {
         abstract public object evaluate();
    }

    public abstract class Var:Expr {
        protected readonly string type;
        protected readonly object Value;

        public Var(string type,object Value)
        {
            this.type=type;
            this.Value=Value;
        }
    }

    public class Number:Var {
        public Number(string Value):base("Number",double.Parse(Value)) {
        }

        public override object evaluate() {
            return Value;
        }
    }

    abstract public class BinExpr:Expr {
        protected readonly Expr left;
        protected readonly Expr right;

        public BinExpr(Expr left,Expr right) {
            this.left=left;
            this.right=right;
        }
    }

    abstract public class ArthmExpr:BinExpr {
        public ArthmExpr(Expr left,Expr right):base(left,right) {
        }
    }

    public class AddExpr:ArthmExpr {
        public AddExpr(Expr left,Expr right):base(left,right) {}

        public override object evaluate() {
            return (double)(left.evaluate())+(double)(right.evaluate());
        }
    }

    public class SubExpr:ArthmExpr {
        public SubExpr(Expr left,Expr right):base(left,right) {}

        public override object evaluate() {
            return (double)(left.evaluate())-(double)(right.evaluate());
        }
    }

    public class MulExpr:ArthmExpr {
        public MulExpr(Expr left,Expr right):base(left,right) {}

        public override object evaluate() {
            return (double)(left.evaluate())*(double)(right.evaluate());
        }
    }

    public class DivExpr:ArthmExpr {
        public DivExpr(Expr left,Expr right):base(left,right) {}

        public override object evaluate() {
            return (double)(left.evaluate())/(double)(right.evaluate());
        }
    }

    public class ParserError:Exception{
        public ParserError():base() {}

        public ParserError(string message):base(message) {}
    }

    public class Parser{
        
        private Queue<Token> _tokens;
        private Expr _ast;

        public Queue<Token> Tokens{
            get {return _tokens;}
            set {_tokens=value;}
        }

        public Expr AST{
            get {return _ast;}
        }

        public Parser(){ }

        public Parser(Queue<Token> tokens){
            this._tokens=tokens;
        }

        public void Parse(){
            _ast=Exprs(_tokens);
        }

        private void log(IEnumerable<Token> tokens){
            foreach(var token in tokens){
                Console.WriteLine(token);
            }
            Console.WriteLine("*******");
        }

        private Expr Exprs(IEnumerable<Token> tokens){
            int index=(int)tokens.LongCount();
            if (index==0){
                throw new ParserError("empty tokens stream");
            }

            var FactorTokens=new LinkedList<Token>();
            foreach(var token in tokens.Take(index-1).Reverse<Token>()){
                index--;
                switch(token.type){
                    case TokensType.Add:
                        return new AddExpr(
                                Exprs(tokens.Take(index)),
                                Factor(FactorTokens.Reverse()));
                    case TokensType.Sub:
                        return new SubExpr(
                                Exprs(tokens.Take(index)),
                                Factor(FactorTokens.Reverse()));
                    case TokensType.Eof:
                        return Factor(FactorTokens.Reverse());
                    default: FactorTokens.AddLast(token);break;
                }
            }
                    return Factor(FactorTokens.Reverse());
        }

        private Expr Factor(IEnumerable<Token> tokens){
            int index=(int)tokens.LongCount();
            if (index==0){
                throw new ParserError("empty tokens stream");
            }

            var TermTokens=new LinkedList<Token>();
            foreach(var token in tokens.Reverse<Token>()){
                switch(token.type){
                    case TokensType.Mul:
                        return new MulExpr(
                                Exprs(tokens.Take(index)),
                                Term(TermTokens.Reverse()));
                    case TokensType.Div:
                        return new DivExpr(
                                Exprs(tokens.Take(index)),
                                Term(TermTokens.Reverse()));
                    case TokensType.Eof:return Term(TermTokens);
                    default:TermTokens.AddLast(token);break;
                }
                index--;
            }
            return Term(TermTokens.Reverse());
        }

        private Expr Term(IEnumerable<Token> tokens){
            if (tokens.LongCount()==0){
                throw new ParserError("empty tokens stream");
            }

            if (tokens.LongCount()>1){
                throw new ParserError("More than one Number token");
            }
            return new Number(tokens.ToArray<Token>()[0].Value);
        }

    }
}
