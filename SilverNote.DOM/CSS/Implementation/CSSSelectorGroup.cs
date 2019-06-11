/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SilverNote.Common;

namespace DOM.CSS.Internal
{
    public class CSSSelectorGroup : List<CSSSelector>
    {
        /// <summary>
        /// Determine if this selector selects the given node
        /// </summary>
        /// <param name="node">The node to be tested</param>
        /// <returns>The highest specificity of all matching selectors, or -1 if no match was found</returns>
        public int Match(Node node)
        {
            int result = -1;

            for (int i = 0; i < Count; i++)
            {
                var selector = this[i];

                int specificity = selector.Specificity;

                if (specificity > result && selector.Match(node))
                {
                    result = specificity;
                }
            }

            return result;
        }

        public override string ToString()
        {
            return CSSFormatter.FormatSelectorGroup(this);
        }
    }
}
