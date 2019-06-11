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
using System.Windows;

namespace SilverNote.Editor
{
    public interface ITextElement : IFormattable, INavigable, ISelectableText
    {
        void Insert(string value);
        void Replace(string value);
        int Replace(string oldValue, string newValue);

        bool MoveLeftByWord();
        bool MoveRightByWord();
        bool MoveToLineStart();
        bool MoveToLineEnd();
        bool MoveToParagraphStart();
        bool MoveToParagraphEnd();

        bool SelectLeftByWord();
        bool SelectRightByWord();
        bool SelectToLineStart();
        bool SelectToLineEnd();
        bool SelectToParagraphStart();
        bool SelectToParagraphEnd();

        bool DeleteBack();
        bool DeleteBackByWord();
        bool DeleteBackByParagraph();
        bool DeleteForward();
        bool DeleteForwardByWord();
        bool DeleteForwardByParagraph();
        bool EnterLineBreak();
        bool EnterParagraphBreak();

        ITextElement Split();
        bool Merge(ITextElement other);
    }

}
