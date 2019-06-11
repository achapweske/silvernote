using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SilverNote.Data.Models
{
    [XmlType("clipart_group")]
    public class ClipartGroupDataModel : DataModelBase
    {
        [XmlAttribute("id")]
        public Int64 ID { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("name_modified_at")]
        public DateTime NameModifiedAt { get; set; }

        public bool ShouldSerializeNameModifiedAt()
        { return NameModifiedAt != default(DateTime); }

        [XmlArray("items")]
        public ClipartDataModel[] Items { get; set; }

        [XmlIgnore]
        public override object LocalID
        {
            get { return this.ID; }
        }

        [XmlIgnore]
        public ClipartGroupDataModel Metadata
        {
            get
            {
                var metadata = new ClipartGroupDataModel();

                metadata.ID = this.ID;
                metadata.Hash = this.Hash;

                if (this.Items != null)
                {
                    metadata.Items = (from item in Items select item.Metadata).ToArray();
                }
                
                return metadata;
            }
        }

        public override void Update(DataModelBase _update, bool purge)
        {
            var update = (ClipartGroupDataModel)_update;

            base.Update(update, purge);

            // Update ID

            if (update.ID != 0)
            {
                this.ID = update.ID;
            }

            // Update name

            if (update.Name != null &&
                update.NameModifiedAt >= this.NameModifiedAt)
            {
                this.Name = update.Name;
                this.NameModifiedAt = update.NameModifiedAt;
            }

            // Update items

            this.Items = UpdateArray(this.Items, update.Items, purge);
        }

        public void Validate(Int64 id, DateTime now)
        {
            ID = id;
            Name = (Name != null) ? Name : String.Empty;
            NameModifiedAt = (NameModifiedAt < now) ? NameModifiedAt : now;
        }

        public override object Clone()
        {
            var clone = (ClipartGroupDataModel)MemberwiseClone();
            clone.Items = CloneArray(this.Items);
            return clone;
        }

    }
}
