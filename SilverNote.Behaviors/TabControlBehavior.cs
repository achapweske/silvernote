/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SilverNote.Behaviors
{
    public static class TabControlBehavior
    {
        #region IsRearrangeEnabled

        public static readonly DependencyProperty IsRearrangeEnabledProperty = DependencyProperty.RegisterAttached(
            "IsRearrangeEnabled",
            typeof(bool),
            typeof(TabControlBehavior),
            new UIPropertyMetadata(false, IsRearrangeEnabled_Changed)
        );

        public static bool GetIsRearrangeEnabled(UIElement element)
        {
            return (bool)element.GetValue(IsRearrangeEnabledProperty);
        }

        public static void SetIsRearrangeEnabled(UIElement element, bool value)
        {
            element.SetValue(IsRearrangeEnabledProperty, value);
        }

        static void IsRearrangeEnabled_Changed(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            TabControl tabControl = dep as TabControl;

            if (tabControl == null)
            {
                Debug.WriteLine("Error: IsRearrangeEnabled can only be applied to TabControl objects");
                return;
            }

            if ((bool)e.NewValue)
            {
                tabControl.PreviewMouseLeftButtonDown += IsRearrangeEnabledTarget_PreviewMouseLeftButtonDown;
                tabControl.MouseMove += IsRearrangeEnabledTarget_MouseMove;
                tabControl.MouseLeftButtonUp += IsRearrangeEnabledTarget_MouseLeftButtonUp;
                tabControl.MouseLeave += IsRearrangeEnabledTarget_MouseLeave;
                tabControl.LostMouseCapture += IsRearrangeEnabledTarget_LostMouseCapture;
            }
            else
            {
                tabControl.PreviewMouseLeftButtonDown -= IsRearrangeEnabledTarget_PreviewMouseLeftButtonDown;
                tabControl.MouseMove -= IsRearrangeEnabledTarget_MouseMove;
                tabControl.MouseLeftButtonUp -= IsRearrangeEnabledTarget_MouseLeftButtonUp;
                tabControl.MouseLeave -= IsRearrangeEnabledTarget_MouseLeave;
                tabControl.LostMouseCapture -= IsRearrangeEnabledTarget_LostMouseCapture;
            }
        }

        static void IsRearrangeEnabledTarget_PreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            var tabControl = (TabControl)sender;

            var tabItem = LayoutHelper.GetSelfOrAncestor<TabItem>((DependencyObject)e.OriginalSource);
            if (tabItem == null)
            {
                return;
            }

            var state = new RearrangeParameter
            {
                StartPoint = e.GetPosition(tabControl),
                Source = tabItem,
                Target = tabItem
            };

            SetRearrangeParameter(tabControl, state);
        }

        static void IsRearrangeEnabledTarget_MouseMove(object sender, MouseEventArgs e)
        {
            if (!e.LeftButton.HasFlag(MouseButtonState.Pressed))
            {
                return;
            }

            var tabControl = (TabControl)sender;

            var state = GetRearrangeParameter(tabControl);
            if (state == null)
            {
                return;
            }

            // Sanity check to ensure we're still working with the original TabItem
            var tabItem = LayoutHelper.GetSelfOrAncestor<TabItem>((DependencyObject)e.OriginalSource);
            if (tabItem != state.Source)
            {
                ResetRearrangeEffects(tabControl);
                SetRearrangeParameter(tabControl, null);
                return;
            }

            Point currentPoint = e.GetPosition(tabControl);

            if (!state.IsMoving)
            {
                if (Math.Abs(currentPoint.X - state.StartPoint.X) < 9)
                {
                    return;
                }

                state.Source.CaptureMouse();

                state.IsMoving = true;
            }

            var generator = tabControl.ItemContainerGenerator;
            int sourceIndex = generator.IndexFromContainer(state.Source);
            int targetIndex = generator.IndexFromContainer(state.Target);
            double itemWidth = state.Source.RenderSize.Width;
            double tabItemCenter = itemWidth * (0.5 + sourceIndex - targetIndex);
            tabItemCenter += currentPoint.X - state.StartPoint.X;

            if (tabItemCenter < -5)
            {
                targetIndex = Math.Max(targetIndex - 1, 0);
            }
            else if (tabItemCenter > itemWidth + 5)
            {
                targetIndex = Math.Min(targetIndex + 1, tabControl.Items.Count - 1);
            }

            var newTarget = (TabItem)generator.ContainerFromIndex(targetIndex);
            if (newTarget != null)
            {
                state.Target = newTarget;
            }

            for (int i = 0; i < tabControl.Items.Count; i++)
            {
                TabItem container = (TabItem)generator.ContainerFromIndex(i);
                if (container == null)
                {
                    continue;
                }

                if (i < targetIndex && i < sourceIndex ||
                    i > targetIndex && i > sourceIndex)
                {
                    container.RenderTransform = null;
                    continue;
                }

                var transform = container.RenderTransform as TranslateTransform;
                if (transform == null)
                {
                    container.RenderTransform = transform = new TranslateTransform(0, 0);
                }

                if (i == sourceIndex)
                {
                    transform.X = currentPoint.X - state.StartPoint.X;
                }
                else if (i >= targetIndex && i < sourceIndex)
                {
                    transform.X = container.RenderSize.Width;
                }
                else if (i <= targetIndex && i > sourceIndex)
                {
                    transform.X = -container.RenderSize.Width;
                }
            }
        }

        static void IsRearrangeEnabledTarget_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            var tabControl = (TabControl)sender;

            var state = GetRearrangeParameter(tabControl);
            if (state == null)
            {
                return;
            }

            // Release mouse capture

            state.Source.ReleaseMouseCapture();

            // Clear all transforms

            ResetRearrangeEffects(tabControl);

            // Invoke RearrangedEvent

            if (state != null && state.Target != null && state.Target != state.Source)
            {
                var args = new RearrangeEventArgs(RearrangedEvent, tabControl);
                args.SourceItem = state.Source;
                args.TargetItem = state.Target;
                tabControl.RaiseEvent(args);
            }

            // Reset state

            SetRearrangeParameter(tabControl, null);
        }

        static void IsRearrangeEnabledTarget_MouseLeave(object sender, MouseEventArgs e)
        {
            // Called if the tab is closed while the mouse is over it WITHOUT left button down (?)

            var tabControl = (TabControl)sender;
            SetRearrangeParameter(tabControl, null);
        }

        static void IsRearrangeEnabledTarget_LostMouseCapture(object sender, MouseEventArgs e)
        {
            // Called if the tab is closed while the mouse is over it WITH left button down

            var tabControl = (TabControl)sender;
            SetRearrangeParameter(tabControl, null);
        }

        static void ResetRearrangeEffects(TabControl tabControl)
        {
            if (tabControl.ItemsSource != null)
            {
                for (int i = 0; i < tabControl.Items.Count; i++)
                {
                    var container = (TabItem)tabControl.ItemContainerGenerator.ContainerFromIndex(i);
                    if (container != null)
                    {
                        container.RenderTransform = null;
                    }
                }
            }
        }

        #endregion

        #region Rearranged

        public delegate void RearrangeEventHandler(object sender, RearrangeEventArgs e);

        public static readonly RoutedEvent RearrangedEvent = EventManager.RegisterRoutedEvent ( 
            "Rearranged", 
            RoutingStrategy.Bubble, 
            typeof(RearrangeEventHandler), 
            typeof(TabControlBehavior)
        );

        public static void AddRearrangedHandler(DependencyObject dep, RearrangeEventHandler handler)
        {
            UIElement element = dep as UIElement;
            if (element != null)
            {
                element.AddHandler(RearrangedEvent, handler);
            }
        }

        public static void RemoveRearrangedHandler(DependencyObject dep, RearrangeEventHandler handler)
        {
            UIElement element = dep as UIElement;
            if (element != null)
            {
                element.RemoveHandler(RearrangedEvent, handler);
            }
        }

        #endregion

        #region RearrangeParameter

        class RearrangeParameter
        {
            public TabItem Source { get; set; }
            public TabItem Target { get; set; }
            public Point StartPoint { get; set; }
            public bool IsMoving { get; set; }
        }

        static readonly DependencyProperty RearrangeParameterProperty = DependencyProperty.RegisterAttached(
            "RearrangeParameter",
            typeof(RearrangeParameter),
            typeof(TabControlBehavior),
            new UIPropertyMetadata(null)
        );

        static RearrangeParameter GetRearrangeParameter(UIElement element)
        {
            return (RearrangeParameter)element.GetValue(RearrangeParameterProperty);
        }

        static void SetRearrangeParameter(UIElement element, RearrangeParameter value)
        {
            element.SetValue(RearrangeParameterProperty, value);
        }

        #endregion

    }

    public class RearrangeEventArgs : RoutedEventArgs
    {
        public RearrangeEventArgs()
            : base()
        {

        }

        public RearrangeEventArgs(RoutedEvent routedEvent)
            : base(routedEvent)
        {

        }

        public RearrangeEventArgs(RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {

        }

        public TabItem SourceItem { get; set; }
        public TabItem TargetItem { get; set; }
    }
}
