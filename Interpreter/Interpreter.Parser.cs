using System.Collections.Generic;
using PL.Tokenize;
using PL.AST;
using PL.Error;

namespace PL.Parse {

    public class Parser{
        
        public Parser(){ }

        public Statement_List Parse(LinkedList<Token> tokens){
            return ParseStatements_List(tokens);
        }

        private Statement_List ParseStatements_List(LinkedList<Token> tokens){
            if(tokens.Count==0){
                throw new ParserError("invalid tokens stream");
            }

            var statements_list=new Queue<Statement>();

            var first_statement_tokens=new LinkedList<Token>();
            var token=tokens.First.Value;
            while(token.type!=TokensType.Semi){
                first_statement_tokens.AddLast(token);
                tokens.RemoveFirst();
                if(tokens.Count==0){
                    throw new ParserError("missing ;");
                }
            token=tokens.First.Value;
            }
            first_statement_tokens.AddLast(new Token(TokensType.Eof,null));
            var first_statement=ParseStatement(first_statement_tokens);
            statements_list.Enqueue(first_statement);
            tokens.RemoveFirst();

            if(tokens.First.Value.type!=TokensType.Eof){
                foreach(var stmt in ParseStatements_List(tokens).statements){
                    statements_list.Enqueue(stmt);
                }
            }

            return new Statement_List(statements_list.ToArray());

        }

        private Statement ParseStatement(LinkedList<Token> tokens){
            if(tokens.Count<2){
                throw new ParserError("invalid tokens stream");
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

            Expr AddtvNode=ParseMul(tokens);

            var token=tokens.First.Value;

            while((token.type==TokensType.Add) || (token.type==TokensType.Sub)){
                tokens.RemoveFirst();
                if(token.type==TokensType.Add){
                        AddtvNode= new AddExpr(AddtvNode,ParseMul(tokens));
                }
                else {
                        AddtvNode= new SubExpr(AddtvNode,ParseMul(tokens));
                }
             token=tokens.First.Value;
            }
            if(token.type==TokensType.Eof){
                return AddtvNode;
            }
            throw new ParserError("expected operation");
        }

        private Expr ParseMul(LinkedList<Token> tokens){

            Expr MultplNode=ParseUnOpr(tokens);

            var token=tokens.First.Value;

            while((token.type==TokensType.Mul) || (token.type==TokensType.Div)){
                tokens.RemoveFirst();
                if(token.type==TokensType.Mul){
                        MultplNode= new MulExpr(MultplNode,ParseUnOpr(tokens));
                }
                else {
                        MultplNode= new DivExpr(MultplNode,ParseUnOpr(tokens));
                }
                token=tokens.First.Value;
            }
            return MultplNode;
        }

        private Expr ParseUnOpr(LinkedList<Token> tokens){

            var node=tokens.First.Value;
            switch(node.type){
                case TokensType.Add:
                    tokens.RemoveFirst();
                    return new PlusExpr(ParseParenth(tokens));
                case TokensType.Sub:
                    tokens.RemoveFirst();
                    return new MinusExpr(ParseParenth(tokens));
                default:return ParseParenth(tokens);
            }
        }

        private Expr ParseParenth(LinkedList<Token> tokens){

            var node=tokens.First.Value;

            if(node.type==TokensType.LP){
                int LPN=1;
                tokens.RemoveFirst();
                var tokensstream=new LinkedList<Token>();

                while(LPN>0){
                    node=tokens.First.Value;
                    tokens.RemoveFirst();
                    if(node.type==TokensType.LP){
                        LPN++;
                        continue;
                    }

                    if(node.type==TokensType.RP){
                        LPN--;
                        continue;
                    }
                    tokensstream.AddLast(node);
                    if(tokens.Count<=0){
                        throw new ParserError("missing )");
                    }
                }
                tokensstream.AddLast(new Token(TokensType.Eof,null));
                return ParseExpr(tokensstream);
            }

            return ParseTerm(tokens);
        }

        private Expr ParseTerm(LinkedList<Token> tokens){

            if (tokens.Count==0){
                throw new ParserError("empty tokens stream");
            }

            var node=tokens.First.Value;
            tokens.RemoveFirst();
            switch(node.type){
                case TokensType.Number:return new Number(node.Value);
                case TokensType.Id:return new Id(node.Value);
                default:throw new ParserError("expected Number token");
            }
        }
    }
}
