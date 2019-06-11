using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SilverNote.Data.Models
{
    [XmlType("file")]
    public class FileDataModel : DataModelBase, ICloneable
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("note_id")]
        [DefaultValue(0)]
        public Int64 NoteID { get; set; }

        [XmlElement("notebook_id")]
        [DefaultValue(0)]
        public Int64 NotebookID { get; set; }

        [XmlElement("type")]
        public string Type { get; set; }

        [XmlElement("data")]
        public byte[] Data { get; set; }

        [XmlIgnore]
        public override object LocalID
        {
            get { return this.Name; }
        }

        [XmlIgnore]
        public FileDataModel Metadata
        {
            get
            {
                return new FileDataModel
                {
                    Name = this.Name
                };
            }
        }

        public override void Update(DataModelBase _update, bool purge)
        {
            var update = (FileDataModel)_update;

            base.Update(update, purge);

            // Update Name

            if (update.Name != null)
            {
                this.Name = update.Name;
            }

            // Update NoteID

            if (update.NoteID != 0)
            {
                this.NoteID = update.NoteID;
            }

            // Update NotebookID

            if (update.NotebookID != 0)
            {
                this.NotebookID = update.NotebookID;
            }

            // Update Type

            if (update.Type != null)
            {
                this.Type = update.Type;
            }

            // Update Data

            if (update.Data != null)
            {
                this.Data = update.Data;
            }
        }

        public void Validate(Int64 notebookID, Int64 noteID, string name, DateTime now)
        {
            Name = name;
            NotebookID = notebookID;
            NoteID = noteID;

            base.Validate(now);
        }

        public override bool Equals(object obj)
        {
            var other = obj as FileDataModel;

            return
                base.Equals(other) &&
                Object.Equals(this.Name, other.Name) &&
                this.NotebookID.Equals(other.NotebookID) &&
                this.NotebookID.Equals(other.NoteID) &&
                Object.Equals(this.Type, other.Type) &&
                Object.Equals(this.Data, other.Data);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override object Clone()
        {
            return MemberwiseClone();
        }
    }
}
