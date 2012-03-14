using NaiveTigerCompiler.Translate;
using NaiveTigerCompiler.Temp;
namespace NaiveTigerCompiler
{
    namespace Semantics
    {
        public class ExpressionType
        {
            public Exp Exp;
            public Types.Type Type;
            public ExpressionType(Exp exp, Types.Type type)
            {
                Exp = exp;
                Type = type;
            }
        }
    }
}