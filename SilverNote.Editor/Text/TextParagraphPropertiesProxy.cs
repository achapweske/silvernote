/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.TextFormatting;

namespace SilverNote.Editor
{
    public class TextParagraphPropertiesProxy : TextParagraphProperties
    {
        #region Fields

        private readonly ITextParagraphPropertiesSource _Source;
        private bool _FirstLineInParagraph;

        #endregion

        #region Constructors

        public TextParagraphPropertiesProxy(ITextParagraphPropertiesSource source)
        {
            _Source = source;
        }

        #endregion

        #region Properties

        public ITextParagraphPropertiesSource Source
        {
            get { return _Source; }
        }

        public override bool AlwaysCollapsible
        {
            get { return _Source.AlwaysCollapsible; }
        }

        public override double DefaultIncrementalTab
        {
            get 
            {
                if (!Double.IsNaN(_Source.DefaultIncrementalTab))
                {
                    return _Source.DefaultIncrementalTab;
                }
                else
                {
                    return _Source.TabSize * _Source.ColumnWidth;
                }
            }
        }

        public override TextRunProperties DefaultTextRunProperties
        {
            get { return _Source.DefaultTextRunProperties; }
        }

        public override bool FirstLineInParagraph
        {
            get { return _FirstLineInParagraph; }
        }

        public override FlowDirection FlowDirection
        {
            get { return _Source.FlowDirection; }
        }

        public override double Indent
        {
            get { return _Source.Indent; }
        }

        public override double LineHeight
        {
            get { return _Source.LineHeight; }
        }

        public override double ParagraphIndent
        {
            get { return _Source.ParagraphIndent; }
        }

        public override IList<TextTabProperties> Tabs
        {
            get { return _Source.Tabs; }
        }

        public override TextAlignment TextAlignment
        {
            get { return _Source.TextAlignment; }
        }

        public override TextDecorationCollection TextDecorations
        {
            get { return _Source.TextDecorations; }
        }

        public override TextMarkerProperties TextMarkerProperties
        {
            get { return _Source.TextMarkerProperties; }
        }

        public override TextWrapping TextWrapping
        {
            get { return _Source.TextWrapping; }
        }

        #endregion

        #region Methods

        public void SetFirstLineInParagraph(bool firstLineInParagraph)
        {
            _FirstLineInParagraph = firstLineInParagraph;
        }

        #endregion

    }

    public interface ITextParagraphPropertiesSource
    {
        bool AlwaysCollapsible { get; set; }
        double ColumnWidth { get; }
        double DefaultIncrementalTab { get; set; }
        TextRunProperties DefaultTextRunProperties { get; set; }
        FlowDirection FlowDirection { get; set; }
        double Indent { get; set; }
        double LineHeight { get; set; }
        double ParagraphIndent { get; set; }
        IList<TextTabProperties> Tabs { get; set; }
        int TabSize { get; set; }
        TextAlignment TextAlignment { get; set; }
        TextDecorationCollection TextDecorations { get; set; }
        TextMarkerProperties TextMarkerProperties { get; set; }
        TextWrapping TextWrapping { get; set; }
    }
}
