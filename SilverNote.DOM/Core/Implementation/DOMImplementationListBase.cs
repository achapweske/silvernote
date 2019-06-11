using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    public class DOMImplementationListBase : DOMImplementationList
    {
        #region Fields

        DOMImplementation[] _Items;

        #endregion

        #region Constructors

        public DOMImplementationListBase(IEnumerable<DOMImplementation> items)
        {
            _Items = items.ToArray();
        }

        #endregion

        #region IDOMImplementationList

        public DOMImplementation this[int index]
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

        public int Length
        {
            get { return _Items.Length; }
        }

        #endregion
    }
}
