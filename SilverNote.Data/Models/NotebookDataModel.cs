using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.Globalization;

namespace SilverNote.Data.Models
{
    [XmlType("notebook")]
    public class NotebookDataModel : DataModelBase, ICloneable
    {
        [XmlAttribute("id")]
        [JsonIgnore]
        public Int64 ID { get; set; }

        // JSON can't handle 64-bit values
        [JsonProperty("ID")]
        [XmlIgnore]
        public string IDString
        {
            get { return ID.ToString(CultureInfo.InvariantCulture); }
        }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("name_modified_at")]
        public DateTime NameModifiedAt { get; set; }
        public bool ShouldSerializeNameModifiedAt()
        {
            return NameModifiedAt != default(DateTime);
        }

        [XmlElement("selected_note_id")]
        [DefaultValue(0)]
        public Int64 SelectedNoteID { get; set; }

        [XmlArray("open_notes")]
        public Int64[] OpenNotes { get; set; }

        [XmlArray("categories")]
        public CategoryDataModel[] Categories { get; set; }

        [XmlElement("notes")]
        public NoteDataModel[] Notes { get; set; }

        [XmlIgnore]
        public override object LocalID
        {
            get { return this.ID; }
        }

        public override void Update(DataModelBase _update, bool purge)
        {
            var update = (NotebookDataModel)_update;

            base.Update(update, purge);

            // Update ID

            if (update.ID != 0)
            {
                this.ID = update.ID;
            }

            // Update Name

            if (update.Name != null &&
                update.NameModifiedAt >= this.NameModifiedAt)
            {
                this.Name = update.Name;
                this.NameModifiedAt = update.NameModifiedAt;
            }

            // Update SelectedNoteID

            if (update.SelectedNoteID != 0)
            {
                this.SelectedNoteID = update.SelectedNoteID;
            }

            // Update OpenNotes

            if (update.OpenNotes != null)
            {
                this.OpenNotes = update.OpenNotes;
            }

            // Update Categories

            this.Categories = UpdateArray(this.Categories, update.Categories, purge);

            // Update Notes

            this.Notes = UpdateArray(this.Notes, update.Notes, purge);
        }

        public void Validate(Int64 id, DateTime now)
        {
            ID = id;
            Name = (Name != null) ? Name : String.Empty;
            NameModifiedAt = (NameModifiedAt < now) ? NameModifiedAt : now;
            Categories = (Categories != null) ? Categories : new CategoryDataModel[] { };
            SelectedNoteID = (SelectedNoteID != 0) ? SelectedNoteID : -1;
        }

        public override object Clone()
        {
            var clone = (NotebookDataModel)MemberwiseClone();
            clone.OpenNotes = Clone(this.OpenNotes);
            clone.Categories = CloneArray(this.Categories);
            clone.Notes = CloneArray(this.Notes);
            return clone;
        }
    }
}
