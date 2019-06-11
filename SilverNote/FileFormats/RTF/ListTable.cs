/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileFormats.RTF
{
    public class ListTable
    {
        public IList<ListTableEntry> Lists { get; set; } 
    }

    public class ListTableEntry
    {
        public int ListID { get; set; }
        public string ListName { get; set; } 
        public int ListTemplateID { get; set; }
        public bool IsHybrid { get; set; }
        public IList<ListLevel> ListLevels { get; set; }
    }

    public class ListLevel
    {
        public int ListType { get; set; }
        public int Justify { get; set; }
        public int Follow { get; set; }
        public int StartAt { get; set; }
        public int Space { get; set; }
        public int Indent { get; set; }
        public LevelText Text { get; set; }
        public string LevelNumbers { get; set; }
        public string CharFormat { get; set; }
    }

    public class LevelText
    {
        public int TemplateID { get; set; }
        public string Value { get; set; }
    }
}
