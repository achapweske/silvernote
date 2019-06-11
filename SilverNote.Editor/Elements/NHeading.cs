/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Media;
using DOM;
using DOM.HTML;

namespace SilverNote.Editor
{
    public class NHeading : TextParagraph
    {
        public NHeading()
        {
            SetTextProperty(TextProperties.FontSizeProperty, 24.0);     // 24px = 18 pt
        }

        public NHeading(NHeading copy)
            : base(copy)
        {

        }

        public override ITextElement Split()
        {
            return null;
        }

        public override bool Merge(ITextElement other)
        {
            var paragraph = other as TextParagraph;
            if (paragraph != null && paragraph.Length == 0)
            {
                MoveToEnd();
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string GetNodeName(NodeContext context)
        {
            return HTMLElements.H1;
        }

        #region ICloneable

        public override object Clone()
        {
            return new NHeading(this);
        }

        #endregion
    }
}
