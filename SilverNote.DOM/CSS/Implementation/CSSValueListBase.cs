/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DOM.CSS.Internal
{
    public class CSSValueListBase : CSSValueBase, CSSValueList, IEnumerable<CSSValue>
    {
        #region Fields

        CSSValue[] _Items;
        char _Separator;

        #endregion

        #region Constructors

        public CSSValueListBase(IEnumerable<CSSValue> values, char separator = ' ')
            : base(CSSValueType.CSS_VALUE_LIST)
        {
            _Items = values.ToArray();
            _Separator = separator;
        }

        protected CSSValueListBase(CSSValueType cssValueType)
            : base(cssValueType)
        {
            _Items = new CSSValueBase[0];
            _Separator = ' ';
        }

        #endregion

        #region ICSSValueList

        public int Length 
        {
            get { return _Items.Length; }
        }

        public CSSValue this[int index]
        {
            get
            {
                if (index >= 0 && index < _Items.Length)
                {
                    return _Items[index];
                }
                else
                {
                    return null;
                }
            }
        }

        private string _CssText;

        public override string CssText
        {
            get
            {
                if (_CssText == null)
                {
                    _CssText = CSSFormatter.FormatValueList(this);
                }

                return _CssText;
            }
            set
            {
                throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
            }
        }

        #endregion

        #region Extensions

        /// <summary>
        /// A CSSValueList representing the value "inherit"
        /// </summary>
        public static readonly CSSValueListBase Inherit = new CSSValueListBase(CSSValueType.CSS_INHERIT);

        /// <summary>
        /// An empty value list
        /// </summary>
        public static readonly CSSValueListBase Empty = new CSSValueListBase(CSSValueType.CSS_VALUE_LIST);

        /// <summary>
        /// Get the list item separator character
        /// </summary>
        public char Separator
        {
            get { return _Separator; }
        }

        /// <summary>
        /// Determine if two CSSValueLists are equivalent
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool AreEquivalent(CSSValueListBase a, CSSValueListBase b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null)
            {
                return false;
            }

            int length = a.Length;

            if (b.Length != length)
            {
                return false;
            }

            for (int i = 0; i < length; i++)
            {
                if (!CSSValueBase.AreEquivalent(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Parsing

        private static Dictionary<string, CSSValueListBase> Cache = new Dictionary<string, CSSValueListBase>();

        public static CSSValueListBase Parse(string str)
        {
            return Parse(str, ' ');
        }

        public static CSSValueListBase Parse(string str, char separator)
        {
            CSSValueListBase result;

            if (TryParse(str, separator, out result))
            {
                return result;
            }
            else
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }
        }

        public static bool TryParse(string str, char separator, out CSSValueListBase result)
        {
            str = str.Trim();

            if (str.Length == 0)
            {
                result = null;
                return true;
            }

            if (Cache.TryGetValue(str, out result))
            {
                return true;
            }

            if (str == "inherit")
            {
                result = Inherit;
            }
            else if (str == "none")
            {
                result = Empty;
            }
            else
            {
                result = CSSParser.ParseValueList(str, separator);
            }

            if (Cache.Count > 1000)
            {
                Cache.Clear();
            }

            Cache[str] = result;

            return true;
        }

        #endregion

        #region IEnumerable

        public IEnumerator<CSSValue> GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return CssText;
        }

        #endregion

    }
}
