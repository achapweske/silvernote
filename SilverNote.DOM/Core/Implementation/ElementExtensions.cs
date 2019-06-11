using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    static class ElementExtensions
    {
        /// <summary>
        /// Get the specified attribute's value as a bool.
        /// </summary>
        /// <param name="element">Target element</param>
        /// <param name="name">Name of the attribute whose value is to be retrieved.</param>
        /// <returns></returns>
        public static bool GetAttributeAsBool(this Element element, string name, bool defaultValue)
        {
            if (defaultValue)
            {
                throw new ArgumentException("defaultValue");
            }

            string value = element.GetAttribute(name);

            return !String.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Set the specified attribute's value as a bool.
        /// </summary>
        /// <param name="element">Target element</param>
        /// <param name="name">Name of the attribute whose value is to be set.</param>
        /// <param name="value">The new attribute value</param>
        public static void SetAttributeAsBool(this Element element, string name, bool value)
        {
            if (value)
            {
                element.SetAttribute(name, name);
            }
            else
            {
                element.RemoveAttribute(name);
            }
        }

        /// <summary>
        /// Get the specified attribute's value as an int.
        /// </summary>
        /// <param name="element">Target element</param>
        /// <param name="name">Name of the attribute whose value is to be retrieved.</param>
        /// <param name="defaultValue">Value to return if the given attribute is not set or not parseable.</param>
        /// <returns></returns>
        public static int GetAttributeAsInt(this Element element, string name, int defaultValue)
        {
            string value = element.GetAttribute(name);

            int result;
            if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Set the specified attribute's value as an int.
        /// </summary>
        /// <param name="element">Target element</param>
        /// <param name="name">Name of the attribute whose value is to be set.</param>
        /// <param name="value">The new attribute value</param>
        public static void SetAttributeAsInt(this Element element, string name, int value)
        {
            string stringValue = value.ToString(CultureInfo.InvariantCulture.NumberFormat);

            element.SetAttribute(name, stringValue);
        }

        /// <summary>
        /// Get the specified attribute's value as a double.
        /// </summary>
        /// <param name="element">Target element</param>
        /// <param name="name">Name of the attribute whose value is to be retrieved.</param>
        /// <param name="defaultValue">Value to return if the given attribute is not set or not parseable.</param>
        /// <returns></returns>
        public static double GetAttributeAsDouble(this Element element, string name, double defaultValue)
        {
            string value = element.GetAttribute(name);

            double result;
            if (Double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Set the specified attribute's value as a double.
        /// </summary>
        /// <param name="element">Target element</param>
        /// <param name="name">Name of the attribute whose value is to be set.</param>
        /// <param name="value">The new attribute value</param>
        public static void SetAttributeAsDouble(this Element element, string name, double value)
        {
            string stringValue = value.ToString(CultureInfo.InvariantCulture.NumberFormat);

            element.SetAttribute(name, stringValue);
        }
    }
}
