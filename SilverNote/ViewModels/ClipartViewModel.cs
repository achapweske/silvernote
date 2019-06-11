/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using SilverNote.Common;
using SilverNote.Models;
using SilverNote.Editor;

namespace SilverNote.ViewModels
{
    public class ClipartViewModel : ViewModelBase<ClipartModel, ClipartViewModel>
    {
        #region Static Methods

        public static ClipartViewModel FromDrawing(string name, Shape drawing)
        {
            if (drawing != null)
            {
                return FromSVG(name, NCanvas.ToSVG(drawing));
            }
            else
            {
                return FromSVG(name, String.Empty);
            }
        }

        public static ClipartViewModel FromSVG(string name, string svg)
        {
            var model = new ClipartModel(null, null, 0);
            model.Create(name, svg, false);
            return FromModel(model);
        }

        #endregion

        #region Fields

        HashSet<Shape> _DrawingCache = new HashSet<Shape>();

        #endregion

        #region Constructors

        protected override void OnInitialize()
        {
            Model.WhenPropertyChanged("Name", (i, j) => RaisePropertyChanged("DisplayName"));
            Model.WhenPropertyChanged("Data", Model_DataChanged);
        }

        #endregion

        #region Properties

        public RepositoryViewModel Repository
        {
            get { return RepositoryViewModel.FromModel(Model.Repository); }
        }

        public ClipartGroupViewModel ClipartGroup
        {
            get { return ClipartGroupViewModel.FromModel(Model.ClipartGroup); }
        }

        public string Name
        {
            get { return Model.Name; }
            set { Model.Name = value; }
        }

        public string DisplayName
        {
            get { return Model.Name.TrimStart("0123456789".ToArray()).Trim(); }
        }

        public string Data
        {
            get { return Model.Data; }
            set { Model.Data = value; }
        }

        /// <summary>
        /// Get a drawing representing this view model's SVG data
        /// 
        /// The returned object is a visual and so we must take care to
        /// not return a visual that is already in the visual tree
        /// (A visual can only appear in the visual tree once). However,
        /// we don't want to create a unique clone every time this is
        /// invoked because that causes a noticable delay in switching
        /// between notes. Therefore we maintain a cache of drawings,
        /// returning the first one not already in the visual tree.
        /// </summary>
        public Shape Drawing
        {
            get 
            {
                var drawing = _DrawingCache.FirstOrDefault(d => d.Parent == null);
                if (drawing == null)
                {
                    try
                    {
                        if (_DrawingCache.Count > 0)
                        {
                            drawing = (Shape)_DrawingCache.First().Clone();
                        }
                        else if (!String.IsNullOrEmpty(Data))
                        {
                            drawing = CreateDrawing(Data);
                        }
                        if (drawing != null)
                        {
                            _DrawingCache.Add(drawing);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error: " + e.Message + " (ClipartViewModel)");
                    }
                }
                return drawing;
            }
            set
            {
                var canvas = new NCanvas();
                canvas.Drawings.Add(value);
                Data = canvas.ToSVG();
                _DrawingCache.Add(value);
            }
        }

        public NCanvas Canvas
        {
            get
            {
                var drawing = Drawing;
                if (drawing != null)
                {
                    var canvas = new NCanvas();
                    canvas.Drawings.Add(drawing);
                    return canvas;
                }
                return null;
            }
        }

        #endregion

        #region Implementation

        private static Shape CreateDrawing(string data)
        {
            var canvas = NCanvas.FromSVG(data);
            if (canvas != null)
            {
                return canvas.LastOrDefault();
            }
            else
            {
                return null;
            }
        }

        private void Model_DataChanged(object sender, PropertyChangedEventArgs e)
        {
            _DrawingCache.Clear();
            RaisePropertyChanged("Drawing");
            RaisePropertyChanged("Canvas");
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
