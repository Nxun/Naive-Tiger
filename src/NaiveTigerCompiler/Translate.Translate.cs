using NaiveTigerCompiler.Tree;
using NaiveTigerCompiler.Temp;
using System.Collections.Generic;
namespace NaiveTigerCompiler
{
    namespace Translate
    {
        public class Translate
        {
            public Frame.Frame Frame = null;
            public Frag Frags = null;

            public Translate(Frame.Frame frame)
            {
                Frame = frame;
            }

            public Frag GetResult()
            {
                return Frags;
            }

            public void AddFrag(Frag frag)
            {
                frag.Next = Frags;
                Frags = frag;
            }

            public Exp TranslateIntExp(int Value)
            {
                return new IntExp(Value);
            }

            public Exp TranslateNilExp()
            {
                return new Ex(new CONST(0));
            }

            public Exp TranslateStringExp(string Value)
            {
                Label label = new Label();
                AddFrag(new DataFrag(label, Frame.String(label, Value)));
                return new Ex(new NAME(label));
            }

            public Exp TranslateVariableExp(Exp e)
            {
                return e;
            }

            public Exp TranslateCalculateExp(BINOP.Op op, Exp left, Exp right)
            {
                return new Ex(new BINOP(op, left.UnEx(), right.UnEx()));
            }

            public Exp TranslateStringRelExp(CJUMP.Rel op, Exp left, Exp right)
            {
                Expr comp = Frame.ExternalCall("_strcmp", new Tree.ExpList(left.UnEx(), new Tree.ExpList(right.UnEx(), null)));
                return new RelCx(op, new Ex(comp), new Ex(new CONST(0)));
            }

            public Exp TranslateRelExp(CJUMP.Rel op, Exp left, Exp right)
            {
                return new RelCx(op, left, right);
            }

            public Exp TranslateAssignExp(Exp lvalue, Exp e)
            {
                if (lvalue == null)
                    return null;
                return new Nx(new MOVE(lvalue.UnEx(), e.UnEx()));
            }

            public Exp TranslateCallExp(Level home, Level dst, Label name, List<Exp> argValue)
            {
                Tree.ExpList args = null;
                for (int i = argValue.Count - 1; i >= 0; --i)
                {
                    args = new Tree.ExpList(argValue[i].UnEx(), args);
                }
                Level lv = home;
                Expr staticLink = new TEMP(lv.Frame.FP());
                while (dst.Parent != lv)
                {
                    staticLink = lv.StaticLink().Acc.Exp(staticLink);
                    lv = lv.Parent;
                }
                if (!name.Name.StartsWith("_"))
                    args = new Tree.ExpList(staticLink, args);
                return new Ex(new CALL(new NAME(name), args));
            }

            public Exp TranslateRecordExp(Level home, List<Exp> field)
            {
                Temp.Temp addr = new Temp.Temp();
                Expr alloc = home.Frame.ExternalCall("_record",
                    new Tree.ExpList(new CONST(
                        (field.Count == 0 ? 1 : field.Count) * home.Frame.WordSize()), null));
                Stm init = new EXP(new CONST(0));
                for (int i = field.Count - 1; i >= 0; --i)
                {
                    Expr offset = new BINOP(BINOP.Op.Plus, new TEMP(addr), new CONST(i * home.Frame.WordSize()));
                    Expr v = field[i].UnEx();
                    init = new SEQ(new MOVE(new MEM(offset), v), init);
                }
                return new Ex(new ESEQ(new SEQ(new MOVE(new TEMP(addr), alloc), init), new TEMP(addr)));
            }

            public Exp TranslateArrayExp(Level home, Exp init, Exp size)
            {
                Expr alloc = home.Frame.ExternalCall("_array", new Tree.ExpList(size.UnEx(), new Tree.ExpList(init.UnEx(), null)));
                return new Ex(alloc);
            }

            public Exp TranslateMultiArrayExp(Level home, Exp init, Exp size)
            {
                Expr alloc = home.Frame.ExternalCall("_malloc",
                    new Tree.ExpList(
                        new BINOP(BINOP.Op.Times, size.UnEx(), new CONST(Frame.WordSize())),
                        null));
                Temp.Temp addr = new Temp.Temp();
                Access var = home.AllocLocal(false);
                Stm initial = (new ForExp(home, var, new Ex(new CONST(0)),
                    new Ex(new BINOP(BINOP.Op.Minus, size.UnEx(), new CONST(1))),
                    new Nx(new MOVE(new MEM(new BINOP(BINOP.Op.Plus, new TEMP(addr), new BINOP(BINOP.Op.Times, var.Acc.Exp(null), new CONST(Frame.WordSize())))),
                        init.UnEx())),
                        new Label())).UnNx();
                return new Ex(new ESEQ(new SEQ(new MOVE(new TEMP(addr), alloc), initial), new TEMP(addr)));
            }

            public Exp TranslateIfThenElseExp(Exp test, Exp t, Exp e)
            {
                return new IfThenElseExp(test, t, e);
            }

            public Exp TranslateWhileExp(Exp test, Exp body, Label done)
            {
                return new WhileExp(test, body, done);
            }

            public Exp TranslateForExp(Level home, Access var, Exp low, Exp high, Exp body, Label done)
            {
                return new ForExp(home, var, low, high, body, done);
            }

            public Exp TranslateBreakExp(Label done)
            {
                return new Nx(new JUMP(done));
            }

            public Exp TranslateSimpleVar(Access var, Level home)
            {
                Expr res = new TEMP(home.Frame.FP());
                Level l = home;
                while (l != var.Home)
                {
                    res = l.StaticLink().Acc.Exp(res);
                    l = l.Parent;
                }
                return new Ex(var.Acc.Exp(res));
            }

            public Exp TranslateSubscriptVar(Exp var, Exp index)
            {
                Expr array_addr = var.UnEx();
                Expr array_offset;
                if (index.UnEx() is CONST)
                    array_offset = new CONST(((CONST)index.UnEx()).Value * Frame.WordSize());
                else
                    array_offset = new BINOP(BINOP.Op.Times, index.UnEx(), new CONST(Frame.WordSize()));
                return new Ex(new MEM(new BINOP(BINOP.Op.Plus, array_addr, array_offset)));
            }

            public Exp TranslateFieldVar(Exp var, int num)
            {
                Expr addr = var.UnEx();
                Expr offset = new CONST(num * Frame.WordSize());
                return new Ex(new MEM(new BINOP(BINOP.Op.Plus, addr, offset)));
            }

            public Exp TranslateSeqExp(ExpList el, bool isVOID)
            {
                if (el == null) return new Ex(new CONST(0));
                if (el.Tail == null) return el.Head;
                if (el.Tail.Tail == null)
                    if (isVOID)
                        return new Nx(new SEQ(el.Head.UnNx(), el.Tail.Head.UnNx()));
                    else
                        return new Ex(new ESEQ(el.Head.UnNx(), el.Tail.Head.UnEx()));
                ExpList ptr = el.Tail, prev = el;
                SEQ res = null;
                for (; ptr.Tail != null; ptr = ptr.Tail)
                {
                    if (res == null)
                        res = new SEQ(prev.Head.UnNx(), ptr.Head.UnNx());
                    else
                        res = new SEQ(res, ptr.Head.UnNx());
                }
                if (isVOID)
                    return new Nx(new SEQ(res, ptr.Head.UnNx()));
                else
                    return new Ex(new ESEQ(res, ptr.Head.UnEx()));
            }

            public Exp TranslateLetExp(ExpList eDec, Exp body, bool isVOID)
            {
                if (isVOID)
                    return new Nx(new SEQ(TranslateSeqExp(eDec, true).UnNx(), body.UnNx()));
                else
                    return new Ex(new ESEQ(TranslateSeqExp(eDec, true).UnNx(), body.UnEx()));
            }

            public Exp TranslateNoOp()
            {
                return new Ex(new CONST(0));
            }

            public void ProcessEntryExit(Level level, Exp body, bool returnValue)
            {
                Stm stm = null;
                if (returnValue)
                    stm = new MOVE(new TEMP(level.Frame.RV()), body.UnEx());
                else
                    stm = body.UnNx();
                stm = level.Frame.ProcessEntryExit1(stm);
                AddFrag(new ProcFrag(stm, level.Frame));
            }
        }
    }
}