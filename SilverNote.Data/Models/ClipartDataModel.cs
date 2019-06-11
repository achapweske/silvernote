using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SilverNote.Data.Models
{
    [XmlType("clipart")]
    public class ClipartDataModel : DataModelBase
    {
        public ClipartDataModel()
        { }

        public ClipartDataModel(Int64 id, Int64 groupID)
        {
            this.ID = id;
            this.GroupID = groupID;
        }

        [XmlElement("id")]
        public Int64 ID { get; set; }

        [XmlElement("group_id")]
        public Int64 GroupID { get; set; }

        [XmlElement("group_id_modified_at")]
        public DateTime GroupIDModifiedAt { get; set; }

        public bool ShouldSerializeGroupIDModifiedAt()
        { return GroupIDModifiedAt != default(DateTime); }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("name_modified_at")]
        public DateTime NameModifiedAt { get; set; }

        public bool ShouldSerializeNameModifiedAt()
        { return NameModifiedAt != default(DateTime); }

        [XmlElement("data")]
        public string Data { get; set; }

        [XmlElement("data_modified_at")]
        public DateTime DataModifiedAt { get; set; }

        public bool ShouldSerializeDataModifiedAt()
        { return GroupIDModifiedAt != default(DateTime); }

        [XmlIgnore]
        public override object LocalID
        {
            get { return this.ID; }
        }

        [XmlIgnore]
        public ClipartDataModel Metadata
        {
            get
            {
                return new ClipartDataModel
                {
                    ID = this.ID,
                    Hash = this.Hash,
                    IsDeleted = this.IsDeleted
                };
            }
        }

        public override void Update(DataModelBase _update, bool purge)
        {
            var update = (ClipartDataModel)_update;

            base.Update(update, purge);

            // Update ID

            if (update.ID != 0)
            {
                this.ID = update.ID;
            }

            // Update group ID

            if (update.GroupID != 0 && 
                update.GroupIDModifiedAt >= this.GroupIDModifiedAt)
            {
                this.GroupID = update.GroupID;
                this.GroupIDModifiedAt = update.GroupIDModifiedAt;
            }

            // Update name

            if (update.Name != null && 
                update.NameModifiedAt >= this.NameModifiedAt)
            {
                this.Name = update.Name;
                this.NameModifiedAt = update.NameModifiedAt;
            }

            // Update Data

            if (update.Data != null && 
                update.DataModifiedAt >= this.DataModifiedAt)
            {
                this.Data = update.Data;
                this.DataModifiedAt = update.DataModifiedAt;
            }
        }

        public override object Clone()
        {
            return MemberwiseClone();
        }

    }
}
