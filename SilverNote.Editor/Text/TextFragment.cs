/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using DOM;
using DOM.CSS;
using DOM.HTML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Editor
{
    public class TextFragment : DocumentElement, ICloneable
    {
        #region Fields

        TextBuffer _Buffer;

        #endregion

        #region Constructors
        
        public TextFragment()
        {
            Initialize();
        }

        public TextFragment(TextBuffer buffer)
        {
            Initialize(buffer);
        }

        public TextFragment(TextFragment copy)
            : base(copy)
        {
            var buffer = (TextBuffer)copy.Buffer.Clone();
            Initialize(buffer);
        }

        private void Initialize(TextBuffer buffer = null)
        {
            _Buffer = buffer ?? new TextBuffer();
            _Buffer.Owner = this;
        }

        #endregion

        #region Properties

        public TextBuffer Buffer
        {
            get { return _Buffer; }
        }

        #endregion

        #region HTML

        public override string GetHTMLNodeName(NodeContext context)
        {
            return HTMLElements.SPAN;
        }

        public override IEnumerable<object> GetHTMLChildNodes(NodeContext context)
        {
            return Buffer.GetChildNodes(context);
        }

        public override object CreateHTMLNode(NodeContext context)
        {
            return Buffer.CreateNode(context);
        }

        public override void AppendHTMLNode(NodeContext context, object newChild)
        {
            Buffer.AppendNode(context, newChild);
        }

        public override void InsertHTMLNode(NodeContext context, object newChild, object refChild)
        {
            Buffer.InsertNode(context, newChild, refChild);
        }

        public override void RemoveHTMLNode(NodeContext context, object oldChild)
        {
            Buffer.RemoveNode(context, oldChild);
        }

        private static readonly string[] _HTMLStyles = new string[0];

        public override IList<string> GetHTMLStyles(ElementContext context)
        {
            return _HTMLStyles;
        }

        public override CSSValue GetHTMLStyle(ElementContext context, string propertyName)
        {
            return null;
        }

        public override void SetHTMLStyle(ElementContext context, string name, CSSValue value)
        {

        }

        #endregion

        #region ICloneable

        public virtual object Clone()
        {
            return new TextFragment(this);
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return Buffer.ToString();
        }

        #endregion
    }
}
