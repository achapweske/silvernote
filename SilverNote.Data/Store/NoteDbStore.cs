/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Xml.Serialization;
using SilverNote.Data.Models;

namespace SilverNote.Data.Store
{
    public class NoteDbStore : NoteStore
    {
        #region Fields

        private bool _HasPassword;

        #endregion

        #region Static Methods

        /// <summary>
        /// Create an instance of NoteDbStore capable of servicing the given URL.
        /// 
        /// If the URL does not specify an sqlite repository, this method returns null.
        /// </summary>
        new public static NoteStore Create(Uri uri, string username, SecureString password, bool autoCreate)
        {
            if (uri.Scheme != "sqlite")
            {
                return null;
            }

            NoteDbStore result = new NoteDbStore();
            string filePath = FilePathFromUri(uri);
            result.Open(filePath, username, password, autoCreate);
            return result;
        }

        private static string FilePathFromUri(Uri uri)
        {
            string filePath = uri.LocalPath;
            if (filePath.StartsWith("/")) filePath = "." + filePath;
            filePath = filePath.Replace('/', Path.DirectorySeparatorChar);
            return filePath;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the database filename passed Open()
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// Get the username passed to Open()
        /// </summary>
        public string Username { get; private set; }

        #endregion

        #region Open/Close

        /// <summary>
        /// Initialize the underlying SQLite connection
        /// </summary>
        public void Open(string filename, string username, SecureString password = null, bool autoCreate = true)
        {
            Filename = filename;
            Username = username;
            _HasPassword = password != null && password.Length > 0;
            //Password = password;
            // (For security reasons, the password is disposed after initialization)

            // If the database file doesn't exist, we'll need to write its schema after it's been created

            bool needSchema = !System.IO.File.Exists(filename);

            // Now actually attempt to open/create the database

            DoOpen(filename, password, autoCreate);

            // Write the schema if needed

            if (needSchema)
            {
                try
                {
                    WriteSchema();
                }
                catch (Exception e)
                {
                    // Clean-up after ourselves if an error occurs

                    Close();
                    System.IO.File.Delete(filename);
                    string message = "An error occurred while initializing the database:\n" + e.Message;
                    throw new Exception(message, e);
                }
            }

            // Apply patches

            int schemaVersion = GetSchemaVersion();
            int currentVersion = GetRepositoryVersion();

            for (int i = currentVersion; i < schemaVersion; i++)
            {
                if (!UpdateSchema(i))
                {
                    string message = String.Format("Repository update {0} not found", i);
                    throw new Exception(message);
                }
            }
        }

        /// <summary>
        /// Close the underlyign SQLite connection
        /// </summary>
        public void Close()
        {
            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
                Connection = null;
            }
        }

        /// <summary>
        /// Determine if the underlying SQLite connection is open
        /// </summary>
        /// <returns></returns>
        public bool IsOpen()
        {
            return (Connection != null);
        }

        #endregion

        #region Read/Write

        /// <summary>
        /// Create a DbCommand object from the given string
        /// </summary>
        public DbCommand CreateCommand(string sql, params object[] args)
        {
            DbCommand command = null;
            try
            {
                command = Connection.CreateCommand();

                // CommandText
                
                command.CommandText = sql;

                // Parameters

                for (int i = 0; (i + 1) < args.Length; i += 2)
                {
                    string name = (string)args[i];
                    object value = args[i + 1];

                    AddParameter(command, name, value);
                }
            }
            catch
            {
                if (command != null)
                {
                    command.Dispose();
                    command = null;
                }
                throw;
            }

            return command;
        }

        public static void AddParameter(DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();

            parameter.ParameterName = name;
            parameter.Value = value;

            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Execute an SQL "write" operation (INSERT, UPDATE, etc.)
        /// </summary>
        /// <returns>true if at least one row was modified; otherwise false</returns>
        public bool Write(string sql, params object[] args)
        {
            using (var command = CreateCommand(sql, args))
            {
                return command.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Read a single scalar object
        /// 
        /// A NotFoundException() is thrown if the result set is empty
        /// </summary>
        /// <returns>this always returns the read object</returns>
        private object Read(string sql, params object[] args)
        {
            using (var command = CreateCommand(sql, args))
            {
                return Read(command);
            }
        }

        /// <summary>
        /// Overload
        /// </summary>
        private object Read(DbCommand command)
        {
            object result = command.ExecuteScalar();

            if (result == null)
            {
                throw new NotFoundException("The following command returned null: \n\n" + command.CommandText);
            }

            return result;
        }

        /// <summary>
        /// Execute an SQL "read" operation and return the first result as a string
        /// 
        /// If no item was found, this throws a NotFoundException();
        /// if an item was found but set to null, this returns String.Empty;
        /// otherwise, this returns the read string.
        /// </summary>
        private string ReadString(string sql, params object[] args)
        {
            object result = Read(sql, args);

            if (result != DBNull.Value)
            {
                return Convert.ToString(result);
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Execute an SQL "read" operation and return the first result as an Int32
        /// 
        /// If no item was found, this throws a NotFoundException();
        /// if an item was found but set to null, this returns 0;
        /// otherwise, this returns the read integer.
        /// </summary>
        private Int32 ReadInt32(string sql, params object[] args)
        {
            object result = Read(sql, args);

            if (result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Overload
        /// </summary>
        private Int32 ReadInt32(DbCommand command)
        {
            object result = Read(command);

            if (result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Execute an SQL "read" operation and return the first result as an Int64
        /// 
        /// If no item was found, this throws a NotFoundException();
        /// if an item was found but set to null, this returns 0;
        /// otherwise, this returns the read integer.
        /// </summary>
        private Int64 ReadInt64(string sql, params object[] args)
        {
            object result = Read(sql, args);
            if (result != DBNull.Value)
            {
                return Convert.ToInt64(result);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Execute an SQL "read" operation and return the first result as a DateTime
        /// 
        /// If no item was found, this throws a NotFoundException();
        /// if an item was found but set to null, this returns default(DateTime);
        /// otherwise, this returns the read DateTime.
        /// </summary>
        private DateTime ReadDateTime(string sql, params object[] args)
        {
            object result = Read(sql, args);
            if (result != DBNull.Value)
            {
                return Convert.ToDateTime(result);
            }
            else
            {
                return default(DateTime);
            }
        }

        /// <summary>
        /// Execute an SQL "read" operation and return the first result as a byte array
        /// 
        /// If no item was found, this throws a NotFoundException();
        /// if an item was found but set to null, this returns null;
        /// otherwise, this returns the read byte array.
        /// </summary>
        private byte[] ReadBytes(string sql, params object[] args)
        {
            object result = Read(sql, args);
            if (result != DBNull.Value)
            {
                return result as byte[];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Execute an SQL "read" operation and return the first row in the result set as an object of the given type.
        /// 
        /// If the result set is empty, this returns null
        /// </summary>
        public T ReadObject<T>(DbCommand command) where T : class, new()
        {
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return ReadObject<T>(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Overload
        /// </summary>
        private T ReadObject<T>(string sql, params object[] args) where T : class, new()
        {
            using (var command = CreateCommand(sql, args))
            {
                return ReadObject<T>(command);
            }
        }

        /// <summary>
        /// Execute an SQL "read" operation and return each row as an object of the given type
        /// 
        /// If no results are found, this returns an empty array.
        /// </summary>
        private T[] ReadArray<T>(DbCommand command)
        {
            // Execute the query

            using (var reader = command.ExecuteReader())
            {
                List<T> result = new List<T>();

                // Parse each row in the result set

                while (reader.Read())
                {
                    T value = (T)reader.GetValue(0);

                    result.Add(value);
                }

                return result.ToArray();
            }
        }

        /// <summary>
        /// Overload
        /// </summary>
        private T[] ReadArray<T>(string sql, params object[] args)
        {
            using (DbCommand command = CreateCommand(sql, args))
            {
                return ReadArray<T>(command);
            }
        }

        /// <summary>
        /// Execute an SQL "read" command and convert each row in the result set to an object of the given type
        /// </summary>
        private T[] ReadObjects<T>(DbCommand command) where T : new()
        {
            using (var reader = command.ExecuteReader())
            {
                List<T> result = new List<T>();

                while (reader.Read())
                {
                    result.Add(ReadObject<T>(reader));
                }

                return result.ToArray();
            }
        }

        /// <summary>
        /// Overload
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private T[] ReadObjects<T>(string sql, params object[] args) where T : new()
        {
            using (DbCommand command = CreateCommand(sql, args))
            {
                return ReadObjects<T>(command);
            }
        }

        /// <summary>
        /// Convert the current row in the given result set to an object of the given type
        /// </summary>
        private T ReadObject<T>(DbDataReader reader) where T : new()
        {
            var result = new T();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string name = reader.GetName(i);
                object value = reader.GetValue(i);

                SetProperty(result, name, value);
            }

            return result;
        }

        /// <summary>
        /// For the given object, find its property that corresponds to the 
        /// given database column name and set its value
        /// </summary>
        private void SetProperty(object target, string name, object value)
        {
            var property = GetProperty(target.GetType(), name);

            if (property != null)
            {
                object convertedValue = null;

                if (value != DBNull.Value)
                {
                    convertedValue = Convert.ChangeType(value, property.PropertyType);
                }

                property.SetValue(target, convertedValue, null);
            }
        }

        // Map types to attributes
        class PropertyMap : Dictionary<Type, AttributeMap>
        { }

        // Map attributes to properties
        class AttributeMap : Dictionary<string, PropertyInfo>
        { }

        PropertyMap CachedProperties = new PropertyMap();

        /// <summary>
        /// Get the specified property belonging to the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private PropertyInfo GetProperty(Type type, string name)
        {
            AttributeMap attributes;
            if (!CachedProperties.TryGetValue(type, out attributes))
            {
                attributes = GetAttributes(type);
                CachedProperties.Add(type, attributes);
            }

            PropertyInfo result;
            if (attributes.TryGetValue(name, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get the set of all properties belonging to type indexed by
        /// their XmlAttribute and XmlElement attribute names.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static AttributeMap GetAttributes(Type type)
        {
            var results = new AttributeMap();

            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                // Get all XmlAttribute attributes

                var attributes = property.GetCustomAttributes(typeof(XmlAttributeAttribute), false);

                foreach (XmlAttributeAttribute attribute in attributes)
                {
                    results.Add(attribute.AttributeName, property);
                }

                // Get all XmlElement attribute

                var elements = property.GetCustomAttributes(typeof(XmlElementAttribute), false);

                foreach (XmlElementAttribute element in elements)
                {
                    results.Add(element.ElementName, property);
                }
            }

            return results;
        }

        #endregion

        #region Schema

        /// <summary>
        /// Write our schema to the underlying database
        /// </summary>
        public void WriteSchema()
        {
            string schema = GetSchema();

            Write(schema);

            Write("INSERT OR IGNORE INTO Repository (user_id) VALUES (@user_id)", "@user_id", Username);
        }

        /// <summary>
        /// Update the schema FROM the given version 
        /// </summary>
        /// <returns>True on success, or false if no update is available</returns>
        protected bool UpdateSchema(int version)
        {
            string update = GetSchemaUpdate(version);

            if (String.IsNullOrEmpty(update))
            {
                return false;
            }

            return Write(update);
        }

        const string SCHEMA_PATH = "SilverNote.Data.Store.NoteDbSchema.txt";

        /// <summary>
        /// Get our database schema (stored in the embedded NoteDbSchema.txt)
        /// </summary>
        private static string GetSchema()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(SCHEMA_PATH))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static int GetSchemaVersion()
        {
            string schema = GetSchema();

            int i = schema.IndexOf("Repository", 0, StringComparison.OrdinalIgnoreCase);
            if (i == -1)
            {
                throw new Exception("Repository table not found");
            }

            i = schema.IndexOf("version INTEGER DEFAULT", i, StringComparison.OrdinalIgnoreCase);
            if (i == -1)
            {
                throw new Exception("Repository table not valid");
            }

            i += "version INTEGER DEFAULT".Length;

            int j = schema.IndexOfAny(",)".ToCharArray(), i);
            if (j == -1)
            {
                throw new Exception("Repository table not valid");
            }

            string version = schema.Substring(i, j - i).Trim();

            return int.Parse(version);
        }

        const string UPDATES_PATH = "SilverNote.Data.Store.NoteDbUpdates";

        private static string GetSchemaUpdate(int version)
        {
            string updatePath = String.Format("{0}.{1}.txt", UPDATES_PATH, version);

            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(updatePath))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        #endregion

        #region Repository

        public int GetRepositoryVersion()
        {
            try
            {
                return ReadInt32("SELECT version FROM Repository");
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public bool SetRepositoryVersion(int newVersion)
        {
            return Write("UPDATE Repository SET version=@version", "@version", newVersion);
        }

        public override RepositoryDataModel GetRepository()
        {
            var result = ReadObject<RepositoryDataModel>("SELECT * FROM Repository");

            if (result != null)
            {
                result.Notebooks = GetNotebooks();
            }

            return result;
        }

        public override bool SetRepository(RepositoryDataModel repository)
        {
            if (!Write(
                "UPDATE Repository SET selected_notebook_id=@id",
                "@id", repository.SelectedNotebookID
            )) return false;

            return SetNotebooks(repository.Notebooks);
        }

        public override Int64 GetSelectedNotebook()
        {
            return ReadInt64("SELECT selected_notebook_id FROM Repository");
        }

        public override bool SetSelectedNotebook(Int64 notebookID)
        {
            return Write("UPDATE Repository SET selected_notebook_id=@id",
                "@id", notebookID
            );
        }

        public override bool HasPassword
        {
            get { return _HasPassword; }
        }

        public override bool ChangePassword(SecureString oldPassword, SecureString newPassword)
        {
            string filename = this.Filename;
            string username = this.Username;

            // Try to change the password using a new connection

            using (var db = new NoteDbStore())
            {
                try
                {
                    db.Open(filename, username, oldPassword, false);

                    // Now actually change the password
                    if (newPassword != null && newPassword.Length > 0)
                    {
                        string key = SecureStringToHashedString(newPassword);
                        db.Connection.ChangePassword(key);
                    }
                    else
                    {
                        db.Connection.ChangePassword("");
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            // If that succeeds, re-open our current connection using the new password

            Close();
            Open(filename, username, newPassword, false);

            return true;
        }

        #endregion

        #region Notebooks

        public override NotebookDataModel[] GetNotebooks()
        {
            var notebooks = ReadObjects<NotebookDataModel>("SELECT * FROM Notebooks");

            if (notebooks == null)
            {
                notebooks = new NotebookDataModel[0];
            }

            foreach (var notebook in notebooks)
            {
                notebook.OpenNotes = GetOpenNotes(notebook.ID);
            }

            return notebooks;
        }

        public override Int64 CreateNotebook(NotebookDataModel notebook)
        {
            if (notebook.ID == 0)
            {
                throw new ArgumentException("0 is not a valid ID", "notebook");
            }

            if (!Write(
                "INSERT INTO Notebooks (id, name, name_modified_at, selected_note_id, created_at, modified_at, viewed_at, is_deleted) VALUES (@id, @name, @name_modified_at, @selected_note_id, @created_at, @modified_at, @viewed_at, @is_deleted)",
                "@id", notebook.ID,
                "@name", notebook.Name,
                "@name_modified_at", notebook.NameModifiedAt,
                "@selected_note_id", notebook.SelectedNoteID,
                "@is_deleted", notebook.IsDeleted,
                "@created_at", notebook.CreatedAt,
                "@modified_at", notebook.ModifiedAt,
                "@viewed_at", notebook.ViewedAt
            )) return 0;

            return notebook.ID;
        }

        public override bool PurgeNotebook(Int64 notebookID)
        {
            return Write("DELETE FROM Notebooks WHERE id=@id", "@id", notebookID);
        }

        public override NotebookDataModel GetNotebook(Int64 notebookID)
        {
            var result = ReadObject<NotebookDataModel>(
                "SELECT * FROM Notebooks WHERE id=@id",
                "@id", notebookID
            );

            if (result != null)
            {
                result.Categories = GetCategories(notebookID);
                result.OpenNotes = GetOpenNotes(notebookID);
            }

            return result;
        }

        public override bool SetNotebook(Int64 notebookID, NotebookDataModel notebook)
        {
            if (!Write(
                "UPDATE Notebooks SET name=@name, name_modified_at=@name_modified_at, selected_note_id=@selected_note_id, created_at=@created_at, modified_at=@modified_at, viewed_at=@viewed_at, is_deleted=@is_deleted WHERE id=@id",
                "@name", notebook.Name,
                "@name_modified_at", notebook.NameModifiedAt,
                "@selected_note_id", notebook.SelectedNoteID,
                "@created_at", notebook.CreatedAt,
                "@modified_at", notebook.ModifiedAt,
                "@viewed_at", notebook.ViewedAt,
                "@is_deleted", notebook.IsDeleted,
                "@id", notebook.ID
            )) return false;

            bool success = SetCategories(notebookID, notebook.Categories);
            success &= SetOpenNotes(notebookID, notebook.OpenNotes);

            return success;
        }

        public override string GetNotebookName(Int64 notebookID)
        {
            return ReadString("SELECT name FROM Notebooks WHERE id=@id", "@id", notebookID);
        }

        public override Int64 GetSelectedNote(Int64 notebookID)
        {
            return ReadInt64("SELECT selected_note_id FROM Notebooks WHERE id=@notebook_id", "@notebook_id", notebookID);
        }

        public override bool SetSelectedNote(Int64 notebookID, Int64 noteID)
        {
            return Write(
                "UPDATE Notebooks SET selected_note_id=@note_id WHERE id=@notebook_id",
                "@note_id", noteID,
                "@notebook_id", notebookID
            );
        }

        public override Int64[] GetOpenNotes(Int64 notebookID)
        {
            return ReadArray<Int64>(
                "SELECT note_id FROM OpenNotes WHERE notebook_id=@notebook_id ORDER BY sequence",
                "@notebook_id", notebookID
            );
        }

        public override bool SetOpenNotes(Int64 notebookID, Int64[] newOpenNotes)
        {
            var oldOpenNotes = GetOpenNotes(notebookID);
            if (oldOpenNotes == null)
            {
                return false;
            }

            foreach (var noteID in oldOpenNotes)
            {
                CloseNote(notebookID, noteID);
            }

            if (newOpenNotes != null)
            {
                foreach (var noteID in newOpenNotes)
                {
                    OpenNote(notebookID, noteID);
                }
            }

            return true;
        }

        public override bool OpenNote(Int64 notebookID, Int64 noteID)
        {
            if (!ContainsNote(notebookID, noteID))
            {
                throw new NotFoundException();
            }

            return Write(
                "INSERT INTO OpenNotes SELECT @notebook_id, @note_id, IFNULL(MAX(sequence), 0) + 1 FROM OpenNotes",
                "@notebook_id", notebookID,
                "@note_id", noteID
            );
        }

        public override bool CloseNote(Int64 notebookID, Int64 noteID)
        {
            return Write(
                "DELETE FROM OpenNotes WHERE notebook_id=@notebook_id AND note_id=@note_id",
                "@notebook_id", notebookID,
                "@note_id", noteID
            );
        }

        #endregion

        #region Notes

        #region FindNotes

        private class FindNotesResultDataModel
        {
            [XmlElement("id")]
            public Int64 ID { get; set; }

            [XmlElement("text")]
            public string Text { get; set; }

            [XmlElement("offsets")]
            public string Offsets { get; set; }
        }

        public override SearchResultsDataModel FindNotes(Int64 notebookID, string phrase, DateTime createdAfter, DateTime createdBefore, DateTime modifiedAfter, DateTime modifiedBefore, DateTime viewedAfter, DateTime viewedBefore, NoteSort sort, NoteOrder order, int offset, int limit, bool returnText)
        {
            var results = new SearchResultsDataModel
            {
                NotebookID = notebookID,
                SearchString = phrase,
                Offset = offset
            };

            if (limit == -1)
            {
                results.Total = CountNotes(notebookID, phrase, createdAfter, createdBefore, modifiedAfter, modifiedBefore, viewedAfter, viewedBefore);
                if (results.Total == -1)
                {
                    return null;
                }
            }

            if (limit != -1)
            {
                results.Results = SearchNotes(notebookID, phrase, createdAfter, createdBefore, modifiedAfter, modifiedBefore, viewedAfter, viewedBefore, sort, order, offset, limit, returnText);
            }
            else
            {
                results.Results = new SearchResultDataModel[0];
            }

            return results;
        }

        DbCommand BuildCountCommand(Int64 notebookID, string query, DateTime createdAfter, DateTime createdBefore, DateTime modifiedAfter, DateTime modifiedBefore, DateTime viewedAfter, DateTime viewedBefore)
        {
            var tables = new HashSet<string>();

            var tokens = TokenizeSearchString(query.TrimEnd('*'));
            string fullTextQuery = GetSearchPhraseSQL(tokens, tables);
            string categoriesQuery = GetSearchCategoriesSQL(tokens, tables);
            string filterQuery = GetSearchFilter(createdAfter, createdBefore, modifiedAfter, modifiedBefore, viewedAfter, viewedBefore, tables);
            string notebookQuery = GetSearchNotebookSQL(notebookID, tables);
            string joinsQuery = GetJoinSQL(tables);

            var expressions = new List<string>();
            expressions.Add(fullTextQuery);
            expressions.Add(joinsQuery);
            expressions.Add(categoriesQuery);
            expressions.Add(notebookQuery);
            expressions.Add(filterQuery);
            expressions.RemoveAll((expr) => String.IsNullOrWhiteSpace(expr));
            expressions = expressions.ConvertAll((expr) => "(" + expr + ")");
            string expressionsSQL = String.Join(" AND ", expressions);

            DbCommand command = null;

            try
            {
                command = Connection.CreateCommand();

                command.CommandText =
                    "SELECT COUNT(*) " +
                    "FROM " + String.Join(", ", tables) + " " +
                    "WHERE " + expressionsSQL;

                return command;
            }
            catch (Exception)
            {
                if (command != null)
                {
                    command.Dispose();
                }

                throw;
            }
        }

        private int CountNotes(long notebookID, string phrase, DateTime createdAfter, DateTime createdBefore, DateTime modifiedAfter, DateTime modifiedBefore, DateTime viewedAfter, DateTime viewedBefore)
        {
            int result;
            using (DbCommand dbCommand = BuildCountCommand(notebookID, phrase, createdAfter, createdBefore, modifiedAfter, modifiedBefore, viewedAfter, viewedBefore))
            {
                result = ReadInt32(dbCommand);
            }
            return result;
        }

        DbCommand BuildSearchCommand(Int64 notebookID, string query, DateTime createdAfter, DateTime createdBefore, DateTime modifiedAfter, DateTime modifiedBefore, DateTime viewedAfter, DateTime viewedBefore, NoteSort sort, NoteOrder order, int offset, int limit, bool returnText)
        {
            var tables = new HashSet<string>();

            var tokens = TokenizeSearchString(query.TrimEnd('*'));
            string fullTextQuery = GetSearchPhraseSQL(tokens, tables);
            string categoriesQuery = GetSearchCategoriesSQL(tokens, tables);
            string filterQuery = GetSearchFilter(createdAfter, createdBefore, modifiedAfter, modifiedBefore, viewedAfter, viewedBefore, tables);
            string notebookQuery = GetSearchNotebookSQL(notebookID, tables);
            string sortColumn = GetSortColumnName(sort, tables);

            string joinsQuery = GetJoinSQL(tables);

            var expressions = new List<string>();
            expressions.Add(fullTextQuery);
            expressions.Add(joinsQuery);
            expressions.Add(categoriesQuery);
            expressions.Add(notebookQuery);
            expressions.Add(filterQuery);
            expressions.RemoveAll((expr) => String.IsNullOrWhiteSpace(expr));
            expressions = expressions.ConvertAll((expr) => "(" + expr + ")");
            string expressionsSQL = String.Join(" AND ", expressions);

            DbCommand command = null;

            try
            {
                command = Connection.CreateCommand();

                if (!String.IsNullOrWhiteSpace(fullTextQuery))
                {
                    command.CommandText =
                        "SELECT docid AS id, text " +
                        "FROM FullTextSearch JOIN (" +
                            "SELECT docid, " + sortColumn + " as rank " +
                            "FROM " + String.Join(", ", tables) + " " +
                            "WHERE " + expressionsSQL + " " +
                            "ORDER BY rank " + GetOrderType(order) + " " +
                            "LIMIT @limit OFFSET @offset " +
                        ") AS results USING(docid) " +
                        "WHERE " + fullTextQuery + " " +
                        "ORDER BY results.rank " + GetOrderType(order);
                }
                else if (returnText)
                {
                    command.CommandText =
                        "SELECT docid as id, text " +
                        "FROM FullTextSearch JOIN (" +
                            "SELECT Notes.id as docid, " + sortColumn + " as rank " +
                            "FROM " + String.Join(", ", tables) + " " +
                            "WHERE " + expressionsSQL + " " +
                            "ORDER BY rank " + GetOrderType(order) + " " +
                            "LIMIT @limit OFFSET @offset " +
                        ") AS results USING(docid) " +
                        "ORDER BY results.rank " + GetOrderType(order);
                }
                else
                {
                    command.CommandText =
						"SELECT Notes.id, Notes.title, " + sortColumn + " as rank " + 
                        "FROM " + String.Join(", ", tables) + " " +
						"WHERE " + expressionsSQL + " " +
						"ORDER BY rank " + GetOrderType(order) + " " +
						"LIMIT @limit OFFSET @offset";
                }

                DbParameter limitParameter = command.CreateParameter();
                limitParameter.ParameterName = "@limit";
                limitParameter.Value = limit;
                command.Parameters.Add(limitParameter);

                DbParameter offsetParameter = command.CreateParameter();
                offsetParameter.ParameterName = "@offset";
                offsetParameter.Value = offset;
                command.Parameters.Add(offsetParameter);

                return command;
            }
            catch (Exception)
            {
                if (command != null)
                {
                    command.Dispose();
                }

                throw;
            }
        }

        private SearchResultDataModel[] SearchNotes(long notebookID, string query, DateTime createdAfter, DateTime createdBefore, DateTime modifiedAfter, DateTime modifiedBefore, DateTime viewedAfter, DateTime viewedBefore, NoteSort sort, NoteOrder order, int offset, int limit, bool returnText)
        {
            using (DbCommand command = BuildSearchCommand(notebookID, query, createdAfter, createdBefore, modifiedAfter, modifiedBefore, viewedAfter, viewedBefore, sort, order, offset, limit, returnText))
            {
                if (returnText)
                {
                    var results = ReadObjects<NoteDbStore.FindNotesResultDataModel>(command);
                    if (results != null)
                    {
                        return (
                            from result in results
                            select new SearchResultDataModel
                            {
                                Text = result.Text,
                                Offsets = (result.Offsets != null) ? SearchResultOffsetDataModel.FromString(result.Offsets, 1) : null,
                                Note = ReadObject<NoteDataModel>("SELECT id, title FROM Notes WHERE id=@id", new object[]
								{
									"@id",
									result.ID.ToString()
								})
                            }).ToArray();
                    }
                }
                else
                {
                    var results = ReadObjects<NoteDataModel>(command);
                    if (results != null)
                    {
                        return (
                            from note in results
                            select new SearchResultDataModel
                            {
                                Note = note
                            }).ToArray();
                    }
                }
            }
            return null;
        }

        private static string GetSortColumnName(NoteSort sort, HashSet<string> tables)
        {
            switch (sort)
            {
                case NoteSort.ModifiedAt:
                    tables.Add("Notes");
                    return "Notes.modified_at";
                case NoteSort.CreatedAt:
                    tables.Add("Notes");
                    return "Notes.created_at";
                case NoteSort.Title:
                    tables.Add("Notes");
                    return "Notes.title";
                default:
                    tables.Add("Notes");
                    return "Notes.viewed_at";
            }
        }

        private static string GetOrderType(NoteOrder order)
        {
            switch (order)
            {
                case NoteOrder.Ascending:
                    return "ASC";
                default:
                    return "DESC";
            }
        }

        const string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";

        static string GetSearchFilter(DateTime createdAfter, DateTime createdBefore, DateTime modifiedAfter, DateTime modifiedBefore, DateTime viewedAfter, DateTime viewedBefore, HashSet<string> tables)
        {
            var results = new List<string>();

            if (createdAfter != default(DateTime) && createdAfter != DateTime.MinValue)
            {
                results.Add(String.Format("Notes.created_at > '{0}'", createdAfter.ToString(DATETIME_FORMAT)));
                tables.Add("Notes");
            }

            if (createdBefore != default(DateTime) && createdBefore != DateTime.MaxValue)
            {
                results.Add(String.Format("Notes.created_at < '{0}'", createdBefore.ToString(DATETIME_FORMAT)));
                tables.Add("Notes");
            }

            if (modifiedAfter != default(DateTime) && modifiedAfter != DateTime.MinValue)
            {
                results.Add(String.Format("Notes.modified_at > '{0}'", modifiedAfter.ToString(DATETIME_FORMAT)));
                tables.Add("Notes");
            }

            if (modifiedBefore != default(DateTime) && modifiedBefore != DateTime.MaxValue)
            {
                results.Add(String.Format("Notes.modified_at < '{0}'", modifiedBefore.ToString(DATETIME_FORMAT)));
                tables.Add("Notes");
            }

            if (viewedAfter != default(DateTime) && viewedAfter != DateTime.MinValue)
            {
                results.Add(String.Format("Notes.viewed_at > '{0}'", viewedAfter.ToString(DATETIME_FORMAT)));
                tables.Add("Notes");
            }

            if (viewedBefore != default(DateTime) && viewedBefore != DateTime.MaxValue)
            {
                results.Add(String.Format("Notes.viewed_at < '{0}'", viewedBefore.ToString(DATETIME_FORMAT)));
                tables.Add("Notes");
            }

            return String.Join(" AND ", results);
        }

        /// <summary>
        /// Tokenize a search string.
        /// 
        /// "+" is converted to "AND", and "-" is converted to "NOT"
        /// </summary>
        /// <param name="phrase">String to be tokenized</param>
        /// <returns>a list of tokens</returns>
        static IList<string> TokenizeSearchString(string phrase)
        {
            // search := [ "(" ] [ unary-operator WS ] ( term ) (WS [ binary-operator WS ] [ unary-operator WS ] term )* [ ")" ]
            // binary-operator  := "AND" | "OR" | "+"
            // unary-operator   := "NOT" | "-"
            // term             := token | quoted-string

            var tokens = new List<string>();

            bool inQuotes = false;
            var token = new StringBuilder();
            for (int i = 0; i < phrase.Length; i++)
            {
                char c = phrase[i];

                if (c == '\"')
                {
                    inQuotes = !inQuotes;
                }
                else if (inQuotes)
                {
                    token.Append(c);
                }
                else if (c == '+')
                {
                    tokens.Add(token.ToString());
                    token.Clear();
                    tokens.Add("AND");
                }
                else if (c == '-')
                {
                    tokens.Add(token.ToString());
                    token.Clear();
                    tokens.Add("NOT");
                }
                else if (c == '(' || c == ')')
                {
                    tokens.Add(token.ToString());
                    token.Clear();
                    tokens.Add(c.ToString());
                }
                else if (c == ':')
                {
                    tokens.Add(token.ToString() + ":");
                    token.Clear();
                }
                else if (Char.IsWhiteSpace(c))
                {
                    tokens.Add(token.ToString());
                    token.Clear();
                }
                else
                {
                    token.Append(c);
                }
            }

            tokens.Add(token.ToString());

            // Remove all empty tokens

            tokens.RemoveAll((item) => String.IsNullOrWhiteSpace(item));

            return tokens;
        }

        /// <summary>
        /// Extract search phrase tokens.
        /// </summary>
        /// <param name="tokens">Tokens to be parsed</param>
        /// <returns>Tokens belonging to a search phrase</returns>
        static IList<string> ParseSearchPhrase(IList<string> tokens)
        {
            var result = new List<string>();

            for (int i = 0; i < tokens.Count; i++)
            {
                switch (tokens[i].ToLower())
                {
                    case "except":
                        result.Add("AND");
                        result.Add("NOT");
                        break;
                    case "category:":
                    case "categoryid:":
                    case "tag:":
                        i++;
                        break;
                    default:
                        result.Add(tokens[i]);
                        break;
                }
            }

            return NormalizeSearchString(result);
        }

        /// <summary>
        /// Extract category filter tokens
        /// </summary>
        /// <param name="tokens">Tokens to be parsed</param>
        /// <returns>Tokens belonging to the category filter</returns>
        static IList<string> ParseSearchCategories(IList<string> tokens)
        {
            var result = new List<string>();

            for (int i = 0; i < tokens.Count; i++)
            {
                switch (tokens[i].ToLower())
                {
                    case "(":
                    case ")":
                    case "and":
                    case "or":
                    case "not":
                        result.Add(tokens[i]);
                        break;
                    case "except":
                        result.Add("AND");
                        result.Add("NOT");
                        break;
                    case "category:":
                    case "categoryid:":
                    case "tag:":
                        result.Add(tokens[i]);
                        if (++i < tokens.Count)
                        {
                            result.Add(tokens[i]);
                        }
                        break;
                    default:
                        break;
                }
            }

            return NormalizeSearchString(result);
        }

        /// <summary>
        /// Convert a set of tokens into a valid query expression
        /// </summary>
        /// <param name="tokens">Tokens to be normalized</param>
        /// <returns>Normalized set of tokens</returns>
        static IList<string> NormalizeSearchString(IList<string> tokens)
        {
            var result = new List<string>(tokens);

            for (int i = 0; i < result.Count; i++)
            {
                switch (result[i].ToLower())
                {
                    case ")":
                        // Remove empty ()'s
                        if (i > 0 && result[i - 1] == "(")
                        {
                            result.RemoveAt(i);
                            result.RemoveAt(i - 1);
                            i -= 2;
                        }
                        // Remove operators with no operands
                        else if (i > 0 && (result[i - 1].ToLower() == "and" || result[i - 1].ToLower() == "or" || result[i - 1].ToLower() == "not"))
                        {
                            result.RemoveAt(i - 1);
                            i--;
                        }
                        break;
                    case "and":
                    case "or":
                        // Remove redundant "and"s and "or"s
                        if (i > 0 && (result[i - 1].ToLower() == "and" || result[i - 1].ToLower() == "or"))
                        {
                            result.RemoveAt(i - 1);
                            i--;
                        }
                        break;
                    case "not":
                        // Remove redundant "not"s
                        if (i > 0 && (result[i - 1].ToLower() == "not"))
                        {
                            result.RemoveAt(i - 1);
                            i--;
                        }
                        break;
                    case "tag:":
                    case "category:":
                    case "categoryid:":
                        // Must not be followed by "(" or ")"
                        if (i + 1 < result.Count)
                        {
                            string arg = result[++i];
                            if (arg == "(" || arg == ")")
                            {
                                result.RemoveAt(i-1);
                                i--;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            // Balance parentheses

            int openingParens = result.Count((token) => token == "(");
            int closingParens = result.Count((token) => token == ")");

            while (openingParens > closingParens)
            {
                result.Add(")");
                closingParens++;
            }

            while (closingParens > openingParens)
            {
                result.Insert(0, "(");
                openingParens++;
            }

            return result;
        }

        /// <summary>
        /// Format an SQL expression based on a set of tokens
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        static string GetSearchPhraseSQL(IList<string> tokens, HashSet<string> tables)
        {
            tokens = ParseSearchPhrase(tokens);
            tokens = (from token in tokens where token != "(" && token != ")" select token).ToList();

            if (tokens.Count == 2 && tokens[0].ToLower() == "title:")
            {
                tables.Add("FullTextSearch");

                return String.Format("FullTextSearch.title=\'{0}\'", SQLEscape(tokens[1]));
            }

            var buffer = new StringBuilder();

            bool hasOperators = false;
            string binaryOperator = String.Empty;
            bool not = false;
            bool firstArgument = true;

            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens[i].ToLower();

                if (token == "and" || token == "or")
                {
                    binaryOperator = token;
                    not = false;
                    continue;
                }

                if (token == "not")
                {
                    not = true;
                    continue;
                }

                string argument = null;

                if (token == "title:")
                {
                    if (++i < tokens.Count)
                    {
                        argument = "title:" + tokens[i];
                        hasOperators = true;
                    }
                }
                else if (token == "content:")
                {
                    if (++i < tokens.Count)
                    {
                        argument = "text:" + tokens[i];
                        hasOperators = true;
                    }
                }
                else if (token == "near" || token.StartsWith("near/"))
                {
                    argument = tokens[i];
                    hasOperators = true;
                }
                else
                {
                    argument = tokens[i];
                }

                if (argument != null)
                {
                    if (!firstArgument && !String.IsNullOrEmpty(binaryOperator))
                    {
                        if (binaryOperator.ToLower() != "and")
                        {
                            buffer.Append(' ');
                            buffer.Append(binaryOperator.ToUpper());
                        }
                        hasOperators = true;
                    }

                    buffer.Append(' ');
                    if (not)
                    {
                        buffer.Append('-');
                        hasOperators = true;
                    }
                    buffer.Append(argument);
                    if (i == tokens.Count - 1 && !not && !buffer.ToString().EndsWith("\""))
                    {
                        buffer.Append('*');
                    }

                    binaryOperator = String.Empty;
                    not = false;
                    firstArgument = false;
                }
            }

            string result = buffer.ToString().TrimStart();

            if (result.Length == 0)
            {
                return String.Empty;
            }

            if (!hasOperators)
            {
                result = "\"" + result + "\"";
            }

            tables.Add("FullTextSearch");

            return String.Format("FullTextSearch MATCH \'{0}\'", SQLEscape(result));
        }

        /// <summary>
        /// Format an SQL expression based on a set of tokens
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        static string GetSearchCategoriesSQL(IList<string> tokens, HashSet<string> tables)
        {
            tokens = ParseSearchCategories(tokens);

            var buffer = new StringBuilder();

            bool needOperator = false;
            string binaryOperator = String.Empty;
            bool not = false;

            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens[i].ToLower();

                if (token == "and" || token == "or")
                {
                    binaryOperator = token;
                    not = false;
                    continue;
                }

                if (token == "not")
                {
                    not = true;
                    continue;
                }

                if (token == ")")
                {
                    buffer.Append(')');
                    needOperator = true;
                    binaryOperator = String.Empty;
                    not = false;
                    continue;
                }

                string argument = null;

                if (token == "(")
                {
                    argument = token;
                }
                else if (token == "category:" || token == "tag:")
                {
                    if (++i < tokens.Count)
                    {
                        string name = SQLEscape(tokens[i]);

                        if (name.Equals("Uncategorized", StringComparison.OrdinalIgnoreCase))
                        {
                            if (not)
                            {
                                argument = "(SELECT COUNT(*) FROM NoteCategories WHERE note_id=Notes.id) > 0";
                            }
                            else
                            {
                                argument = "(SELECT COUNT(*) FROM NoteCategories WHERE note_id=Notes.id) = 0";
                            }

                            tables.Add("Notes");
                        }
                        else
                        {
                            string categoryIDQuery;

                            if (i == tokens.Count - 1)
                            {
                                categoryIDQuery = String.Format("SELECT id FROM Categories WHERE name LIKE \'{0}%\'", name);
                            }
                            else
                            {
                                categoryIDQuery = String.Format("SELECT id FROM Categories WHERE name=\'{0}\'", name);
                            }

                            if (not)
                            {
                                argument = String.Format("Notes.id NOT IN (SELECT note_id FROM NoteCategories WHERE category_id IN ({0}))", categoryIDQuery);
                            }
                            else
                            {
                                argument = String.Format("Notes.id IN (SELECT note_id FROM NoteCategories WHERE category_id IN ({0}))", categoryIDQuery);
                            }

                            tables.Add("Notes");
                        }
                    }
                }
                else if (token == "categoryid:")
                {
                    if (++i < tokens.Count)
                    {
                        Int64 id;
                        if (Int64.TryParse(tokens[i], out id))
                        {
                            if (id == 0)
                            {
                                // All notes

                                if (not)
                                {
                                    argument = "0";
                                }
                                else
                                {
                                    argument = "1";
                                }
                            }
                            else if (id == 1)
                            {
                                // Uncategorized

                                if (not)
                                {
                                    argument = "(SELECT COUNT(*) FROM NoteCategories WHERE note_id=Notes.id) > 0";
                                }
                                else
                                {
                                    argument = "(SELECT COUNT(*) FROM NoteCategories WHERE note_id=Notes.id) = 0";
                                }
                                tables.Add("Notes");
                            }
                            else
                            {
                                // Specific category

                                if (not)
                                {
                                    argument = String.Format("Notes.id NOT IN (SELECT note_id FROM NoteCategories WHERE category_id={0})", id);
                                }
                                else
                                {
                                    argument = String.Format("Notes.id IN (SELECT note_id FROM NoteCategories WHERE category_id={0})", id);
                                }
                                tables.Add("Notes");
                            }
                        }
                    }
                }

                if (argument != null)
                {
                    if (needOperator)
                    {
                        if (String.IsNullOrEmpty(binaryOperator))
                        {
                            binaryOperator = "AND";
                        }

                        buffer.Append(' ');
                        buffer.Append(binaryOperator.ToUpper());
                    }

                    buffer.Append(' ');
                    buffer.Append(argument);

                    binaryOperator = String.Empty;
                    not = false;
                    needOperator = (argument != "(");
                }
            }

            return buffer.ToString().Trim();
        }

        static string GetSearchNotebookSQL(Int64 notebookID, HashSet<string> tables)
        {
            tables.Add("Notes");
            return String.Format("Notes.notebook_id={0}", notebookID);
        }

        static string GetJoinSQL(HashSet<string> tables)
        {
            var joins = new List<string>();

            if (tables.Contains("FullTextSearch"))
            {
                if (tables.Contains("Notes"))
                {
                    joins.Add("Notes.id=FullTextSearch.docid");
                }
                if (tables.Contains("NoteCategories"))
                {
                    joins.Add("NoteCategories.note_id=FullTextSearch.docid");
                }
            }
            else if (tables.Contains("Notes") && tables.Contains("NoteCategories"))
            {
                joins.Add("NoteCategories.note_id=Notes.id");
            }

            return String.Join(" AND ", joins);
        }

        static string SQLEscape(string sql)
        {
            return sql.Replace("\'", "\'\'");
        }

        #endregion

        public override NoteDataModel[] GetNotes(Int64 notebookID)
        {
            return ReadObjects<NoteDataModel>(
                "SELECT id, title, is_deleted, hash, last_sent_hash, last_recv_hash FROM Notes WHERE notebook_id=@notebook_id",
                "@notebook_id", notebookID
            );
        }

        public override NoteDataModel[] GetNotesMetadata(Int64 notebookID)
        {
            return ReadObjects<NoteDataModel>(
                "SELECT id, hash, is_deleted FROM Notes WHERE notebook_id=@notebook_id",
                "@notebook_id", notebookID
            );
        }

        public override bool ContainsNote(Int64 notebookID, Int64 noteID)
        {
            return ReadInt32(
                "SELECT COUNT(*) FROM Notes WHERE id=@id AND notebook_id=@notebook_id",
                "@id", noteID,
                "@notebook_id", notebookID
            ) > 0;
        }

        public override int GetNoteCount(long notebookID)
        {
            return ReadInt32(
                "SELECT COUNT(*) FROM Notes WHERE notebook_id=@notebook_id AND is_deleted=0",
                "@notebook_id", notebookID
            );
        }

        public override Int64 CreateNote(Int64 notebookID, NoteDataModel note)
        {
            if (note.ID == 0)
            {
                throw new ArgumentException("0 is not a valid ID", "note");
            }

            if (!Write(
                "INSERT INTO Notes (id, notebook_id, is_deleted) VALUES (@id, @notebook_id, @is_deleted)",
                "@id", note.ID,
                "@notebook_id", notebookID,
                "@is_deleted", note.IsDeleted
            )) return 0;

            if (!note.IsDeleted)
            {
                if (!SetNote(notebookID, note.ID, note))
                {
                    return 0;
                }

                if (!IndexNote(notebookID, note.ID))
                {
                    return 0;
                }
            }

            return note.ID;
        }

        public override NoteDataModel GetNote(Int64 notebookID, Int64 noteID)
        {
            NoteDataModel result;

            try
            {
                result = ReadObject<NoteDataModel>(
                    "SELECT * FROM Notes WHERE id=@id AND notebook_id=@notebook_id",
                    "@id", noteID,
                    "@notebook_id", notebookID
                );
            }
            catch (NotFoundException)
            {
                result = null;
            }

            if (result != null)
            {
                result.Categories = GetNoteCategories(notebookID, noteID);
                result.Text = GetNoteText(notebookID, noteID);
            }

            return result;
        }

        public override bool SetNote(Int64 notebookID, Int64 noteID, NoteDataModel note)
        {
            string hash = null;

            if (!String.IsNullOrEmpty(note.Content))
            {
                hash = Hash(note.Content);
            }

            if (note.LastSentHash == "*")
            {
                note.LastSentHash = hash;
            }

            if (!Write(
                "UPDATE Notes SET title=@title, content=@content, categories_modified_at=@categories_modified_at, created_at=@created_at, modified_at=@modified_at, viewed_at=@viewed_at, is_deleted=@is_deleted, hash=@hash, last_sent_hash=@last_sent_hash, last_recv_hash=@last_recv_hash WHERE id=@id AND notebook_id=@notebook_id",
                "@title", note.Title,
                "@content", note.Content,
                "@categories_modified_at", note.CategoriesModifiedAt,
                "@created_at", note.CreatedAt,
                "@modified_at", note.ModifiedAt,
                "@viewed_at", note.ViewedAt,
                "@is_deleted", note.IsDeleted,
                "@hash", hash,
                "@last_sent_hash", note.LastSentHash,
                "@last_recv_hash", note.LastRecvHash,
                "@id", noteID,
                "@notebook_id", notebookID
            )) return false;

            if (note.Categories != null)
            {
                if (!SetNoteCategories(notebookID, noteID, note.Categories))
                {
                    return false;
                }
            }

            if (note.Text != null)
            {
                if (!SetNoteText(notebookID, noteID, note.Text))
                {
                    return false;
                }
            }

            if (note.Title != null)
            {
                if (!Write(
                    "UPDATE FullTextSearch SET title=@title WHERE rowid = @id",
                    "@title", note.Title,
                    "@id", note.ID
                )) return false;
            }

            return true;
        }

        public override bool PurgeNote(long notebookID, long noteID)
        {
            return Write(
                "DELETE FROM Notes WHERE id=@id AND notebook_id=@notebook_id",
                "@id", noteID,
                "@notebook_id", notebookID
            );
        }

        public override NoteDataModel GetNoteMetadata(Int64 notebookID, Int64 noteID)
        {
            return ReadObject<NoteDataModel>(
                "SELECT id, hash, is_deleted FROM Notes WHERE id=@id AND notebook_id=@notebook_id",
                "@id", noteID,
                "@notebook_id", notebookID
            );
        }

        public override string GetNoteTitle(long notebookID, long noteID)
        {
            try
            {
                return ReadString(
                    "SELECT title FROM Notes WHERE id=@id AND notebook_id=@notebook_id",
                    "@id", noteID,
                    "@notebook_id", notebookID
                );
            }
            catch (NotFoundException)
            {
                return null;
            }
        }

        public override string GetNoteContent(long notebookID, long noteID)
        {
            try
            {
                return ReadString(
                    "SELECT content FROM Notes WHERE id=@id AND notebook_id=@notebook_id",
                    "@id", noteID,
                    "@notebook_id", notebookID
                );
            }
            catch (NotFoundException)
            {
                return null;
            }
        }

        public override string GetNoteText(long notebookID, long noteID)
        {
            try
            {
                return ReadString(
                    "SELECT text FROM FullTextSearch WHERE rowid=@id",
                    "@id", noteID
                );
            }
            catch (NotFoundException)
            {
                return null;
            }
        }

        public override bool SetNoteText(long notebookID, long noteID, string newText)
        {
            bool success = DoSetNoteText(notebookID, noteID, newText);

            // FullTextSearch may have become out of sync with the Notes table.
            //
            // Automatically add row to FullTextSearch as appropriate.

            if (!success)
            {
                if (ContainsNote(notebookID, noteID))
                {
                    IndexNote(notebookID, noteID);

                    success = DoSetNoteText(notebookID, noteID, newText);
                }
            }

            return success;
        }

        private bool DoSetNoteText(Int64 notebookID, Int64 noteID, string newText)
        {
            return Write(
                "UPDATE FullTextSearch SET text=@text WHERE rowid = @id",
                "@text", newText,
                "@id", noteID
            );
        }

        private bool IndexNote(Int64 notebookID, Int64 noteID)
        {
            return Write(
                "INSERT INTO FullTextSearch(docid) VALUES(@id)",
                "@id", noteID
            );
        }

        public override DateTime GetNoteCreatedAt(long notebookID, long noteID)
        {
            return ReadDateTime(
                "SELECT created_at FROM Notes WHERE id=@id AND notebook_id=@notebook_id",
                "@id", noteID,
                "@notebook_id", notebookID
            );
        }

        public override DateTime GetNoteModifiedAt(long notebookID, long noteID)
        {
            return ReadDateTime(
                "SELECT modified_at FROM Notes WHERE id=@id AND notebook_id=@notebook_id",
                "@id", noteID,
                "@notebook_id", notebookID
            );
        }

        public override DateTime GetNoteViewedAt(long notebookID, long noteID)
        {
            return ReadDateTime(
                "SELECT viewed_at FROM Notes WHERE id=@id AND notebook_id=@notebook_id",
                "@id", noteID,
                "@notebook_id", notebookID
            );
        }

        public override bool SetNoteViewedAt(long notebookID, long noteID, DateTime viewedAt)
        {
            return Write(
                "UPDATE Notes SET viewed_at=@viewed_at WHERE id=@id AND notebook_id=@notebook_id",
                "@viewed_at", viewedAt,
                "@id", noteID,
                "@notebook_id", notebookID
            );
        }

        public override CategoryDataModel[] GetNoteCategories(Int64 notebookID, Int64 noteID)
        {
            return ReadObjects<CategoryDataModel>(
                "SELECT category_id as id FROM NoteCategories WHERE note_id=@note_id",
                "@note_id", noteID
            );
        }

        public override bool SetNoteCategories(Int64 notebookID, Int64 noteID, CategoryDataModel[] categories)
        {
            bool success = true;

            var oldCategories = GetNoteCategories(notebookID, noteID);
            if (oldCategories == null)
            {
                return false;
            }

            var newCategories = categories;
            if (categories == null)
            {
                newCategories = new CategoryDataModel[] { };
            }

            var removeCategories = oldCategories.Except(newCategories, DataModelComparer.Instance);
            foreach (CategoryDataModel category in removeCategories)
            {
                success &= RemoveNoteCategory(notebookID, noteID, category.ID);
            }

            var addCategories = newCategories.Except(oldCategories, DataModelComparer.Instance);
            foreach (CategoryDataModel category in addCategories)
            {
                success &= AddNoteCategory(notebookID, noteID, category.ID);
            }

            return true;
        }

        public override bool AddNoteCategory(Int64 notebookID, Int64 noteID, Int64 categoryID)
        {
            return Write(
                "INSERT INTO NoteCategories (note_id, category_id) VALUES (@note_id, @category_id)",
                "@note_id", noteID,
                "@category_id", categoryID
            );
        }

        public override bool RemoveNoteCategory(Int64 notebookID, Int64 noteID, Int64 categoryID)
        {
            return Write(
                "DELETE FROM NoteCategories WHERE note_id=@note_id AND category_id=@category_id",
                "@note_id", noteID,
                "@category_id", categoryID
            );
        }

        #endregion

        #region Files

        public override FileDataModel[] GetFiles(Int64 notebookID, Int64 noteID)
        {
            return ReadObjects<FileDataModel>(
                "SELECT * FROM Files WHERE note_id=@note_id AND notebook_id=@notebook_id",
                "@note_id", noteID,
                "@notebook_id", notebookID
            );
        }

        public override FileDataModel[] GetFilesMetadata(Int64 notebookID, Int64 noteID)
        {
            return ReadObjects<FileDataModel>(
                "SELECT name FROM Files WHERE note_id=@note_id AND notebook_id=@notebook_id",
                "@note_id", noteID,
                "@notebook_id", notebookID
            );
        }

        public override bool ContainsFile(Int64 notebookID, Int64 noteID, string fileName)
        {
            return ReadInt32(
                "SELECT COUNT(*) FROM Files WHERE name=@name AND note_id=@note_id AND notebook_id=@notebook_id",
                "@name", fileName,
                "@note_id", noteID,
                "@notebook_id", notebookID
            ) > 0;
        }

        public override string CreateFile(Int64 notebookID, Int64 noteID, FileDataModel file)
        {
            if (file.Name == null)
            {
                throw new ArgumentException("null is not a valid ID", "file");
            }

            if (!Write(
                "INSERT INTO Files (name, note_id, notebook_id) VALUES (@name, @note_id, @notebook_id)",
                "@name", file.Name,
                "@note_id", noteID,
                "@notebook_id", notebookID
            )) return null;

            if (!SetFile(notebookID, noteID, file.Name, file))
            {
                return null;
            }

            return file.Name;
        }

        public override bool PurgeFile(Int64 notebookID, Int64 noteID, string fileName)
        {
            return Write(
                "DELETE FROM Files WHERE name=@name AND note_id=@note_id AND notebook_id=@notebook_id",
                "@name", fileName,
                "@note_id", noteID,
                "@notebook_id", notebookID
            );
        }

        public override FileDataModel GetFile(Int64 notebookID, Int64 noteID, string fileName)
        {
            return ReadObject<FileDataModel>(
                "SELECT * FROM Files WHERE name=@name AND note_id=@note_id AND notebook_id=@notebook_id",
                "@name", fileName,
                "@note_id", noteID,
                "@notebook_id", notebookID
            );
        }

        public override bool SetFile(Int64 notebookID, Int64 noteID, string fileName, FileDataModel file)
        {
            if (file.Name == null)
            {
                throw new ArgumentException("null is not a valid ID", "file");
            }

            return Write(
                "UPDATE Files SET type=@type, data=@data WHERE name=@name AND note_id=@note_id AND notebook_id=@notebook_id",
                "@type", file.Type,
                "@data", file.Data,
                "@name", file.Name,
                "@note_id", noteID,
                "@notebook_id", notebookID
            );
        }

        #endregion

        #region Categories

        public override CategoryDataModel[] GetCategories(Int64 notebookID)
        {
            return ReadObjects<CategoryDataModel>(
                "SELECT * FROM Categories WHERE notebook_id=@notebook_id",
                "@notebook_id", notebookID
            );
        }

        public override bool SetCategories(Int64 notebookID, CategoryDataModel[] newCategories)
        {
            var oldCategories = GetCategories(notebookID);
            if (oldCategories == null)
            {
                return false;
            }

            if (newCategories == null)
            {
                newCategories = new CategoryDataModel[] { };
            }

            var purgeCategories = oldCategories.Except(newCategories, DataModelComparer.Instance);
            foreach (CategoryDataModel category in purgeCategories)
            {
                PurgeCategory(notebookID, category.ID);
            }

            var createCategories = newCategories.Except(oldCategories, DataModelComparer.Instance);
            foreach (CategoryDataModel category in createCategories)
            {
                CreateCategory(notebookID, category);
            }

            foreach (var newCategory in newCategories)
            {
                var oldCategory = oldCategories.FirstOrDefault((category) => category.ID == newCategory.ID);
                if (oldCategory != null)
                {
                    if (!newCategory.Equals(oldCategory))
                    {
                        SetCategory(notebookID, newCategory.ID, newCategory);
                    }
                }
            }

            return true;
        }

        public override Int64 CreateCategory(Int64 notebookID, CategoryDataModel category)
        {
            if (category.ID == 0)
            {
                throw new ArgumentException("0 is not a valid ID", "category");
            }

            if (!Write(
                "INSERT INTO Categories (id, notebook_id, parent_id, parent_id_modified_at, name, name_modified_at, is_deleted) VALUES (@id, @notebook_id, @parent_id, @parent_id_modified_at, @name, @name_modified_at, @is_deleted)",
                "@id", category.ID,
                "@notebook_id", notebookID,
                "@parent_id", category.ParentID,
                "@parent_id_modified_at", category.ParentIDModifiedAt,
                "@name", category.Name,
                "@name_modified_at", category.NameModifiedAt,
                "@is_deleted", category.IsDeleted
            )) return 0;

            return category.ID;
        }

        public override bool PurgeCategory(Int64 notebookID, Int64 categoryID)
        {
            PurgeCategoryChildren(notebookID, categoryID);

            return Write(
                "DELETE FROM Categories WHERE id=@id AND notebook_id=@notebook_id",
                "@id", categoryID,
                "@notebook_id", notebookID
            );
        }

        private void PurgeCategoryChildren(Int64 notebookID, Int64 categoryID)
        {
            var children = GetCategoryChildren(notebookID, categoryID);

            foreach (var child in children)
            {
                PurgeCategory(notebookID, child.ID);
            }
        }

        public override CategoryDataModel GetCategory(Int64 notebookID, Int64 categoryID)
        {
            var category = ReadObject<CategoryDataModel>(
                "SELECT * FROM Categories WHERE id=@id AND notebook_id=@notebook_id",
                "@id", categoryID,
                "@notebook_id", notebookID
            );

            if (category != null)
            {
                category.Children = GetCategoryChildren(notebookID, categoryID);
            }

            return category;
        }

        public override bool SetCategory(Int64 notebookID, Int64 categoryID, CategoryDataModel category)
        {
            return Write(
                "UPDATE Categories SET parent_id=@parent_id, parent_id_modified_at=@parent_id_modified_at, name=@name, name_modified_at=@name_modified_at, is_deleted=@is_deleted WHERE id=@id AND notebook_id=@notebook_id",
                "@parent_id", category.ParentID,
                "@parent_id_modified_at", category.ParentIDModifiedAt,
                "@name", category.Name,
                "@name_modified_at", category.NameModifiedAt,
                "@is_deleted", category.IsDeleted,
                "@id", categoryID,
                "@notebook_id", notebookID
            );
        }

        public override CategoryDataModel[] GetCategoryChildren(Int64 notebookID, Int64 categoryID)
        {
            return ReadObjects<CategoryDataModel>(
                "SELECT * FROM Categories WHERE parent_id=@parent_id AND notebook_id=@notebook_id",
                "@parent_id", categoryID,
                "@notebook_id", notebookID
            );
        }

        #endregion

        #region Clipart

        public override ClipartGroupDataModel[] GetClipartGroups()
        {
            var groups = ReadObjects<ClipartGroupDataModel>("SELECT * FROM ClipartGroups");

            if (groups == null)
            {
                groups = new ClipartGroupDataModel[0];
            }

            return groups;
        }

        public override Int64 CreateClipartGroup(ClipartGroupDataModel group)
        {
            if (group.ID == 0)
            {
                throw new ArgumentException("0 is not a valid ID", "group");
            }

            if (!Write(
                "INSERT INTO ClipartGroups (id, name, name_modified_at, is_deleted) VALUES (@id, @name, @name_modified_at, @is_deleted)",
                "@id", group.ID,
                "@name", group.Name,
                "@name_modified_at", group.NameModifiedAt,
                "@is_deleted", group.IsDeleted
            )) return 0;

            return group.ID;
        }

        public override bool PurgeClipartGroup(Int64 groupID)
        {
            return Write(
                "DELETE FROM ClipartGroups WHERE id=@id",
                "@id", groupID
            );
        }

        public override ClipartGroupDataModel GetClipartGroup(Int64 groupID)
        {
            var result = ReadObject<ClipartGroupDataModel>(
                "SELECT * FROM ClipartGroups WHERE id=@id",
                "@id", groupID
            );

            if (result != null)
            {
                result.Items = GetClipartItems(groupID);
            }

            return result;
        }

        public override bool SetClipartGroup(Int64 groupID, ClipartGroupDataModel group)
        {
            return Write(
                "UPDATE ClipartGroups SET name=@name, name_modified_at=@name_modified_at, is_deleted=@is_deleted WHERE id=@id",
                "@name", group.Name,
                "@name_modified_at", group.NameModifiedAt,
                "@is_deleted", group.IsDeleted,
                "@id", groupID
            );
        }

        public override ClipartDataModel[] GetClipartItems(Int64 groupID)
        {
            return ReadObjects<ClipartDataModel>(
                "SELECT id, name, is_deleted, hash, last_sent_hash, last_recv_hash FROM Clipart WHERE group_id=@group_id",
                "@group_id", groupID
            );
        }

        public override ClipartDataModel[] GetClipartItemsMetadata(Int64 groupID)
        {
            return ReadObjects<ClipartDataModel>(
                "SELECT id, hash, is_deleted FROM Clipart WHERE group_id=@group_id",
                "@group_id", groupID
            );
        }

        public override Int64 CreateClipart(Int64 groupID, ClipartDataModel clipart)
        {
            if (clipart.ID == 0)
            {
                throw new ArgumentException("0 is not a valid ID", "clipart");
            }

            if (!Write(
                "INSERT INTO Clipart (id, group_id) VALUES (@id, @group_id)",
                "@id", clipart.ID,
                "@group_id", groupID
            )) return 0;

            if (!SetClipart(groupID, clipart.ID, clipart))
            {
                return 0;
            }

            return clipart.ID;
        }

        public override bool PurgeClipart(long groupID, long clipartID)
        {
            return Write(
                "DELETE FROM Clipart WHERE id=@id AND group_id=@group_id",
                "@id", clipartID,
                "@group_id", groupID
            );
        }

        public override ClipartDataModel GetClipart(Int64 groupID, Int64 clipartID)
        {
            return ReadObject<ClipartDataModel>(
                "SELECT * FROM Clipart WHERE id=@id AND group_id=@group_id",
                "@id", clipartID,
                "@group_id", groupID
            );
        }

        public override bool SetClipart(Int64 groupID, Int64 clipartID, ClipartDataModel clipart)
        {
            string hash = "";
            if (clipart.Data != null)
            {
                hash = Hash(clipart.Data);
            }

            string lastSentHash = clipart.LastSentHash;
            if (clipart.LastSentHash == "*")
            {
                lastSentHash = hash;
            }

            return Write(
                "UPDATE Clipart SET name=@name, name_modified_at=@name_modified_at, data=@data, data_modified_at=@data_modified_at, created_at=@created_at, modified_at=@modified_at, viewed_at=@viewed_at, is_deleted=@is_deleted, hash=@hash, last_sent_hash=@last_sent_hash, last_recv_hash=@last_recv_hash WHERE id=@id AND group_id=@group_id",
                "@name", clipart.Name,
                "@name_modified_at", clipart.NameModifiedAt,
                "@data", clipart.Data,
                "@data_modified_at", clipart.DataModifiedAt,
                "@created_at", clipart.CreatedAt,
                "@modified_at", clipart.ModifiedAt,
                "@viewed_at", clipart.ViewedAt,
                "@is_deleted", clipart.IsDeleted,
                "@hash", hash,
                "@last_sent_hash", lastSentHash,
                "@last_recv_hash", clipart.LastRecvHash,
                "@id", clipartID,
                "@group_id", groupID
            );
        }

        public override ClipartDataModel GetClipartMetadata(Int64 groupID, Int64 clipartID)
        {
            return ReadObject<ClipartDataModel>(
                "SELECT id, hash, last_sent_hash, last_recv_hash FROM Clipart WHERE id=@id AND group_id=@group_id",
                "@id", clipartID,
                "@group_id", groupID
            );
        }

        #endregion

        #region IDisposable

        private bool _IsDisposed = false;

        public override void Dispose(bool disposing)
        {
            if (_IsDisposed)
            {
                return;
            }

            if (Connection != null)
            {
                Connection.Dispose();
            }

            _IsDisposed = true;
        }

        #endregion

        #region Implementation

        /// <summary>
        /// The underlying SQLite connection
        /// </summary>
        protected SQLiteConnection Connection { get; private set; }

        /// <summary>
        /// Initialize the underlying SQLite connection
        /// </summary>
        private void DoOpen(string filename, SecureString password = null, bool autoCreate = true)
        {
            // If the database doesn't exist and autoCreate = false, try to handle
            // that error here so we can throw a more descriptive exception.

            if (!autoCreate && !System.IO.File.Exists(filename))
            {
                string message = String.Format("The following file does not exist:\n\n{0}", filename);
                throw new NotFoundException(message);
            }

            string connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = filename,
                FailIfMissing = !autoCreate,
                DateTimeKind = DateTimeKind.Utc
            }.ToString();

            Debug.WriteLine("Opening " + connectionString);

            Connection = new SQLiteConnection(connectionString);

            try
            {
                if (password != null && password.Length > 0)
                {
                    // Obfuscate the password's representation in memory
                    string key = SecureStringToHashedString(password);
                    Connection.SetPassword(key);
                }
                Connection.Open();

                // Since initialization occurs lazily, we need to attempt to
                // actually read something in order to be sure we have a valid
                // (and properly decrypted) database
                ReadInt32("pragma schema_version;");
            }
            catch (Exception e)
            {
                Connection.Dispose();
                Connection = null;
                DoOpenFailed(filename, password, autoCreate, e);
            }
        }

        /// <summary>
        /// Handle an initialization error
        /// </summary>
        private static void DoOpenFailed(string filename, SecureString password, bool autoCreate, Exception e)
        {
            if (e is SQLiteException)
            {
                if (((SQLiteException)e).ResultCode == SQLiteErrorCode.NotADb)
                {
                    // Unable to decrypt an encrypted database

                    // Here, we throw an UnauthorizedException, which NoteServer will translate
                    // to an UNAUTHORIZED Response object, which the UI will handle by displaying
                    // a password dialog displaying the enclosed message.

                    string message;
                    if (password == null || password.Length == 0)
                    {
                        // A password was not entered
                        message = "Attempting to access encrypted data...";
                    }
                    else
                    {
                        // An incorrect password was entered
                        message = String.Format("The password you provided is not valid for the following file:\n\n\"{0}\"", filename);
                    }

                    throw new UnauthorizedException(message);
                }
            }

            throw e;
        }


        static string SecureStringToHashedString(SecureString secureString)
        {
            if (secureString == null)
            {
                return "";
            }

            byte[] plainBytes = new byte[secureString.Length * 2];

            IntPtr bstr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(secureString);
            try
            {
                System.Runtime.InteropServices.Marshal.Copy(bstr, plainBytes, 0, plainBytes.Length);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(bstr);
            }

            byte[] hashedBytes;
            try
            {
                hashedBytes = System.Security.Cryptography.SHA1.Create().ComputeHash(plainBytes);
            }
            finally
            {
                ZeroArray(plainBytes);
            }

            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }

        static void ZeroArray(byte[] a)
        {
            if (a != null)
            {
                for (int i = 0; i < a.Length; i++)
                {
                    a[i] = 0;
                }
            }
        }

        #endregion

    }
}
