using NaiveTigerCompiler.SymbolTable;
namespace NaiveTigerCompiler
{
    namespace Types
    {
        public abstract class Type
        {
            public static Type _int = new INT();
            public static Type _string = new STRING();
            public static Type _void = new VOID();
            public static Type _nil = new NIL();
            public static Type _unknown = new RECORD(null, null, null);

            public virtual Type Actual
            {
                get
                {
                    return this;
                }
            }

            public virtual bool CoerceTo(Type type)
            {
                return false;
            }
        }

        public class ARRAY : Type
        {
            public Type Element;
            public ARRAY(Type e)
            {
                Element = e;
            }

            public override bool CoerceTo(Type type)
            {
                return this == type.Actual;
            }

            public override string ToString()
            {
                return "array";
            }
        }

        public class INT : Type
        {
            public INT() {}

            public override bool CoerceTo(Type type)
            {
                return (type.Actual is INT);
            }

            public override string ToString()
            {
                return "int";
            }
        }

        public class NAME : Type
        {
            public Symbol Name;
            private Type Binding;
            public NAME(Symbol name)
            {
                Name = name;
            }
            public bool isLoop()
            {
                Type b = Binding;
                bool any;
                Binding = null;
                if (b == null)
                    any = true;
                else if (b is NAME)
                    any = ((NAME)b).isLoop();
                else
                    any = false;
                Binding = b;
                return any;
            }

            public override Type Actual
            {
                get
                {
                    return Binding.Actual;
                }
            }

            public override bool CoerceTo(Type type)
            {
                return this.Actual.CoerceTo(type);
            }

            public void Bind(Type type)
            {
                Binding = type;
            }

        }

        public class NIL : Type
        {
            public NIL() { }

            public override bool CoerceTo(Type type)
            {
                Type a = type.Actual;
                return (a is RECORD || a is NIL);
            }

            public override string ToString()
            {
                return "nil";
            }
        }

        public class RECORD : Type
        {
            public Symbol FieldName;
            public Type FieldType;
            public RECORD Tail;

            public RECORD(Symbol name, Type type, RECORD x)
            {
                FieldName = name;
                FieldType = type;
                Tail = x;
            }

            public override bool CoerceTo(Type type)
            {
                return this == type.Actual;
            }

            public override string ToString()
            {
                return "record";
            }
        }

        public class STRING : Type
        {
            public STRING() { }
            public override bool CoerceTo(Type type)
            {
                return (type.Actual is STRING);
            }
            public override string ToString()
            {
                return "string";
            }
        }

        public class VOID : Type
        {
            public VOID() { }
            public override bool CoerceTo(Type type)
            {
                return (type.Actual is VOID);
            }
            public override string ToString()
            {
                return "void";
            }
        }
    }
}