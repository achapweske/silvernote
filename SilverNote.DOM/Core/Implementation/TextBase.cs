using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DOM.Internal
{
    public class TextBase : CharacterDataBase, Text
    {
        #region Constructors

        internal TextBase(Document ownerDocument, string data)
            : this("#text", NodeType.TEXT_NODE, ownerDocument, data)
        {

        }

        internal TextBase(string nodeName, NodeType nodeType, Document ownerDocument, string data)
            : base(ownerDocument, nodeName, nodeType)
        {
            Data = data;
        }

        #endregion

        #region Text

        public Text SplitText(int offset)
        {
            string splitData = this.SubstringData(offset, Length - offset);

            this.DeleteData(offset, Length - offset);

            var newNode = OwnerDocument.CreateTextNode(splitData);
            if (ParentNode != null)
            {
                if (NextSibling != null)
                {
                    ParentNode.InsertBefore(newNode, NextSibling);
                }
                else
                {
                    ParentNode.AppendChild(newNode);
                }
            }

            return newNode;
        }

        public bool IsElementContentWhitespace
        {
            get { throw new NotImplementedException(); }
        }

        public string WholeText
        {
            get { throw new NotImplementedException(); }
        }

        public Text ReplaceWholeText(string content)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Node

        public override Node CloneNode(bool deep)
        {
            return OwnerDocument.CreateTextNode(Data);
        }

        #endregion

    }
}
