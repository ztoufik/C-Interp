using System.Collections.Generic;
using PL.Tokenize;
using PL.AST;
using PL.Parse;
using PL.Error;

namespace PL {

    public class Interpreter{
        readonly private Tokenizer _tokenzier;
        readonly private Parser _parser;
        private LinkedList<Token> _tokens;
        private Compound_Statement _ast;
        private string _input;
        private IDictionary<string,ObjNode> _scope;

        public string Input{
            get {return _input;}
            set {_input =value;}
        }

        public LinkedList<Token> Tokens{
            get {return _tokens;}
            //set {_tokens =value;}
        }

        public Compound_Statement Ast{
            get {return _ast;}
            //set {_ast =value;}
        }

        public IDictionary<string,ObjNode> Scope{
            get {return _scope;}
        }
        public Interpreter(){
            this._tokenzier=new Tokenizer();
            this._parser=new Parser();
            this._scope=new Dictionary<string,ObjNode>();
        }

        public void Tokenize(){
            this._tokens=this._tokenzier.Tokenize(_input);
        }

        public void Parse(){
            this._ast=this._parser.Parse(this._tokens);
        }

        public void Execute(){
            Tokenize();
            Parse();
            Visit_Compound_Statment(this._ast);
        }

        private void Visit_Compound_Statment(Compound_Statement compound_statment){
            foreach(var stmt in compound_statment.statement_list){
                Visit_Statment(stmt);
            }
        }

        private void Visit_Statment(Statement stmt){
            switch(stmt){
                case Expr expr:Visit_Expr(expr);break;
                case Assign assign:VisitAssign(assign);break;
                case Compound_Statement compound_statement:Visit_Compound_Statment(compound_statement);break;
            }
        }

        private void VisitAssign(Assign assign){
            this._scope[assign.Id.VarName]=Visit_Expr(assign.expr);
        }

        private ObjNode Visit_Expr(Expr expr){
            switch(expr){
                case Id id:
                    return Visit_Id(id);

                case ObjNode objnode:
                    return objnode;

                case BinOp binop:
                    return Visit_BinOp(binop);

                case UnOp unop:
                    return Visit_UnOp(unop);

                default: throw new ExecuteError("indefined AST Node");
            }
        }

        private ObjNode Visit_BinOp(BinOp binop){
            var left=Visit_Expr(binop.Left);
            var right=Visit_Expr(binop.Right);
            switch(binop){
                case AddExpr addexpr:
                    if((left is Number)&&(right is Number)){
                            return (Number)left+(Number)right;
                        }

                    if((left is Str)&&(right is Str)){
                            return (Str)left+(Str)right;
                        }
                    throw new ExecuteError("type mismatch");

                case SubExpr subexpr:
                    if((left is Number)&&(right is Number)){
                            return (Number)left-(Number)right;
                        }
                    throw new ExecuteError("only arthmtic types can be substracted");
                     
                case MulExpr mulexpr:
                    if((left is Number)&&(right is Number)){
                            return (Number)left*(Number)right;
                        }
                    throw new ExecuteError("only arthmtic types can be Multiplied");

                case DivExpr divexpr:
                    if((left is Number)&&(right is Number)){
                            return (Number)left/(Number)right;
                        }
                    throw new ExecuteError("only arthmtic types can be Divided");

                default: throw new ExecuteError("invalid op");
                }
        }

        private ObjNode Visit_UnOp(UnOp unop){
            var op=Visit_Expr(unop.Op);
            switch(unop){
                case PlusExpr plusexpr:
                    if(op is Number){
                        return (Number)op;
                    }
                    throw new ExecuteError("expected Number operand");

                case MinusExpr minusexpr:
                    if(op is Number){
                        return new Number("-1")*(Number)op;
                    }
                    throw new ExecuteError("expected Number operand");
                default: throw new ExecuteError("invalid op");
            }
        }
        private ObjNode Visit_Id(Id id){
            if(!this._scope.Keys.Contains(id.VarName)){
                throw new ExecuteError("indefined identifier");
            }
            return this._scope[id.VarName];
        }
   }
}
