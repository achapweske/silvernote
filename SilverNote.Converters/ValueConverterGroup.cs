/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SilverNote.Converters
{
    [System.Windows.Markup.ContentProperty("Converters")]
    public class ValueConverterGroup : IValueConverter
    {
        #region Fields

        readonly ObservableCollection<IValueConverter> _Converters = new ObservableCollection<IValueConverter>();
        readonly Dictionary<IValueConverter, ValueConversionAttribute> _CachedAttributes = new Dictionary<IValueConverter, ValueConversionAttribute>();
        
        #endregion

        #region Constructors

        public ValueConverterGroup()
        {
            _Converters.CollectionChanged += Converters_CollectionChanged;
        }

        #endregion

        #region Properties

        public ObservableCollection<IValueConverter> Converters
        {
            get { return _Converters; }
        }

        #endregion

        #region IValueConverter Members
 
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] parameters = new string[Converters.Count];
            if (parameter is string)
            {
                ((string)parameter).Split('|').CopyTo(parameters, 0);
            }

            object output = value;
            
            for (int i = 0; i < Converters.Count; i++)
            {
                var converter = Converters[i];
                Type currentTargetType = GetTargetType(i, targetType, true);

                output = converter.Convert(output, currentTargetType, parameters[i], culture);
                if (output == Binding.DoNothing)
                {
                    break;
                }
            }

            return output;
        }

        object IValueConverter.ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            string[] parameters = new string[Converters.Count];
            if (parameter is string)
            {
                ((string)parameter).Split('|').CopyTo(parameters, 0);
            }

            object output = value;

            for (int i = Converters.Count - 1; i >= 0; i--)
            {
                var converter = Converters[i];
                Type currentTargetType = GetTargetType(i, targetType, false);

                output = converter.ConvertBack(output, currentTargetType, parameters[i], culture);
                if (output == Binding.DoNothing)
                {
                    break;
                }
            }
            
            return output;
        }

        #endregion
        
        #region Implementation
        
        protected virtual Type GetTargetType(int converterIndex, Type finalTargetType, bool convert)
        {
            IValueConverter nextConverter = null;
            if (convert)
            {
                if (converterIndex < Converters.Count - 1)
                {
                    nextConverter = Converters[converterIndex + 1];
                    if (nextConverter == null)
                    {
                        throw new InvalidOperationException("The Converters collection of the ValueConverterGroup contains a null reference at index: " + (converterIndex + 1));
                    }
                }
            }
            else
            {
                if (converterIndex > 0)
                {
                    nextConverter = Converters[converterIndex - 1];
                    if (nextConverter == null)
                    {
                        throw new InvalidOperationException("The Converters collection of the ValueConverterGroup contains a null reference at index: " + (converterIndex - 1));
                    }
                }
            }
            if (nextConverter != null)
            {
                ValueConversionAttribute conversionAttribute = _CachedAttributes[nextConverter];
                return convert ? conversionAttribute.SourceType : conversionAttribute.TargetType;
            }

            return finalTargetType;
        }

        void Converters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IList converters = null;

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                converters = e.NewItems;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (IValueConverter converter in e.OldItems)
                {
                    _CachedAttributes.Remove(converter);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _CachedAttributes.Clear();
                converters = _Converters;
            }

            if (converters != null)
            {
                foreach (IValueConverter converter in converters)
                {
                    object[] attributes = converter.GetType().GetCustomAttributes(typeof(ValueConversionAttribute), false);
                    if (attributes.Length != 1)
                    {
                        throw new InvalidOperationException("All value converters added to a ValueConverterGroup must be decorated with the ValueConversionAttribute attribute exactly once.");
                    }

                    _CachedAttributes.Add(converter, attributes[0] as ValueConversionAttribute);
                }
            }
        }

        #endregion
    }
}
