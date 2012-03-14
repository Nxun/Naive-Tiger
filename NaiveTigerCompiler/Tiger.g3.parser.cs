using Antlr.Runtime;
namespace NaiveTigerCompiler
{
    partial class TigerParser
    {
        public override void DisplayRecognitionError(string[] tokenNames, RecognitionException e)
        {
            Compiler.ErrorList.Add(new Error(new Position(e.Line, e.CharPositionInLine), GetErrorMessage(e, tokenNames)));
        }
    }
}
