/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Models
{
    public static class Locator
    {
        public static object Locate(NotebookModel notebook, Uri uri)
        {            
            var notebookUri = new Uri(notebook.Source.Client.Uri.ToString().TrimEnd('/') + "/notebooks/" + notebook.ID);
            if (!uri.ToString().StartsWith(notebookUri.ToString()))
            {
                return null;
            }
            var path = uri.Segments.Skip(notebookUri.Segments.Length).Select(str => str.Trim('/')).ToList();

            if (path.Count == 0)
            {
                return notebook;
            }

            if (path[0] == "notes")
            {
                if (path.Count == 1)
                {
                    return notebook.Search;
                }

                Int64 noteID;
                if (!Int64.TryParse(path[1], out noteID))
                {
                    return null;
                }

                var note = notebook.GetNote(noteID);
                if (path.Count == 2)
                {
                    return note;
                }
                else
                {
                    return null;
                }
            }
            else if (path[0] == "categories")
            {
                if (path.Count == 1)
                {
                    return notebook.Categories;
                }

                Int64 categoryID;
                if (!Int64.TryParse(path[1], out categoryID))
                {
                    return null;
                }

                var category = notebook.GetCategory(categoryID);
                if (path.Count == 2)
                {
                    return category;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
