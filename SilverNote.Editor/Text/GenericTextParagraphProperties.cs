/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using DOM;
using DOM.CSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.TextFormatting;

namespace SilverNote.Editor
{
    public class GenericTextParagraphProperties : TextParagraphProperties, ICloneable
    {
        #region Fields

        bool _AlwaysCollapsible;
        double _DefaultIncrementalTab;
        TextRunProperties _DefaultTextRunProperties;
        bool _FirstLineInParagraph;
        FlowDirection _FlowDirection;
        double _Indent;
        double _LineHeight;
        double _ParagraphIndent;
        IList<TextTabProperties> _Tabs;
        TextAlignment _TextAlignment;
        TextDecorationCollection _TextDecorations;
        TextMarkerProperties _TextMarkerProperties;
        TextWrapping _TextWrapping;

        #endregion

        #region Constructors

        public GenericTextParagraphProperties()
        {
            _DefaultIncrementalTab = Double.NaN;
            _FlowDirection = FlowDirection.LeftToRight;
            _TextAlignment = TextAlignment.Left;
            _TextWrapping = TextWrapping.Wrap;
        }

        public GenericTextParagraphProperties(TextParagraphProperties copy)
        {
            _AlwaysCollapsible = copy.AlwaysCollapsible;
            _DefaultIncrementalTab = copy.DefaultIncrementalTab;
            _DefaultTextRunProperties = copy.DefaultTextRunProperties;
            _FirstLineInParagraph = copy.FirstLineInParagraph;
            _FlowDirection = copy.FlowDirection;
            _Indent = copy.Indent;
            _LineHeight = copy.LineHeight;
            _ParagraphIndent = copy.ParagraphIndent;
            _Tabs = copy.Tabs;
            _TextAlignment = copy.TextAlignment;
            _TextDecorations = copy.TextDecorations;
            _TextMarkerProperties = copy.TextMarkerProperties;
            _TextWrapping = copy.TextWrapping;

            if (_DefaultTextRunProperties is ICloneable)
            {
                _DefaultTextRunProperties = (TextRunProperties)((ICloneable)_DefaultTextRunProperties).Clone();
            }

            if (_TextDecorations != null && !_TextDecorations.IsFrozen)
            {
                _TextDecorations = _TextDecorations.Clone();
            }
        }

        #endregion

        #region Properties

        public override bool AlwaysCollapsible
        {
            get { return _AlwaysCollapsible; }
        }

        public override double DefaultIncrementalTab
        {
            get
            {
                if (!Double.IsNaN(_DefaultIncrementalTab))
                {
                    return _DefaultIncrementalTab;
                }
                else
                {
                    return 4 * DefaultTextRunProperties.FontRenderingEmSize;
                }
            }
        }

        public override TextRunProperties DefaultTextRunProperties
        {
            get { return _DefaultTextRunProperties; }
        }

        public override bool FirstLineInParagraph
        {
            get { return _FirstLineInParagraph; }
        }

        public override FlowDirection FlowDirection
        {
            get { return _FlowDirection; }
        }

        public override double Indent
        {
            get { return _Indent; }
        }

        public override double LineHeight
        {
            get { return _LineHeight; }
        }

        public override double ParagraphIndent
        {
            get { return _ParagraphIndent; }
        }

        public override IList<TextTabProperties> Tabs
        {
            get { return _Tabs; }
        }

        public override TextAlignment TextAlignment
        {
            get { return _TextAlignment; }
        }

        public override TextDecorationCollection TextDecorations
        {
            get { return _TextDecorations; }
        }

        public override TextMarkerProperties TextMarkerProperties
        {
            get { return _TextMarkerProperties; }
        }

        public override TextWrapping TextWrapping
        {
            get { return _TextWrapping; }
        }

        #endregion

        #region Methods

        public void SetAlwaysCollapsible(bool alwaysCollapsible)
        {
            _AlwaysCollapsible = alwaysCollapsible;
        }

        public void SetDefaultIncrementalTab(double defaultIncrementalTab)
        {
            _DefaultIncrementalTab = defaultIncrementalTab;
        }

        public void SetDefaultTextRunProperties(TextRunProperties defaultTextRunProperties)
        {
            _DefaultTextRunProperties = defaultTextRunProperties;
        }

        /// <summary>
        /// Not a "real" property - used internally by the formatter to maintain state
        /// </summary>
        /// <param name="firstLineInParagraph"></param>
        public void SetFirstLineInParagraph(bool firstLineInParagraph)
        {
            _FirstLineInParagraph = firstLineInParagraph;
        }

        public void SetFlowDirection(FlowDirection flowDirection)
        {
            _FlowDirection = flowDirection;
        }

        public void SetIndent(double indent)
        {
            _Indent = indent;
        }

        public void SetLineHeight(double lineHeight)
        {
            _LineHeight = lineHeight;
        }

        public void SetParagraphIndent(double paragraphIndent)
        {
            _ParagraphIndent = paragraphIndent;
        }

        public void SetTabs(IList<TextTabProperties> tabs)
        {
            _Tabs = tabs;
        }

        public void SetTextAlignment(TextAlignment textAlignment)
        {
            _TextAlignment = textAlignment;
        }

        public void SetTextDecorations(TextDecorationCollection textDecorations)
        {
            _TextDecorations = textDecorations;
        }

        public void SetTextMarkerProperties(TextMarkerProperties textMarkerProperties)
        {
            _TextMarkerProperties = textMarkerProperties;
        }

        public void SetTextWrapping(TextWrapping textWrapping)
        {
            _TextWrapping = textWrapping;
        }

        #endregion

        #region ICloneable

        public virtual object Clone()
        {
            return new GenericTextParagraphProperties(this);
        }

        #endregion
    }
}
