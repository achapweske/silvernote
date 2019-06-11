using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    public class CDATASectionBase : TextBase, CDATASection
    {
        #region Constructors

        internal CDATASectionBase(Document ownerDocument, string data)
            : base("#cdata-section", NodeType.CDATA_SECTION_NODE, ownerDocument, data)
        {

        }

        #endregion

        #region Node

        public override Node CloneNode(bool deep)
        {
            return OwnerDocument.CreateCDATASection(Data);
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return String.Format("<![CDATA[{0}]]>", Data);
        }

        #endregion
    }
}
