using Xunit;
using PL.Error;

namespace PL.Test.InterpreterTest
{
    public class InterpreterTest {

        private readonly Interpreter _interpreter;

        public InterpreterTest(){
            _interpreter=new Interpreter();
        }

        private void setup(string input){
            _interpreter.Input=input;
            _interpreter.Execute();
        }

        [Theory]
        [InlineData("{a=3;a=a+3;}",6,"a")]
        [InlineData("{b=3;a=b+3;}",6,"a")]
        public void Test_CompoundStatements(string input,double expected,string varname)
        {
            setup(input);
            var result=_interpreter.Scope[varname];
            Assert.Equal(expected.ToString(),result.ToString());
        }

        [Theory]
        [InlineData("{a=3;}",3,"a")]
        [InlineData("{a=0.3;}",0.3,"a")]
        [InlineData("{a=1.0;}",1.0,"a")]
        [InlineData("{a=10.34143;}",10.34143,"a")]
        [InlineData("{a=3+4;}",3+4,"a")]
        [InlineData("{a=3+4+4;}",3+4+4,"a")]
        [InlineData("{a=3-4;}",3-4,"a")]
        [InlineData("{a=3*4;}",3*4,"a")]
        [InlineData("{a=3/4;}",3/4.0,"a")]
        [InlineData("{a=3+4-6;}",3+4-6,"a")]
        [InlineData("{a=3-4+6;}",3-4+6,"a")]
        [InlineData("{a=3*4/2;}",3*4/2.0,"a")]
        [InlineData("{a=3/4*2;}",3/4.0*2,"a")]
        [InlineData("{a=3-4*6;}",3-4*6,"a")]
        [InlineData("{a=3*4+6;}",3*4+6,"a")]
        [InlineData("{a=3/4+6;}",3/4.0+6,"a")]
        [InlineData("{a=3-4/6;}",3-4/6.0,"a")]
        [InlineData("{a=(1);}",1,"a")]
        [InlineData("{a=(1+2);}",1+2,"a")]
        [InlineData("{a=(3-1);}",3-1,"a")]
        [InlineData("{a=(4*4);}",4*4,"a")]
        [InlineData("{a=(3/2);}",3/2.0,"a")]
        [InlineData("{a=1+(3/2);}",1+(3/2.0),"a")]
        [InlineData("{a=(1+3)/2;}",(1+3)/2,"a")]
        [InlineData("{a=(1+3)*2;}",(1+3)*2,"a")]
        [InlineData("{a=(((1+3)))-2;}",(((1+3)))-2,"a")]
        [InlineData("{a=1+(3-2);}",1+(3-2),"a")]
        [InlineData("{a=1-(3-2);}",1-(3-2),"a")]
        [InlineData("{a=-1;}",-1,"a")]
        [InlineData("{a=-(1);}",-(1),"a")]
        [InlineData("{a=-1+1;}",0,"a")]
        [InlineData("{a=-1-1;}",-2,"a")]
        [InlineData("{a=-2*3;}",-2*3,"a")]
        [InlineData("{a=-2/3;}",-2/3.0,"a")]
        [InlineData("{a=-2-3*2;}",-2-3*2,"a")]
        public void Test_Arthm_Assignement_Expression(string input,double expected,string varname)
        {
            setup(input);
            var result=_interpreter.Scope[varname];
            Assert.Equal(expected.ToString(),result.ToString());
        }

        [Theory]
        [InlineData("{a=\"test\";}","test","a")]
        public void Test_Str_Assignement_Expression(string input,string expected,string varname)
        {
            setup(input);
            var result=_interpreter.Scope[varname];
            Assert.Equal(expected,result.ToString());
        }
        [Theory]
        [InlineData("{3/0;}")]
        [InlineData("{0/0;}")]
        public void Test_DivideByZero(string input)
        {
            _interpreter.Input=input;
            Assert.Throws<DivideByZeroError>(()=>_interpreter.Execute());
        }
    }
}
