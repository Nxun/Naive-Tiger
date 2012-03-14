using NaiveTigerCompiler.AbstractSyntax;
using System.Collections.Generic;
using NaiveTigerCompiler.Temp;
using NaiveTigerCompiler.Utilities;
using NaiveTigerCompiler.Translate;
namespace NaiveTigerCompiler
{
    namespace Semantics
    {
        public class Semantics
        {
            Environment Env;
            Level Level;
            Translate.Translate Translate;
            static int Count = 1;

            public Semantics(Frame.Frame frame)
            {
                Translate = new Translate.Translate(frame);
                Level = new Level(frame);
                Env = new Environment(Level);
            }

            public Frag TranslateProgram(Expression e)
            {
                Level = new Level(Level, new Label(SymbolTable.Symbol.GetSymbol("main")), null);
                ExpressionType et = TranslateExpression(e);
                Translate.ProcessEntryExit(Level, et.Exp, false);
                Level = Level.Parent;
                return Translate.GetResult();
            }

            Exp CheckInteger(Position pos, ExpressionType et)
            {
                if (!et.Type.CoerceTo(Types.Type._int))
                {
                    Error.Report(pos, "Integer required");
                }
                return et.Exp;
            }

            #region Translate Variable
            ExpressionType TranslateVariable(Variable var)
            {
                if (var is SimpleVariable)
                    return TranslateVariable(var as SimpleVariable);
                if (var is FieldVariable)
                    return TranslateVariable(var as FieldVariable);
                if (var is SubscriptVariable)
                    return TranslateVariable(var as SubscriptVariable);

                throw new FatalError(var.Pos, "Unknown variable");
            }

            ExpressionType TranslateVariable(SimpleVariable var)
            {
                Entry ent = Env.ValueEnvironment[var.Name] as Entry;
                if (ent is VariableEntry)
                {
                    VariableEntry vent = ent as VariableEntry;
                    return new ExpressionType(Translate.TranslateSimpleVar(vent.Access, Level), vent.Type);
                }
                else
                {
                    Error.Report(var.Pos, "Undefined variable");
                    return new ExpressionType(null, Types.Type._unknown);
                }
            }

            ExpressionType TranslateVariable(FieldVariable field)
            {
                ExpressionType et = TranslateVariable(field.Var);
                if (et.Type.Actual is Types.RECORD)
                {
                    int index = 0;
                    Types.RECORD type = et.Type.Actual as Types.RECORD;
                    for (; type != null; type = type.Tail, ++index)
                    {
                        if (type.FieldName == field.Field)
                            break;
                    }
                    if (type != null)
                    {
                        return new ExpressionType(Translate.TranslateFieldVar(et.Exp, index), type.FieldType.Actual);
                    }
                    else
                    {
                        Error.Report(field.Pos, "Field '" + field.Field.ToString() + "' does not exist.");
                        return new ExpressionType(null, Types.Type._unknown);
                    }
                }
                else
                {
                    Error.Report(field.Pos, "Record type required");
                    return new ExpressionType(null, Types.Type._unknown);
                }

            }

            ExpressionType TranslateVariable(SubscriptVariable sub)
            {
                ExpressionType var = TranslateVariable(sub.Var);
                if (var.Type.Actual is Types.ARRAY)
                {
                    ExpressionType index = TranslateExpression(sub.Index);
                    CheckInteger(sub.Index.Pos, index);
                    return new ExpressionType(Translate.TranslateSubscriptVar(var.Exp, index.Exp), (var.Type.Actual as Types.ARRAY).Element.Actual);
                }
                else
                {
                    Error.Report(sub.Pos, "Array type required");
                    return new ExpressionType(null, Types.Type._unknown);
                }
            }
            #endregion

            #region Translate Expression
            ExpressionType TranslateExpression(Expression e)
            {
                if (e is ArrayExpression)
                    return TranslateExpression(e as ArrayExpression);
                if (e is AssignExpression)
                    return TranslateExpression(e as AssignExpression);
                if (e is BreakExpression)
                    return TranslateExpression(e as BreakExpression);
                if (e is CallExpression)
                    return TranslateExpression(e as CallExpression);
                if (e is ForExpression)
                    return TranslateExpression(e as ForExpression);
                if (e is IfExpression)
                    return TranslateExpression(e as IfExpression);
                if (e is IntegerExpression)
                    return TranslateExpression(e as IntegerExpression);
                if (e is LetExpression)
                    return TranslateExpression(e as LetExpression);
                if (e is NilExpression)
                    return TranslateExpression(e as NilExpression);
                if (e is OperatorExpression)
                    return TranslateExpression(e as OperatorExpression);
                if (e is RecordExpression)
                    return TranslateExpression(e as RecordExpression);
                if (e is SequenceExpression)
                    return TranslateExpression(e as SequenceExpression);
                if (e is StringExpression)
                    return TranslateExpression(e as StringExpression);
                if (e is VariableExpression)
                    return TranslateExpression(e as VariableExpression);
                if (e is WhileExpression)
                    return TranslateExpression(e as WhileExpression);

                Error.Report(e.Pos, "Unknown expression");
                return null;
            }

            ExpressionType TranslateExpression(ArrayExpression e)
            {
                Types.Type type = Env.TypeEnvironment[e.Type] as Types.Type;
                if (type == null)
                {
                    Error.Report(e.Pos, "Undefined array type");
                    return new ExpressionType(null, Types.Type._unknown);
                }
                type = type.Actual;
                if (!(type is Types.ARRAY))
                {
                    Error.Report(e.Pos, "Array type required");
                    return new ExpressionType(null, Types.Type._unknown);
                }
                ExpressionType size = TranslateExpression(e.Size);
                CheckInteger(e.Size.Pos, size);

                ExpressionType init = TranslateExpression(e.Init);
                if (!init.Type.CoerceTo((type as Types.ARRAY).Element.Actual))
                {
                    Error.Report(e.Init.Pos, "Incorrect initialized type");
                    return new ExpressionType(null, Types.Type._unknown);
                }
                if (init.Type.CoerceTo(Types.Type._int))
                    return new ExpressionType(Translate.TranslateArrayExp(Level, init.Exp, size.Exp), type);
                else
                    return new ExpressionType(Translate.TranslateMultiArrayExp(Level, init.Exp, size.Exp), type);
            }

            ExpressionType TranslateExpression(AssignExpression e)
            {
                ExpressionType lvalue = TranslateVariable(e.Var);
                ExpressionType exp = TranslateExpression(e.Exp);
                if (e.Var is SimpleVariable && Env.ValueEnvironment[(e.Var as SimpleVariable).Name] is LoopVariableEntry)
                {
                    Error.Report(e.Var.Pos, "Loop variable cannot be assigned");
                    return new ExpressionType(null, Types.Type._void);
                }
                if (exp.Type.CoerceTo(Types.Type._void))
                {
                    Error.Report(e.Exp.Pos, "Cannot assign a 'void'");
                    return new ExpressionType(null, Types.Type._void);
                }
                if (!exp.Type.CoerceTo(lvalue.Type))
                {
                    Error.Report(e.Pos, "Cannot assign a '" + exp.Type.ToString() + "' to '" + lvalue.Type.ToString() + "'");
                    return new ExpressionType(null, Types.Type._void);
                }
                return new ExpressionType(Translate.TranslateAssignExp(lvalue.Exp, exp.Exp), Types.Type._void);
            }

            ExpressionType TranslateExpression(BreakExpression e)
            {
                if (!Env.LoopEnvironment.InLoop())
                {
                    Error.Report(e.Pos, "No loop to break");
                    return new ExpressionType(null, Types.Type._void);
                }
                return new ExpressionType(Translate.TranslateBreakExp(Env.LoopEnvironment.Done()), Types.Type._void);
            }

            ExpressionType TranslateExpression(CallExpression e)
            {
                Entry func = Env.ValueEnvironment[e.Func] as Entry;
                if (func == null || !(func is FunctionEntry))
                {
                    Error.Report(e.Pos, "Undefined function '" + e.Func.ToString() + "'");
                    return new ExpressionType(null, Types.Type._void);
                }

                Types.RECORD formals = (func as FunctionEntry).Formals;
                List<Exp> list = new List<Exp>();
                for (ExpressionList args = e.Args; args != null; args = args.Tail, formals = formals.Tail)
                {
                    if (formals == null)
                    {
                        Error.Report(e.Pos, "Too much parameters in call '" + e.Func.ToString() + "'");
                        return new ExpressionType(null, (func as FunctionEntry).Result);
                    }
                    ExpressionType et = TranslateExpression(args.Head);
                    if (!et.Type.CoerceTo(formals.FieldType))
                    {
                        Error.Report(args.Head.Pos, "Type mismatch for parameter '" + formals.FieldName.ToString() + "'");
                        return new ExpressionType(null, (func as FunctionEntry).Result);
                    }
                    list.Add(et.Exp);
                }
                if (formals != null)
                {
                    Error.Report(e.Pos, "Too few parameters in call '" + e.Func.ToString() + "'");
                }
                return new ExpressionType(Translate.TranslateCallExp(Level, (func as FunctionEntry).Level, (func as FunctionEntry).Label, list), (func as FunctionEntry).Result.Actual);
            }

            ExpressionType TranslateExpression(ForExpression e)
            {
                Env.ValueEnvironment.BeginScope();
                ExpressionType init = TranslateExpression(e.VarDec.Init);
                if (!init.Type.CoerceTo(Types.Type._int))
                {
                    Error.Report(e.VarDec.Pos, "Loop variable must be integer");
                }
                Access access = Level.AllocLocal(e.VarDec.Escape);
                Env.ValueEnvironment[e.VarDec.Name] = new LoopVariableEntry(access, init.Type);
                ExpressionType high = TranslateExpression(e.High);
                CheckInteger(e.High.Pos, high);
                Env.LoopEnvironment.EnterLoop();
                ExpressionType body = TranslateExpression(e.Body);
                if (!body.Type.CoerceTo(Types.Type._void))
                {
                    Error.Report(e.Body.Pos, "For expression must return a 'void'");
                }
                Exp exp = Translate.TranslateForExp(Level, access, init.Exp, high.Exp, body.Exp, Env.LoopEnvironment.Done());
                Env.LoopEnvironment.ExitLoop();
                Env.ValueEnvironment.EndScope();
                return new ExpressionType(exp, Types.Type._void);
            }

            Types.Type CheckIfType(Position pos, ExpressionType et1, ExpressionType et2)
            {
                if (et1.Type.CoerceTo(Types.Type._nil) && et2.Type.Actual is Types.RECORD)
                    return et2.Type;
                if (et2.Type.CoerceTo(Types.Type._nil) && et1.Type.Actual is Types.RECORD)
                    return et1.Type;
                if (et1.Type.CoerceTo(et2.Type))
                    return et1.Type;
                Error.Report(pos, "Type mismatch in if expression");
                return null;
            }

            ExpressionType TranslateExpression(IfExpression e)
            {
                Types.Type temp;
                ExpressionType test = TranslateExpression(e.Test);
                ExpressionType then = TranslateExpression(e.Then);
                ExpressionType els = e.Else == null ? null : TranslateExpression(e.Else);
                CheckInteger(e.Test.Pos, test);
                if (els != null)
                {
                    if ((temp = CheckIfType(e.Pos, then, els)) != null)
                        return new ExpressionType(Translate.TranslateIfThenElseExp(test.Exp, then.Exp, els.Exp), temp);
                    else
                    {
                        Error.Report(e.Pos, "Type mismatch in if expression");
                        return new ExpressionType(null, Types.Type._void);
                    }
                }
                else
                {
                    if (then.Type.CoerceTo(Types.Type._void))
                        return new ExpressionType(Translate.TranslateIfThenElseExp(test.Exp, then.Exp, null), Types.Type._void);
                    else
                    {
                        Error.Report(e.Pos, "Then clause must return 'void'");
                        return new ExpressionType(null, Types.Type._void);
                    }
                }
            }

            ExpressionType TranslateExpression(IntegerExpression e)
            {
                return new ExpressionType(Translate.TranslateIntExp(e.Value), Types.Type._int);
            }

            ExpressionType TranslateExpression(LetExpression e)
            {
                Env.ValueEnvironment.BeginScope();
                Env.TypeEnvironment.BeginScope();
                ExpList eDec = null, p = null;
                for (DeclarationList ptr = e.Decs; ptr != null; ptr = ptr.Tail)
                {
                    if (eDec == null)
                        p = eDec = new ExpList(TranslateDeclaration(ptr.Head), null);
                    else
                        p = p.Tail = new ExpList(TranslateDeclaration(ptr.Head), null);
                }
                ExpressionType eBody = TranslateExpression(e.Body);
                Env.ValueEnvironment.EndScope();
                Env.TypeEnvironment.EndScope();
                return new ExpressionType(Translate.TranslateLetExp(eDec, eBody.Exp, eBody.Type.CoerceTo(Types.Type._void)), eBody.Type);
            }

            ExpressionType TranslateExpression(NilExpression e)
            {
                return new ExpressionType(Translate.TranslateNilExp(), Types.Type._nil);
            }

            Types.Type checkEqualType(Position pos, ExpressionType et1, ExpressionType et2)
            {
                if (et1.Type.CoerceTo(et2.Type)
                    && (et1.Type.CoerceTo(Types.Type._int) || et1.Type.CoerceTo(Types.Type._string)
                        || et1.Type.Actual is Types.RECORD || et1.Type.Actual is Types.ARRAY))
                    return et1.Type;
                if (et1.Type.CoerceTo(Types.Type._nil) && et2.Type.Actual is Types.RECORD)
                    return et2.Type;
                if (et2.Type.CoerceTo(Types.Type._nil) && et1.Type.Actual is Types.RECORD)
                    return et1.Type;
                Error.Report(pos, "Type mismatch in relational expression");
                return Types.Type._int;
            }

            Types.Type CheckCmpType(Position pos, ExpressionType et1, ExpressionType et2)
            {
                if (et1.Type.CoerceTo(Types.Type._int) && et2.Type.CoerceTo(Types.Type._int))
                    return et1.Type;
                if (et1.Type.CoerceTo(Types.Type._string) && et2.Type.CoerceTo(Types.Type._string))
                    return et1.Type;
                Error.Report(pos, "Type mismatch in relational expression");
                return Types.Type._int;
            }

            ExpressionType TranslateExpression(OperatorExpression e)
            {
                ExpressionType eLeft = TranslateExpression(e.Left);
                ExpressionType eRight = TranslateExpression(e.Right);

                if (e.Op == Operator.Plus || e.Op == Operator.Minus || e.Op == Operator.Times || e.Op == Operator.Divide)
                {
                    CheckInteger(e.Left.Pos, eLeft);
                    CheckInteger(e.Right.Pos, eRight);
                    return new ExpressionType(Translate.TranslateCalculateExp((Tree.BINOP.Op)e.Op, eLeft.Exp, eRight.Exp), Types.Type._int);
                }
                else if (e.Op == Operator.Equal || e.Op == Operator.NotEqual)
                {
                    Types.Type ty = checkEqualType(e.Pos, eLeft, eRight);
                    if (ty.CoerceTo(Types.Type._string))
                        return new ExpressionType(Translate.TranslateStringRelExp(ConvertOp(e.Op), eLeft.Exp, eRight.Exp), Types.Type._int);
                    else
                        return new ExpressionType(Translate.TranslateRelExp(ConvertOp(e.Op), eLeft.Exp, eRight.Exp), Types.Type._int);
                }
                else
                {
                    Types.Type ty = CheckCmpType(e.Pos, eLeft, eRight);
                    if (ty.CoerceTo(Types.Type._string))
                        return new ExpressionType(Translate.TranslateStringRelExp(ConvertOp(e.Op), eLeft.Exp, eRight.Exp), Types.Type._int);
                    else
                        return new ExpressionType(Translate.TranslateRelExp(ConvertOp(e.Op), eLeft.Exp, eRight.Exp), Types.Type._int);
                }
            }


            private Tree.CJUMP.Rel ConvertOp(Operator op)
            {
                switch (op)
                {
                    case Operator.Equal: return Tree.CJUMP.Rel.Equal;
                    case Operator.NotEqual: return Tree.CJUMP.Rel.NotEqual;
                    case Operator.LessThan: return Tree.CJUMP.Rel.LessThan;
                    case Operator.LessEqual: return Tree.CJUMP.Rel.LessEqual;
                    case Operator.GreaterThan: return Tree.CJUMP.Rel.GreaterThan;
                    case Operator.GreaterEqual: return Tree.CJUMP.Rel.GreaterEqual;
                    default: throw new FatalError("Cannot convert a operator");
                }
            }

            ExpressionType TranslateExpression(RecordExpression e)
            {
                Types.Type eType = Env.TypeEnvironment[e.Type] as Types.Type;
                if (eType == null)
                {
                    Error.Report(e.Pos, "Undefined record type " + e.Type.ToString());
                    return new ExpressionType(null, Types.Type._int);
                }

                eType = eType.Actual;
                if (!(eType is Types.RECORD))
                {
                    Error.Report(e.Pos, "Record type required");
                    return new ExpressionType(null, Types.Type._int);
                }

                FieldExpressionList eFields = e.Fields;
                Types.RECORD eRecord = (Types.RECORD)eType;
                ExpressionType et;

                List<Exp> fieldList = new List<Exp>();
                for (; eFields != null; eFields = eFields.Tail, eRecord = eRecord.Tail)
                {
                    if (eRecord == null)
                    {
                        Error.Report(eFields.Pos,
                                "Field " + eFields.Name.ToString() + " has not been declared");
                        break;
                    }
                    if (eRecord.FieldName != eFields.Name)
                    {
                        Error.Report(eFields.Pos,
                                eRecord.FieldName.ToString() + " field dismatch");
                        break;
                    }
                    et = TranslateExpression(eFields.Init);
                    fieldList.Add(et.Exp);
                    if (!et.Type.CoerceTo(eRecord.FieldType))
                        Error.Report(eFields.Pos, "Type mismatch in record field");
                }

                if (eRecord != null)
                    Error.Report(eFields.Pos, "Missing record fields");
                return new ExpressionType(Translate.TranslateRecordExp(Level, fieldList), eType);
            }

            ExpressionType TranslateExpression(SequenceExpression e)
            {
                Types.Type type = Types.Type._void;
                ExpressionType et;
                ExpList el = null, ptr = null;
                for (ExpressionList h = e.Exps; h != null; h = h.Tail)
                {
                    et = TranslateExpression(h.Head);
                    type = et.Type;
                    if (el == null)
                        el = ptr = new ExpList(et.Exp, null);
                    else
                        ptr = ptr.Tail = new ExpList(et.Exp, null);
                }
                return new ExpressionType(Translate.TranslateSeqExp(el, type.CoerceTo(Types.Type._void)), type);

            }

            ExpressionType TranslateExpression(StringExpression e)
            {
                return new ExpressionType(Translate.TranslateStringExp(e.Value), Types.Type._string);
            }


            ExpressionType TranslateExpression(VariableExpression e)
            {
                return TranslateVariable(e.Var);
            }


            ExpressionType TranslateExpression(WhileExpression e)
            {
                ExpressionType eTest = TranslateExpression(e.Test);
                CheckInteger(e.Test.Pos, eTest);
                Env.LoopEnvironment.EnterLoop();
                ExpressionType body = TranslateExpression(e.Body);
                if (body.Type != Types.Type._void)
                    Error.Report(e.Body.Pos, "While expression should return void");
                Exp eWhile = Translate.TranslateWhileExp(eTest.Exp, body.Exp, Env.LoopEnvironment.Done());
                Env.LoopEnvironment.ExitLoop();
                return new ExpressionType(eWhile, Types.Type._void);
            }
            #endregion

            #region Translate Declaration
            Exp TranslateDeclaration(Declaration dec)
            {
                if (dec is VariableDeclaration)
                    return TranslateDeclaration(dec as VariableDeclaration);
                if (dec is FunctionDeclaration)
                    return TranslateDeclaration(dec as FunctionDeclaration);
                if (dec is TypeDeclaration)
                    return TranslateDeclaration(dec as TypeDeclaration);
                Error.Report(dec.Pos, "Unknown declaration");
                return null;
            }

            Exp TranslateDeclaration(VariableDeclaration dec)
            {
                ExpressionType init = TranslateExpression(dec.Init);
                Types.Type type = null;
                if (dec.Type == null)
                {
                    if (init.Type.CoerceTo(Types.Type._nil))
                    {
                        Error.Report(dec.Init.Pos, "Unknown type cannot be initialized with 'nil'");
                        return null;
                    }
                    else
                    {
                        type = init.Type;
                    }
                }
                else
                {
                    type = TranslateType(dec.Type).Actual;
                    if (!init.Type.CoerceTo(type))
                    {
                        Error.Report(dec.Init.Pos, "Type mismatch in variable initialization");
                        return null;
                    }
                }
                VariableEntry var = new VariableEntry(Level.AllocLocal(dec.Escape), type);
                Env.ValueEnvironment[dec.Name] = var;
                return Translate.TranslateAssignExp(Translate.TranslateSimpleVar(var.Access, Level), init.Exp);
            }

            Exp TranslateDeclaration(TypeDeclaration dec)
            {
                List<SymbolTable.Symbol> list = new List<SymbolTable.Symbol>();
                for (TypeDeclaration d = dec; d != null; d = d.Next)
                {
                    if (list.Contains(d.Name))
                    {
                        Error.Report(d.Pos, "Type redefined in a sequence");
                        return null;
                    }
                    else
                    {
                        list.Add(d.Name);
                        Env.TypeEnvironment[d.Name] = new Types.NAME(d.Name);
                    }
                }
                for (TypeDeclaration d = dec; d != null; d = d.Next)
                {
                    (Env.TypeEnvironment[d.Name] as Types.NAME).Bind(TranslateType(d.Type));
                }
                for (TypeDeclaration d = dec; d != null; d = d.Next)
                {
                    if ((Env.TypeEnvironment[d.Name] as Types.NAME).isLoop())
                    {
                        Error.Report(d.Pos, "Type circular definition");
                    }
                }
                return Translate.TranslateNoOp();
            }

            Exp TranslateDeclaration(FunctionDeclaration dec)
            {
                List<SymbolTable.Symbol> list = new List<SymbolTable.Symbol>();
                for (FunctionDeclaration d = dec; d != null; d = d.Next)
                {
                    if (Env.ValueEnvironment[d.Name] != null && Env.ValueEnvironment[d.Name] is StandardFunctionEntry)
                    {
                        Error.Report(d.Pos, "Function in standard libaray cannot be redefined");
                    }
                    else if (list.Contains(d.Name))
                    {
                        Error.Report(d.Pos, "Function cannot be redefined in a sequence");
                    }
                    else
                    {
                        list.Add(d.Name);
                        Types.Type result = d.Result == null ? Types.Type._void : TranslateType(d.Result).Actual;
                        Types.RECORD formals = TranslateTypeFields(d.Param);
                        Label label = new Label(d.Name + "_" + Count++.ToString());
                        Translate.Level level = new Translate.Level(Level, label, BoolList.BuildFromFieldList(d.Param));
                        Env.ValueEnvironment[d.Name] = new FunctionEntry(level, label, formals, result);
                    }
                }
                for (FunctionDeclaration d = dec; d != null; d = d.Next)
                {
                    FunctionEntry function = Env.ValueEnvironment[d.Name] as FunctionEntry;
                    Env.ValueEnvironment.BeginScope();
                    Env.LoopEnvironment.BeginScope();
                    Translate.Level backup = Level;
                    Level = function.Level;
                    Translate.AccessList al = Level.Formals.Tail;
                    for (FieldList field = d.Param; field != null; field = field.Tail, al = al.Tail)
                    {
                        Types.Type type = Env.TypeEnvironment[field.Type] as Types.Type;
                        if (type == null)
                        {
                            Error.Report(field.Pos, "Undefined type '" + field.Name + "'");
                        }
                        else
                        {
                            Translate.Access access = new Translate.Access(Level, al.Head.Acc);
                            Env.ValueEnvironment[field.Name] = new VariableEntry(access, type.Actual);
                        }
                    }
                    ExpressionType et = TranslateExpression(d.Body);
                    Translate.ProcessEntryExit(Level, et.Exp, !(et.Type.CoerceTo(Types.Type._void)));
                    if (!et.Type.CoerceTo((Env.ValueEnvironment[d.Name] as FunctionEntry).Result))
                    {
                        Error.Report(d.Result != null ? d.Result.Pos : d.Body.Pos, "Type mismatched for function return value");
                    }
                    Env.ValueEnvironment.EndScope();
                    Env.LoopEnvironment.EndScope();
                    Level = backup;
                }
                return Translate.TranslateNoOp();
            }
            #endregion

            #region TranslateType
            Types.Type TranslateType(Type t)
            {
                if (t is NameType)
                    return TranslateType(t as NameType);
                if (t is RecordType)
                    return TranslateType(t as RecordType);
                if (t is ArrayType)
                    return TranslateType(t as ArrayType);
                throw new FatalError(t.Pos, "Unknown type");
            }

            Types.Type TranslateType(NameType t)
            {
                Types.Type result = Env.TypeEnvironment[t.Name] as Types.Type;
                if (result == null)
                {
                    Error.Report(t.Pos, "Undefined type '" + t.Name.ToString() + "'");
                    return Types.Type._int;
                }
                else
                {
                    return result;
                }
            }

            Types.Type TranslateType(RecordType t)
            {
                Types.RECORD result = null, p = null;
                Types.Type type;
                List<SymbolTable.Symbol> list = new List<SymbolTable.Symbol>();
                for (FieldList field = t.Fields; field != null; field = field.Tail)
                {
                    type = Env.TypeEnvironment[field.Type] as Types.Type;
                    if (type == null)
                    {
                        Error.Report(field.Pos, "Undefined type '" + field.Type.ToString() + "'");
                        return null;
                    }
                    if (list.Contains(field.Name))
                        Error.Report(field.Pos, "Redefined field name " + field.Name.ToString() + "'");
                    else
                        list.Add(field.Name);
                    if (p == null)
                        result = p = new Types.RECORD(field.Name, type, null);
                    else
                        p = p.Tail = new Types.RECORD(field.Name, type, null);
                }

                return result;
            }

            Types.Type TranslateType(ArrayType t)
            {
                Types.Type type = Env.TypeEnvironment[t.Type] as Types.Type;
                if (type == null)
                {
                    Error.Report(t.Pos, "Undefined type '" + t.Type.ToString() + "'");
                    return Types.Type._int;
                }
                else
                {
                    return new Types.ARRAY(type.Actual);
                }
            }
            #endregion

            Types.RECORD TranslateTypeFields(FieldList field)
            {
                Types.RECORD type = null, result = null;
                for (FieldList f = field; f != null; f = f.Tail)
                {
                    Types.Type t = Env.TypeEnvironment[f.Type] as Types.Type;
                    t = t.Actual;

                    if (t == null)
                    {
                        Error.Report(f.Pos, "Undefined type '" + f.Type.ToString() + "'");
                    }
                    else if (type == null)
                    {
                        result = type = new Types.RECORD(f.Name, t, null);
                    }
                    else
                    {
                        type.Tail = new Types.RECORD(f.Name, t, null);
                        type = type.Tail;
                    }
                }
                return result;
            }
        }
    }
}