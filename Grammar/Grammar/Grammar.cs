using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Text.Parse.Grammar
{
    public enum SymbolType
    {
        Terminal,
        NonTerminal
    }

    public abstract class Symbol
    {

        public abstract SymbolType Type
        {
            get;
        }

        internal int id = -1;
        public int Id
        {
            get { return id; }
        }

        public Symbol()
        {
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj is Symbol)
            {
                Symbol tk = (Symbol)obj;
                return this.id == tk.id && this.Type == tk.Type;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Type.GetHashCode() ^ this.id.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0}){1}", this.Id, this.Type);
        }
    }

    public class Terminal : Symbol
    {
        public override SymbolType Type
        {
            get
            {
                return SymbolType.Terminal;
            }
        }

        private string value;
        public string Value
        {
            get { return value; }
        }

        private static Terminal end = new Terminal("\0");
        public static Terminal End
        {
            get { return end; }
        }

        private static Terminal empty = new Terminal("");
        public static Terminal Empty
        {
            get { return empty; }
        }

        public Terminal(string value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj is Terminal)
            {
                Terminal tm = (Terminal)obj;
                return base.Equals(obj) && (this.value == tm.value);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ this.value.GetHashCode();
        }

        public override string ToString()
        {
            if (this.Value.Equals(""))
            {
                return "''";
            }
            return "'" + this.value.Replace("\n", "\\n").Replace("\t", "\\t").Replace("\0", "\\0") + "'";
        }
    }

    public class TerminalSet : HashSet<Terminal>
    {
        public void Merge(TerminalSet set)
        {
            foreach (Terminal tm in set)
            {
                this.Add(tm);
            }
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj is TerminalSet)
            {
                TerminalSet tset = (TerminalSet)obj;
                return this.SetEquals(tset);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (Terminal tm in this)
            {
                hash ^= tm.GetHashCode();
            }
            return hash;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Terminal tm in this)
            {
                sb.Append(tm.ToString());
                sb.Append(",");
            }
            if (sb.Length > 0)
            {
                sb.Length--;
            }
            return sb.ToString();
        }
    }

    public class NonTerminal : Symbol
    {
        public override SymbolType Type
        {
            get
            {
                return SymbolType.NonTerminal;
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
        }

        public NonTerminal(string name)
        {
            this.name = name;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ this.name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj is NonTerminal)
            {
                NonTerminal ntm = (NonTerminal)obj;
                return base.Equals(obj) && (this.name == ntm.name);
            }
            return false;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class StateSymbol
    {
        private int state;

        public int State
        {
            get { return state; }
        }
        private int symbol;

        public int Symbol
        {
            get { return symbol; }
        }

        public StateSymbol(int state, int symbol)
        {
            this.state = state;
            this.symbol = symbol;
        }
    }

    public enum ActionType
    {
        ShiftAction,
        ReduceAction,
        AcceptAction,
        ErrorAction
    }

    public abstract class Action
    {
        public abstract ActionType Type
        {
            get;
        }
    }

    public class ShiftAction : Action
    {
        public override ActionType Type
        {
            get { return ActionType.ShiftAction; }
        }

        private int nextState;
        public int NextState
        {
            get { return nextState; }
        }

        public ShiftAction(int nextState)
        {
            this.nextState = nextState;
        }
    }

    public class AcceptAction : Action
    {
        public override ActionType Type
        {
            get { return ActionType.AcceptAction; }
        }
    }

    public class ReduceAction : Action
    {
        public override ActionType Type
        {
            get { return ActionType.ReduceAction; }
        }

        private int rule;
        public int Rule
        {
            get { return rule; }
        }

        public ReduceAction(int rule)
        {
            this.rule = rule;
        }

    }

    public class ErrorAction : Action
    {
        public override ActionType Type
        {
            get { return ActionType.ErrorAction; }
        }
    }

    public class ActionConflictException : Exception
    {
        private Action action1;
        public Action Action1
        {
            get { return this.action1; }
        }
        private Action action2;
        public Action Action2
        {
            get { return this.action2; }
        }

        public ActionConflictException(Action action1, Action action2)
        {
            this.action1 = action1;
            this.action2 = action2;
        }
    }

    public class AnalyzeTableRow
    {
        public int state;
        private int State
        {
            get { return state; }
        }

        private Dictionary<Symbol, Action> actions;
        public Dictionary<Symbol, Action> Actions
        {
            get { return actions; }
        }

        public AnalyzeTableRow(int state, Dictionary<Symbol, Action> actions)
        {
            this.state = state;
            this.actions = actions;
        }
    }

    public class GrammarRule
    {
        private static int stId = 0;

        private int id;
        private NonTerminal leftSymbol;
        private Symbol[] sentence;
        private string reduceAction;

        public int Id
        {
            get { return id; }
        }

        public NonTerminal LeftSymbol
        {
            get { return this.leftSymbol; }
        }

        public Symbol[] Sentence
        {
            get { return this.sentence; }
        }

        public string ReduceAction
        {
            get { return this.reduceAction; }
        }

        public GrammarRule(NonTerminal leftSymbol, Symbol[] sentence)
        {
            this.leftSymbol = leftSymbol;
            this.sentence = sentence;
            stId++;
            this.id = stId;
        }

        public GrammarRule(NonTerminal leftSymbol, Symbol[] sentence, string reduceAction)
            : this(leftSymbol, sentence)
        {
            this.reduceAction = reduceAction;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            sb.Append(this.Id);
            sb.Append(")");
            sb.Append(LeftSymbol.Name);
            sb.Append(" :");
            foreach (Symbol symbol in sentence)
            {
                sb.Append(" ");
                if (symbol is NonTerminal)
                {
                    sb.Append(((NonTerminal)symbol).Name);
                }
                else if (symbol is Terminal)
                {
                    sb.Append(((Terminal)symbol).Value);
                }
            }

            return sb.ToString();
        }
    }

    public abstract class Grammar
    {
        private GrammarRule[] grammarRules;
        private NonTerminal startSymbol;
        private NonTerminal[] nonTerminals;
        private Terminal[] terminals;
        protected string[] reduceActions;

        private bool isArgumented = false;
        private GrammarRule argumentedGrammarRule = null;
        protected int[][] grammarRuleTable;
        protected Action[][] actionTable;
        protected int[][] goTable;
        protected int[][] gotoTable;
        //private string propertyType;

        public GrammarRule[] GrammarRules
        {
            get { return this.grammarRules; }
        }

        public NonTerminal StartSymbol
        {
            get { return startSymbol; }
        }

        public NonTerminal[] NonTerminals
        {
            get
            {
                if (this.nonTerminals == null)
                {
                    Dictionary<NonTerminal, NonTerminal> nonTerminals = new Dictionary<NonTerminal, NonTerminal>();

                    foreach (GrammarRule rule in this.grammarRules)
                    {
                        nonTerminals[rule.LeftSymbol] = rule.LeftSymbol;
                        foreach (Symbol symbol in rule.Sentence)
                        {
                            if (symbol is NonTerminal)
                            {
                                NonTerminal nonTerminal = (NonTerminal)symbol;
                                nonTerminals[nonTerminal] = nonTerminal;
                            }
                        }
                    }
                    this.nonTerminals = nonTerminals.Keys.ToArray();
                    for (int i = 0; i < this.nonTerminals.Length; i++)
                    {
                        this.nonTerminals[i].id = i;
                    }
                }
                return this.nonTerminals;
            }
        }

        public Terminal[] Terminals
        {
            get
            {
                if (this.terminals == null)
                {
                    Dictionary<Terminal, Terminal> terminals = new Dictionary<Terminal, Terminal>();

                    foreach (GrammarRule rule in this.grammarRules)
                    {
                        foreach (Symbol symbol in rule.Sentence)
                        {
                            if (symbol is Terminal)
                            {
                                Terminal terminal = (Terminal)symbol;
                                terminals[terminal] = terminal;
                            }
                        }
                    }
                    this.terminals = terminals.Keys.ToArray();
                    for (int i = 0; i < this.terminals.Length; i++)
                    {
                        this.terminals[i].id = i;
                    }
                }
                return this.terminals;
            }
        }

        public string[] ReduceActions
        {
            get
            {
                if (reduceActions == null)
                {
                    reduceActions = new string[GrammarRules.Length];
                    for (int i = reduceActions.Length - 1; i >= 0; i--)
                    {
                        reduceActions[i] = GrammarRules[i].ReduceAction;
                    }
                }
                return reduceActions;
            }
        }

        public bool IsArgumented
        {
            get { return isArgumented; }
        }

        public GrammarRule ArgumentedGrammarRule
        {
            get { return argumentedGrammarRule; }
        }

        public Action[][] ActionTable
        {
            get { return actionTable; }
        }

        public int[][] GotoTable
        {
            get { return gotoTable; }
        }

        public int[][] GoTable
        {
            get
            {
                return goTable;
            }
        }

        public int[][] GrammarRuleTable
        {
            get
            {
                if (grammarRuleTable == null)
                {
                    grammarRuleTable = new int[GrammarRules.Length][];

                    for (int i = GrammarRules.Length - 1; i >= 0; i--)
                    {
                        grammarRuleTable[i] = new int[GrammarRules[i].Sentence.Length + 1];
                        grammarRuleTable[i][0] = GrammarRules[i].LeftSymbol.Id;
                        for (int j = GrammarRules[i].Sentence.Length; j > 0; j--)
                        {
                            grammarRuleTable[i][j] = GrammarRules[i].Sentence[j - 1].Id;
                        }
                    }
                }
                return grammarRuleTable;
            }
        }

        public Grammar(GrammarRule[] grammarRules, NonTerminal startSymbol)
        {
            this.grammarRules = grammarRules;
            this.startSymbol = startSymbol;
        }

        public void ArgumentGrammar()
        {
            if (!this.IsArgumented)
            {
                List<GrammarRule> grammarRules = new List<GrammarRule>(this.grammarRules);
                NonTerminal symbol = new NonTerminal("Argumented_" + this.StartSymbol.Name);
                GrammarRule rule = new GrammarRule(symbol, new Symbol[] { this.StartSymbol });
                grammarRules.Add(rule);
                this.argumentedGrammarRule = rule;
                this.startSymbol = symbol;
                this.isArgumented = true;
            }
        }

        protected abstract void Process();
    }
}
