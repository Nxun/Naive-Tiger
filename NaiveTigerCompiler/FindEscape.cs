using NaiveTigerCompiler.AbstractSyntax;
using NaiveTigerCompiler.SymbolTable;
namespace NaiveTigerCompiler
{
    namespace FindEscape
    {
        public abstract class Escape
        {
            public int Depth;
            public abstract void SetEscape();
        }

        public class FormalEscape : Escape
        {
            public FieldList Fields;
            public FormalEscape(int depth, FieldList fields)
            {
                Depth = depth;
                Fields = fields;
                Fields.Escape = false;
            }

            public override void SetEscape()
            {
                Fields.Escape = true;
            }
        }

        public class VariableEscape : Escape
        {
            public VariableDeclaration VarDec;
            public VariableEscape(int depth, VariableDeclaration vd)
            {
                Depth = depth;
                VarDec = vd;
                VarDec.Escape = false;
            }
            public override void SetEscape()
            {
                VarDec.Escape = true;
            }
        }

        public class FindEscape
        {
            public Table EscapeEnv = new Table();
            public void TraverseVariable(int depth, Variable var)
            {
                if (var == null)
                    return;
                else if (var is SimpleVariable)
                {
                    Escape escape = EscapeEnv[(var as SimpleVariable).Name] as Escape;
                    if (escape == null)
                        ;
                    if (escape.Depth < depth)
                        escape.SetEscape();
                }
                else if (var is FieldVariable)
                {
                    TraverseVariable(depth, (var as FieldVariable).Var);
                }
                else if (var is SubscriptVariable)
                {
                    TraverseVariable(depth, (var as SubscriptVariable).Var);
                    TraverseExpression(depth, (var as SubscriptVariable).Index);
                }
            }

            public void TraverseExpression(int depth, Expression exp)
            {
                if (exp == null)
                    return;
                else if (exp is VariableExpression)
                    TraverseVariable(depth, (exp as VariableExpression).Var);
                else if (exp is CallExpression)
                    for (ExpressionList el = (exp as CallExpression).Args; el != null; el = el.Tail)
                        TraverseExpression(depth, el.Head);
                else if (exp is OperatorExpression)
                {
                    TraverseExpression(depth, (exp as OperatorExpression).Left);
                    TraverseExpression(depth, (exp as OperatorExpression).Right);
                }
                else if (exp is RecordExpression)
                    for (FieldExpressionList el = (exp as RecordExpression).Fields; el != null; el = el.Tail)
                        TraverseExpression(depth, el.Init);
                else if (exp is SequenceExpression)
                    for (ExpressionList el = (exp as SequenceExpression).Exps; el != null; el = el.Tail)
                        TraverseExpression(depth, el.Head);
                else if (exp is AssignExpression)
                {
                    TraverseVariable(depth, (exp as AssignExpression).Var);
                    TraverseExpression(depth, (exp as AssignExpression).Exp);
                }
                else if (exp is IfExpression)
                {
                    TraverseExpression(depth, (exp as IfExpression).Test);
                    TraverseExpression(depth, (exp as IfExpression).Then);
                    TraverseExpression(depth, (exp as IfExpression).Else);
                }
                else if (exp is WhileExpression)
                {
                    TraverseExpression(depth, (exp as WhileExpression).Test);
                    TraverseExpression(depth, (exp as WhileExpression).Body);
                }
                else if (exp is ForExpression)
                {
                    TraverseDeclaration(depth, (exp as ForExpression).VarDec);
                    TraverseExpression(depth, (exp as ForExpression).High);
                    TraverseExpression(depth, (exp as ForExpression).Body);
                }
                else if (exp is LetExpression)
                {
                    for (DeclarationList dl = (exp as LetExpression).Decs; dl != null; dl = dl.Tail)
                        TraverseDeclaration(depth, dl.Head);
                    TraverseExpression(depth, (exp as LetExpression).Body);
                }
                else if (exp is ArrayExpression)
                {
                    TraverseExpression(depth, (exp as ArrayExpression).Size);
                    TraverseExpression(depth, (exp as ArrayExpression).Init);
                }
            }

            public void TraverseDeclaration(int depth, Declaration dec)
            {
                if (dec == null)
                    return;
                else if (dec is VariableDeclaration)
                {
                    TraverseExpression(depth, (dec as VariableDeclaration).Init);
                    EscapeEnv[(dec as VariableDeclaration).Name] = new VariableEscape(depth, dec as VariableDeclaration);
                }
                else if (dec is FunctionDeclaration)
                {
                    for (FunctionDeclaration fd = dec as FunctionDeclaration; fd != null; fd = fd.Next)
                    {
                        for (FieldList f = fd.Param; f != null; f = f.Tail)
                            EscapeEnv[f.Name] = new FormalEscape(depth + 1, f);
                        TraverseExpression(depth + 1, fd.Body);
                    }
                }
            }

            public void Find(Expression e)
            {
                TraverseExpression(0, e);
            }
        }
    }
}