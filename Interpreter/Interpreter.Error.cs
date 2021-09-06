using System;

namespace Interpreter.Error {

    public class TokenError:Exception{

        public TokenError():base() {}

        public TokenError(string message):base(message) {}
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
