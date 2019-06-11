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
    public enum CSSCombinator
    {
        None,
        Adjacent,       // adjacent sibling
        Child,          // child node
        Descendant,     // descendant node
        Sibling         // any sibling
    }

    /// <summary>
    /// Helpers for iterating through nodes, starting with refNode and
    /// continuing in the *opposite* direction as the given combinator
    /// </summary>
    internal class CSSCombinatorHelper
    {
        public static Node SelectFirst(Node refNode, CSSCombinator combinator)
        {
            switch (combinator)
            {
                case CSSCombinator.None:
                    return refNode;
                case CSSCombinator.Child:
                    return refNode.ParentNode;
                case CSSCombinator.Descendant:
                    return refNode.ParentNode;
                case CSSCombinator.Sibling:
                    return SelectLastSibling(refNode);
                case CSSCombinator.Adjacent:
                    return SelectLastAdjacent(refNode);
                default:
                    return null;
            }
        }

        public static Node SelectNext(Node curNode, Node refNode, CSSCombinator combinator)
        {
            switch (combinator)
            {
                case CSSCombinator.None:
                    return null;
                case CSSCombinator.Child:
                    return null;
                case CSSCombinator.Descendant:
                    return curNode.ParentNode;
                case CSSCombinator.Sibling:
                    return SelectPreviousSibling(curNode, refNode);
                case CSSCombinator.Adjacent:
                    return SelectPreviousAdjacent(curNode, refNode);
                default:
                    return null;
            }
        }

        private static Node SelectLastSibling(Node refNode)
        {
            Node node = refNode.ParentNode.LastChild;

            while (node != null)
            {
                if (node != refNode)
                {
                    return node;
                }
                node = node.PreviousSibling;
            }

            return null;
        }

        private static Node SelectPreviousSibling(Node curNode, Node refNode)
        {
            do
            {
                curNode = curNode.PreviousSibling;

                if (curNode != refNode)
                {
                    return curNode;
                }

            } while (curNode != null);

            return null;
        }

        private static Node SelectLastAdjacent(Node refNode)
        {
            var nextNode = refNode.NextSibling;
            if (nextNode != null)
            {
                return nextNode;
            }

            var prevNode = refNode.PreviousSibling;
            if (prevNode != null)
            {
                return prevNode;
            }

            return null;
        }

        private static Node SelectPreviousAdjacent(Node curNode, Node refNode)
        {
            var prevNode = refNode.PreviousSibling;
            if (prevNode != null && curNode != prevNode)
            {
                return prevNode;
            }

            return null;
        }
    }
}
