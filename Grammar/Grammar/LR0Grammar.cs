using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Text.Parse.Grammar
{

    public class LR0Item : LRItem
    {
        private int hashCode;
        public LR0Item(GrammarRule grammarRule, int currentPosition)
            : base(grammarRule, currentPosition)
        {
            hashCode = base.GetHashCode();
        }
    }

    public class LR0Grammar : LRGrammar
    {
        public LR0Grammar(GrammarRule[] grammarRules, NonTerminal startSymbol)
            : base(grammarRules, startSymbol)
        {
        }

        public LRItemSet GetClosureSet(LRItemSet lritemSet)
        {
            LRItemSet followAdded = new LRItemSet();
            List<LRItem> itemsAdded = new List<LRItem>();

            LRItemSet result = lritemSet.Clone();
            itemsAdded.AddRange(lritemSet);

            for (int i = 0; i < itemsAdded.Count; i++)
            {
                LRItem item = itemsAdded[i];
                if (item.NextSymbol != null)
                {
                    if (!followAdded.Contains(item))
                    {
                        foreach (GrammarRule rule in this.GrammarRules)
                        {
                            if (rule.LeftSymbol.Equals(item.NextSymbol))
                            {
                                LRItem j = new LR0Item(rule, 0);
                                if (!result.Contains(j))
                                {
                                    result.Add(j);
                                    itemsAdded.Add(j);
                                }
                            }
                        }
                        followAdded.Add(item);
                    }
                }
            }
            return result;
        }

        public Action GetAction(LRItemSet lritemSet, Symbol symbol)
        {
            Action action = null;
            LRItemSet result = new LRItemSet();
            foreach (LRItem i in lritemSet)
            {
                if (i.NextSymbol == null)
                {
                    Action newAction = null;
                    if (i.GrammarRule.Equals(this.ArgumentedGrammarRule) && symbol.Equals(Terminal.End))
                    {
                        newAction = new AcceptAction();
                    }
                    else
                    {
                        newAction = new ReduceAction(i.GrammarRule.Id);
                    }
                    if (action != null)
                    {
                        if (!action.Equals(newAction))
                        {
                            throw new ActionConflictException(action, newAction);
                        }
                    }
                    else
                    {
                        action = newAction;
                    }
                }
                if (i.NextSymbol != null && i.NextSymbol.Equals(symbol))
                {
                    LRItem item = new LR0Item(i.GrammarRule, i.CurrentPosition + 1);
                    result.Add(item);
                }
            }

            result = GetClosureSet(result);
            if (result.Count > 0)
            {
                action = new LRShiftAction(result);
            }

            if (action == null)
            {
                action = new ErrorAction();
            }
            return action;
        }

        protected override void Process()
        {
            ArgumentGrammar();

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

                #region process terminals to generate action table

                actionTable.Add(new Action[this.Terminals.Length]);

                for (int i = 0; i < this.Terminals.Length; i++)
                {
                    Action action = null;
                    Action newAction = null;
                    Terminal terminal = this.Terminals[i];
                    LRItemSet nextState = new LRItemSet();

                    foreach (LR1Item item in initialSet)
                    {
                        if (item.NextSymbol == null)
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
                            if (item.NextSymbol.Equals(terminal))
                            {
                                nextState.Add(new LR1Item(item.GrammarRule, item.CurrentPosition + 1, item.LookAhead));
                            }
                        }
                    }

                    nextState = GetClosureSet(nextState);

                    if (nextState.Count > 0)
                    {
                        int nextStateId = lRItemSets.IndexOf(nextState);
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
                    else
                    {
                        goTable[rowIdx][terminal] = -1;
                    }

                    actionTable[rowIdx][i] = action;
                }
                #endregion

                #region process nonterminals to generate goto table

                gotoTable.Add(new int[this.NonTerminals.Length]);

                for (int i = 0; i < this.NonTerminals.Length; i++)
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
                    gotoTable[rowIdx][i]= nextStateId;
                    goTable[rowIdx][nonTerminal] = nextStateId;
                }
                #endregion

                rowIdx++;
            }

            this.actionTable = actionTable.ToArray();
        }

    }
}
