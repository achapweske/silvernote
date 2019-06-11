using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    public class DOMConfigurationBase : DOMConfiguration
    {
        #region Fields

        Dictionary<string, object> _Parameters = new Dictionary<string, object>();

        #endregion

        #region Constructors

        public DOMConfigurationBase()
        {

        }

        #endregion

        #region DOMConfiguration

        public virtual void SetParameter(string name, object value)
        {
            _Parameters[name] = value;
        }

        public virtual object GetParameter(string name)
        {
            object value;
            if (_Parameters.TryGetValue(name, out value))
            {
                return value;
            }
            else
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }
        }

        public virtual bool CanSetParameter(string name, object value)
        {
            return true;
        }

        public virtual DOMStringList ParameterNames
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
