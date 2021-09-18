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

                case ArthmExpr arthmExpr:
                    return Visit_ArthmExpr(arthmExpr);

                case UnArthmExpr unarthmexpr:
                    return Visit_UnArthmExpr(unarthmexpr);

                default: throw new ExecuteError("indefined AST Node");
            }
        }

        private Number Visit_ArthmExpr(ArthmExpr expr){
            Number left=ShouldReturnNumber(expr.Left);
            Number right=ShouldReturnNumber(expr.Right);
            switch(expr){
                case AddExpr addexpr:
                    return left+right;

                case SubExpr subexpr:
                    return left-right;

                case MulExpr mulexpr:
                    return left*right;

                case DivExpr divexpr:
                    return left/right;

                default: throw new ExecuteError("indefined AST Node");
            }
        }

        private Number Visit_UnArthmExpr(UnArthmExpr expr){
            var op=ShouldReturnNumber(expr.Op);
            switch(expr){
                case PlusExpr plusexpr:
                    return op;

                case MinusExpr minusexpr:
                    return new Number("-1")*op;

                default: throw new ExecuteError("indefined AST Node");
            }
        }

        private ObjNode Visit_Id(Id id){
            if(!this._scope.Keys.Contains(id.VarName)){
                throw new ExecuteError("indefined identifier");
            }
            return this._scope[id.VarName];
        }

        private Number ShouldReturnNumber(Expr expr){
            if(expr is Id){
                ObjNode variable=this.Visit_Id((Id)expr);
                if(!(variable is Number)){
                    throw new ExecuteError("expected number variable");
                }
                return (Number)variable;
            }
            return (Number)Visit_Expr(expr);
        }
    }

    static class Utils{
        public static void CheckArthmType(Expr expr){
            switch(expr){
                case ArthmExpr arthmExpr:break;
                case UnArthmExpr unarthmexpr:break;
                case Number number:break;
                case Id id:break;
                default: throw new ParserError("operand must be arthmetic type");
            }
        }

        public static void CheckStrType(Expr expr){
            switch(expr){
                case StrConct arthmExpr:break;
                case Str str:break;
                case Id id:break;
                default: throw new ParserError("operand must be string type");
            }
        }

    }
}
