/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Xml;
using DOM;
using DOM.CSS;
using DOM.HTML;
using DOM.SVG;
using System.ComponentModel;

namespace SilverNote.Editor
{
    public class TextRunSource : INodeSource, IStyleable, ICloneable
    {
        #region Fields

        StringBuilder _Buffer;
        GenericTextRunProperties _Properties;
        TextRunSource _Next;
        TextRunSource _Previous;

        #endregion

        #region Constructors

        public TextRunSource(GenericTextRunProperties properties = null)
        {
            _Buffer = new StringBuilder();

            if (properties != null)
            {
                Properties = (GenericTextRunProperties)properties.Clone();
            }
        }

        #endregion

        #region Properties

        public int Length
        {
            get { return _Buffer.Length; }
        }

        public string Text
        {
            get 
            { 
                return _Buffer.ToString(); 
            }
            set
            {
                var oldValue = Text;
                if (value != oldValue)
                {
                    _Buffer.Clear();
                    _Buffer.Append(value);
                    OnTextChanged(0, value.Length, oldValue.Length);
                }
            }
        }

        public GenericTextRunProperties Properties
        {
            get
            {
                return _Properties;
            }
            set
            {
                if (value != _Properties)
                {
                    var oldValue = _Properties;
                    _Properties = value;
                    OnPropertiesChanged(value, oldValue);
                }
            }
        }

        public int Offset { get; set; }

        public TextBuffer Parent { get; set; }

        public TextRunSource Next
        {
            get
            {
                if (_Next != null)
                {
                    _Next.Offset = this.Offset + this.Length;
                }
                return _Next;
            }
            set { _Next = value; }
        }

        public TextRunSource Previous
        {
            get
            {
                if (_Previous != null)
                {
                    _Previous.Offset = this.Offset - _Previous.Length;
                }
                return _Previous;
            }
            set { _Previous = value; }
        }

        #endregion

        #region Events

        public event EventHandler<TextChangedEventArgs> TextChanged;

        private void RaiseTextChanged(int offset, int numAdded, int numRemoved)
        {
            if (TextChanged != null)
            {
                TextChanged(this, new TextChangedEventArgs(offset, numAdded, numRemoved));
            }
        }

        public event EventHandler<FormatChangedEventArgs> FormatChanged;

        private void RaiseFormatChanged(FormatChangedEventArgs e)
        {
            if (FormatChanged != null)
            {
                FormatChanged(this, e);
            }
        }

        #endregion

        #region Operations

        public void Append(string text)
        {
            if (text.Length > 0)
            {
                int offset = _Buffer.Length;

                _Buffer.Append(text);

                OnTextChanged(offset, text.Length, 0);
            }
        }

        public void Insert(int offset, string text)
        {
            if (text.Length > 0)
            {
                _Buffer.Insert(offset, text);

                OnTextChanged(offset, text.Length, 0);
            }
        }

        public void Remove(int offset, int length)
        {
            if (length > 0)
            {
                _Buffer.Remove(offset, length);

                OnTextChanged(offset, 0, length);
            }
        }

        public string Substring(int startIndex, int length)
        {
            return _Buffer.ToString().Substring(startIndex, length);
        }

        public TextRunSource Cut(int offset, int length)
        {
            var result = new TextRunSource(Properties);
            string text = this.Substring(offset, length);
            result.Append(text);
            this.Remove(offset, length);
            return result;
        }

        public TextRunSource Copy(int offset, int length)
        {
            var result = new TextRunSource(Properties);
            string text = this.Substring(offset, length);
            result.Append(text);
            return result;
        }

        public void Paste(int offset, TextRunSource other)
        {
            Insert(offset, other.Text);
        }

        #endregion

        #region TextSource
        
        public TextRun GetTextRun(int offset)
        {
            return new TextCharacters(Text, offset, Length - offset, Properties);
        }

        public TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int index)
        {
            return new TextSpan<CultureSpecificCharacterBufferRange>(
                index, new CultureSpecificCharacterBufferRange(
                    CultureInfo.CurrentUICulture, new CharacterBufferRange(Text.ToString(), 0, index)
                )
            );
        }

        #endregion

        #region INodeSource

        public virtual NodeType GetNodeType(NodeContext context)
        {
            return NodeType.ELEMENT_NODE;
        }

        public virtual string GetNodeName(NodeContext context)
        {
            return Properties.GetNodeName(context);
        }

        public virtual IList<string> GetNodeAttributes(ElementContext context)
        {
            return Properties.GetNodeAttributes(context);
        }

        public virtual string GetNodeAttribute(ElementContext context, string name)
        {
            return Properties.GetNodeAttribute(context, name);
        }

        public virtual void SetNodeAttribute(ElementContext context, string name, string value)
        {
            Properties.SetNodeAttribute(context, name, value);
        }

        public virtual void ResetNodeAttribute(ElementContext context, string name)
        {
            Properties.ResetNodeAttribute(context, name);
        }

        public virtual object GetParentNode(NodeContext context)
        {
            return Parent != null ? Parent.Owner : null;
        }

        public virtual IEnumerable<object> GetChildNodes(NodeContext context)
        {
            yield return context.OwnerDocument.CreateTextNode(Text);
        }

        public virtual object CreateNode(NodeContext newNode)
        {
            if (newNode is Text)
            {
                return newNode;
            }
            else
            {
                return null;
            }
        }

        public virtual void AppendNode(NodeContext context, object newChild)
        {
            this.Text = ((Text)newChild).Data;
        }

        public virtual void InsertNode(NodeContext context, object newChild, object refChild)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveNode(NodeContext context, object oldChild)
        {
            throw new NotImplementedException();
        }

        public object GetSVGChildNode(NodeContext context, int charOffset, int charLength, Point position)
        {
            string text = Text.ToString().Substring(charOffset, charLength);

            if (Properties == null)
            {
                return context.OwnerDocument.CreateTextNode(text);
            }

            var tspan = (SVGTSpanElement)context.OwnerDocument.CreateElementNS(SVGElements.NAMESPACE, SVGElements.TSPAN);

            if (!Double.IsNaN(position.X))
            {
                tspan.SetAttribute(SVGAttributes.X, SafeConvert.ToString(position.X));
            }
            if (!Double.IsNaN(position.Y))
            {
                tspan.SetAttribute(SVGAttributes.Y, SafeConvert.ToString(position.Y));
            }

            if (context is ElementContext)
            {
                foreach (string name in Properties.GetNodeAttributes((ElementContext)context))
                {
                    string value = Properties.GetNodeAttribute((ElementContext)context, name);
                    if (value != null)
                    {
                        tspan.SetAttribute(name, value);
                    }
                }
            }

            tspan.TextContent = text;

            return tspan;
        }

        public event NodeEventHandler NodeEvent;

        protected void RaiseNodeEvent(IEventSource evt)
        {
            if (NodeEvent != null)
            {
                NodeEvent(evt);
            }
        }

        #endregion

        #region IStyleable

        public IList<string> GetSupportedStyles(ElementContext context)
        {
            return Properties.GetSupportedStyles(context);
        }

        public virtual CSSValue GetStyleProperty(ElementContext context, string name)
        {
            if (GetNodeType(context) == NodeType.ELEMENT_NODE)
            {
                return Properties.GetStyleProperty(context, name);
            }
            else
            {
                return null;
            }
        }

        public virtual void SetStyleProperty(ElementContext context, string name, CSSValue value)
        {
            Properties.SetStyleProperty(context, name, value);
        }

        #endregion

        #region ICloneable

        public object Clone()
        {
            return Copy(0, Length);
        }

        #endregion

        #region Implementation

        void OnTextChanged(int offset, int numAdded, int numRemoved)
        {
            RaiseTextChanged(offset, numAdded, numRemoved);
        }

        void OnPropertiesChanged(GenericTextRunProperties newValue, GenericTextRunProperties oldValue)
        {
            if (oldValue != null)
            {
                oldValue.FormatChanged -= Properties_FormatChanged;
            }

            if (newValue != null)
            {
                newValue.FormatChanged += Properties_FormatChanged;
            }

            RaiseFormatChanged(new FormatChangedEventArgs());
        }

        void Properties_FormatChanged(object sender, FormatChangedEventArgs e)
        {
            RaiseFormatChanged(e);
        }

        #endregion
    }

    public class TextFormatChangedEventArgs : EventArgs
    {
        int _Offset;
        int _Length;
        string _PropertyName;

        public TextFormatChangedEventArgs(int offset, int length, string propertyName)
        {
            _Offset = offset;
            _Length = length;
            _PropertyName = propertyName;
        }

        public int Offset
        {
            get { return _Offset; }
        }

        public int Length
        {
            get { return _Length; }
        }

        public string PropertyName
        {
            get { return _PropertyName; }
        }
    }

    public class TextChangedEventArgs : EventArgs
    {
        int _Offset;
        int _NumAdded;
        int _NumRemoved;

        public TextChangedEventArgs(int offset, int numAdded, int numRemoved)
        {
            _Offset = offset;
            _NumAdded = numAdded;
            _NumRemoved = numRemoved;
        }

        public int Offset
        {
            get { return _Offset; }
        }

        public int NumAdded
        {
            get { return _NumAdded; }
        }

        public int NumRemoved
        {
            get { return _NumRemoved; }
        }
    }
}
