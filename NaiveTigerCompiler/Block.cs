using System.Collections.Generic;
using NaiveTigerCompiler.Quadruple;
using NaiveTigerCompiler.ReachingDefinition;
namespace NaiveTigerCompiler
{
    namespace Block
    {
        public class BasicBlock
        {
            public List<TExp> List = new List<TExp>();
            public List<BasicBlock> Prev = new List<BasicBlock>();
            public List<BasicBlock> Next = new List<BasicBlock>();

            public void AddEdge(BasicBlock target)
            {
                if (target == null)
                    return;
                Next.Add(target);
                target.Prev.Add(this);
            }

            public HashSet<Temp.Temp> LiveKill = new HashSet<Temp.Temp>();
            public HashSet<Temp.Temp> LiveGen = new HashSet<Temp.Temp>();
            public HashSet<Temp.Temp> LiveIn = new HashSet<Temp.Temp>();
            public HashSet<Temp.Temp> LiveOut = new HashSet<Temp.Temp>();

            public HashSet<PairInt> ReachKill = new HashSet<PairInt>();
            public HashSet<PairInt> ReachGen = new HashSet<PairInt>();
            public HashSet<PairInt> ReachIn = new HashSet<PairInt>();
            public HashSet<PairInt> ReachOut = new HashSet<PairInt>();

            public bool Visited;
        }

        public class BuildBlocks
        {
            public List<BasicBlock> Blocks = new List<BasicBlock>();

            public BuildBlocks(List<TExp> instList)
            {
                Dictionary<Temp.Label, BasicBlock> LabelToBlock = new Dictionary<Temp.Label, BasicBlock>();
                foreach (TExp e in instList)
                {
                    if (e is Label)
                    {
                        BasicBlock b = new BasicBlock();
                        Blocks.Add(b);
                        LabelToBlock[(e as Label).Lab] = b;
                    }
                    Blocks[Blocks.Count - 1].List.Add(e);
                }
                for (int i = 0; i < Blocks.Count; ++i)
                {
                    BasicBlock b = Blocks[i];
                    if (b.List[b.List.Count - 1] is Jump)
                    {
                        b.AddEdge(LabelToBlock[(b.List[b.List.Count - 1] as Jump).Label.Lab]);
                    }
                    else
                    {
                        if (i + 1 < Blocks.Count)
                            b.AddEdge(Blocks[i + 1]);
                        if (b.List[b.List.Count - 1] is CJump)
                            b.AddEdge(LabelToBlock[(b.List[b.List.Count - 1] as CJump).Label.Lab]);
                        else if (b.List[b.List.Count - 1] is CJumpInt)
                            b.AddEdge(LabelToBlock[(b.List[b.List.Count - 1] as CJumpInt).Label.Lab]);
                    }
                }
            }
        }
    }
}