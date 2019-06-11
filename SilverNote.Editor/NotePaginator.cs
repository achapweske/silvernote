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
using System.Windows.Media;
using System.Windows.Documents;
using System.Diagnostics;

namespace SilverNote.Editor
{
    class NotePaginator : DocumentPaginator
    {
        public NotePaginator(NoteEditor document)
        {
            Document = document;
            Margins = new Thickness(left: 1.25 * 96, top: 1.00 * 96, right: 1.25 * 96, bottom: 1.00 * 96);
        }

        NoteEditor Document { get; set; }

        DocumentPage[] Pages { get; set; }

        public void Paginate()
        {
            var pages = new List<DocumentPage>();

            // TODO: We have to unselect all in order to transfer drawings from the DrawingBoard
            // to individual canvases. This is a bit of a hack.
            Document.Selection.UnselectAll();
            Document.SelectDefault(Positioning.Static);

            DocumentPanel panel = new DocumentPanel();
            panel.Padding = Margins;
            foreach (UIElement element in Document.Children)
            {
                if (element is PrimaryCanvas)
                {
                    continue;
                }

                if (!(element is ICloneable))
                {
                    Debug.WriteLine("Warning: Paginator skipping non-cloneable element " + element);
                    continue;
                }

                UIElement clone = (UIElement)((ICloneable)element).Clone();
                panel.Children.Add(clone);
                panel.Measure(PageSize);

                if (panel.DesiredSize.Height >= PageSize.Height &&
                    panel.Children.Count > 1)
                {
                    panel.Children.Remove(clone);
                    panel.Measure(PageSize);
                    panel.Arrange(new Rect(new Point(0, 0), PageSize));
                    pages.Add(new DocumentPage(panel, PageSize, BleedBox, ContentBox));
                    
                    panel = new DocumentPanel();
                    panel.Padding = Margins;
                    panel.Children.Add(clone);
                    panel.Measure(PageSize);
                }

                if (panel.DesiredSize.Height >= PageSize.Height &&
                    panel.Children.Count == 1)
                {
                    ScaleTransform scale = new ScaleTransform();
                    scale.CenterX = 0;
                    scale.CenterY = 0;
                    scale.ScaleY = PageSize.Height / panel.DesiredSize.Height;
                    scale.ScaleX = scale.ScaleY;
                    panel.RenderTransform = scale;
                    panel.Arrange(new Rect(new Point(0, 0), panel.DesiredSize));
                    pages.Add(new DocumentPage(panel, PageSize, BleedBox, ContentBox));
                    panel = new DocumentPanel();
                    panel.Padding = Margins;
                }
            }

            if (panel.Children.Count > 0)
            {
                panel.Arrange(new Rect(new Point(0, 0), panel.DesiredSize));
                pages.Add(new DocumentPage(panel, PageSize, BleedBox, ContentBox));
            }

            Pages = pages.ToArray();
        }

        #region DocumentPaginator

        public override IDocumentPaginatorSource Source
        {
            get { return Document; }
        }

        public Thickness Margins { get; set; }

        private Size pageSize;
        public override Size PageSize 
        {
            get { return pageSize; }
            set
            {
                pageSize = value;
                Paginate();
            }
        }

        public Rect ContentBox
        {
            get
            {
                double left = Margins.Left;
                double top = Margins.Top;
                double right = PageSize.Width - Margins.Right;
                double bottom = PageSize.Height - Margins.Bottom;
                double width = Math.Max(right - left, 0);
                double height = Math.Max(bottom - top, 0);

                return new Rect(left, top, width, height);
            }
        }

        public Rect BleedBox
        {
            get
            {
                return new Rect(PageSize);
            }
        }

        public override bool IsPageCountValid 
        {
            get { return Pages != null; } 
        }

        public override int PageCount 
        {
            get { return Pages.Length; } 
        }

        public override DocumentPage GetPage(int pageNumber)
        {
            return Pages[pageNumber];
        }

        #endregion
    }
}
