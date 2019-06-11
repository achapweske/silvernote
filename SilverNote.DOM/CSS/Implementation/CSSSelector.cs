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
    /// <summary>
    /// A selector is an ordered collection of one or more sequences of 
    /// simple selectors separated by combinators.
    /// 
    /// http://www.w3.org/TR/css3-selectors/#selector-syntax
    /// </summary>
    public class CSSSelector : List<CSSSelectorItem>
    {
        #region Constructors

        public CSSSelector()
        {
            _Optimization = OptimizationType.None;
        }

        #endregion

        #region Properties

        /// <summary>
        /// http://www.w3.org/TR/css3-selectors/#specificity
        /// </summary>
        public int Specificity
        {
            get
            {
                // Optimization

                if (_OptName != null)
                {
                    return CSSSimpleSelector.TYPE_SPECIFICITY;
                }

                // Generic implementation

                return this.Sum((item) => item.Sequence.Specificity);
            }
        }

        /// <summary>
        /// Parse/format the given string
        /// </summary>
        public string CssText
        {
            get
            {
                return CSSFormatter.FormatSelector(this);
            }
            set
            {
                CSSParser.ParseSelector(value, this);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determine if the given node is a match for this selector
        /// </summary>
        public bool Match(Node node)
        {
            switch (_Optimization)
            {
                case OptimizationType.SingleTypeSelector:
                    return CSSSimpleSelector.MatchType(node, _OptNamespace, _OptName, _OptComparisonType);
                case OptimizationType.SingleIDSelector:
                    return CSSSimpleSelector.MatchID(node, _OptName, _OptComparisonType);
                case OptimizationType.SingleClassSelector:
                    return CSSSimpleSelector.MatchClass(node, _OptNamespace, _OptName, _OptComparisonType);
                case OptimizationType.None:
                default:
                    return Match(node, this.Count - 1);
            }
        }

        /// <summary>
        /// Determine if the given node is a match for this selector.
        /// </summary>
        private bool Match(Node node, int index)
        {
            // Starting with the last item in this selector, we find all nodes
            // for which applying that selector results in the given node. For each
            // of those, repeat for the next item in this selector and continue
            // recursively until we've evaluated the entire selector or no matches
            // are found.

            var selector = this[index];
            var sequence = selector.Sequence;
            var combinator = selector.Combinator;

            // Get all nodes that, when the given combinator is applied, result in the reference node

            Node candidate;
            for (candidate = CSSCombinatorHelper.SelectFirst(node, combinator);
                 candidate != null;
                 candidate = CSSCombinatorHelper.SelectNext(candidate, node, combinator))
            {
                if (sequence.Match(candidate))
                {
                    if (index > 0)
                    {
                        if (Match(candidate, index - 1))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return CssText;
        }

        #endregion

        #region Implementation

        enum OptimizationType
        {
            None,
            SingleTypeSelector,
            SingleIDSelector,
            SingleClassSelector
        };

        OptimizationType _Optimization;

        /// <summary>
        /// (Optimization) If this selector consists only of a single sequence
        /// containing a single simple selector of type 'Type', then 
        /// _OptTypeNamespace will contain that selector's namespace.
        /// </summary>
        private string _OptNamespace;

        /// <summary>
        /// (Optimization) If this selector consists only of a single sequence
        /// containing a single simple selector of type 'Type', then _OptTypeName
        /// will contain that selector's name.
        /// </summary>
        private string _OptName;

        /// <summary>
        /// If any of the above _Opt* values are set, this contains the target
        /// comparison type
        /// </summary>
        private StringComparison _OptComparisonType;

        /// <summary>
        /// Update the values of all _Opt* properties
        /// </summary>
        internal void Optimize()
        {
            _Optimization = OptimizationType.None;
            _OptName = null;
            _OptNamespace = null;
            _OptComparisonType = StringComparison.OrdinalIgnoreCase;

            if (this.Count == 1 && this[0].Sequence.Count == 1)
            {
                var selector = this[0].Sequence[0];

                _OptName = selector.Name;
                _OptNamespace = selector.Namespace;
                _OptComparisonType = selector.ComparisonType;

                switch (selector.Type)
                {
                    case CSSSimpleSelectorType.Type:
                        _Optimization = OptimizationType.SingleTypeSelector;
                        break;
                    case CSSSimpleSelectorType.ID:
                        _Optimization = OptimizationType.SingleIDSelector;
                        break;
                    case CSSSimpleSelectorType.Class:
                        _Optimization = OptimizationType.SingleClassSelector;
                        break;
                }
            }
        }

        #endregion
    }

    public class CSSSelectorItem
    {
        #region Constructors

        public CSSSelectorItem()
        {

            }

        public CSSSelectorItem(CSSSimpleSelectorSequence sequence)
        {
            Sequence = sequence;
        }

        public CSSSelectorItem(CSSSimpleSelectorSequence sequence, CSSCombinator combinator)
        {
            Sequence = sequence;
            Combinator = combinator;
        }

        #endregion

        #region Properties

        /// <summary>
        /// A simple selector sequence
        /// </summary>
        public CSSSimpleSelectorSequence Sequence { get; set; }

        /// <summary>
        /// Relationship between the current sequence and the next sequence.
        /// 
        /// This will be None for the last item in the selector.
        /// </summary>
        public CSSCombinator Combinator { get; set; }

        #endregion

        #region Object

        public override string ToString()
        {
            return CSSFormatter.FormatSelectorItem(this);
        }

        #endregion
    }
}
