/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Text.RegularExpressions;
using SilverNote.Data.Models;

namespace SilverNote.Data.Store
{
    public delegate NoteStore NoteStoreFactory(Uri uri, string username, SecureString password, bool autoCreate);

    public class NoteStore : IDisposable
    {
        #region Static Members

        static NoteStore()
        {
            Factories = new List<NoteStoreFactory>();
            Factories.Add(NoteDbStore.Create);
            Factories.Add(NoteFileStore.Create);
        }

        public static List<NoteStoreFactory> Factories { get; private set; }

        public static NoteStore Create(Uri uri, string username, SecureString password, bool autoCreate)
        {
            foreach (var factory in Factories)
            {
                var result = factory(uri, username, password, autoCreate);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        #endregion

        #region Algorithms

        public virtual DateTime Now
        {
            get { return DateTime.UtcNow; }
        }

        public virtual string Hash(string inputString)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);
            byte[] outputBytes = System.Security.Cryptography.SHA1.Create().ComputeHash(inputBytes);
            return Convert.ToBase64String(outputBytes);
        }

        public virtual string XmlToPlaintext(string xml)
        {
            int bodyIndex = xml.IndexOf("<body");
            if (bodyIndex != -1)
            {
                xml = xml.Substring(bodyIndex);
            }

            xml = Regex.Replace(xml, @"</(h\d|p|li)>", (match) => " " + match.ToString());
            xml = Regex.Replace(xml, @"<[^>]+>", "");
            xml = Regex.Replace(xml, @"\s+", " ");

            return xml;
        }

        #endregion

        #region Repository

        public virtual RepositoryDataModel GetRepository()
        {
            throw new NotImplementedException();
        }

        public virtual bool SetRepository(RepositoryDataModel repository)
        {
            throw new NotImplementedException();
        }

        public virtual bool UpdateRepository(RepositoryDataModel update, bool purge = false)
        {
            var repository = GetRepository();

            repository.Update(update, purge);

            return SetRepository(repository);
        }

        public virtual Int64 GetSelectedNotebook()
        {
            var repository = GetRepository();
            if (repository != null)
            {
                return repository.SelectedNotebookID;
            }
            else
            {
                return -1;
            }
        }

        public virtual bool SetSelectedNotebook(Int64 notebookID)
        {
            var repository = new RepositoryDataModel
            {
                SelectedNotebookID = notebookID
            };

            return UpdateRepository(repository);
        }

        public virtual bool HasPassword
        {
            get { return false; }
        }

        public virtual bool ChangePassword(SecureString oldPassword, SecureString newPassword)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Notebooks

        public virtual NotebookDataModel[] GetNotebooks()
        {
            var repository = GetRepository();

            if (repository != null)
            {
                return repository.Notebooks;
            }
            else
            {
                return null;
            }
        }

        public virtual bool SetNotebooks(NotebookDataModel[] newNotebooks)
        {
            var oldNotebooks = GetNotebooks();
            if (oldNotebooks == null)
            {
                return false;
            }

            var purgeNotebooks = oldNotebooks.Except(newNotebooks, DataModelComparer.Instance);
            foreach (NotebookDataModel notebook in purgeNotebooks)
            {
                PurgeNotebook(notebook.ID);
            }

            var createNotebooks = newNotebooks.Except(oldNotebooks, DataModelComparer.Instance);
            foreach (NotebookDataModel notebook in createNotebooks)
            {
                CreateNotebook(notebook);
            }

            var setNotebooks = newNotebooks.Intersect(oldNotebooks, DataModelComparer.Instance);
            foreach (NotebookDataModel notebook in setNotebooks)
            {
                SetNotebook(notebook.ID, notebook);
            }

            return true;
        }

        public virtual bool UpdateNotebooks(NotebookDataModel[] updates, bool deleteMissingItems, bool purge)
        {
            bool success = true;

            if (deleteMissingItems)
            {
                var oldNotebooks = GetNotebooks();
                if (oldNotebooks != null)
                {
                    var deleteNotebooks = oldNotebooks.Except(updates, DataModelComparer.Instance);
                    foreach (NotebookDataModel notebook in deleteNotebooks)
                    {
                        DeleteNotebook(notebook.ID, purge);
                    }
                }
            }

            foreach (var update in updates)
            {
                success &= UpdateNotebook(update.ID, update, purge);
            }

            return success;
        }

        public virtual Int64 CreateNotebook(NotebookDataModel notebook)
        {
            throw new NotImplementedException();
        }

        public virtual bool DeleteNotebook(Int64 notebookID, bool purge = false)
        {
            if (purge)
            {
                return PurgeNotebook(notebookID);
            }

            var notebook = new NotebookDataModel
            {
                ID = notebookID,
                IsDeleted = true
            };

            if (!SetNotebook(notebookID, notebook))
            {
                return CreateNotebook(notebook) != 0;
            }
            else
            {
                return true;
            }
        }

        public virtual bool PurgeNotebook(Int64 notebookID)
        {
            throw new NotImplementedException();
        }

        public virtual NotebookDataModel GetNotebook(Int64 notebookID)
        {
            throw new NotImplementedException();
        }

        public virtual bool SetNotebook(Int64 notebookID, NotebookDataModel notebook)
        {
            throw new NotImplementedException();
        }

        public virtual bool UpdateNotebook(Int64 notebookID, NotebookDataModel update, bool purge = false)
        {
            if (update.IsDeleted)
            {
                return DeleteNotebook(notebookID, purge) || purge;
            }

            var notebook = GetNotebook(notebookID);
            if (notebook != null)
            {
                notebook.Update(update, purge);

                return SetNotebook(notebookID, notebook);
            }
            else
            {
                return CreateNotebook(update) > 0;
            }
        }

        public virtual string GetNotebookName(Int64 notebookID)
        {
            var notebook = GetNotebook(notebookID);
            if (notebook != null)
            {
                return notebook.Name;
            }
            else
            {
                return null;
            }
        }

        public virtual bool SetNotebookName(Int64 notebookID, string newName, DateTime modifiedAt)
        {
            var notebook = new NotebookDataModel
            {
                ID = notebookID,
                Name = newName,
                NameModifiedAt = modifiedAt
            };

            return UpdateNotebook(notebookID, notebook);
        }

        public virtual Int64 GetSelectedNote(Int64 notebookID)
        {
            var notebook = GetNotebook(notebookID);
            if (notebook != null)
            {
                return notebook.SelectedNoteID;
            }
            else
            {
                return -1;
            }
        }

        public virtual bool SetSelectedNote(Int64 notebookID, Int64 noteID)
        {
            var notebook = new NotebookDataModel
            {
                ID = notebookID,
                SelectedNoteID = noteID
            };

            return UpdateNotebook(notebookID, notebook);

        }

        public virtual Int64[] GetOpenNotes(Int64 notebookID)
        {
            var notebook = GetNotebook(notebookID);
            if (notebook != null)
            {
                return notebook.OpenNotes;
            }
            else
            {
                return null;
            }
        }

        public virtual bool SetOpenNotes(Int64 notebookID, Int64[] openNotes)
        {
            var notebook = new NotebookDataModel
            {
                ID = notebookID,
                OpenNotes = openNotes
            };

            return UpdateNotebook(notebookID, notebook);
        }

        public virtual bool OpenNote(Int64 notebookID, Int64 noteID)
        {
            var oldOpenNotes = GetOpenNotes(notebookID);
            if (oldOpenNotes == null)
            {
                oldOpenNotes = new Int64[] { };
            }

            var newOpenNotes = new List<Int64>(oldOpenNotes);

            newOpenNotes.Add(noteID);

            return SetOpenNotes(notebookID, newOpenNotes.ToArray());
        }

        public virtual bool CloseNote(Int64 notebookID, Int64 noteID)
        {
            var oldOpenNotes = GetOpenNotes(notebookID);
            if (oldOpenNotes == null)
            {
                oldOpenNotes = new Int64[] { };
            }

            var newOpenNotes = new List<Int64>(oldOpenNotes);

            newOpenNotes.Remove(noteID);

            return SetOpenNotes(notebookID, newOpenNotes.ToArray());
        }

        #endregion

        #region Notes

        public enum NoteSort
        {
            ViewedAt,
            ModifiedAt,
            CreatedAt,
            Title
        }
        public enum NoteOrder
        {
            Descending,
            Ascending
        }

        public virtual SearchResultsDataModel FindNotes(Int64 notebookID, string search, DateTime createdAfter, DateTime createdBefore, DateTime modifiedAfter, DateTime modifiedBefore, DateTime viewedAfter, DateTime viewedBefore, NoteSort sort, NoteOrder order, int offset, int limit, bool returnText)
        {
            throw new NotImplementedException();
        }

        public virtual NoteDataModel[] GetNotes(Int64 notebookID)
        {
            throw new NotImplementedException();
        }

        public virtual bool UpdateNotes(Int64 notebookID, NoteDataModel[] updates, bool deleteMissingItems, bool purge)
        {
            bool success = true;

            if (deleteMissingItems)
            {
                var oldNotes = GetNotesMetadata(notebookID);
                if (oldNotes != null)
                {
                    var deleteNotes = oldNotes.Except(updates, DataModelComparer.Instance);
                    foreach (NoteDataModel note in deleteNotes)
                    {
                        DeleteNote(notebookID, note.ID, purge);
                    }
                }
            }

            foreach (var update in updates)
            {
                success &= UpdateNote(notebookID, update.ID, update, autoCreate: true, purge: purge);
            }

            return success;
        }

        public virtual NoteDataModel[] GetNotesMetadata(Int64 notebookID)
        {
            var notes = GetNotes(notebookID);
            if (notes == null)
            {
                return null;
            }

            for (int i = 0; i < notes.Length; i++)
            {
                notes[i] = notes[i].Metadata;
            }

            return notes;
        }

        public virtual bool ContainsNote(Int64 notebookID, Int64 noteID)
        {
            return GetNoteMetadata(notebookID, noteID) != null;
        }

        public virtual int GetNoteCount(Int64 notebookID)
        {
            var notes = GetNotesMetadata(notebookID);
            if (notes != null)
            {
                return (from note in notes where !note.IsDeleted select note).Count();
            }
            else
            {
                return -1;
            }
        }

        public virtual Int64 CreateNote(Int64 notebookID, NoteDataModel note)
        {
            throw new NotImplementedException();
        }

        public virtual bool DeleteNote(Int64 notebookID, Int64 noteID, bool purge = false)
        {
            if (purge)
            {
                return PurgeNote(notebookID, noteID);
            }

            var note = new NoteDataModel
            {
                ID = noteID,
                NotebookID = notebookID,
                IsDeleted = true
            };

            if (!SetNote(notebookID, noteID, note))
            {
                return CreateNote(notebookID, note) > 0;
            }
            else
            {
                return true;
            }
        }

        public virtual bool PurgeNote(Int64 notebookID, Int64 noteID)
        {
            throw new NotImplementedException();
        }

        public virtual NoteDataModel GetNote(Int64 notebookID, Int64 noteID)
        {
            throw new NotImplementedException();
        }

        public virtual bool SetNote(Int64 notebookID, Int64 noteID, NoteDataModel note)
        {
            throw new NotImplementedException();
        }

        public virtual bool UpdateNote(Int64 notebookID, Int64 noteID, NoteDataModel update, bool autoCreate = false, bool purge = false)
        {
            // Delete the note if marked for deletion

            if (update.IsDeleted)
            {
                return DeleteNote(notebookID, noteID, purge) || purge;
            }

            // Optimization (update contains no data)

            if (IsUpdateEmpty(update))
            {
                if (autoCreate && !ContainsNote(notebookID, noteID))
                {
                    return CreateNote(notebookID, update) > 0;
                }

                return true;
            }

            // Update the current note or create one if not found

            var note = GetNote(notebookID, noteID);
            if (note != null)
            {
                note.Update(update, false);

                return SetNote(notebookID, noteID, note);
            }
            else if (autoCreate)
            {
                return CreateNote(notebookID, update) > 0;
            }
            else
            {
                return false;
            }
        }

        public static bool IsUpdateEmpty(NoteDataModel update)
        {
            var emptyNote = new NoteDataModel(update.ID, update.NotebookID);

            return update.Equals(emptyNote);
        }

        public virtual NoteDataModel GetNoteMetadata(Int64 notebookID, Int64 noteID)
        {
            var note = GetNote(notebookID, noteID);
            if (note != null)
            {
                return note.Metadata;
            }
            else
            {
                return null;
            }
        }

        public virtual string GetNoteTitle(Int64 notebookID, Int64 noteID)
        {
            NoteDataModel note = GetNote(notebookID, noteID);
            if (note != null)
            {
                return note.Title;
            }
            else
            {
                return null;
            }
        }

        public virtual bool SetNoteTitle(Int64 notebookID, Int64 noteID, string newTitle)
        {
            var update = new NoteDataModel
            {
                ID = noteID,
                NotebookID = notebookID,
                Title = newTitle
            };

            return UpdateNote(notebookID, noteID, update);
        }

        public virtual string GetNoteContent(Int64 notebookID, Int64 noteID)
        {
            NoteDataModel note = GetNote(notebookID, noteID);
            if (note != null)
            {
                return note.Content;
            }
            else
            {
                return null;
            }
        }

        public virtual bool SetNoteContent(Int64 notebookID, Int64 noteID, string newContent, DateTime modifiedAt)
        {
            var update = new NoteDataModel
            {
                ID = noteID,
                NotebookID = notebookID,
                Title = ExtractTitle(newContent),
                Content = newContent,
                ModifiedAt = modifiedAt
            };

            return UpdateNote(notebookID, noteID, update);
        }

        public virtual string GetNoteText(Int64 notebookID, Int64 noteID)
        {
            NoteDataModel note = GetNote(notebookID, noteID);
            if (note != null)
            {
                return note.Text;
            }
            else
            {
                return null;
            }
        }

        public virtual bool SetNoteText(Int64 notebookID, Int64 noteID, string newText)
        {
            var update = new NoteDataModel
            {
                ID = noteID,
                NotebookID = notebookID,
                Text = newText
            };

            return UpdateNote(notebookID, noteID, update);
        }

        public virtual string ExtractTitle(string content)
        {
            int i = content.IndexOf("<title>");
            if (i == -1)
            {
                return String.Empty;
            }

            i += "<title>".Length;

            int j = content.IndexOf("</title>", i);
            if (j == -1)
            {
                return String.Empty;
            }

            int length = j - i;

            return content.Substring(i, length);
        }

        public virtual DateTime GetNoteCreatedAt(Int64 notebookID, Int64 noteID)
        {
            NoteDataModel note = GetNote(notebookID, noteID);
            if (note != null)
            {
                return note.CreatedAt;
            }
            else
            {
                return default(DateTime);
            }
        }

        public virtual DateTime GetNoteModifiedAt(Int64 notebookID, Int64 noteID)
        {
            NoteDataModel note = GetNote(notebookID, noteID);
            if (note != null)
            {
                return note.ModifiedAt;
            }
            else
            {
                return default(DateTime);
            }
        }

        public virtual DateTime GetNoteViewedAt(Int64 notebookID, Int64 noteID)
        {
            NoteDataModel note = GetNote(notebookID, noteID);
            if (note != null)
            {
                return note.ViewedAt;
            }
            else
            {
                return default(DateTime);
            }
        }

        public virtual bool SetNoteViewedAt(Int64 notebookID, Int64 noteID, DateTime viewedAt)
        {
            var update = new NoteDataModel
            {
                ID = noteID,
                NotebookID = notebookID,
                ViewedAt = viewedAt
            };

            return UpdateNote(notebookID, noteID, update);
        }

        public virtual CategoryDataModel[] GetNoteCategories(Int64 notebookID, Int64 noteID)
        {
            NoteDataModel note = GetNote(notebookID, noteID);
            if (note != null)
            {
                return note.Categories;
            }
            else
            {
                return null;
            }
        }

        public virtual bool SetNoteCategories(Int64 notebookID, Int64 noteID, CategoryDataModel[] categories)
        {
            var update = new NoteDataModel 
            {
                ID = noteID,
                NotebookID = notebookID,
                Categories = categories 
            };

            return UpdateNote(notebookID, noteID, update);
        }

        public virtual bool AddNoteCategory(Int64 notebookID, Int64 noteID, Int64 categoryID)
        {
            var categories = new List<CategoryDataModel>(GetNoteCategories(notebookID, noteID));

            var newCategory = new CategoryDataModel
            {
                ID = categoryID,
                NotebookID = notebookID
            };

            categories.Add(newCategory);

            return SetNoteCategories(notebookID, noteID, categories.ToArray());
        }

        public virtual bool RemoveNoteCategory(Int64 notebookID, Int64 noteID, Int64 categoryID)
        {
            var categories = new List<CategoryDataModel>(GetNoteCategories(notebookID, noteID));

            var category = categories.Find(DataModelBase.IDEqualsPredicate(categoryID));
            if (category != null)
            {
                categories.Remove(category);
                return SetNoteCategories(notebookID, noteID, categories.ToArray());
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Files

        public virtual FileDataModel[] GetFiles(Int64 notebookID, Int64 noteID)
        {
            throw new NotImplementedException();
        }

        public virtual bool SetFiles(Int64 notebookID, Int64 noteID, FileDataModel[] newFiles, bool purge)
        {
            var oldFiles = GetFilesMetadata(notebookID, noteID);
            if (oldFiles == null)
            {
                return false;
            }

            bool success = true;

            var purgeFiles = oldFiles.Except(newFiles, DataModelComparer.Instance);
            foreach (FileDataModel file in purgeFiles)
            {
                success &= DeleteFile(notebookID, noteID, file.Name, purge);
            }

            var createFiles = newFiles.Except(oldFiles, DataModelComparer.Instance);
            foreach (FileDataModel file in createFiles)
            {
                success &= CreateFile(notebookID, noteID, file) != null;
            }

            var updateFiles = newFiles.Intersect(oldFiles, DataModelComparer.Instance);
            foreach (FileDataModel file in updateFiles)
            {
                success &= UpdateFile(notebookID, noteID, file.Name, file);
            }

            return success;
        }

        public virtual bool UpdateFiles(Int64 notebookID, Int64 noteID, FileDataModel[] updates, bool purge)
        {
            bool success = true;

            foreach (var update in updates)
            {
                success &= UpdateFile(notebookID, noteID, update.Name, update, purge);
            }

            return success;
        }

        public virtual FileDataModel[] GetFilesMetadata(Int64 notebookID, Int64 noteID)
        {
            var files = GetFiles(notebookID, noteID);

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Metadata;
            }

            return files;
        }

        public virtual bool ContainsFile(Int64 notebookID, Int64 noteID, string fileName)
        {
            return GetFileMetadata(notebookID, noteID, fileName) != null;
        }

        public virtual string CreateFile(Int64 notebookID, Int64 noteID, FileDataModel file)
        {
            throw new NotImplementedException();
        }

        public virtual bool DeleteFile(Int64 notebookID, Int64 noteID, string fileName, bool purge = false)
        {
            if (purge)
            {
                return PurgeFile(notebookID, noteID, fileName);
            }

            var update = new FileDataModel 
            { 
                Name = fileName,
                NotebookID = notebookID,
                NoteID = noteID,
                IsDeleted = true 
            };

            return SetFile(notebookID, noteID, fileName, update);
        }

        public virtual bool PurgeFile(Int64 notebookID, Int64 noteID, string fileName)
        {
            throw new NotImplementedException();
        }

        public virtual FileDataModel GetFile(Int64 notebookID, Int64 noteID, string fileName)
        {
            throw new NotImplementedException();
        }

        public virtual bool SetFile(Int64 notebookID, Int64 noteID, string fileName, FileDataModel file)
        {
            throw new NotImplementedException();
        }

        public virtual bool UpdateFile(Int64 notebookID, Int64 noteID, string fileName, FileDataModel update, bool purge = false)
        {
            if (update.IsDeleted)
            {
                return DeleteFile(notebookID, noteID, fileName, purge) || purge;
            }

            if (IsUpdateEmpty(update))
            {
                if (!ContainsFile(notebookID, noteID, fileName))
                {
                    return CreateFile(notebookID, noteID, update) != null;
                }

                return true;
            }

            var file = GetFile(notebookID, noteID, fileName);
            if (file != null)
            {
                file.Update(update, false);

                return SetFile(notebookID, noteID, fileName, file);
            }
            else
            {
                return CreateFile(notebookID, noteID, update) != null;
            }
        }

        private static bool IsUpdateEmpty(FileDataModel file)
        {
            var emptyFile = new FileDataModel { Name = file.Name };

            return file.Equals(emptyFile);
        }

        public virtual FileDataModel GetFileMetadata(Int64 notebookID, Int64 noteID, string fileName)
        {
            var file = GetFile(notebookID, noteID, fileName);
            if (file != null)
            {
                return file.Metadata;
            }
            else
            {
                return null;
            }
        }

        public virtual byte[] GetFileData(Int64 notebookID, Int64 noteID, string fileName)
        {
            FileDataModel file = GetFile(notebookID, noteID, fileName);
            if (file != null)
            {
                return file.Data;
            }
            else
            {
                return null;
            }
        }

        public virtual bool SetFileData(Int64 notebookID, Int64 noteID, string fileName, byte[] newData, DateTime modifiedAt)
        {
            var file = new FileDataModel
            {
                Name = fileName,
                NotebookID = notebookID,
                NoteID = noteID,
                Data = newData,
                ModifiedAt = modifiedAt
            };

            return UpdateFile(notebookID, noteID, fileName, file);
        }

        #endregion

        #region Categories

        public virtual CategoryDataModel[] GetCategories(Int64 notebookID)
        {
            var notebook = GetNotebook(notebookID);
            if (notebook == null)
            {
                return null;
            }
            
            if (notebook.Categories != null)
            {
                return notebook.Categories;
            }
            else
            {
                return new CategoryDataModel[] { };
            }
        }

        public virtual bool SetCategories(Int64 notebookID, CategoryDataModel[] newCategories)
        {
            var notebook = GetNotebook(notebookID);
            if (notebook != null)
            {
                notebook.Categories = newCategories;
                return SetNotebook(notebookID, notebook);
            }
            else
            {
                return false;
            }
        }

        public virtual bool UpdateCategories(Int64 notebookID, CategoryDataModel[] updates, bool purge)
        {
            var categories = GetCategories(notebookID);
            if (categories == null)
            {
                return false;
            }

            var notebook = new NotebookDataModel { Categories = categories };
            var update = new NotebookDataModel { Categories = updates };

            notebook.Update(update, purge);

            return SetCategories(notebookID, notebook.Categories);
        }

        public virtual Int64 CreateCategory(Int64 notebookID, CategoryDataModel category)
        {
            if (category.ID == 0)
            {
                throw new ArgumentException("0 is not a valid category ID", "category");
            }

            var oldCategories = GetCategories(notebookID);
            if (oldCategories == null)
            {
                return 0;
            }

            if (oldCategories.Contains(category, DataModelComparer.Instance))
            {
                return 0;
            }

            var newCategories = new List<CategoryDataModel>(oldCategories);
            newCategories.Add(category);
            if (!SetCategories(notebookID, newCategories.ToArray()))
            {
                return 0;
            }

            return category.ID;
        }

        public virtual bool DeleteCategory(Int64 notebookID, Int64 categoryID, bool purge)
        {
            if (purge)
            {
                return PurgeCategory(notebookID, categoryID);
            }

            var category = new CategoryDataModel 
            {
                ID = categoryID,
                NotebookID = notebookID,
                IsDeleted = true 
            };

            if (!SetCategory(notebookID, categoryID, category))
            {
                return CreateCategory(notebookID, category) > 0;
            }
            else
            {
                return true;
            }
        }

        public virtual bool PurgeCategory(Int64 notebookID, Int64 categoryID)
        {
            var oldCategories = GetCategories(notebookID);
            if (oldCategories == null)
            {
                return false;
            }

            var newCategories = new List<CategoryDataModel>(oldCategories);

            if (newCategories.RemoveAll(DataModelBase.IDEqualsPredicate(categoryID)) == 0)
            {
                return false;
            }

            return SetCategories(notebookID, newCategories.ToArray());
        }

        public virtual CategoryDataModel GetCategory(Int64 notebookID, Int64 categoryID)
        {
            var categories = GetCategories(notebookID);
            if (categories == null)
            {
                return null;
            }

            var category = (from c in categories where c.ID == categoryID select c).FirstOrDefault();

            if (category != null)
            {
                category.Children = (from c in categories where c.ParentID == categoryID select c).ToArray();
            }

            return category;
        }

        public virtual bool SetCategory(Int64 notebookID, Int64 categoryID, CategoryDataModel category)
        {
            var categories = GetCategories(notebookID);
            if (categories == null)
            {
                return false;
            }

            for (int i = 0; i < categories.Length; i++)
            {
                if (categories[i].ID == categoryID)
                {
                    categories[i] = category;
                }
            }

            return SetCategories(notebookID, categories);
        }

        public virtual bool UpdateCategory(Int64 notebookID, Int64 categoryID, CategoryDataModel update)
        {
            var category = GetCategory(notebookID, categoryID);
            if (category != null)
            {
                category.Update(update, false);
                return SetCategory(notebookID, categoryID, category);
            }
            else
            {
                return CreateCategory(notebookID, update) > 0;
            }
        }

        public virtual string GetCategoryName(Int64 notebookID, Int64 categoryID)
        {
            var category = GetCategory(notebookID, categoryID);
            if (category != null)
            {
                return category.Name;
            }
            else
            {
                return null;
            }
        }

        public virtual bool SetCategoryName(Int64 notebookID, Int64 categoryID, string name, DateTime modifiedAt)
        {
            var update = new CategoryDataModel
            {
                ID = categoryID,
                NotebookID = notebookID,
                Name = name,
                NameModifiedAt = modifiedAt
            };

            return UpdateCategory(notebookID, categoryID, update);
        }

        public virtual Int64 GetCategoryParent(Int64 notebookID, Int64 categoryID)
        {
            var category = GetCategory(notebookID, categoryID);
            if (category != null)
            {
                return category.ParentID;
            }
            else
            {
                return 0;
            }
        }

        public virtual bool SetCategoryParent(Int64 notebookID, Int64 categoryID, Int64 parentID, DateTime modifiedAt)
        {
            var update = new CategoryDataModel
            {
                ID = categoryID,
                NotebookID = notebookID,
                ParentID = parentID,
                ParentIDModifiedAt = modifiedAt
            };

            return UpdateCategory(notebookID, categoryID, update);
        }

        public virtual CategoryDataModel[] GetCategoryChildren(Int64 notebookID, Int64 categoryID)
        {
            var category = GetCategory(notebookID, categoryID);
            if (category != null)
            {
                return category.Children;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Clipart

        public virtual ClipartGroupDataModel[] GetClipartGroups()
        {
            throw new NotImplementedException();
        }

        public virtual bool SetClipartGroups(ClipartGroupDataModel[] newClipartGroups)
        {
            var oldClipartGroups = GetClipartGroups();
            if (oldClipartGroups == null)
            {
                return false;
            }

            var purgeClipartGroups = oldClipartGroups.Except(newClipartGroups, DataModelComparer.Instance);
            foreach (ClipartGroupDataModel notebook in purgeClipartGroups)
            {
                PurgeClipartGroup(notebook.ID);
            }

            var createClipartGroups = newClipartGroups.Except(oldClipartGroups, DataModelComparer.Instance);
            foreach (ClipartGroupDataModel notebook in createClipartGroups)
            {
                CreateClipartGroup(notebook);
            }

            var setClipartGroups = newClipartGroups.Intersect(oldClipartGroups, DataModelComparer.Instance);
            foreach (ClipartGroupDataModel notebook in setClipartGroups)
            {
                SetClipartGroup(notebook.ID, notebook);
            }

            return true;
        }

        public virtual bool UpdateClipartGroups(ClipartGroupDataModel[] updates, bool deleteMissingItems, bool purge)
        {
            bool success = true;

            if (deleteMissingItems)
            {
                var oldGroups = GetClipartGroups();
                if (oldGroups != null)
                {
                    var deleteGroups = oldGroups.Except(updates, DataModelComparer.Instance);
                    foreach (ClipartGroupDataModel group in deleteGroups)
                    {
                        DeleteClipartGroup(group.ID, purge);
                    }
                }
            }

            foreach (var update in updates)
            {
                success &= UpdateClipartGroup(update.ID, update, purge);
            }

            return success;
        }

        public virtual Int64 CreateClipartGroup(ClipartGroupDataModel group)
        {
            throw new NotImplementedException();
        }

        public virtual bool DeleteClipartGroup(Int64 groupID, bool purge = false)
        {
            if (purge)
            {
                return PurgeClipartGroup(groupID);
            }

            var group = new ClipartGroupDataModel
            {
                ID = groupID,
                IsDeleted = true
            };

            if (!SetClipartGroup(groupID, group))
            {
                return CreateClipartGroup(group) != 0;
            }
            else
            {
                return true;
            }
        }

        public virtual bool PurgeClipartGroup(Int64 groupID)
        {
            throw new NotImplementedException();
        }

        public virtual ClipartGroupDataModel GetClipartGroup(Int64 groupID)
        {
            throw new NotImplementedException();
        }

        public virtual bool SetClipartGroup(Int64 groupID, ClipartGroupDataModel group)
        {
            throw new NotImplementedException();
        }

        public virtual bool UpdateClipartGroup(Int64 groupID, ClipartGroupDataModel update, bool purge = false)
        {
            if (update.IsDeleted)
            {
                return DeleteClipartGroup(groupID, purge) || purge;
            }

            var group = GetClipartGroup(groupID);
            if (group != null)
            {
                group.Update(update, purge);

                return SetClipartGroup(groupID, group);
            }
            else
            {
                return CreateClipartGroup(update) > 0;
            }
        }

        public virtual string GetClipartGroupName(Int64 groupID)
        {
            var group = GetClipartGroup(groupID);
            if (group != null)
            {
                return group.Name;
            }
            else
            {
                return null;
            }
        }

        public virtual bool SetClipartGroupName(Int64 groupID, string name, DateTime modifiedAt)
        {
            var update = new ClipartGroupDataModel
            {
                ID = groupID,
                Name = name,
                NameModifiedAt = modifiedAt
            };

            return UpdateClipartGroup(groupID, update);
        }

        public virtual ClipartDataModel[] GetClipartItems(Int64 groupID)
        {
            var group = GetClipartGroup(groupID);
            if (group != null)
            {
                return group.Items;
            }
            else
            {
                return null;
            }
        }

        public virtual bool UpdateClipartItems(Int64 groupID, ClipartDataModel[] updates, bool deleteMissingItems, bool purge)
        {
            bool success = true;

            if (deleteMissingItems)
            {
                var oldItems = GetClipartItemsMetadata(groupID);
                if (oldItems != null)
                {
                    var deleteItems = oldItems.Except(updates, DataModelComparer.Instance);
                    foreach (ClipartDataModel item in deleteItems)
                    {
                        DeleteClipart(groupID, item.ID, purge);
                    }
                }
            }

            foreach (var update in updates)
            {
                success &= UpdateClipart(groupID, update.ID, update, purge);
            }

            return success;
        }

        public virtual ClipartDataModel[] GetClipartItemsMetadata(Int64 groupID)
        {
            var items = GetClipartItems(groupID);
            if (items == null)
            {
                return null;
            }

            for (int i = 0; i < items.Length; i++)
            {
                items[i] = items[i].Metadata;
            }

            return items;
        }

        public virtual bool ContainsClipart(Int64 groupID, Int64 clipartID)
        {
            return GetClipartMetadata(groupID, clipartID) != null;
        }

        public virtual Int64 CreateClipart(Int64 groupID, ClipartDataModel clipart)
        {
            throw new NotImplementedException();
        }

        public virtual bool DeleteClipart(Int64 groupID, Int64 clipartID, bool purge = false)
        {
            if (purge)
            {
                return PurgeClipart(groupID, clipartID);
            }

            var clipart = new ClipartDataModel
            {
                GroupID = groupID,
                ID = clipartID,
                IsDeleted = true
            };

            if (!SetClipart(groupID, clipartID, clipart))
            {
                return CreateClipart(groupID, clipart) != 0;
            }
            else
            {
                return true;
            }
        }

        public virtual bool PurgeClipart(Int64 groupID, Int64 clipartID)
        {
            throw new NotImplementedException();
        }

        public virtual ClipartDataModel GetClipart(Int64 groupID, Int64 clipartID)
        {
            throw new NotImplementedException();
        }

        public virtual bool SetClipart(Int64 groupID, Int64 clipartID, ClipartDataModel clipart)
        {
            throw new NotImplementedException();
        }

        public virtual bool UpdateClipart(Int64 groupID, Int64 clipartID, ClipartDataModel update, bool purge = false)
        {
            // Delete the note if marked for deletion

            if (update.IsDeleted)
            {
                return DeleteClipart(groupID, clipartID, purge) || purge;
            }

            // Optimization (update contains no data)

            if (IsUpdateEmpty(update))
            {
                if (!ContainsClipart(groupID, clipartID))
                {
                    return CreateClipart(groupID, update) > 0;
                }

                return true;
            }

            // Update the current note or create one if not found

            var clipart = GetClipart(groupID, clipartID);
            if (clipart != null)
            {
                clipart.Update(update, purge);

                return SetClipart(groupID, clipartID, clipart);
            }
            else
            {
                return CreateClipart(groupID, update) > 0;
            }
        }

        public static bool IsUpdateEmpty(ClipartDataModel update)
        {
            var emptyItem = new ClipartDataModel(update.ID, update.GroupID);

            return update.Equals(emptyItem);
        }

        public virtual ClipartDataModel GetClipartMetadata(Int64 groupID, Int64 clipartID)
        {
            var clipart = GetClipart(groupID, clipartID);
            if (clipart != null)
            {
                return clipart.Metadata;
            }
            else
            {
                return null;
            }
        }

        public virtual string GetClipartName(Int64 groupID, Int64 clipartID)
        {
            var clipart = GetClipart(groupID, clipartID);
            if (clipart != null)
            {
                return clipart.Name;
            }
            else
            {
                return null;
            }
        }

        public virtual bool SetClipartName(Int64 groupID, Int64 clipartID, string name, DateTime modifiedAt)
        {
            var update = new ClipartDataModel
            {
                ID = clipartID,
                GroupID = groupID,
                Name = name,
                NameModifiedAt = modifiedAt
            };

            return UpdateClipart(groupID, clipartID, update);
        }

        public virtual string GetClipartData(Int64 groupID, Int64 clipartID)
        {
            var clipart = GetClipart(groupID, clipartID);
            if (clipart != null)
            {
                return clipart.Data;
            }
            else
            {
                return null;
            }
        }

        public virtual bool SetClipartData(Int64 groupID, Int64 clipartID, string data, DateTime modifiedAt)
        {
            var update = new ClipartDataModel
            {
                ID = clipartID,
                GroupID = groupID,
                Data = data,
                DataModifiedAt = modifiedAt
            };

            return UpdateClipart(groupID, clipartID, update);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {

        }

        #endregion
    }

    public class NotFoundException : Exception
    {
        public NotFoundException()
            : this("Not found")
        { }

        public NotFoundException(string message)
            : base(message)
        { }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException()
            : this("Unauthorized")
        { }

        public UnauthorizedException(string message)
            : base(message)
        { }

        public UnauthorizedException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    public class NoteStoreException : Exception
    {
        public NoteStoreException()
        { }

        public NoteStoreException(string message)
            : base(message)
        { }

        public NoteStoreException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
