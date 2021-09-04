using System;

namespace Interpreter.Error {

    public class TokenExp:Exception{

        public TokenExp():base() {}

        public TokenExp(string message):base(message) {}
    }

    public class ParserError:Exception{
        public ParserError():base() {}

        public ParserError(string message):base(message) {}
    }

    public class DivideByZeroError:Exception{
        public DivideByZeroError():base(){}

        public DivideByZeroError(string message):base(message) {}
    }

}
