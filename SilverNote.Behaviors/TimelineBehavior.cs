/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Input;

namespace SilverNote.Behaviors
{
    public static class TimelineBehavior
    {
        #region CompletedCommand

        public static ICommand GetCompletedCommand(Timeline element)
        {
            return (ICommand)element.GetValue(CompletedCommandProperty);
        }

        public static void SetCompletedCommand(Timeline element, ICommand value)
        {
            element.SetValue(CompletedCommandProperty, value);
        }

        public static readonly DependencyProperty CompletedCommandProperty = DependencyProperty.RegisterAttached(
            "CompletedCommand",
            typeof(ICommand),
            typeof(TimelineBehavior),
            new UIPropertyMetadata(null, OnCompletedCommandChanged)
        );

        static void OnCompletedCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            Timeline commandTarget = dep as Timeline;

            if (e.OldValue != null)
            {
                commandTarget.Completed -= CompletedCommandTarget_Completed;
            }

            if (e.NewValue != null)
            {
                commandTarget.Completed += CompletedCommandTarget_Completed;
            }
        }

        static void CompletedCommandTarget_Completed(object sender, EventArgs e)
        {
            var timeline = ((AnimationClock)sender).Timeline;
            var command = GetCompletedCommand(timeline);
            var parameter = GetCompletedCommandParameter(timeline);

            command.Execute(parameter);
        }

        #endregion

        #region CompletedCommandParameter

        public static object GetCompletedCommandParameter(Timeline element)
        {
            return element.GetValue(CompletedCommandParameterProperty);
        }

        public static void SetCompletedCommandParameter(Timeline element, object value)
        {
            element.SetValue(CompletedCommandParameterProperty, value);
        }

        public static readonly DependencyProperty CompletedCommandParameterProperty = DependencyProperty.RegisterAttached(
            "CompletedCommandParameter",
            typeof(object),
            typeof(TimelineBehavior)
        );

        #endregion

    }
}
