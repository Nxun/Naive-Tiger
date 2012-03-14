using NaiveTigerCompiler.SymbolTable;
namespace NaiveTigerCompiler
{
    namespace AbstractSyntax
    {
        public abstract class Variable : AbstractSyntaxBase
        {

        }

        public class FieldVariable : Variable
        {
            public Variable Var;
            public Symbol Field;
            public FieldVariable(Position pos, Variable var, Symbol field)
            {
                Pos = pos;
                Var = var;
                Field = field;
            }
        }

        public class SimpleVariable : Variable
        {
            public Symbol Name;
            public SimpleVariable(Position pos, Symbol name)
            {
                Pos = pos;
                Name = name;
            }
        }

        public class SubscriptVariable : Variable
        {
            public Variable Var;
            public Expression Index;
            public SubscriptVariable(Position pos, Variable var, Expression index)
            {
                Pos = pos;
                Var = var;
                Index = index;
            }
        }
    }
}