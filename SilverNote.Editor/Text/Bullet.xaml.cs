/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Commands;
using SilverNote.Common;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace SilverNote.Editor
{
    /// <summary>
    /// Interaction logic for Bullet.xaml
    /// </summary>
    public partial class Bullet : UserControl
    {
        #region Fields

        TextParagraph _Paragraph;
        Point? _DragStartPosition;

        #endregion

        #region Constructors

        public Bullet(TextParagraph paragraph)
        {
            _Paragraph = paragraph;
            InitializeComponent();

            TextBlock.FontFamily = paragraph.DefaultTextRunProperties.Typeface.FontFamily;
            TextBlock.FontSize = paragraph.DefaultTextRunProperties.FontRenderingEmSize;
        }

        #endregion

        #region Properties

        public string Text
        {
            get { return TextBlock.Text; }
            set { TextBlock.Text = value; }
        }

        #endregion

        #region Implementation

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (TextBlock.Effect == null)
            {
                TextBlock.Effect = new DropShadowEffect
                {
                    ShadowDepth = 0,
                    BlurRadius = 6,
                    Color = Colors.Black
                };
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (ContextMenu == null || !ContextMenu.IsOpen)
            {
                TextBlock.Effect = null;
                TextBlock.Opacity = 1.0;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _DragStartPosition = e.GetPosition(this);

                TextBlock.CaptureMouse();
            }

            TextBlock.Opacity = 0.5;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _DragStartPosition != null)
            {
                _DragStartPosition = null;

                TextBlock.ReleaseMouseCapture();

                ContextMenu = CreateBulletLeftClickMenu();
                ContextMenu.IsOpen = true;
                ContextMenu.Closed += BulletMenu_Closed;

                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                ContextMenu = CreateBulletRightClickMenu();
                ContextMenu.IsOpen = true;
                ContextMenu.Closed += BulletMenu_Closed;

                e.Handled = true;
            }
            else
            {
                TextBlock.Opacity = 1.0;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_DragStartPosition != null &&
                (_DragStartPosition.Value - e.GetPosition(this)).Length > 2)
            {
                _DragStartPosition = null;

                SetListOpacity(_Paragraph, 0.5);

                var items = _Paragraph.List.SelfAndDescendants.ToArray();
                IDataObject data = NDataObject.CreateDataObject(items);
                DragDropEffects result = DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
                if (result.HasFlag(DragDropEffects.Move))
                {
                    EditingPanel.DeleteCommand.Execute(items, this);
                }

                SetListOpacity(_Paragraph, 1.0);
            }

            e.Handled = true;
        }

        void SetListOpacity(TextParagraph paragraph, double opacity)
        {
            paragraph.Opacity = opacity;

            foreach (TextParagraph listItem in paragraph.List.Children)
            {
                SetListOpacity(listItem, opacity);
            }
        }

        ContextMenu CreateBulletRightClickMenu()
        {
            ContextMenu menu = new ContextMenu();
            MenuItem item;

            item = new MenuItem();
            item.Header = "\u2713 Check";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "check";
            item.CommandTarget = this;
            menu.Items.Add(item);

            menu.Items.Add(new Separator());

            item = new MenuItem();
            item.Header = "\u25CF Circle";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "disc";
            item.CommandTarget = this;
            menu.Items.Add(item);

            item = new MenuItem();
            item.Header = "\u25A0 Square";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "square";
            item.CommandTarget = this;
            menu.Items.Add(item);

            item = new MenuItem();
            item.Header = "\u25C6 Diamond";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "diamond";
            item.CommandTarget = this;
            menu.Items.Add(item);

            item = new MenuItem();
            item.Header = "\u25B6 Triangle";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "triangle";
            item.CommandTarget = this;
            menu.Items.Add(item);

            menu.Items.Add(new Separator());

            item = new MenuItem();
            item.Header = "\u25CB Circle";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "circle";
            item.CommandTarget = this;
            menu.Items.Add(item);

            item = new MenuItem();
            item.Header = "\u25A1 Square";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "open-square";
            item.CommandTarget = this;
            menu.Items.Add(item);

            item = new MenuItem();
            item.Header = "\u25C7 Diamond";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "open-diamond";
            item.CommandTarget = this;
            menu.Items.Add(item);

            item = new MenuItem();
            item.Header = "\u25B7 Triangle";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "open-triangle";
            item.CommandTarget = this;
            menu.Items.Add(item);

            menu.Items.Add(new Separator());

            item = new MenuItem();
            item.Header = "1, 2, 3";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "decimal";
            item.CommandTarget = this;
            menu.Items.Add(item);

            item = new MenuItem();
            item.Header = "a, b, c";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "lower-alpha";
            item.CommandTarget = this;
            menu.Items.Add(item);

            item = new MenuItem();
            item.Header = "i, ii, iii";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "lower-roman";
            item.CommandTarget = this;
            menu.Items.Add(item);

            item = new MenuItem();
            item.Header = "A, B, C";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "upper-alpha";
            item.CommandTarget = this;
            menu.Items.Add(item);

            item = new MenuItem();
            item.Header = "I, II, III";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "upper-roman";
            item.CommandTarget = this;
            menu.Items.Add(item);

            return menu;
        }

        ContextMenu CreateBulletLeftClickMenu()
        {
            ContextMenu menu = new ContextMenu();
            MenuItem item;

            item = new MenuItem();
            item.Header = "New Item";
            item.FontWeight = FontWeights.Bold;
            item.Command = NTextCommands.InsertParagraphAfter;
            item.CommandParameter = this;
            item.CommandTarget = this;
            item.InputGestureText = " ";
            menu.Items.Add(item);

            item = new MenuItem();
            item.Header = "Duplicate";
            item.Command = NTextCommands.DuplicateParagraph;
            item.CommandParameter = this;
            item.CommandTarget = this;
            item.InputGestureText = " ";
            menu.Items.Add(item);

            item = new MenuItem();
            item.Header = "Select";
            item.Command = NTextCommands.SelectParagraph;
            item.CommandParameter = this;
            item.CommandTarget = this;
            item.InputGestureText = " ";
            menu.Items.Add(item);

            menu.Items.Add(new Separator());

            // Cut
            item = new MenuItem();
            item.Header = "Cu_t";
            item.Command = NEditingCommands.Cut;
            item.CommandTarget = this;
            item.Icon = Images.GetImage("cut.png", 16, 16);
            menu.Items.Add(item);

            // Copy
            item = new MenuItem();
            item.Header = "_Copy";
            item.Command = NEditingCommands.Copy;
            item.CommandTarget = this;
            item.Icon = Images.GetImage("copy.png", 16, 16);
            menu.Items.Add(item);

            // Paste
            item = new MenuItem();
            item.Header = "_Paste";
            item.Command = NEditingCommands.Paste;
            item.CommandTarget = this;
            item.Icon = Images.GetImage("paste.png", 16, 16);
            menu.Items.Add(item);

            // Delete
            item = new MenuItem();
            item.Header = "_Delete";
            item.Command = NTextCommands.DeleteParagraph;
            item.CommandParameter = this;
            item.CommandTarget = this;
            item.Icon = Images.GetImage("delete.png", 16, 16);
            menu.Items.Add(item);

            menu.Items.Add(new Separator());

            var bulletMenu = new MenuItem();
            bulletMenu.Header = "Bullet";
            bulletMenu.Icon = Images.GetImage("bullets.png", 16, 16);
            menu.Items.Add(bulletMenu);

            item = new MenuItem();
            item.Header = "None";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "none";
            item.CommandTarget = this;
            bulletMenu.Items.Add(item);

            bulletMenu.Items.Add(new Separator());

            item = new MenuItem();
            item.Header = "\u2713 Check";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "check";
            item.CommandTarget = this;
            bulletMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "\u25CF Circle";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "disc";
            item.CommandTarget = this;
            bulletMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "\u25A0 Square";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "square";
            item.CommandTarget = this;
            bulletMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "\u25C6 Diamond";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "diamond";
            item.CommandTarget = this;
            bulletMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "\u25B6 Triangle";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "triangle";
            item.CommandTarget = this;
            bulletMenu.Items.Add(item);

            bulletMenu.Items.Add(new Separator());

            item = new MenuItem();
            item.Header = "\u25CB Circle";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "circle";
            item.CommandTarget = this;
            bulletMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "\u25A1 Square";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "open-square";
            item.CommandTarget = this;
            bulletMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "\u25C7 Diamond";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "open-diamond";
            item.CommandTarget = this;
            bulletMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "\u25B7 Triangle";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "open-triangle";
            item.CommandTarget = this;
            bulletMenu.Items.Add(item);

            bulletMenu.Items.Add(new Separator());

            item = new MenuItem();
            item.Header = "1, 2, 3";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "decimal";
            item.CommandTarget = this;
            bulletMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "a, b, c";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "lower-alpha";
            item.CommandTarget = this;
            bulletMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "i, ii, iii";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "lower-roman";
            item.CommandTarget = this;
            bulletMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "A, B, C";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "upper-alpha";
            item.CommandTarget = this;
            bulletMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "I, II, III";
            item.Command = NFormattingCommands.SetListStyle;
            item.CommandParameter = "upper-roman";
            item.CommandTarget = this;
            bulletMenu.Items.Add(item);

            var styleMenu = new MenuItem();
            styleMenu.Header = "Style";
            styleMenu.Icon = Images.GetImage("font.png", 16, 16);
            menu.Items.Add(styleMenu);

            item = new MenuItem();
            item.Header = "Normal";
            item.Click += BulletStyleMenu_Normal;
            styleMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "Bold";
            item.FontWeight = FontWeights.Bold;
            item.Click += BulletStyleMenu_Bold;
            styleMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "Italic";
            item.FontStyle = FontStyles.Italic;
            item.Click += BulletStyleMenu_Italic;
            styleMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = new TextBlock { Text = "Underline", TextDecorations = TextDecorations.Underline };
            item.Click += BulletStyleMenu_Underline;
            styleMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = new TextBlock { Text = "Strikethrough", TextDecorations = TextDecorations.Strikethrough };
            item.Click += BulletStyleMenu_Strikethrough;
            styleMenu.Items.Add(item);

            var colorMenu = new MenuItem();
            colorMenu.Header = "Color";
            colorMenu.Icon = Images.GetImage("textcolor.png", 16, 16);
            colorMenu.ItemContainerStyle = (Style)Application.Current.FindResource("ContainerMenuItemStyle");
            var colorPicker = new Controls.ColorPicker();
            colorPicker.MouseLeftButtonUp += BulletColorMenu_Clicked;
            colorMenu.Items.Add(colorPicker);
            menu.Items.Add(colorMenu);

            var highlightMenu = new MenuItem();
            highlightMenu.Header = "Highlight";
            highlightMenu.Icon = Images.GetImage("highlight.png", 16, 16);
            highlightMenu.ItemContainerStyle = (Style)Application.Current.FindResource("ContainerMenuItemStyle");
            var highlightPicker = new Controls.HighlighterPicker();
            highlightPicker.MouseLeftButtonUp += BulletHighlightMenu_Clicked;
            highlightMenu.Items.Add(highlightPicker);

            menu.Items.Add(highlightMenu);

            menu.Items.Add(new Separator());

            var shortcutsMenu = new MenuItem();
            shortcutsMenu.Header = "Keyboard Shortcuts";
            shortcutsMenu.Icon = Images.GetImage("keyboard.png", 16, 16);
            menu.Items.Add(shortcutsMenu);

            // Toggle list item
            item = new MenuItem();
            item.Header = "Toggle list item";
            item.Command = NFormattingCommands.ToggleList;
            item.CommandParameter = this;
            item.CommandTarget = this;
            item.SetBinding(
                MenuItem.InputGestureTextProperty,
                new Binding { Source = item, Path = new PropertyPath("Command.InputGestureText") }
            );
            shortcutsMenu.Items.Add(item);

            // Indent list item
            item = new MenuItem();
            item.Header = "Indent list item";
            item.Command = NFormattingCommands.IncreaseIndentation;
            item.CommandParameter = this;
            item.CommandTarget = this;
            item.SetBinding(
                MenuItem.InputGestureTextProperty,
                new Binding { Source = item, Path = new PropertyPath("Command.InputGestureText") }
            );
            shortcutsMenu.Items.Add(item);

            // Outdent list item
            item = new MenuItem();
            item.Header = "Outdent list item";
            item.Command = NFormattingCommands.DecreaseIndentation;
            item.CommandParameter = this;
            item.CommandTarget = this;
            item.SetBinding(
                MenuItem.InputGestureTextProperty,
                new Binding { Source = item, Path = new PropertyPath("Command.InputGestureText") }
            );
            shortcutsMenu.Items.Add(item);

            shortcutsMenu.Items.Add(new Separator());

            // New list item above
            item = new MenuItem();
            item.Header = "New list item above";
            item.Command = NTextCommands.InsertParagraphBefore;
            item.CommandParameter = this;
            item.CommandTarget = this;
            item.SetBinding(
                MenuItem.InputGestureTextProperty,
                new Binding { Source = item, Path = new PropertyPath("Command.InputGestureText") }
            );
            shortcutsMenu.Items.Add(item);

            // New list item below
            item = new MenuItem();
            item.Header = "New list item below";
            item.Command = NTextCommands.InsertParagraphAfter;
            item.CommandParameter = this;
            item.CommandTarget = this;
            item.SetBinding(
                MenuItem.InputGestureTextProperty,
                new Binding { Source = item, Path = new PropertyPath("Command.InputGestureText") }
            );
            shortcutsMenu.Items.Add(item);

            shortcutsMenu.Items.Add(new Separator());

            // Move list item up
            item = new MenuItem();
            item.Header = "Move list item up";
            item.Command = NTextCommands.MoveParagraphUp;
            item.CommandParameter = this;
            item.CommandTarget = this;
            item.SetBinding(
                MenuItem.InputGestureTextProperty,
                new Binding { Source = item, Path = new PropertyPath("Command.InputGestureText") }
            );
            shortcutsMenu.Items.Add(item);

            // Move list item down
            item = new MenuItem();
            item.Header = "Move list item down";
            item.Command = NTextCommands.MoveParagraphDown;
            item.CommandParameter = this;
            item.CommandTarget = this;
            item.SetBinding(
                MenuItem.InputGestureTextProperty,
                new Binding { Source = item, Path = new PropertyPath("Command.InputGestureText") }
            );
            shortcutsMenu.Items.Add(item);

            shortcutsMenu.Items.Add(new Separator());

            // Select list item
            item = new MenuItem();
            item.Header = "Select list item";
            item.Command = NTextCommands.SelectParagraph;
            item.CommandParameter = this;
            item.CommandTarget = this;
            item.SetBinding(
                MenuItem.InputGestureTextProperty,
                new Binding { Source = item, Path = new PropertyPath("Command.InputGestureText") }
            );
            shortcutsMenu.Items.Add(item);

            // Duplicate list item
            item = new MenuItem();
            item.Header = "Duplicate list item";
            item.Command = NTextCommands.DuplicateParagraph;
            item.CommandParameter = this;
            item.CommandTarget = this;
            item.SetBinding(
                MenuItem.InputGestureTextProperty,
                new Binding { Source = item, Path = new PropertyPath("Command.InputGestureText") }
            );
            shortcutsMenu.Items.Add(item);

            // Delete list item
            item = new MenuItem();
            item.Header = "Delete list item";
            item.Command = NTextCommands.DeleteParagraph;
            item.CommandParameter = this;
            item.CommandTarget = this;
            item.SetBinding(
                MenuItem.InputGestureTextProperty,
                new Binding { Source = item, Path = new PropertyPath("Command.InputGestureText") }
            );
            shortcutsMenu.Items.Add(item);

            return menu;
        }

        void BulletMenu_Closed(object sender, RoutedEventArgs e)
        {
            TextBlock.Opacity = 1.0;

            TextBlock.Effect = null;
        }

        void BulletStyleMenu_Normal(object sender, RoutedEventArgs e)
        {
            _Paragraph.SetTextProperty(0, _Paragraph.Length, TextProperties.FontWeightProperty, FontWeights.Normal);
            _Paragraph.SetTextProperty(0, _Paragraph.Length, TextProperties.FontStyleProperty, FontStyles.Normal);
            _Paragraph.SetTextProperty(0, _Paragraph.Length, TextProperties.TextDecorationsProperty, null);
        }

        void BulletStyleMenu_Bold(object sender, RoutedEventArgs e)
        {
            _Paragraph.SetTextProperty(0, _Paragraph.Length, TextProperties.FontWeightProperty, FontWeights.Bold);
        }

        void BulletStyleMenu_Italic(object sender, RoutedEventArgs e)
        {
            _Paragraph.SetTextProperty(0, _Paragraph.Length, TextProperties.FontStyleProperty, FontStyles.Italic);
        }

        void BulletStyleMenu_Underline(object sender, RoutedEventArgs e)
        {
            _Paragraph.SetTextProperty(0, _Paragraph.Length, TextProperties.TextDecorationsProperty, TextDecorations.Underline);
        }

        void BulletStyleMenu_Strikethrough(object sender, RoutedEventArgs e)
        {
            _Paragraph.SetTextProperty(0, _Paragraph.Length, TextProperties.TextDecorationsProperty, TextDecorations.Strikethrough);
        }

        void BulletColorMenu_Clicked(object sender, MouseEventArgs e)
        {
            var colorPicker = (Controls.ColorPicker)sender;

            if (colorPicker.SelectedBrush != null)
            {
                _Paragraph.SetTextProperty(0, _Paragraph.Length, TextProperties.ForegroundBrushProperty, colorPicker.SelectedBrush);
            }
        }

        void BulletHighlightMenu_Clicked(object sender, MouseEventArgs e)
        {
            var highlightPicker = (Controls.HighlighterPicker)sender;

            if (highlightPicker.SelectedItem != null)
            {
                _Paragraph.SetTextProperty(0, _Paragraph.Length, TextProperties.BackgroundBrushProperty, highlightPicker.SelectedItem);
            }
        }

        #endregion
    }
}
