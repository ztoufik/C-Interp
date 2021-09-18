using System.Collections.Generic;
using System.Text;
using PL.Tokenize;
using PL.AST;
using PL.Error;

namespace PL.Parse {

    public class Parser{
        
        public Parser(){ }

        public Compound_Statement Parse(LinkedList<Token> tokens){
            return ParseCompoundStatement(tokens);
        }

        private Compound_Statement ParseCompoundStatement(LinkedList<Token> tokens){
            var firsttoken=tokens.First.Value;
            if(firsttoken.type!=TokensType.Begin){
                throw new ParserError("missing {");
            }
            tokens.RemoveFirst();

            var statement_list=new LinkedList<Statement>();
            firsttoken=tokens.First.Value;

            while(firsttoken.type!=TokensType.End){
                statement_list.AddLast(ParseStatement(tokens));
                if(tokens.First.Value.type!=TokensType.Semi){
                    throw new ParserError("missing ;");
                }
                tokens.RemoveFirst();

                if(tokens.Count<=0){
                    throw new ParserError("missing }");
                }

                firsttoken=tokens.First.Value;
            }

            tokens.RemoveFirst();

            return new Compound_Statement(statement_list);
        }

        private Statement ParseStatement(LinkedList<Token> tokens){
            if(tokens.Count<2){
                throw new ParserError("invalid tokens stream");
            }

            var firsttoken=tokens.First.Value;
            if(firsttoken.type==TokensType.Begin){
                return ParseCompoundStatement(tokens);
            }

            var secondtoken=tokens.First.Next.Value;
            if(secondtoken.type==TokensType.Assign){
                return ParseAssignment(tokens);
            }
            return ParseExpr(tokens);
        }

        private Statement ParseAssignment(LinkedList<Token> tokens){
            var id=new Id(tokens.First.Value.Value);
            tokens.RemoveFirst();
            tokens.RemoveFirst();
            if(tokens.First.Value.type==TokensType.Eof){
                throw new ParserError("expected expression");
            }
            return new Assign(id,ParseExpr(tokens));
        }

        private Expr ParseExpr(LinkedList<Token> tokens){
            Expr leftnode=ParseMul(tokens),rightnode=null;

            var token=tokens.First.Value;

            while((token.type==TokensType.Add) || (token.type==TokensType.Sub)){
                tokens.RemoveFirst();
                rightnode=ParseMul(tokens);
                if(token.type==TokensType.Add){
                    leftnode= new AddExpr(leftnode,rightnode);
                }
                else {
                    leftnode= new SubExpr(leftnode,rightnode);
                }
               token=tokens.First.Value;
            }
            return leftnode;
        }

        private Expr ParseMul(LinkedList<Token> tokens){
            Expr leftnode=ParseUnOpr(tokens),rightnode=null;

            var token=tokens.First.Value;

            while((token.type==TokensType.Mul) || (token.type==TokensType.Div)){
                tokens.RemoveFirst();
                rightnode=ParseUnOpr(tokens);
                if(token.type==TokensType.Mul){
                        leftnode= new MulExpr(leftnode,rightnode);
                }
                else {
                        leftnode= new DivExpr(leftnode,rightnode);
                }
                token=tokens.First.Value;
            }
            return leftnode;
        }

        private Expr ParseUnOpr(LinkedList<Token> tokens){
            var node=tokens.First.Value;
            if((node.type==TokensType.Add)||(node.type==TokensType.Sub)){
                tokens.RemoveFirst();
                switch(node.type){
                    case TokensType.Add:
                        return new PlusExpr(ParseParenth(tokens));
                    case TokensType.Sub:
                        return new MinusExpr(ParseParenth(tokens));
                }
            }
            return ParseParenth(tokens);
        }

        private Expr ParseParenth(LinkedList<Token> tokens){
            var node=tokens.First.Value;

            if(node.type==TokensType.LP){
                tokens.RemoveFirst();
                var Term=ParseExpr(tokens);
                node=tokens.First.Value;
                if(node.type!=TokensType.RP){
                    throw new ParserError("missing )");
                }
                tokens.RemoveFirst();
                return Term;
            }
            return ParseTerm(tokens);
        }

        private ObjNode ParseTerm(LinkedList<Token> tokens){
            if (tokens.Count==0){
                throw new ParserError("empty tokens stream");
            }

            var node=tokens.First.Value;
            tokens.RemoveFirst();
            switch(node.type){
                case TokensType.Number:return new Number(node.Value);
                case TokensType.str:return new Id(node.Value);
                case TokensType.DQ:return ParseString(tokens);
                default:throw new ParserError("expected Number token");
            }
        }

        private Str ParseString(LinkedList<Token> tokens){
            var firsttoken=tokens.First.Value;
            var str=new StringBuilder();
            while(firsttoken.type!=TokensType.DQ){
                str.Append(firsttoken.Value);
                tokens.RemoveFirst();
                if(tokens.Count<=0){
                    throw new ParserError(" missing \"");
                }
                firsttoken=tokens.First.Value;
            }
            tokens.RemoveFirst();
            return new Str(str.ToString());
        }
    }
}
