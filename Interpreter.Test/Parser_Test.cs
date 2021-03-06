using System;
using System.Linq;
using Xunit;
using PL.Error;
using PL.AST;
using PL.Parse;
using PL.Tokenize;

namespace PL.Test.ParserTest {
    public class Parser_Test {
        private Program SetupParsing(string input){
            return Parser.Parse(Tokenizer.TokenizeString(input));
        }

        [Theory]
        [InlineData("a;")]
        [InlineData("Get testfile;")]
        [InlineData("a=True;")]
        [InlineData("3;")]
        [InlineData("1+3;")]
        [InlineData("A+b;")]
        [InlineData("a=b;")]
        [InlineData("a=b;3;")]
        [InlineData("a=3*b;a=3;")]
        [InlineData("If(True){3;};")]
        [InlineData("Loop(True){3;};")]
        [InlineData("3==4;")]
        [InlineData("3!=4;")]
        [InlineData("3>4;")]
        [InlineData("3>=4;")]
        [InlineData("3<=4;")]
        [InlineData("3<4;")]
        public void Test_Parsing_Program(string input)
        {
            var programast=SetupParsing(input);
            Assert.IsType<Program>(programast);
        }

        [Theory]
        [InlineData("3+3-3;",typeof(SubExpr))]
        [InlineData("3-3+3;",typeof(AddExpr))]
        [InlineData("3*3/3;",typeof(DivExpr))]
        [InlineData("3/3*3;",typeof(MulExpr))]
        public void Test_LeftAssciativity(string input,Type expected)
        {
            var programast=SetupParsing(input);
            var node=programast.StmtList.First();
            Assert.IsType(expected,node);
        }

        [Theory]
        [InlineData("3+3*3;",typeof(AddExpr))]
        [InlineData("3/3+3;",typeof(AddExpr))]
        [InlineData("3*3+3;",typeof(AddExpr))]
        [InlineData("3/3-3;",typeof(SubExpr))]
        [InlineData("3-3*3;",typeof(SubExpr))]
        public void Test_Precedence(string input,Type expected)
        {
            var programast=SetupParsing(input);
            var node=programast.StmtList.First();
            Assert.IsType(expected,node);
        }

        [Theory]
        [InlineData("0.3;",typeof(ObjNode))]
        [InlineData("0.03;",typeof(ObjNode))]
        [InlineData("\" test\";",typeof(ObjNode))]
        [InlineData("True;",typeof(ObjNode))]
        [InlineData("False;",typeof(ObjNode))]
        [InlineData("Function (){a3;};",typeof(ObjNode))]
        [InlineData("Function (aaa){a3;};",typeof(ObjNode))]
        public void Test_ObjNode(string input,Type expected)
        {
            var programast=SetupParsing(input);
            var node=programast.StmtList.First();
            Assert.IsAssignableFrom(expected,node);
        }

        [Theory]
        [InlineData("3;",typeof(Number))]
        [InlineData("0.3;",typeof(Number))]
        [InlineData("\"test\";",typeof(Str))]
        [InlineData("True;",typeof(BLN))]
        [InlineData("False;",typeof(BLN))]
        [InlineData("Function (){a3;};",typeof(Function))]
        [InlineData("Function (aaa){a3;};",typeof(Function))]
        [InlineData("Function (aaa,bbb){a3;};",typeof(Function))]
        public void Test_ObjNodeType(string input,Type expected)
        {
            var programast=SetupParsing(input);
            var node=programast.StmtList.First();
            Assert.IsAssignableFrom(expected,node);
        }

        [Theory]
        [InlineData("[3:4];",typeof(TableExpr))]
        [InlineData("[3:4,\"toufik\":True];",typeof(TableExpr))]
        [InlineData("[];",typeof(TableExpr))]
        public void Test_TableExpr(string input,Type expected)
        {
            var programast=SetupParsing(input);
            var node=programast.StmtList.First();
            Assert.IsAssignableFrom(expected,node);
        }
        [Theory]
        [InlineData("a();",typeof(Call))]
        [InlineData("b()();",typeof(Call))]
        [InlineData("Function (){a3;}();",typeof(Call))]
        [InlineData("a(a);",typeof(Call))]
        [InlineData("b(a)(b);",typeof(Call))]
        [InlineData("Function (){a3;}(a);",typeof(Call))]
        [InlineData("a(a,b);",typeof(Call))]
        [InlineData("b(b,a)(c,d);",typeof(Call))]
        [InlineData("Function (){a3;}(a,b);",typeof(Call))]
        public void Test_Call(string input,Type expected)
        {
            var programast=SetupParsing(input);
            var node=programast.StmtList.First();
            Assert.IsAssignableFrom(expected,node);
        }

        [Theory]
        [InlineData("a[3];",typeof(TIndex))]
        [InlineData("a()[3];",typeof(TIndex))]
        [InlineData("[3:5][3];",typeof(TIndex))]
        public void Test_Index(string input,Type expected)
        {
            var programast=SetupParsing(input);
            var node=programast.StmtList.First();
            Assert.IsAssignableFrom(expected,node);
        }

        [Theory]
        [InlineData("If(True){3;};")]
        [InlineData("If(False){3;};")]
        [InlineData("If(False){3;}Else{4;};")]
        [InlineData("If(True){3;}Else{4;};")]
        public void Test_IfClause(string input) {
                var programast=SetupParsing(input);
                var node=programast.StmtList.First();
                Assert.IsType<IfClause>(node);
            }

        [Theory]
        [InlineData("Loop(True){3;};")]
        [InlineData("Loop(False){3;};")]
        public void Test_Loop(string input) {
                var programast=SetupParsing(input);
                var node=programast.StmtList.First();
                Assert.IsType<Loop>(node);
        }

        [Theory]
        [InlineData("a=3;")]
        [InlineData("a=b;")]
        public void Test_Assignement(string input) {
                var programast=SetupParsing(input);
                var node=programast.StmtList.First();
                Assert.IsType<Assign>(node);
        }

        [Theory]
        [InlineData("Return 3;")]
        [InlineData("Return a;")]
        public void Test_Return(string input) {
                var programast=SetupParsing(input);
                var node=programast.StmtList.First();
                Assert.IsType<Return>(node);
        }

        [Theory]
        [InlineData("a=&3;")]
        [InlineData("a=&b;")]
        public void Test_RefAssignement(string input) {
                var programast=SetupParsing(input);
                var node=programast.StmtList.First();
                Assert.IsType<RefAssign>(node);
        }

        [Theory]
        [InlineData("Get testfile;")]
        public void Test_Get(string input) {
                var programast=SetupParsing(input);
                var node=programast.StmtList.First();
                Assert.IsType<Get>(node);
        }

        [Theory]
        [InlineData("0test;")]
        [InlineData("(1;")]
        [InlineData("1);")]
        [InlineData("+;")]
        [InlineData("-*;")]
        [InlineData("/+;")]
        [InlineData("<=;")]
        [InlineData("<=*;")]
        [InlineData("=;")]
        [InlineData("a=;")]
        [InlineData("=&;")]
        [InlineData("a=&;")]
        [InlineData("Get 11;")]
        [InlineData("If {3;}")]
        [InlineData("If;")]
        [InlineData("If (True) {3;}Else;")]
        [InlineData("Loop {3;};")]
        [InlineData("Loop (True);")]
        [InlineData("Function ({a3;};")]
        [InlineData("Function ){a3;};")]
        [InlineData("Function (a,){a3;};")]
        [InlineData("Function (,a){a3;};")]
        [InlineData("Function (b a){a3;};")]
        [InlineData("[3:3;")]
        [InlineData("3:3];")]
        [InlineData("[3:3,];")]
        [InlineData("[3:3 4:3];")]
        [InlineData("a(;")]
        [InlineData("a);")]
        [InlineData("b(,a)(c,d);")]
        [InlineData("Function (){a3;}(a,);")]
        [InlineData("Return ;")]
        [InlineData("a[];")]
        [InlineData("[3:5][];")]
        public void Test_ParseError(string input)
        {
            Assert.Throws<ParserError>(()=>SetupParsing(input));
        }
    }
}
