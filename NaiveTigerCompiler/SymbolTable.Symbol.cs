using System.Collections;
namespace NaiveTigerCompiler
{
    namespace SymbolTable
    {
        public class Symbol
        {
            string Name;
            static Hashtable dict = new Hashtable();

            Symbol(string name)
            {
                Name = name;
            }

            public override string ToString()
            {
                return Name;
            }

            public static Symbol GetSymbol(string name)
            {
                if (!dict.ContainsKey(name))
                {
                    dict.Add(name, new Symbol(name));
                }
                return dict[name] as Symbol;
            }
        }

    }
}