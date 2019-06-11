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
using System.Security;
using System.Xml.Serialization;
using System.Diagnostics;

namespace SilverNote
{
    public abstract class Database : IDisposable
    {
        public DbConnection Connection { get; protected set; }

        public abstract DbConnection CreateConnection(string connectionString, SecureString password);

        public bool Open(string connectionString, SecureString password)
        {
            Debug.WriteLine("Opening " + connectionString);

            try
            {
                Connection = CreateConnection(connectionString, password);

                Connection.Open();

                return true;
            }
            catch (Exception e)
            {
                if (Connection != null)
                {
                    Connection.Dispose();
                    Connection = null;
                }

                Debug.WriteLine(e.Message);
                return false;
            }
        }

        public void Close()
        {
            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
                Connection = null;
            }
        }

        public bool IsOpen()
        {
            return (Connection != null);
        }

        public DbCommand CreateCommand(string sql, params object[] args)
        {
            DbCommand command = null;
            try
            {
                command = Connection.CreateCommand();

                command.CommandText = sql;

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

        public bool Write(string sql, params object[] args)
        {
            using (var command = CreateCommand(sql, args))
            {
                return command.ExecuteNonQuery() > 0;
            }
        }

        public object Read(string sql, params object[] args)
        {
            using (var command = CreateCommand(sql, args))
            {
                return command.ExecuteScalar();
            }
        }

        public object ReadOne(string sql, params object[] args)
        {
            object result = Read(sql, args);
            if (result != null)
            {
                return result;
            }
            else
            {
                throw new NotFoundException();
            }
        }

        public string ReadString(string sql, params object[] args)
        {
            object result = ReadOne(sql, args);
            if (result != DBNull.Value)
            {
                return Convert.ToString(result);
            }
            else
            {
                return default(string);
            }
        }

        public string ReadStringOrDefault(string sql, params object[] args)
        {
            try
            {
                return ReadString(sql, args);
            }
            catch
            {
                return default(string);
            }
        }

        public Int32 ReadInt32(string sql, params object[] args)
        {
            object result = ReadOne(sql, args);
            if (result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }
            else
            {
                return default(Int32);
            }
        }

        public Int32 ReadInt32OrDefault(string sql, params object[] args)
        {
            try
            {
                return ReadInt32(sql, args);
            }
            catch
            {
                return default(Int32);
            }
        }

        public Int64 ReadInt64(string sql, params object[] args)
        {
            object result = ReadOne(sql, args);
            if (result != DBNull.Value)
            {
                return Convert.ToInt64(result);
            }
            else
            {
                return default(Int64);
            }
        }

        public Int64 ReadInt64OrDefault(string sql, params object[] args)
        {
            try
            {
                return ReadInt64(sql, args);
            }
            catch
            {
                return default(Int64);
            }
        }

        public DateTime ReadDateTime(string sql, params object[] args)
        {
            object result = ReadOne(sql, args);
            if (result != DBNull.Value)
            {
                return Convert.ToDateTime(result);
            }
            else
            {
                return default(DateTime);
            }
        }

        public DateTime ReadDateTimeOrDefault(string sql, params object[] args)
        {
            try
            {
                return ReadDateTime(sql, args);
            }
            catch
            {
                return default(DateTime);
            }
        }

        public byte[] ReadBlob(string sql, params object[] args)
        {
            object result = ReadOne(sql, args);
            if (result != DBNull.Value)
            {
                return result as byte[];
            }
            else
            {
                return null;
            }
        }

        public byte[] ReadBlobOrDefault(string sql, params object[] args)
        {
            try
            {
                return ReadBlob(sql, args);
            }
            catch
            {
                return default(byte[]);
            }
        }

        private T[] ReadArray<T>(string sql, params object[] args)
        {
            using (DbCommand command = CreateCommand(sql, args))
            {
                return ReadArray<T>(command);
            }
        }

        private T[] ReadArray<T>(DbCommand command)
        {
            using (var reader = command.ExecuteReader())
            {
                List<T> result = new List<T>();

                while (reader.Read())
                {
                    T value = (T)Convert.ChangeType(reader.GetValue(0), typeof(T));

                    result.Add(value);
                }

                return result.ToArray();
            }
        }

        private T ReadObject<T>(string sql, params object[] args) where T : class, new()
        {
            using (var command = CreateCommand(sql, args))
            {
                return ReadObject<T>(command);
            }
        }

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

        private T[] ReadObjects<T>(string sql, params object[] args) where T : new()
        {
            using (DbCommand command = CreateCommand(sql, args))
            {
                return ReadObjects<T>(command);
            }
        }

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

        private void SetProperty(object target, string name, object value)
        {
            Type type = target.GetType();

            var properties = type.GetProperties();
            //var properties = type.GetFields();
            foreach (var property in properties)
            {
                object[] attributes = property.GetCustomAttributes(typeof(XmlAttributeAttribute), false);
                foreach (XmlAttributeAttribute attribute in attributes)
                {
                    if (attribute.AttributeName == name)
                    {
                        object convertedValue;
                        if (value != DBNull.Value)
                            convertedValue = Convert.ChangeType(value, property.PropertyType);
                        else
                            convertedValue = null;
                        property.SetValue(target, convertedValue, null);
                        return;
                    }
                }

                object[] elements = property.GetCustomAttributes(typeof(XmlElementAttribute), false);
                foreach (XmlElementAttribute element in elements)
                {
                    if (element.ElementName == name)
                    {
                        object convertedValue;
                        if (value != DBNull.Value)
                            convertedValue = Convert.ChangeType(value, property.PropertyType);
                        else
                            convertedValue = null;
                        property.SetValue(target, convertedValue, null);
                        return;
                    }
                }
            }
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (Connection != null)
                    {
                        Connection.Dispose();
                    }
                }
            }

            disposed = true;
        }

        #endregion

        public class NotFoundException : Exception
        {
            public NotFoundException()
            { }

            public NotFoundException(string message)
                : base(message)
            { }

            public NotFoundException(string message, Exception innerException)
                : base(message, innerException)
            { }
        }
    }
}
