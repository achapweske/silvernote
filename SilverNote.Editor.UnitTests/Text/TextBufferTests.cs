/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SilverNote.Editor.UnitTests.Text
{
    [TestClass]
    public class TextBufferTests
    {
        [TestMethod]
        public void TestInsert()
        {
            var buffer = new TextBuffer();
            object sender = null;
            TextChangedEventArgs args = null;
            buffer.TextChanged += (_sender, _args) =>
            {
                sender = _sender;
                args = _args;
            };

            string text = "This is a test.";
            buffer.Insert(0, text);
            
            Assert.AreEqual(text, buffer.Text);
            Assert.AreEqual(buffer, sender);
            Assert.AreEqual(text.Length, args.NumAdded);
            Assert.AreEqual(0, args.NumRemoved);
            Assert.AreEqual(0, args.Offset);
        }

        [TestMethod]
        public void TestInsert_Twice()
        {
            var buffer = new TextBuffer();
            object sender = null;
            TextChangedEventArgs args = null;
            buffer.TextChanged += (_sender, _args) =>
            {
                sender = _sender;
                args = _args;
            };

            string str1 = "This is a test.";
            buffer.Insert(0, str1);
            string str2 = "This is another test.";
            buffer.Insert(0, str2);

            Assert.AreEqual(str1 + str2, buffer.Text);
            Assert.AreEqual(buffer, sender);
            Assert.AreEqual(str2.Length, args.NumAdded);
            Assert.AreEqual(0, args.NumRemoved);
            Assert.AreEqual(str1.Length, args.Offset);
        }

        [TestMethod]
        public void TestDelete()
        {
            var buffer = new TextBuffer();
            object sender = null;
            TextChangedEventArgs args = null;
            buffer.TextChanged += (_sender, _args) =>
            {
                sender = _sender;
                args = _args;
            };

            string str = "This is a test.";
            buffer.Insert(0, str);
            buffer.Delete(0, 5);

            Assert.AreEqual(str.Remove(0, 5), buffer.Text);
            Assert.AreEqual(buffer, sender);
            Assert.AreEqual(0, args.NumAdded);
            Assert.AreEqual(5, args.NumRemoved);
            Assert.AreEqual(0, args.Offset);
        }

        [TestMethod]
        public void TestDelete_Middle()
        {
            var buffer = new TextBuffer();
            object sender = null;
            TextChangedEventArgs args = null;
            buffer.TextChanged += (_sender, _args) =>
            {
                sender = _sender;
                args = _args;
            };

            string str = "This is a test.";
            buffer.Insert(0, str);
            buffer.Delete(5, 3);

            Assert.AreEqual(str.Remove(5, 3), buffer.Text);
            Assert.AreEqual(buffer, sender);
            Assert.AreEqual(0, args.NumAdded);
            Assert.AreEqual(3, args.NumRemoved);
            Assert.AreEqual(5, args.Offset);
        }

        [TestMethod]
        public void TestDelete_End()
        {
            var buffer = new TextBuffer();
            object sender = null;
            TextChangedEventArgs args = null;
            buffer.TextChanged += (_sender, _args) =>
            {
                sender = _sender;
                args = _args;
            };

            string str = "This is a test.";
            buffer.Insert(0, str);
            buffer.Delete(str.Length - 6, 6);

            Assert.AreEqual(str.Remove(str.Length - 6, 6), buffer.Text);
            Assert.AreEqual(buffer, sender);
            Assert.AreEqual(0, args.NumAdded);
            Assert.AreEqual(6, args.NumRemoved);
            Assert.AreEqual(str.Length - 6, args.Offset);
        }

        [TestMethod]
        public void TestDelete_All()
        {
            var buffer = new TextBuffer();
            object sender = null;
            TextChangedEventArgs args = null;
            buffer.TextChanged += (_sender, _args) =>
            {
                sender = _sender;
                args = _args;
            };

            string str = "This is a test.";
            buffer.Insert(0, str);
            buffer.Delete(0, str.Length);

            Assert.AreEqual("", buffer.Text);
            Assert.AreEqual(buffer, sender);
            Assert.AreEqual(0, args.NumAdded);
            Assert.AreEqual(str.Length, args.NumRemoved);
            Assert.AreEqual(0, args.Offset);
        }
    }
}
