/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM
{
    public interface CharacterData : Node
    {
        string Data { get; set; }
        int Length { get; }
        string SubstringData(int offset, int count);
        void AppendData(string arg);
        void InsertData(int offset, string arg);
        void DeleteData(int offset, int count);
        void ReplaceData(int offset, int count, string arg);
    }
}
