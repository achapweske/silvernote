/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Editor
{
    internal class UpperAlphaListStyle : IListStyle
    {
        public virtual string Description
        {
            get { return "A, B, C"; }
        }

        public bool IsOrdered
        {
            get { return true; }
        }

        public virtual string GetMarker(int index)
        {
            string result = "";

            while (index >= 0)
            {
                int i = index % 26;
                index -= 26;
                result += (char)((int)'A' + i);
            }
            result += ".";

            return result;
        }

        public virtual IListStyle NextStyle
        {
            get { return ListStyles.UpperRoman; }
        }

        public virtual IListStyle PreviousStyle
        {
            get { return ListStyles.LowerRoman; }
        }

        public override string ToString()
        {
            return "upper-alpha";
        }
    }
}
