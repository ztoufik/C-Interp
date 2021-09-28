using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using PL.Tokenize;
using PL.AST;
using PL.Error;

namespace PL.Parse {
    static public class Parser{

        static public Program Parse(TokenStream tokenstream){
            return ParseProgram(tokenstream);
        }

        static private Program ParseProgram(TokenStream tokenstream){
            var statementlist=new LinkedList<Statement>();
            while(tokenstream.Current.Value.type!=TokensType.Eof){
                statementlist.AddLast(ParseStatement(tokenstream));
                if(!tokenstream.Eat(TokensType.Semi)){
                    throw new ParserError("missing ';'");
                }
            }
            tokenstream.Advance();
            return new Program(statementlist);
        }

        static private Statement ParseStatement(TokenStream tokenstream){
            var token=tokenstream.Current.Value;
            switch(token.type){
                case TokensType.Begin: 
                    return ParseCompoundStatement(tokenstream);
                case TokensType.Get:
                    return ParseGet(tokenstream);
                case TokensType.If:
                    return ParseIf_Clause(tokenstream);
                case TokensType.Loop:
                    return ParseLoop(tokenstream);
            }

            token=tokenstream.Next.Value;
            if(token.type==TokensType.Assign){
                return ParseAssignment(tokenstream);
            }

            if(token.type==TokensType.RefAssign){
                return ParseRefAssignement(tokenstream);
            }
            return ParseExpr(tokenstream);
        }

        static private Compound_Statement ParseCompoundStatement(TokenStream tokenstream){
            if(!tokenstream.Eat(TokensType.Begin)){
                throw new ParserError("missing {");
            }

            var statement_list=new LinkedList<Statement>();

            while(tokenstream.Current.Value.type!=TokensType.End){
                statement_list.AddLast(ParseStatement(tokenstream));
                if(!tokenstream.Eat(TokensType.Semi)){
                    throw new ParserError("missing ;");
                }
                if(tokenstream.Count<=0){
                    throw new ParserError("missing }");
                }
            }
            tokenstream.Advance();

            return new Compound_Statement(statement_list);
        }

        static private IfClause ParseIf_Clause(TokenStream tokenstream){
            if(!tokenstream.Eat(TokensType.If)){
                throw new ParserError("missing If statement");
            }

            if(!tokenstream.Eat(TokensType.LP)){
                throw new ParserError("missing (");
            }

            var condition=ParseExpr(tokenstream);
            if(!tokenstream.Eat(TokensType.RP)){
                throw new ParserError("missing )");
            }
            Compound_Statement truestmt=ParseCompoundStatement(tokenstream),elsestmt=null;
            if(tokenstream.Current.Value.type==TokensType.Else){
                tokenstream.Advance();
                elsestmt=ParseCompoundStatement(tokenstream);
            }
            return new IfClause(condition,truestmt,elsestmt);
        }

        static private Loop ParseLoop(TokenStream tokenstream){
            if(!tokenstream.Eat(TokensType.Loop)){
                throw new ParserError("missing Loop statement");
            }

            if(!tokenstream.Eat(TokensType.LP)){
                throw new ParserError("missing (");
            }

            var condition=ParseExpr(tokenstream);
            if(!tokenstream.Eat(TokensType.RP)){
                throw new ParserError("missing )");
            }
            Compound_Statement body=ParseCompoundStatement(tokenstream);
            return new Loop(condition,body);
        }

        static private Get ParseGet(TokenStream tokenstream){
            if(!tokenstream.Eat(TokensType.Get)){
                throw new ParserError("missing Get statement");
            }
            var filename=tokenstream.Current.Value;

            if(!tokenstream.Eat(TokensType.str)){
                throw new ParserError("invalid script file name ");
            }
            var path=Path.Join(".",filename.Value);
            if(!File.Exists(Path.Join(".",path))){
                throw new ParserError($"scritp file {path} is not in current path");
            }

            return new Get(filename.Value,Parser.ParseProgram(Tokenizer.TokenizeFile(path)));
        }

        static private Assign ParseAssignment(TokenStream tokenstream){
            var id=new Id(tokenstream.Current.Value.Value);
            tokenstream.Advance();
            tokenstream.Advance();
            if(tokenstream.Eat(TokensType.Eof)){
                throw new ParserError("expected expression");
            }
            return new Assign(id,ParseExpr(tokenstream));
        }

        static private RefAssign ParseRefAssignement(TokenStream tokenstream){
            var id=new Id(tokenstream.Current.Value.Value);
            tokenstream.Advance();
            tokenstream.Advance();
            if(tokenstream.Eat(TokensType.Eof)){
                throw new ParserError("expected expression");
            }
            return new RefAssign(id,ParseExpr(tokenstream));
        }

        static private Expr ParseExpr(TokenStream tokenstream){
            Expr leftnode=ParseAdd(tokenstream),rightnode=null;

            var token=tokenstream.Current.Value;

            if(new TokensType[]{TokensType.Eq,TokensType.NEq,TokensType.GT,TokensType.GE,TokensType.LE,TokensType.LT}.Contains(token.type)){
                tokenstream.Advance();
                rightnode=ParseAdd(tokenstream);
                return new CmpOp(token.type,leftnode,rightnode);
            }
            return leftnode;
        }

        static private Expr ParseAdd(TokenStream tokenstream){
            Expr leftnode=ParseMul(tokenstream),rightnode=null;
            var token=tokenstream.Current.Value;

            while((token.type==TokensType.Add) || (token.type==TokensType.Sub)){
                tokenstream.Advance();
                rightnode=ParseMul(tokenstream);
                if(token.type==TokensType.Add){
                    leftnode= new AddExpr(leftnode,rightnode);
                }
                else {
                    leftnode= new SubExpr(leftnode,rightnode);
                }
                token=tokenstream.Current.Value;
            }

            return leftnode;
        }

        static private Expr ParseMul(TokenStream tokenstream){
            Expr leftnode=ParseUnOpr(tokenstream),rightnode=null;
            var token=tokenstream.Current.Value;

            while((token.type==TokensType.Mul) || (token.type==TokensType.Div)){
                tokenstream.Advance();
                rightnode=ParseUnOpr(tokenstream);
                if(token.type==TokensType.Mul){
                    leftnode= new MulExpr(leftnode,rightnode);
                }
                else {
                    leftnode= new DivExpr(leftnode,rightnode);
                }
                token=tokenstream.Current.Value;
            }

            return leftnode;
        }

        static private Expr ParseUnOpr(TokenStream tokenstream){
            if(tokenstream.Current.Value.type==TokensType.Add){
                tokenstream.Advance();
                return new PlusExpr(ParseParenth(tokenstream));
            }
            if(tokenstream.Current.Value.type==TokensType.Sub){
                tokenstream.Advance();
                return new MinusExpr(ParseParenth(tokenstream));
            }
            return ParseParenth(tokenstream);
        }

        static private Expr ParseParenth(TokenStream tokenstream){
            if(tokenstream.Current.Value.type==TokensType.LP){
                tokenstream.Advance();
                var Term=ParseExpr(tokenstream);
                if(!tokenstream.Eat(TokensType.RP)){
                    throw new ParserError("missing )");
                }
                return Term;
            }
            return ParseTerm(tokenstream);
        }

        static private Expr ParseTerm(TokenStream tokenstream){
            if (tokenstream.Count==0){
                throw new ParserError("empty tokens stream");
            }
            var token=tokenstream.Current.Value;
            tokenstream.Advance();

            switch(token.type){
                case TokensType.Number: return new Number(double.Parse(token.Value));
                case TokensType.True:return new BLN(true);
                case TokensType.False:return new BLN(false);
                case TokensType.str:{ 
                            var id =new Id(token.Value);
                            //if(tokenstream.Current.Value.type==TokensType.LP){
                            //    tokenstream.Advance();
                            //    var args=ParseCallExpr(tokenstream);
                            //    tokenstream.Eat(TokensType.RP);
                            //    return new Call(id,args);
                            //}
                            return id; }
                case TokensType.DQ:return ParseString(tokenstream);
                //case TokensType.Fn:return ParseFunction(tokenstream);
                default:throw new ParserError("invalid type");
            }
        }

        static private Function ParseFunction(TokenStream tokenstream){
            if(tokenstream.Eat(TokensType.LP)){
                throw new ParserError("missing (");
            }
            var ids=new LinkedList<Id>();
            do{
                var token=tokenstream.Current.Value;
                if(tokenstream.Eat(TokensType.str)){
                    throw new ParserError("Identifier expected");
                }
                ids.AddLast(new Id(token.Value));
                if(tokenstream.Count<=0){
                    throw new ParserError("missing )");
                }

            }while(tokenstream.Current.Value.type==TokensType.Colon);

            if(tokenstream.Eat(TokensType.RP)){
                throw new ParserError("missing )");
            }

            return new Function(ids,ParseCompoundStatement(tokenstream));
        }

        static private LinkedList<Expr> ParseCallExpr(TokenStream tokenstream){
            LinkedList<Expr> exprslist=new LinkedList<Expr>();
            if(tokenstream.Current.Value.type!=TokensType.RP){
                exprslist.AddLast(ParseExpr(tokenstream));

                if(tokenstream.Current.Value.type==TokensType.Colon){
                    tokenstream.Advance();
                    foreach(var expr in ParseCallExpr(tokenstream)){
                        exprslist.AddLast(expr);
                    }
                }

            }
            tokenstream.Advance();

            return exprslist;
        }

        static private Str ParseString(TokenStream tokenstream){
            var str=new StringBuilder();
            while(tokenstream.Current.Value.type!=TokensType.DQ){
                str.Append(tokenstream.Current.Value.Value);
                tokenstream.Advance();
                if(tokenstream.Count<=0){
                    throw new ParserError(" missing \"");
                }
            }
            tokenstream.Advance();
            return new Str(str.ToString());
        }
    }
}