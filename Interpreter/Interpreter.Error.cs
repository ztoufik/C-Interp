using System;

namespace Interpreter.Error {

    public class TokenExp:Exception{
        public TokenExp(string message):base(message)
        {}

        public TokenExp():base()
        {}
    }

    public class ParserError:Exception{
        public ParserError():base() {}

        public ParserError(string message):base(message) {}
    }
}
