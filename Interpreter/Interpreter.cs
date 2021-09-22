using System.IO;
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
        private Scope _currentscope;

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

        public Scope scope{
            get {return this._currentscope;}
        }

        public Interpreter(){
            this._tokenzier=new Tokenizer();
            this._parser=new Parser();
            this._currentscope=new Scope(null);
        }

        public void Tokenize(){
            this._tokens=this._tokenzier.Tokenize(_input);
        }

        public void Parse(){
            this._ast=this._parser.Parse(this._tokens);
        }

        public void Execute(string statement){
            this.Input=statement;
            Tokenize();
            Parse();
            Visit_Compound_Statment(this._ast);
        }

        public void Load(string filepath){
            string cwd=Directory.GetCurrentDirectory();
            string cfilepath=string.Join($"{Path.DirectorySeparatorChar}",new string[2]{cwd,filepath});
            if(!File.Exists(cfilepath)){
                throw new ExecuteError($"{cfilepath} can't be found in the current directory");
            }
            int linenr=0;
            var tokenslist=new LinkedList<LinkedList<Token>>();
            this._tokens=new LinkedList<Token>();
            foreach(string line in File.ReadLines(cfilepath)){
                try{
                    tokenslist.AddLast(this._tokenzier.Tokenize(line));
                }
                catch(TokenError e){
                    string message=e.Message;
                    throw new TokenError($"{message} line:{linenr} file:{cfilepath}");
                }
                linenr++;
            }

            foreach(LinkedList<Token> tokens in tokenslist){
                foreach(Token token in tokens){
                    if(token.type==TokensType.Eof){
                        continue;
                    }
                    this._tokens.AddLast(token);
                }
            }
            this._tokens.AddLast(new Token(TokensType.Eof,null));
            try{
                this.Parse();
                Visit_Compound_Statment(this._ast);
            }
            catch(ParserError e){
                string message=e.Message;
                throw new ParserError($"{message} file:{cfilepath}");
            }
            catch(ExecuteError e){
                string message=e.Message;
                throw new ExecuteError($"{message} file:{cfilepath}");
            }
        }

        private void Visit_Compound_Statment(Compound_Statement compound_statment){
            foreach(var stmt in compound_statment.statement_list){
                Visit_Statment(stmt);
            }
        }

        private void Visit_Statment(Statement stmt){
            switch(stmt){
                case Expr expr:Visit_Expr(expr);break;
                case Assign assign:Visit_Assign(assign);break;
                case RefAssign refassign:Visit_RefAssign(refassign);break;
                case Compound_Statement compound_statement:Visit_Compound_Statment(compound_statement);break;
                case Import import:Visit_Import(import);break;
                case If_Clause if_clause:Visit_If_Clause(if_clause);break;
                case Loop loop:Visit_Loop(loop);break;
                default:throw new ExecuteError("indefined statement node type");
            }
        }

        private void Visit_Assign(Assign assign){
            this._currentscope[assign.Id.VarName]=Visit_Expr(assign.expr);
        }

        private void Visit_RefAssign(RefAssign refassign){
            ObjNode Value=Visit_Expr(refassign.expr);
            Scope scope=this._currentscope.Parent;
            while(!(scope is null)){
                if(!(scope[refassign.Id.VarName] is null)){
                    scope[refassign.Id.VarName]=Value;
                    return;
                }
                scope=scope.Parent;
            }
            throw new ExecuteError("referenced variable is not identified");
        }

        private void Visit_If_Clause(If_Clause if_clause){
            Scope localscope=new Scope(this._currentscope);
            this._currentscope=localscope;
            ObjNode conditioneexpr=Visit_Expr(if_clause.Condition);
            if(!(conditioneexpr is BLN)){ throw new ExecuteError("expected BLN value");}
            bool condition=(bool)conditioneexpr.Value;
            if(condition)
            {Visit_Compound_Statment(if_clause.TrueStmt);}
            else
            {Visit_Compound_Statment(if_clause.FalseStmt);}
            this._currentscope=this._currentscope.Parent; 
        }

        private void Visit_Import(Import import){
            this.Load(import.ScriptFile);
        }

        private void Visit_Loop(Loop loop){
            Scope localscope=new Scope(this._currentscope);
            this._currentscope=localscope;
            ObjNode conditioneexpr=Visit_Expr(loop.Condition);
            if(!(conditioneexpr is BLN)){ throw new ExecuteError("expected BLN value");}
            bool condition=(bool)conditioneexpr.Value;
            while(condition){
                Visit_Compound_Statment(loop.Body);
                conditioneexpr=Visit_Expr(loop.Condition);
                if(!(conditioneexpr is BLN)){ throw new ExecuteError("expected BLN value");}
                condition=(bool)conditioneexpr.Value;
            }
            this._currentscope=this._currentscope.Parent; 
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

                case CmpOp cmpop:
                    if((left is Number)&&(right is Number)){
                        Number Nleft=(Number)left;
                        Number Nright=(Number)right;
                        switch(cmpop.Op){
                            case TokensType.Eq:return new BLN(Nleft==Nright);
                            case TokensType.NEq:return new BLN(Nleft!=Nright);
                            case TokensType.GT:return new BLN(Nleft > Nright);
                            case TokensType.GE:return new BLN(Nleft>=Nright);
                            case TokensType.LT:return new BLN(Nleft<Nright);
                            case TokensType.LE:return new BLN(Nleft<=Nright);
                            }
                        }
                    throw new ExecuteError("only arthmtic types can be compared");

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
                        return new Number(-1)*(Number)op;
                    }
                    throw new ExecuteError("expected Number operand");
                default: throw new ExecuteError("invalid op");
            }
        }

        private ObjNode Visit_Id(Id id){
            ObjNode Value=this.LookUp(id.VarName,this._currentscope);
            if(Value is null){
                throw new ExecuteError("indefined identifier");
            }
            return Value;
        }

        private ObjNode LookUp(string key,Scope scope){
            if(scope is null){
                return null;
            }
            return scope[key]??LookUp(key,scope.Parent);
        }
   }
}
