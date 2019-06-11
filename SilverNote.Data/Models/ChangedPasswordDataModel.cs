using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Security;

namespace SilverNote.Data.Models
{
    [XmlType("changed_password")]
    public class ChangedPasswordDataModel : DataModelBase, ICloneable
    {
        [XmlElement("old_password")]
        public SecureString OldPassword { get; set; }

        [XmlElement("new_password")]
        public SecureString NewPassword { get; set; }

        public override object Clone()
        {
            return MemberwiseClone();
        }
    }
}
