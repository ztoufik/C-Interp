using System.Collections.Generic;
using PL.Error;

namespace PL.AST {

    abstract public class Statement {
        public Statement(){ }
    }

    abstract public class Expr:Statement {
    }

    public abstract class ObjNode:Expr {
        protected readonly object Value;

        public ObjNode(object Value) {
            this.Value=Value;
        }

        public override string ToString(){
            return this.Value.ToString();
        }
    }

    public class Number:ObjNode {
        public Number(string Value):base(double.Parse(Value)) {
        }

        static public Number operator +(Number own,Number other){
            var sum=(double)own.Value+(double)(other.Value);
            return new Number(sum.ToString());
        }

        static public Number operator -(Number own,Number other){
            var sum=(double)own.Value+ (-(double)(other.Value));
            return new Number(sum.ToString());
        }

        static public Number operator *(Number own,Number other){
            var sum=(double)own.Value*(double)(other.Value);
            return new Number(sum.ToString());
        }

        static public Number operator /(Number own,Number other){
            if((double)other.Value==0){
                throw new DivideByZeroError();
            }

            var sum=(double)own.Value/(double)(other.Value);
            return new Number(sum.ToString());
        }
    }

    public class Id:ObjNode {
        private string _varname;

        public string VarName{
            get { return _varname;}
        }

        public Id(string varname):base(varname) {
            this._varname=varname;
        }

    }

    public class Str:ObjNode {
        public Str(string Value):base(Value.ToString()) {
        }

        static public Str operator +(Str own,Str other){
            var concat=own.Value.ToString()+other.Value.ToString();
            return new Str(concat);
        }
    }

    abstract public class BinExpr:Expr {
        protected readonly Expr _left;
        protected readonly Expr _right;

        public Expr Left{ get {return _left;}}
        public Expr Right{ get {return _right;}}

        public BinExpr(Expr left,Expr right) {
            this._left=left;
            this._right=right;
        }
    }
    abstract public class StrConct:BinExpr {
            public StrConct(Expr left,Expr right):base(left,right) {
                Utils.CheckStrType(left);
                Utils.CheckStrType(right);
            }

        }

    abstract public class ArthmExpr:BinExpr {
        public ArthmExpr(Expr left,Expr right):base(left,right) {
            Utils.CheckArthmType(left);
            Utils.CheckArthmType(right);
        }

    }

    public class AddExpr:ArthmExpr {
        public AddExpr(Expr left,Expr right):base(left,right) {}
    }

    public class SubExpr:ArthmExpr {
        public SubExpr(Expr left,Expr right):base(left,right) {}
    }

    public class MulExpr:ArthmExpr {
        public MulExpr(Expr left,Expr right):base(left,right) {}
    }

    public class DivExpr:ArthmExpr {
        public DivExpr(Expr left,Expr right):base(left,right) {}
    }

    

    abstract public class UnExpr:Expr {
        protected readonly Expr _Op;
        public Expr Op{get{return _Op;}}

        public UnExpr(Expr Op) {
            this._Op=Op;
        }
    }

    abstract public class UnArthmExpr:UnExpr {
        public UnArthmExpr(Expr Op):base(Op) {
            Utils.CheckArthmType(Op);
        }
    }

    public class PlusExpr:UnArthmExpr {
        public PlusExpr(Expr Op):base(Op) {}
    }

    public class MinusExpr:UnArthmExpr {
        public MinusExpr(Expr Op):base(Op) {}
    }

    public class Assign:Statement {
        private Id _id;
        private Expr _expr;

        public Id Id {get {return _id;}}
        public Expr expr {get {return _expr;}}

        public Assign(Id id,Expr expr):base() {
            this._id=id;
            this._expr=expr;
        }
    }

    public class Compound_Statement:Statement{
        private readonly LinkedList<Statement> _statements_list;

        public LinkedList<Statement> statement_list {get {return this._statements_list;}}

        public Compound_Statement(LinkedList<Statement> statement_list){
            this._statements_list=statement_list;
        }
    }

}
