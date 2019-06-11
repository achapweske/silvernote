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
using System.Windows.Controls;
using System.Windows.Markup;
using System.IO;

namespace SilverNote.Views
{
    [ContentProperty("ContainerStyles")]
    public class SimpleContainerStyleSelector : StyleSelector
    {
        #region Constructors

        public SimpleContainerStyleSelector()
        {
            ContainerStyles = new List<SimpleContainerStyle>();
            DefaultStyle = null;
        }

        #endregion

        #region Properties

        public List<SimpleContainerStyle> ContainerStyles { get; set; }

        public Style DefaultStyle { get; set; }

        #endregion

        #region Methods

        public override Style SelectStyle(object item, DependencyObject container)
        {
            return ContainerStyles
                .Where(style => style.Style.TargetType == null || style.Style.TargetType == container.GetType())
                .Where(style => style.DataType == null || style.DataType.IsInstanceOfType(item))
                .Select(style => style.Style)
                .FirstOrDefault() 
                ?? DefaultStyle;
        }

        #endregion
    }

    [ContentProperty("Style")]
    public class SimpleContainerStyle
    {
        public Type DataType { get; set; }
        public Style Style { get; set; }
    }
}
