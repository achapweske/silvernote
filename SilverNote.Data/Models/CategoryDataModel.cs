using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SilverNote.Data.Models
{
    [XmlType("category")]
    public class CategoryDataModel : DataModelBase
    {
        [XmlAttribute("id")]
        public Int64 ID { get; set; }

        [XmlElement("notebook_id")]
        [DefaultValue(0)]
        public Int64 NotebookID { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("name_modified_at")]
        public DateTime NameModifiedAt { get; set; }

        public bool ShouldSerializeNameModifiedAt()
        { return NameModifiedAt != default(DateTime); }

        [XmlElement("parent_id")]
        [DefaultValue(0)]
        public Int64 ParentID { get; set; }

        [XmlElement("parent_id_modified_at")]
        public DateTime ParentIDModifiedAt { get; set; }

        public bool ShouldSerializeParentIDModifiedAt()
        { return ParentIDModifiedAt != default(DateTime); }

        [XmlElement("children")]
        public CategoryDataModel[] Children { get; set; }

        [XmlIgnore]
        public override object LocalID
        {
            get { return this.ID; }
        }

        public override void Update(DataModelBase _update, bool purge)
        {
            CategoryDataModel update = (CategoryDataModel)_update;

            base.Update(update, purge);

            // Update ID

            if (update.ID != 0)
            {
                this.ID = update.ID;
            }

            // Update NotebookID

            if (update.NotebookID != 0)
            {
                this.NotebookID = update.NotebookID;
            }

            // Update Name

            if (update.Name != null &&
                update.NameModifiedAt >= this.NameModifiedAt)
            {
                this.Name = update.Name;
                this.NameModifiedAt = update.NameModifiedAt;
            }

            // Update ParentID

            if (update.ParentID != 0 && 
                update.ParentIDModifiedAt >= this.ParentIDModifiedAt)
            {
                this.ParentID = update.ParentID;
                this.ParentIDModifiedAt = update.ParentIDModifiedAt;
            }

            // Update Children

            this.Children = UpdateArray(this.Children, update.Children);
        }

        public override bool Equals(object obj)
        {
            var other = obj as CategoryDataModel;

            return
                base.Equals(other) &&
                this.ID.Equals(other.ID) &&
                this.NotebookID.Equals(other.NotebookID) &&
                Object.Equals(this.Name, other.Name) &&
                this.NameModifiedAt.Equals(other.NameModifiedAt) &&
                this.ParentID.Equals(other.ParentID) &&
                ArrayEquals(this.Children, other.Children);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override object Clone()
        {
            var clone = (CategoryDataModel)MemberwiseClone();
            clone.Children = CloneArray(this.Children);
            return clone;
        }

    }
}
