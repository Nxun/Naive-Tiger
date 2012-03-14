using System.Collections.Generic;
using NaiveTigerCompiler.Tree;
using NaiveTigerCompiler.Mips;
using NaiveTigerCompiler.Temp;
using NaiveTigerCompiler.Frame;
namespace NaiveTigerCompiler
{
    namespace Quadruple
    {
        public class Quadruple
        {
            public List<TExp> InstrList = new List<TExp>();

            MipsFrame Frame;

            public Quadruple(MipsFrame frame)
            {
                Frame = frame;
            }

            public void CodeGenerate(StmList stms)
            {
                for (StmList stm = stms; stm != null; stm = stm.Tail)
                {
                    TranslateStm(stm.Head);
                }
            }

            private void TranslateStm(Stm stm)
            {
                if (stm is CJUMP)
                    TranslateStm(stm as CJUMP);
                else if (stm is EXP)
                    TranslateStm(stm as EXP);
                else if (stm is JUMP)
                    TranslateStm(stm as JUMP);
                else if (stm is LABEL)
                    TranslateStm(stm as LABEL);
                else if (stm is MOVE)
                    TranslateStm(stm as MOVE);
                else
                    throw new FatalError("Error in IR to TA: Stm");
            }

            private void TranslateStm(CJUMP stm)
            {
                if (stm.Left is CONST)
                {
                    Temp.Temp r = TranslateExpr(stm.Right);
                    if (stm.Relop == CJUMP.Rel.Equal || stm.Relop == CJUMP.Rel.NotEqual)
                        InstrList.Add(new CJumpInt(stm.Relop, r, (stm.Left as CONST).Value, new Label(stm.IfTrue)));
                    else
                        InstrList.Add(new CJumpInt(CJUMP.NotRel(stm.Relop), r, (stm.Left as CONST).Value, new Label(stm.IfTrue)));
                }
                else if (stm.Right is CONST)
                {
                    Temp.Temp l = TranslateExpr(stm.Left);
                    InstrList.Add(new CJumpInt(stm.Relop, l, (stm.Right as CONST).Value, new Label(stm.IfTrue)));
                }
                else
                {
                    Temp.Temp l = TranslateExpr(stm.Left);
                    Temp.Temp r = TranslateExpr(stm.Right);
                    InstrList.Add(new CJump(stm.Relop, l, r, new Label(stm.IfTrue)));
                }
            }

            private void TranslateStm(EXP stm)
            {
                TranslateExpr(stm.Exp);
            }

            private void TranslateStm(JUMP stm)
            {
                InstrList.Add(new Jump(new Label(stm.Targets.Head)));
            }

            private void TranslateStm(LABEL stm)
            {
                Label l = new Label(stm.Label);
                InstrList.Add(l);
            }

            private void TranslateStm(MOVE stm)
            {
                if (stm.Dst is TEMP)
                {
                    if (stm.Src is CONST)
                        InstrList.Add(new MoveInt((stm.Dst as TEMP).Temp, (stm.Src as CONST).Value));
                    else if (stm.Src is MEM)
                    {
                        Temp.Temp mem;
                        int offset;
                        if ((stm.Src as MEM).Exp is BINOP && ((stm.Src as MEM).Exp as BINOP).Right is CONST)
                        {
                            mem = TranslateExpr(((stm.Src as MEM).Exp as BINOP).Left);
                            offset = (((stm.Src as MEM).Exp as BINOP).Right as CONST).Value;
                        }
                        else
                        {
                            mem = TranslateExpr((stm.Src as MEM).Exp);
                            offset = 0;
                        }
                        InstrList.Add(new Load(mem, offset, TranslateExpr(stm.Dst)));
                    }
                    else
                    {
                        InstrList.Add(new Move((stm.Dst as TEMP).Temp, TranslateExpr(stm.Src)));
                    }

                }
                else if (stm.Dst is MEM)
                {
                    Temp.Temp mem;
                    int offset;
                    if ((stm.Dst as MEM).Exp is BINOP && ((stm.Dst as MEM).Exp as BINOP).Right is CONST)
                    {
                        mem = TranslateExpr(((stm.Dst as MEM).Exp as BINOP).Left);
                        offset = (((stm.Dst as MEM).Exp as BINOP).Right as CONST).Value;
                    }
                    else
                    {
                        mem = TranslateExpr((stm.Dst as MEM).Exp);
                        offset = 0;
                    }
                    InstrList.Add(new Store(mem, offset, TranslateExpr(stm.Src)));
                }
            }

            Temp.Temp TranslateExpr(Expr expr)
            {
                if (expr is BINOP)
                    return TranslateExpr((BINOP)expr);
                else if (expr is CALL)
                    return TranslateExpr((CALL)expr);
                else if (expr is CONST)
                    return TranslateExpr((CONST)expr);
                else if (expr is MEM)
                    return TranslateExpr((MEM)expr);
                else if (expr is NAME)
                    return TranslateExpr((NAME)expr);
                else if (expr is TEMP)
                    return TranslateExpr((TEMP)expr);
                else
                    throw new FatalError("Error in IR to TA: expr");
            }

            Temp.Temp TranslateExpr(BINOP expr)
            {
                Temp.Temp result = new Temp.Temp();
                if (expr.Right is CONST)
                    InstrList.Add(new BinOpInt(expr.Binop, result, TranslateExpr(expr.Left), (expr.Right as CONST).Value));
                else if (expr.Left is CONST)
                {
                    switch (expr.Binop)
                    {
                        case BINOP.Op.Plus:
                        case BINOP.Op.Times:
                            InstrList.Add(new BinOpInt(expr.Binop, result, TranslateExpr(expr.Right), (expr.Left as CONST).Value));
                            break;
                        case BINOP.Op.Minus:
                            InstrList.Add(new BinOpInt(expr.Binop, result, TranslateExpr(expr.Right), (expr.Left as CONST).Value));
                            InstrList.Add(new BinOp(expr.Binop, result, MipsFrame.Reg[0], result));
                            break;
                        case BINOP.Op.Divide:
                            InstrList.Add(new BinOp(expr.Binop, result, TranslateExpr(expr.Right), TranslateExpr(expr.Left)));
                            break;
                        default:
                            throw new FatalError("Error in Quadruple: TranslateExpr(BINOP)");
                            break;
                    }
                }
                else
                    InstrList.Add(new BinOp(expr.Binop, result, TranslateExpr(expr.Left), TranslateExpr(expr.Right)));
                return result;
            }

            Temp.Temp TranslateExpr(CONST expr)
            {
                if (expr.Value == 0)
                    return MipsFrame.Reg[0];
                Temp.Temp result = new Temp.Temp();
                InstrList.Add(new MoveInt(result, expr.Value));
                return result;
            }

            Temp.Temp TranslateExpr(MEM expr)
            {
                Temp.Temp result = new Temp.Temp();
                Temp.Temp mem;
                int offset;
                if (expr.Exp is BINOP && (expr.Exp as BINOP).Right is CONST)
                {
                    mem = TranslateExpr((expr.Exp as BINOP).Left);
                    offset = ((expr.Exp as BINOP).Right as CONST).Value;
                }
                else
                {
                    mem = TranslateExpr(expr.Exp);
                    offset = 0;
                }
                InstrList.Add(new Load(mem, offset, result));
                return result;
            }

            Temp.Temp TranslateExpr(NAME expr)
            {
                Temp.Temp result = new Temp.Temp();
                InstrList.Add(new MoveLabel(result, new Label(expr.Label)));
                return result;
            }

            Temp.Temp TranslateExpr(TEMP expr)
            {
                return expr.Temp;
            }

            Temp.Temp TranslateExpr(CALL expr)
            {
                Call c = new Call();
                c.Name = new Label((expr.Func as NAME).Label);
                TempList args = null, ptr = null;
                for (ExpList exp = expr.Args; exp != null; exp = exp.Tail)
                {
                    Temp.Temp arg = TranslateExpr(exp.Head);
                    if (args == null)
                        ptr = args = new TempList(arg, null);
                    else
                        ptr = ptr.Tail = new TempList(arg, null);
                }
                c.Param = args;
                MipsFrame t = null;
                foreach (MipsFrame f in MipsFrame.AllFrames)
                {
                    if (c.Name.Lab == f.Name)
                    {
                        t = f;
                        break;
                    }
                }
                if (t == null)
                {
                    int count = 0;
                    for (ptr = c.Param; ptr != null; ptr = ptr.Tail)
                    {
                        InstrList.Add(new Move(Frame.A(count), ptr.Head));
                        ++count;
                    }
                }
                else
                {
                    int count = 0;
                    ptr = c.Param;
                    for (AccessList al = t.Formals; al != null; al = al.Tail, ptr = ptr.Tail)
                    {
                        if (al.Head is InReg)
                        {
                            InstrList.Add(new Move(t.A(count), ptr.Head));
                            ++count;
                        }
                        else
                            InstrList.Add(new Store(t.SP(), (al.Head as InFrame).Offset, ptr.Head));
                    }
                }
                InstrList.Add(c);
                return Frame.RV();
            }
        }
    }
}