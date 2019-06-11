using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DOM.Internal
{
    /// <summary>
    /// http://www.w3.org/TR/DOM-Level-2-Core/core.html#ID-FF21A306
    /// </summary>
    public abstract class CharacterDataBase : NodeProxy, CharacterData
    {
        #region Constructors

        internal CharacterDataBase(Document ownerDocument, string nodeName, NodeType nodeType)
            : base(ownerDocument, nodeName, nodeType)
        {

        }

        #endregion

        #region CharacterData

        public string Data { get; set; }

        public int Length 
        {
            get { return Data.Length; }
        }

        public string SubstringData(int offset, int count)
        {
            string data = this.Data;

            if (offset < 0 || offset >= data.Length || count < 0)
            {
                throw new DOMException(DOMException.INDEX_SIZE_ERR);
            }

            count = Math.Min(count, data.Length - offset);

            return data.Substring(offset, count);
        }

        public void AppendData(string arg)
        {
            Data += arg;
        }

        public void InsertData(int offset, string arg)
        {
            string data = this.Data;

            if (offset < 0 || offset > data.Length)
            {
                throw new DOMException(DOMException.INDEX_SIZE_ERR);
            }

            Data = data.Insert(offset, arg);
        }

        public void DeleteData(int offset, int count)
        {
            string data = this.Data;

            if (offset < 0 || offset >= data.Length || count < 0)
            {
                throw new DOMException(DOMException.INDEX_SIZE_ERR);
            }

            count = Math.Min(count, data.Length - offset);

            Data = data.Remove(offset, count);
        }


        public void ReplaceData(int offset, int count, string arg)
        {
            DeleteData(offset, count);
            InsertData(offset, arg);
        }

        #endregion

        #region Node

        public override string NodeValue
        {
            get { return Data; }
            set { Data = value; }
        }

        public override NodeList ChildNodes
        {
            get { return new NodeListBase(); }
        }

        public override Node FirstChild
        {
            get { return null; }
        }

        public override Node LastChild
        {
            get { return null; }
        }

        public override Node AppendChild(Node newChild)
        {
            throw new DOMException(DOMException.HIERARCHY_REQUEST_ERR);
        }

        public override Node InsertBefore(Node newChild, Node refChild)
        {
            throw new DOMException(DOMException.HIERARCHY_REQUEST_ERR);
        }

        public override Node RemoveChild(Node oldChild)
        {
            throw new DOMException(DOMException.NOT_FOUND_ERR);
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return Data;
        }

        #endregion

    }
}
