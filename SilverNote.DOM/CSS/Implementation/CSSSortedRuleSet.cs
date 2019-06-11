/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.CSS.Internal
{
    /// <summary>
    /// Rules in this collection are sorted from highest specificity to
    /// lowest specificity. Among rules with the same specificity, items
    /// are sorted in the reverse order in which they were added (i.e.,
    /// the last rule to appear in the stylesheet is returned first).
    /// </summary>
    internal class CSSSortedRuleSet
    {
        #region Fields

        /// <summary>
        /// Master list of all items in this collection, sorted
        /// </summary>
        List<CSSRuleItem> _Items = new List<CSSRuleItem>();

        Dictionary<string, List<CSSRuleItem>> _ItemsForType;
        Dictionary<string, List<CSSRuleItem>> _ItemsForID;
        Dictionary<string, List<CSSRuleItem>> _ItemsForClass;

        #endregion

        #region Properties

        public int Count
        {
            get { return _Items.Count; }
        }

        public CSSRuleItem this[int index]
        {
            get { return _Items[index]; }
        }

        #endregion

        #region Methods

        public void Add(CSSStyleRuleBase rule)
        {
            foreach (CSSSelector selector in rule.Selector)
            {
                Add(selector, rule.Style);
            }
        }

        public void Remove(CSSStyleRuleBase rule)
        {
            foreach (CSSSelector selector in rule.Selector)
            {
                Remove(selector, rule.Style);
            }
        }

        public IList<CSSRuleItem> ItemsFor(Element element)
        {
            // Find all rules that apply to the given element filtered by tag, 
            // ID, and class then return the smallest of those lists.

            var results = new List<IList<CSSRuleItem>>();
            results.Add(_Items);

            if (HasItemsForType)
            {
                var itemsForType = ItemsForType(element.TagName);
                if (itemsForType.Count == 0)
                {
                    return itemsForType;
                }
                results.Add(itemsForType);
            }

            if (HasItemsForID)
            {
                string id = element.GetAttribute(Attributes.ID);
                if (!String.IsNullOrEmpty(id))
                {
                    var itemsForID = ItemsForID(id);
                    if (itemsForID.Count == 0)
                    {
                        return itemsForID;
                    }
                    results.Add(itemsForID);
                }
            }

            if (HasItemsForClass)
            {
                var classes = element.GetAttribute(Attributes.CLASS);
                if (!String.IsNullOrEmpty(classes))
                {
                    var tokens = classes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var token in tokens)
                    {
                        var itemsForClass = ItemsForClass(token);
                        if (itemsForClass.Count == 0)
                        {
                            return itemsForClass;
                        }
                        results.Add(itemsForClass);
                    }
                }
            }

            // Return the smallest list

            IList<CSSRuleItem> smallestResult = null;
            foreach (var result in results)
            {
                if (smallestResult == null || result.Count < smallestResult.Count)
                {
                    smallestResult = result;
                }
            }

            return smallestResult;
        }

        #endregion

        #region Implementation

        private void Add(CSSSelector selector, CSSStyleDeclaration style)
        {
            _ItemsForType = null;
            _ItemsForID = null;
            _ItemsForClass = null;

            for (int i = 0; i < _Items.Count; i++)
            {
                if (selector.Specificity >= _Items[i].Selector.Specificity)
                {
                    _Items.Insert(i, new CSSRuleItem(selector, style));
                    return;
                }
            }

            _Items.Add(new CSSRuleItem(selector, style));
        }

        private void Remove(CSSSelector selector, CSSStyleDeclaration style)
        {
            _ItemsForType = null;
            _ItemsForID = null;
            _ItemsForClass = null;

            for (int i = 0; i < _Items.Count; i++)
            {
                if (_Items[i].Selector == selector && _Items[i].Style == style)
                {
                    _Items.RemoveAt(i);
                    break;
                }
            }
        }

        bool HasItemsForType
        {
            get
            {
                if (_ItemsForType == null)
                {
                    _ItemsForType = CompileItemsFor(_Items, CSSSimpleSelectorType.Type);
                }

                return _ItemsForType.Count > 1;
            }
        }

        /// <summary>
        /// Get all rules that can apply to the given element type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IList<CSSRuleItem> ItemsForType(string type)
        {
            if (_ItemsForType == null)
            {
                _ItemsForType = CompileItemsFor(_Items, CSSSimpleSelectorType.Type);
            }

            List<CSSRuleItem> result;
            if (_ItemsForType.TryGetValue(type, out result))
            {
                return result;
            }

            if (_ItemsForType.TryGetValue("*", out result))
            {
                return result;
            }

            return _Items;
        }

        bool HasItemsForID
        {
            get
            {
                if (_ItemsForID == null)
                {
                    _ItemsForID = CompileItemsFor(_Items, CSSSimpleSelectorType.ID);
                }

                return _ItemsForID.Count > 1;
            }
        }

        /// <summary>
        /// Get all rules that can apply to an element with the given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IList<CSSRuleItem> ItemsForID(string id)
        {
            if (_ItemsForID == null)
            {
                _ItemsForID = CompileItemsFor(_Items, CSSSimpleSelectorType.ID);
            }

            List<CSSRuleItem> result;
            if (_ItemsForID.TryGetValue(id, out result))
            {
                return result;
            }

            if (_ItemsForID.TryGetValue("*", out result))
            {
                return result;
            }

            return _Items;
        }

        bool HasItemsForClass
        {
            get
            {
                if (_ItemsForClass == null)
                {
                    _ItemsForClass = CompileItemsFor(_Items, CSSSimpleSelectorType.Class);
                }

                return _ItemsForClass.Count > 1;
            }
        }

        /// <summary>
        /// Get all rules that can apply to an element with the given class
        /// </summary>
        /// <param name="clazz"></param>
        /// <returns></returns>
        public IList<CSSRuleItem> ItemsForClass(string clazz)
        {
            if (_ItemsForClass == null)
            {
                _ItemsForClass = CompileItemsFor(_Items, CSSSimpleSelectorType.Class);
            }

            List<CSSRuleItem> result;
            if (_ItemsForClass.TryGetValue(clazz, out result))
            {
                return result;
            }

            if (_ItemsForClass.TryGetValue("*", out result))
            {
                return result;
            }

            return _Items;
        }

        private Dictionary<string, List<CSSRuleItem>> CompileItemsFor(IList<CSSRuleItem> items, CSSSimpleSelectorType type)
        {
            var results = new Dictionary<string, List<CSSRuleItem>>();

            // Borrowing mozilla terminology, a "key" is the last sequence in a selector

            // For each unique key of the given type, add one entry to results
            foreach (var item in items)
            {
                var key = item.Selector.Last().Sequence;
                var component = key.FirstOrDefault(k => k.Type == type);
                if (component != null)
                {
                    if (!results.ContainsKey(component.Name))
                    {
                        results.Add(component.Name, new List<CSSRuleItem>());
                    }
                }
            }

            results.Add("*", new List<CSSRuleItem>());

            // Add items to appropriate entry(s) in results (in order)
            foreach (var item in items)
            {
                var key = item.Selector.Last().Sequence;
                var component = key.FirstOrDefault(k => k.Type == type);
                if (component != null)
                {
                    results[component.Name].Add(item);
                }
                else
                {
                    foreach (var result in results)
                    {
                        result.Value.Add(item);
                    }

                    results["*"].Add(item);
                }
            }

            return results;
        }

        #endregion

    }

    internal class CSSRuleItem
    {
        public CSSRuleItem(CSSSelector selector, CSSStyleDeclaration style)
        {
            Selector = selector;
            Style = style;
        }

        public readonly CSSSelector Selector;
        public readonly CSSStyleDeclaration Style;
    }
}
