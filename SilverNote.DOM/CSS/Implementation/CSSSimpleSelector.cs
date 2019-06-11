/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace DOM.CSS.Internal
{
    /// <summary>
    /// A simple selector is one of:
    /// 
    /// * Type selector
    /// * Universal selector
    /// * Attribute selector
    /// * Class selector
    /// * ID selector
    /// * Pseudo-class
    /// 
    /// http://www.w3.org/TR/css3-selectors/#selector-syntax
    /// </summary>
    public class CSSSimpleSelector
    {
        #region Constructors

        public CSSSimpleSelector()
        {
            ComparisonType = StringComparison.OrdinalIgnoreCase;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Selector type
        /// </summary>
        public CSSSimpleSelectorType Type { get; set; }

        /// <summary>
        /// Negation
        /// </summary>
        public bool Negate { get; set; }

        /// <summary>
        /// Namespace
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Operation (Attribute selectors)
        /// </summary>
        public CSSSimpleSelectorOperation Operation { get; set; }

        /// <summary>
        /// Operand (Attribute selectors)
        /// </summary>
        public string Operand { get; set; }

        /// <summary>
        /// Expression (Pseudo selectors)
        /// </summary>
        public string[] Expression { get; set; }

        /// <summary>
        /// Get this selector's specificity.
        /// 
        /// http://www.w3.org/TR/css3-selectors/#specificity
        /// </summary>
        public int Specificity
        {
            get { return GetSpecificity(Type); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determine if node is a match for this simple selector
        /// 
        /// http://www.w3.org/TR/css3-selectors/#simple-selectors
        /// </summary>
        public bool Match(Node node)
        {
            bool result = false;

            switch (Type)
            {
                case CSSSimpleSelectorType.Universal:
                    result = true;
                    break;
                case CSSSimpleSelectorType.Type:
                    result = MatchType(node, Namespace, Name, ComparisonType);
                    break;
                case CSSSimpleSelectorType.Attribute:
                    result = MatchAttribute(node, Namespace, Name, Operation, Operand, ComparisonType);
                    break;
                case CSSSimpleSelectorType.Class:
                    result = MatchClass(node, Namespace, Name, ComparisonType);
                    break;
                case CSSSimpleSelectorType.ID:
                    result = MatchID(node, Name, ComparisonType);
                    break;
                case CSSSimpleSelectorType.PseudoClass:
                    result = MatchPseudoClass(node, Namespace, Name, Expression, ComparisonType);
                    break;
                case CSSSimpleSelectorType.PseudoElement:
                    result = MatchPseudoElement(node, Namespace, Name);
                    break;
                default:
                    return false;
            }

            if (Negate)
            {
                result = !result;
            }

            return result;
        }

        #endregion

        #region Implementation

        public const int ID_SPECIFICITY = 100;
        public const int ATTRIBUTE_SPECIFICITY = 10;
        public const int CLASS_SPECIFICITY = 10;
        public const int PSEUDO_CLASS_SPECIFICITY = 10;
        public const int TYPE_SPECIFICITY = 1;
        public const int PSEUDO_ELEMENT_SPECIFICITY = 1;
        public const int UNIVERSAL_SPECIFICITY = 0;

        /// <summary>
        /// Get the specificity of the given selector type
        /// 
        /// http://www.w3.org/TR/css3-selectors/#specificity
        /// </summary>
        public static int GetSpecificity(CSSSimpleSelectorType type)
        {
            switch (type)
            {
                case CSSSimpleSelectorType.ID:
                    return ID_SPECIFICITY;
                case CSSSimpleSelectorType.Attribute:
                    return ATTRIBUTE_SPECIFICITY;
                case CSSSimpleSelectorType.Class:
                    return CLASS_SPECIFICITY;
                case CSSSimpleSelectorType.PseudoClass:
                    return PSEUDO_CLASS_SPECIFICITY;
                case CSSSimpleSelectorType.Type:
                    return TYPE_SPECIFICITY;
                case CSSSimpleSelectorType.PseudoElement:
                    return PSEUDO_ELEMENT_SPECIFICITY;
                case CSSSimpleSelectorType.Universal:
                    return UNIVERSAL_SPECIFICITY;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// String comparison
        /// </summary>
        internal StringComparison ComparisonType { get; set; }

        /// <summary>
        /// http://www.w3.org/TR/css3-selectors/#type-selectors
        /// </summary>
        internal static bool MatchType(Node node, string ns, string name, StringComparison comparisonType)
        {
            return node.NodeName.Equals(name, comparisonType);
        }

        /// <summary>
        /// http://www.w3.org/TR/css3-selectors/#attribute-selectors
        /// </summary>
        private static bool MatchAttribute(Node node, string ns, string name, CSSSimpleSelectorOperation operation, string operand, StringComparison comparisonType)
        {
            var element = node as Element;
            if (element == null || !element.HasAttribute(name))
            {
                return false;
            }

            if (operation == CSSSimpleSelectorOperation.None)
            {
                return true;    // Just checking for presence of attribute
            }

            if (operand != null)
            {
                operand = operand.Trim('\'', '\"');
            }

            string value = element.GetAttribute(name);

            switch (operation)
            {
                case CSSSimpleSelectorOperation.None:
                    return true;
                case CSSSimpleSelectorOperation.Equals:
                    return MatchAttributeEquals(value, operand, comparisonType);
                case CSSSimpleSelectorOperation.Includes:
                    return MatchAttributeIncludes(value, operand, comparisonType);
                case CSSSimpleSelectorOperation.PrefixMatch:
                    return MatchAttributePrefix(value, operand, comparisonType);
                case CSSSimpleSelectorOperation.SuffixMatch:
                    return MatchAttributeSuffix(value, operand, comparisonType);
                case CSSSimpleSelectorOperation.SubstringMatch:
                    return MatchAttributeSubstring(value, operand, comparisonType);
                case CSSSimpleSelectorOperation.DashMatch:
                    return MatchAttributeDash(value, operand, comparisonType);
                default:
                    return false;
            }
        }

        /// <summary>
        /// true iff attr exactly equals value
        /// </summary>
        private static bool MatchAttributeEquals(string attr, string value, StringComparison comparisonType)
        {
            return attr.Equals(value, comparisonType);
        }

        /// <summary>
        /// true if attr is a whitespace-separated list of words, one of which exactly equals value
        /// </summary>
        private static bool MatchAttributeIncludes(string attr, string value, StringComparison comparisonType)
        {
            string[] words = attr.Split();

            foreach (var word in words)
            {
                if (word.Equals(value, comparisonType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// true iff attr begins with the given prefix
        /// </summary>
        private static bool MatchAttributePrefix(string attr, string prefix, StringComparison comparisonType)
        {
            return attr.StartsWith(prefix, comparisonType);
        }

        /// <summary>
        /// true iff attr ends with the given suffix
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        private static bool MatchAttributeSuffix(string attr, string suffix, StringComparison comparisonType)
        {
            return attr.EndsWith(suffix, comparisonType);
        }

        /// <summary>
        /// true iff attr contains the given substring
        /// </summary>
        private static bool MatchAttributeSubstring(string attr, string substring, StringComparison comparisonType)
        {
            return attr.IndexOf(substring, comparisonType) != -1;
        }

        /// <summary>
        /// true if attr begins with value + "-", or exactly equals value
        /// </summary>
        private static bool MatchAttributeDash(string attr, string prefix, StringComparison comparisonType)
        {
            if (attr.Length == prefix.Length)
            {
                return attr.Equals(prefix, comparisonType);
            }
            else
            {
                return attr.StartsWith(prefix + "-", comparisonType);
            }
        }

        /// <summary>
        /// http://www.w3.org/TR/css3-selectors/#class-html
        /// </summary>
        internal static bool MatchClass(Node node, string ns, string name, StringComparison comparisonType)
        {
            var element = node as Element;
            if (element == null)
            {
                return false;
            }

            string classAttr = element.GetAttribute(Attributes.CLASS);
            if (String.IsNullOrEmpty(classAttr))
            {
                return false;
            }

            var classNames = classAttr.Split();
            foreach (var className in classNames)
            {
                if (className.Equals(name, comparisonType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// http://www.w3.org/TR/css3-selectors/#id-selectors
        /// </summary>
        internal static bool MatchID(Node node, string id, StringComparison comparisonType)
        {
            var element = node as Element;
            if (element == null)
            {
                return false;
            }

            string idAttr = element.GetAttribute(Attributes.ID);
            if (String.IsNullOrEmpty(idAttr))
            {
                return false;
            }

            return idAttr.Equals(id, comparisonType);
        }

        /// <summary>
        /// http://www.w3.org/TR/css3-selectors/#pseudo-classes
        /// </summary>
        private static bool MatchPseudoClass(Node node, string ns, string name, string[] expr, StringComparison comparisonType)
        {
            switch (name)
            {
                // dynamic pseudo-classes
                case "link":
                    return MatchPseudoLink(node);
                case "visited":
                case "hover":
                case "active":
                case "focus":
                    return false;
                // target pseudo-class
                case "target":
                    return false;
                // language pseudo-class
                case "lang":
                    return false;
                // UI state pseudo-classes
                case "enabled":
                case "disabled":
                case "checked":
                case "indeterminate":
                    return false;
                // structural pseudo-classes
                case "root":
                    return MatchPseudoRoot(node);
                case "nth-child":
                case "nth-last-child":
                case "nth-of-type":
                    return false;
                case "first-child":
                    return MatchPseudoFirstChild(node);
                case "last-child":
                    return MatchPseudoLastChild(node);
                case "first-of-type":
                case "last-of-type":
                case "only-child":
                case "only-of-type":
                case "empty":
                    return false;
                default:
                    return false;
            }
        }

        private static bool MatchPseudoLink(Node node)
        {
            var element = node as Element;

            return element != null && element.TagName == "a" && !String.IsNullOrWhiteSpace(element.GetAttribute("href"));
        }

        /// <summary>
        /// http://www.w3.org/TR/css3-selectors/#root-pseudo
        /// </summary>
        private static bool MatchPseudoRoot(Node node)
        {
            return node.OwnerDocument != null && node == node.OwnerDocument.DocumentElement;
        }

        /// <summary>
        /// http://www.w3.org/TR/css3-selectors/#first-child-pseudo
        /// </summary>
        private static bool MatchPseudoFirstChild(Node node)
        {
            return node.ParentNode != null && node == node.ParentNode.FirstChild;
        }

        /// <summary>
        /// http://www.w3.org/TR/css3-selectors/#last-child-pseudo
        /// </summary>
        private static bool MatchPseudoLastChild(Node node)
        {
            return node.ParentNode != null && node == node.ParentNode.LastChild;
        }

        /// <summary>
        /// http://www.w3.org/TR/css3-selectors/#pseudo-elements
        /// </summary>
        private static bool MatchPseudoElement(Node node, string ns, string name)
        {
            return false;
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return CSSFormatter.FormatSimpleSelector(this);
        }

        #endregion
    }

    public enum CSSSimpleSelectorType
    {
        Type,
        Universal,
        Attribute,
        Class,
        ID,
        PseudoClass,
        PseudoElement
    }

    public enum CSSSimpleSelectorOperation
    {
        None,               // [att]
        Equals,             // [att=val]
        Includes,           // [att~=val]
        PrefixMatch,        // [att^=val]
        SuffixMatch,        // [att$=val]
        SubstringMatch,     // [att*=val]
        DashMatch           // [att|=val]
    }
}
