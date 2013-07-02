using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace System.Text.Parse.Grammar
{
    public abstract class CompilerCompilerCodeProvider
    {
        public abstract string StatckInit();
        public abstract string StackPop();
        public abstract string StackPush(string item);
        public abstract string LoopWhileBegin(string condition);
        public abstract string LoopWhileEnd();
        public abstract string LabelBlockBegin();
        public abstract string LabelBlockEnd();
        public abstract string LabelBegin(string labelName);
        public abstract string LabelEnd();
        public abstract string SwitchBegin(string variableName);
        public abstract string SwitchEnd();
        public abstract string SwitchLabelBegin(string labelName);
        public abstract string SwitchLabelEnd();
        public abstract string TrueValue
        {
            get;
        }
    }

    public class CompilerCompilerCSharpCodeProvider : CompilerCompilerCodeProvider
    {

        public override string StatckInit()
        {
            return "Stack<int> stack = new Stack<int>();";
        }

        public override string StackPop()
        {
            return "stack.Pop();";
        }

        public override string StackPush(string item)
        {
            return string.Concat("stack.Push(", item, ");");
        }

        public override string LoopWhileBegin(string condition)
        {
            return string.Concat("while(", condition, "){");
        }

        public override string LoopWhileEnd()
        {
            return "}";
        }

        public override string LabelBlockBegin()
        {
            return string.Empty;
        }

        public override string LabelBlockEnd()
        {
            return string.Empty;
        }

        public override string LabelBegin(string labelName)
        {
            return string.Concat(labelName, ":");
        }

        public override string LabelEnd()
        {
            return string.Empty;
        }

        public override string SwitchBegin(string variableName)
        {
            return string.Concat("switch(", variableName, "){");
        }

        public override string SwitchEnd()
        {
            return "}";
        }

        public override string SwitchLabelBegin(string labelName)
        {
            return string.Concat("case \"", labelName, "\":");
        }

        public override string SwitchLabelEnd()
        {
            throw new NotImplementedException();
        }

        public override string TrueValue
        {
            get { throw new NotImplementedException(); }
        }
    }

    public abstract class CompilerCompiler
    {
        public virtual string LexName
        {
            get { return "lex"; }
        }

        public virtual string ReaderName
        {
            get { return "reader"; }
        }

        public virtual string GotoTableName
        {
            get { return "gotoTable"; }
        }

        public virtual string ActionTableName
        {
            get { return "actionTable"; }
        }

        public virtual string GrammarRuleTableName
        {
            get { return "grammarRuleTable"; }
        }

        public virtual string ReduceActionsName
        {
            get { return "reduceActions"; }
        }

        public virtual string PropertyType
        {
            get { return "string"; }
        }

        public virtual string PropertyName
        {
            get { return "property"; }
        }

        public abstract string CreateAction(Action action);
        public abstract void CreateGrammarType(TextWriter writer, Grammar grammar);
        public abstract void CreateActionTable(TextWriter writer, Grammar grammar, string variableName);
        public abstract void CreateGotoTable(TextWriter writer, Grammar grammar, string variableName);
        public abstract void CreateGrammarRuleTable(TextWriter writer, Grammar grammar, string variableName);
        public abstract void CreateReduceActions(TextWriter writer, Grammar grammar, string propertyName);
        public abstract void CreateSymbols(TextWriter writer, Grammar grammar);
        public abstract void CreateLex(TextWriter writer, string lexName, string readerName);

        public abstract void CreateCompiler(TextWriter writer, Grammar grammar, CompilerCompilerCodeProvider codeProvider);
    }

    public class CSharpCompilerCompiler : CompilerCompiler
    {
        public override string CreateAction(Action action)
        {
            switch (action.Type)
            {
                case ActionType.ShiftAction:
                    {
                        ShiftAction shiftAction = (ShiftAction)action;
                        return string.Format("new ShiftAction({0})", shiftAction.NextState);
                    }
                case ActionType.AcceptAction:
                    return "new AcceptAction()";
                case ActionType.ReduceAction:
                    {
                        ReduceAction reduceAction = (ReduceAction)action;
                        return string.Format("new ReduceAction({0})", reduceAction.Rule);
                    }
                case ActionType.ErrorAction:
                    return "new ErrorAction()";
                default:
                    throw new ArgumentException();
            }
        }

        public override void CreateGrammarType(TextWriter writer, Grammar grammar)
        {
        }

        public override void CreateReduceActions(TextWriter writer, Grammar grammar, string propertyName)
        {
            string[] actions = grammar.ReduceActions;
            int[][] grammarRuleTable = grammar.GrammarRuleTable;

            for (int i = 0; i < actions.Length; i++)
            {
                string action = actions[i];
                int[] grammarRule = grammarRuleTable[i];
                for (int j = grammarRule.Length - 1; i > 0; i--)
                {
                    action = action.Replace("$" + j.ToString(), string.Format("{0}[{1}]", propertyName, j));
                }
                action = action.Replace("$$", string.Format("{0}[0]", propertyName));
                action = actions[i].Replace("\"", "\\\"").Replace(@"\", @"\\");

                writer.WriteLine(string.Concat(@"case ", i.ToString(), @":
        ", action, @"
        break;"));
            }
        }

        public override void CreateSymbols(TextWriter writer, Grammar grammar)
        {
            throw new NotImplementedException();
        }

        public void CreateArray<T>(TextWriter writer, T[][] array, string arrayName) where T : struct
        {
            writer.WriteLine("{0}[][] {1} = new {0}[][]{", typeof(T).Name, arrayName);
            for (int i = 0; i < array.Length; i++)
            {
                writer.Write("new {0}[]{", typeof(T).Name);
                for (int j = 0; j < array[i].Length; j++)
                {
                    writer.Write(array[i][j]);
                    writer.Write(',');
                }
                writer.Write("},");
                writer.WriteLine();
            }
            writer.Write("};");
        }

        public override void CreateActionTable(TextWriter writer, Grammar grammar, string variableName)
        {
            Action[][] actionTable = grammar.ActionTable;
            writer.WriteLine("Action[][] {0} = new Action[][]{", variableName);
            for (int i = 0; i < actionTable.Length; i++)
            {
                writer.WriteLine("new Action[]{");

                for (int j = 0; j < actionTable[i].Length; j++)
                {
                    writer.Write(CreateAction(actionTable[i][j]));
                    writer.Write(',');
                }

                writer.Write("},");
            }
            writer.WriteLine("};");
        }

        public override void CreateGrammarRuleTable(TextWriter writer, Grammar grammar, string variableName)
        {
            CreateArray(writer, grammar.GrammarRuleTable, variableName);
        }

        public override void CreateGotoTable(TextWriter writer, Grammar grammar, string variableName)
        {
            CreateArray(writer, grammar.GotoTable, variableName);
        }

        public override void CreateLex(TextWriter writer, string lexName, string readerName)
        {
            writer.WriteLine("Lex {0} = new Lex({1});", lexName, readerName);
        }

        public override void CreateCompiler(TextWriter writer, Grammar grammar, CompilerCompilerCodeProvider codeProvider)
        {
            CreateGrammarType(writer, grammar);
            CreateLex(writer, LexName, ReaderName);
            CreateGrammarRuleTable(writer, grammar, GrammarRuleTableName);
            CreateActionTable(writer, grammar, ActionTableName);
            CreateGotoTable(writer, grammar, GotoTableName);

            CreateParser(writer, grammar, LexName, GrammarRuleTableName, ReduceActionsName, PropertyType, PropertyName, ActionTableName, GotoTableName);
        }

        public virtual void CreateParser(
            TextWriter writer,
            Grammar grammar,
            string lexName,
            string grammarRuleTableName,
            string reduceActionsName,
            string propertyType,
            string propertyName,
            string actionTableName,
            string gotoTableName)
        {
            writer.WriteLine(string.Concat(@"
            Stack<StateSymbol> stack = new Stack<StateSymbol>();
            Stack<", propertyType, @"> propertyStack = new Stack<", propertyType, @">();
            stack.Push(new StateSymbol(0, Terminal.End.Id));
            while (stack.Count > 0)
            {
                StateSymbol currentState = stack.Pop();
                int token = ", lexName, @".Read();
                Action action = ", actionTableName, @"[currentState.State][token];
                switch (action.Type)
                {
                    case ActionType.AcceptAction:
                        return;
                    case ActionType.ErrorAction:
                        throw new Exception();
                    case ActionType.ShiftAction:
                        {
                            ShiftAction shiftAction = (ShiftAction)action;
                            stack.Push(new StateSymbol(shiftAction.NextState, token));
                        }
                        break;
                    case ActionType.ReduceAction:
                        {
                            ReduceAction reduceAction = (ReduceAction)action;
                            int[] grammarRule = ", grammarRuleTableName, @"[reduceAction.Rule];
                            ", propertyType, @"[] ", propertyName, @" = new ", propertyType, @"[grammarRule.Length];
                            for (int i = grammarRule.Length - 1; i > 0; i--)
                            {
                                stack.Pop();
                                ", propertyName, @"[i] = propertyStack.Pop();
                            }
                            ", propertyName, @"[0] = new ", propertyType, @"();
                            switch(reduceAction.Rule)
                            {"));

            CreateReduceActions(writer, grammar, propertyName);
            writer.WriteLine(@"}
                            stack.Push(
                                new StateSymbol(
                                    gotoTable[stack.Peek().State][grammarRule[0]],
                                    token));
                            propertyStack.Push(", propertyName, @"[0]);
                        }
                        break;
                    default:
                        break;
                }
            }");
        }
    }
}
