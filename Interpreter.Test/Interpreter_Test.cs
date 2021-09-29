using Xunit;
using PL.Error;

namespace PL.Test.InterpreterTest {
    public class InterpreterTest {
        private readonly Interpreter _interpreter;

        public InterpreterTest(){
            _interpreter=new Interpreter();
        }

        private object setup(string input,string variablename){
            this._interpreter.ExecuteStatement(input);
            return this._interpreter.scope[variablename].Value;
        }

        [Theory]
        [InlineData("a=3;",3,"a")]
        [InlineData("a=0.3;",0.3,"a")]
        [InlineData("a=1.0;",1.0,"a")]
        [InlineData("a=10.34143;",10.34143,"a")]
        [InlineData("a=3+4;",3+4,"a")]
        [InlineData("a=3+4+4;",3+4+4,"a")]
        [InlineData("a=3-4;",3-4,"a")]
        [InlineData("a=3*4;",3*4,"a")]
        [InlineData("a=3/4;",3/4.0,"a")]
        [InlineData("a=3+4-6;",3+4-6,"a")]
        [InlineData("a=3-4+6;",3-4+6,"a")]
        [InlineData("a=3*4/2;",3*4/2.0,"a")]
        [InlineData("a=3/4*2;",3/4.0*2,"a")]
        [InlineData("a=3-4*6;",3-4*6,"a")]
        [InlineData("a=3*4+6;",3*4+6,"a")]
        [InlineData("a=3/4+6;",3/4.0+6,"a")]
        [InlineData("a=3-4/6;",3-4/6.0,"a")]
        [InlineData("a=(1);",1,"a")]
        [InlineData("a=(1+2);",1+2,"a")]
        [InlineData("a=(3-1);",3-1,"a")]
        [InlineData("a=(4*4);",4*4,"a")]
        [InlineData("a=(3/2);",3/2.0,"a")]
        [InlineData("a=1+(3/2);",1+(3/2.0),"a")]
        [InlineData("a=(1+3)/2;",(1+3)/2,"a")]
        [InlineData("a=(1+3)*2;",(1+3)*2,"a")]
        [InlineData("a=(((1+3)))-2;",(((1+3)))-2,"a")]
        [InlineData("a=1+(3-2);",1+(3-2),"a")]
        [InlineData("a=1-(3-2);",1-(3-2),"a")]
        [InlineData("a=-1;",-1,"a")]
        [InlineData("a=-(1);",-(1),"a")]
        [InlineData("a=-1+1;",0,"a")]
        [InlineData("a=-1-1;",-2,"a")]
        [InlineData("a=-2*3;",-2*3,"a")]
        [InlineData("a=-2/3;",-2/3.0,"a")]
        [InlineData("a=-2-3*2;",-2-3*2,"a")]
        public void Test_Arthm_Expression(string input,double expected,string varname)
        {
            var result=setup(input,varname);
            Assert.Equal(expected,result);
        }

        [Theory]
        [InlineData("a=\"test\";","\"test\"","a")]
        [InlineData("a=\"test \";","\"test \"","a")]
        [InlineData("a=\"1\"+\"2\";","\"12\"","a")]
        public void Test_Str_Expression(string input,string expected,string varname)
        {
            var result=setup(input,varname);
            Assert.Equal(expected,result);
        }

        [Theory]
        [InlineData("a=True;",true,"a")]
        [InlineData("a=False;",false,"a")]
        public void Test_BooleanType(string input,bool expected,string varname)
        {
            var result=setup(input,varname);
            Assert.Equal(expected,result);
        }

        [Theory]
        [InlineData("b=4>3;",true,"b")]
        [InlineData("b=4>=3;",true,"b")]
        [InlineData("b=3<4;",true,"b")]
        [InlineData("b=3<=4;",true,"b")]
        [InlineData("b=4==3;",false,"b")]
        [InlineData("b=4!=3;",true,"b")]
        public void Test_LogicalOP(string input,bool expected,string varname)
        {
            var result=setup(input,varname);
            Assert.Equal(expected,result);
        }


        [Theory]
        [InlineData("a=True;b=0;If(a){b=&True;}Else{b=&False;};",true,"b")]
        [InlineData("a=False;b=0;If(a){b=&True;}Else{b=&False;};",false,"b")]
        [InlineData("a=True;b=False;Loop(a){a=&False;b=&1;};",1,"b")]
        public void Test_RefAssignement(string input,object expected,string varname)
        {
            var result=setup(input,varname);
            Assert.Equal(expected.ToString(),result.ToString());
        }

        [Theory]
        [InlineData("a=True;If(a){b=1;}Else{b=0;};",true,"a")]
        [InlineData("a=False;If(a){b=1;}Else{b=0;};",false,"a")]
        public void Test_IfClause(string input,object expected,string varname)
        {
            var result=setup(input,varname);
            Assert.Equal(expected,result);
        }

        [Theory]
        [InlineData("a=True;b=False;Loop(b){a=False;b=1;};",true,"a")]
        [InlineData("a=False;b=0;Loop(a){a=False;b=1;};",false,"a")]
        public void Test_Loop(string input,object expected,string varname)
        {
            var result=setup(input,varname);
            Assert.Equal(expected,result);
        }

        [Theory]
        [InlineData("Get testfile;a=a*3;",18,"a")]
        public void Test_Load_File(string input,double expected,string varname) {
            var result=setup(input,varname);
            Assert.Equal(expected,result);
        }

        [Theory]
        [InlineData("a=3;Function(){a=&4;}();",4,"a")]
        [InlineData("a=3;Function(b){a=&b;}(4);",4,"a")]
        [InlineData("a=3;b=Function(b,c){a=&b+c;};b(4,1+3);",8,"a")]
        [InlineData("b=Function(b,c){Return b+c;};a=b(4,1+3);",8,"a")]
        [InlineData("b=Function(b,c){Return b+c;};a=b(\" function\",\" call \");","\" function call \"" ,"a")]
        public void Test_FunctionCall(string input,object expected,string varname)
        {
            var result=setup(input,varname);
            Assert.Equal(expected.ToString(),result.ToString());
        }

        [Theory]
        [InlineData("3/0;")]
        [InlineData("0/0;")]
        public void Test_DivideByZero(string input)
        {
            Assert.Throws<DivideByZeroError>(()=>_interpreter.ExecuteStatement(input));
        }

        [Theory]
        [InlineData("a=\"test\";a+1;")]
        [InlineData("b=Function(b,c){a=&b+c;};b(4,1+3);")]
        public void Test_ExecuteError(string input)
        {
            Assert.Throws<ExecuteError>(()=>_interpreter.ExecuteStatement(input));
        }
    }
}
