using System.Collections.Generic;
using NaiveTigerCompiler.SymbolTable;
namespace NaiveTigerCompiler
{
    namespace Temp
    {
        public class Temp
        {
            private static int Count;
            public int Num;
            public override string ToString()
            {
                return "t" + Num;
            }
            public Temp()
            {
                Num = Count++;
            }
        }

        public class TempList
        {
            public Temp Head;
            public TempList Tail;
            public TempList(Temp head, TempList tail)
            {
                Head = head;
                Tail = tail;
            }
        }

        public interface TempMap
        {
            string TempMap(Temp temp);
        }

        public class Label
        {
            public string Name;
            private static int Count;
            public override string ToString()
            {
                return Name;
            }
            public Label(string name)
            {
                Name = name;
            }
            public Label()
            {
                Name = "L" + Count++;
            }
            public Label(Symbol symbol)
            {
                Name = symbol.ToString();
            }
        }

        public class LabelList
        {
            public Label Head;
            public LabelList Tail;
            public LabelList(Label head, LabelList tail)
            {
                Head = head;
                Tail = tail;
            }
        }

        public class DefaultMap : TempMap
        {
            public string TempMap(Temp temp)
            {
                return temp.ToString();
            }

            public DefaultMap()
            {
            }
        }

        public class CombineMap : TempMap
        {
            TempMap TempMap1, TempMap2;
            public string TempMap(Temp temp)
            {
                string s = TempMap1.TempMap(temp);
                if (s != null)
                    return s;
                return TempMap2.TempMap(temp);
            }

            public CombineMap(TempMap t1, TempMap t2)
            {
                TempMap1 = t1;
                TempMap2 = t2;
            }
        }
    }
}