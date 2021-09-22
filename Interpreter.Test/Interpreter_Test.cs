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
            _interpreter.Execute(input);
        }

        [Theory]
        [InlineData("{a=3;a=a+3;}",6,"a")]
        [InlineData("{b=3;a=b+3;}",6,"a")]
        public void Test_CompoundStatements(string input,double expected,string varname)
        {
            setup(input);
            var result=(double)_interpreter.scope[varname].Value;
            Assert.Equal(expected,result);
        }

        [Theory]
        [InlineData("{a=True;if(a){b=1;}else{b=0;};}",true,"a")]
        [InlineData("{a=False;if(a){b=1;}else{b=0;};}",false,"a")]
        [InlineData("{a=True;b=False;Loop(b){a=False;b=1;};}",true,"a")]
        [InlineData("{a=False;b=0;Loop(a){a=False;b=1;};}",false,"a")]
        public void Test_IfClauseAndLoop(string input,bool expected,string varname)
        {
            setup(input);
            var result=(bool)_interpreter.scope[varname].Value;
            Assert.Equal(expected,result);
        }

        [Theory]
        [InlineData("{b=4>3;}",true,"b")]
        [InlineData("{b=4>=3;}",true,"b")]
        [InlineData("{b=3<4;}",true,"b")]
        [InlineData("{b=3<=4;}",true,"b")]
        [InlineData("{b=4==3;}",false,"b")]
        [InlineData("{b=4!=3;}",true,"b")]
        public void Test_LogicalOP(string input,bool expected,string varname)
        {
            setup(input);
            var result=(bool)_interpreter.scope[varname].Value;
            Assert.Equal(expected,result);
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
            var result=_interpreter.scope[varname];
            Assert.Equal(expected.ToString(),result.ToString());
        }

        [Theory]
        [InlineData("{a=\"test\";}","test","a")]
        [InlineData("{a=\"1\"+\"2\";}","12","a")]
        public void Test_Str_Assignement_Expression(string input,string expected,string varname)
        {
            setup(input);
            var result=_interpreter.scope[varname];
            Assert.Equal(expected,result.ToString());
        }

        [Theory]
        [InlineData("{a=True;}","True","a")]
        [InlineData("{a=False;}","False","a")]
        public void Test_BooleanType(string input,string expected,string varname)
        {
            setup(input);
            var result=_interpreter.scope[varname];
            Assert.Equal(expected,result.ToString());
        }

        //[Theory]
        //[InlineData("{Get load;a=a*3;}","18","a")]
        //public void Test_Load_File(string stmt,string expected,string varname) {
        //    setup(stmt);
        //    var result=_interpreter.scope[varname];
        //    Assert.Equal(expected,result.ToString());
        //}

        [Theory]
        [InlineData("{3/0;}")]
        [InlineData("{0/0;}")]
        public void Test_DivideByZero(string input)
        {
            Assert.Throws<DivideByZeroError>(()=>_interpreter.Execute(input));
        }

        [Theory]
        [InlineData("{a=\"test\";a+1;}")]
        public void Test_ExecuteError(string input)
        {
            Assert.Throws<ExecuteError>(()=>_interpreter.Execute(input));
        }
    }
}
