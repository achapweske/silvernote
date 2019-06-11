/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DOM.CSS.Internal
{
    /// <summary>
    /// An ordered collection of simple selectors.
    /// 
    /// The first item must be a universal selector or type selector.
    /// 
    /// http://www.w3.org/TR/css3-selectors/#selector-syntax
    /// </summary>
    public class CSSSimpleSelectorSequence : List<CSSSimpleSelector>
    {
        #region Constructors

        public CSSSimpleSelectorSequence()
        {

        }

        #endregion

        #region Properties

        /// <summary>
        /// Get this sequence's specificity.
        /// 
        /// http://www.w3.org/TR/css3-selectors/#specificity
        /// </summary>
        public int Specificity
        {
            get
            {
                return this.Sum((item) => item.Specificity);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determine if node is a match for this sequence
        /// </summary>
        public bool Match(Node node)
        {
            for (int i = 0; i < Count; i++)
            {
                if (!this[i].Match(node))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return CSSFormatter.FormatSimpleSelectorSequence(this);
        }

        #endregion
    }
}
