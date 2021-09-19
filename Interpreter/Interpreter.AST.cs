using System.Collections.Generic;
using PL.Error;

namespace PL.AST {

    abstract public class Statement {
        public Statement(){ }
    }

    abstract public class Expr:Statement {
        public Expr():base(){}
    }

    public abstract class ObjNode:Expr {
        protected readonly object Value;

        public ObjNode(object Value):base() {
            this.Value=Value;
        }

        public override string ToString(){
            return this.Value.ToString();
        }
    }

    // types

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

    public class BLN:ObjNode {
        public BLN(bool Value):base(Value) {
        }

        public override string ToString(){
            if((bool)(Value))
                return "True";
            return "False";
        }
    }

    abstract public class BinOp:Expr {
        protected readonly Expr _left;
        protected readonly Expr _right;

        public Expr Left{ get {return _left;}}
        public Expr Right{ get {return _right;}}

        public BinOp(Expr left,Expr right):base() {
            this._left=left;
            this._right=right;
        }
    }

    // unary operations

    abstract public class UnOp:Expr {
        protected readonly Expr _Op;
        public Expr Op{get{return _Op;}}

        public UnOp(Expr Op):base() {
            this._Op=Op;
        }
    }

    // arthemtic operations

    public class AddExpr:BinOp {
        public AddExpr(Expr left,Expr right):base(left,right) {}
    }

    public class SubExpr:BinOp {
        public SubExpr(Expr left,Expr right):base(left,right) {}
    }

    public class MulExpr:BinOp {
        public MulExpr(Expr left,Expr right):base(left,right) {}
    }

    public class DivExpr:BinOp {
        public DivExpr(Expr left,Expr right):base(left,right) {}
    }

    public class PlusExpr:UnOp {
        public PlusExpr(Expr Op):base(Op) {}
    }

    public class MinusExpr:UnOp {
        public MinusExpr(Expr Op):base(Op) {}
    }

    // statements

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

        public Compound_Statement(LinkedList<Statement> statement_list):base(){
            this._statements_list=statement_list;
        }
    }

    public class Import:Statement{
        private readonly string _scriptfile;

        public string ScriptFile{get{return this._scriptfile;} }

        public Import(string scriptfile):base(){
            this._scriptfile=scriptfile;
        }
    }

    public class If_Clause:Statement{
        private readonly Expr _condition;
        private readonly Compound_Statement _truestmt;
        private readonly Compound_Statement _falsestmt;

        public Expr Condition{get{return _condition;}}
        public Compound_Statement TrueStmt{get{return _truestmt;}}
        public Compound_Statement FalseStmt{get{return _falsestmt;}}

        public If_Clause(Expr condition,Compound_Statement truestmt,Compound_Statement falsestmt):base(){
            this._condition=condition;
            this._truestmt=truestmt;
            this._falsestmt=falsestmt;
        }
    }
}
