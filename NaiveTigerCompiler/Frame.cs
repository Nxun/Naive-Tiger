using NaiveTigerCompiler.Tree;
using NaiveTigerCompiler.Utilities;
using System.Collections.Generic;
using NaiveTigerCompiler.Quadruple;
using NaiveTigerCompiler.Temp;
namespace NaiveTigerCompiler
{
    namespace Frame
    {
        public abstract class Access
        {
            public abstract Expr Exp(Expr FramePtr);
            public abstract Expr ExpFromStack(Expr StackPtr);
        }

        public class AccessList
        {
            public Access Head;
            public AccessList Tail;
            public AccessList(Access head, AccessList tail)
            {
                Head = head;
                Tail = tail;
            }
        }

        public abstract class Frame : TempMap
        {
            public Temp.Label Name;
            public AccessList Formals = null;
            public abstract Frame NewFrame(Temp.Label name, BoolList formals);
            public abstract Access AllocLocal(bool escape);
            public abstract Temp.Temp FP();
            public abstract Temp.Temp SP();
            public abstract Temp.Temp RA();
            public abstract Temp.Temp RV();
            public abstract Temp.Temp[] Registers();
            public abstract Expr ExternalCall(string funcName, ExpList args);
            public abstract Stm ProcessEntryExit1(Stm body);
            public abstract List<TExp> ProcessEntryExit2(List<TExp> body);
            public abstract List<TExp> ProcessEntryExit3(List<TExp> body);
            public abstract string String(Temp.Label label, string values);
            public abstract int WordSize();
            public abstract string TempMap(Temp.Temp temp);
        }
    }
}