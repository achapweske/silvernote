/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SilverNote.Editor
{
    public class FileDataConverter : IDataConverter
    {
        public string Format
        {
            get { return DataFormats.FileDrop; }
        }

        public void SetData(IDataObject obj, IList<object> items)
        {

        }

        public IList<object> GetData(IDataObject obj)
        {
            var filePaths = obj.GetData(DataFormats.FileDrop) as IEnumerable<string>;
            if (filePaths != null)
            {
                return filePaths.Select(filePath => new NFile(filePath)).ToArray();
            }
            else
            {
                return new object[0];
            }
        }
    }
}
