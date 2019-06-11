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
    internal class CheckListStyle : IListStyle
    {
        public string Description
        {
            get { return "\u2713 Check"; }
        }

        public bool IsOrdered
        {
            get { return false; }
        }

        public string GetMarker(int index)
        {
            return "\u2713";
        }

        public IListStyle NextStyle
        {
            get { return ListStyles.Check; }
        }

        public IListStyle PreviousStyle
        {
            get { return ListStyles.Check; }
        }

        public override string ToString()
        {
            return "check";
        }
    }
}
