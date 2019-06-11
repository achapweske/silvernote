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
    internal class DiamondListStyle : IListStyle
    {
        public string Description
        {
            get { return "\u25C6 Diamond"; }
        }

        public bool IsOrdered
        {
            get { return false; }
        }

        public string GetMarker(int index)
        {
            return "\u25C6";
        }

        public IListStyle NextStyle
        {
            get { return ListStyles.OpenDiamond; }
        }

        public IListStyle PreviousStyle
        {
            get { return ListStyles.OpenSquare; }
        }

        public override string ToString()
        {
            return "diamond";
        }
    }
}
