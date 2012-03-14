using System.Collections.Generic;
using NaiveTigerCompiler.Mips;
using NaiveTigerCompiler.Block;
using NaiveTigerCompiler.Quadruple;
using NaiveTigerCompiler.Liveness;
namespace NaiveTigerCompiler
{
    namespace RegisterAllocate
    {
        public class Node
        {
            public Temp.Temp Temp;
            public Node(Temp.Temp temp)
            {
                Temp = temp;
            }
            public List<Node> AdjList = new List<Node>();
            public List<Move> MoveList = new List<Move>();
            public int Degree = 0;
            public Node Alias;
            public int Color = -1;
            public bool IsNew = false;
        }

        public class RegisterAllocate
        {
            static int K = 28;
            static int Infinity = 1 << 28;

            MipsFrame Frame;
            public List<BasicBlock> Blocks;

            class Edge
            {
                Node Start, Target;
                public Edge(Node s, Node t)
                {
                    Start = s;
                    Target = t;
                }
                public override bool Equals(object obj)
                {
                    if (!(obj is Edge))
                        return false;
                    Edge o = obj as Edge;
                    return (o.Start.Equals(Start) && o.Target.Equals(Target));
                }
                public override int GetHashCode()
                {
                    return 37 * Start.GetHashCode() + Target.GetHashCode();
                }
            }

            HashSet<Edge> AdjSet = new HashSet<Edge>();

            public Dictionary<Temp.Temp, Node> TempToNode = new Dictionary<Temp.Temp, Node>();

            HashSet<Node> Precolored = new HashSet<Node>();
            HashSet<Node> Initial = new HashSet<Node>();
            HashSet<Node> SimplifyWorklist = new HashSet<Node>(),
                FreezeWorklist = new HashSet<Node>(),
                SpillWorklist = new HashSet<Node>(),
                SpilledNodes = new HashSet<Node>(),
                CoalescedNodes = new HashSet<Node>(),
                ColoredNodes = new HashSet<Node>();

            Stack<Node> SelectStack = new Stack<Node>();

            List<Move> CoalescedMoves = new List<Move>(),
                ConstrainedMoves = new List<Move>(),
                FrozenMoves = new List<Move>(),
                WorklistMoves = new List<Move>(),
                ActiveMoves = new List<Move>();

            public RegisterAllocate(MipsFrame frame, List<BasicBlock> blocks, Temp.Temp[] precolored)
            {
                Frame = frame;
                Blocks = blocks;
                for (int i = 0; i < precolored.Length; ++i)
                {
                    Precolored.Add(GetNodeByTemp(precolored[i]));
                    GetNodeByTemp(precolored[i]).Color = i;
                    GetNodeByTemp(precolored[i]).Degree = Infinity;
                }
                foreach (BasicBlock b in blocks)
                    foreach (TExp i in b.List)
                    {
                        LivenessNode n = new LivenessNode(i);
                        foreach (Temp.Temp t in n.Def)
                            if (!TempToNode.ContainsKey(t))
                                Initial.Add(GetNodeByTemp(t));
                        foreach (Temp.Temp t in n.Use)
                            if (!TempToNode.ContainsKey(t))
                                Initial.Add(GetNodeByTemp(t));
                    }
            }

            public void Allocate()
            {
                Liveness.Liveness liveness = new Liveness.Liveness(Blocks);
                liveness.LivenessAnalyze();

                Build();
                MakeWorklist();
                do
                {
                    if (SimplifyWorklist.Count != 0)
                        Simplify();
                    else if (WorklistMoves.Count != 0)
                        Coalesce();
                    else if (FreezeWorklist.Count != 0)
                        Freeze();
                    else if (SpillWorklist.Count != 0)
                        SelectSpill();
                }
                while (SimplifyWorklist.Count != 0 || WorklistMoves.Count != 0 ||
                         FreezeWorklist.Count != 0 || SpillWorklist.Count != 0);
                AssignColor();
                if (SpilledNodes.Count != 0)
                {
                    RewriteProgram(SpilledNodes);
                    Allocate();
                }
            }

            private void RewriteProgram(HashSet<Node> spilledNodes)
            {
                HashSet<Node> newTemps = new HashSet<Node>();
                foreach (Node v in spilledNodes)
                {
                    InFrame a = (InFrame)Frame.AllocLocal(true);
                    foreach (BasicBlock b in Blocks)
                        for (int i = 0; i < b.List.Count; i++)
                        {
                            TExp inst = b.List[i];
                            if (inst.LivenessNode == null) continue;
                            if (inst.LivenessNode.Use.Contains(v.Temp))
                            {
                                Temp.Temp p = new Temp.Temp();
                                newTemps.Add(GetNodeByTemp(p));
                                GetNodeByTemp(p).IsNew = true;
                                b.List.Insert(i, new Load(Frame.FP(), a.Offset, p));
                                b.List[++i].ReplaceUse(v.Temp, p);
                            }
                            if (inst.LivenessNode.Def.Contains(v.Temp))
                            {
                                Temp.Temp p = new Temp.Temp();
                                newTemps.Add(GetNodeByTemp(p));
                                GetNodeByTemp(p).IsNew = true;
                                b.List.Insert(i + 1, new Store(Frame.FP(), a.Offset, p));
                                b.List[i++].ReplaceDef(v.Temp, p);
                            }
                        }
                }
                spilledNodes.Clear();
                Initial = newTemps;
                Initial.UnionWith(ColoredNodes);
                Initial.UnionWith(CoalescedNodes);
                ColoredNodes.Clear();
                CoalescedNodes.Clear();
            }

            private void AssignColor()
            {
                while (SelectStack.Count != 0)
                {
                    Node n = SelectStack.Pop();
                    List<int> okColors = new List<int>();
                    for (int i = 8; i <= 25; i++)
                        okColors.Add(i);
                    for (int i = 2; i <= 7; i++)
                        okColors.Add(i);
                    for (int i = 28; i <= 31; i++)
                        okColors.Add(i);
                    HashSet<Node> nodes = new HashSet<Node>(Precolored);
                    nodes.UnionWith(ColoredNodes);
                    foreach (Node w in n.AdjList)
                    {
                        if (nodes.Contains(GetAlias(w)))
                            okColors.Remove(GetAlias(w).Color);
                    }
                    if (okColors.Count == 0)
                        SpilledNodes.Add(n);
                    else
                    {
                        ColoredNodes.Add(n);
                        int c = okColors[0];
                        n.Color = c;
                    }
                }
                foreach (Node n in CoalescedNodes)
                    n.Color = GetAlias(n).Color;
            }

            private void SelectSpill()
            {
                Node m = null;
                double minPriority = Infinity;
                foreach (Node n in SpillWorklist)
                    if (!Precolored.Contains(n) && !n.IsNew && Priority(n) < minPriority)
                    {
                        m = n;
                        minPriority = Priority(n);
                    }
                if (m == null)
                {
                    throw new FatalError("Error at SelectSpill in RegisterAllocate");
                }
                SpillWorklist.Remove(m);
                SimplifyWorklist.Add(m);
                FreezeMoves(m);
            }

            private double Priority(Node m)
            {
                return (double)1 / m.Degree;
            }

            private void Freeze()
            {
                HashSet<Node>.Enumerator it = FreezeWorklist.GetEnumerator();
                it.MoveNext();
                Node n = it.Current;
                FreezeWorklist.Remove(n);
                SimplifyWorklist.Add(n);
                FreezeMoves(n);
            }

            private void FreezeMoves(Node u)
            {
                foreach (Move m in NodeMoves(u))
                {
                    Node x = GetNodeByTemp(m.Dst);
                    Node y = GetNodeByTemp(m.Src);
                    Node v;
                    if (GetAlias(y) == GetAlias(u))
                        v = GetAlias(x);
                    else
                        v = GetAlias(y);
                    ActiveMoves.Remove(m);
                    FrozenMoves.Add(m);
                    if (NodeMoves(v).Count == 0 && v.Degree < K)
                    {
                        FreezeWorklist.Remove(v);
                        SimplifyWorklist.Add(v);
                    }
                }
            }

            private void Coalesce()
            {
                Move m = WorklistMoves[0];
                WorklistMoves.RemoveAt(0);
                Node x = GetAlias(GetNodeByTemp(m.Dst));
                Node y = GetAlias(GetNodeByTemp(m.Src));
                Node u, v;
                if (Precolored.Contains(y))
                {
                    u = y;
                    v = x;
                }
                else
                {
                    u = x;
                    v = y;
                }
                List<Node> nodes = new List<Node>(Adjacent(u));
                nodes.AddRange(Adjacent(v));
                if (u == v)
                {
                    CoalescedMoves.Add(m);
                    AddWorkList(u);
                }
                else if (Precolored.Contains(v) || AdjSet.Contains(new Edge(u, v)))
                {
                    ConstrainedMoves.Add(m);
                    AddWorkList(u);
                    AddWorkList(v);
                }
                else if (Precolored.Contains(u) && CheckOK(u, v) || !Precolored.Contains(u) && Conservative(nodes))
                {
                    CoalescedMoves.Add(m);
                    Combine(u, v);
                    AddWorkList(u);
                }
                else
                    ActiveMoves.Add(m);
            }

            private void Combine(Node u, Node v)
            {
                if (FreezeWorklist.Contains(v))
                    FreezeWorklist.Remove(v);
                else
                    SpillWorklist.Remove(v);
                CoalescedNodes.Add(v);
                v.Alias = u;
                HashSet<Move> tmp = new HashSet<Move>(u.MoveList);
                tmp.UnionWith(v.MoveList);
                u.MoveList = new List<Move>(tmp);
                List<Node> vv = new List<Node>();
                vv.Add(v);
                EnableMoves(vv);
                foreach (Node t in Adjacent(v))
                {
                    AddEdge(t.Temp, u.Temp);
                    DecrementDegree(t);
                }
                if (u.Degree >= K && FreezeWorklist.Contains(u))
                {
                    FreezeWorklist.Remove(u);
                    SpillWorklist.Add(u);
                }
            }

            private bool Conservative(List<Node> nodes)
            {
                int k = 0;
                foreach (Node n in nodes)
                    if (n.Degree >= K)
                        k++;
                return k < K;
            }

            private bool CheckOK(Node u, Node v)
            {
                foreach (Node t in Adjacent(v))
                    if (!OK(t, u))
                        return false;
                return true;
            }

            private bool OK(Node t, Node r)
            {
                return t.Degree < K || Precolored.Contains(t) || AdjSet.Contains(new Edge(t, r));
            }

            private void AddWorkList(Node u)
            {
                if (!Precolored.Contains(u) && !MoveRelated(u) && u.Degree < K)
                {
                    FreezeWorklist.Remove(u);
                    SimplifyWorklist.Add(u);
                }
            }

            private Node GetAlias(Node n)
            {
                if (CoalescedNodes.Contains(n))
                    return GetAlias(n.Alias);
                else
                    return n;
            }

            private void Simplify()
            {
                HashSet<Node>.Enumerator it = SimplifyWorklist.GetEnumerator();
                it.MoveNext();
                Node n = it.Current;
                SimplifyWorklist.Remove(n);
                SelectStack.Push(n);
                foreach (Node m in Adjacent(n))
                    DecrementDegree(m);
            }

            private void DecrementDegree(Node m)
            {
                int d = m.Degree;
                m.Degree = d - 1;
                if (d == K)
                {
                    List<Node> nodes = new List<Node>(Adjacent(m));
                    nodes.Add(m);
                    EnableMoves(nodes);
                    SpillWorklist.Remove(m);
                    if (MoveRelated(m))
                        FreezeWorklist.Add(m);
                    else
                        SimplifyWorklist.Add(m);
                }
            }

            private void EnableMoves(List<Node> nodes)
            {
                foreach (Node n in nodes)
                    foreach (Move m in NodeMoves(n))
                        if (ActiveMoves.Contains(m))
                        {
                            ActiveMoves.Remove(m);
                            WorklistMoves.Add(m);
                        }
            }

            private void MakeWorklist()
            {
                while (Initial.Count != 0)
                {
                    HashSet<Node>.Enumerator it = Initial.GetEnumerator();
                    it.MoveNext();
                    Node n = it.Current;
                    Initial.Remove(n);
                    if (n.Degree >= K)
                        SpillWorklist.Add(n);
                    else if (MoveRelated(n))
                        FreezeWorklist.Add(n);
                    else
                        SimplifyWorklist.Add(n);
                }
            }

            private bool MoveRelated(Node n)
            {
                return NodeMoves(n).Count != 0;
            }

            private List<Move> NodeMoves(Node n)
            {
                List<Move> res = new List<Move>(ActiveMoves);
                res.AddRange(WorklistMoves);
                res.RemoveAll(delegate(Move m) { return !n.MoveList.Contains(m); });
                return res;
            }

            private List<Node> Adjacent(Node node)
            {
                List<Node> res = new List<Node>(node.AdjList);
                res.RemoveAll(delegate(Node n) { return SelectStack.Contains(n); });
                res.RemoveAll(delegate(Node n) { return CoalescedNodes.Contains(n); });
                return res;
            }

            private void Build()
            {
                foreach (BasicBlock b in Blocks)
                {
                    HashSet<Temp.Temp> live = new HashSet<Temp.Temp>(b.LiveOut);
                    for (int i = b.List.Count - 1; i >= 0; i--)
                    {
                        TExp inst = b.List[i];
                        if (inst is Move)
                        {
                            live.ExceptWith(inst.LivenessNode.Use);
                            HashSet<Temp.Temp> nodes = new HashSet<Temp.Temp>(inst.LivenessNode.Def);
                            nodes.UnionWith(inst.LivenessNode.Use);
                            foreach (Temp.Temp n in nodes)
                                GetNodeByTemp(n).MoveList.Add((Move)inst);
                            WorklistMoves.Add((Move)inst);
                        }
                        live.UnionWith(inst.LivenessNode.Def);
                        foreach (Temp.Temp d in inst.LivenessNode.Def)
                            foreach (Temp.Temp l in live)
                                AddEdge(l, d);
                        live.ExceptWith(inst.LivenessNode.Def);
                        live.UnionWith(inst.LivenessNode.Use);
                    }
                }
            }

            private Node GetNodeByTemp(Temp.Temp t)
            {
                if (!TempToNode.ContainsKey(t))
                {
                    TempToNode.Add(t, new Node(t));
                }
                return TempToNode[t];
            }

            private void AddEdge(Temp.Temp u, Temp.Temp v)
            {
                Node uu = GetNodeByTemp(u), vv = GetNodeByTemp(v);
                if (u != v && !AdjSet.Contains(new Edge(uu, vv)))
                {
                    AdjSet.Add(new Edge(uu, vv));
                    AdjSet.Add(new Edge(vv, uu));
                    if (!Precolored.Contains(uu))
                    {
                        uu.AdjList.Add(vv);
                        uu.Degree++;
                    }
                    if (!Precolored.Contains(vv))
                    {
                        vv.AdjList.Add(uu);
                        vv.Degree++;
                    }
                }
            }

            public void Print(System.IO.TextWriter o)
            {
                foreach (Node n in ColoredNodes)
                    o.WriteLine(n.Temp + ": $" + n.Color);
            }
        }

    }
}