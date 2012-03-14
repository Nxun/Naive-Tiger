using System.Collections.Generic;
using NaiveTigerCompiler.Block;
using NaiveTigerCompiler.Tree;
using NaiveTigerCompiler.Temp;
namespace NaiveTigerCompiler
{
    namespace Quadruple
    {
        public class Print
        {
            System.IO.TextWriter Out;
            public Print(System.IO.TextWriter o) { Out = o; }
            public void print(List<BasicBlock> blocks)
            {
                foreach (BasicBlock b in blocks)
                    foreach (TExp it in b.List)
                        print(it);
            }

            public void print(TExp exp)
            {
                if (exp is BinOp)
                    print((BinOp)exp);
                else if (exp is BinOpInt)
                    print((BinOpInt)exp);
                else if (exp is Call)
                    print((Call)exp);
                else if (exp is CJump)
                    print((CJump)exp);
                else if (exp is CJumpInt)
                    print((CJumpInt)exp);
                else if (exp is Jump)
                    print((Jump)exp);
                else if (exp is Label)
                    print((Label)exp);
                else if (exp is Load)
                    print((Load)exp);
                else if (exp is Move)
                    print((Move)exp);
                else if (exp is MoveInt)
                    print((MoveInt)exp);
                else if (exp is MoveLabel)
                    print((MoveLabel)exp);
                else if (exp is ReturnSink)
                    print((ReturnSink)exp);
                else if (exp is Store)
                    print((Store)exp);
            }

            void print(BinOp exp)
            {
                Out.Write("BinOp ");
                switch (exp.Op)
                {
                    case BINOP.Op.Plus: Out.Write('+'); break;
                    case BINOP.Op.Minus: Out.Write('-'); break;
                    case BINOP.Op.Times: Out.Write('*'); break;
                    case BINOP.Op.Divide: Out.Write('/'); break;
                    default: Out.Write(exp.Op); break;
                }
                Out.WriteLine(' ' + exp.Dst.ToString() + ' ' + exp.Left + ' ' + exp.Right);
            }

            void print(BinOpInt exp)
            {
                Out.Write("BinOp ");
                switch (exp.Op)
                {
                    case BINOP.Op.Plus: Out.Write('+'); break;
                    case BINOP.Op.Minus: Out.Write('-'); break;
                    case BINOP.Op.Times: Out.Write('*'); break;
                    case BINOP.Op.Divide: Out.Write('/'); break;
                    default: Out.Write(exp.Op); break;
                }
                Out.WriteLine(' ' + exp.Dst.ToString() + ' ' + exp.Left + ' ' + exp.Right);
            }

            void print(Call exp)
            {
                Out.Write("Call " + exp.Name.Lab + '(');
                for (TempList it = exp.Param; it != null; it = it.Tail)
                {
                    Out.Write(it.Head);
                    if (it.Tail != null) Out.Write(", ");
                }
                Out.WriteLine(')');
            }

            void print(CJump exp)
            {
                Out.WriteLine("CJump " + exp.Relop + ' ' + exp.Left + ' ' + exp.Right + ' ' + exp.Label.Lab);
            }

            void print(CJumpInt exp)
            {
                Out.WriteLine("CJumpI " + exp.Relop + ' ' + exp.Left + ' ' + exp.Right + ' ' + exp.Label.Lab);
            }

            void print(Jump exp)
            {
                Out.WriteLine("Jump " + exp.Label.Lab);
            }

            void print(Label exp)
            {
                Out.WriteLine("Label " + exp.Lab);
            }

            void print(Load exp)
            {
                Out.WriteLine("Load " + exp.Dst + ' ' + exp.Mem + ' ' + exp.Offset);
            }

            void print(Move exp)
            {
                Out.WriteLine("Move " + exp.Dst + ' ' + exp.Src);
            }

            void print(MoveInt exp)
            {
                Out.WriteLine("MoveI " + exp.Dst + ' ' + exp.Src);
            }

            void print(MoveLabel exp)
            {
                Out.WriteLine("MoveLabel " + exp.Dst + ' ' + exp.Src.Lab);
            }

            void print(ReturnSink exp)
            {
                Out.WriteLine("ReturnSink");
            }

            void print(Store exp)
            {
                Out.WriteLine("Store " + exp.Src + ' ' + exp.Mem + ' ' + exp.Offset);
            }
        }

    }
}