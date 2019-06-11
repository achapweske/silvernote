using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    public class ProcessingInstructionBase : NodeBase, ProcessingInstruction
    {
        #region Fields

        string _Data;

        #endregion

        #region Constructors

        internal ProcessingInstructionBase(Document ownerDocument, string target, string data)
            : base(ownerDocument, NodeType.PROCESSING_INSTRUCTION_NODE, target)
        {
            _Data = data;
        }

        #endregion

        #region ProcessingInstruction

        public string Target
        {
            get { return NodeName; }
        }

        public string Data
        {
            get { return _Data; }
            set { _Data = value; }
        }

        #endregion

        #region Node

        public override string NodeValue
        {
            get { return Data; }
            set { Data = value; }
        }

        public override Node CloneNode(bool deep)
        {
            return OwnerDocument.CreateProcessingInstruction(Target, Data);
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return String.Format("<?{0} {1}?>", Target, Data);
        }

        #endregion
    }
}
