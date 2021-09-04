using Xunit;
using Interpreter.Tokenize;
using Interpreter.Error;

namespace Interpreter.Test.TokenizerTest
{
    public class Tokenize_Test
    {
        private readonly Tokenizer _tokenize;
        public Tokenize_Test(){
            _tokenize=new Tokenizer();
        }

        private void setup(string input){
            _tokenize.Input=input;
            _tokenize.Tokenize();
        }

        [Theory]
        [InlineData("3",TokensType.Number)]
        [InlineData("0.3",TokensType.Number)]
        [InlineData("+",TokensType.Add)]
        [InlineData(" +",TokensType.Add)]
        [InlineData("-",TokensType.Sub)]
        [InlineData("*",TokensType.Mul)]
        [InlineData("/",TokensType.Div)]
        public void Test_TokenType(string input,TokensType type)
        {
            setup(input);
            var result=_tokenize.Tokens.Peek().type;
            Assert.Equal(type,result);
        }

        [Theory]
        [InlineData("3")]
        [InlineData("0.3")]
        [InlineData("+")]
        [InlineData("-")]
        [InlineData("*")]
        [InlineData("/")]
        public void Test_Tokenvalue(string input)
        {
            setup(input);
            var result=_tokenize.Tokens.Peek().Value;
            Assert.Equal(input,result);
        }

        [Theory]
        [InlineData(";")]
        [InlineData("0. 3")]
        public void Test_TokenException(string input)
        {
            _tokenize.Input=input;
            Assert.Throws<TokenExp>(()=>_tokenize.Tokenize());
        }

        [Theory]
        [InlineData("3",1)]
        [InlineData("0.3",1)]
        [InlineData("+",1)]
        [InlineData("-",1)]
        [InlineData("*",1)]
        [InlineData("/",1)]
        [InlineData("3*3",3)]
        [InlineData("3-4/3",5)]
        public void Test_TokensCount(string input,int count)
        {
            setup(input);
            var result=_tokenize.Tokens.Count;
            Assert.Equal(count+1,result);
        }

        [Theory]
        [InlineData("3")]
        [InlineData("0.3")]
        [InlineData("+")]
        [InlineData("-")]
        [InlineData("*")]
        [InlineData("/")]
        [InlineData("3*3")]
        [InlineData("3-4/3")]
        public void Test_TokensStream(string input)
        {
            setup(input);
            var result=_tokenize.Tokens;
            Assert.All(result,e=>Assert.IsType<Token>(e));
        }

    }
}
