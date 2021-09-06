using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Interpreter.Error;

namespace Interpreter.Tokenize{

    public enum TokensType{
        Number,Add,Sub,Mul,Div,LP,RP,Eof
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
        public readonly LinkedList<Token> Tokens;
        private string input;

        public string Input {
            get {return input;}
            set {input=value;}
        }

        public Tokenizer() {
            this.Tokens=new LinkedList<Token>();
        }

        public Tokenizer(string input):this() {
            this.input=input;
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

        public void Tokenize(){
            int pos=0;

            while (pos<input.Length){
                switch(input[pos]){
                    case '+':Tokens.AddLast(new Token(TokensType.Add,"+"));break;
                    case '-':Tokens.AddLast(new Token(TokensType.Sub,"-"));break;
                    case '*':Tokens.AddLast(new Token(TokensType.Mul,"*"));break;
                    case '/':Tokens.AddLast(new Token(TokensType.Div,"/"));break;
                    case '(':Tokens.AddLast(new Token(TokensType.LP,"("));break;
                    case ')':Tokens.AddLast(new Token(TokensType.RP,")"));break;
                    case var digit when Char.IsDigit(digit):
                            Tokens.AddLast(new Token(TokensType.Number,Number(input,ref pos)));break;
                    case var space when Char.IsWhiteSpace(space):break;
                    default: throw new TokenError($"invalid token {input[pos]} at {pos}");
                }

                pos++;
            }

            Tokens.AddLast(new Token(TokensType.Eof,null));
        }

        public override string ToString(){
            return string.Join("\n",this.Tokens.Select(token => token.ToString()));
        }
    }
}
