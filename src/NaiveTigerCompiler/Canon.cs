using System.Collections;
namespace NaiveTigerCompiler
{
    namespace Canon
    {
        public class StmListList
        {
            public Tree.StmList Head;
            public StmListList Tail;
            public StmListList(Tree.StmList h, StmListList t) { Head = h; Tail = t; }
        }


        public class BasicBlocks
        {
            public StmListList Blocks;
            public Temp.Label Done;

            private StmListList LastBlock;
            private Tree.StmList LastStm;

            private void AddStm(Tree.Stm s)
            {
                LastStm = LastStm.Tail = new Tree.StmList(s, null);
            }

            private void DoStms(Tree.StmList l)
            {
                if (l == null)
                    DoStms(new Tree.StmList(new Tree.JUMP(Done), null));
                else if (l.Head is Tree.JUMP
                    || l.Head is Tree.CJUMP)
                {
                    AddStm(l.Head);
                    MakeBlocks(l.Tail);
                }
                else if (l.Head is Tree.LABEL)
                    DoStms(new Tree.StmList(new Tree.JUMP(((Tree.LABEL)l.Head).Label),
                            l));
                else
                {
                    AddStm(l.Head);
                    DoStms(l.Tail);
                }
            }

            public void MakeBlocks(Tree.StmList l)
            {
                if (l == null) return;
                else if (l.Head is Tree.LABEL)
                {
                    LastStm = new Tree.StmList(l.Head, null);
                    if (LastBlock == null)
                        LastBlock = Blocks = new StmListList(LastStm, null);
                    else
                        LastBlock = LastBlock.Tail = new StmListList(LastStm, null);
                    DoStms(l.Tail);
                }
                else MakeBlocks(new Tree.StmList(new Tree.LABEL(new Temp.Label()), l));
            }


            public BasicBlocks(Tree.StmList stms)
            {
                Done = new Temp.Label();
                MakeBlocks(stms);
            }
        }

        public class MoveCall : Tree.Stm
        {
            public Tree.TEMP Dst;
            public Tree.CALL Src;
            public MoveCall(Tree.TEMP d, Tree.CALL s) { Dst = d; Src = s; }
            public override Tree.ExpList Kids() { return Src.Kids(); }
            public override Tree.Stm Build(Tree.ExpList kids)
            {
                return new Tree.MOVE(Dst, Src.Build(kids));
            }
        }

        public class ExpCall : Tree.Stm
        {
            public Tree.CALL Call;
            public ExpCall(Tree.CALL c) { Call = c; }
            public override Tree.ExpList Kids() { return Call.Kids(); }
            public override Tree.Stm Build(Tree.ExpList kids)
            {
                return new Tree.EXP(Call.Build(kids));
            }
        }

        public class StmExpList
        {
            public Tree.Stm Stm;
            public Tree.ExpList Exps;
            public StmExpList(Tree.Stm s, Tree.ExpList e) { Stm = s; Exps = e; }
        }

        public class Canon
        {

            static bool IsNop(Tree.Stm a)
            {
                return a is Tree.EXP
                       && ((Tree.EXP)a).Exp is Tree.CONST;
            }

            static Tree.Stm Seq(Tree.Stm a, Tree.Stm b)
            {
                if (IsNop(a)) return b;
                else if (IsNop(b)) return a;
                else return new Tree.SEQ(a, b);
            }

            static bool Commute(Tree.Stm a, Tree.Expr b)
            {
                return IsNop(a)
                    || b is Tree.NAME
                    || b is Tree.CONST;
            }

            static Tree.Stm DoStm(Tree.SEQ s)
            {
                return Seq(DoStm(s.Left), DoStm(s.Right));
            }

            static Tree.Stm DoStm(Tree.MOVE s)
            {
                if (s.Dst is Tree.TEMP
                     && s.Src is Tree.CALL)
                    return ReorderStm(new MoveCall((Tree.TEMP)s.Dst, (Tree.CALL)s.Src));
                else if (s.Dst is Tree.ESEQ)
                    return DoStm(new Tree.SEQ(((Tree.ESEQ)s.Dst).Stm,
                                new Tree.MOVE(((Tree.ESEQ)s.Dst).Exp,
                                      s.Src)));
                else return ReorderStm(s);
            }

            static Tree.Stm DoStm(Tree.EXP s)
            {
                if (s.Exp is Tree.CALL)
                    return ReorderStm(new ExpCall((Tree.CALL)s.Exp));
                else return ReorderStm(s);
            }

            static Tree.Stm DoStm(Tree.Stm s)
            {
                if (s is Tree.SEQ) return DoStm((Tree.SEQ)s);
                else if (s is Tree.MOVE) return DoStm((Tree.MOVE)s);
                else if (s is Tree.EXP) return DoStm((Tree.EXP)s);
                else return ReorderStm(s);
            }

            static Tree.Stm ReorderStm(Tree.Stm s)
            {
                StmExpList x = Reorder(s.Kids());
                return Seq(x.Stm, s.Build(x.Exps));
            }

            static Tree.ESEQ DoExp(Tree.ESEQ e)
            {
                Tree.Stm stms = DoStm(e.Stm);
                Tree.ESEQ b = DoExp(e.Exp);
                return new Tree.ESEQ(Seq(stms, b.Stm), b.Exp);
            }

            static Tree.ESEQ DoExp(Tree.Expr e)
            {
                if (e is Tree.ESEQ) return DoExp((Tree.ESEQ)e);
                else return ReorderExp(e);
            }

            public static Tree.ESEQ ReorderExp(Tree.Expr e)
            {
                StmExpList x = Reorder(e.Kids());
                return new Tree.ESEQ(x.Stm, e.Build(x.Exps));
            }

            public static StmExpList NopNull = new StmExpList(new Tree.EXP(new Tree.CONST(0)), null);

            public static StmExpList Reorder(Tree.ExpList exps)
            {
                if (exps == null) return NopNull;
                else
                {
                    Tree.Expr a = exps.Head;
                    if (a is Tree.CALL)
                    {
                        Temp.Temp t = new Temp.Temp();
                        Tree.Expr e = new Tree.ESEQ(new Tree.MOVE(new Tree.TEMP(t), a),
                                       new Tree.TEMP(t));
                        return Reorder(new Tree.ExpList(e, exps.Tail));
                    }
                    else
                    {
                        Tree.ESEQ aa = DoExp(a);
                        StmExpList bb = Reorder(exps.Tail);
                        if (Commute(bb.Stm, aa.Exp))
                            return new StmExpList(Seq(aa.Stm, bb.Stm),
                                      new Tree.ExpList(aa.Exp, bb.Exps));
                        else
                        {
                            Temp.Temp t = new Temp.Temp();
                            return new StmExpList(
                                   Seq(aa.Stm,
                                     Seq(new Tree.MOVE(new Tree.TEMP(t), aa.Exp),
                                      bb.Stm)),
                                   new Tree.ExpList(new Tree.TEMP(t), bb.Exps));
                        }
                    }
                }
            }

            static Tree.StmList Linear(Tree.SEQ s, Tree.StmList l)
            {
                return Linear(s.Left, Linear(s.Right, l));
            }
            static Tree.StmList Linear(Tree.Stm s, Tree.StmList l)
            {
                if (s is Tree.SEQ) return Linear((Tree.SEQ)s, l);
                else return new Tree.StmList(s, l);
            }

            static public Tree.StmList Linearize(Tree.Stm s)
            {
                return Linear(DoStm(s), null);
            }
        }



        public class TraceSchedule
        {

            public Tree.StmList Stms;
            BasicBlocks Blocks;
            Hashtable Table = new Hashtable();

            Tree.StmList GetLast(Tree.StmList block)
            {
                Tree.StmList l = block;
                while (l.Tail.Tail != null) l = l.Tail;
                return l;
            }

            void Trace(Tree.StmList l)
            {
                for (; ; )
                {
                    Tree.LABEL lab = (Tree.LABEL)l.Head;
                    Table.Remove(lab.Label);
                    Tree.StmList last = GetLast(l);
                    Tree.Stm s = last.Tail.Head;
                    if (s is Tree.JUMP)
                    {
                        Tree.JUMP j = (Tree.JUMP)s;
                        Tree.StmList target = (Tree.StmList)Table[j.Targets.Head];
                        if (j.Targets.Tail == null && target != null)
                        {
                            last.Tail = target;
                            l = target;
                        }
                        else
                        {
                            last.Tail.Tail = GetNext();
                            return;
                        }
                    }
                    else if (s is Tree.CJUMP)
                    {
                        Tree.CJUMP j = (Tree.CJUMP)s;
                        Tree.StmList t = (Tree.StmList)Table[j.IfTrue];
                        Tree.StmList f = (Tree.StmList)Table[j.IfFalse];
                        if (f != null)
                        {
                            last.Tail.Tail = f;
                            l = f;
                        }
                        else if (t != null)
                        {
                            last.Tail.Head = new Tree.CJUMP(Tree.CJUMP.NotRel(j.Relop),
                                          j.Left, j.Right,
                                          j.IfFalse, j.IfTrue);
                            last.Tail.Tail = t;
                            l = t;
                        }
                        else
                        {
                            Temp.Label ff = new Temp.Label();
                            last.Tail.Head = new Tree.CJUMP(j.Relop, j.Left, j.Right,
                                          j.IfTrue, ff);
                            last.Tail.Tail = new Tree.StmList(new Tree.LABEL(ff),
                                         new Tree.StmList(new Tree.JUMP(j.IfFalse),
                                              GetNext()));
                            return;
                        }
                    }
                    else throw new FatalError("Bad basic block in TraceSchedule");
                }
            }

            Tree.StmList GetNext()
            {
                if (Blocks.Blocks == null)
                    return new Tree.StmList(new Tree.LABEL(Blocks.Done), null);
                else
                {
                    Tree.StmList s = Blocks.Blocks.Head;
                    Tree.LABEL lab = (Tree.LABEL)s.Head;
                    if (Table[lab.Label] != null)
                    {
                        Trace(s);
                        return s;
                    }
                    else
                    {
                        Blocks.Blocks = Blocks.Blocks.Tail;
                        return GetNext();
                    }
                }
            }

            public TraceSchedule(BasicBlocks b)
            {
                Blocks = b;
                for (StmListList l = b.Blocks; l != null; l = l.Tail)
                    Table[((Tree.LABEL)l.Head.Head).Label] = l.Head;
                Stms = GetNext();
                Table = null;
            }
        }

    }
}