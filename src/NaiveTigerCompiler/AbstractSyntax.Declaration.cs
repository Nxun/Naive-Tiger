using NaiveTigerCompiler.SymbolTable;
namespace NaiveTigerCompiler
{
    namespace AbstractSyntax
    {
        public abstract class Declaration : AbstractSyntaxBase
        {

        }

        public class FunctionDeclaration : Declaration
        {
            public Symbol Name;
            public FieldList Param;
            public NameType Result;
            public Expression Body;
            public FunctionDeclaration Next;
            public FunctionDeclaration(Position pos, Symbol name, FieldList param, NameType result, Expression body, FunctionDeclaration next)
            {
                Pos = pos;
                Name = name;
                Param = param;
                Result = result;
                Body = body;
                Next = next;
            }
        }

        public class TypeDeclaration : Declaration
        {
            public Symbol Name;
            public Type Type;
            public TypeDeclaration Next;
            public TypeDeclaration(Position pos, Symbol name, Type type, TypeDeclaration next)
            {
                Pos = pos;
                Name = name;
                Type = type;
                Next = next;
            }
        }

        public class VariableDeclaration : Declaration
        {
            public Symbol Name;
            public bool Escape = true;
            public NameType Type;
            public Expression Init;
            public VariableDeclaration(Position pos, Symbol name, NameType type, Expression init)
            {
                Pos = pos;
                Name = name;
                Type = type;
                Init = init;
            }
        }
    }
}