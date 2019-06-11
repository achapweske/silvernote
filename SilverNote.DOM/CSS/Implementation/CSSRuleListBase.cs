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
using DOM.Internal;
using SilverNote.Common;

namespace DOM.CSS.Internal
{
    public class CSSRuleListBase : DOMList<CSSRule>, CSSRuleList
    {
        #region Fields

        /// <summary>
        /// Map property names to rules that reference them
        /// </summary>
        Dictionary<string, CSSSortedRuleSet> _NormalRules = new Dictionary<string, CSSSortedRuleSet>();

        /// <summary>
        /// Map property names to rules that reference them
        /// </summary>
        Dictionary<string, CSSSortedRuleSet> _ImportantRules = new Dictionary<string, CSSSortedRuleSet>();

        #endregion

        #region Constructors

        public CSSRuleListBase()
        {

        }

        #endregion

        #region Extensions

        public static string GetPropertyValue(CSSRuleList rules, NodeBase refNode, string propertyName)
        {
            if (rules is CSSRuleListBase)
            {
                return ((CSSRuleListBase)rules).GetPropertyValue(refNode, propertyName);
            }
            else
            {
                return String.Empty;
            }
        }

        public static string GetPropertyValue(CSSRuleList rules, NodeBase refNode, string propertyName, string priority)
        {
            if (rules is CSSRuleListBase)
            {
                return ((CSSRuleListBase)rules).GetPropertyValue(refNode, propertyName, priority);
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Lookup a style property value for the given node.
        /// </summary>
        /// <param name="refNode">Reference node</param>
        /// <param name="propertyName">Target property name</param>
        /// <returns>The retrieved property value, or String.Empty if not found</returns>
        public string GetPropertyValue(NodeBase refNode, string propertyName)
        {
            CSSSortedRuleSet rules;

            if (!_NormalRules.TryGetValue(propertyName, out rules))
            {
                return String.Empty;
            }

            var candidates = rules.ItemsFor((Element)refNode);

            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];

                if (candidate.Selector.Match(refNode))
                {
                    return candidate.Style.GetPropertyValue(propertyName);
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// Lookup a style property value for the given node.
        /// </summary>
        /// <param name="refNode">Reference node</param>
        /// <param name="propertyName">Target property name</param>
        /// <param name="priority">Property priority (e.g. "!important")</param>
        /// <returns>The retrieved property value, or String.Empty if not found</returns>
        public string GetPropertyValue(NodeBase refNode, string propertyName, string priority)
        {
            CSSSortedRuleSet rules;

            if (!String.IsNullOrEmpty(priority))
            {
                if (!_ImportantRules.TryGetValue(propertyName, out rules))
                {
                    return String.Empty;
                }
            }
            else
            {
                if (!_NormalRules.TryGetValue(propertyName, out rules))
                {
                    return String.Empty;
                }
            }

            var candidates = rules.ItemsFor((Element)refNode);

            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];

                if (candidate.Selector.Match(refNode))
                {
                    return candidate.Style.GetPropertyWithPriority(propertyName, priority);
                }
            }

            return String.Empty;
        }

        #endregion

        #region Implementation

        protected override void OnCollectionChanged(IList<CSSRule> removedItems, IList<CSSRule> addedItems)
        {
            if (removedItems != null)
            {
                foreach (var removedItem in removedItems.OfType<CSSStyleRuleBase>())
                {
                    OnRuleRemoved(removedItem);
                }

                var emptyImportantItems = _ImportantRules.Where((rule) => rule.Value.Count == 0).ToArray();
                foreach (var item in emptyImportantItems)
                {
                    _ImportantRules.Remove(item.Key);
                }

                var emptyNormalItems = _NormalRules.Where((rule) => rule.Value.Count == 0).ToArray();
                foreach (var item in emptyNormalItems)
                {
                    _NormalRules.Remove(item.Key);
                }
            }

            if (addedItems != null)
            {
                foreach (var addedItem in addedItems.OfType<CSSStyleRuleBase>())
                {
                    OnRuleAdded(addedItem);
                }
            }
        }

        private void OnRuleAdded(CSSStyleRuleBase addedItem)
        {
            CSSSortedRuleSet rules;

            for (int i = 0; i < addedItem.Style.Length; i++)
            {
                string propertyName = addedItem.Style[i];
                string priority = addedItem.Style.GetPropertyPriority(propertyName);

                if (!String.IsNullOrEmpty(priority))
                {
                    if (!_ImportantRules.TryGetValue(propertyName, out rules))
                    {
                        rules = new CSSSortedRuleSet();
                        _ImportantRules.Add(propertyName, rules);
                    }
                }
                else
                {
                    if (!_NormalRules.TryGetValue(propertyName, out rules))
                    {
                        rules = new CSSSortedRuleSet();
                        _NormalRules.Add(propertyName, rules);
                    }
                }

                rules.Add(addedItem);
            }
        }

        private void OnRuleRemoved(CSSStyleRuleBase removedItem)
        {
            foreach (var rule in _ImportantRules)
            {
                rule.Value.Remove(removedItem);
            }

            foreach (var rule in _NormalRules)
            {
                rule.Value.Remove(removedItem);
            }
        }
        
        #endregion

        #region Object

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                foreach (var item in this)
                {
                    writer.Write(item.CssText);
                    writer.Write('\n');
                }
                return writer.ToString();
            }
        }

        #endregion
    }
}
