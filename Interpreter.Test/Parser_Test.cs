using System;
using System.Linq;
using System.Reflection;
using Xunit;
using PL.Error;
using PL.AST;
using PL.Parse;
using PL.Tokenize;

namespace PL.Test.ParserTest
{
    public class Parser_Test {

        private readonly Parser _parser;
        private readonly Tokenizer _tokenizer;
        private Expr _expr;
        private Statement _statement;
        private Statement_List _statement_list;

        public Parser_Test(){
            _tokenizer=new Tokenizer();
            _parser=new Parser();
        }

        private void SetupStatementListParsing(string input){
            var tokens=this._tokenizer.Tokenize(input);
            Type parsertype=typeof(Parser);
            MethodInfo parsestmtlst=parsertype.GetMethods(BindingFlags.NonPublic|BindingFlags.Instance).
                Where(x=>x.Name=="ParseStatements_List" && x.IsPrivate).First();
            this._statement_list=(Statement_List)parsestmtlst.Invoke(_parser,new object[]{tokens});
        }

        private void SetupStatementParsing(string input){
            var tokens=this._tokenizer.Tokenize(input);
            Type parsertype=typeof(Parser);
            MethodInfo parsestmt=parsertype.GetMethods(BindingFlags.NonPublic|BindingFlags.Instance).
                Where(x=>x.Name=="ParseStatement" && x.IsPrivate).First();
            this._statement=(Statement)parsestmt.Invoke(_parser,new object[]{tokens});

        }

        private void SetupExprParsing(string input){
            var tokens=this._tokenizer.Tokenize(input);
            Type parsertype=typeof(Parser);
            MethodInfo parseexpr=parsertype.GetMethods(BindingFlags.NonPublic|BindingFlags.Instance).
                Where(x=>x.Name=="ParseExpr" && x.IsPrivate).First();
            this._expr=(Expr)parseexpr.Invoke(_parser,new object[]{tokens});
        }

        private void SetupParsing(string input){
            var tokens=this._tokenizer.Tokenize(input);
            this._parser.Parse(tokens);
        }

        [Theory]
        [InlineData("a;")]
        [InlineData("3;")]
        [InlineData("1+3;")]
        [InlineData("A+b;")]
        [InlineData("a=b;")]
        [InlineData("a=b;3;")]
        [InlineData("a=3*b;a=3;")]
        public void Test_StatementListParse(string input)
        {
            SetupStatementListParsing(input);
            Assert.IsType<Statement_List>(this._statement_list);
        }

        [Theory]
        [InlineData("a",typeof(Expr))]
        [InlineData("3",typeof(Expr))]
        [InlineData("1+3",typeof(Expr))]
        [InlineData("A+b",typeof(Expr))]
        [InlineData("a=b",typeof(Assign))]
        public void Test_StatementParse(string input,Type expected)
        {
            SetupStatementParsing(input);
            Assert.IsAssignableFrom(expected,this._statement);
        }

        [Theory]
        [InlineData("3+3",typeof(AddExpr))]
        [InlineData("3-3",typeof(SubExpr))]
        [InlineData("3*3",typeof(MulExpr))]
        [InlineData("3/3",typeof(DivExpr))]
        [InlineData("-3",typeof(MinusExpr))]
        [InlineData("+3",typeof(PlusExpr))]
        public void Test_Op(string input,Type expected)
        {
            SetupExprParsing(input);
            Assert.IsType(expected,this._expr);
        }

        [Theory]
        [InlineData("3+3-3",typeof(SubExpr))]
        [InlineData("3-3+3",typeof(AddExpr))]
        [InlineData("3*3/3",typeof(DivExpr))]
        [InlineData("3/3*3",typeof(MulExpr))]
        public void Test_LeftAssciativity(string input,Type expected)
        {
            SetupExprParsing(input);
            Assert.IsType(expected,this._expr);
        }

        [Theory]
        [InlineData("3+3*3",typeof(AddExpr))]
        [InlineData("3/3+3",typeof(AddExpr))]
        [InlineData("3*3+3",typeof(AddExpr))]
        [InlineData("3/3-3",typeof(SubExpr))]
        [InlineData("3-3*3",typeof(SubExpr))]
        public void Test_Precedence(string input,Type expected)
        {
            SetupExprParsing(input);
            Assert.IsType(expected,this._expr);
        }

        [Theory]
        [InlineData("3",typeof(Number))]
        [InlineData("0.3",typeof(Number))]
        [InlineData("test",typeof(Id))]
        public void Test_ObjNodeType(string input,Type expected)
        {
            SetupExprParsing(input);
            Assert.IsType(expected,this._expr);
        }

        [Theory]
        [InlineData("0.3",typeof(ObjNode))]
        [InlineData("0.03",typeof(ObjNode))]
        [InlineData("test",typeof(ObjNode))]
        public void Test_ObjNode(string input,Type expected)
        {
            SetupExprParsing(input);
            Assert.IsAssignableFrom(expected,this._expr);
        }

        [Theory]
        [InlineData(" ;")]
        [InlineData("0test;")]
        [InlineData("+*;")]
        [InlineData("(1;")]
        [InlineData("1);")]
        [InlineData("a=;")]
        public void Test_ParseError(string input)
        {
            Assert.Throws<ParserError>(()=>SetupParsing(input));
        }
    }
}
