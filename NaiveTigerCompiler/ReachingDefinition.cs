using System.Collections.Generic;
using NaiveTigerCompiler.Quadruple;
using NaiveTigerCompiler.Block;
using NaiveTigerCompiler.Liveness;
namespace NaiveTigerCompiler
{
    namespace ReachingDefinition
    {
        public class PairInt
        {
            public int i;
            public int j;
            public PairInt(int a, int b)
            {
                i = a;
                j = b;
            }
            public override bool Equals(object obj)
            {
                if (obj is PairInt)
                    return (i == (obj as PairInt).i && j == (obj as PairInt).j);
                else
                    return false;
            }
            public override int GetHashCode()
            {
                return (i << 10) + j;
            }
        }

        public class Node
        {
            public HashSet<PairInt> Gen = new HashSet<PairInt>();
            public HashSet<PairInt> Kill = new HashSet<PairInt>();

            public Node(TExp e, int i, int j, Dictionary<Temp.Temp, HashSet<PairInt>> defs)
            {
                LivenessNode node = new LivenessNode(e);
                foreach (Temp.Temp t in node.Def)
                {
                    Kill.UnionWith(defs[t]);
                }
                Kill.Remove(new PairInt(i, j));
                if (node.Def.Count != 0)
                {
                    Gen.Add(new PairInt(i, j));
                }
            }
        }

        public class TempPairInt
        {
            Temp.Temp Temp;
            PairInt Pair;
            public TempPairInt(Temp.Temp temp, PairInt pair)
            {
                Temp = temp;
                Pair = pair;
            }
            public override bool Equals(object obj)
            {
                if (obj is TempPairInt)
                {
                    return (Temp.Equals((obj as TempPairInt).Temp) && Pair.Equals((obj as TempPairInt).Pair));
                }
                else
                {
                    return false;
                }
            }
            public override int GetHashCode()
            {
                return Temp.GetHashCode() ^ Pair.GetHashCode();
            }
        }
        public class ReachingDefinition
        {
            public List<BasicBlock> Blocks;
            public Dictionary<TempPairInt, HashSet<PairInt>> DefineUseChain = new Dictionary<TempPairInt, HashSet<PairInt>>();
            public Dictionary<TempPairInt, HashSet<PairInt>> UseDefineChain = new Dictionary<TempPairInt, HashSet<PairInt>>();
            public Dictionary<Temp.Temp, HashSet<PairInt>> Defs = new Dictionary<Temp.Temp, HashSet<PairInt>>();

            public ReachingDefinition(List<BasicBlock> blocks)
            {
                Blocks = blocks;
                for (int i = 0; i < Blocks.Count; ++i)
                {
                    BasicBlock b = Blocks[i];
                    for (int j = 0; j < b.List.Count; ++j)
                    {
                        TExp inst = b.List[j];
                        LivenessNode node = inst.LivenessNode = new LivenessNode(inst);
                        foreach (Temp.Temp t in node.Def)
                        {
                            if (Defs[t] == null)
                                Defs.Add(t, new HashSet<PairInt>());
                            Defs[t].Add(new PairInt(i, j));
                        }
                    }
                }
                for (int i = 0; i < Blocks.Count; ++i)
                {
                    BasicBlock b = Blocks[i];
                    b.ReachIn.Clear();
                    b.ReachOut.Clear();
                    b.ReachGen.Clear();
                    b.ReachKill.Clear();
                    for (int j = 0; j < b.List.Count; ++j)
                    {
                        TExp inst = b.List[j];
                        Node node = inst.ReachingDefinitionNode = new Node(inst, i, j, Defs);
                        b.ReachGen.ExceptWith(node.Kill);
                        b.ReachGen.UnionWith(node.Gen);
                        b.ReachKill.UnionWith(node.Kill);
                    }
                }
            }

            public void ReachingDefinitions()
            {
                Queue<BasicBlock> queue = new Queue<BasicBlock>(Blocks);
                do
                {
                    BasicBlock b = queue.Dequeue();
                    HashSet<PairInt> old = new HashSet<PairInt>(b.ReachOut);
                    b.ReachIn = new HashSet<PairInt>();
                    foreach (BasicBlock p in b.Prev)
                    {
                        b.ReachIn.UnionWith(p.ReachOut);
                    }
                    b.ReachOut = new HashSet<PairInt>(b.ReachIn);
                    b.ReachOut.ExceptWith(b.ReachKill);
                    b.ReachOut.UnionWith(b.ReachGen);
                    if (!b.ReachOut.SetEquals(old))
                    {
                        foreach (BasicBlock s in b.Next)
                            queue.Enqueue(s);
                    }
                }
                while (queue.Count != 0);

                for (int i = 0; i < Blocks.Count; ++i)
                {
                    BasicBlock b = Blocks[i];
                    HashSet<PairInt> reachin = new HashSet<PairInt>(b.ReachIn);
                    for (int j = 0; j < b.List.Count; ++j)
                    {
                        TExp inst = b.List[j];
                        LivenessNode node = inst.LivenessNode;
                        PairInt q = new PairInt(i, j);
                        foreach (Temp.Temp t in node.Use)
                            foreach (PairInt p in reachin)
                                if (Blocks[p.i].List[p.j].LivenessNode.Def.Contains(t))
                                {
                                    SetOf(DefineUseChain, t, p).Add(q);
                                    SetOf(UseDefineChain, t, q).Add(p);
                                }
                        reachin.ExceptWith(inst.ReachingDefinitionNode.Kill);
                        reachin.UnionWith(inst.ReachingDefinitionNode.Gen);
                    }
                }
            }

            private HashSet<PairInt> SetOf(Dictionary<TempPairInt, HashSet<PairInt>> chain, Temp.Temp t, PairInt p)
            {
                TempPairInt key = new TempPairInt(t, p);
                if (chain[key] == null)
                    chain.Add(key, new HashSet<PairInt>());
                return chain[key];
            }
        }
    }
}