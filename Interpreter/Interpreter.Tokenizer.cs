using System;
using System.Text;
using System.Collections.Generic;
using PL.Error;

namespace PL.Tokenize{

    public enum TokensType{
        Number,Add,Sub,Mul,Div,LP,RP,Begin,End,Semi,Assign,DQ,str,Eof
    }

    public struct Token {
        public readonly TokensType type;
        public readonly string Value;

        public Token(TokensType type,string Value)
        {
            this.type=type;
            this.Value=Value;
        }

        public override string ToString(){
            return $"({this.type},{this.Value})";
        }
    }

    public class Tokenizer {
        private IDictionary<string,Token> KeyWords;

        public Tokenizer() {
            KeyWords=new Dictionary<string,Token>();
        }

        private string Number(in string input,ref int pos){
            StringBuilder number=new StringBuilder(10);
            bool decipnt=false;
            while (pos<input.Length){
                if(Char.IsDigit(input[pos])) {
                    number.Append(input[pos]);
                }
                else 
                    if(input[pos]=='.'){
                        if(!decipnt){
                            try {
                                // test what character afterward
                                if(Char.IsDigit(input[pos+1])){
                                    number.Append(input[pos]);
                                    decipnt=true;
                                }
                                else
                                    throw new TokenError($"invalid decimal point at {pos}");
                            }
                            catch (IndexOutOfRangeException ){
                                throw new TokenError($"invalid decimal point at {pos}");
                            }
                        }
                        else
                            throw new TokenError($"invalid decimal point at {pos}");
                    }
                    else
                        break;

                pos++;
            }
            pos--;// point to the last digit character
            return number.ToString();
        }

        private string Str(in string input,ref int pos){
            StringBuilder id=new StringBuilder(10);
            while (pos<input.Length){
                if(Char.IsLetterOrDigit(input[pos])) {
                    id.Append(input[pos]);
                }
                else{
                    break;
                }
                pos++;
            }
            pos--;// point to the last digit character
            return id.ToString();
        }

        public LinkedList<Token> Tokenize(string input){
            var tokens=new LinkedList<Token>();
            int pos=0;

            while (pos<input.Length){
                switch(input[pos]){
                    case '+':tokens.AddLast(new Token(TokensType.Add,"+"));break;
                    case '-':tokens.AddLast(new Token(TokensType.Sub,"-"));break;
                    case '*':tokens.AddLast(new Token(TokensType.Mul,"*"));break;
                    case '/':tokens.AddLast(new Token(TokensType.Div,"/"));break;
                    case '(':tokens.AddLast(new Token(TokensType.LP,"("));break;
                    case ')':tokens.AddLast(new Token(TokensType.RP,")"));break;
                    case '{':tokens.AddLast(new Token(TokensType.Begin,"{"));break;
                    case '}':tokens.AddLast(new Token(TokensType.End,"}"));break;
                    case ';':tokens.AddLast(new Token(TokensType.Semi,";"));break;
                    case '=':tokens.AddLast(new Token(TokensType.Assign,"="));break;
                    case '"':tokens.AddLast(new Token(TokensType.DQ,"\""));break;
                    case var digit when Char.IsDigit(digit):
                            tokens.AddLast(new Token(TokensType.Number,Number(input,ref pos)));break;
                    case var alpha when Char.IsLetter(alpha):
                            string id=Str(input,ref pos);
                            //if(KeyWords.ContainsKey(id)){
                             //       }
                            tokens.AddLast(new Token(TokensType.str,id));break;
                    case var space when Char.IsWhiteSpace(space):break;
                    default: throw new TokenError($"invalid token {input[pos]} at {pos}");
                }

                pos++;
            }

            tokens.AddLast(new Token(TokensType.Eof,null));
            return tokens;
        }
    }
}
