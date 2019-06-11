/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using SilverNote.Editor;
using SilverNote.ViewModels;

namespace SilverNote.Controls
{
    /// <summary>
    /// Interaction logic for ClipartPicker.xaml
    /// </summary>
    public partial class ClipartPicker : UserControl
    {
        public ClipartPicker()
        {
            InitializeComponent();
        }

        #region Dependency Properties

        #region ClipartGroups

        public static readonly DependencyProperty ClipartGroupsProperty = DependencyProperty.Register(
            "ClipartGroups",
            typeof(ClipartGroupViewModel[]),
            typeof(ClipartPicker),
            new FrameworkPropertyMetadata(null)
        );

        public ClipartGroupViewModel[] ClipartGroups
        {
            get { return (ClipartGroupViewModel[])GetValue(ClipartGroupsProperty); }
            set { SetValue(ClipartGroupsProperty, value); }
        }

        #endregion

        #region SelectedClipart

        public static readonly DependencyProperty SelectedClipartProperty = DependencyProperty.Register(
            "SelectedClipart",
            typeof(Shape),
            typeof(ClipartPicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public Shape SelectedClipart
        {
            get { return (Shape)GetValue(SelectedClipartProperty); }
            set { SetValue(SelectedClipartProperty, value); }
        }

        #endregion

        #region CreateGroupCommand

        public static readonly DependencyProperty CreateGroupCommandProperty = DependencyProperty.Register(
            "CreateGroupCommand",
            typeof(ICommand),
            typeof(ClipartPicker)
        );

        public ICommand CreateGroupCommand
        {
            get { return (ICommand)GetValue(CreateGroupCommandProperty); }
            set { SetValue(CreateGroupCommandProperty, value); }
        }

        #endregion

        #region DeleteGroupCommand

        public static readonly DependencyProperty DeleteGroupCommandProperty = DependencyProperty.Register(
            "DeleteGroupCommand",
            typeof(ICommand),
            typeof(ClipartPicker)
        );

        public ICommand DeleteGroupCommand
        {
            get { return (ICommand)GetValue(DeleteGroupCommandProperty); }
            set { SetValue(DeleteGroupCommandProperty, value); }
        }

        #endregion

        #region SelectClipartCommand

        public static readonly DependencyProperty SelectClipartCommandProperty = DependencyProperty.Register(
            "SelectClipartCommand",
            typeof(RoutedUICommand),
            typeof(ClipartPicker)
        );

        public RoutedUICommand SelectClipartCommand
        {
            get { return (RoutedUICommand)GetValue(SelectClipartCommandProperty); }
            set { SetValue(SelectClipartCommandProperty, value); }
        }

        #endregion

        #region CommandTarget

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
            "CommandTarget",
            typeof(IInputElement),
            typeof(ClipartPicker)
        );

        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        #endregion

        #endregion

        #region Implementation

        private void Clipart_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item != null)
            {
                ClipartViewModel clipart = item.Content as ClipartViewModel;
                if (clipart != null)
                {
                    SelectedClipart = clipart.Drawing;

                    if (SelectClipartCommand != null)
                    {
                        SelectClipartCommand.Execute(clipart.Drawing, CommandTarget);
                    }
                }
            }
        }

        private void AddGroupBackground_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void AddGroupBackground_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AddGroupTextBox.Focus();

            e.Handled = true;
        }

        private void AddGroupTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            AddGroupButton.Visibility = Visibility.Visible;
        }

        private void AddGroupTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (AddGroupTextBox.Text.Length == 0)
            {
                AddGroupButton.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            string name = AddGroupTextBox.Text;
            CreateGroupCommand.Execute(name);
            AddGroupTextBox.Text = String.Empty;
        }

        #endregion

    }
}
