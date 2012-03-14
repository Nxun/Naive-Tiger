using NaiveTigerCompiler.Translate;
using NaiveTigerCompiler.Temp;
namespace NaiveTigerCompiler
{
    namespace Semantics
    {
        public abstract class Entry
        {

        }

        public class FunctionEntry : Entry
        {
            public Level Level;
            public Label Label;
            public Types.RECORD Formals;
            public Types.Type Result;
            public FunctionEntry(Level level, Label label, Types.RECORD formals, Types.Type result)
            {
                Level = level;
                Label = label;
                Formals = formals;
                Result = result;
            }
        }

        public class StandardFunctionEntry : FunctionEntry
        {
            public StandardFunctionEntry(Level level, Label label, Types.RECORD formals, Types.Type result)
                : base(level, label, formals, result)
            {
            }
        }

        public class VariableEntry : Entry
        {
            public Access Access;
            public Types.Type Type;
            public VariableEntry(Access access, Types.Type type)
            {
                Access = access;
                Type = type;
            }
        }

        public class LoopVariableEntry : VariableEntry
        {
            public LoopVariableEntry(Access access, Types.Type type)
                : base(access, type)
            {
            }
        }
    }
}