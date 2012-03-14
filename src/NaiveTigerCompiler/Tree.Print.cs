using NaiveTigerCompiler.Temp;
namespace NaiveTigerCompiler
{
    namespace Tree
    {
        public class Print
        {
            System.IO.TextWriter Out;
            TempMap TempMap;

            public Print(System.IO.TextWriter o, TempMap t)
            {
                Out = o;
                TempMap = t;
            }

            public Print(System.IO.TextWriter o)
            {
                Out = o;
                TempMap = new DefaultMap();
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

            void SayLn(string s)
            {
                Out.WriteLine(s);
            }

            void PrintStm(SEQ s, int d)
            {
                Indent(d); SayLn("SEQ("); PrintStm(s.Left, d + 1); SayLn(",");
                PrintStm(s.Right, d + 1); Say(")");
            }

            void PrintStm(LABEL s, int d)
            {
                Indent(d); Say("LABEL "); Say(s.Label.ToString());
            }

            void PrintStm(JUMP s, int d)
            {
                Indent(d); SayLn("JUMP("); PrintExp(s.Exp, d + 1); Say(")");
            }

            void PrintStm(CJUMP s, int d)
            {
                Indent(d); Say("CJUMP(");
                switch (s.Relop)
                {
                    case CJUMP.Rel.Equal: Say("EQ"); break;
                    case CJUMP.Rel.NotEqual: Say("NE"); break;
                    case CJUMP.Rel.LessThan: Say("LT"); break;
                    case CJUMP.Rel.GreaterThan: Say("GT"); break;
                    case CJUMP.Rel.LessEqual: Say("LE"); break;
                    case CJUMP.Rel.GreaterEqual: Say("GE"); break;
                    case CJUMP.Rel.UnsignedLT: Say("ULT"); break;
                    case CJUMP.Rel.UnsignedLE: Say("ULE"); break;
                    case CJUMP.Rel.UnsignedGT: Say("UGT"); break;
                    case CJUMP.Rel.UnsignedGE: Say("UGE"); break;
                    default:
                        throw new FatalError("Print.PrintStm.CJUMP");
                }
                SayLn(","); PrintExp(s.Left, d + 1); SayLn(",");
                PrintExp(s.Right, d + 1); SayLn(",");
                Indent(d + 1); Say(s.IfTrue.ToString()); Say(",");
                Say(s.IfFalse.ToString()); Say(")");
            }

            void PrintStm(MOVE s, int d)
            {
                Indent(d); SayLn("MOVE("); PrintExp(s.Dst, d + 1); SayLn(",");
                PrintExp(s.Src, d + 1); Say(")");
            }

            void PrintStm(EXP s, int d)
            {
                Indent(d); SayLn("EXP("); PrintExp(s.Exp, d + 1); Say(")");
            }

            void PrintStm(Stm s, int d)
            {
                if (s is SEQ) PrintStm((SEQ)s, d);
                else if (s is LABEL) PrintStm((LABEL)s, d);
                else if (s is JUMP) PrintStm((JUMP)s, d);
                else if (s is CJUMP) PrintStm((CJUMP)s, d);
                else if (s is MOVE) PrintStm((MOVE)s, d);
                else if (s is EXP) PrintStm((EXP)s, d);
                else throw new FatalError("Print.PrintStm");
            }

            void PrintExp(BINOP e, int d)
            {
                Indent(d); Say("BINOP(");
                switch (e.Binop)
                {
                    case BINOP.Op.Plus: Say("PLUS"); break;
                    case BINOP.Op.Minus: Say("MINUS"); break;
                    case BINOP.Op.Times: Say("MUL"); break;
                    case BINOP.Op.Divide: Say("DIV"); break;
                    case BINOP.Op.And: Say("AND"); break;
                    case BINOP.Op.Or: Say("OR"); break;
                    case BINOP.Op.LShift: Say("LSHIFT"); break;
                    case BINOP.Op.RShift: Say("RSHIFT"); break;
                    case BINOP.Op.ArShift: Say("ARSHIFT"); break;
                    case BINOP.Op.Xor: Say("XOR"); break;
                    default:
                        throw new FatalError("Print.PrintExp.BINOP");
                }
                SayLn(",");
                PrintExp(e.Left, d + 1); SayLn(","); PrintExp(e.Right, d + 1); Say(")");
            }

            void PrintExp(MEM e, int d)
            {
                Indent(d);
                SayLn("MEM("); PrintExp(e.Exp, d + 1); Say(")");
            }

            void PrintExp(TEMP e, int d)
            {
                Indent(d); Say("TEMP ");
                Say(TempMap.TempMap(e.Temp));
            }

            void PrintExp(ESEQ e, int d)
            {
                Indent(d); SayLn("ESEQ("); PrintStm(e.Stm, d + 1); SayLn(",");
                PrintExp(e.Exp, d + 1); Say(")");

            }

            void PrintExp(NAME e, int d)
            {
                Indent(d); Say("NAME "); Say(e.Label.ToString());
            }

            void PrintExp(CONST e, int d)
            {
                Indent(d); Say("CONST "); Say(e.Value.ToString());
            }

            void PrintExp(CALL e, int d)
            {
                Indent(d); SayLn("CALL(");
                PrintExp(e.Func, d + 1);
                for (ExpList a = e.Args; a != null; a = a.Tail)
                {
                    SayLn(","); PrintExp(a.Head, d + 2);
                }
                Say(")");
            }

            void PrintExp(Expr e, int d) {
                if (e is BINOP) PrintExp((BINOP)e, d);
                else if (e is MEM) PrintExp((MEM)e, d);
                else if (e is TEMP) PrintExp((TEMP)e, d);
                else if (e is ESEQ) PrintExp((ESEQ)e, d);
                else if (e is NAME) PrintExp((NAME)e, d);
                else if (e is CONST) PrintExp((CONST)e, d);
                else if (e is CALL) PrintExp((CALL)e, d);
                else throw new FatalError("Print.PrintExp");
            }

            public void PrintStm(Stm s) { PrintStm(s, 0); Say("\n"); }
            public void PrintExp(Expr e) { PrintExp(e, 0); Say("\n"); }
        }
    }
}