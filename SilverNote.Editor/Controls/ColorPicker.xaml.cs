/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SilverNote.Controls
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        #region Constructors

        public ColorPicker()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        #region AllowsNone

        public static readonly DependencyProperty AllowsNoneProperty = DependencyProperty.Register(
            "AllowsNone",
            typeof(bool),
            typeof(ColorPicker)
        );

        public bool AllowsNone
        {
            get { return (bool)GetValue(AllowsNoneProperty); }
            set { SetValue(AllowsNoneProperty, value); }
        }

        #endregion

        #region AllowsGradient

        public static readonly DependencyProperty AllowsGradientProperty = DependencyProperty.Register(
            "AllowsGradient",
            typeof(bool),
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(false)
        );

        public bool AllowsGradient
        {
            get { return (bool)GetValue(AllowsGradientProperty); }
            set { SetValue(AllowsGradientProperty, value); }
        }

        #endregion

        #region SelectedColor

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            "SelectedColor",
            typeof(Color),
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedColorProperty_Changed)
        );

        static void SelectedColorProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((ColorPicker)sender).SelectedColorProperty_Changed(e);
        }

        void SelectedColorProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            var newValue = (Color)e.NewValue;
            SelectedBrush = ToBrush(newValue, IsGradientEnabled);
        }

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        #endregion

        #region IsGradientEnabled

        public static readonly DependencyProperty IsGradientEnabledProperty = DependencyProperty.Register(
            "IsGradientEnabled",
            typeof(bool),
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsGradientEnabledProperty_Changed)
        );

        static void IsGradientEnabledProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((ColorPicker)sender).IsGradientEnabledProperty_Changed(e);
        }

        void IsGradientEnabledProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            bool newValue = (bool)e.NewValue;
            SelectedBrush = ToBrush(SelectedColor, newValue);
        }

        public bool IsGradientEnabled
        {
            get { return (bool)GetValue(IsGradientEnabledProperty); }
            set { SetValue(IsGradientEnabledProperty, value); }
        }

        #endregion

        #region SelectedBrush

        public static readonly DependencyProperty SelectedBrushProperty = DependencyProperty.Register(
            "SelectedBrush",
            typeof(Brush),
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedBrushProperty_Changed)
        );

        static void SelectedBrushProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((ColorPicker)sender).SelectedBrushProperty_Changed(e);
        }

        void SelectedBrushProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            bool hasGradient;
            SelectedColor = ToColor((Brush)e.NewValue, out hasGradient);
            IsGradientEnabled = hasGradient;
        }

        public Brush SelectedBrush
        {
            get { return (Brush)GetValue(SelectedBrushProperty); }
            set { SetValue(SelectedBrushProperty, value); }
        }

        #endregion

        #region Command

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(RoutedUICommand),
            typeof(ColorPicker)
        );

        public RoutedUICommand Command
        {
            get { return (RoutedUICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        #endregion

        #region CommandTarget

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
            "CommandTarget",
            typeof(IInputElement),
            typeof(ColorPicker)
        );

        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        #endregion

        #endregion

        #region Implementation

        static Brush ToBrush(Color color, bool isGradientEnabled = false)
        {
            if (color == Colors.Transparent)
            {
                return Brushes.Transparent;
            }
            else if (isGradientEnabled)
            {
                Color startColor = color;
                Color endColor = Color.FromRgb(
                    (byte)((color.R + 2 * 255) / 3),
                    (byte)((color.G + 2 * 255) / 3),
                    (byte)((color.B + 2 * 255) / 3)
                );
                var brush = new LinearGradientBrush(endColor, startColor, 90);
                brush.Freeze();
                return brush;
            }
            else
            {
                var brush = new SolidColorBrush(color);
                brush.Freeze();
                return brush;
            }
        }

        static Color ToColor(Brush brush, out bool hasGradient)
        {
            if (brush == null || brush == Brushes.Transparent)
            {
                hasGradient = false;
                return Colors.Transparent;
            }
            else if (brush is SolidColorBrush)
            {
                hasGradient = false;
                return ((SolidColorBrush)brush).Color;
            }
            else if (brush is LinearGradientBrush)
            {
                hasGradient = true;
                return ((LinearGradientBrush)brush).GradientStops.Last().Color;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private void NoneButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedColor = Colors.Transparent;

            if (Command != null)
            {
                Command.Execute(Brushes.Transparent, CommandTarget);
            }

            CloseParent();
        }

        private void Color_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Command != null)
            {
                var element = (FrameworkElement)sender;
                Color color = (Color)element.DataContext;
                Brush brush = ToBrush(color, IsGradientEnabled);

                Command.Execute(brush, CommandTarget);
            }
        }

        private void GradientCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (Command != null)
            {
                Brush brush = ToBrush(SelectedColor, IsGradientEnabled);

                Command.Execute(brush, CommandTarget);
            }
        }

        void CloseParent()
        {
            DependencyObject parent = LogicalTreeHelper.GetParent(this);
            while (parent != null)
            {
                if (parent is ToggleButton)
                {
                    ((ToggleButton)parent).IsChecked = false;
                    break;
                }
                parent = LogicalTreeHelper.GetParent(parent);
            }
        }

        #endregion

    }

    public class ColorPickerConverter : IMultiValueConverter
    {
        private static ColorPickerConverter instance = null;
        public static ColorPickerConverter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ColorPickerConverter();
                }

                return instance;
            }
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = (bool)values[0];
            if (isNull)
            {
                return null;
            }

            Color color = (Color)values[1];

            bool isGradient = (bool)values[2];
            if (isGradient)
            {
                Color startColor = color;
                Color endColor = Color.FromRgb(
                    (byte)((color.R + 2 * 255) / 3),
                    (byte)((color.G + 2 * 255) / 3),
                    (byte)((color.B + 2 * 255) / 3)
                );
                LinearGradientBrush brush = new LinearGradientBrush(startColor, endColor, 90);
                brush.Freeze();
                return brush;
            }
            else
            {
                Brush brush = new SolidColorBrush(color);
                brush.Freeze();
                return brush;
            }
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
