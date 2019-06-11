using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.TextFormatting;

namespace SilverNote.Editor
{
    public class TextParagraphRun : TextRunSource
    {
        #region Static Members

        new public static TextRunSource Create(TextRunProperties properties)
        {
            return new TextParagraphRun(properties);
        }

        #endregion

        #region Constructors

        protected TextParagraphRun(TextRunProperties properties)
        {

        }

        #endregion
    }
}
