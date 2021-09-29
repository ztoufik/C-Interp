using System.Collections.Generic;
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
                case TokensType.IF:
                    return ParseIf_Clause(tokenstream);
                case TokensType.Loop:
                    return ParseLoop(tokenstream);
                case TokensType.Ret: {tokenstream.Advance(); return new Return(ParseExpr(tokenstream));}
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
            if(!tokenstream.Eat(TokensType.IF)){
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

            if(!tokenstream.Eat(TokensType.Str)){
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

            if(new TokensType[]{TokensType.EQ,TokensType.Neq,TokensType.GT,TokensType.GE,TokensType.LE,TokensType.LT}.Contains(token.type)){
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
                var Term=ParseAdd(tokenstream);
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
            Expr expr=null;
            tokenstream.Advance();

            switch(token.type){
                case TokensType.Number: expr= new Number(double.Parse(token.Value));break;
                case TokensType.True:expr= new BLN(true);break;
                case TokensType.False:expr= new BLN(false);break;
                case TokensType.Str:expr= new Id(token.Value);break;
                case TokensType.Qstr:expr= new Str(token.Value);break;
                case TokensType.FN:expr= ParseFunction(tokenstream);break;
                default:throw new ParserError("invalid type");
            }
            // check for consecutive call
            while(tokenstream.Current.Value.type==TokensType.LP){
                expr=new Call(expr,ParseArgsList(tokenstream));
            }

            return expr;
        }

        static private Function ParseFunction(TokenStream tokenstream){
            if(!tokenstream.Eat(TokensType.LP)){
                throw new ParserError("missing (");
            }
            var args=new LinkedList<Id>();
            while(tokenstream.Current.Value.type!=TokensType.RP){
                if(tokenstream.Current.Value.type!=TokensType.Str){
                    throw new ParserError("Identifier expected");
                }
                args.AddLast(new Id(tokenstream.Current.Value.Value));
                tokenstream.Advance();
                if(tokenstream.Count<=0){
                    throw new ParserError("missing )");
                }
                if(tokenstream.Current.Value.type==TokensType.CM){
                    if(tokenstream.Next.Value.type==TokensType.RP){
                        throw new ParserError("Identifier expected");
                    }
                    tokenstream.Advance();
                }
            }
            if(!tokenstream.Eat(TokensType.RP)){
                throw new ParserError("Identifier expected");
            }

            return new Function(args,ParseCompoundStatement(tokenstream));
        }

        static private LinkedList<Expr> ParseCallExpr(TokenStream tokenstream){
            LinkedList<Expr> exprslist=new LinkedList<Expr>();
            if(tokenstream.Current.Value.type!=TokensType.RP){
                exprslist.AddLast(ParseExpr(tokenstream));

                if(tokenstream.Current.Value.type==TokensType.CM){
                    tokenstream.Advance();
                    foreach(var expr in ParseCallExpr(tokenstream)){
                        exprslist.AddLast(expr);
                    }
                }

            }
            tokenstream.Advance();

            return exprslist;
        }

        static private LinkedList<Expr> ParseArgsList(TokenStream tokenstream){
            if(!tokenstream.Eat(TokensType.LP)){
                throw new ParserError("missing (");
            }
            var argslist=new LinkedList<Expr>();
            while(tokenstream.Current.Value.type!=TokensType.RP){
                argslist.AddLast(ParseExpr(tokenstream));
                if(tokenstream.Count<=0){
                    throw new ParserError("missing )");
                }
                if(tokenstream.Current.Value.type==TokensType.CM){
                    if(tokenstream.Next.Value.type==TokensType.RP){
                        throw new ParserError("expression expected");
                    }
                    tokenstream.Advance();
                }
            }
            if(!tokenstream.Eat(TokensType.RP)){
                throw new ParserError("missing ')'");
            }
            return argslist;
        }
    }
}
