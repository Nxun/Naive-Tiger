using NaiveTigerCompiler.SymbolTable;
namespace NaiveTigerCompiler
{
    namespace AbstractSyntax
    {
        public abstract class Type : AbstractSyntaxBase
        {

        }

        public class ArrayType : Type
        {
            public Symbol Type;
            public ArrayType(Position pos, Symbol type)
            {
                Pos = pos;
                Type = type;
            }
        }

        public class NameType : Type
        {
            public Symbol Name;
            public NameType(Position pos, Symbol name)
            {
                Pos = pos;
                Name = name;
            }
        }

        public class RecordType : Type
        {
            public FieldList Fields;
            public RecordType(Position pos, FieldList fields)
            {
                Pos = pos;
                Fields = fields;
            }
        }
    }
}