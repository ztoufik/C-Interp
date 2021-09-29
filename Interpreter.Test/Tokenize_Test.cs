using Xunit;
using PL.Tokenize;
using PL.Error;

namespace PL.Test.TokenizerTest {
    public class Tokenize_Test
    {
        [Theory]
        [InlineData("+",TokensType.Add)]
        [InlineData("-",TokensType.Sub)]
        [InlineData("*",TokensType.Mul)]
        [InlineData("/",TokensType.Div)]
        [InlineData("(",TokensType.LP)]
        [InlineData(")",TokensType.RP)]
        [InlineData("{",TokensType.Begin)]
        [InlineData("}",TokensType.End)]
        [InlineData(";",TokensType.Semi)]
        [InlineData("=",TokensType.Assign)]
        [InlineData(",",TokensType.Colon)]
        [InlineData("Get",TokensType.Get)]
        [InlineData("True",TokensType.True)]
        [InlineData("False",TokensType.False)]
        [InlineData("If",TokensType.If)]
        [InlineData("Else",TokensType.Else)]
        [InlineData("Loop",TokensType.Loop)]
        [InlineData("Function",TokensType.Fn)]
        [InlineData("Return",TokensType.Ret)]
        [InlineData("==",TokensType.Eq)]
        [InlineData("!=",TokensType.NEq)]
        [InlineData("<",TokensType.LT)]
        [InlineData("<=",TokensType.LE)]
        [InlineData(">",TokensType.GT)]
        [InlineData(">=",TokensType.GE)]
        [InlineData("=&",TokensType.RefAssign)]
        public void Test_TokenKeyWords_Type(string input,TokensType type) {
            var result=Tokenizer.TokenizeString(input).Current.Value.type;
            Assert.Equal(type,result);
        }

        [Theory]
        [InlineData("+")]
        [InlineData("-")]
        [InlineData("*")]
        [InlineData("/")]
        [InlineData("(")]
        [InlineData(")")]
        [InlineData("{")]
        [InlineData("}")]
        [InlineData(";")]
        [InlineData("=")]
        [InlineData(",")]
        [InlineData("Get")]
        [InlineData("True")]
        [InlineData("False")]
        [InlineData("If")]
        [InlineData("Else")]
        [InlineData("Loop")]
        [InlineData("Function")]
        [InlineData("Return")]
        [InlineData("==")]
        [InlineData("!=")]
        [InlineData("<")]
        [InlineData("<=")]
        [InlineData(">")]
        [InlineData(">=")]
        [InlineData("=&")]
        public void Test_TokenKeyWords_Value(string input) {
            var result=Tokenizer.TokenizeString(input).Current.Value.Value;
            Assert.Equal(input,result);
        }

        [Theory]
        [InlineData("3",TokensType.Number)]
        [InlineData("0.3",TokensType.Number)]
        [InlineData("test",TokensType.str)]
        [InlineData("\" test \"",TokensType.Qstr)]
        public void Test_Tokens_Type(string input,TokensType type) {
            var result=Tokenizer.TokenizeString(input).Current.Value.type;
            Assert.Equal(type,result);
        }

        [Theory]
        [InlineData("3")]
        [InlineData("0.3")]
        [InlineData("test")]
        [InlineData("\" test \"")]
        public void Test_Tokens_Value(string input) {
            var result=Tokenizer.TokenizeString(input).Current.Value.Value;
            Assert.Equal(input,result);
        }

        [Theory]
        [InlineData(";#comment#",1)]
        [InlineData("{a;#test#}",4)]
        public void Test_Comments_Spaces(string input,uint count) {
            var result=Tokenizer.TokenizeString(input).Count;
            Assert.Equal(count+1,result);
        }

        [Theory]
        [InlineData("3",1)]
        [InlineData("test",1)]
        [InlineData("+",1)]
        [InlineData("-",1)]
        [InlineData("*",1)]
        [InlineData("/",1)]
        [InlineData("3*3",3)]
        [InlineData("3-4/3",5)]
        [InlineData("test+3",3)]
        public void Test_TokensCount(string input,uint count)
        {
            var result=Tokenizer.TokenizeString(input).Count;
            Assert.Equal(count+1,result);
        }

        [Theory]
        [InlineData("0. 3")]
        public void Test_TokenException(string input)
        {
            Assert.Throws<TokenError>(()=>Tokenizer.TokenizeString(input));
        }
    }
}
