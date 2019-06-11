/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SilverNote.Views
{
    /// <summary>
    /// Interaction logic for ClientView.xaml
    /// </summary>
    public partial class ClientView : UserControl
    {
        public ClientView()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        GridLength _RequestExpanderHeight = GridLength.Auto;
        GridLength _ResponseExpanderHeight = GridLength.Auto;

        private void RequestExpander_Expanded(object sender, RoutedEventArgs e)
        {
            if (_RequestExpanderHeight.IsAuto)
            {
                RequestExpanderRow.Height = new GridLength(ActualHeight * 0.125);
            }
            else
            {
                RequestExpanderRow.Height = _RequestExpanderHeight;
            }
        }

        private void RequestExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            _RequestExpanderHeight = RequestExpanderRow.Height;
            RequestExpanderRow.Height = GridLength.Auto;
        }

        private void ResponseExpander_Expanded(object sender, RoutedEventArgs e)
        {
            if (_ResponseExpanderHeight.IsAuto)
            {
                ResponseExpanderRow.Height = new GridLength(ActualHeight * .375);
            }
            else
            {
                ResponseExpanderRow.Height = _ResponseExpanderHeight;
            }
        }

        private void ResponseExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            _ResponseExpanderHeight = ResponseExpanderRow.Height;
            ResponseExpanderRow.Height = GridLength.Auto;
        }

        private void ResponseSplitter_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (!ResponseExpanderRow.Height.IsAbsolute)
            {
                ResponseExpanderRow.Height = new GridLength(ResponseExpanderRow.ActualHeight);
            }

            double newHeight = Math.Max(ResponseExpanderRow.Height.Value - e.VerticalChange, 20);
            ResponseExpanderRow.Height = new GridLength(newHeight);

            if (RequestExpander.IsExpanded)
            {
                if (!RequestExpanderRow.Height.IsAbsolute)
                {
                    RequestExpanderRow.Height = new GridLength(RequestExpanderRow.ActualHeight);
                }

                newHeight = Math.Max(RequestExpanderRow.Height.Value + e.VerticalChange, 20);
                RequestExpanderRow.Height = new GridLength(newHeight);
            }

            e.Handled = true;
        }
    }
}
