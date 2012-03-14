using System.Collections.Generic;
using NaiveTigerCompiler.Block;
using NaiveTigerCompiler.Quadruple;
using NaiveTigerCompiler.Mips;
namespace NaiveTigerCompiler
{
    namespace Liveness
    {
        public class LivenessNode
        {
            public HashSet<Temp.Temp> Use = new HashSet<Temp.Temp>();
            public HashSet<Temp.Temp> Def = new HashSet<Temp.Temp>();

            public LivenessNode(TExp k)
            {
                if (k is BinOp)
                {
                    Use.Add((k as BinOp).Left);
                    Use.Add((k as BinOp).Right);
                    Def.Add((k as BinOp).Dst);
                }
                else if (k is BinOpInt)
                {
                    Use.Add(((BinOpInt)k).Left);
                    Def.Add(((BinOpInt)k).Dst);
                }
                else if (k is Call)
                {
                    //Use: a0-a3
                    for (int i = 0; i < 4; i++)
                        Use.Add(MipsFrame.Reg[4 + i]);
                    //define: v0, v1
                    Def.Add(MipsFrame.Reg[2]);
                    Def.Add(MipsFrame.Reg[3]);
                    //define: a0-a3
                    for (int i = 0; i < 4; i++)
                        Def.Add(MipsFrame.Reg[4 + i]);
                    //define: t0-t9
                    for (int i = 0; i < 8; i++)
                        Def.Add(MipsFrame.Reg[8 + i]);
                    Def.Add(MipsFrame.Reg[24]);
                    Def.Add(MipsFrame.Reg[25]);
                    //define: ra
                    Def.Add(MipsFrame.Reg[31]);
                    //define: gp
                    Def.Add(MipsFrame.Reg[28]);
                }
                else if (k is CJump)
                {
                    Use.Add(((CJump)k).Left);
                    Use.Add(((CJump)k).Right);
                }
                else if (k is CJumpInt)
                {
                    Use.Add(((CJumpInt)k).Left);
                }
                else if (k is Jump)
                {

                }
                else if (k is Label)
                {

                }
                else if (k is Load)
                {
                    Use.Add(((Load)k).Mem);
                    Def.Add(((Load)k).Dst);
                }
                else if (k is Move)
                {
                    Use.Add(((Move)k).Src);
                    Def.Add(((Move)k).Dst);
                }
                else if (k is MoveInt)
                {
                    Def.Add(((MoveInt)k).Dst);
                }
                else if (k is MoveLabel)
                {
                    Def.Add(((MoveLabel)k).Dst);
                }
                else if (k is ReturnSink)
                {
                    //v0
                    Use.Add(MipsFrame.Reg[2]);
                    //sp
                    Use.Add(MipsFrame.Reg[29]);
                    //fp
                    Use.Add(MipsFrame.Reg[30]);
                    //ra
                    Use.Add(MipsFrame.Reg[31]);
                    //s0-s7
                    for (int i = 0; i < 8; i++)
                        Use.Add(MipsFrame.Reg[16 + i]);
                }
                else if (k is Store)
                {
                    Use.Add(((Store)k).Mem);
                    Use.Add(((Store)k).Src);
                }
            }
        }

        public class Liveness
        {
            public List<BasicBlock> Blocks;

            public Liveness(List<BasicBlock> blocks)
            {
                Blocks = blocks;
                foreach (BasicBlock b in Blocks)
                {
                    HashSet<Temp.Temp> temp = new HashSet<Temp.Temp>();
                    foreach (TExp n in b.List)
                    {
                        n.LivenessNode = new LivenessNode(n);
                        temp.Clear();
                        temp.UnionWith(n.LivenessNode.Use);
                        temp.ExceptWith(b.LiveKill);
                        b.LiveGen.UnionWith(temp);
                        b.LiveKill.UnionWith(n.LivenessNode.Def);
                    }
                }
            }

            public void LivenessAnalyze()
            {
                bool flag;
                do
                {
                    flag = false;
                    for (int i = Blocks.Count - 1; i >= 0; --i)
                    {
                        BasicBlock b = Blocks[i];
                        HashSet<Temp.Temp> inTemp = new HashSet<Temp.Temp>(b.LiveIn);
                        HashSet<Temp.Temp> outTemp = new HashSet<Temp.Temp>(b.LiveOut);
                        b.LiveOut = new HashSet<Temp.Temp>();
                        foreach (BasicBlock s in b.Next)
                        {
                            b.LiveOut.UnionWith(s.LiveIn);
                        }
                        b.LiveIn = new HashSet<Temp.Temp>(b.LiveOut);
                        b.LiveIn.ExceptWith(b.LiveKill);
                        b.LiveIn.UnionWith(b.LiveGen);

                        if (!b.LiveIn.SetEquals(inTemp) || !b.LiveOut.SetEquals(outTemp))
                            flag = true;
                    }
                }
                while (flag);
            }
        }
    }
}