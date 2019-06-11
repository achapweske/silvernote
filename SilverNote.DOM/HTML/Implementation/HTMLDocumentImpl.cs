/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DOM.Internal;
using DOM.CSS;
using DOM.Views;
using DOM.Style.Internal;
using DOM.Style;

namespace DOM.HTML.Internal
{
    /// <summary>
    /// An HTMLDocument is the root of the HTML hierarchy and holds the entire content. Besides providing access to the hierarchy, it also provides some convenience methods for accessing certain sets of information from the document.
    /// 
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-26809268
    /// </summary>
    public class HTMLDocumentImpl : DocumentBase, HTMLDocument, DocumentCSS
    {
        #region Fields

        string _Referrer;
        string _Domain;
        HTMLHeadElement _Head;
        HTMLTitleElement _Title;
        HTMLBodyElement _Body;
        MutableStyleSheetList _StyleSheets;
        string _Cookie;
        StringBuilder _HtmlBuffer;
        HTMLViewBase _DefaultView;

        #endregion

        #region Constructors

        static HTMLDocumentImpl()
        {
            // fix issue where HtmlAgilityPack doesn't handle form tags properly
            HtmlAgilityPack.HtmlNode.ElementsFlags.Remove("form");
        }

        public HTMLDocumentImpl(DOMImplementationBase implementation, DocumentType docType)
            : base(implementation, docType)
        {
            var documentElement = CreateElement(HTMLElements.HTML);
            AppendChild(documentElement);
            documentElement.AppendChild(CreateElement(HTMLElements.HEAD));
            documentElement.AppendChild(CreateElement(HTMLElements.BODY));
        }

        #endregion

        #region HTMLDocument

        public string Title
        {
            get 
            {
                if (_Title != null)
                {
                    return _Title.Text;
                }
                else
                {
                    return String.Empty;
                }
            }
            set 
            {
                if (value != null)
                {
                    GetOrCreateTitle().Text = value;
                }
                else if (_Title != null)
                {
                    _Title.ParentNode.RemoveChild(_Title);
                }
            }
        }

        public string Referrer
        {
            get { return _Referrer; }
        }

        public string Domain
        {
            get { return _Domain; }
        }

        public string URL
        {
            get { return DocumentURI; }
        }

        public HTMLElement Body
        {
            get 
            { 
                return _Body; 
            }
            set 
            {
                if (_Body != null)
                {
                    _Body.ParentNode.RemoveChild(_Body);
                }

                if (value != null)
                {
                    DocumentElement.AppendChild(value);
                }
            }
        }

        public HTMLCollection Images 
        {
            get { throw new NotImplementedException(); } 
        }

        public HTMLCollection Applets 
        {
            get { throw new NotImplementedException(); } 
        }

        public HTMLCollection Links 
        {
            get { throw new NotImplementedException(); } 
        }

        public HTMLCollection Forms 
        {
            get { throw new NotImplementedException(); }  
        }

        public HTMLCollection Anchors 
        {
            get { throw new NotImplementedException(); } 
        }

        public string Cookie 
        {
            get { return _Cookie; }  
            set { _Cookie = value; } 
        }

        public void Open()
        {
            _HtmlBuffer = new StringBuilder();
        }

        public void Close()
        {
            string html = _HtmlBuffer.ToString();

            _HtmlBuffer = null;

            var document = new HtmlAgilityPack.HtmlDocument();

            document.LoadHtml(html);

            ParseDocument(document);
        }

        public void Write(string text)
        {
            if (_HtmlBuffer != null)
            {
                _HtmlBuffer.Append(text);
            }
        }

        public void WriteLn(string text)
        {
            if (_HtmlBuffer != null)
            {
                _HtmlBuffer.AppendLine(text);
            }
        }

        public NodeList GetElementsByName(string elementName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region DocumentView

        public override AbstractView DefaultView
        {
            get
            {
                if (_DefaultView == null)
                {
                    _DefaultView = new HTMLViewBase(this);
                }

                return _DefaultView;
            }
        }

        #endregion

        #region DocumentCSS

        public StyleSheetList StyleSheets
        {
            get 
            {
                if (_StyleSheets != null)
                {
                    return _StyleSheets;
                }
                else
                {
                    return StyleSheetListBase.Empty;
                }
            }
        }

        public CSSStyleDeclaration GetOverrideStyle(Element elt, string pseudoElt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// User style sheet (optional)
        /// </summary>
        public CSSStyleSheet UserStyleSheet { get; set; }

        #endregion

        #region Extensions

        /// <summary>
        /// Convert a relative URL to an absolute URL based on this document's URL
        /// </summary>
        public string ResolveURL(string url)
        {
            if (String.IsNullOrEmpty(URL) || url.Contains(":"))
            {
                return url;
            }

            string baseUrl = URL;

            // Strip query and hash
            int index = baseUrl.IndexOfAny("?#".ToCharArray());
            if (index != -1)
            {
                baseUrl = baseUrl.Remove(index);
            }

            // Strip file name
            index = baseUrl.LastIndexOf('/');
            if (index != -1 && index + 1 < baseUrl.Length)
            {
                baseUrl = baseUrl.Remove(index + 1);
            }

            if (url.StartsWith("//"))
            {
                index = baseUrl.IndexOf("://");
                if (index != -1)
                {
                    baseUrl = baseUrl.Substring(0, index + 1);
                }
            }
            else if (url.StartsWith("/"))
            {
                index = baseUrl.IndexOf("://");
                if (index != -1)
                {
                    index = baseUrl.IndexOf('/', index + 3);

                    if (index != -1)
                    {
                        baseUrl = baseUrl.Substring(0, index);
                    }
                }
            }

            return baseUrl + url;
        }

        public void SetReferrer(string newReferrer)
        {
            _Referrer = newReferrer;
        }

        public void SetDomain(string newDomain)
        {
            _Domain = newDomain;
        }

        #endregion

        #region Parsing

        private void ParseDocument(HtmlAgilityPack.HtmlDocument document)
        {
            if (DocumentElement != null)
            {
                RemoveChild(DocumentElement);
            }

            foreach (var child in document.DocumentNode.ChildNodes)
            {
                var newNode = ParseNode(child);

                if (newNode != null && !ChildNodes.Contains(newNode))
                {
                    AppendChild(newNode);
                }
            }
        }

        private Node ParseNode(HtmlAgilityPack.HtmlNode node)
        {
            switch (node.NodeType)
            {
                case HtmlAgilityPack.HtmlNodeType.Document:
                    return ParseElement(node);
                case HtmlAgilityPack.HtmlNodeType.Element:
                    return ParseElement(node);
                case HtmlAgilityPack.HtmlNodeType.Text:
                    return ParseText(node);
                case HtmlAgilityPack.HtmlNodeType.Comment:
                    return ParseComment(node);
                default:
                    return null;
            }
        }

        private Element ParseElement(HtmlAgilityPack.HtmlNode element)
        {
            var result = CreateElement(element.Name);

            foreach (var attribute in element.Attributes)
            {
                string value = HttpUtility.HtmlDecode(attribute.Value);
                result.SetAttribute(attribute.Name, value);
            }

            foreach (var child in element.ChildNodes)
            {
                var newChild = ParseNode(child);

                if (newChild != null)
                {
                    result.AppendChild(newChild);
                }
            }

            return result;
        }

        private Text ParseText(HtmlAgilityPack.HtmlNode input)
        {
            string text = HtmlAgilityPack.HtmlEntity.DeEntitize(input.InnerText);

            return CreateTextNode(text);
        }

        private Comment ParseComment(HtmlAgilityPack.HtmlNode input)
        {
            string text = HtmlAgilityPack.HtmlEntity.DeEntitize(input.InnerText);

            if (text.StartsWith("<!--") && text.EndsWith("-->"))
            {
                text = text.Remove(0, 4);
                text = text.Remove(text.Length - 3);
            }

            return CreateComment(text);
        }

        #endregion

        #region Formatting

        private HtmlAgilityPack.HtmlDocument FormatDocument()
        {
            var result = new HtmlAgilityPack.HtmlDocument();

            foreach (NodeBase child in this.ChildNodes)
            {
                var newChild = FormatNode(child, result);

                if (newChild != null)
                {
                    result.DocumentNode.AppendChild(newChild);
                }
            }

            return result;
        }

        private HtmlAgilityPack.HtmlNode FormatNode(NodeBase node, HtmlAgilityPack.HtmlDocument document)
        {
            switch (node.NodeType)
            {
                case DOM.NodeType.DOCUMENT_TYPE_NODE:
                    return FormatDocumentType((DocumentTypeBase)node, document);
                case DOM.NodeType.ELEMENT_NODE:
                    return FormatElement((ElementBase)node, document);
                case DOM.NodeType.TEXT_NODE:
                    return FormatText((TextBase)node, document);
                case DOM.NodeType.COMMENT_NODE:
                    return FormatComment((CommentBase)node, document);
                default:
                    return null;
            }
        }

        private HtmlAgilityPack.HtmlNode FormatDocumentType(DocumentTypeBase docType, HtmlAgilityPack.HtmlDocument document)
        {
            string text = SGMLFormatter.FormatDocumentType(docType);

            return document.CreateComment(text);
        }

        private HtmlAgilityPack.HtmlNode FormatElement(ElementBase element, HtmlAgilityPack.HtmlDocument document)
        {
            var result = document.CreateElement(element.TagName);

            // Children

            foreach (NodeBase child in element.ChildNodes)
            {
                var newChild = FormatNode(child, document);
                if (newChild != null)
                {
                    result.AppendChild(newChild);
                }
            }

            // Attributes

            if (element is HTMLElementBase)
            {
                ((HTMLElementBase)element).UpdateStyle(false);
            }

            foreach (AttrBase attr in element.Attributes)
            {
                string value = HttpUtility.HtmlAttributeEncode(attr.Value);
                if (!String.IsNullOrEmpty(value))
                {
                    result.SetAttributeValue(attr.Name, value);
                }
            }

            return result;
        }

        private HtmlAgilityPack.HtmlNode FormatText(TextBase element, HtmlAgilityPack.HtmlDocument document)
        {
            string escapedText = HtmlAgilityPack.HtmlEntity.Entitize(element.Data);
            escapedText = escapedText.Replace("<", "&lt;");
            escapedText = escapedText.Replace(">", "&gt;");
            return document.CreateTextNode(escapedText);
        }

        private HtmlAgilityPack.HtmlNode FormatComment(CommentBase element, HtmlAgilityPack.HtmlDocument document)
        {
            string escapedText = HtmlAgilityPack.HtmlEntity.Entitize(element.Data);
            escapedText = escapedText.Replace("<", "&lt;");
            escapedText = escapedText.Replace(">", "&gt;");
            escapedText = "<!-- " + escapedText.Trim() + " -->";
            return document.CreateComment(escapedText);
        }

        #endregion

        #region Implementation

        HTMLHeadElement GetOrCreateHead()
        {
            if (_Head != null)
            {
                return _Head;
            }

            var head = (HTMLHeadElement)CreateElement(HTMLElements.HEAD);

            DocumentElement.InsertBefore(head, Body);

            return head;
        }

        HTMLTitleElement GetOrCreateTitle()
        {
            if (_Title != null)
            {
                return _Title;
            }

            var title = (HTMLTitleElement)CreateElement(HTMLElements.TITLE);

            GetOrCreateHead().AppendChild(title);

            return title;
        }

        protected override Element OnCreateElement(string tagName)
        {
            switch (tagName.ToLower())
            {
                case HTMLElements.A:
                    return new HTMLAnchorElementBase(this);
                case HTMLElements.APPLET:
                    return new HTMLAppletElementBase(this);
                case HTMLElements.AREA:
                    return new HTMLAreaElementBase(this);
                case HTMLElements.BASE:
                    return new HTMLBaseElementBase(this);
                case HTMLElements.BASEFONT:
                    return new HTMLBaseFontElementBase(this);
                case HTMLElements.BLOCKQUOTE:
                    return new HTMLQuoteElementBase(tagName, this);
                case HTMLElements.BODY:
                    return new HTMLBodyElementBase(this);
                case HTMLElements.BR:
                    return new HTMLBRElementBase(this);
                case HTMLElements.BUTTON:
                    return new HTMLButtonElementBase(this);
                case HTMLElements.CAPTION:
                    return new HTMLTableCaptionElementBase(this);
                case HTMLElements.COL:
                    return new HTMLColElementBase(this);
                case HTMLElements.DEL:
                    return new HTMLModElementBase(tagName, this);
                case HTMLElements.DIR:
                    return new HTMLDirectoryElementBase(this);
                case HTMLElements.DIV:
                    return new HTMLDivElementBase(this);
                case HTMLElements.DL:
                    return new HTMLDListElementBase(this);
                case HTMLElements.FIELDSET:
                    return new HTMLFieldSetElementBase(this);
                case HTMLElements.FONT:
                    return new HTMLFontElementBase(this);
                case HTMLElements.FORM:
                    return new HTMLFormElementBase(this);
                case HTMLElements.FRAME:
                    return new HTMLFrameElementBase(this);
                case HTMLElements.FRAMESET:
                    return new HTMLFrameSetElementBase(this);
                case HTMLElements.H1:
                case HTMLElements.H2:
                case HTMLElements.H3:
                case HTMLElements.H4:
                case HTMLElements.H5:
                case HTMLElements.H6:
                    return new HTMLHeadingElementBase(tagName, this);
                case HTMLElements.HEAD:
                    return new HTMLHeadElementBase(this);
                case HTMLElements.HR:
                    return new HTMLHRElementBase(this);
                case HTMLElements.HTML:
                    return new HTMLHtmlElementBase(this);
                case HTMLElements.IFRAME:
                    return new HTMLIFrameElementBase(this);
                case HTMLElements.IMG:
                    return new HTMLImageElementBase(this);
                case HTMLElements.INPUT:
                    return new HTMLInputElementBase(this);
                case HTMLElements.INS:
                    return new HTMLModElementBase(tagName, this);
                case HTMLElements.ISINDEX:
                    return new HTMLIsIndexElementBase(this);
                case HTMLElements.LABEL:
                    return new HTMLLabelElementBase(this);
                case HTMLElements.LEGEND:
                    return new HTMLLegendElementBase(this);
                case HTMLElements.LI:
                    return new HTMLLIElementBase(this);
                case HTMLElements.LINK:
                    return new HTMLLinkElementImpl(this);
                case HTMLElements.MAP:
                    return new HTMLMapElementBase(this);
                case HTMLElements.MENU:
                    return new HTMLMenuElementBase(this);
                case HTMLElements.META:
                    return new HTMLMetaElementBase(this);
                case HTMLElements.OBJECT:
                    return new HTMLObjectElementBase(this);
                case HTMLElements.OL:
                    return new HTMLOListElementBase(this);
                case HTMLElements.OPTGROUP:
                    return new HTMLOptGroupElementBase(this);
                case HTMLElements.OPTION:
                    return new HTMLOptionElementBase(this);
                case HTMLElements.P:
                    return new HTMLParagraphElementBase(this);
                case HTMLElements.PARAM:
                    return new HTMLParamElementBase(this);
                case HTMLElements.PRE:
                    return new HTMLPreElementBase(this);
                case HTMLElements.Q:
                    return new HTMLQuoteElementBase(tagName, this);
                case HTMLElements.SCRIPT:
                    return new HTMLScriptElementBase(this);
                case HTMLElements.SELECT:
                    return new HTMLSelectElementBase(this);
                case HTMLElements.STYLE:
                    return new HTMLStyleElementBase(this);
                case HTMLElements.TABLE:
                    return new HTMLTableElementBase(this);
                case HTMLElements.TBODY:
                case HTMLElements.THEAD:
                case HTMLElements.TFOOT:
                    return new HTMLTableSectionElementBase(tagName, this);
                case HTMLElements.TD:
                case HTMLElements.TH:
                    return new HTMLTableCellElementBase(tagName, this);
                case HTMLElements.TEXTAREA:
                    return new HTMLTextAreaElementBase(this);
                case HTMLElements.TR:
                    return new HTMLTableRowElementBase(this);
                case HTMLElements.TITLE:
                    return new HTMLTitleElementBase(this);
                case HTMLElements.UL:
                    return new HTMLUListElementBase(this);
                default:
                    return new HTMLElementBase(tagName, this);
            }
        }
        /*
        protected override Element OnCreateElementNS(string namespaceURI, string qualifiedName)
        {
            return OnCreateElement(qualifiedName);
        }
        */

        protected override void OnNodeAdded(Node node)
        {
            if (node.NodeType == NodeType.ELEMENT_NODE)
            {
                switch (node.NodeName)
                {
                    case HTMLElements.HEAD:
                        _Head = (HTMLHeadElement)node;
                        break;
                    case HTMLElements.TITLE:
                        _Title = (HTMLTitleElement)node;
                        break;
                    case HTMLElements.BODY:
                        _Body = (HTMLBodyElement)node;
                        break;
                    case HTMLElements.STYLE:
                        var style = (LinkStyle)node;
                        if (style.Sheet != null)
                            AddStyleSheet(style.Sheet);
                        break;
                    case HTMLElements.LINK:
                        Load((HTMLLinkElement)node);
                        var link = (LinkStyle)node;
                        if (link.Sheet != null)
                            AddStyleSheet(link.Sheet);
                        break;
                }
            }
        }

        protected override void OnNodeRemoved(Node node)
        {
            if (node.NodeType == NodeType.ELEMENT_NODE)
            {
                switch (node.NodeName)
                {
                    case HTMLElements.HEAD:
                        _Head = null;
                        break;
                    case HTMLElements.TITLE:
                        _Title = null;
                        break;
                    case HTMLElements.BODY:
                        _Body = null;
                        break;
                    case HTMLElements.STYLE:
                        var style = (LinkStyle)node;
                        if (style.Sheet != null)
                            RemoveStyleSheet(style.Sheet);
                        break;
                    case HTMLElements.LINK:
                        var link = (LinkStyle)node;
                        if (link.Sheet != null)
                            RemoveStyleSheet(link.Sheet);
                        break;
                }
            }
        }

        private void AddStyleSheet(StyleSheet styleSheet)
        {
            if (_StyleSheets == null)
            {
                _StyleSheets = new MutableStyleSheetList();
            }
            _StyleSheets.AppendSheet(styleSheet);
        }

        private void RemoveStyleSheet(StyleSheet styleSheet)
        {
            if (_StyleSheets != null)
            {
                _StyleSheets.RemoveSheet(styleSheet);
            }
        }

        private void Load(HTMLLinkElement link)
        {
            if (link.Rel.Equals("stylesheet", StringComparison.OrdinalIgnoreCase))
            {
                string url = ResolveURL(link.HRef);
                System.Diagnostics.Debug.WriteLine("Loading " + url);
                try
                {
                    var request = System.Net.WebRequest.Create(url);
                    var response = request.GetResponse();
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();
                            var styleSheet = CSSParser.ParseStylesheet(result);
                            ((HTMLLinkElementImpl)link).SetStyleSheet(styleSheet);
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
        }

        #endregion

        #region Object

        public override string ToString()
        {
            var html = FormatDocument();

            using (var writer = new StringWriter())
            {
                html.Save(writer);
                return writer.ToString();
            }
        }

        #endregion

    }

}
