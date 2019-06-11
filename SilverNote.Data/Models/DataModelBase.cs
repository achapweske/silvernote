using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SilverNote.Data.Models
{
    public class DataModelBase : ICloneable
    {
        /// <summary>
        /// </summary>
        [XmlElement("created_at")]
        public DateTime CreatedAt { get; set; }

        public bool ShouldSerializeCreatedAt()
        { return CreatedAt != default(DateTime); }

        /// <summary>
        /// </summary>
        [XmlElement("modified_at")]
        public DateTime ModifiedAt { get; set; }

        public bool ShouldSerializeModifiedAt()
        { return ModifiedAt != default(DateTime); }

        /// <summary>
        /// </summary>
        [XmlElement("viewed_at")]
        public DateTime ViewedAt { get; set; }

        public bool ShouldSerializeViewedAt()
        { return ViewedAt != default(DateTime); }

        /// <summary>
        /// True iff this object is marked for deletion
        /// </summary>
        [XmlElement("is_deleted")]
        public bool IsDeleted { get; set; }

        public bool ShouldSerializeIsDeleted()
        { return IsDeleted; }

        /// <summary>
        /// </summary>
        [XmlElement("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// </summary>
        [XmlElement("remote_hash")]
        public string RemoteHash { get; set; }

        /// <summary>
        /// </summary>
        [XmlElement("last_sent_hash")]
        public string LastSentHash { get; set; }

        /// <summary>
        /// </summary>
        [XmlElement("last_recv_hash")]
        public string LastRecvHash { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public virtual object LocalID
        {
            get { return null; }
        }

        public bool IDEquals(object id)
        {
            return LocalID.Equals(id);
        }

        public static Func<DataModelBase, bool> IDEqualsFunc(object id)
        {
            return (dataModel) => { return dataModel.IDEquals(id); };
        }

        public static Predicate<DataModelBase> IDEqualsPredicate(object id)
        {
            return (dataModel) => { return dataModel.IDEquals(id); };
        }

        /// <summary>
        /// Update properties specified by the given object
        /// 
        /// This method will not set properties to invalid values.
        /// 
        /// This method may choose to not update a property if the current
        /// value takes precedent over the given value. 
        /// </summary>
        /// <param name="update"></param>
        /// <returns>true iff the invoking DataModel was modified</returns>
        public virtual void Update(DataModelBase update, bool purge)
        {
            if (update.CreatedAt != default(DateTime))
            {
                this.CreatedAt = update.CreatedAt;
            }

            if (update.ModifiedAt != default(DateTime))
            {
                this.ModifiedAt = update.ModifiedAt;
            }

            if (update.ViewedAt != default(DateTime))
            {
                this.ViewedAt = update.ViewedAt;
            }

            if (update.IsDeleted)
            {
                this.IsDeleted = true;
            }

            if (update.Hash != null)
            {
                this.Hash = update.Hash;
            }

            if (update.RemoteHash != null)
            {
                this.RemoteHash = update.RemoteHash;
            }

            if (update.LastSentHash != null)
            {
                this.LastSentHash = update.LastSentHash;
            }

            if (update.LastRecvHash != null)
            {
                this.LastRecvHash = update.LastRecvHash;
            }
        }

        protected void Validate(DateTime now)
        {
            CreatedAt = (CreatedAt < now) ? CreatedAt : now;
            ModifiedAt = (ModifiedAt < now) ? ModifiedAt : now;
            ViewedAt = (ViewedAt < now) ? ViewedAt : now;

            Hash = (Hash != null) ? Hash : String.Empty;
        }

        public override bool Equals(object obj)
        {
            var other = obj as DataModelBase;

            return other != null &&
                other.CreatedAt.Equals(this.CreatedAt) &&
                other.ModifiedAt.Equals(this.ModifiedAt) &&
                other.ViewedAt.Equals(this.ViewedAt) &&
                other.IsDeleted.Equals(this.IsDeleted) &&
                Object.Equals(other.Hash, this.Hash) &&
                Object.Equals(other.RemoteHash, this.RemoteHash) &&
                Object.Equals(other.LastSentHash, this.LastSentHash) &&
                Object.Equals(other.LastRecvHash, this.LastRecvHash);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        protected static T Clone<T>(T obj) where T : class, ICloneable
        {
            if (obj != null)
            {
                return (T)obj.Clone();
            }
            else
            {
                return null;
            }
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            var type = this.GetType();

            buffer.Append(Normalize(type.Name) + ":\n");

            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                if (property.GetCustomAttributes(typeof(XmlIgnoreAttribute), true).Length > 0)
                {
                    continue;
                }

                string name = property.Name;
                object value = property.GetValue(this, null);
                if (value == null)
                {
                    continue;
                }
                Type valueType = value.GetType();
                if (valueType.IsValueType && value.Equals(Activator.CreateInstance(valueType)))
                {
                    continue;
                }

                buffer.Append('\t');
                buffer.Append(Normalize(name));
                buffer.Append(": ");
                buffer.Append(ToString(value));
                buffer.AppendLine();
            }

            return buffer.ToString().TrimEnd();
        }

        private static string ToString(Array value)
        {
            if (value.Length == 0)
            {
                return "{ }";
            }

            var buffer = new StringBuilder();

            foreach (object item in value)
            {
                string str = ToString(item);

                buffer.Append(str);
            }

            return buffer.ToString();
        }

        private static string ToString(object value)
        {
            if (value is Array)
            {
                return ToString((Array)value);
            }
            
            if (Type.GetTypeCode(value.GetType()) != TypeCode.Object)
            {
                return value.ToString();
            }

            var buffer = new StringBuilder();
            buffer.AppendLine();
            string str = value.ToString();
            str = Indent(str);
            str = Indent(str);
            buffer.Append(str);
            return buffer.ToString();
        }

        public static string Normalize(string propertyName)
        {
            var buffer = new StringBuilder();

            propertyName = propertyName.Replace("DataModel", "");

            for (int i = 0; i < propertyName.Length; i++)
            {
                buffer.Append(Char.ToLower(propertyName[i]));

                if ((i + 1) < propertyName.Length)
                {
                    if (Char.IsLower(propertyName[i]) &&
                        Char.IsUpper(propertyName[i + 1]))
                    {
                        buffer.Append('_');
                    }
                }
            }

            return buffer.ToString();
        }

        public static string Indent(string str)
        {
            return "\t" + str.Replace("\n", "\n\t");
        }

        #region Array Helpers

        protected static T[] UpdateArray<T>(T[] items, T[] updates, bool purge = false) where T : DataModelBase, new()
        {
            if (updates == null)
            {
                return items;
            }

            if (items == null)
            {
                return CloneArray(updates);
            }

            var updatedItems = new List<T>(items);

            foreach (var update in updates)
            {
                var item = (T)updatedItems.Find(IDEqualsPredicate(update.LocalID));
                if (item != null)
                {
                    item.Update(update, purge);
                }
                else
                {
                    item = (T)update.Clone();
                    updatedItems.Add(item);
                }

                if (purge && item.IsDeleted)
                {
                    updatedItems.Remove(item);
                }
            }

            return updatedItems.ToArray();
        }

        protected static bool ArrayEquals(DataModelBase[] a, DataModelBase[] b)
        {
            if (a == null && b == null)
            {
                return true;
            }

            if (a == null && b != null ||
                a != null && b == null)
            {
                return false;
            }

            if (a.Except(b, DataModelComparer.Instance).Count() > 0 ||
                b.Except(a, DataModelComparer.Instance).Count() > 0)
            {
                return false;
            }

            foreach (var aa in a)
            {
                var bb = b.Where(IDEqualsFunc(aa.LocalID)).First();

                if (!aa.Equals(bb))
                {
                    return false;
                }
            }

            return true;
        }

        protected static T[] CloneArray<T>(T[] array) where T : DataModelBase
        {
            if (array != null)
            {
                return (T[])array.Select(Clone<T>).ToArray();
            }
            else
            {
                return null;
            }
        }

        #endregion
    }

    public sealed class DataModelComparer : IEqualityComparer<DataModelBase>
    {
        private static readonly DataModelComparer _Instance = new DataModelComparer();

        public static DataModelComparer Instance
        {
            get { return _Instance; }
        }

        public bool Equals(DataModelBase a, DataModelBase b)
        {
            if (a.LocalID != null)
            {
                return a.LocalID.Equals(b.LocalID);
            }
            else
            {
                return b.LocalID == null;
            }
        }

        public int GetHashCode(DataModelBase dataModel)
        {
            if (dataModel.LocalID != null)
            {
                return dataModel.LocalID.GetHashCode();
            }
            else
            {
                return 0;
            }
        }
    }
}
