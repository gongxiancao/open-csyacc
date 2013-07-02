using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Text.Parse.Grammar
{
    public class LR1Item : LRItem
    {
        private TerminalSet lookAhead;
        public TerminalSet LookAhead
        {
            get { return lookAhead; }
        }

        private LR0Item lr0Item;
        public LR0Item LR0Item
        {
            get
            {
                if (lr0Item == null)
                {
                    lr0Item = new LR0Item(GrammarRule, CurrentPosition);
                }
                return lr0Item;
            }
        }

        private int hashCode;
        public LR1Item(GrammarRule grammarRule, int currentPosition, TerminalSet lookAhead)
            : base(grammarRule, currentPosition)
        {
            this.lookAhead = lookAhead;
            this.hashCode = ComputeHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj is LR1Item)
            {
                if (base.Equals(obj))
                {
                    LR1Item item = (LR1Item)obj;
                    return this.lookAhead.Equals(item.lookAhead);
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.hashCode;
        }

        public override string ToString()
        {
            return string.Format("{0} , {1}", base.ToString(), this.LookAhead.ToString());
        }

        private int ComputeHashCode()
        {
            return this.GrammarRule.GetHashCode() ^ this.CurrentPosition.GetHashCode() ^ this.LookAhead.GetHashCode();
        }
    }

    public class LR1Grammar : LRGrammar
    {
        public LR1Grammar(GrammarRule[] grammarRules, NonTerminal startSymbol)
            : base(grammarRules, startSymbol)
        {
        }

        public GrammarRule[] GetGrammarRules(NonTerminal leftSymbol)
        {
            Dictionary<GrammarRule, GrammarRule> rules = new Dictionary<GrammarRule, GrammarRule>();
            foreach (GrammarRule rule in GrammarRules)
            {
                if (rule.LeftSymbol == leftSymbol)
                {
                    rules[rule] = rule;
                }
            }
            return rules.Values.ToArray();
        }

        private Dictionary<Symbol, TerminalSet> firstSymbols = new Dictionary<Symbol, TerminalSet>();

        public TerminalSet GetFirstTerminal(NonTerminal nonTerminator)
        {
            if (!firstSymbols.ContainsKey(nonTerminator))
            {
                TerminalSet tset = new TerminalSet();
                foreach (GrammarRule rule in GrammarRules)
                {
                    if (rule.LeftSymbol == nonTerminator)
                    {
                        foreach (Symbol symbol in rule.Sentence)
                        {
                            if (symbol is Terminal)
                            {
                                Terminal tm = (Terminal)symbol;
                                tset.Add(tm);
                            }
                            else if (symbol is NonTerminal)
                            {
                                if (symbol != nonTerminator)
                                {
                                    TerminalSet firstTk = GetFirstTerminal((NonTerminal)symbol);
                                    if (!firstTk.Contains(Terminal.Empty))
                                    {
                                        break;
                                    }
                                    tset.UnionWith(firstTk);
                                }
                            }
                        }
                    }
                }
                firstSymbols[nonTerminator] = tset;
                return tset;
            }
            return firstSymbols[nonTerminator];
        }

        public TerminalSet GetFirstTerminal(Symbol[] sentence, TerminalSet lookAhead)
        {
            TerminalSet firstSet = new TerminalSet();
            foreach (Symbol symbol in sentence)
            {
                if (symbol is Terminal)
                {
                    Terminal tm = (Terminal)symbol;
                    firstSet.Add(tm);
                    break;
                }
                else if (symbol is NonTerminal)
                {
                    TerminalSet set = GetFirstTerminal((NonTerminal)symbol);
                    firstSet.UnionWith(set);
                    if (!set.Contains(Terminal.Empty))
                    {
                        firstSet.Remove(Terminal.Empty);
                        break;
                    }
                }
            }
            if (firstSet.Count == 0 || firstSet.Contains(Terminal.Empty))
            {
                firstSet.UnionWith(lookAhead);
            }
            return firstSet;
        }

        public LRItemSet ShrinkItemSet(LRItemSet lritemSet)
        {
            Dictionary<LR0Item, LR1Item> lr0itemsToLr1Items = new Dictionary<LR0Item, LR1Item>();
            foreach (LR1Item item in lritemSet)
            {
                LR1Item newItem = item;
                if (lr0itemsToLr1Items.ContainsKey(item.LR0Item))
                {
                    TerminalSet lookAhead = item.LookAhead;
                    lookAhead.Merge(lr0itemsToLr1Items[item.LR0Item].LookAhead);
                    newItem = new LR1Item(item.GrammarRule, item.CurrentPosition, lookAhead);
                }
                lr0itemsToLr1Items[newItem.LR0Item] = newItem;
            }

            LRItemSet result = new LRItemSet();
            foreach (LRItem item in lr0itemsToLr1Items.Values)
            {
                result.Add(item);
            }
            return result;
        }

        public LRItemSet GetClosureSet(LRItemSet lritemSet)
        {
            LRItemSet result = lritemSet.Clone();
            List<LRItem> itemsAdded = new List<LRItem>();
            LRItemSet followAdded = new LRItemSet();

            itemsAdded.AddRange(lritemSet);

            for (int i = 0; i < itemsAdded.Count; i++)
            {
                LR1Item item = (LR1Item)itemsAdded[i];
                if (item.NextSymbol != null)
                {
                    if (!followAdded.Contains(item))
                    {
                        Symbol[] follow = new Symbol[item.GrammarRule.Sentence.Length - item.CurrentPosition - 1];
                        Array.Copy(item.GrammarRule.Sentence, item.CurrentPosition + 1, follow, 0, follow.Length);

                        TerminalSet first = GetFirstTerminal(follow.ToArray(), item.LookAhead);

                        foreach (GrammarRule rule in GrammarRules)
                        {
                            if (rule.LeftSymbol.Equals(item.NextSymbol))
                            {
                                LRItem newItem = new LR1Item(rule, 0, first);
                                if (!result.Contains(newItem))
                                {
                                    result.Add(newItem);
                                    itemsAdded.Add(newItem);
                                }
                            }
                        }

                        followAdded.Add(item);
                    }
                }
            }
            return ShrinkItemSet(result);
        }

        protected override void Process()
        {
            ArgumentGrammar();

            Dictionary<LRItemSet, Dictionary<Symbol, Action>> table = new Dictionary<LRItemSet, Dictionary<Symbol, Action>>();

            List<LRItemSet> lRItemSets = new List<LRItemSet>();

            TerminalSet lookAhead = new TerminalSet();
            lookAhead.Add(Terminal.End);

            LRItemSet initialSet = new LRItemSet();
            initialSet.Add(new LR1Item(this.ArgumentedGrammarRule, 0, lookAhead));
            initialSet = GetClosureSet(initialSet);
            lRItemSets.Add(initialSet);

            int rowIdx = 0;

            List<Dictionary<Symbol, int>> goTable = new List<Dictionary<Symbol, int>>();

            List<Action[]> actionTable = new List<Action[]>();
            List<int[]> gotoTable = new List<int[]>();

            while (rowIdx < lRItemSets.Count)
            {
                initialSet = lRItemSets[rowIdx];

                Dictionary<Symbol, Action> row = new Dictionary<Symbol, Action>();

                #region process terminals to generate action table

                actionTable.Add(new Action[this.Terminals.Length]);

                for (int i = 0; i < this.Terminals.Length; i ++)
                {
                    Action action = null;
                    Action newAction = null;
                    Terminal terminal = this.Terminals[i];
                    LRItemSet nextState = new LRItemSet();

                    foreach (LR1Item item in initialSet)
                    {
                        if (item.NextSymbol == null)
                        {
                            if (item.LookAhead.Contains(terminal))
                            {
                                if (terminal.Equals(Terminal.End) && item.GrammarRule.Equals(ArgumentedGrammarRule))
                                {
                                    newAction = new AcceptAction();
                                }
                                else
                                {
                                    newAction = new ReduceAction(item.GrammarRule.Id);
                                }

                                if (action == null)
                                {
                                    action = newAction;
                                }
                                else
                                {
                                    if (!action.Equals(newAction))
                                    {
                                        throw new ActionConflictException(action, newAction);
                                    }
                                }
                            }
                            else
                            {
                                action = new ErrorAction();
                            }
                        }
                        else
                        {
                            if (item.NextSymbol.Equals(terminal))
                            {
                                nextState.Add(new LR1Item(item.GrammarRule, item.CurrentPosition + 1, item.LookAhead));
                            }
                        }
                    }

                    nextState = GetClosureSet(nextState);

                    int nextStateId = -1;
                    if (nextState.Count > 0)
                    {
                        nextStateId = lRItemSets.IndexOf(nextState);
                        if (nextStateId < 0)
                        {
                            nextStateId = lRItemSets.Count;
                            lRItemSets.Add(nextState);
                        }

                        newAction = new ShiftAction(nextStateId);
                        if (action == null)
                        {
                            action = newAction;
                        }
                        else
                        {
                            if (!action.Equals(newAction))
                            {
                                throw new ActionConflictException(action, newAction);
                            }
                        }

                        goTable[rowIdx][terminal] = nextStateId;
                    }
                    goTable[rowIdx][terminal] = nextStateId;

                    actionTable[rowIdx][i] = action;
                }
                #endregion

                #region process nonterminals to generate goto table

                gotoTable.Add(new int[this.NonTerminals.Length]);

                for (int i = 0; i < this.NonTerminals.Length; i ++)
                {
                    NonTerminal nonTerminal = this.NonTerminals[i];
                    LRItemSet nextState = new LRItemSet();
                    foreach (LR1Item item in initialSet)
                    {
                        if (item.NextSymbol != null)
                        {
                            if (item.NextSymbol.Equals(nonTerminal))
                            {
                                nextState.Add(new LR1Item(item.GrammarRule, item.CurrentPosition + 1, item.LookAhead));
                            }
                        }
                    }

                    nextState = GetClosureSet(nextState);

                    int nextStateId = -1;
                    if (nextState.Count > 0)
                    {
                        nextStateId = lRItemSets.IndexOf(nextState);
                        if (nextStateId < 0)
                        {
                            nextStateId = lRItemSets.Count;
                            lRItemSets.Add(nextState);
                        }
                    }
                    gotoTable[rowIdx][i] = nextStateId;
                    goTable[rowIdx][nonTerminal] = nextStateId;
                }
                #endregion

                rowIdx++;
            }

            this.actionTable = actionTable.ToArray();
        }

    }
}
