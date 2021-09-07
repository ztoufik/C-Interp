using Xunit;
using Interpreter.Tokenize;
using Interpreter.Error;

namespace Interpreter.Test.ParserTest
{
    public class Parser_Test {

        private readonly Tokenizer _tokenize;
        private Parser _parser;

        public Parser_Test(){
            _tokenize=new Tokenizer();
        }

        private void setup(string input){
            _tokenize.Input=input;
            _tokenize.Tokenize();
            _parser=new Parser(_tokenize.Tokens);
            _parser.Parse();
        }

        [Theory]
        [InlineData("3",3)]
        [InlineData("0.3",0.3)]
        [InlineData("1.0",1)]
        [InlineData("10.34143",10.34143)]
        [InlineData("3+4",3+4)]
        [InlineData("3+4+4",3+4+4)]
        [InlineData("3-4",3-4)]
        [InlineData("3*4",3*4)]
        [InlineData("3/4",3/4.0)]
        [InlineData("3+4-6",3+4-6)]
        [InlineData("3-4+6",3-4+6)]
        [InlineData("3*4/2",3*4/2.0)]
        [InlineData("3/4*2",3/4.0*2)]
        [InlineData("3-4*6",3-4*6)]
        [InlineData("3*4+6",3*4+6)]
        [InlineData("3/4+6",3/4.0+6)]
        [InlineData("3-4/6",3-4/6.0)]
        [InlineData("(1)",1)]
        [InlineData("(1+2)",1+2)]
        [InlineData("(3-1)",3-1)]
        [InlineData("(4*4)",4*4)]
        [InlineData("(3/2)",3/2.0)]
        [InlineData("1+(3/2)",1+(3/2.0))]
        [InlineData("(1+3)/2",(1+3)/2)]
        [InlineData("(1+3)*2",(1+3)*2)]
        [InlineData("(((1+3)))-2",(((1+3)))-2)]
        [InlineData("1+(3-2)",1+(3-2))]
        [InlineData("1-(3-2)",1-(3-2))]
        [InlineData("-1",-1)]
        [InlineData("-(1)",-(1))]
        [InlineData("-1+1",0)]
        [InlineData("-1-1",-2)]
        [InlineData("-2*3",-2*3)]
        [InlineData("-2/3",-2/3.0)]
        [InlineData("-2-3*2",-2-3*2)]
        public void Test_TokenType(string input,double expected)
        {
            setup(input);
            var result=_parser.AST.evaluate();
            Assert.Equal(expected,result);
        }

        [Theory]
        [InlineData("3/0")]
        [InlineData("0/0")]
        public void Test_DivideByZero(string input)
        {
            setup(input);
            Assert.Throws<DivideByZeroError>(()=>_parser.AST.evaluate());
        }
    }
}
