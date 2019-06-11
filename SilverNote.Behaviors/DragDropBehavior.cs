/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SilverNote.Behaviors
{
    public class DragDropBehavior
    {
        #region DragContext

        public static DragContext DragContext
        {
            get;
            protected set;
        }

        #endregion

        #region AllowDrag

        public static readonly DependencyProperty AllowDragProperty = DependencyProperty.RegisterAttached(
            "AllowDrag", 
            typeof(bool), 
            typeof(DragDropBehavior), 
            new UIPropertyMetadata(false, DragDropBehavior.AllowDragProperty_PropertyChanged));
        
        public static bool GetAllowDrag(DependencyObject element)
        {
            return (bool)element.GetValue(DragDropBehavior.AllowDragProperty);
        }
        
        public static void SetAllowDrag(DependencyObject element, bool value)
        {
            element.SetValue(DragDropBehavior.AllowDragProperty, value);
        }
        
        private static void AllowDragProperty_PropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            var element = dep as UIElement;
            if (element == null)
            {
                throw new Exception("AllowDrag target must be a UIElement");
            }

            if ((bool)e.NewValue)
            {
                element.MouseEnter += AllowDragTarget_MouseEnter;
                element.MouseLeave += AllowDragTarget_MouseLeave;
                element.PreviewMouseDown += AllowDragTarget_PreviewMouseDown;
                element.PreviewMouseUp += AllowDragTarget_PreviewMouseUp;
                element.MouseUp += AllowDragTarget_MouseUp;
                element.MouseMove += AllowDragTarget_MouseMove;
                DragDrop.AddGiveFeedbackHandler(element, AllowDragTarget_GiveFeedback);
                DragDrop.AddQueryContinueDragHandler(element, AllowDragTarget_QueryContinueDrag);
            }
            else
            {
                element.MouseEnter -= AllowDragTarget_MouseEnter;
                element.MouseLeave -= AllowDragTarget_MouseLeave;
                element.PreviewMouseDown -= AllowDragTarget_PreviewMouseDown;
                element.PreviewMouseUp -= AllowDragTarget_PreviewMouseUp;
                element.MouseUp -= AllowDragTarget_MouseUp;
                element.MouseMove -= AllowDragTarget_MouseMove;
                DragDrop.RemoveGiveFeedbackHandler(element, AllowDragTarget_GiveFeedback);
                DragDrop.RemoveQueryContinueDragHandler(element, AllowDragTarget_QueryContinueDrag);
            }
        }
        
        private static void AllowDragTarget_MouseEnter(object sender, MouseEventArgs e)
        {
            UIElement element = (UIElement)sender;
            DragDropBehavior.SetDragParameter(element, null);
        }
        
        private static void AllowDragTarget_MouseLeave(object sender, MouseEventArgs e)
        {
            UIElement element = (UIElement)sender;
            DragDropBehavior.SetDragParameter(element, null);
        }
        
        private static void AllowDragTarget_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var target = (UIElement)sender;
            var parameter = DragDropBehavior.GetDragParameter(target);
            if (parameter == null)
            {
                parameter = new DragDropBehavior.DragParameter();
                DragDropBehavior.SetDragParameter(target, parameter);
            }
            parameter.StartPoint = e.GetPosition(target);

            AllowDragTarget_FilterPreviewMouseDown(sender, e);
        }
      
        private static void AllowDragTarget_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var target = (UIElement)sender;
            DragDropBehavior.SetDragParameter(target, null);
        }

        private static void AllowDragTarget_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AllowDragTarget_FilterMouseUp(sender, e);
        }
        
        private static void AllowDragTarget_MouseMove(object sender, MouseEventArgs e)
        {
            if (!e.LeftButton.HasFlag(MouseButtonState.Pressed))
            {
                return;
            }

            var target = (UIElement)sender;

            var drag = DragDropBehavior.GetDragParameter(target);
            if (drag == null)
            {
                return;
            }

            var delta = e.GetPosition(target) - drag.StartPoint;
            if (Math.Abs(delta.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(delta.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            var context = DragContext = new DragContext();
            try
            {
                context.DragSource = target;
                context.DragData = GetDragData(target) ?? target;
                var converted = GetConvertedDragData(target, context.DragData);
                context.AllowedEffects = GetDragEffects(target);

                var feedback = GetDragFeedbackProvider(target);
                if (feedback != null)
                {
                    feedback.DragStarted(target, e);
                }

                context.ActualEffects = DragDrop.DoDragDrop(context.DragSource, converted, context.AllowedEffects);

                if (feedback != null)
                {
                    feedback.DragEnded(target);
                }

                if (context.ActualEffects.HasFlag(DragDropEffects.Move))
                {
                    var command = GetDragMoveCommand(target);
                    if (command != null)
                    {
                        var parameter = GetDragMoveCommandParameter(target);
                        command.Execute(parameter);
                    }
                }
                if (context.ActualEffects.HasFlag(DragDropEffects.Copy))
                {
                    var command = GetDragCopyCommand(target);
                    if (command != null)
                    {
                        var parameter = GetDragCopyCommandParameter(target);
                        command.Execute(parameter);
                    }
                }
            }
            finally
            {
                DragContext = null;
            }
        }

        private static void AllowDragTarget_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            var target = (DependencyObject)sender;

            var feedback = GetDragFeedbackProvider(target);
            if (feedback != null)
            {
                feedback.GiveFeedback(target, e);
            }
        }

        private static void AllowDragTarget_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (DragContext != null && DragContext.Action != DragAction.Continue)
            {
                e.Action = DragContext.Action;
                e.Handled = true;
            }
        }

        private static void AllowDragTarget_FilterPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // By default, ListBox doesn't handle multi-selection in a way that's compatible with drag-and-drop.

            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null)
            {
                return;
            }

            var listBox = LayoutHelper.GetSelfOrAncestor<ListBox>(listBoxItem) as ListBox;
            if (listBox == null)
            {
                return;
            }

            if (listBox.SelectionMode != SelectionMode.Single &&
                listBox.SelectedItems.Count > 1 &&
                listBoxItem.IsSelected &&
                !Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
                !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
            }
        }


        private static void AllowDragTarget_FilterMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null)
            {
                return;
            }

            var listBox = LayoutHelper.GetSelfOrAncestor<ListBox>(listBoxItem) as ListBox;
            if (listBox == null)
            {
                return;
            }

            if (listBox.SelectionMode != SelectionMode.Single &&
                listBox.SelectedItems.Count > 1 &&
                !Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
                !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                listBox.UnselectAll();

                listBoxItem.IsSelected = true;
            }
        }

        #endregion

        #region DragParameter

        private class DragParameter
        {
            public Point StartPoint { get; set; }
        }

        private static readonly DependencyProperty DragParameterProperty = DependencyProperty.RegisterAttached(
            "DragParameter", 
            typeof(DragParameter), 
            typeof(DragDropBehavior), 
            new PropertyMetadata(null)
        );

        private static DragParameter GetDragParameter(DependencyObject element)
        {
            return (DragParameter)element.GetValue(DragParameterProperty);
        }
        
        private static void SetDragParameter(DependencyObject element, DragParameter value)
        {
            element.SetValue(DragParameterProperty, value);
        }

        #endregion

        #region PreviewDrag

        public static readonly DependencyProperty PreviewDragProperty = DependencyProperty.RegisterAttached(
            "PreviewDrag", 
            typeof(bool), 
            typeof(DragDropBehavior), 
            new UIPropertyMetadata(false, OnPreviewDragChanged));

        public static bool GetPreviewDrag(DependencyObject element)
        {
            return (bool)element.GetValue(PreviewDragProperty);
        }
        
        public static void SetPreviewDrag(DependencyObject element, bool value)
        {
            element.SetValue(PreviewDragProperty, value);
        }
        
        private static void OnPreviewDragChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                DragDrop.AddGiveFeedbackHandler(dep, PreviewDragTarget_GiveFeedback);
            }
            else
            {
                DragDrop.RemoveGiveFeedbackHandler(dep, PreviewDragTarget_GiveFeedback);
            }
        }
        
        private static void PreviewDragTarget_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            var target = (FrameworkElement)sender;
            
            Point position = Mouse.GetPosition((IInputElement)target.Parent);
            if (!(target.RenderTransform is TranslateTransform))
            {
                target.RenderTransform = new TranslateTransform();
            }

            var transform = (TranslateTransform)target.RenderTransform;
            transform.X = position.X;
            transform.Y = position.Y;
        }

        #endregion

        #region DragData

        public static readonly DependencyProperty DragDataProperty = DependencyProperty.RegisterAttached("DragData", typeof(object), typeof(DragDropBehavior), new UIPropertyMetadata(null));

        public static object GetDragData(DependencyObject element)
        {
            return element.GetValue(DragDropBehavior.DragDataProperty);
        }
        
        public static void SetDragData(DependencyObject element, object value)
        {
            element.SetValue(DragDropBehavior.DragDataProperty, value);
        }

        #endregion

        #region DragDataConverter

        public static readonly DependencyProperty DragDataConverterProperty = DependencyProperty.RegisterAttached("DragDataConverter", typeof(IValueConverter), typeof(DragDropBehavior), new UIPropertyMetadata(null));

        public static IValueConverter GetDragDataConverter(DependencyObject element)
        {
            return (IValueConverter)element.GetValue(DragDropBehavior.DragDataConverterProperty);
        }

        public static void SetDragDataConverter(DependencyObject element, IValueConverter value)
        {
            element.SetValue(DragDropBehavior.DragDataConverterProperty, value);
        }

        protected static object GetConvertedDragData(DependencyObject element, object data)
        {
            var converter = GetDragDataConverter(element);
            if (converter != null)
            {
                return converter.Convert(data, typeof(object), null, CultureInfo.CurrentCulture);
            }
            else
            {
                return data;
            }
        }

        #endregion

        #region DragEffects

        public static readonly DependencyProperty DragEffectsProperty = DependencyProperty.RegisterAttached("DragEffects", typeof(DragDropEffects), typeof(DragDropBehavior), new UIPropertyMetadata(DragDropEffects.All));

        public static DragDropEffects GetDragEffects(DependencyObject element)
        {
            return (DragDropEffects)element.GetValue(DragDropBehavior.DragEffectsProperty);
        }
        
        public static void SetDragEffects(DependencyObject element, DragDropEffects value)
        {
            element.SetValue(DragDropBehavior.DragEffectsProperty, value);
        }

        #endregion

        #region DragMoveCommand

        public static readonly DependencyProperty DragMoveCommandProperty = DependencyProperty.RegisterAttached("DragMoveCommand", typeof(ICommand), typeof(DragDropBehavior), new PropertyMetadata(null));

        public static ICommand GetDragMoveCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(DragDropBehavior.DragMoveCommandProperty);
        }
        
        public static void SetDragMoveCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(DragDropBehavior.DragMoveCommandProperty, value);
        }

        #endregion

        #region DragMoveCommandParameter

        public static readonly DependencyProperty DragMoveCommandParameterProperty = DependencyProperty.RegisterAttached("DragMoveCommandParameter", typeof(object), typeof(DragDropBehavior), new UIPropertyMetadata(null));

        public static object GetDragMoveCommandParameter(DependencyObject element)
        {
            return element.GetValue(DragDropBehavior.DragMoveCommandParameterProperty);
        }
        
        public static void SetDragMoveCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(DragDropBehavior.DragMoveCommandParameterProperty, value);
        }

        #endregion

        #region DragCopyCommand

        public static readonly DependencyProperty DragCopyCommandProperty = DependencyProperty.RegisterAttached("DragCopyCommand", typeof(ICommand), typeof(DragDropBehavior), new PropertyMetadata(null));

        public static ICommand GetDragCopyCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(DragDropBehavior.DragCopyCommandProperty);
        }

        public static void SetDragCopyCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(DragDropBehavior.DragCopyCommandProperty, value);
        }

        #endregion DragCopyCommand

        #region DragCopyCommandParameter

        public static readonly DependencyProperty DragCopyCommandParameterProperty = DependencyProperty.RegisterAttached(
            "DragCopyCommandParameter", 
            typeof(object), 
            typeof(DragDropBehavior), 
            new PropertyMetadata(null));

        public static object GetDragCopyCommandParameter(DependencyObject element)
        {
            return element.GetValue(DragDropBehavior.DragCopyCommandParameterProperty);
        }
        
        public static void SetDragCopyCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(DragDropBehavior.DragCopyCommandParameterProperty, value);
        }

        #endregion

        #region DragFeedbackProvider

        public static readonly DependencyProperty DragFeedbackProviderProperty = DependencyProperty.RegisterAttached(
            "DragFeedbackProvider",
            typeof(IDragFeedbackProvider),
            typeof(DragDropBehavior),
            new PropertyMetadata(null));

        public static IDragFeedbackProvider GetDragFeedbackProvider(DependencyObject element)
        {
            return (IDragFeedbackProvider)element.GetValue(DragFeedbackProviderProperty);
        }

        public static void SetDragFeedbackProvider(DependencyObject element, IDragFeedbackProvider value)
        {
            element.SetValue(DragFeedbackProviderProperty, value);
        }

        #endregion

        #region AllowDrop

        public static readonly DependencyProperty AllowDropProperty = DependencyProperty.RegisterAttached(
            "AllowDrop", 
            typeof(bool), 
            typeof(DragDropBehavior), 
            new UIPropertyMetadata(false, AllowDropProperty_PropertyChanged));

        public static bool GetAllowDrop(DependencyObject element)
        {
            return (bool)element.GetValue(AllowDropProperty);
        }
        
        public static void SetAllowDrop(DependencyObject element, bool value)
        {
            element.SetValue(AllowDropProperty, value);
        }
        
        private static void AllowDropProperty_PropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            bool newValue = (bool)e.NewValue;

            if (dep is UIElement)
            {
                ((UIElement)dep).AllowDrop = newValue;
            }
            else if (dep is ContentElement)
            {
                ((ContentElement)dep).AllowDrop = newValue;
            }
            else
            {
                throw new Exception("Drop target must be a UIElement or ContentElement");
            }

            if (newValue)
            {
                DragDrop.AddDragEnterHandler(dep, AllowDropTarget_DragEnter);
                DragDrop.AddDragOverHandler(dep, AllowDropTarget_DragOver);
                DragDrop.AddDragLeaveHandler(dep, AllowDropTarget_DragLeave);
                DragDrop.AddDropHandler(dep, AllowDropTarget_Drop);
            }
            else
            {
                DragDrop.RemoveDragEnterHandler(dep, AllowDropTarget_DragEnter);
                DragDrop.RemoveDragOverHandler(dep, AllowDropTarget_DragOver);
                DragDrop.RemoveDragLeaveHandler(dep, AllowDropTarget_DragLeave);
                DragDrop.RemoveDropHandler(dep, AllowDropTarget_Drop);
            }
        }
        
        private static void AllowDropTarget_DragEnter(object sender, DragEventArgs e)
        {
            var target = (DependencyObject)sender;

            e.Effects = DetermineEffects(target, e.Effects, e.KeyStates);
            e.Handled = true;

            var feedback = GetDropFeedbackProvider(target);
            if (feedback != null)
            {
                feedback.DragEnter(sender, e);
            }
        }
        
        private static void AllowDropTarget_DragLeave(object sender, DragEventArgs e)
        {
            var target = (DependencyObject)sender;

            e.Handled = true;

            var feedback = GetDropFeedbackProvider(target);
            if (feedback != null)
            {
                feedback.DragLeave(sender, e);
            }
        }
        
        private static void AllowDropTarget_DragOver(object sender, DragEventArgs e)
        {
            var target = (DependencyObject)sender;

            e.Effects = DetermineEffects((DependencyObject)sender, e.Effects, e.KeyStates);
            e.Handled = true;

            var feedback = GetDropFeedbackProvider(target);
            if (feedback != null)
            {
                feedback.DragOver(sender, e);
            }
        }
        
        private static void AllowDropTarget_Drop(object sender, DragEventArgs e)
        {
            if (!e.Handled)
            {
                var target = (DependencyObject)sender;

                var feedback = GetDropFeedbackProvider(target);
                if (feedback != null)
                {
                    feedback.Drop(sender, e);
                }

                string format = GetDropDataFormat(target);
                if (!String.IsNullOrEmpty(format) && !e.Data.GetDataPresent(format))
                {
                    return;
                }

                object data = e.Data;
                if (!String.IsNullOrEmpty(format))
                {
                    data = e.Data.GetData(format);
                }

                e.Effects = DetermineEffects(target, e.Effects, e.KeyStates);

                ICommand command = null;
                if (command == null && e.Effects.HasFlag(DragDropEffects.Move))
                {
                    command = GetDropMoveCommand(target);
                }
                if (command == null && e.Effects.HasFlag(DragDropEffects.Copy))
                {
                    command = GetDropCopyCommand(target);
                }
                if (command == null)
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
                }

                if (DragContext != null)
                {
                    DragContext.DropTarget = target;
                    DragContext.DropData = data;
                }

                try
                {
                    command.Execute(data);
                }
                catch (DragDropException)
                {
                    e.Effects = DragDropEffects.None;
                }

                e.Handled = true;
            }
        }

        private static DragDropEffects DetermineEffects(DependencyObject dep, DragDropEffects allowedEffects, DragDropKeyStates keyStates)
        {
            if (keyStates.HasFlag(DragDropKeyStates.ControlKey))
            {
                DragDropEffects dropControlEffects = DragDropBehavior.GetDropControlEffects(dep);
                if (dropControlEffects != DragDropEffects.None && allowedEffects.HasFlag(dropControlEffects))
                {
                    return dropControlEffects;
                }
            }
            if (keyStates.HasFlag(DragDropKeyStates.AltKey))
            {
                DragDropEffects dropAltEffects = DragDropBehavior.GetDropAltEffects(dep);
                if (dropAltEffects != DragDropEffects.None && allowedEffects.HasFlag(dropAltEffects))
                {
                    return dropAltEffects;
                }
            }
            if (keyStates.HasFlag(DragDropKeyStates.ShiftKey))
            {
                DragDropEffects dropShiftEffects = DragDropBehavior.GetDropShiftEffects(dep);
                if (dropShiftEffects != DragDropEffects.None && allowedEffects.HasFlag(dropShiftEffects))
                {
                    return dropShiftEffects;
                }
            }
            DragDropEffects dropEffects = DragDropBehavior.GetDropEffects(dep);
            if (allowedEffects.HasFlag(dropEffects))
            {
                return dropEffects;
            }
            return DragDropEffects.None;
        }

        #endregion

        #region DropMoveCommand

        public static readonly DependencyProperty DropMoveCommandProperty = DependencyProperty.RegisterAttached("DropMoveCommand", typeof(ICommand), typeof(DragDropBehavior), new PropertyMetadata(null));

        public static ICommand GetDropMoveCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(DragDropBehavior.DropMoveCommandProperty);
        }
        
        public static void SetDropMoveCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(DragDropBehavior.DropMoveCommandProperty, value);
        }

        #endregion

        #region DropCopyCommand

        public static readonly DependencyProperty DropCopyCommandProperty = DependencyProperty.RegisterAttached("DropCopyCommand", typeof(ICommand), typeof(DragDropBehavior), new PropertyMetadata(null));

        public static ICommand GetDropCopyCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(DragDropBehavior.DropCopyCommandProperty);
        }
        
        public static void SetDropCopyCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(DragDropBehavior.DropCopyCommandProperty, value);
        }

        #endregion

        #region DropDataFormat

        public static readonly DependencyProperty DropDataFormatProperty = DependencyProperty.RegisterAttached("DropDataFormat", typeof(String), typeof(DragDropBehavior), new PropertyMetadata(null));

        public static String GetDropDataFormat(DependencyObject element)
        {
            return (String)element.GetValue(DragDropBehavior.DropDataFormatProperty);
        }

        public static void SetDropDataFormat(DependencyObject element, String value)
        {
            element.SetValue(DragDropBehavior.DropDataFormatProperty, value);
        }

        #endregion

        #region DropEffects

        public static readonly DependencyProperty DropEffectsProperty = DependencyProperty.RegisterAttached("DropEffects", typeof(DragDropEffects), typeof(DragDropBehavior), new PropertyMetadata(DragDropEffects.Move));

        public static DragDropEffects GetDropEffects(DependencyObject element)
        {
            return (DragDropEffects)element.GetValue(DragDropBehavior.DropEffectsProperty);
        }
        
        public static void SetDropEffects(DependencyObject element, DragDropEffects value)
        {
            element.SetValue(DragDropBehavior.DropEffectsProperty, value);
        }

        #endregion

        #region DropControlEffects

        public static readonly DependencyProperty DropControlEffectsProperty = DependencyProperty.RegisterAttached("DropControlEffects", typeof(DragDropEffects), typeof(DragDropBehavior), new PropertyMetadata(DragDropEffects.None));

        public static DragDropEffects GetDropControlEffects(DependencyObject element)
        {
            return (DragDropEffects)element.GetValue(DragDropBehavior.DropControlEffectsProperty);
        }
        
        public static void SetDropControlEffects(DependencyObject element, DragDropEffects value)
        {
            element.SetValue(DragDropBehavior.DropControlEffectsProperty, value);
        }

        #endregion

        #region DropAltEffects

        public static readonly DependencyProperty DropAltEffectsProperty = DependencyProperty.RegisterAttached("DropAltEffects", typeof(DragDropEffects), typeof(DragDropBehavior), new PropertyMetadata(DragDropEffects.None));

        public static DragDropEffects GetDropAltEffects(DependencyObject element)
        {
            return (DragDropEffects)element.GetValue(DropAltEffectsProperty);
        }
        
        public static void SetDropAltEffects(DependencyObject element, DragDropEffects value)
        {
            element.SetValue(DropAltEffectsProperty, value);
        }

        #endregion

        #region DropShiftEffects

        public static readonly DependencyProperty DropShiftEffectsProperty = DependencyProperty.RegisterAttached("DropShiftEffects", typeof(DragDropEffects), typeof(DragDropBehavior), new PropertyMetadata(DragDropEffects.None));

        public static DragDropEffects GetDropShiftEffects(DependencyObject element)
        {
            return (DragDropEffects)element.GetValue(DragDropBehavior.DropShiftEffectsProperty);
        }
        
        public static void SetDropShiftEffects(DependencyObject element, DragDropEffects value)
        {
            element.SetValue(DragDropBehavior.DropShiftEffectsProperty, value);
        }

        #endregion

        #region DropFeedbackProvider

        public static readonly DependencyProperty DropFeedbackProviderProperty = DependencyProperty.RegisterAttached(
            "DropFeedbackProvider",
            typeof(IDropFeedbackProvider), 
            typeof(DragDropBehavior), 
            new PropertyMetadata(null));

        public static IDropFeedbackProvider GetDropFeedbackProvider(DependencyObject element)
        {
            return (IDropFeedbackProvider)element.GetValue(DropFeedbackProviderProperty);
        }
        
        public static void SetDropFeedbackProvider(DependencyObject element, IDropFeedbackProvider value)
        {
            element.SetValue(DropFeedbackProviderProperty, value);
        }

        #endregion

        #region AutoScroll

        public static readonly DependencyProperty AutoScrollProperty = DependencyProperty.RegisterAttached(
            "AutoScroll", 
            typeof(bool), 
            typeof(DragDropBehavior), 
            new UIPropertyMetadata(false, AutoScrollProperty_PropertyChanged));

        public static bool GetAutoScroll(DependencyObject element)
        {
            return (bool)element.GetValue(AutoScrollProperty);
        }
        
        public static void SetAutoScroll(DependencyObject element, bool value)
        {
            element.SetValue(AutoScrollProperty, value);
        }

        private static void AutoScrollProperty_PropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            var element = dep as FrameworkElement;
            if (element == null)
            {
                throw new ArgumentException("AutoScroll can only be applied to objects of type FrameworkElement");
            }

            element.PreviewDragOver -= AutoScrollTarget_PreviewDragOver;

            if (true.Equals(e.NewValue))
            {
                element.PreviewDragOver += AutoScrollTarget_PreviewDragOver;
            }
        }

        private static void AutoScrollTarget_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (!e.AllowedEffects.HasFlag(DragDropEffects.Scroll))
            {
                return;
            }

            var element = sender as FrameworkElement;

            var scrollViewer = element as ScrollViewer;
            if (scrollViewer == null)
            {
                return;
            }

            Point position = e.GetPosition(scrollViewer);

            if (position.Y < 30 && scrollViewer.VerticalOffset > 0)
            {
                double delta = 6 * (30 - position.Y);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - delta);
                e.Handled = true;
            }
            else if (position.Y > scrollViewer.ActualHeight - 50 && scrollViewer.VerticalOffset < scrollViewer.ScrollableHeight)
            {
                double delta = 6 * (position.Y - (scrollViewer.ActualHeight - 50));
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + delta);
                e.Handled = true;
            }
        }

        #endregion


    }

    public class DragContext
    {
        public DragContext()
        {
            Action = DragAction.Continue;
        }

        public DependencyObject DragSource { get; set; }
        public DependencyObject DropTarget { get; set; }
        public object DragData { get; set; }
        public object DropData { get; set; }
        public DragDropEffects AllowedEffects { get; set; }
        public DragDropEffects ActualEffects { get; set; }
        public DragAction Action { get; set; }
    }

    public interface IDragFeedbackProvider
    {
        void DragStarted(object sender, MouseEventArgs e);

        void GiveFeedback(object sender, GiveFeedbackEventArgs e);

        void DragEnded(object sender);
    }

    public interface IDropFeedbackProvider
    {
        void DragEnter(object sender, DragEventArgs e);

        void DragLeave(object sender, DragEventArgs e);

        void DragOver(object sender, DragEventArgs e);

        void Drop(object sender, DragEventArgs e);
    }

	public class DragDropException : Exception
	{
		public DragDropException()
		{
		}

		public DragDropException(string message) : base(message)
		{
		}
	}
}

