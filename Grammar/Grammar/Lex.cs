using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace System.Text.Parse.Grammar
{
    public abstract class Lex
    {
        private TextReader reader = null;
        protected int tokenStartPos = 0;
        protected int tokenEndPos = 0;
        private int currentPos = 0;
        protected int lexValue;

        public int TokenStartPos
        {
            get { return tokenStartPos; }
        }

        public int TokenEndPos
        {
            get { return tokenEndPos; }
        }

        public int CurrentPos
        {
            get { return currentPos; }
        }

        public int LexValue
        {
            get { return lexValue; }
        }

        public Lex(TextReader reader)
        {
            this.reader = reader;
        }

        public int ReadChar()
        {
            currentPos++;
            return reader.Read();
        }

        public abstract int Read();
    }
}
