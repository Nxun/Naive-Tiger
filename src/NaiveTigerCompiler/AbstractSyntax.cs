using System.Collections.Generic;
using Antlr.Runtime;
using NaiveTigerCompiler.SymbolTable;
namespace NaiveTigerCompiler
{
    public class Position
    {
        public int Line;
        public int Column;
        public Position(int line, int column)
        {
            Line = line;
            Column = column;
        }
        public Position(IToken token)
        {
            this.Line = token.Line;
            this.Column = token.CharPositionInLine;
        }
    }

    namespace AbstractSyntax
    {
        public abstract class AbstractSyntaxBase
        {
            public Position Pos;
        }

        public class FieldExpressionList : AbstractSyntaxBase
        {
            public Symbol Name;
            public Expression Init;
            public FieldExpressionList Tail;
            public FieldExpressionList(Position pos, Symbol name, Expression init, FieldExpressionList tail)
            {
                Pos = pos;
                Name = name;
                Init = init;
                Tail = tail;
            }
        }

        public class FieldList : AbstractSyntaxBase
        {
            public Symbol Name;
            public Symbol Type;
            public bool Escape = true;
            public FieldList Tail;
            public FieldList(Position pos, Symbol name, Symbol type, FieldList tail)
            {
                Pos = pos;
                Name = name;
                Type = type;
                Tail = tail;
            }
        }

        public class ExpressionList
        {
            public Expression Head;
            public ExpressionList Tail;
            public ExpressionList(Expression head, ExpressionList tail)
            {
                Head = head;
                Tail = tail;
            }
        }

        public class DeclarationList
        {
            public Declaration Head;
            public DeclarationList Tail;
            public DeclarationList(Declaration head, DeclarationList tail)
            {
                Head = head;
                Tail = tail;
            }
        }
    }
}