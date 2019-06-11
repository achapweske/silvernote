using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SilverNote.Data.Models
{
    [XmlType("note")]
    public class NoteDataModel : DataModelBase, ICloneable
    {
        public NoteDataModel()
        { }

        public NoteDataModel(Int64 id, Int64 notebookID)
        {
            this.ID = id;
            this.NotebookID = notebookID;
        }

        [XmlAttribute("id")]
        public Int64 ID { get; set; }

        [XmlElement("notebook_id")]
        [DefaultValue(0)]
        public Int64 NotebookID { get; set; }

        [XmlAttribute("href")]
        public string Url { get; set; }

        [XmlElement("title")]
        [DefaultValue(null)]
        public string Title { get; set; }

        [XmlElement("content")]
        [DefaultValue(null)]
        public string Content { get; set; }

        [XmlElement("text")]
        [DefaultValue(null)]
        public string Text { get; set; }

        [XmlElement("files")]
        public FileDataModel[] Files { get; set; }

        [XmlArray("categories")]
        public CategoryDataModel[] Categories { get; set; }

        [XmlElement("categories_modified_at")]
        public DateTime CategoriesModifiedAt { get; set; }

        public bool ShouldSerializeCategoriesModifiedAt()
        { return CategoriesModifiedAt != default(DateTime); }

        [XmlIgnore]
        public override object LocalID
        {
            get { return ID; }
        }

        [XmlIgnore]
        public NoteDataModel Metadata
        {
            get
            {
                return new NoteDataModel
                {
                    ID = this.ID,
                    Hash = this.Hash,
                    IsDeleted = this.IsDeleted
                };
            }
        }

        public override void Update(DataModelBase _update, bool purge)
        {
            var update = (NoteDataModel)_update;

            base.Update(update, purge);

            // ID

            if (update.ID != 0)
            {
                this.ID = update.ID;
            }

            // NotebookID

            if (update.NotebookID != 0)
            {
                this.NotebookID = update.NotebookID;
            }

            // Url

            if (update.Url != null)
            {
                this.Url = update.Url;
            }

            // Title

            if (update.Title != null)
            {
                this.Title = update.Title;
            }

            // Content

            if (update.Content != null)
            {
                this.Content = update.Content;
            }

            // Text

            if (update.Text != null)
            {
                this.Text = update.Text;
            }

            // Files

            if (update.Files != null)
            {
                this.Files = UpdateArray(this.Files, update.Files, purge);
            }

            // Categories

            if (update.Categories != null)
            {
                this.Categories = UpdateArray(this.Categories, update.Categories, purge);
            }
        }

        public void Validate(Int64 notebookID, Int64 noteID, DateTime now)
        {
            ID = noteID;
            NotebookID = notebookID;
            Categories = (Categories != null) ? Categories : new CategoryDataModel[] { };

            base.Validate(now);
        }

        public override bool Equals(object obj)
        {
            var other = obj as NoteDataModel;

            return
                base.Equals(other) &&
                this.ID.Equals(other.ID) &&
                this.NotebookID.Equals(other.NotebookID) &&
                Object.Equals(this.Url, other.Url) &&
                Object.Equals(this.Title, other.Title) &&
                Object.Equals(this.Content, other.Content) &&
                Object.Equals(this.Text, other.Text) &&
                ArrayEquals(this.Files, other.Files) &&
                ArrayEquals(this.Categories, other.Categories) &&
                this.CategoriesModifiedAt.Equals(other.CategoriesModifiedAt);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override object Clone()
        {
            var clone = (NoteDataModel)MemberwiseClone();
            clone.Categories = CloneArray(this.Categories);
            return clone;
        }

    }
}
