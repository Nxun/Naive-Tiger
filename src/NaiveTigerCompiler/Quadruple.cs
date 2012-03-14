using NaiveTigerCompiler.ReachingDefinition;
using NaiveTigerCompiler.Liveness;
using NaiveTigerCompiler.Tree;
using NaiveTigerCompiler.Temp;
namespace NaiveTigerCompiler
{
    namespace Quadruple
    {
        public abstract class TExp
        {
            public LivenessNode LivenessNode = null;
            public Node ReachingDefinitionNode = null;
            public bool Mark;
            public abstract void ReplaceUse(Temp.Temp oldTemp, Temp.Temp newTemp);
            public abstract void ReplaceDef(Temp.Temp oldTemp, Temp.Temp newTemp);
        }

        public class BinOp : TExp
        {
            public BINOP.Op Op;
            public Temp.Temp Left;
            public Temp.Temp Right;
            public Temp.Temp Dst;

            public BinOp(BINOP.Op op, Temp.Temp dst, Temp.Temp left, Temp.Temp right)
            {
                Op = op;
                Dst = dst;
                Left = left;
                Right = right;
            }

            public override void ReplaceDef(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                if (Dst == oldTemp)
                    Dst = newTemp;
            }

            public override void ReplaceUse(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                if (Left == oldTemp)
                    Left = newTemp;
                if (Right == oldTemp)
                    Right = newTemp;
            }
        }

        public class BinOpInt : TExp
        {
            public BINOP.Op Op;
            public Temp.Temp Left;
            public int Right;
            public Temp.Temp Dst;

            public BinOpInt(BINOP.Op op, Temp.Temp dst, Temp.Temp left, int right)
            {
                Op = op;
                Dst = dst;
                Left = left;
                Right = right;
            }

            public override void ReplaceDef(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                if (Dst == oldTemp)
                    Dst = newTemp;
            }

            public override void ReplaceUse(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                if (Left == oldTemp)
                    Left = newTemp;
            }
        }

        public class Call : TExp
        {
            public Label Name;
            public TempList Param;
            public override void ReplaceDef(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                throw new FatalError("No ReplaceDef method for Call");
            }
            public override void ReplaceUse(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                throw new FatalError("No ReplaceUse method for Call");
            }
        }

        public class CJump : TExp
        {
            public CJUMP.Rel Relop;
            public Temp.Temp Left;
            public Temp.Temp Right;
            public Label Label;

            public CJump(CJUMP.Rel rel, Temp.Temp left, Temp.Temp right, Label label)
            {
                Relop = rel;
                Left = left;
                Right = right;
                Label = label;
            }

            public override void ReplaceDef(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                throw new FatalError("No ReplaceDef method for CJump");
            }
            public override void ReplaceUse(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                if (Left == oldTemp)
                    Left = newTemp;
                if (Right == oldTemp)
                    Right = newTemp;
            }
        }

        public class CJumpInt : TExp
        {
            public CJUMP.Rel Relop;
            public Temp.Temp Left;
            public int Right;
            public Label Label;

            public CJumpInt(CJUMP.Rel rel, Temp.Temp left, int right, Label label)
            {
                Relop = rel;
                Left = left;
                Right = right;
                Label = label;
            }
            public override void ReplaceDef(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                throw new FatalError("No ReplaceDef method for CJumpInt");
            }
            public override void ReplaceUse(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                if (Left == oldTemp)
                    Left = newTemp;
            }
        }

        public class Jump :TExp
        {
            public Label Label;
            public Jump(Label label)
            {
                Label = label;
            }
            public override void ReplaceDef(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                throw new FatalError("No ReplaceDef method for Jump");
            }
            public override void ReplaceUse(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                throw new FatalError("No ReplaceUse method for Jump");
            }
        }

        public class Label : TExp
        {
            public Temp.Label Lab;
            public Label(Temp.Label label)
            {
                Lab = label;
            }
            public override void ReplaceDef(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                throw new FatalError("No ReplaceDef method for Label");
            }
            public override void ReplaceUse(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                throw new FatalError("No ReplaceUse method for Label");
            }
        }

        public class Load : TExp
        {
            public Temp.Temp Mem;
            public int Offset;
            public Temp.Temp Dst;
            public Load(Temp.Temp mem, int offset, Temp.Temp dst)
            {
                Mem = mem;
                Offset = offset;
                Dst = dst;
            }
            public override void ReplaceDef(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                if (Dst == oldTemp)
                    Dst = newTemp;
            }

            public override void ReplaceUse(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                if (Mem == oldTemp)
                    Mem = newTemp;
            }
        }

        public class Move : TExp
        {
            public Temp.Temp Dst;
            public Temp.Temp Src;
            public Move(Temp.Temp dst, Temp.Temp src)
            {
                Dst = dst;
                Src = src;
            }
            public override void ReplaceDef(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                if (Dst == oldTemp)
                    Dst = newTemp;
            }

            public override void ReplaceUse(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                if (Src == oldTemp)
                    Src = newTemp;
            }
        }

        public class MoveInt : TExp
        {
            public Temp.Temp Dst;
            public int Src;
            public MoveInt(Temp.Temp dst, int src)
            {
                Dst = dst;
                Src = src;
            }
            public override void ReplaceDef(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                if (Dst == oldTemp)
                    Dst = newTemp;
            }

            public override void ReplaceUse(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                throw new FatalError("No ReplaceUse method for MoveInt");
            }
        }

        public class MoveLabel :TExp
        {
            public Temp.Temp Dst;
            public Label Src;
            public MoveLabel(Temp.Temp dst, Label src)
            {
                Dst = dst;
                Src = src;
            }
            public override void ReplaceDef(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                if (Dst == oldTemp)
                    Dst = newTemp;
            }

            public override void ReplaceUse(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                throw new FatalError("No ReplaceUse method for MoveLabel");
            }
        }

        public class ReturnSink : TExp
        {
            public override void ReplaceDef(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                throw new FatalError("No ReplaceDef method for ReturnSink");
            }
            public override void ReplaceUse(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                throw new FatalError("No ReplaceUse method for ReturnSink");
            }
        }

        public class Store : TExp
        {
            public Temp.Temp Mem;
            public int Offset;
            public Temp.Temp Src;
            public Store(Temp.Temp mem, int offset, Temp.Temp src)
            {
                Mem = mem;
                Offset = offset;
                Src = src;
            }
            public override void ReplaceDef(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                throw new FatalError("No ReplaceDef method for Store");
            }
            public override void ReplaceUse(Temp.Temp oldTemp, Temp.Temp newTemp)
            {
                if (Mem == oldTemp)
                    Mem = newTemp;
                if (Src == oldTemp)
                    Src = newTemp;
            }
        }
    }
}