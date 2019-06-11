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
    internal class OpenDiamondListStyle : IListStyle
    {
        public string Description
        {
            get { return "\u25C7 Diamond"; }
        }

        public bool IsOrdered
        {
            get { return false; }
        }

        public string GetMarker(int index)
        {
            return "\u25C7";
        }

        public IListStyle NextStyle
        {
            get { return ListStyles.Triangle; }
        }

        public IListStyle PreviousStyle
        {
            get { return ListStyles.Diamond; }
        }

        public override string ToString()
        {
            return "open-diamond";
        }
    }
}
