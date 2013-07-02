using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace System.Text.Parse.Grammar
{
    public class LexLex : Lex
    {
        private string token;
        public string Token
        {
            get { return token; }
        }

        LexLex(TextReader reader)
            : base(reader)
        {
        }

        public override int Read()
        {
            int result = -1;
            tokenStartPos = CurrentPos;
            int ch = -1;

            StringBuilder sb = new StringBuilder();
            ch = ReadChar();
            while (ch > 0 && ch < (int)char.MaxValue && !char.IsSeparator((char)ch))
            {
                sb.Append((char)ch);
                ch = ReadChar();
            }

            tokenEndPos = CurrentPos;

            if (sb.Length > 0)
            {
                result = 1;
            }

            this.token = sb.ToString();
            return result;
        }
    }
}
