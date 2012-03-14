using System.Collections.Generic;
using NaiveTigerCompiler.Mips;
using NaiveTigerCompiler.Quadruple;
using NaiveTigerCompiler.RegisterAllocate;
using NaiveTigerCompiler.Tree;
namespace NaiveTigerCompiler
{
    namespace CodeGenerate
    {
        public class CodeGen
        {
            List<TExp> InstrList;
            Dictionary<Temp.Temp, Node> TempToNode;
            System.IO.TextWriter Out;
            private MipsFrame Frame;
            public CodeGen(List<TExp> instrList, Dictionary<Temp.Temp, Node> temp2Node, MipsFrame frame)
            {
                this.InstrList = instrList;
                this.TempToNode = temp2Node;
                this.Frame = frame;
            }

            public void CodeGenerate(System.IO.TextWriter o)
            {
                Out = o;
                Out.WriteLine(".text");
                foreach (TExp e in InstrList)
                    Write(e);
                if (Frame.Name.Name != "main")
                    Out.WriteLine("	jr $ra");
                else
                    Out.WriteLine("	j _exit");
            }

            private string GetColor(Temp.Temp t)
            {
                string[] map = {"zero", "at", "v0", "v1",
				"a0", "a1", "a2", "a3", "t0", "t1",
				"t2", "t3", "t4", "t5", "t6", "t7",
				"s0", "s1", "s2", "s3", "s4", "s5",
				"s6", "s7", "t8", "t9", "k0", "k1",
				"gp", "sp", "fp", "ra"};
                string res = "$" + map[TempToNode[t].Color];
                return res;
            }

            void Write(TExp exp)
            {
                if (exp is BinOp)
                    Write((BinOp)exp);
                else if (exp is BinOpInt)
                    Write((BinOpInt)exp);
                else if (exp is Call)
                    Write((Call)exp);
                else if (exp is CJump)
                    Write((CJump)exp);
                else if (exp is CJumpInt)
                    Write((CJumpInt)exp);
                else if (exp is Jump)
                    Write((Jump)exp);
                else if (exp is Label)
                    Write((Label)exp);
                else if (exp is Load)
                    Write((Load)exp);
                else if (exp is Move)
                    Write((Move)exp);
                else if (exp is MoveInt)
                    Write((MoveInt)exp);
                else if (exp is MoveLabel)
                    Write((MoveLabel)exp);
                else if (exp is ReturnSink)
                    Write((ReturnSink)exp);
                else if (exp is Store)
                    Write((Store)exp);
            }

            void Write(BinOp exp)
            {
                Out.Write('\t');
                switch (exp.Op)
                {
                    case Tree.BINOP.Op.Plus: Out.Write("add"); break;
                    case Tree.BINOP.Op.Minus: Out.Write("sub"); break;
                    case Tree.BINOP.Op.Times: Out.Write("mul"); break;
                    case Tree.BINOP.Op.Divide: Out.Write("div"); break;
                    default: Out.Write(exp.Op);
                        break;
                }
                Out.WriteLine(' ' + GetColor(exp.Dst) + ", " + GetColor(exp.Left) + ", " + GetColor(exp.Right));
            }

            void Write(BinOpInt exp)
            {
                Out.Write('\t');
                switch (exp.Op)
                {
                    case BINOP.Op.Plus:
                        Out.Write("add");
                        Out.WriteLine(' ' + GetColor(exp.Dst) + ", " + GetColor(exp.Left) + ", " + exp.Right);
                        break;
                    case BINOP.Op.Minus:
                        Out.Write("sub");
                        Out.WriteLine(' ' + GetColor(exp.Dst) + ", " + GetColor(exp.Left) + ", " + exp.Right);
                        break;
                    case BINOP.Op.Times:
                        if ((exp.Right & (exp.Right - 1)) == 0)
                        {
                            Out.Write("sll");
                            Out.WriteLine(' ' + GetColor(exp.Dst) + ", " + GetColor(exp.Left) + ", " + Log2(exp.Right));
                        }
                        else
                        {
                            Out.Write("mul");
                            Out.WriteLine(' ' + GetColor(exp.Dst) + ", " + GetColor(exp.Left) + ", " + exp.Right);
                        }
                        break;
                    case BINOP.Op.Divide:
                        if ((exp.Right & (exp.Right - 1)) == 0)
                        {
                            Out.Write("sra");
                            Out.WriteLine(' ' + GetColor(exp.Dst) + ", " + GetColor(exp.Left) + ", " + Log2(exp.Right));
                        }
                        else
                        {
                            Out.Write("div");
                            Out.WriteLine(' ' + GetColor(exp.Dst) + ", " + GetColor(exp.Left) + ", " + exp.Right);
                        }
                        break;
                    default: throw new FatalError("Error at BinOpI_R in Codegen");
                }
            }

            private int Log2(int right)
            {
                int res = 0;
                while (1 << res < right)
                    res++;
                return res;
            }

            void Write(Call exp)
            {
                Out.Write('\t');
                Out.WriteLine("jal " + exp.Name.Lab);
                Out.Write('\t');
                Out.WriteLine("add $fp, $sp, " + -Frame.Offset);
            }

            void Write(CJump exp)
            {
                Out.Write('\t');
                switch (exp.Relop)
                {
                    case CJUMP.Rel.Equal: Out.Write("beq"); break;
                    case CJUMP.Rel.NotEqual: Out.Write("bne"); break;
                    case CJUMP.Rel.LessThan: Out.Write("blt"); break;
                    case CJUMP.Rel.GreaterThan: Out.Write("bgt"); break;
                    case CJUMP.Rel.LessEqual: Out.Write("ble"); break;
                    case CJUMP.Rel.GreaterEqual: Out.Write("bge"); break;
                    default: throw new FatalError("Error at CJump in Codegen " + exp.Relop);
                }
                Out.WriteLine(' ' + GetColor(exp.Left) + ", " + GetColor(exp.Right) + ", " + exp.Label.Lab);
            }

            void Write(CJumpInt exp)
            {
                Out.Write('\t');
                switch (exp.Relop)
                {
                    case CJUMP.Rel.Equal: Out.Write("beq"); break;
                    case CJUMP.Rel.NotEqual: Out.Write("bne"); break;
                    case CJUMP.Rel.LessThan: Out.Write("blt"); break;
                    case CJUMP.Rel.GreaterThan: Out.Write("bgt"); break;
                    case CJUMP.Rel.LessEqual: Out.Write("ble"); break;
                    case CJUMP.Rel.GreaterEqual: Out.Write("bge"); break;
                    default: throw new FatalError("Error at CJumpI in Codegen");
                }
                Out.WriteLine(' ' + GetColor(exp.Left) + ", " + exp.Right + ", " + exp.Label.Lab);
            }

            void Write(Jump exp)
            {
                Out.Write('\t');
                Out.WriteLine("j " + exp.Label.Lab);
            }

            void Write(Label exp)
            {
                Out.WriteLine(exp.Lab + ":");
            }

            void Write(Load exp)
            {
                Out.Write('\t');
                Out.WriteLine("lw " + GetColor(exp.Dst) + ", " + exp.Offset + '(' + GetColor(exp.Mem) + ')');
            }

            void Write(Move exp)
            {
                if (!GetColor(exp.Dst).Equals(GetColor(exp.Src)))
                {
                    Out.Write('\t');
                    Out.WriteLine("move " + GetColor(exp.Dst) + ", " + GetColor(exp.Src));
                }
            }

            void Write(MoveInt exp)
            {
                Out.Write('\t');
                Out.WriteLine("li " + GetColor(exp.Dst) + ", " + exp.Src);
            }

            void Write(MoveLabel exp)
            {
                Out.Write('\t');
                Out.WriteLine("la " + GetColor(exp.Dst) + ", " + exp.Src.Lab);
            }

            void Write(ReturnSink exp)
            {

            }

            void Write(Store exp)
            {
                Out.Write('\t');
                Out.WriteLine("sw " + GetColor(exp.Src) + ", " + exp.Offset + '(' + GetColor(exp.Mem) + ')');
            }
        }
    }
}