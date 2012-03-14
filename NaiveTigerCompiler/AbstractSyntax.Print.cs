using System;
namespace NaiveTigerCompiler
{
    namespace AbstractSyntax
    {
        public class Print
        {
            System.IO.TextWriter Out;

            public Print(System.IO.TextWriter o)
            {
                Out = o;
            }

            void Indent(int d)
            {
                for (int i = 0; i < d; ++i)
                    Out.Write("  ");
            }

            void Say(string s)
            {
                Out.Write(s);
            }

            void Say(int i)
            {
                Out.Write(i);
            }

            void Say(bool b)
            {
                Out.Write(b);
            }

            void SayLn(string s)
            {
                Out.WriteLine(s);
            }

            void PrintVariable(SimpleVariable v, int d)
            {
                Say("SimpleVariable(");
                Say(v.Name.ToString());
                Say(")");
            }

            void PrintVariable(FieldVariable v, int d)
            {
                SayLn("FieldVariable(");
                PrintVariable(v.Var, d + 1);
                SayLn(",");
                Indent(d + 1);
                Say(v.Field.ToString());
                Say(")");
            }

            void PrintVariable(SubscriptVariable v, int d)
            {
                SayLn("SubscriptVariable(");
                PrintVariable(v.Var, d + 1);
                SayLn(",");
                PrintExpression(v.Index, d + 1);
                Say(")");
            }

            void PrintVariable(Variable v, int d)
            {
                Indent(d);
                if (v is SimpleVariable) PrintVariable((SimpleVariable) v, d);
                else if (v is FieldVariable) PrintVariable((FieldVariable) v, d);
                else if (v is SubscriptVariable) PrintVariable((SubscriptVariable) v, d);
                else throw new Exception("Print.PrintVariable");
  }

            void PrintExpression(OperatorExpression e, int d)
            {
                SayLn("OperatorExpression(");
                Indent(d + 1);
                switch (e.Op)
                {
                    case Operator.Plus: Say("PLUS"); break;
                    case Operator.Minus: Say("MINUS"); break;
                    case Operator.Times: Say("MUL"); break;
                    case Operator.Divide: Say("DIV"); break;
                    case Operator.Equal: Say("EQ"); break;
                    case Operator.NotEqual: Say("NE"); break;
                    case Operator.LessThan: Say("LT"); break;
                    case Operator.LessEqual: Say("LE"); break;
                    case Operator.GreaterThan: Say("GT"); break;
                    case Operator.GreaterEqual: Say("GE"); break;
                    default:
                        throw new Exception("Print.PrintExpression.OperatorExpression");
                }
                SayLn(",");
                PrintExpression(e.Left, d + 1);
                SayLn(",");
                PrintExpression(e.Right, d + 1);
                Say(")");
            }

            void PrintExpression(VariableExpression e, int d)
            {
                SayLn("VariableExpression(");
                PrintVariable(e.Var, d + 1);
                Say(")");
            }

            void PrintExpression(NilExpression e, int d)
            {
                Say("NilExpression()");
            }

            void PrintExpression(IntegerExpression e, int d)
            {
                Say("IntegerExpression(");
                Say(e.Value);
                Say(")");
            }

            void PrintExpression(StringExpression e, int d)
            {
                Say("StringExpression(");
                Say(e.Value);
                Say(")");
            }

            void PrintExpression(CallExpression e, int d)
            {
                Say("CallExpression("); Say(e.Func.ToString()); SayLn(",");
                PrintExpressionList(e.Args, d + 1); Say(")");
            }

            void PrintExpression(RecordExpression e, int d)
            {
                Say("RecordExpression("); Say(e.Type.ToString()); SayLn(",");
                PrintFieldExpressionList(e.Fields, d + 1); Say(")");
            }

            void PrintExpression(SequenceExpression e, int d)
            {
                SayLn("SequenceExpression(");
                PrintExpressionList(e.Exps, d + 1); Say(")");
            }

            void PrintExpression(AssignExpression e, int d)
            {
                SayLn("AssignExpression(");
                PrintVariable(e.Var, d + 1); SayLn(",");
                PrintExpression(e.Exp, d + 1); Say(")");
            }

            void PrintExpression(IfExpression e, int d)
            {
                SayLn("IfExpression(");
                PrintExpression(e.Test, d + 1); SayLn(",");
                PrintExpression(e.Then, d + 1);
                if (e.Else != null)
                { /* else is optional */
                    SayLn(",");
                    PrintExpression(e.Else, d + 1);
                }
                Say(")");
            }

            void PrintExpression(WhileExpression e, int d)
            {
                SayLn("WhileExpression(");
                PrintExpression(e.Test, d + 1); SayLn(",");
                PrintExpression(e.Body, d + 1); SayLn(")");
            }

            void PrintExpression(ForExpression e, int d)
            {
                SayLn("ForExpression(");
                Indent(d + 1); PrintDeclaration(e.VarDec, d + 1); SayLn(",");
                PrintExpression(e.High, d + 1); SayLn(",");
                PrintExpression(e.Body, d + 1); Say(")");
            }

            void PrintExpression(BreakExpression e, int d)
            {
                Say("BreakExpression()");
            }

            void PrintExpression(LetExpression e, int d)
            {
                Say("LetExpression("); SayLn("");
                PrintDeclarationList(e.Decs, d + 1); SayLn(",");
                PrintExpression(e.Body, d + 1); Say(")");
            }

            void PrintExpression(ArrayExpression e, int d)
            {
                Say("ArrayExpression("); Say(e.Type.ToString()); SayLn(",");
                PrintExpression(e.Size, d + 1); SayLn(",");
                PrintExpression(e.Init, d + 1); Say(")");
            }

            /* Print Exp class types. Indent d spaces. */
            public void PrintExpression(Expression e, int d)
            {
                Indent(d);
                if (e is OperatorExpression)
                    PrintExpression((OperatorExpression)e, d);
                else if (e is VariableExpression)
                    PrintExpression((VariableExpression) e, d);
                else if (e is NilExpression)
                    PrintExpression((NilExpression) e, d);
                else if (e is IntegerExpression)
                    PrintExpression((IntegerExpression) e, d);
                else if (e is StringExpression)
                    PrintExpression((StringExpression) e, d);
                else if (e is CallExpression)
                    PrintExpression((CallExpression) e, d);
                else if (e is RecordExpression)
                    PrintExpression((RecordExpression) e, d);
                else if (e is SequenceExpression)
                    PrintExpression((SequenceExpression) e, d);
                else if (e is AssignExpression)
                    PrintExpression((AssignExpression) e, d);
                else if (e is IfExpression)
                    PrintExpression((IfExpression) e, d);
                else if (e is WhileExpression)
                    PrintExpression((WhileExpression) e, d);
                else if (e is ForExpression)
                    PrintExpression((ForExpression) e, d);
                else if (e is BreakExpression)
                    PrintExpression((BreakExpression) e, d);
                else if (e is LetExpression)
                    PrintExpression((LetExpression) e, d);
                else if (e is ArrayExpression)
                    PrintExpression((ArrayExpression) e, d);
                else throw new Exception("Print.PrintExpression");
            }

            void PrintDeclaration(FunctionDeclaration d, int i)
            {
                Say("FunctionDeclaration(");
                if (d != null)
                {
                    SayLn(d.Name.ToString());
                    PrintFieldlist(d.Param, i+1); SayLn(",");
                    if (d.Result!=null) {
                    Indent(i+1); SayLn(d.Result.Name.ToString());
                }
                PrintExpression(d.Body, i+1); SayLn(",");
                Indent(i+1); PrintDeclaration(d.Next, i+1);
                }
                Say(")");
            }

            void PrintDeclaration(VariableDeclaration d, int i)
            {
                Say("VariableDeclaration("); Say(d.Name.ToString()); SayLn(",");
                if (d.Type != null)
                {
                    Indent(i + 1); Say(d.Type.Name.ToString()); SayLn(",");
                }
                PrintExpression(d.Init, i + 1); SayLn(",");
                Indent(i + 1); Say(d.Escape); Say(")");
            }

            void PrintDeclaration(TypeDeclaration d, int i)
            {
                if (d != null)
                {
                    Say("TypeDeclaration("); Say(d.Name.ToString()); SayLn(",");
                    prTy(d.Type, i + 1);
                    if (d.Next != null)
                    {
                        Say(","); PrintDeclaration(d.Next, i + 1);
                    }
                    Say(")");
                }
            }

            void PrintDeclaration(Declaration d, int i)
            {
                Indent(i);
                if (d is FunctionDeclaration) PrintDeclaration((FunctionDeclaration) d, i);
                else if (d is VariableDeclaration) PrintDeclaration((VariableDeclaration) d, i);
                else if (d is TypeDeclaration) PrintDeclaration((TypeDeclaration) d, i);
                else throw new Exception("Print.PrintDeclaration");
            }

            void prTy(NameType t, int i)
            {
                Say("NameType("); Say(t.Name.ToString()); Say(")");
            }

            void prTy(RecordType t, int i)
            {
                SayLn("RecordType(");
                PrintFieldlist(t.Fields, i + 1); Say(")");
            }

            void prTy(ArrayType t, int i)
            {
                Say("ArrayType("); Say(t.Type.ToString()); Say(")");
            }

            void prTy(Type t, int i)
            {
                if (t!=null)
                {
                    Indent(i);
                    if (t is NameType) prTy((NameType) t, i);
                    else if (t is RecordType) prTy((RecordType) t, i);
                    else if (t is ArrayType) prTy((ArrayType) t, i);
                    else throw new Exception("Print.prTy");
                }
            }

            void PrintFieldlist(FieldList f, int d)
            {
                Indent(d);
                SayLn("Fieldlist(");

                if (f != null)
                {
                    SayLn("");
                    Indent(d + 1); Say(f.Name.ToString()); SayLn("");
                    Indent(d + 1); Say(f.Type.ToString()); SayLn(",");
                    Indent(d + 1); Say(f.Escape); SayLn(",");
                    PrintFieldlist(f.Tail, d + 1);
                }

                Say(")");
            }

            void PrintExpressionList(ExpressionList e, int d)
            {
                Indent(d);
                SayLn("ExpressionList(");
                if (e != null)
                {
                    SayLn("");
                    PrintExpression(e.Head, d + 1);
                    if (e.Tail != null)
                    {
                        SayLn(",");
                        PrintExpressionList(e.Tail, d + 1);
                    }
                }

                Say(")");
            }

            void PrintDeclarationList(DeclarationList v, int d)
            {
                Indent(d);
                SayLn("DeclarationList(");
                if (v != null)
                {
                    SayLn("");
                    PrintDeclaration(v.Head, d + 1);
                    SayLn(",");
                    PrintDeclarationList(v.Tail, d + 1);
                }

                Say(")");
            }

            void PrintFieldExpressionList(FieldExpressionList f, int d)
            {
                Indent(d);
                SayLn("FieldExpressionList(");

                if (f != null)
                {
                    SayLn("");
                    Say(f.Name.ToString());
                    SayLn(",");
                    PrintExpression(f.Init, d + 1);
                    SayLn(",");
                    PrintFieldExpressionList(f.Tail, d + 1);
                }

                Say(")");
            }
        }
    }
}