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
        private Statement_List _ast;
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

        public Statement_List Ast{
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
            Visit_Statments_List(this._ast);
        }

        private void Visit_Statments_List(Statement_List stmts_list){
            foreach(var stmt in stmts_list.statements){
                Visit_Statment(stmt);
            }

        }
        private void Visit_Statment(Statement stmt){
            switch(stmt){
                case Expr expr:Visit_Expr(expr);break;
                case Assign assign:VisitAssign(assign);break;
            }
        }

        private void VisitAssign(Assign assign){
            this._scope[assign.Id.VarName]=Visit_Expr(assign.expr);
        }

        private ObjNode Visit_Expr(Expr expr){
            switch(expr){
                case Id id:
                    if(!this._scope.Keys.Contains(id.VarName)){
                        throw new ExecuteError("indefined identifier");
                    }
                        return this._scope[id.VarName];

                case ObjNode objnode:
                    return objnode;

                case AddExpr addexpr:
                    return (Number)(Visit_Expr(addexpr.Left))+(Number)(Visit_Expr(addexpr.Right));

                case SubExpr subexpr:
                    return (Number)(Visit_Expr(subexpr.Left))-(Number)(Visit_Expr(subexpr.Right));

                case MulExpr mulexpr:
                    return (Number)(Visit_Expr(mulexpr.Left))*(Number)(Visit_Expr(mulexpr.Right));

                case DivExpr divexpr:
                    return (Number)(Visit_Expr(divexpr.Left))/(Number)(Visit_Expr(divexpr.Right));

                case PlusExpr plusexpr:
                    return Visit_Expr(plusexpr.Op);

                case MinusExpr minusexpr:
                    return new Number("-1")*(Number)Visit_Expr(minusexpr.Op);


                default: throw new ExecuteError("indefined AST Node");
            }
        }
    }
}
