/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System.Windows;
using System.Windows.Media;

namespace SilverNote
{
    public static class AnimationSettings
    {
        public static readonly DependencyProperty IsAnimationEnabledProperty = DependencyProperty.RegisterAttached(
            "IsAnimationEnabled",
            typeof(bool),
            typeof(AnimationSettings),
            new PropertyMetadata((RenderCapability.Tier >> 16) == 2));

        public static bool GetIsAnimationEnabled(DependencyObject dep)
        {
            return (bool)IsAnimationEnabledProperty.DefaultMetadata.DefaultValue;
        }

        public static void SetIsAnimationEnabled(DependencyObject dep, bool newValue)
        {
            IsAnimationEnabledProperty.DefaultMetadata.DefaultValue = newValue;
        }

        public static bool IsAnimationEnabled
        {
            get { return GetIsAnimationEnabled(null); }
            set { SetIsAnimationEnabled(null, value); }
        }
    }
}
