/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SilverNote.Models
{
    public class NotebookLocationModel : ModelBase
    {
        public NotebookLocationModel()
        {

        }

        public NotebookLocationModel(string filePath)
        {
            Name = Path.GetFileNameWithoutExtension(filePath);
            Repository = new RepositoryLocationModel(filePath);
            Repository.Uri = Repository.Uri.Replace("file://", "sqlite://");
        }

        /// <summary>
        /// Display name for this notebook
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Repository containing this notebook
        /// </summary>
        public RepositoryLocationModel Repository { get; set; }

        /// <summary>
        /// ID assigned to this notebook by its repository
        /// </summary>
        public Int64 NotebookID { get; set; }

    }

    [XmlType("NotebookLocationCollection")]
    public class NotebookLocationCollection : List<NotebookLocationModel>
    {
        public NotebookLocationCollection()
        {

        }

        public NotebookLocationCollection(IEnumerable<NotebookLocationModel> collection)
            : base(collection)
        {

        }
    }
}
