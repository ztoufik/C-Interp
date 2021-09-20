using System.Collections.Generic;
using Xunit;
using PL.Tokenize;
using PL.Error;

namespace PL.Test.TokenizerTest
{
    public class Tokenize_Test
    {
        private LinkedList<Token> _tokens;
        private readonly Tokenizer _tokenize;

        public Tokenize_Test(){
            _tokenize=new Tokenizer();
        }

        private void setup(string input){
            _tokens=_tokenize.Tokenize(input);
        }

        [Theory]
        [InlineData("3")]
        [InlineData("0.3")]
        [InlineData("+")]
        [InlineData("-")]
        [InlineData("*")]
        [InlineData("/")]
        [InlineData("\"")]
        [InlineData("3*3")]
        [InlineData("3-4/3")]
        [InlineData("a=b")]
        [InlineData("test+3")]
        [InlineData("a=b+C")]
        public void Test_TokensStream(string input)
        {
            setup(input);
            var result=_tokens;
            Assert.All(result,e=>Assert.IsType<Token>(e));
        }

        [Theory]
        [InlineData("+",TokensType.Add)]
        [InlineData(" +",TokensType.Add)]
        [InlineData("-",TokensType.Sub)]
        [InlineData("*",TokensType.Mul)]
        [InlineData("/",TokensType.Div)]
        [InlineData("(",TokensType.LP)]
        [InlineData(")",TokensType.RP)]
        [InlineData("{",TokensType.Begin)]
        [InlineData("}",TokensType.End)]
        [InlineData(";",TokensType.Semi)]
        [InlineData("=",TokensType.Assign)]
        [InlineData("\"",TokensType.DQ)]
        [InlineData("Get",TokensType.Get)]
        [InlineData("True",TokensType.True)]
        [InlineData("False",TokensType.False)]
        [InlineData("if",TokensType.If)]
        [InlineData("else",TokensType.Else)]
        [InlineData("Loop",TokensType.Loop)]
        public void Test_TokenKeyWords_Type(string input,TokensType type) {
            setup(input);
            var result=_tokens.First.Value.type;
            Assert.Equal(type,result);
        }

        [Theory]
        [InlineData("+")]
        [InlineData(" +")]
        [InlineData("-")]
        [InlineData("*")]
        [InlineData("/")]
        [InlineData("(")]
        [InlineData(")")]
        [InlineData("{")]
        [InlineData("}")]
        [InlineData(";")]
        [InlineData("=")]
        [InlineData("\"")]
        [InlineData("Get")]
        [InlineData("True")]
        [InlineData("False")]
        [InlineData("if")]
        [InlineData("else")]
        [InlineData("Loop")]
        public void Test_TokenKeyWords_Value(string input) {
            setup(input);
            var result=_tokens.First.Value.Value;
            Assert.Equal(input.Trim(),result);
        }

        [Theory]
        [InlineData("3",TokensType.Number)]
        [InlineData("0.3",TokensType.Number)]
        [InlineData("test",TokensType.str)]
        public void Test_Tokens_Type(string input,TokensType type) {
            setup(input);
            var result=_tokens.First.Value.type;
            Assert.Equal(type,result);
        }

        [Theory]
        [InlineData("3")]
        [InlineData("0.3")]
        [InlineData("test")]
        public void Test_Tokens_Value(string input) {
            setup(input);
            var result=_tokens.First.Value.Value;
            Assert.Equal(input,result);
        }

        [Theory]
        [InlineData(";#comment#",1)]
        [InlineData("{a;#test#}",4)]
        public void Test_Comments(string input,int count) {
            setup(input);
            var result=_tokens.Count;
            Assert.Equal(count+1,result);
        }

        [Theory]
        [InlineData("3",1)]
        [InlineData("\"",1)]
        [InlineData("test",1)]
        [InlineData("+",1)]
        [InlineData("-",1)]
        [InlineData("*",1)]
        [InlineData("/",1)]
        [InlineData("3*3",3)]
        [InlineData("3-4/3",5)]
        [InlineData("test+3",3)]
        public void Test_TokensCount(string input,int count)
        {
            setup(input);
            var result=_tokens.Count;
            Assert.Equal(count+1,result);
        }

        [Theory]
        [InlineData("0. 3")]
        public void Test_TokenException(string input)
        {
            Assert.Throws<TokenError>(()=>_tokenize.Tokenize(input));
        }
    }
}
