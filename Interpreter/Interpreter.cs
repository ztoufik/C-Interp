using System.Linq;
using System.Collections.Generic;
using PL.Tokenize;
using PL.AST;
using PL.Parse;
using PL.Error;

namespace PL {
    public class Interpreter{
        private Scope _currentscope;

        public Scope scope{
            get {return this._currentscope;}
        }

        public Interpreter(){
            this._currentscope=new Scope(null);
        }

        public void ExecuteStatement(string statement){
            VisitProgram(Parser.Parse(Tokenizer.TokenizeString(statement)));
        }

        public void ExecuteScriptFile(string filepath){
            VisitProgram(Parser.Parse(Tokenizer.TokenizeFile(filepath)));
        }

        private void VisitProgram(Program Ast){
            foreach(var stmt in Ast.StmtList){
                VisitStatment(stmt);
            }
        }

        private void VisitCompoundStatment(Compound_Statement compound_statment){
            foreach(var stmt in compound_statment.statement_list){
                VisitStatment(stmt);
            }
        }

        private void VisitStatment(Statement stmt){
            switch(stmt){
                case Expr expr:VisitExpr(expr);break;
                case Assign assign:VisitAssign(assign);break;
                case RefAssign refassign:VisitRefAssign(refassign);break;
                case Compound_Statement compoundstatement:VisitCompoundStatment(compoundstatement);break;
                case Get Get:VisitGet(Get);break;
                case IfClause ifclause:VisitIfClause(ifclause);break;
                case Loop loop:VisitLoop(loop);break;
                case Return ret:VisitReturn(ret);break;
                default:throw new ExecuteError("indefined statement node type");
            }
        }

        private void VisitAssign(Assign assign){
            switch(assign.Id){
                case Id id: this._currentscope[id.VarName]=VisitExpr(assign.expr);break;
                case TIndex tindex: {
                                        Table table=VisitExpr(tindex.Indexer) as Table;
                                        if(table is null){
                                            throw new ExecuteError("can't index Null objec");
                                        }
                                        ObjNode index=VisitExpr(tindex.Index);
                                        if(index is Null){
                                            throw new ExecuteError("can't index Null objec");
                                        }
                                        table[index]=VisitExpr(assign.expr);

                                    }break;
                default:throw new ExecuteError("invalid expression to assign to ");
            }
        }

        private void VisitRefAssign(RefAssign refassign){
            ObjNode Value=VisitExpr(refassign.expr);
            switch(refassign.Id){
                case Id id: {
                                Scope scope=this._currentscope.Parent;
                                while(!(scope is null)){
                                    if(!(scope[id.VarName] is null)){
                                        scope[id.VarName]=Value;
                                        return;
                                    }
                                    scope=scope.Parent;
                                }
                                throw new ExecuteError("referenced variable is not identified");
                            }
                case TIndex tindex: {
                                        Table table=VisitExpr(tindex.Indexer) as Table;
                                        if(table is null){
                                            throw new ExecuteError("can't index Null");
                                        }
                                        ObjNode index=VisitExpr(tindex.Index);
                                        if(index is Null){
                                            throw new ExecuteError("index value can't be Null");
                                        }
                                        table[index]=VisitExpr(refassign.expr);

                                    }break;
                default:throw new ExecuteError("invalid expression to assign to ");
            }
        }

        private void VisitIfClause(IfClause if_clause){
            Scope localscope=new Scope(this._currentscope);
            this._currentscope=localscope;
            ObjNode conditioneexpr=VisitExpr(if_clause.Condition);
            if(!(conditioneexpr is BLN)){ throw new ExecuteError("expected BLN value");}
            bool condition=(bool)conditioneexpr.Value;
            if(condition)
            {VisitCompoundStatment(if_clause.TrueStmt);}
            else
            {VisitCompoundStatment(if_clause.FalseStmt);}
            this._currentscope=this._currentscope.Parent; 
        }

        private void VisitGet(Get get){
            VisitProgram(get.program);
        }

        private void VisitReturn(Return ret){
            var funcscope=this._currentscope as FuncScope;
            if(funcscope is null){
                throw new ExecuteError("no function stack");
            }
            funcscope.Return=VisitExpr(ret.expr);
        }

        private void VisitLoop(Loop loop){
            Scope localscope=new Scope(this._currentscope);
            this._currentscope=localscope;
            ObjNode conditioneexpr=VisitExpr(loop.Condition);
            if(!(conditioneexpr is BLN)){ throw new ExecuteError("expected BLN value");}
            bool condition=(bool)conditioneexpr.Value;
            while(condition){
                VisitCompoundStatment(loop.Body);
                conditioneexpr=VisitExpr(loop.Condition);
                if(!(conditioneexpr is BLN)){ throw new ExecuteError("expected BLN value");}
                condition=(bool)conditioneexpr.Value;
            }
            this._currentscope=this._currentscope.Parent; 
        }

        private ObjNode VisitExpr(Expr expr){
            switch(expr){
                case Id id:
                    return VisitId(id);

                case TableExpr tableexpr:
                    return VisitTableExpr(tableexpr);

                case TIndex tindex:
                    return VisitIndex(tindex);

                case BinOp binop:
                    return VisitBinOp(binop);

                case Call call:
                    return VisitCall(call);

                case UnOp unop:
                    return VisitUnOp(unop);

                case ObjNode objnode:
                    return objnode;

                default: throw new ExecuteError("indefined AST Node");
            }
        }

        private ObjNode VisitBinOp(BinOp binop){
            var left=VisitExpr(binop.Left);
            var right=VisitExpr(binop.Right);
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
                            case TokensType.EQ:return new BLN(Nleft==Nright);
                            case TokensType.Neq:return new BLN(Nleft!=Nright);
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

        private ObjNode VisitUnOp(UnOp unop){
            var op=VisitExpr(unop.Op);
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

        private ObjNode VisitId(Id id){
            ObjNode Value=this.LookUp(id.VarName,this._currentscope);
            if(Value is null){
                throw new ExecuteError("indefined identifier");
            }
            return Value;
        }

        private ObjNode VisitIndex(TIndex index){
            var table=VisitExpr(index.Indexer) as Table;
            if(table is null){
                throw new ExecuteError("invalid table expression");
            }
            ObjNode indexvalue=VisitExpr(index.Index);
            if(indexvalue is Null){
                throw new ExecuteError("index value can't be null");
            }
            var objnode=table[indexvalue];
            return  objnode is null?new Null():VisitExpr(objnode);
        }

        private Table VisitTableExpr(TableExpr tableexpr){
            var dict=new Dictionary<ObjNode,ObjNode>();
            foreach(var exprpair in tableexpr.ExprsList){
                dict.Add(VisitExpr(exprpair.Key),VisitExpr(exprpair.Value));
            }
            return new Table(dict);
        }

        private ObjNode LookUp(string key,Scope scope){
            if(scope is null){
                return null;
            }
            return scope[key]??LookUp(key,scope.Parent);
        }

        private ObjNode VisitCall(Call call){
            FuncScope localscope=new FuncScope(this._currentscope);
            this._currentscope=localscope;
            var func=VisitExpr(call.Caller);
            if(func.GetType()!=typeof(Function)){
                throw new ExecuteError("function expression expected");
            }
            var zippedlist=((Function)func).Args.Zip<Id,Expr,KeyValuePair<string,ObjNode>>(
                    call.ArgsExprs,(id,expr)=>new KeyValuePair<string,ObjNode>(id.VarName,VisitExpr(expr)));
            foreach(var arg in zippedlist){
                this._currentscope[arg.Key]=arg.Value;
            }
            var cmpstmt=((Function)func).Body;
            VisitCompoundStatment(cmpstmt);
            var ret=localscope.Return;
            this._currentscope=this._currentscope.Parent; 
            return ret;
        }
    }
}
