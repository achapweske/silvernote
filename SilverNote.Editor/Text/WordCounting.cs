/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SilverNote.Editor
{
    public static class WordCounting
    {
        public static int CountWords(string s)
        {
            var matches = Regex.Matches(s, @"[\S]+");
            return matches.Count;
        }

        public static int CountCharacters(string str, bool includeSpaces)
        {
            if (includeSpaces)
            {
                return str.Count((c) => c != '\r' && c != '\n');
            }
            else
            {
                return str.Count((c) => !Char.IsWhiteSpace(c));
            }
        }
    }
}
