using System.Collections.Generic;


namespace PL.Tokenize{
    public enum TokensType{
        Number,Str,Qstr,True,False,FN,
        Add,Sub,Mul,Div,
        LP,RP,LB,RB,Begin,End,Semi,Assign,RefAssign,Get,Colon,CM,
        Loop,IF,Else,Ret,
        EQ,Neq,GT,GE,LT,LE,
        Eof
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

    public class TokenStream{
        private LinkedList<Token> _tokenstream;
        private LinkedListNode<Token> _cursor;
        private uint _count;

        public Token? Current{get {return _cursor?.Value;}}
        public Token? Next{get {return _cursor?.Next?.Value;}}
        public uint Count{get {return (uint)this._tokenstream.Count-this._count;}}

        public TokenStream(LinkedList<Token> tokens) {
            this._tokenstream=tokens;
            this._cursor=this._tokenstream?.First;
        }

        public void Advance(){
            _cursor=_cursor?.Next;
            _count++;
        }

        public Token Pop(){
            var token=this._cursor.Value;
            this.Advance();
            return token;
        }

        public bool Eat(TokensType tokentype){
            if(this.Current?.type==tokentype){
                this.Advance();
                return true;
            }
            return false;
        }
    }
}
