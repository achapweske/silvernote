/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Internal;
using DOM.CSS;
using DOM.CSS.Internal;

namespace DOM.Internal
{
    /// <summary>
    /// http://www.w3.org/TR/selectors-api/
    /// </summary>
    public static class DOMSelectors
    {
        /// <summary>
        /// Get the first element matching the given selector
        /// </summary>
        /// <param name="root">Begin searching at this node</param>
        /// <param name="selectorsText">Selector text</param>
        /// <returns>The first matching element, or null if none</returns>
        public static Element QuerySelector(Node root, string selectorsText)
        {
            selectorsText = selectorsText.Trim();

            CSSSelectorGroup selectors = CSSParser.ParseSelectorGroup(selectorsText);

            return QuerySelector(root, selectors);
        }

        public static Element QuerySelector(Node node, CSSSelectorGroup selectors)
        {
            if (node.NodeType == NodeType.ELEMENT_NODE)
            {
                if (selectors.Match(node) != -1)
                {
                    return (Element)node;
                }
            }

            foreach (Node child in node.ChildNodes)
            {
                Element result = QuerySelector(child, selectors);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all elements matching the given selector
        /// </summary>
        /// <param name="node">Begin searching at this node</param>
        /// <param name="selectorsText">Selector text</param>
        /// <returns>All matching elements, or an empty list if none</returns>
        public static NodeList QuerySelectorAll(Node node, string selectorsText)
        {
            selectorsText = selectorsText.Trim();

            CSSSelectorGroup selectors = CSSParser.ParseSelectorGroup(selectorsText);

            return QuerySelectorAll(node, selectors);
        }

        public static NodeList QuerySelectorAll(Node node, CSSSelectorGroup selectors)
        {
            List<Node> results = new List<Node>();

            QuerySelectorAll(node, selectors, results);

            return new NodeListBase(results);
        }

        private static void QuerySelectorAll(Node node, CSSSelectorGroup selectors, IList<Node> results)
        {
            if (node.NodeType == NodeType.ELEMENT_NODE)
            {
                if (selectors.Match(node) != -1)
                {
                    results.Add(node);
                }
            }

            var children = node.ChildNodes;

            for (int i = 0; i < children.Length; i++)
            {
                QuerySelectorAll(children[i], selectors, results);
            }
        }
    }
}
