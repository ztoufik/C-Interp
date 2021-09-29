using System;
using System.IO;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using PL.Error;

namespace PL.Tokenize{
    static public class Tokenizer {
        static private OrderedDictionary map=new OrderedDictionary();
        private const uint CHARS_NUMBER=21;

        static Tokenizer() {
            KeyWordsInit();
            TokensInit();
        }

        static public TokenStream TokenizeFile(string filepath){
            int linenr=0;
            var tokenslist=new LinkedList<Token>();
            var tokensstreamlist=new LinkedList<LinkedList<Token>>();

            foreach(string line in File.ReadLines(filepath)){
                try{
                    tokensstreamlist.AddLast(GenerateTokens(line));
                }
                catch(TokenError e){
                    string message=e.Message;
                    throw new TokenError($"{message} line:{linenr} file:{filepath}");
                }
                linenr++;
            }

            foreach(var tokens in tokensstreamlist){
                foreach(var token in tokens){
                    tokenslist.AddLast(token);
                }
            }
            tokenslist.AddLast(new Token(TokensType.Eof,null));

            return new TokenStream(tokenslist);
        }


        static public  TokenStream TokenizeString(string input){
            var tokens=GenerateTokens(input);
            tokens.AddLast(new Token(TokensType.Eof,null));
            return new TokenStream(tokens);
        }

        static private  LinkedList<Token> GenerateTokens(string input){
            var tokens=new LinkedList<Token>();
            foreach(var tokenstring in LexicalGroup(input.AsSpan())){
                tokens.AddLast(Tokenize(tokenstring));
            }
            return tokens;
        }

        static private IEnumerable<string> LexicalGroup(ReadOnlySpan<char> subject){
            var lexicalgroup=new LinkedList<string>();
            var ignorepattern=new Regex(@"(^#.*#)|(^\s+)");
            string pattern=StringifyKeys();
            var regex=new Regex(pattern);
            string Value,ssubject;
            Match Matche,commentmatch;
            while(subject.Length>0){
                ssubject=subject.ToString();
                commentmatch=ignorepattern.Match(ssubject);
                if(commentmatch.Success){
                    Value=commentmatch.Value;
                }
                else{
                    Matche=regex.Match(ssubject);;
                    if(!Matche.Success){
                        throw new TokenError($"invalid token '{subject[0]}'");
                    }
                    Value=Matche.Value;
                    if(Value.Length>CHARS_NUMBER){
                        throw new TokenError($"Too Long Token '{Value}'");
                    }
                    lexicalgroup.AddLast(Value);
                }
                subject=subject.Slice(Value.Length);
            }
            return lexicalgroup;
        }

        static private  Token Tokenize(string tokenstring){
            var pattern=Match(tokenstring);
            return new Token((TokensType)map[pattern],tokenstring);
        }

        static private void TokensInit(){
            map[@"(^\d+(\.\d+)?)"]=TokensType.Number;
            map[@"(^\w[\w\d]*)"]=TokensType.Str;
            map["(^\".*?\")"]=TokensType.Qstr;
            map[@"(^=&)"]=TokensType.RefAssign;
            map[@"(^==)"]=TokensType.EQ;
            map[@"(^!=)"]=TokensType.Neq;
            map[@"(^\>=)"]=TokensType.GE;
            map[@"(^\<=)"]=TokensType.LE;
            map[@"(^\+)"]=TokensType.Add;
            map[@"(^\-)"]=TokensType.Sub;
            map[@"(^\*)"]=TokensType.Mul;
            map[@"(^\/)"]=TokensType.Div;
            map[@"(^\()"]=TokensType.LP;
            map[@"(^\))"]=TokensType.RP;
            map[@"(^\[)"]=TokensType.LB;
            map[@"(^\])"]=TokensType.RB;
            map[@"(^\{)"]=TokensType.Begin;
            map[@"(^\})"]=TokensType.End;
            map[@"(^;)"]=TokensType.Semi;
            map[@"(^=)"]=TokensType.Assign;
            map[@"(^:)"]=TokensType.Colon;
            map[@"(^,)"]=TokensType.CM;
            map[@"(^\>)"]=TokensType.GT;
            map[@"(^\<)"]=TokensType.LT;
        }

        static private void KeyWordsInit(){
            map[@"(^Function)"]=TokensType.FN;
            map[@"(^True)"]=TokensType.True;
            map[@"(^False)"]=TokensType.False;
            map[@"(^Loop)"]=TokensType.Loop;
            map[@"(^If)"]=TokensType.IF;
            map[@"(^Else)"]=TokensType.Else;
            map[@"(^Get)"]=TokensType.Get;
            map[@"(^Return)"]=TokensType.Ret;
        }

        static private string Match(string input){
            Regex regex;
            foreach(var key in map.Keys){
                regex=new Regex(key.ToString());
                if(regex.IsMatch(input)){ return key.ToString();};
            }
            return null;
        }

        static private string StringifyKeys(){
            var keys=new LinkedList<string>();
            foreach(var key in map.Keys){
                keys.AddLast(key.ToString());
            }
            string pattern=string.Join('|',keys);
            return pattern;
        }
    }
}
