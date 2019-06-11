/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.SQLite;
using System.Security;
using System.Diagnostics;

namespace SilverNote
{
    public class SQLiteDatabase : Database
    {
        public SQLiteDatabase(string filePath, SecureString password = null, bool autoCreate = false)
        {
            Open(filePath, password, autoCreate);
        }

        public SQLiteDatabase()
        {

        }

        public override DbConnection CreateConnection(string connectionString, SecureString password)
        {
            return new SQLiteConnection(connectionString);
        }

        public bool Open(string filePath, SecureString password, bool autoCreate)
        {
            try
            {
                // This call brings into context System.Data.SQLite, and an
                // exception will be thrown if the assembly cannot be found
                return UnsafeOpen(filePath, password, autoCreate);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n\n" + e.StackTrace);
                return false;
            }
        }

        private bool UnsafeOpen(string filePath, SecureString password, bool autoCreate)
        {
            string connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = filePath,
                FailIfMissing = !autoCreate
            }.ToString();

            return Open(connectionString, password);
        }
    }
}
