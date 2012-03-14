using NaiveTigerCompiler.SymbolTable;
namespace NaiveTigerCompiler
{
    namespace AbstractSyntax
    {
        public enum Operator
        {
            Plus,
            Minus,
            Times,
            Divide,
            Equal,
            NotEqual,
            LessThan,
            LessEqual,
            GreaterThan,
            GreaterEqual
        }

        public abstract class Expression : AbstractSyntaxBase
        {

        }

        public class ArrayExpression : Expression
        {
            public Symbol Type;
            public Expression Size, Init;
            public ArrayExpression(Position pos, Symbol type, Expression size, Expression init)
            {
                Pos = pos;
                Type = type;
                Size = size;
                Init = init;
            }
        }

        public class AssignExpression : Expression
        {
            public Variable Var;
            public Expression Exp;
            public AssignExpression(Position pos, Variable var, Expression exp)
            {
                Pos = pos;
                Var = var;
                Exp = exp;
            }
        }

        public class BreakExpression : Expression
        {
            public BreakExpression(Position pos)
            {
                Pos = pos;
            }
        }

        public class CallExpression : Expression
        {
            public Symbol Func;
            public ExpressionList Args;
            public CallExpression(Position pos, Symbol func, ExpressionList args)
            {
                Pos = pos;
                Func = func;
                Args = args;
            }
        }

        public class ForExpression : Expression
        {
            public VariableDeclaration VarDec;
            public Expression High, Body;
            public ForExpression(Position pos, VariableDeclaration varDec, Expression high, Expression body)
            {
                Pos = pos;
                VarDec = varDec;
                High = high;
                Body = body;
            }
        }

        public class IfExpression : Expression
        {
            public Expression Test;
            public Expression Then;
            public Expression Else;
            public IfExpression(Position pos, Expression test, Expression then)
            {
                Pos = pos;
                Test = test;
                Then = then;
            }
            public IfExpression(Position pos, Expression test, Expression then, Expression els)
            {
                Pos = pos;
                Test = test;
                Then = then;
                Else = els;
            }
        }

        public class IntegerExpression : Expression
        {
            public int Value;
            public IntegerExpression(Position pos, int value)
            {
                Pos = pos;
                Value = value;
            }
        }

        public class LetExpression : Expression
        {
            public DeclarationList Decs;
            public Expression Body;
            public LetExpression(Position pos, DeclarationList decs, Expression body)
            {
                Pos = pos;
                Decs = decs;
                Body = body;
            }
        }

        public class NilExpression : Expression
        {
            public NilExpression(Position pos)
            {
                Pos = pos;
            }
        }

        public class OperatorExpression : Expression
        {
            public Expression Left, Right;
            public Operator Op;
            public OperatorExpression(Position pos, Expression left, Operator op, Expression right)
            {
                Pos = pos;
                Left = left;
                Right = right;
                Op = op;
            }
        }

        public class RecordExpression : Expression
        {
            public Symbol Type;
            public FieldExpressionList Fields;
            public RecordExpression(Position pos, Symbol type, FieldExpressionList fields)
            {
                Pos = pos;
                Type = type;
                Fields = fields;
            }
        }

        public class SequenceExpression : Expression
        {
            public ExpressionList Exps;
            public SequenceExpression(Position pos, ExpressionList exps)
            {
                Pos = pos;
                Exps = exps;
            }
        }

        public class StringExpression : Expression
        {
            public string Value;
            public StringExpression(Position pos, string value)
            {
                Pos = pos;
                Value = value;
            }
        }

        public class VariableExpression : Expression
        {
            public Variable Var;
            public VariableExpression(Position pos, Variable var)
            {
                Pos = pos;
                Var = var;
            }
        }

        public class WhileExpression : Expression
        {
            public Expression Test, Body;
            public WhileExpression(Position pos, Expression test, Expression body)
            {
                Pos = pos;
                Test = test;
                Body = body;
            }
        }
    }
}