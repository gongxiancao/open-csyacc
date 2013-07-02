using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Text.Parse.Grammar
{
    public class LRItem
    {
        private static int stId = 0;
        private int id;
        public int Id
        {
            get { return id; }
        }

        private GrammarRule grammarRule;
        public GrammarRule GrammarRule
        {
            get { return grammarRule; }
        }
        private int currentPosition;
        public int CurrentPosition
        {
            get { return currentPosition; }
        }

        public Symbol NextSymbol
        {
            get
            {
                if (this.CurrentPosition < this.GrammarRule.Sentence.Length)
                {
                    return this.GrammarRule.Sentence[this.CurrentPosition];
                }
                return null;
            }
        }

        public LRItem(GrammarRule grammarRule, int currentPosition)
        {
            this.grammarRule = grammarRule;
            this.currentPosition = currentPosition;
            stId++;
            this.id = stId;
        }

        public override int GetHashCode()
        {
            return this.grammarRule.GetHashCode() ^ this.currentPosition.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj is LRItem)
            {
                LRItem item = (LRItem)obj;
                return this.grammarRule.Equals(item.grammarRule) && this.currentPosition == item.currentPosition;
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(this.grammarRule.LeftSymbol.ToString());
            sb.Append(" :");
            for (int i = 0; i < this.grammarRule.Sentence.Length; i ++ )
            {
                if (i == this.currentPosition)
                {
                    sb.Append(" ●");
                }
                sb.Append(" ");
                sb.Append(this.grammarRule.Sentence[i].ToString());
            }
            if (this.grammarRule.Sentence.Length == this.currentPosition)
            {
                sb.Append(" ●");
            }
            return sb.ToString();
        }
    }

    public class LRItemSet : HashSet<LRItem>
    {
        public LRItemSet Clone()
        {
            LRItemSet set = new LRItemSet();
            foreach (LRItem item in this)
            {
                set.Add(item);
            }
            return set;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (LRItem item in this)
            {
                hash ^= item.GetHashCode();
            }
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj is LRItemSet)
            {
                LRItemSet right = (LRItemSet)obj;
                return SetEquals(right);
            }
            return false;
        }
    }

    public abstract class LRGrammar : Grammar
    {
        protected class LRShiftAction : Action
        {
            public override ActionType Type
            {
                get { return ActionType.ShiftAction; }
            }

            private LRItemSet nextState;
            public LRItemSet NextState
            {
                get { return nextState; }
            }

            public LRShiftAction(LRItemSet nextState)
            {
                this.nextState = nextState;
            }
        }

        public LRGrammar(GrammarRule[] grammarRules, NonTerminal startSymbol)
            : base(grammarRules, startSymbol)
        {
        }
    }
}
