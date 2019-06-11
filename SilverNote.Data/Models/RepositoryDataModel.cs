using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SilverNote.Data.Models
{
    [XmlType("repository")]
    public class RepositoryDataModel : DataModelBase, ICloneable
    {
        [XmlElement("selected_notebook_id")]
        [DefaultValue(0)]
        public Int64 SelectedNotebookID { get; set; }

        [XmlElement("has_password")]
        public bool? HasPassword { get; set; }

        [XmlArray("notebooks")]
        public NotebookDataModel[] Notebooks { get; set; }

        [XmlElement("clipart_groups")]
        public ClipartGroupDataModel[] ClipartGroups { get; set; }

        public override void Update(DataModelBase _update, bool purge)
        {
            var update = (RepositoryDataModel)_update;

            base.Update(update, purge);

            // Update SelectedNotebookID

            if (update.SelectedNotebookID != 0)
            {
                this.SelectedNotebookID = update.SelectedNotebookID;
            }

            // Update HasPassword

            if (update.HasPassword != null)
            {
                HasPassword = update.HasPassword;
            }

            // Update Notebooks

            this.Notebooks = UpdateArray(this.Notebooks, update.Notebooks, purge);

            // Update ClipartGroups

            this.ClipartGroups = UpdateArray(this.ClipartGroups, update.ClipartGroups, purge);

        }

        public override object Clone()
        {
            var clone = (RepositoryDataModel)MemberwiseClone();
            clone.Notebooks = CloneArray(this.Notebooks);
            clone.ClipartGroups = CloneArray(this.ClipartGroups);
            return clone;
        }

    }
}
