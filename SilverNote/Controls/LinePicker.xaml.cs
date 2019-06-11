/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using SilverNote.Editor;
using SilverNote.ViewModels;

namespace SilverNote.Controls
{
    /// <summary>
    /// Interaction logic for LinePicker.xaml
    /// </summary>
    public partial class LinePicker : UserControl
    {
        public LinePicker()
        {
            InitializeComponent();
        }

        #region Lines

        public static readonly DependencyProperty LinesProperty = DependencyProperty.Register(
            "Lines",
            typeof(ClipartViewModel[]),
            typeof(LinePicker),
            new FrameworkPropertyMetadata(null, Lines_Changed)
        );

        public ClipartViewModel[] Lines
        {
            get { return (ClipartViewModel[])GetValue(LinesProperty); }
            set { SetValue(LinesProperty, value); }
        }

        static void Lines_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((LinePicker)sender).Lines_Changed(e);
        }

        void Lines_Changed(DependencyPropertyChangedEventArgs e)
        {
            var oldValue = e.OldValue as ClipartViewModel[];
            if (oldValue != null)
            {
                foreach (var clipart in oldValue)
                {
                    clipart.PropertyChanged -= Line_Changed;
                }
            }

            Connectors = new ClipartViewModel[0];
            NonConnectors = new ClipartViewModel[0];

            var newValue = e.NewValue as ClipartViewModel[];
            if (newValue != null)
            {
                foreach (var clipart in newValue)
                {
                    Line_Changed(clipart, new PropertyChangedEventArgs("Drawing"));
                    clipart.PropertyChanged += Line_Changed;
                }
            }
            else
            {
                NonConnectors = null;
                Connectors = null;
            }
        }

        void Line_Changed(object sender, PropertyChangedEventArgs e)
        {
            var clipart = (ClipartViewModel)sender;

            if (e.PropertyName == "Drawing")
            {
                var drawing = clipart.Drawing;
                if (drawing != null)
                {
                    if ((drawing is LineBase) && ((LineBase)drawing).IsConnector)
                    {
                        var result = Connectors.ToList();
                        if (!result.Contains(clipart))
                        {
                            result.Add(clipart);
                        }
                        Connectors = result.ToArray();
                    }
                    else
                    {
                        var result = NonConnectors.ToList();
                        if (!result.Contains(clipart))
                        {
                            result.Add(clipart);
                        }
                        NonConnectors = result.ToArray();
                    }
                }
            }
        }

        #endregion

        #region NonConnectors

        public static readonly DependencyProperty NonConnectorsProperty = DependencyProperty.Register(
            "NonConnectors",
            typeof(ClipartViewModel[]),
            typeof(LinePicker),
            new FrameworkPropertyMetadata(null)
        );

        public ClipartViewModel[] NonConnectors
        {
            get { return (ClipartViewModel[])GetValue(NonConnectorsProperty); }
            set { SetValue(NonConnectorsProperty, value); }
        }

        #endregion

        #region Connectors

        public static readonly DependencyProperty ConnectorsProperty = DependencyProperty.Register(
            "Connectors",
            typeof(ClipartViewModel[]),
            typeof(LinePicker),
            new FrameworkPropertyMetadata(null)
        );

        public ClipartViewModel[] Connectors
        {
            get { return (ClipartViewModel[])GetValue(ConnectorsProperty); }
            set { SetValue(ConnectorsProperty, value); }
        }

        #endregion

        #region Markers

        public static readonly DependencyProperty MarkersProperty = DependencyProperty.Register(
            "Markers",
            typeof(ClipartViewModel[]),
            typeof(LinePicker),
            new FrameworkPropertyMetadata(null)
        );

        public ClipartViewModel[] Markers
        {
            get { return (ClipartViewModel[])GetValue(MarkersProperty); }
            set { SetValue(MarkersProperty, value); }
        }

        #endregion

        #region LineStyles

        private ClipartViewModel[] _LineStyles;

        public ClipartViewModel[] LineStyles
        {
            get 
            {
                if (_LineStyles == null)
                {
                    _LineStyles = CreateLineStyles();
                }
                return _LineStyles;
            }
        }

        private static ClipartViewModel[] CreateLineStyles()
        {
            var result = new List<ClipartViewModel>();
            Shape drawing;

            drawing = new Line(0, 8, 24, 8);
            result.Add(ClipartViewModel.FromDrawing("Line", drawing));
            drawing = new QuadraticCurve(0, 5, 12, 20, 24, 5);
            result.Add(ClipartViewModel.FromDrawing("Curve", drawing));
            drawing = new RoutedLine(0, 4, 12, 4, 12, 12, 24, 12);
            result.Add(ClipartViewModel.FromDrawing("Connector", drawing));

            return result.ToArray();
        }

        #endregion

        #region StandardMarkers

        private ClipartViewModel[] _StandardMarkers;

        public ClipartViewModel[] StandardMarkers
        {
            get 
            {
                if (_StandardMarkers == null)
                {
                    _StandardMarkers = CreateStandardMarkers();
                }
                return _StandardMarkers;
            }
        }

        private static ClipartViewModel[] CreateStandardMarkers()
        {
            return new[] { ClipartViewModel.FromDrawing("Default", null) };
        }

        #endregion

        #region SelectedLine

        public static readonly DependencyProperty SelectedLineProperty = DependencyProperty.Register(
            "SelectedLine",
            typeof(Shape),
            typeof(LinePicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public Shape SelectedLine
        {
            get { return (Shape)GetValue(SelectedLineProperty); }
            set { SetValue(SelectedLineProperty, value); }
        }

        #endregion

        #region IsLinesExpanded

        public static readonly DependencyProperty IsLinesExpandedProperty = DependencyProperty.Register(
            "IsLinesExpanded",
            typeof(bool),
            typeof(LinePicker)
        );

        public bool IsLinesExpanded
        {
            get { return (bool)GetValue(IsLinesExpandedProperty); }
            set { SetValue(IsLinesExpandedProperty, value); }
        }

        #endregion

        #region IsConnectorsExpanded

        public static readonly DependencyProperty IsConnectorsExpandedProperty = DependencyProperty.Register(
            "IsConnectorsExpanded",
            typeof(bool),
            typeof(LinePicker)
        );

        public bool IsConnectorsExpanded
        {
            get { return (bool)GetValue(IsConnectorsExpandedProperty); }
            set { SetValue(IsConnectorsExpandedProperty, value); }
        }

        #endregion

        #region CreateLineCommand

        public static readonly DependencyProperty CreateLineCommandProperty = DependencyProperty.Register(
            "CreateLineCommand",
            typeof(ICommand),
            typeof(LinePicker)
        );

        public ICommand CreateLineCommand
        {
            get { return (ICommand)GetValue(CreateLineCommandProperty); }
            set { SetValue(CreateLineCommandProperty, value); }
        }

        #endregion

        #region DeleteLineCommand

        public static readonly DependencyProperty DeleteLineCommandProperty = DependencyProperty.Register(
            "DeleteLineCommand",
            typeof(ICommand),
            typeof(LinePicker)
        );

        public ICommand DeleteLineCommand
        {
            get { return (ICommand)GetValue(DeleteLineCommandProperty); }
            set { SetValue(DeleteLineCommandProperty, value); }
        }

        #endregion

        #region SelectLineCommand

        public static readonly DependencyProperty SelectLineCommandProperty = DependencyProperty.Register(
            "SelectLineCommand",
            typeof(RoutedUICommand),
            typeof(LinePicker)
        );

        public RoutedUICommand SelectLineCommand
        {
            get { return (RoutedUICommand)GetValue(SelectLineCommandProperty); }
            set { SetValue(SelectLineCommandProperty, value); }
        }

        #endregion

        #region CommandTarget

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
            "CommandTarget",
            typeof(IInputElement),
            typeof(LinePicker)
        );

        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        #endregion

        private void Line_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item != null)
            {
                var clipart = item.Content as ClipartViewModel;
                if (clipart != null)
                {
                    SelectedLine = clipart.Drawing;

                    if (SelectLineCommand != null)
                    {
                        SelectLineCommand.Execute(clipart.Drawing, CommandTarget);
                    }
                }
            }
        }

        private void CreateLineButton_Click(object sender, RoutedEventArgs e)
        {
            // Get line
            var lineViewModel = LineStylesComboBox.SelectedItem as ClipartViewModel;
            var line = (LineBase)lineViewModel.Drawing.Clone();

            // Get start marker
            var startMarkerViewModel = StartMarkerComboBox.SelectedItem as ClipartViewModel;
            var startMarker = StartMarkerConverter.MarkerFromViewModel(startMarkerViewModel);

            // Get end marker
            var endMarkerViewModel = EndMarkerComboBox.SelectedItem as ClipartViewModel;
            var endMarker = EndMarkerConverter.MarkerFromViewModel(endMarkerViewModel);

            // Apply markers to line
            line.MarkerStart = startMarker;
            line.MarkerEnd = endMarker;

            line.IsConnector = ConnectorCheckBox.IsChecked == true;

            // Create line
            var canvas = new NCanvas { Drawing = line };
            CreateLineCommand.Execute(canvas);

            // Select line
            SelectedLine = line;
        }
    }

    [ValueConversion(typeof(Shape), typeof(Shape))]
    public class LineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((LineBase)value).LineThumb;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(Shape), typeof(Shape))]
    public class StartMarkerConverter : IValueConverter
    {
        public static Marker MarkerFromViewModel(ClipartViewModel viewModel)
        {
            if (viewModel != null)
            {
                return MarkerFromDrawing(viewModel.Drawing);
            }
            else
            {
                return null;
            }
        }

        public static Marker MarkerFromDrawing(Shape drawing)
        {
            var marker = EndMarkerConverter.MarkerFromDrawing(drawing);
            if (marker != null)
            {
                marker.ScaleAt(-1, 1, marker.MarkerWidth / 2, marker.MarkerHeight / 2);
                marker.Normalize();
            }

            return marker;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var drawing = value as Shape;

            var marker = MarkerFromDrawing(drawing);

            double length = 24;

            if (marker != null)
            {
                length -= marker.RefX;
            }

            Line line = new Line() 
            { 
                X1 = 24 - length, 
                Y1 = 0, 
                X2 = 24, 
                Y2 = 0, 
                MarkerStart = marker ,
                StrokeBrush = Brushes.Black,
                StrokeWidth = 2
            };

            return line;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(Shape), typeof(Shape))]
    public class EndMarkerConverter : IValueConverter
    {
        public static Marker MarkerFromViewModel(ClipartViewModel viewModel)
        {
            if (viewModel != null)
            {
                return MarkerFromDrawing(viewModel.Drawing);
            }
            else
            {
                return null;
            }
        }

        public static Marker MarkerFromDrawing(Shape drawing)
        {
            if (drawing == null)
            {
                return null;
            }

            drawing = (Shape)drawing.Clone();

            if (drawing is Marker)
            {
                return (Marker)drawing;
            }
            else if (drawing is ShapeGroup)
            {
                return new Marker((ShapeGroup)drawing);
            }
            else
            {
                return new Marker(drawing);
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var drawing = value as Shape;

            var marker = MarkerFromDrawing(drawing);

            double length = 24;

            if (marker != null)
            {
                length -= marker.MarkerWidth - marker.RefX;
            }

            Line line = new Line() 
            { 
                X1 = 0, 
                Y1 = 0, 
                X2 = length, 
                Y2 = 0, 
                MarkerEnd = marker,
                StrokeBrush = Brushes.Black,
                StrokeWidth = 2
            };

            return line;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
