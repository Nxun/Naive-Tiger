using System;
namespace NaiveTigerCompiler
{
    public class Error
    {
        public Position Pos;
        public string Message;
        public Error(Position pos, string message)
        {
            Pos = pos;
            Message = message;
        }
        public static void Report(Position pos, string message)
        {
            Compiler.ErrorList.Add(new Error(pos, message));
            //Console.Out.WriteLine("error:line{0} column{1} {2}", pos.Line, pos.Column, message);
        }

    }

    public class FatalError : Exception
    {
        public FatalError(string message) : base(message)
        {
            Compiler.ErrorList.Add(new Error(new Position(0, 0), "Fatal Error: " + message));
        }

        public FatalError(Position pos, string message) : base(message)
        {
            Compiler.ErrorList.Add(new Error(pos, "Fatal Error: " + message));
        }
    }
}
