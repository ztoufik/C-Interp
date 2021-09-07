using Interpreter.Error;

namespace Interpreter.AST {
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
            double rightOperand=(double)(right.evaluate());
            if(rightOperand==0){
                throw new DivideByZeroError();
            }
            return (double)(left.evaluate())/rightOperand;
        }
    }

    abstract public class UnExpr:Expr {
        protected readonly Expr _Op;

        public UnExpr(Expr Op) {
            this._Op=Op;
        }
    }

    public class PlusExpr:UnExpr {
        public PlusExpr(Expr Op):base(Op) {}

        public override object evaluate() {
           return (double)(_Op.evaluate()); 
        }
    }

    public class MinusExpr:UnExpr {
        public MinusExpr(Expr Op):base(Op) {}

        public override object evaluate() {
           return (-1)*(double)(_Op.evaluate()); 
        }
    }
}
