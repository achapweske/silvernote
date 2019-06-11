/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Editor;
using SilverNote.ViewModels;
using System.Windows;
using System.Windows.Media;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for AddToLibraryDialog.xaml
    /// </summary>
    public partial class AddToLibraryDialog : Window
    {
        public AddToLibraryDialog()
        {
            InitializeComponent();
        }

        public Shape Drawing { get; set; }

        public static readonly DependencyProperty ClipartGroupsProperty = DependencyProperty.Register(
            "ClipartGroups",
            typeof(ClipartGroupViewModel[]),
            typeof(AddToLibraryDialog),
            new FrameworkPropertyMetadata(null)
        );

        public ClipartGroupViewModel[] ClipartGroups
        {
            get { return (ClipartGroupViewModel[])GetValue(ClipartGroupsProperty); }
            set { SetValue(ClipartGroupsProperty, value); }
        }

        public ClipartGroupViewModel SelectedGroup { get; set; }

        new public string Name { get; set; }

        public bool IsMarker { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PreviewCanvas.Drawing = Drawing.Preview;
            LineMarkerCheckBox.IsChecked = IsMarker;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedGroup = (ClipartGroupViewModel)GroupsComboBox.SelectedItem;
            Name = NameTextBox.Text;
            IsMarker = LineMarkerCheckBox.IsChecked == true;

            DialogResult = true;
        }
    }
}
