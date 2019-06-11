using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    public class DocumentTypeBase : NodeBase, DocumentType
    {
        #region Fields

        string _PublicId;
        string _SystemId;

        #endregion

        #region Constructors

        public DocumentTypeBase(string name, string publicId, string systemId)
            : base(null, NodeType.DOCUMENT_TYPE_NODE, name)
        {
            _PublicId = publicId;
            _SystemId = systemId;
        }

        #endregion

        #region DocumentType

        public string Name
        {
            get { return NodeName; }
        }

        public NamedNodeMap Entities
        {
            get { return null; }
        }

        public NamedNodeMap Notations
        {
            get { return null; }
        }

        public string PublicId
        {
            get { return _PublicId; }
        }

        public string SystemId
        {
            get { return _SystemId; }
        }

        public string InternalSubset
        {
            get { return null; }
        }

        #endregion

        #region INode

        public override Node CloneNode(bool deep)
        {
            return new DocumentTypeBase(Name, PublicId, SystemId);
        }

        #endregion
    }
}
