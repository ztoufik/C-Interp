using System;

namespace PL.Error {

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

    public class ExecuteError:Exception{
        public ExecuteError():base() {}

        public ExecuteError(string message):base(message) {}
    }

}
