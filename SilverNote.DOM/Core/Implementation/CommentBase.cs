using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DOM.Internal
{
    public class CommentBase : CharacterDataBase, Comment
    {
        #region Constructors

        internal CommentBase(DocumentBase ownerDocument, string text)
            : base(ownerDocument, "#comment", NodeType.COMMENT_NODE)
        {
            Data = text;
        }

        #endregion

        #region Comment

        #endregion

        #region Node

        public override Node CloneNode(bool deep)
        {
            return OwnerDocument.CreateComment(Data);
        }

        #endregion
    }
}
