using NaiveTigerCompiler.SymbolTable;
using NaiveTigerCompiler.Translate;
using NaiveTigerCompiler.Types;
using System.Collections.Generic;
using NaiveTigerCompiler.Temp;
using NaiveTigerCompiler.Utilities;
namespace NaiveTigerCompiler
{
    namespace Semantics
    {
        public class Environment
        {
            public Table ValueEnvironment;
            public Table TypeEnvironment;
            public LoopEnvironment LoopEnvironment;
            Level Root = null;
            public Environment(Level root)
            {
                ValueEnvironment = new Table();
                TypeEnvironment = new Table();
                LoopEnvironment = new LoopEnvironment();
                Root = root;

                // vEnv
                TypeEnvironment[Symbol.GetSymbol("int")] = Type._int;
                TypeEnvironment[Symbol.GetSymbol("string")] = Type._string;

                // tEnv
                Symbol symbol = null;
                RECORD formals = null;
                Type result = null;
                Level level = null;
                Label label = null;

                symbol = Symbol.GetSymbol("print");
                formals = new RECORD(Symbol.GetSymbol("s"), Type._string, null);
                result = Type._void;
                label = new Label("_" + symbol);
                level = new Level(Root, label, new BoolList(false, null), false);
                ValueEnvironment[symbol] = new StandardFunctionEntry(level, label, formals, result);

                symbol = Symbol.GetSymbol("printi");
                formals = new RECORD(Symbol.GetSymbol("i"), Type._int, null);
                result = Type._void;
                label = new Label("_" + symbol);
                level = new Level(Root, label, new BoolList(false, null), false);
                ValueEnvironment[symbol] = new StandardFunctionEntry(level, label, formals, result);

                symbol = Symbol.GetSymbol("flush");
                formals = null;
                result = Type._void;
                label = new Label("_" + symbol);
                level = new Level(Root, label, null, false);
                ValueEnvironment[symbol] = new StandardFunctionEntry(level, label, formals, result);

                symbol = Symbol.GetSymbol("getchar");
                formals = null;
                result = Type._string;
                label = new Label("_" + symbol);
                level = new Level(Root, label, null, false);
                ValueEnvironment[symbol] = new StandardFunctionEntry(level, label, formals, result);

                symbol = Symbol.GetSymbol("ord");
                formals = new RECORD(Symbol.GetSymbol("s"), Type._string, null);
                result = Type._int;
                label = new Label("_" + symbol);
                level = new Level(Root, label, new BoolList(false, null), false);
                ValueEnvironment[symbol] = new StandardFunctionEntry(level, label, formals, result);

                symbol = Symbol.GetSymbol("chr");
                formals = new RECORD(Symbol.GetSymbol("i"), Type._int, null);
                result = Type._string;
                label = new Label("_" + symbol);
                level = new Level(Root, label, new BoolList(false, null), false);
                ValueEnvironment[symbol] = new StandardFunctionEntry(level, label, formals, result);

                symbol = Symbol.GetSymbol("size");
                formals = new RECORD(Symbol.GetSymbol("s"), Type._string, null);
                result = Type._int;
                label = new Label("_" + symbol);
                level = new Level(Root, label, new BoolList(false, null), false);
                ValueEnvironment[symbol] = new StandardFunctionEntry(level, label, formals, result);

                symbol = Symbol.GetSymbol("substring");
                formals = new RECORD(Symbol.GetSymbol("s"), Type._string,
                            new RECORD(Symbol.GetSymbol("f"), Type._int,
                                new RECORD(Symbol.GetSymbol("n"), Type._int, null)));
                result = Type._string;
                label = new Label("_" + symbol);
                level = new Level(Root, label, new BoolList(false, new BoolList(false, new BoolList(false, null))), false);
                ValueEnvironment[symbol] = new StandardFunctionEntry(level, label, formals, result);

                symbol = Symbol.GetSymbol("concat");
                formals = new RECORD(Symbol.GetSymbol("s1"), Type._string,
                            new RECORD(Symbol.GetSymbol("s2"), Type._string, null));
                result = Type._string;
                label = new Label("_" + symbol);
                level = new Level(Root, label, new BoolList(false, new BoolList(false, null)), false);
                ValueEnvironment[symbol] = new StandardFunctionEntry(level, label, formals, result);

                symbol = Symbol.GetSymbol("not");
                formals = new RECORD(Symbol.GetSymbol("i"), Type._int, null);
                result = Type._int;
                label = new Label("_" + symbol);
                level = new Level(Root, label, new BoolList(false, null), false);
                ValueEnvironment[symbol] = new StandardFunctionEntry(level, label, formals, result);

                symbol = Symbol.GetSymbol("exit");
                formals = new RECORD(Symbol.GetSymbol("i"), Type._int, null);
                result = Type._void;
                label = new Label("_" + symbol);
                level = new Level(Root, label, new BoolList(false, null), false);
                ValueEnvironment[symbol] = new StandardFunctionEntry(level, label, formals, result);
            }
        }

        public class LoopEnvironment
        {
            Stack<Stack<Label>> Stack = new Stack<Stack<Label>>();
            
            public LoopEnvironment()
            {
                Stack.Push(new Stack<Label>());
            }

            public void BeginScope()
            {
                Stack.Push(new Stack<Label>());
            }

            public void EndScope()
            {
                Stack.Pop();
            }

            public void EnterLoop()
            {
                Stack.Peek().Push(new Label());
            }

            public Label ExitLoop()
            {
                return Stack.Peek().Pop();
            }

            public Label Done()
            {
                return Stack.Peek().Peek();
            }

            public bool InLoop()
            {
                return Stack.Peek().Count != 0;
            }
        }
    }
}