using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    public class DocumentFragmentBase : NodeBase, DocumentFragment
    {
        #region Constructors

        internal DocumentFragmentBase(DocumentBase ownerDocument)
            : base(ownerDocument, NodeType.DOCUMENT_FRAGMENT_NODE, "#document-fragment")
        {

        }

        #endregion

        #region INode

        public override Node CloneNode(bool deep)
        {
            var result = OwnerDocument.CreateDocumentFragment();

            if (deep)
            {
                foreach (var child in this.ChildNodes)
                {
                    var clone = child.CloneNode(true);

                    result.AppendChild(clone);
                }
            }

            return result;
        }

        #endregion

        #region Selectors

        public Element QuerySelector(string selectors)
        {
            return DOMSelectors.QuerySelector(this, selectors);
        }

        public NodeList QuerySelectorAll(string selectors)
        {
            return DOMSelectors.QuerySelectorAll(this, selectors);
        }

        #endregion
    }
}
