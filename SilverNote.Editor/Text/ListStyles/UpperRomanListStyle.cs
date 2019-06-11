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
    internal class UpperRomanListStyle : IListStyle
    {
        public virtual string Description
        {
            get { return "I, II, III"; }
        }

        public bool IsOrdered
        {
            get { return true; }
        }

        public virtual string GetMarker(int index)
        {
            StringBuilder result = new StringBuilder();

            index += 1;

            while (index >= 1000)
            {
                result.Append("M");
                index -= 1000;
            }

            while (index >= 900)
            {
                result.Append("CM");
                index -= 900;
            }

            while (index >= 500)
            {
                result.Append("D");
                index -= 500;
            }

            while (index >= 400)
            {
                result.Append("CD");
                index -= 400;
            }

            while (index >= 100)
            {
                result.Append("C");
                index -= 100;
            }

            while (index >= 90)
            {
                result.Append("XC");
                index -= 90;
            }

            while (index >= 50)
            {
                result.Append("L");
                index -= 50;
            }

            while (index >= 40)
            {
                result.Append("XL");
                index -= 40;
            }

            while (index >= 10)
            {
                result.Append("X");
                index -= 10;
            }

            while (index >= 9)
            {
                result.Append("IX");
                index -= 9;
            }

            while (index >= 5)
            {
                result.Append("V");
                index -= 5;
            }

            while (index >= 4)
            {
                result.Append("IV");
                index -= 4;
            }

            while (index >= 1)
            {
                result.Append("I");
                index -= 1;
            }

            result.Append('.');
            return result.ToString();
        }

        public virtual IListStyle NextStyle
        {
            get { return ListStyles.Decimal; }
        }

        public virtual IListStyle PreviousStyle
        {
            get { return ListStyles.UpperAlpha; }
        }

        public override string ToString()
        {
            return "upper-roman";
        }
    }
}
