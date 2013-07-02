using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Text.Parse.Grammar
{
    public class Test
    {
        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj is Test)
            {
                Test t = (Test)obj;
                return this.Id == t.Id;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return 1;
        }
    }
    class Program
    {
        private static void Test1()
        {
            /*
            start : rules
            rules : rule ENDL rules
            rule : nonTerminator ':' sentence
            sentence : token sentence
            sentence : EMPTY
            token : NONTERMINATOR
            token : TERMINATOR
            */

            NonTerminal start = new NonTerminal("start");
            NonTerminal rules = new NonTerminal("rules");
            NonTerminal rule = new NonTerminal("rule");
            NonTerminal sentence = new NonTerminal("sentence");
            NonTerminal token = new NonTerminal("token");
            NonTerminal NONTERMINATOR = new NonTerminal("nonTerminator");
            NonTerminal TERMINATOR = new NonTerminal("terminator");
            Terminal ENDL = new Terminal("\n");
            Terminal COLON = new Terminal(":");

            List<GrammarRule> grammarRules = new List<GrammarRule>();

            // start : rules
            grammarRules.Add(new GrammarRule(start, new Symbol[] { rules }));
            // rules : rule ENDL rules
            grammarRules.Add(new GrammarRule(rules, new Symbol[] { rule, ENDL, rules }));
            grammarRules.Add(new GrammarRule(rule, new Symbol[] { NONTERMINATOR, COLON, sentence }));
            grammarRules.Add(new GrammarRule(sentence, new Symbol[] { token, sentence }));
            grammarRules.Add(new GrammarRule(sentence, new Symbol[] { }));
            grammarRules.Add(new GrammarRule(token, new Symbol[] { NONTERMINATOR }));
            grammarRules.Add(new GrammarRule(token, new Symbol[] { TERMINATOR }));

            Grammar grammar = new LR0Grammar(grammarRules.ToArray(), start);
            grammar = new LR1Grammar(grammarRules.ToArray(), start);
        }

        private static void Test2()
        {
            /*
            S : B B
            B : a B
            B : b
            */

            NonTerminal S = new NonTerminal("S");
            NonTerminal B = new NonTerminal("B");
            Terminal a = new Terminal("a");
            Terminal b = new Terminal("b");

            List<GrammarRule> grammarRules = new List<GrammarRule>();
            grammarRules.Add(new GrammarRule(S, new Symbol[] { B, B }));
            grammarRules.Add(new GrammarRule(B, new Symbol[] { a, B }));
            grammarRules.Add(new GrammarRule(B, new Symbol[] { b }));

            Grammar grammar = new LR0Grammar(grammarRules.ToArray(), S);
            grammar = new LR1Grammar(grammarRules.ToArray(), S);
        }

        private static void RegularExpression()
        {
            /*
            start : rules
            rules : rule ENDL rules
            rule : TOKENNAME ':' regularExp
            regularExp : '('regularExp')'
            regularExp : regularExp regularExp
            regularExp : regularExp '|' regularExp
            regularExp : regularExp '*'
            regularExp : regularExp '+'
            regularExp = '[' charSet ']'
            regularExp = '[^' charSet ']'
            charSet = charSet charSet
            charSet = CHAR
            */
            NonTerminal start = new NonTerminal("start");
            NonTerminal rules = new NonTerminal("rules");
            NonTerminal rule = new NonTerminal("rule");
            Terminal ENDL = new Terminal("\n");
            NonTerminal TOKENNAME = new NonTerminal("TOKENNAME");
            Terminal COLON = new Terminal(":");
            NonTerminal regularExp = new NonTerminal("regularExp");
            NonTerminal charSet = new NonTerminal("charSet");
            NonTerminal CHAR = new NonTerminal("CHAR");
            Terminal VBAR = new Terminal("|");
            Terminal STAR = new Terminal("*");
            Terminal PLUS = new Terminal("+");
            Terminal LPAR = new Terminal("(");
            Terminal RPAR = new Terminal(")");
            Terminal LBRA = new Terminal("[");
            Terminal RBRA = new Terminal("]");
            Terminal CARET = new Terminal("^");

            List<GrammarRule> grammarRules = new List<GrammarRule>();
            // start : rules
            grammarRules.Add(new GrammarRule(start, new Symbol[] { rules }));
            // rules : rule ENDL rules
            grammarRules.Add(new GrammarRule(rules, new Symbol[] { rule, ENDL, rules }));
            // rule : TOKENNAME ':' regularExp
            grammarRules.Add(new GrammarRule(rule, new Symbol[] { TOKENNAME, COLON, regularExp }));
            // regularExp : '('regularExp')'
            grammarRules.Add(new GrammarRule(regularExp, new Symbol[] { LPAR, regularExp, RPAR }));
            // regularExp : regularExp regularExp
            grammarRules.Add(new GrammarRule(regularExp, new Symbol[] { regularExp, regularExp }));
            // regularExp : regularExp '|' regularExp
            grammarRules.Add(new GrammarRule(regularExp, new Symbol[] { regularExp, VBAR, regularExp }));
            // regularExp : regularExp '*'
            grammarRules.Add(new GrammarRule(regularExp, new Symbol[] { regularExp, STAR }));
            // regularExp : regularExp '+'
            grammarRules.Add(new GrammarRule(regularExp, new Symbol[] { regularExp, PLUS }));
            // regularExp = '[' charSet ']'
            grammarRules.Add(new GrammarRule(regularExp, new Symbol[] { LBRA, charSet, RBRA }));
            // regularExp = '[''^' charSet ']'
            grammarRules.Add(new GrammarRule(regularExp, new Symbol[] { LBRA, CARET, charSet, RBRA }));
            // charSet = charSet charSet
            grammarRules.Add(new GrammarRule(charSet, new Symbol[] { charSet, charSet }));
            // charSet = CHAR
            grammarRules.Add(new GrammarRule(charSet, new Symbol[] { CHAR }));


        }

        static void Main(string[] args)
        {
            Test1();
        }
    }
}
