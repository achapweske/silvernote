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
    internal class DecimalListStyle : IListStyle
    {
        public string Description
        {
            get { return "1, 2, 3"; }
        }

        public bool IsOrdered
        {
            get { return true; }
        }

        public string GetMarker(int index)
        {
            return (index + 1).ToString() + ".";
        }

        public IListStyle NextStyle
        {
            get { return ListStyles.LowerAlpha; }
        }

        public IListStyle PreviousStyle
        {
            get { return ListStyles.UpperRoman; }
        }

        public override string ToString()
        {
            return "decimal";
        }
    }
}
