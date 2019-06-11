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
    internal class LowerAlphaListStyle : UpperAlphaListStyle
    {
        public override string Description
        {
            get { return "a, b, c"; }
        }

        public override string GetMarker(int index)
        {
            return base.GetMarker(index).ToLower();
        }

        public override IListStyle NextStyle
        {
            get { return ListStyles.LowerRoman; }
        }

        public override IListStyle PreviousStyle
        {
            get { return ListStyles.Decimal; }
        }

        public override string ToString()
        {
            return "lower-alpha";
        }
    }
}
