using NaiveTigerCompiler.Temp;
using NaiveTigerCompiler.Utilities;
using NaiveTigerCompiler.Tree;
namespace NaiveTigerCompiler
{
    namespace Translate
    {
        public class Access
        {
            public Level Home;
            public Frame.Access Acc;
            public Access(Level home, Frame.Access access)
            {
                Home = home;
                Acc = access;
            }
        }

        public class AccessList
        {
            public Access Head;
            public AccessList Tail;
            public AccessList(Access head, AccessList tail)
            {
                Head = head;
                Tail = tail;
            }
        }

        public abstract class Cx : Exp
        {
            //public abstract Stm UnCx(Label t, Label f);

            public override Expr UnEx()
            {
                Temp.Temp r = new Temp.Temp();
                Label t = new Label();
                Label f = new Label();
                return new ESEQ(
                        new SEQ(new MOVE(new TEMP(r), new CONST(1)),
                                new SEQ(UnCx(t, f),
                                    new SEQ(new LABEL(f),
                                        new SEQ(new MOVE(new TEMP(r), new CONST(0)),
                                            new LABEL(t))))),
                        new TEMP(r));
            }

            public override Stm UnNx()
            {
                return new EXP(UnEx());
            }
        }

        public class DataFrag : Frag
        {
            public Label Label = null;
            public string Data = null;
            public DataFrag(Label label, string data)
            {
                Label = label;
                Data = data;
            }
        }

        public class Ex : Exp
        {
            public Expr Exp;
            public Ex(Expr e)
            {
                Exp = e;
            }
            public override Stm UnCx(Label t, Label f)
            {
                return new CJUMP(CJUMP.Rel.NotEqual, Exp, new CONST(0), t, f);
            }
            public override Expr UnEx()
            {
                return Exp;
            }
            public override Stm UnNx()
            {
                return new EXP(Exp);
            }
        }

        public abstract class Exp
        {
            public abstract Expr UnEx();
            public abstract Stm UnNx();
            public abstract Stm UnCx(Label t, Label f);
        }

        public class ExpList
        {
            public Exp Head;
            public ExpList Tail;
            public ExpList(Exp h, ExpList t)
            {
                Head = h;
                Tail = t;
            }
        }

        public class ForExp : Exp
        {
            Level Home;
            Access Var;
            Exp Low, High;
            Exp Body;
            Label Done;

            public ForExp(Level home, Access var, Exp low, Exp high, Exp body, Label done)
            {
                Home = home;
                Var = var;
                Low = low;
                High = high;
                Body = body;
                Done = done;
            }

            public override Stm UnCx(Label t, Label f)
            {
                return null;
            }

            public override Expr UnEx()
            {
                return null;
            }

            public override Stm UnNx()
            {
                Access limit = Home.AllocLocal(false);
                Label start = new Label();
                return new SEQ(new MOVE(Var.Acc.Exp(new TEMP(Home.Frame.FP())), Low.UnEx()),
                        new SEQ(new MOVE(limit.Acc.Exp(new TEMP(Home.Frame.FP())), High.UnEx()),
                            new SEQ(new CJUMP(CJUMP.Rel.LessEqual, Var.Acc.Exp(new TEMP(Home.Frame.FP())), limit.Acc.Exp(new TEMP(Home.Frame.FP())), start, Done),
                                new SEQ(new LABEL(start),
                                    new SEQ(Body.UnNx(),
                                        new SEQ(new MOVE(Var.Acc.Exp(new TEMP(Home.Frame.FP())), new BINOP(BINOP.Op.Plus, Var.Acc.Exp(new TEMP(Home.Frame.FP())), new CONST(1))),
                                            new SEQ(new CJUMP(CJUMP.Rel.LessEqual, Var.Acc.Exp(new TEMP(Home.Frame.FP())), limit.Acc.Exp(new TEMP(Home.Frame.FP())), start, Done),
                                                new LABEL(Done))))))));

            }
        }

        public class Frag
        {
            public Frag Next = null;
        }

        public class IfThenElseExp : Exp
        {
            public Exp Test;
            public Exp Then, Else;

            public IfThenElseExp(Exp t, Exp e1, Exp e2)
            {
                Test = t;
                Then = e1;
                Else = e2;
            }

            public override Stm UnCx(Label tt, Label ff)
            {
                Label t = new Label();
                Label f = new Label();
                return new SEQ(Test.UnCx(t, f),
                        new SEQ(new LABEL(t),
                            new SEQ(Then.UnCx(tt, ff),
                                new SEQ(new LABEL(f),
                                    Else.UnCx(tt, ff)))));
            }

            public override Expr UnEx()
            {
                Temp.Temp r = new Temp.Temp();
                Label join = new Label();
                Label t = new Label();
                Label f = new Label();
                return new ESEQ(new SEQ(Test.UnCx(t, f),
                        new SEQ(new LABEL(t),
                            new SEQ(new MOVE(new TEMP(r), Then.UnEx()),
                                new SEQ(new JUMP(join),
                                    new SEQ(new LABEL(f),
                                        new SEQ(new MOVE(new TEMP(r), Else.UnEx()),
                                            new LABEL(join))))))),
                                            new TEMP(r));
            }

            public override Stm UnNx()
            {
                Label join = new Label();
                Label t = new Label();
                Label f = new Label();
                if (Else == null)
                    return new SEQ(Test.UnCx(t, join),
                            new SEQ(new LABEL(t),
                                new SEQ(Then.UnNx(),
                                    new LABEL(join))));
                else
                    return new SEQ(Test.UnCx(t, f),
                            new SEQ(new LABEL(t),
                                new SEQ(Then.UnNx(),
                                    new SEQ(new JUMP(join),
                                        new SEQ(new LABEL(f),
                                            new SEQ(Else.UnNx(),
                                                new LABEL(join)))))));
            }
        }

        public class IntExp : Exp
        {
            public int Value;
            public IntExp(int v)
            {
                Value = v;
            }
            public override Stm UnCx(Label t, Label f)
            {
                if (Value != 0)
                    return new JUMP(t);
                else
                    return new JUMP(f);
            }
            public override Expr UnEx()
            {
                return new CONST(Value);
            }
            public override Stm UnNx()
            {
                return new EXP(new CONST(Value));
            }
        }

        public class Level
        {
            public Frame.Frame Frame;
            public Level Parent;
            public AccessList Formals;

            public Access StaticLink()
            {
                return Formals.Head;
            }

            public Level(Level p, Label n, BoolList f)
            {
                Parent = p;
                Frame = p.Frame.NewFrame(n, new BoolList(true, f));
                Formals = null;
                AccessList ptr = null;
                for (Frame.AccessList al = Frame.Formals; al != null; al = al.Tail)
                {
                    if (Formals == null)
                        ptr = Formals = new AccessList(new Access(this, al.Head), null);
                    else
                        ptr = ptr.Tail = new AccessList(new Access(this, al.Head), null);
                }
            }

            public Level(Level p, Label n, BoolList f, bool std)
            {
                Parent = p;
                Frame = p.Frame.NewFrame(n, f);
                Formals = null;
                AccessList ptr = null;
                for (Frame.AccessList al = Frame.Formals; al != null; al = al.Tail)
                {
                    if (Formals == null)
                        ptr = Formals = new AccessList(new Access(this, al.Head), null);
                    else
                        ptr = ptr.Tail = new AccessList(new Access(this, al.Head), null);
                }
            }

            public Level(Frame.Frame f)
            {
                Frame = f;
            }

            public Access AllocLocal(bool escape)
            {
                return new Access(this, Frame.AllocLocal(escape));
            }
        }

        public class Nx : Exp
        {
            public Stm Stm;
            public Nx(Stm stm)
            {
                Stm = stm;
            }
            public override Stm UnCx(Label t, Label f)
            {
                return null;
            }
            public override Expr UnEx()
            {
                return null;
            }
            public override Stm UnNx()
            {
                return Stm;
            }
        }

        public class ProcFrag : Frag
        {
            public Stm Body = null;
            public Frame.Frame Frame = null;
            public ProcFrag(Stm b, Frame.Frame f)
            {
                Body = b;
                Frame = f;
            }
        }

        public class RelCx : Cx
        {
            public CJUMP.Rel Relop;
            public Exp Left = null;
            public Exp Right = null;
            public RelCx(CJUMP.Rel rel, Exp left, Exp right)
            {
                Relop = rel;
                Left = left;
                Right = right;
            }
            public override Stm UnCx(Label t, Label f)
            {
                return new CJUMP(Relop, Left.UnEx(), Right.UnEx(), t, f);
            }
        }

        public class WhileExp : Exp
        {
            public Exp Test;
            public Exp Body;
            public Label Done;
            public WhileExp(Exp test, Exp body, Label done)
            {
                Test = test;
                Body = body;
                Done = done;
            }
            public override Stm UnCx(Label t, Label f)
            {
                return null;
            }
            public override Expr UnEx()
            {
                return null;
            }
            public override Stm UnNx()
            {
                Label start = new Label();
                return new SEQ(Test.UnCx(start, Done),
                        new SEQ(new LABEL(start),
                            new SEQ(Body.UnNx(),
                                new SEQ(Test.UnCx(start, Done),
                                    new LABEL(Done)))));
            }
        }
    }
}