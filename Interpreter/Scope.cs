using System.Collections.Generic;
using PL.AST;

namespace PL {
    public class Scope{
        private readonly IDictionary<string,ObjNode> _dict;
        private readonly Scope _parent;

        public Scope Parent {get {return this._parent;}}

        public ObjNode this[string key]{
            get {
                if(this._dict.Keys.Contains(key)){ return this._dict[key]; }
                return null; }
            set =>this._dict[key]=value;
        }

        public Scope(Scope parent){
            this._parent=parent;
            _dict=new Dictionary<string,ObjNode>();
        }

        public IEnumerator<string> GetEnumerator(){
            return this._dict.Keys.GetEnumerator();
        }
    }

    public class FuncScope:Scope{
        private ObjNode _return;

        public ObjNode Return {get {return this._return;} set {this._return=value;}}

        public FuncScope(Scope parent):base(parent){
            this._return=new Null();
        }
    }
}
