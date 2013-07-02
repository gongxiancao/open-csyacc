using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace System.Text.Parse.Grammar
{
    public static class RegularExpressionGrammar
    {
        private static LR1Grammar CreateGrammar()
        {
            #region lex grammar rules
            /*
            regularExp : '('regularExp')'
            regularExp : regularExp regularExp
            regularExp : regularExp '|' regularExp
            regularExp : regularExp '*'
            regularExp : regularExp '+'
            regularExp = '[' charSet ']'
            regularExp = '[^' charSet ']'
            charSet = charSet charSet
            charSet = CHAR
            charSet = CHAR '-' CHAR
            */
            #endregion

            NonTerminal regularExp = new NonTerminal("regularExp");
            NonTerminal charSet = new NonTerminal("charSet");
            Terminal CHAR = new Terminal("CHAR");
            Terminal VBAR = new Terminal("|");
            Terminal STAR = new Terminal("*");
            Terminal PLUS = new Terminal("+");
            Terminal LPAR = new Terminal("(");
            Terminal RPAR = new Terminal(")");
            Terminal LBRA = new Terminal("[");
            Terminal RBRA = new Terminal("]");
            Terminal CARET = new Terminal("^");
            Terminal HYPH = new Terminal("-");

            List<GrammarRule> grammarRules = new List<GrammarRule>();
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
            // charSet = CHAR '-' CHAR
            grammarRules.Add(new GrammarRule(charSet, new Symbol[] { CHAR, HYPH, CHAR }));

            return new LR1Grammar(grammarRules.ToArray(), regularExp);
        }
    }

    public class CSharpLexCompilerCompiler
    {
        public class CSharpLexCompiler : CSharpCompilerCompiler
        {
            public override void CreateLex(System.IO.TextWriter writer, string lexName, string readerName)
            {
                writer.WriteLine(string.Concat(@"Lex ", lexName, @" = new CSharpLexCompilerLex(", readerName, ");"));
            }
        }

        private static Grammar CreateGrammar()
        {
            /*
            start : rules
            rules : rule ENDL rules
            rule : TOKEN ':' TOKEN
            */
            NonTerminal start = new NonTerminal("start");
            NonTerminal rules = new NonTerminal("rules");
            NonTerminal rule = new NonTerminal("rule");
            Terminal TOKEN = new Terminal("TOKEN");
            Terminal ENDL = new Terminal("\n");
            Terminal COLON = new Terminal(":");

            List<GrammarRule> grammarRules = new List<GrammarRule>();
            // start : rules
            grammarRules.Add(new GrammarRule(start, new Symbol[] { rules }));
            // rules : rule ENDL rules
            grammarRules.Add(new GrammarRule(rules, new Symbol[] { rule, ENDL, rules }));
            // rule : TOKEN ':' TOKEN
            grammarRules.Add(new GrammarRule(rule, new Symbol[] { TOKEN, COLON, TOKEN }, "tokenRules[$1]=$2;"));

            return new LR1Grammar(grammarRules.ToArray(), start);
        }

        public void CreateCompiler(TextWriter writer)
        {
            Grammar grammar = CreateGrammar();
            writer.WriteLine("Dictionary<string,string> tokenRules = new Dictionary<string,string>();");
            CSharpLexCompiler compiler = new CSharpLexCompiler();
            compiler.CreateCompiler(writer, grammar, null);
        }
    }
}