using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace System.Text.Parse.Grammar
{
    public class RegularExpressionCCLex : Lex
    {
        public class CharCategory
        {
            private static int[] s_EscapeSequencies = null;
            public static int[] EscapeSequencies
            {
                get { return s_EscapeSequencies; }
            }

            public const char ESCAPE = '\\';

            public static bool IsEscapeSequence(int ch)
            {
                return GetEscapedValue(ch) >= 0;
            }

            public static int GetEscapedValue(int ch)
            {
                return ch < s_EscapeSequencies.Length && ch >= 0 ? s_EscapeSequencies[ch] : -1;
            }

            static CharCategory()
            {
                #region escape sequences
                int[][] escapeSequencies = new int[][]{
                    new int[] {'s', 't', 'n','0','\\', '+', '*'},
                    new int[] {' ', '\t', '\n', '\0', '\\', '+', '*'}
                };

                s_EscapeSequencies = new int[(int)escapeSequencies.Max().First()];
                for (int i = 0; i < s_EscapeSequencies.Length; i++)
                {
                    s_EscapeSequencies[i] = -1;
                }

                foreach (int ch in escapeSequencies[0])
                {
                    s_EscapeSequencies[ch] = escapeSequencies[1][ch];
                }
                #endregion
            }
        }

        public RegularExpressionCCLex(TextReader reader)
            : base(reader)
        {
        }

        public override int Read()
        {
            int ch = ReadChar();
            if (ch < 0)
            {
                return ch;
            }

            if (CharCategory.ESCAPE.Equals(ch))
            {
                ch = ReadChar();
                if (ch < 0)
                {
                    throw new InvalidDataException();
                }
                ch = CharCategory.GetEscapedValue(ch);
                if (ch < 0)
                {
                    throw new InvalidDataException();
                }
            }

            return ch;
        }
    }

}
