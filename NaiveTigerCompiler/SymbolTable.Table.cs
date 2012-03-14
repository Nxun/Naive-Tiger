using System.Collections;
namespace NaiveTigerCompiler
{
    namespace SymbolTable
    {
        class Binder
        {
            public object Value;
            public Symbol PrevTop;
            public Binder Tail;
            public Binder(object value, Symbol prev, Binder tail)
            {
                Value = value;
                PrevTop = prev;
                Tail = tail;
            }
        }

        public class Table
        {
            private Hashtable dict = new Hashtable();
            private Symbol Top;
            private Binder Marks;

            public Table()
            {
            }

            public object this[Symbol key]
            {
                get
                {
                    Binder e = (Binder)dict[key];
                    if (e == null)
                        return null;
                    else
                        return e.Value;
                }

                set
                {
                    dict[key] = new Binder(value, Top, (Binder)dict[key]);
                    Top = key;
                }
            }

            public void BeginScope()
            {
                Marks = new Binder(null, Top, Marks);
                Top = null;
            }

            public void EndScope()
            {
                while (Top != null)
                {
                    Binder e = (Binder)dict[Top];
                    if (e.Tail != null)
                        dict[Top] = e.Tail;
                    else
                        dict.Remove(Top);
                    Top = e.PrevTop;
                }
                Top = Marks.PrevTop;
                Marks = Marks.Tail;
            }
        }
    }
}