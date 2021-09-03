using Xunit;
using Interpreter;
using Interpreter.Tokenize;

namespace Interpreter.Test.ParserTest
{
    public class Parser_Test {

        private readonly Tokenizer _tokenize;
        private readonly Parser _parser;

        public Parser_Test(){
            _tokenize=new Tokenizer();
            _parser=new Parser();
        }

        private void setup(string input){
            _tokenize.Input=input;
            _tokenize.Tokenize();
            _parser.Tokens=_tokenize.Tokens;
            _parser.Parse();
        }

        [Theory]
        [InlineData("3",3)]
        [InlineData("0.3",0.3)]
        [InlineData("1.0",1)]
        [InlineData("10.34143",10.34143)]
        [InlineData("3+4",3+4)]
        [InlineData("3-4",3-4)]
        [InlineData("3*4",3*4)]
        [InlineData("3/4",3/4.0)]
        [InlineData("3+4-6",3+4-6)]
        [InlineData("3-4+6",3-4+6)]
        public void Test_TokenType(string input,double expected)
        {
            setup(input);
            var result=_parser.AST.evaluate();
            Assert.Equal(expected,result);
        }

    }
}
