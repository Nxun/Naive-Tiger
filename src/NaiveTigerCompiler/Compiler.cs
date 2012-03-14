using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;
using System.IO;

namespace NaiveTigerCompiler
{
    public class Compiler
    {
        public static List<Error> ErrorList = new List<Error>();

        string _runtime = "runtime.s";
        StreamWriter _writer;
        string _source;
        string _output;
        public Compiler(string s, string o)
        {
            _source = s;
            _output = o;

            ErrorList.Clear();
        }

        public bool Compile()
        {
            try
            {
                ANTLRStringStream stream = new ANTLRStringStream(_source);
                TigerLexer lexer = new TigerLexer(stream);
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                TigerParser parser = new TigerParser(tokens);

                AbstractSyntax.Expression result = parser.program();

                if (ErrorList.Count > 0)
                    return false;

                Mips.MipsFrame frame = new Mips.MipsFrame();

                FindEscape.FindEscape escape = new FindEscape.FindEscape();
                try
                {
                    escape.Find(result);
                }
                catch
                { }

                Semantics.Semantics semantics = new Semantics.Semantics(frame);
                Translate.Frag frag = semantics.TranslateProgram(result);

                if (ErrorList.Count > 0)
                    return false;

                using (_writer = File.CreateText(_output))
                {
                    
                    for (Translate.Frag f = frag; f != null; f = f.Next)
                    {
                        if (f is Translate.ProcFrag)
                        {
                            EmitProcess(f as Translate.ProcFrag);
                        }
                        else
                        {
                            _writer.WriteLine((f as Translate.DataFrag).Data);
                        }
                    }

                    if (File.Exists(_runtime))
                    {
                        _writer.WriteLine(File.ReadAllText(_runtime));
                    }
                    else
                    {
                        throw new FatalError("mips runtime library not found");
                    }

                }
            }
            catch
            {
                return false;
            }
            if (ErrorList.Count > 0)
                return false;
            else
                return true;
        }

        private void EmitProcess(Translate.ProcFrag f)
        {
            //
            //Temp.TempMap map = new Temp.CombineMap(f.Frame, new Temp.DefaultMap());
            //Tree.Print pt = new Tree.Print(_writer, map);
            //pt.PrintStm(f.Body);
            //

            Tree.StmList stms = Canon.Canon.Linearize(f.Body);
            Canon.BasicBlocks b = new Canon.BasicBlocks(stms);
            Tree.StmList traced = (new Canon.TraceSchedule(b)).Stms;
            Quadruple.Quadruple threeAddr = new Quadruple.Quadruple(f.Frame as Mips.MipsFrame);
            threeAddr.CodeGenerate(traced);
            threeAddr.InstrList = (f.Frame as Mips.MipsFrame).ProcessEntryExit2(threeAddr.InstrList);
            
            Block.BuildBlocks buildBlocks = new Block.BuildBlocks(threeAddr.InstrList);
            //
            //Quadruple.Print print = new Quadruple.Print(_writer);
            //print.print(buildBlocks.Blocks);
            //
            RegisterAllocate.RegisterAllocate reg = new RegisterAllocate.RegisterAllocate((f.Frame as Mips.MipsFrame), buildBlocks.Blocks, f.Frame.Registers());
            reg.Allocate();

            List<Quadruple.TExp> instrList = new List<Quadruple.TExp>();
            foreach (Block.BasicBlock blk in reg.Blocks)
            {
                instrList.AddRange(blk.List);
            }
            instrList = (f.Frame as Mips.MipsFrame).ProcessEntryExit3(instrList);
            CodeGenerate.CodeGen gen = new CodeGenerate.CodeGen(instrList, reg.TempToNode, (f.Frame as Mips.MipsFrame));

            gen.CodeGenerate(_writer);

        }
    }
}
