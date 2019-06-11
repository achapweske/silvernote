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
    internal class SquareListStyle : IListStyle
    {
        public string Description
        {
            get { return "\u25A0 Square"; }
        }

        public bool IsOrdered
        {
            get { return false; }
        }

        public string GetMarker(int index)
        {
            return "\u25A0";
        }

        public IListStyle NextStyle
        {
            get { return ListStyles.OpenSquare; }
        }

        public IListStyle PreviousStyle
        {
            get { return ListStyles.OpenCircle; }
        }

        public override string ToString()
        {
            return "square";
        }
    }
}
