using NaiveTigerCompiler.Temp;
namespace NaiveTigerCompiler
{
    namespace Tree
    {
        public abstract class Expr
        {
            public abstract ExpList Kids();
            public abstract Expr Build(ExpList kids);
        }

        public class ExpList
        {
            public Expr Head;
            public ExpList Tail;
            public ExpList(Expr head, ExpList tail)
            {
                Head = head;
                Tail = tail;
            }
        }

        public abstract class Stm
        {
            public abstract ExpList Kids();
            public abstract Stm Build(ExpList kids);
        }

        public class StmList
        {
            public Stm Head;
            public StmList Tail;
            public StmList(Stm head, StmList tail)
            {
                Head = head;
                Tail = tail;
            }
        }

        public class BINOP : Expr
        {
            public BINOP.Op Binop;
            public Expr Left, Right;
            public BINOP(BINOP.Op b, Expr l, Expr r)
            {
                Binop = b;
                Left = l;
                Right = r;
            }

            public enum Op
            {
                Plus,
                Minus,
                Times,
                Divide,
                And,
                Or,
                LShift,
                RShift,
                ArShift,
                Xor
            }

            public override ExpList Kids()
            {
                return new ExpList(Left, new ExpList(Right, null));
            }

            public override Expr Build(ExpList kids)
            {
                return new BINOP(Binop, kids.Head, kids.Tail.Head);
            }
        }

        public class CALL : Expr
        {
            public Expr Func;
            public ExpList Args;
            public CALL(Expr func, ExpList args)
            {
                Func = func;
                Args = args;
            }
            public override ExpList Kids()
            {
                return new ExpList(Func, Args);
            }

            public override Expr Build(ExpList kids)
            {
                return new CALL(kids.Head, kids.Tail);
            }
        }

        public class CJUMP : Stm
        {
            public enum Rel
            {
                Equal,
                NotEqual,
                LessThan,
                LessEqual,
                GreaterThan,
                GreaterEqual,
                UnsignedLT,
                UnsignedLE,
                UnsignedGT,
                UnsignedGE
            }
            public Rel Relop;
            public Expr Left, Right;
            public Label IfTrue, IfFalse;
            public CJUMP(Rel rel, Expr left, Expr right, Label t, Label f)
            {
                Relop = rel;
                Left = left;
                Right = right;
                IfTrue = t;
                IfFalse = f;
            }

            public override ExpList Kids()
            {
                return new ExpList(Left, new ExpList(Right, null));
            }

            public override Stm Build(ExpList kids)
            {
                return new CJUMP(Relop, kids.Head, kids.Tail.Head, IfTrue, IfFalse);
            }

            public static Rel NotRel(Rel relop)
            {
                switch (relop)
                {
                    case Rel.Equal:
                        return Rel.NotEqual;
                    case Rel.NotEqual:
                        return Rel.Equal;
                    case Rel.LessThan:
                        return Rel.GreaterEqual;
                    case Rel.GreaterEqual:
                        return Rel.LessThan;
                    case Rel.GreaterThan:
                        return Rel.LessEqual;
                    case Rel.LessEqual:
                        return Rel.GreaterThan;
                    case Rel.UnsignedLT:
                        return Rel.UnsignedGE;
                    case Rel.UnsignedGE:
                        return Rel.UnsignedLT;
                    case Rel.UnsignedGT:
                        return Rel.UnsignedLE;
                    case Rel.UnsignedLE:
                        return Rel.UnsignedGT;
                    default:
                        throw new FatalError("No such relop in CJUMP.NotRel()");
                        break;
                }
                return 0;
            }
        }

        public class CONST : Expr
        {
            public int Value;
            public CONST(int value)
            {
                Value = value;
            }
            public override ExpList Kids()
            {
                return null;
            }
            public override Expr Build(ExpList kids)
            {
                return this;
            }
        }

        public class ESEQ : Expr
        {
            public Stm Stm;
            public Expr Exp;
            public ESEQ(Stm stm, Expr exp)
            {
                Stm = stm;
                Exp = exp;
            }
            public override ExpList Kids()
            {
                throw new FatalError("Kids() not applicable to ESEQ");
            }
            public override Expr Build(ExpList kids)
            {
                throw new FatalError("Build() not applicable to ESEQ");
            }
        }

        public class EXP : Stm
        {
            public Expr Exp;
            public EXP(Expr exp)
            {
                Exp = exp;
            }
            public override ExpList Kids()
            {
                return new ExpList(Exp, null);
            }
            public override Stm Build(ExpList kids)
            {
                return new EXP(kids.Head);
            }
        }

        public class JUMP : Stm
        {
            public Expr Exp;
            public LabelList Targets;
            public JUMP(Expr exp, LabelList targets)
            {
                Exp = exp;
                Targets = targets;
            }
            public JUMP(Label target) : this(new NAME(target), new LabelList(target, null))
            {

            }
            public override ExpList Kids()
            {
                return new ExpList(Exp, null);
            }
            public override Stm Build(ExpList kids)
            {
                return new JUMP(kids.Head, Targets);
            }
        }

        public class LABEL : Stm
        {
            public Label Label;
            public LABEL(Label label)
            {
                Label = label;
            }
            public override ExpList Kids()
            {
                return null;
            }
            public override Stm Build(ExpList kids)
            {
                return this;
            }
        }

        public class MEM : Expr
        {
            public Expr Exp;
            public MEM(Expr exp)
            {
                Exp = exp;
            }
            public override ExpList Kids()
            {
                return new ExpList(Exp, null);
            }
            public override Expr Build(ExpList kids)
            {
                return new MEM(kids.Head);
            }
        }

        public class MOVE : Stm
        {
            public Expr Dst, Src;
            public MOVE(Expr dst, Expr src)
            {
                Dst = dst;
                Src = src;
            }
            public override ExpList Kids()
            {
                if (Dst is MEM)
                {
                    return new ExpList((Dst as MEM).Exp, new ExpList(Src, null));
                }
                else
                {
                    return new ExpList(Src, null);
                }
            }
            public override Stm Build(ExpList kids)
            {
                if (Dst is MEM)
                {
                    return new MOVE(new MEM(kids.Head), kids.Tail.Head);
                }
                else
                {
                    return new MOVE(Dst, kids.Head);
                }
            }
        }

        public class NAME : Expr
        {
            public Label Label;
            public NAME(Label label)
            {
                Label = label;
            }
            public override ExpList Kids()
            {
                return null;
            }
            public override Expr Build(ExpList kids)
            {
                return this;
            }
        }

        public class SEQ : Stm
        {
            public Stm Left, Right;
            public SEQ(Stm left, Stm right)
            {
                Left = left;
                Right = right;
            }
            public override ExpList Kids()
            {
                throw new FatalError("Kids() not applicable to SEQ");
            }
            public override Stm Build(ExpList kids)
            {
                throw new FatalError("Build() not applicable to SEQ");
            }
        }

        public class TEMP : Expr
        {
            public Temp.Temp Temp;
            public TEMP(Temp.Temp temp)
            {
                Temp = temp;
            }
            public override ExpList Kids()
            {
                return null;
            }
            public override Expr Build(ExpList kids)
            {
                return this;
            }
        }
    }
}