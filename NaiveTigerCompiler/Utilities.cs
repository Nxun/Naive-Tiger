using System.Collections.Generic;
using NaiveTigerCompiler.AbstractSyntax;
namespace NaiveTigerCompiler
{
    namespace Utilities
    {
        public class BoolList
        {
            public bool Head;
            public BoolList Tail;
            public BoolList(bool head, BoolList tail)
            {
                Head = head;
                Tail = tail;
            }
            public static BoolList BuildFromFieldList(FieldList param)
            {
                if (param == null)
                    return null;
                else
                    return new BoolList(param.Escape, BuildFromFieldList(param.Tail));
            }
        }
    }
}