/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Common;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Common
{
    [TestClass]
    public class DataUriTests
    {
        [TestMethod]
        public void TestParse_Simple()
        {
            string str = "data:,A%20brief%20note";
            DataUri result;
            
            Assert.IsTrue(DataUri.TryParse(str, out result));
            Assert.AreEqual("A brief note", result.Data);
        }

        [TestMethod]
        public void TestParse_Image()
        {
            string data = "R0lGODdhMAAwAPAAAAAAAP///ywAAAAAMAAwAAAC8IyPqcvt3wCcDkiLc7C0qwyGHhSWpjQu5yqmCYsapyuvUUlvONmOZtfzgFzByTB10QgxOR0TqBQejhRNzOfkVJ+5YiUqrXF5Y5lKh/DeuNcP5yLWGsEbtLiOSpa/TPg7JpJHxyendzWTBfX0cxOnKPjgBzi4diinWGdkF8kjdfnycQZXZeYGejmJlZeGl9i2icVqaNVailT6F5iJ90m6mvuTS4OK05M0vDk0Q4XUtwvKOzrcd3iq9uisF81M1OIcR7lEewwcLp7tuNNkM3uNna3F2JQFo97Vriy/Xl4/f1cf5VWzXyym7PHhhx4dbgYKAAA7";
            string str = "data:image/gif;base64," + data;
            DataUri result;

            Assert.IsTrue(DataUri.TryParse(str, out result));
            Assert.AreEqual("image", result.Type);
            Assert.AreEqual("gif", result.Subtype);
            Assert.AreEqual("base64", result.Encoding);
            Assert.AreEqual(data, result.Data);
        }

        [TestMethod]
        public void TestFormat_Simple()
        {
            DataUri uri = new DataUri
            {
                Data = "A brief note"
            };

            string expected = "data:,A%20brief%20note";
            string actual = uri.ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestFormat_Image()
        {
            string data = "R0lGODdhMAAwAPAAAAAAAP///ywAAAAAMAAwAAAC8IyPqcvt3wCcDkiLc7C0qwyGHhSWpjQu5yqmCYsapyuvUUlvONmOZtfzgFzByTB10QgxOR0TqBQejhRNzOfkVJ+5YiUqrXF5Y5lKh/DeuNcP5yLWGsEbtLiOSpa/TPg7JpJHxyendzWTBfX0cxOnKPjgBzi4diinWGdkF8kjdfnycQZXZeYGejmJlZeGl9i2icVqaNVailT6F5iJ90m6mvuTS4OK05M0vDk0Q4XUtwvKOzrcd3iq9uisF81M1OIcR7lEewwcLp7tuNNkM3uNna3F2JQFo97Vriy/Xl4/f1cf5VWzXyym7PHhhx4dbgYKAAA7";

            DataUri uri = new DataUri
            {
                Type = "image",
                Subtype = "gif",
                Encoding = "base64",
                Data = data
            };

            string expected = "data:image/gif;base64," + data;
            string actual = uri.ToString();

            Assert.AreEqual(expected, actual);
        }
    }
}
