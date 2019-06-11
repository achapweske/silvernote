/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SilverNote.Controls
{
    /// <summary>
    /// Interaction logic for HighlighterPicker.xaml
    /// </summary>
    public partial class HighlighterPicker : UserControl
    {
        public HighlighterPicker()
        {
            Items = new ObservableCollection<Brush>();

            Items.Add(Brushes.Transparent);
            Items.Add(Brushes.LightGray);
            Items.Add(Brushes.Red);
            Items.Add(Brushes.Yellow);
            Brush neonGreen = new SolidColorBrush(Color.FromRgb(0x7F, 0xFF, 0x00));
            Items.Add(neonGreen);
            Items.Add(Brushes.Aqua);
            Items.Add(Brushes.Violet);

            InitializeComponent();
        }

        public ObservableCollection<Brush> Items { get; private set; }

        #region SelectedItem

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem",
            typeof(Brush),
            typeof(HighlighterPicker),
            new FrameworkPropertyMetadata(Brushes.Yellow, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public Brush SelectedItem
        {
            get { return (Brush)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        #endregion

        #region ItemClickedEvent

        public static readonly RoutedEvent ItemClickedEvent = EventManager.RegisterRoutedEvent (
            "ItemClicked", 
            RoutingStrategy.Bubble, 
            typeof(RoutedEventHandler), 
            typeof(HighlighterPicker)
        );

        public event RoutedEventHandler ItemClicked
        {
            add { AddHandler(ItemClickedEvent, value); }
            remove { RemoveHandler(ItemClickedEvent, value); }
        }

        #endregion

        private void ListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ItemClickedEvent));
        }
    }
}
