using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    public class NameListBase : NameList
    {
        #region Fields

        class NameListItem
        {
            public string Name;
            public string NamespaceURI;
        }

        List<NameListItem> _Items;

        #endregion

        #region Constructors

        public NameListBase(IEnumerable<string> names)
        {
            _Items = new List<NameListItem>();

            foreach (string name in names)
            {
                var item = new NameListItem
                {
                    Name = name
                };

                _Items.Add(item);
            }
        }

        public NameListBase(IEnumerable<KeyValuePair<string, string>> names)
        {
            _Items = new List<NameListItem>();

            foreach (var name in names)
            {
                var item = new NameListItem
                {
                    Name = name.Key,
                    NamespaceURI = name.Value
                };

                _Items.Add(item);
            }
        }

        #endregion

        #region INameList

        public string GetName(int index)
        {
            if (index >= 0 && index < _Items.Count)
            {
                return _Items[index].Name;
            }
            else
            {
                return null;
            }
        }

        public string GetNamespaceURI(int index)
        {
            if (index >= 0 && index < _Items.Count)
            {
                return _Items[index].NamespaceURI;
            }
            else
            {
                return null;
            }
        }

        public int Length 
        {
            get { return _Items.Count; }
        }

        public bool Contains(string str)
        {
            return _Items.Any((item) => item.Name == str);
        }

        public bool ContainsNS(string namespaceURI, string name)
        {
            return _Items.Any((item) => item.Name == name && item.NamespaceURI == namespaceURI);
        }

        #endregion
    }
}
