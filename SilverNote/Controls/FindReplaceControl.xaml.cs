/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Media.Animation;

namespace SilverNote.Controls
{
    /// <summary>
    /// Interaction logic for FindReplaceControl.xaml
    /// </summary>
    public partial class FindReplaceControl : UserControl
    {
        public FindReplaceControl()
        {
            InitializeComponent();
            
            DependencyPropertyDescriptor
                .FromProperty(UIElement.VisibilityProperty, typeof(UIElement))
                .AddValueChanged(this, OnVisibilityChanged);
        }

        #region Dependency Properties

        #region FindText

        public static readonly DependencyProperty FindTextProperty = DependencyProperty.Register(
            "FindText",
            typeof(string),
            typeof(FindReplaceControl),
            new PropertyMetadata(null)
        );

        public string FindText
        {
            get { return (string)GetValue(FindTextProperty); }
            set { SetValue(FindTextProperty, value); }
        }

        #endregion

        #region ReplaceText

        public static readonly DependencyProperty ReplaceTextProperty = DependencyProperty.Register(
            "ReplaceText",
            typeof(string),
            typeof(FindReplaceControl),
            new PropertyMetadata(null)
        );

        public string ReplaceText
        {
            get { return (string)GetValue(ReplaceTextProperty); }
            set { SetValue(ReplaceTextProperty, value); }
        }

        #endregion

        #region IsReplaceEnabled

        public static readonly DependencyProperty IsReplaceEnabledProperty = DependencyProperty.Register(
            "IsReplaceEnabled",
            typeof(bool),
            typeof(FindReplaceControl),
            new PropertyMetadata(false)
        );

        public bool IsReplaceEnabled
        {
            get { return (bool)GetValue(IsReplaceEnabledProperty); }
            set { SetValue(IsReplaceEnabledProperty, value); }
        }

        #endregion

        #endregion

        #region Routed Events

        #region FindTextChanged

        public static readonly RoutedEvent FindTextChangedEvent = EventManager.RegisterRoutedEvent(
            "FindTextChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(FindReplaceControl)
        );

        public event RoutedEventHandler FindTextChanged
        {
            add { AddHandler(FindTextChangedEvent, value); }
            remove { RemoveHandler(FindTextChangedEvent, value); }
        }

        void RaiseFindTextChangedEvent()
        {
            RaiseEvent(new RoutedEventArgs(FindTextChangedEvent));
        }

        #endregion

        #region FindPreviousPressed

        public static readonly RoutedEvent FindPreviousPressedEvent = EventManager.RegisterRoutedEvent(
            "FindPreviousPressed",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(FindReplaceControl)
        );

        public event RoutedEventHandler FindPreviousPressed
        {
            add { AddHandler(FindPreviousPressedEvent, value); }
            remove { RemoveHandler(FindPreviousPressedEvent, value); }
        }

        void RaiseFindPreviousPressedEvent()
        {
            RaiseEvent(new RoutedEventArgs(FindPreviousPressedEvent));

            if (FindPreviousCommand != null)
            {
                FindPreviousCommand.Execute(null, CommandTarget);
            }

            FindTextBox.Focus();
        }

        #endregion

        #region FindNextPressed

        public static readonly RoutedEvent FindNextPressedEvent = EventManager.RegisterRoutedEvent(
            "FindNextPressed",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(FindReplaceControl)
        );

        public event RoutedEventHandler FindNextPressed
        {
            add { AddHandler(FindNextPressedEvent, value); }
            remove { RemoveHandler(FindNextPressedEvent, value); }
        }

        void RaiseFindNextPressedEvent()
        {
            RaiseEvent(new RoutedEventArgs(FindNextPressedEvent));

            if (FindNextCommand != null)
            {
                FindNextCommand.Execute(null, CommandTarget);
            }

            FindTextBox.Focus();
        }

        #endregion

        #region ReplacePressed

        public static readonly RoutedEvent ReplacePressedEvent = EventManager.RegisterRoutedEvent(
            "ReplacePressed",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(FindReplaceControl)
        );

        public event RoutedEventHandler ReplacePressed
        {
            add { AddHandler(ReplacePressedEvent, value); }
            remove { RemoveHandler(ReplacePressedEvent, value); }
        }

        void RaiseReplacePressedEvent()
        {
            RaiseEvent(new RoutedEventArgs(ReplacePressedEvent));

            if (ReplaceOnceCommand != null)
            {
                ReplaceOnceCommand.Execute(null, CommandTarget);
            }
        }

        #endregion

        #region ReplaceAllPressed

        public static readonly RoutedEvent ReplaceAllPressedEvent = EventManager.RegisterRoutedEvent(
            "ReplaceAllPressed",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(FindReplaceControl)
        );

        public event RoutedEventHandler ReplaceAllPressed
        {
            add { AddHandler(ReplaceAllPressedEvent, value); }
            remove { RemoveHandler(ReplaceAllPressedEvent, value); }
        }

        void RaiseReplaceAllPressedEvent()
        {
            RaiseEvent(new RoutedEventArgs(ReplaceAllPressedEvent));

            if (ReplaceAllCommand != null)
            {
                ReplaceAllCommand.Execute(null, CommandTarget);
            }
        }

        #endregion

        #region Closed

        public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
            "Closed",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(FindReplaceControl)
        );

        public event RoutedEventHandler Closed
        {
            add { AddHandler(ClosedEvent, value); }
            remove { RemoveHandler(ClosedEvent, value); }
        }

        void RaiseClosedEvent()
        {
            RaiseEvent(new RoutedEventArgs(ClosedEvent));
        }

        #endregion

        #endregion

        #region Commands

        #region FindNext

        public static readonly DependencyProperty FindNextCommandProperty = DependencyProperty.Register(
            "FindNextCommand",
            typeof(RoutedUICommand),
            typeof(FindReplaceControl)
        );

        public RoutedUICommand FindNextCommand
        {
            get { return (RoutedUICommand)GetValue(FindNextCommandProperty); }
            set { SetValue(FindNextCommandProperty, value); }
        }

        #endregion

        #region FindPrevious

        public static readonly DependencyProperty FindPreviousCommandProperty = DependencyProperty.Register(
            "FindPreviousCommand",
            typeof(RoutedUICommand),
            typeof(FindReplaceControl)
        );

        public RoutedUICommand FindPreviousCommand
        {
            get { return (RoutedUICommand)GetValue(FindPreviousCommandProperty); }
            set { SetValue(FindPreviousCommandProperty, value); }
        }

        #endregion

        #region ReplaceOnce

        public static readonly DependencyProperty ReplaceOnceCommandProperty = DependencyProperty.Register(
            "ReplaceOnceCommand",
            typeof(RoutedUICommand),
            typeof(FindReplaceControl)
        );

        public RoutedUICommand ReplaceOnceCommand
        {
            get { return (RoutedUICommand)GetValue(ReplaceOnceCommandProperty); }
            set { SetValue(ReplaceOnceCommandProperty, value); }
        }

        #endregion

        #region ReplaceAll

        public static readonly DependencyProperty ReplaceAllCommandProperty = DependencyProperty.Register(
            "ReplaceAllCommand",
            typeof(RoutedUICommand),
            typeof(FindReplaceControl)
        );

        public RoutedUICommand ReplaceAllCommand
        {
            get { return (RoutedUICommand)GetValue(ReplaceAllCommandProperty); }
            set { SetValue(ReplaceAllCommandProperty, value); }
        }

        #endregion

        #region CommandTarget

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
            "CommandTarget",
            typeof(IInputElement),
            typeof(FindReplaceControl)
        );

        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        #endregion

        #endregion

        #region Implementation

        protected void OnVisibilityChanged(object sender, EventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                if (!IsMeasureValid)
                {
                    UpdateLayout();
                }

                SlideIn();
            }
        }

        private void SlideIn()
        {
            var slideIn = new DoubleAnimation(-DesiredSize.Height, 0, TimeSpan.FromSeconds(0.1));
            slideIn.Completed += SlideIn_Completed;
            Translate.BeginAnimation(TranslateTransform.YProperty, slideIn);
        }

        private void SlideIn_Completed(object sender, EventArgs e)
        {
            FindTextBox.SelectAll();
            FindTextBox.Focus();
        }

        private void Close()
        {
            SlideOut();
        }

        private void SlideOut()
        {
            var slideOut = new DoubleAnimation(0, -DesiredSize.Height, TimeSpan.FromSeconds(0.1));
            slideOut.Completed += SlideOut_Completed;
            Translate.BeginAnimation(TranslateTransform.YProperty, slideOut);
        }

        private void SlideOut_Completed(object sender, EventArgs e)
        {
            RaiseClosedEvent();
        }

        private void FindText_TextChanged(object sender, TextChangedEventArgs e)
        {
            RaiseFindTextChangedEvent();
        }

        private void FindText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    RaiseFindNextPressedEvent();
                }
                else
                {
                    RaiseFindPreviousPressedEvent();
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Close();

                e.Handled = true;
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseFindPreviousPressedEvent();

            e.Handled = true;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseFindNextPressedEvent();

            e.Handled = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();

            e.Handled = true;
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseReplacePressedEvent();
        }

        private void ReplaceAllButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseReplaceAllPressedEvent();
        }

        #endregion

    }
}
