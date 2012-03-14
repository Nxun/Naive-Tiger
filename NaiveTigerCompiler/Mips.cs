using NaiveTigerCompiler.Frame;
using NaiveTigerCompiler.Tree;
using NaiveTigerCompiler.Temp;
using NaiveTigerCompiler.Utilities;
using System.Collections.Generic;
using NaiveTigerCompiler.Quadruple;
using System.Text;
namespace NaiveTigerCompiler
{
    namespace Mips
    {
        public class MipsFrame : Frame.Frame
        {
            public static string Unescape(string text)
            {
                StringBuilder sb = new StringBuilder();
                foreach (char ch in text)
                {
                    switch (ch)
                    {
                        case '\a':
                            sb.Append(@"\a");
                            break;
                        case '\b':
                            sb.Append(@"\b");
                            break;
                        case '\f':
                            sb.Append(@"\f");
                            break;
                        case '\n':
                            sb.Append(@"\n");
                            break;
                        case '\r':
                            sb.Append(@"\r");
                            break;
                        case '\t':
                            sb.Append(@"\t");
                            break;
                        case '\v':
                            sb.Append(@"\v");
                            break;
                        case '\\':
                            sb.Append(@"\\");
                            break;
                        case '\"':
                            sb.Append("\\\"");
                            break;
                        case '\'':
                            sb.Append(@"\'");
                            break;
                        default:
                            sb.Append(ch);
                            break;
                    }
                }
                return sb.ToString();
            }
            public int Offset = 0;
            public static List<MipsFrame> AllFrames = new List<MipsFrame>();
            public static int WSize = 4;
            public static Temp.Temp[] Reg = new Temp.Temp[32];
            static MipsFrame()
            {
                for (int i = 0; i < 32; ++i)
                {
                    Reg[i] = new Temp.Temp();
                }
            }
            public Temp.Temp A(int k)
            {
                if (k < 0 || k > 3)
                    throw new FatalError("Register A0-A3: out of range");
                return Reg[4 + k];
            }
            public override Temp.Temp FP()
            {
                return Reg[30];
            }
            public override Temp.Temp RA()
            {
                return Reg[31];
            }
            public override Temp.Temp RV()
            {
                return Reg[2];
            }
            public override Temp.Temp SP()
            {
                return Reg[29];
            }
            public override Access AllocLocal(bool escape)
            {
                if (escape)
                {
                    Access ret = new InFrame(this, Offset);
                    Offset -= WSize;
                    return ret;
                }
                else
                {
                    return new InReg();
                }
            }
            public override Expr ExternalCall(string func, ExpList args)
            {
                return new CALL(new NAME(new Temp.Label(func)), args);
            }
            public override Frame.Frame NewFrame(Temp.Label name, BoolList formals)
            {
                MipsFrame ret = new MipsFrame();
                ret.Name = name;
                AccessList ptr = null;
                int count = 0;
                for (BoolList f = formals; f != null; f = f.Tail)
                {
                    Access a;
                    if (count < 4 && !f.Head)
                    {
                        a = ret.AllocLocal(false);
                    }
                    else
                    {
                        a = ret.AllocLocal(true);
                    }
                    if (ret.Formals == null)
                    {
                        ptr = ret.Formals = new AccessList(a, null);
                    }
                    else
                    {
                        ptr = ptr.Tail = new AccessList(a, null);
                    }
                }
                AllFrames.Add(ret);
                return ret;
            }

            public override Stm ProcessEntryExit1(Stm body)
            {
                Temp.Temp newTemp;
                for (int i = 0; i < 8; ++i)
                {
                    newTemp = new Temp.Temp();
                    body = new SEQ(new MOVE(new TEMP(newTemp), new TEMP(Reg[16 + i])), body);
                    body = new SEQ(body, new MOVE(new TEMP(Reg[16 + i]), new TEMP(newTemp)));
                }
                newTemp = new Temp.Temp();
                body = new SEQ(new MOVE(new TEMP(newTemp), new TEMP(Reg[31])), body);
		        body = new SEQ(body, new MOVE(new TEMP(Reg[31]), new TEMP(newTemp)));

                int count = 0;
                for (AccessList ptr = Formals; ptr != null; ptr = ptr.Tail)
                {
                    if (ptr.Head is InReg)
                    {
                        body = new SEQ(new MOVE(ptr.Head.Exp(null), new TEMP(A(count))), body);
                        ++count;
                    }
                }
                for (; count < 4; count++)
                {
                    body = new SEQ(new MOVE(new TEMP(new Temp.Temp()), new TEMP(A(count))), body);
                }
                return body;

            }

            public override List<TExp> ProcessEntryExit2(List<TExp> body)
            {
                body.Add(new ReturnSink());
                return body;
            }

            public override List<TExp> ProcessEntryExit3(List<TExp> instrList)
            {
                List<TExp> prev = new List<TExp>();
                prev.Add(new Quadruple.Label(Name));
                prev.Add(new Move(FP(), SP()));
                prev.Add(new BinOpInt(BINOP.Op.Minus, SP(), SP(), -Offset));
                instrList.InsertRange(0, prev);
                instrList.Add(new Move(SP(), FP()));
                return instrList;
            }

            public override Temp.Temp[] Registers()
            {
                return Reg;
            }

            public override string String(Temp.Label label, string value)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(".data");
                sb.AppendLine(label.ToString() + ":");
                sb.Append("\t.asciiz \"");
                sb.AppendLine(Unescape(value) + "\"");
                return sb.ToString();
            }

            public override int WordSize()
            {
                return WSize;
            }

            public override string TempMap(Temp.Temp t)
            {
                return t.ToString();
            }
        }

        public class InReg : Access
        {
            public Temp.Temp Reg;
            public InReg()
            {
                Reg = new Temp.Temp();
            }
            public override Expr Exp(Expr framPtr)
            {
                return new TEMP(Reg);
            }
            public override Expr ExpFromStack(Expr stackPtr)
            {
                return new TEMP(Reg);
            }
        }

        public class InFrame : Access
        {
            public MipsFrame Frame;
            public int Offset;

            public InFrame(MipsFrame frame, int offset)
            {
                Frame = frame;
                Offset = offset;
            }
            public override Expr Exp(Expr framePtr)
            {
                return new MEM(new BINOP(BINOP.Op.Plus, framePtr, new CONST(Offset)));
            }
            public override Expr ExpFromStack(Expr stackPtr)
            {
                return new MEM(new BINOP(BINOP.Op.Plus, stackPtr, new CONST(-Frame.Offset + Offset)));
            }
        }
    }
}