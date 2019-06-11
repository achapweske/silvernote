/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SilverNote.Editor
{
    /// <summary>
    /// Interaction logic for SectionToggleButton.xaml
    /// </summary>
    public partial class SectionToggleButton : ToggleButton
    {
        #region Fields

        int _HeadingLevel;

        #endregion

        #region Constructors

        public SectionToggleButton()
        {
            InitializeComponent();
            HeadingLevel = 2;
        }

        #endregion

        #region Properties

        public int HeadingLevel
        {
            get
            {
                return _HeadingLevel;
            }
            set
            {
                switch (value)
                {
                    case 2:
                        this.Template = (ControlTemplate)FindResource("SectionTemplate");
                        break;
                    case 3:
                        this.Template = (ControlTemplate)FindResource("SubsectionTemplate");
                        break;
                    default:
                        this.Template = (ControlTemplate)FindResource("SectionTemplate");
                        break;
                }
                _HeadingLevel = value;
            }
        }

        #endregion

        #region Events

        public event EventHandler Dragging;

        protected void RaiseDragging()
        {
            if (Dragging != null)
            {
                Dragging(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Implementation

        Point? _DragStartPosition;

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _DragStartPosition = e.GetPosition(this);

            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            _DragStartPosition = null;

            base.OnPreviewMouseLeftButtonUp(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (_DragStartPosition == null)
            {
                base.OnPreviewMouseMove(e);
                return;
            }

            var delta = _DragStartPosition.Value - e.GetPosition(this);
            if (delta.Length > 2)
            {
                _DragStartPosition = null;

                RaiseDragging();
            }

            e.Handled = true;
        }

        #endregion

    }
}
