/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace SilverNote.Controls
{
    /// <summary>
    /// Interaction logic for StrokeControl.xaml
    /// </summary>
    public partial class StrokeControl : UserControl
    {
        #region Constructors

        public StrokeControl()
        {
            WidthItems = new ObservableCollection<double>(new [] {
                1.0,
                2.0,
                3.0,
                4.0,
                5.0,
                6.0
            });

            DashPropertyItems = new ObservableCollection<DashProperties>(new [] {
                new DashProperties("Solid", DashStyles.Solid.Dashes),
                new DashProperties("Dash", DashStyles.Dash.Dashes),
                new DashProperties("Dot", DashStyles.Dot.Dashes, PenLineCap.Round)
            });

            InitializeComponent();

            DashStyleComboBox.SelectedItem = DashPropertyItems.First();
        }

        #endregion

        #region Properties

        public class DashProperties
        {
            public DashProperties(string name, DoubleCollection dashArray, PenLineCap dashCap = PenLineCap.Flat)
            {
                Name = name;
                DashArray = dashArray;
                DashCap = dashCap;
            }

            public string Name { get; set; }
            public DoubleCollection DashArray { get; set; }
            public PenLineCap DashCap { get; set; }
        }

        public ObservableCollection<double> WidthItems { get; private set; }

        public ObservableCollection<DashProperties> DashPropertyItems { get; private set; }

        #region SelectedBrush

        public static readonly DependencyProperty SelectedBrushProperty = DependencyProperty.Register(
            "SelectedBrush",
            typeof(Brush),
            typeof(StrokeControl),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public Brush SelectedBrush
        {
            get { return (Brush)GetValue(SelectedBrushProperty); }
            set { SetValue(SelectedBrushProperty, value); }
        }

        #endregion

        #region SelectedWidth

        public static readonly DependencyProperty SelectedWidthProperty = DependencyProperty.Register(
            "SelectedWidth",
            typeof(double),
            typeof(StrokeControl),
            new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public double SelectedWidth
        {
            get { return (double)GetValue(SelectedWidthProperty); }
            set { SetValue(SelectedWidthProperty, value); }
        }

        #endregion

        #region AllowsDash

        public static readonly DependencyProperty AllowsDashProperty = DependencyProperty.Register(
            "AllowsDash",
            typeof(bool),
            typeof(StrokeControl)
        );

        public bool AllowsDash
        {
            get { return (bool)GetValue(AllowsDashProperty); }
            set { SetValue(AllowsDashProperty, value); }
        }

        #endregion

        #region SelectedDashArray

        public static readonly DependencyProperty SelectedDashArrayProperty = DependencyProperty.Register(
            "SelectedDashArray",
            typeof(DoubleCollection),
            typeof(StrokeControl),
            new FrameworkPropertyMetadata(DashStyles.Solid.Dashes, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSelectedDashArrayChanged))
        );

        protected static void OnSelectedDashArrayChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            StrokeControl control = (StrokeControl)target;
            DoubleCollection dashArray = (DoubleCollection)e.NewValue;

            foreach (DashProperties dashItem in control.DashPropertyItems)
            {
                if (dashItem.DashArray == dashArray)
                {
                    control.DashStyleComboBox.SelectedItem = dashItem;
                    break;
                }
            }
        }

        public DoubleCollection SelectedDashArray
        {
            get { return (DoubleCollection)GetValue(SelectedDashArrayProperty); }
            set { SetValue(SelectedDashArrayProperty, value); }
        }

        #endregion

        #region SelectedDashCap

        public static readonly DependencyProperty SelectedDashCapProperty = DependencyProperty.Register(
            "SelectedDashCap",
            typeof(PenLineCap),
            typeof(StrokeControl),
            new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public PenLineCap SelectedDashCap
        {
            get { return (PenLineCap)GetValue(SelectedDashCapProperty); }
            set { SetValue(SelectedDashCapProperty, value); }
        }

        #endregion

        #region SelectBrushCommand

        public static readonly DependencyProperty SelectBrushCommandProperty = DependencyProperty.Register(
            "SelectBrushCommand",
            typeof(RoutedUICommand),
            typeof(StrokeControl)
        );

        public RoutedUICommand SelectBrushCommand
        {
            get { return (RoutedUICommand)GetValue(SelectBrushCommandProperty); }
            set { SetValue(SelectBrushCommandProperty, value); }
        }

        #endregion

        #region SelectWidthCommand

        public static readonly DependencyProperty SelectWidthCommandProperty = DependencyProperty.Register(
            "SelectWidthCommand",
            typeof(RoutedUICommand),
            typeof(StrokeControl)
        );

        public RoutedUICommand SelectWidthCommand
        {
            get { return (RoutedUICommand)GetValue(SelectWidthCommandProperty); }
            set { SetValue(SelectWidthCommandProperty, value); }
        }

        #endregion

        #region CommandTarget

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
            "CommandTarget",
            typeof(IInputElement),
            typeof(StrokeControl)
        );

        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        #endregion

        #endregion

        #region Implementation

        private void StrokeWidthComboBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectWidthCommand != null)
            {
                var comboBoxItem = (ComboBoxItem)sender;
                double width = (double)comboBoxItem.DataContext;
                SelectWidthCommand.Execute(width, CommandTarget);
            }
        }

        private void DashStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DashProperties selected = DashStyleComboBox.SelectedItem as DashProperties;
            if (selected != null)
            {
                SelectedDashArray = selected.DashArray;
                SelectedDashCap = selected.DashCap;
            }
        }

        #endregion
    }
}
