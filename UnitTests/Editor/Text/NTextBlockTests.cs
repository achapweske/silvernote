/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilverNote.Editor;
using System.Windows;

namespace UnitTests.Editor.Text
{
    [TestClass]
    public class NTextBlockTests
    {
        [TestMethod]
        public void TestInsert()
        {
            var textBlock = new TextElement();
            Assert.AreEqual("", textBlock.Text);
            textBlock.Insert(0, "Foe");
            Assert.AreEqual("Foe", textBlock.Text);
            textBlock.Insert(0, "Fee");
            Assert.AreEqual("FeeFoe", textBlock.Text);
            textBlock.Insert(6, "Fum");
            Assert.AreEqual("FeeFoeFum", textBlock.Text);
            textBlock.Insert(3, "Fie");
            Assert.AreEqual("FeeFieFoeFum", textBlock.Text);
        }

        [TestMethod]
        public void TestInsert_Undo()
        {
            var textBlock = new TextElement();
            textBlock.UndoStack = new UndoStack();

            textBlock.Insert(0, "Foe");
            textBlock.Insert(0, "Fee");
            textBlock.Insert(6, "Fum");
            textBlock.Insert(3, "Fie");

            Assert.AreEqual("FeeFieFoeFum", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("FeeFoeFum", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("FeeFoe", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("Foe", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("", textBlock.Text);

            textBlock.UndoStack.Redo();
            Assert.AreEqual("Foe", textBlock.Text);
            textBlock.UndoStack.Redo();
            Assert.AreEqual("FeeFoe", textBlock.Text);
            textBlock.UndoStack.Redo();
            Assert.AreEqual("FeeFoeFum", textBlock.Text);
            textBlock.UndoStack.Redo();
            Assert.AreEqual("FeeFieFoeFum", textBlock.Text);
        }

        [TestMethod]
        public void TestDelete()
        {
            var textBlock = new TextElement();
            textBlock.Text = "FeeFieFoeFum";

            textBlock.Delete(0, 3);
            Assert.AreEqual("FieFoeFum", textBlock.Text);
            textBlock.Delete(3, 3);
            Assert.AreEqual("FieFum", textBlock.Text);
            textBlock.Delete(3, 3);
            Assert.AreEqual("Fie", textBlock.Text);
            textBlock.Delete(0, 3);
            Assert.AreEqual("", textBlock.Text);
        }

        [TestMethod]
        public void TestDelete_Undo()
        {
            var textBlock = new TextElement();
            textBlock.Text = "FeeFieFoeFum";
            textBlock.UndoStack = new UndoStack();

            textBlock.Delete(0, 3);
            textBlock.Delete(3, 3);
            textBlock.Delete(3, 3);
            textBlock.Delete(0, 3);

            Assert.AreEqual("", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("Fie", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("FieFum", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("FieFoeFum", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("FeeFieFoeFum", textBlock.Text);

            textBlock.UndoStack.Redo();
            Assert.AreEqual("FieFoeFum", textBlock.Text);
            textBlock.UndoStack.Redo();
            Assert.AreEqual("FieFum", textBlock.Text);
            textBlock.UndoStack.Redo();
            Assert.AreEqual("Fie", textBlock.Text);
            textBlock.UndoStack.Redo();
            Assert.AreEqual("", textBlock.Text);
        }

        [TestMethod]
        public void TestCut()
        {
            var textBlock = new TextElement();
            textBlock.Text = "FeeFieFoeFum";

            var cutted = textBlock.Cut(0, 3);
            Assert.AreEqual("Fee", cutted.Text);
            Assert.AreEqual("FieFoeFum", textBlock.Text);
            cutted = textBlock.Cut(3, 3);
            Assert.AreEqual("Foe", cutted.Text);
            Assert.AreEqual("FieFum", textBlock.Text);
            cutted = textBlock.Cut(3, 3);
            Assert.AreEqual("Fum", cutted.Text);
            Assert.AreEqual("Fie", textBlock.Text);
            cutted = textBlock.Cut(0, 3);
            Assert.AreEqual("Fie", cutted.Text);
            Assert.AreEqual("", textBlock.Text);
        }

        [TestMethod]
        public void TestCut_Undo()
        {
            var textBlock = new TextElement();
            textBlock.Text = "FeeFieFoeFum";
            textBlock.UndoStack = new UndoStack();

            textBlock.Cut(0, 3);
            textBlock.Cut(3, 3);
            textBlock.Cut(3, 3);
            textBlock.Cut(0, 3);

            Assert.AreEqual("", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("Fie", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("FieFum", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("FieFoeFum", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("FeeFieFoeFum", textBlock.Text);

            textBlock.UndoStack.Redo();
            Assert.AreEqual("FieFoeFum", textBlock.Text);
            textBlock.UndoStack.Redo();
            Assert.AreEqual("FieFum", textBlock.Text);
            textBlock.UndoStack.Redo();
            Assert.AreEqual("Fie", textBlock.Text);
            textBlock.UndoStack.Redo();
            Assert.AreEqual("", textBlock.Text);
        }

        [TestMethod]
        public void TestCopy()
        {
            var textBlock = new TextElement();
            textBlock.Text = "FeeFieFoeFum";

            var copied = textBlock.Copy(0, 3);
            Assert.AreEqual("Fee", copied.Text);
            copied = textBlock.Copy(3, 6);
            Assert.AreEqual("FieFoe", copied.Text);
            copied = textBlock.Copy(9, 3);
            Assert.AreEqual("Fum", copied.Text);
        }

        [TestMethod]
        public void TestPaste()
        {
            var textBlock = new TextElement();

            Assert.AreEqual("", textBlock.Text);
            textBlock.Paste(0, new TextBuffer { Text = "Foe" });
            Assert.AreEqual("Foe", textBlock.Text);
            textBlock.Paste(0, new TextBuffer { Text = "Fee" });
            Assert.AreEqual("FeeFoe", textBlock.Text);
            textBlock.Paste(6, new TextBuffer { Text = "Fum" });
            Assert.AreEqual("FeeFoeFum", textBlock.Text);
            textBlock.Paste(3, new TextBuffer { Text = "Fie" });
            Assert.AreEqual("FeeFieFoeFum", textBlock.Text);
        }

        [TestMethod]
        public void TestPaste_Undo()
        {
            var textBlock = new TextElement();
            textBlock.UndoStack = new UndoStack();

            textBlock.Paste(0, new TextBuffer { Text = "Foe" });
            textBlock.Paste(0, new TextBuffer { Text = "Fee" });
            textBlock.Paste(6, new TextBuffer { Text = "Fum" });
            textBlock.Paste(3, new TextBuffer { Text = "Fie" });

            Assert.AreEqual("FeeFieFoeFum", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("FeeFoeFum", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("FeeFoe", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("Foe", textBlock.Text);
            textBlock.UndoStack.Undo();
            Assert.AreEqual("", textBlock.Text);

            textBlock.UndoStack.Redo();
            Assert.AreEqual("Foe", textBlock.Text);
            textBlock.UndoStack.Redo();
            Assert.AreEqual("FeeFoe", textBlock.Text);
            textBlock.UndoStack.Redo();
            Assert.AreEqual("FeeFoeFum", textBlock.Text);
            textBlock.UndoStack.Redo();
            Assert.AreEqual("FeeFieFoeFum", textBlock.Text);
        }


    }
}
