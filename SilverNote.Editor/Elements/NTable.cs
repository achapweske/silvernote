/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml;
using System.Diagnostics;
using DOM;
using DOM.HTML;
using DOM.CSS;
using SilverNote.Commands;

namespace SilverNote.Editor
{
    /// <summary>
    /// Tables are rendered in ? stages:
    /// 
    /// 1) The table's structure is described by the Rows collection, which 
    ///    contains a set of NTableRows, eaching containing a set of NTableCells.
    ///    Each NTableCell specifies the number of rows and columns that cell spans.
    ///    
    /// 2) Based on this description, the Grid member is initialized to contain
    ///    a 2-D grid of cells, where each entry in that grid contains a single cell.
    ///    Empty cells are automatically added to the Rows structure such that
    ///    Grid contains no null values.
    ///    
    /// 3) ...
    /// 
    /// http://www.whatwg.org/specs/web-apps/current-work/multipage/tabular-data.html#table-model
    /// </summary>
    public class NTable : DocumentElement, IEnumerable<NTableRow>, IEditable, ISearchable, ITextElement, ICloneable
    {
        #region Fields

        NTableRowGroup _Head;
        NTableRowGroup _Body;
        NTableRowGroup _Foot;
        NTableCell[,] _Grid;

        #endregion

        #region Constructors

        public NTable()
        {
            Initialize();
        }

        public NTable(int rowCount, int columnCount)
            : this()
        {
            for (int i = 0; i < rowCount; i++)
            {
                NTableRow row = Body.CreateRow();

                for (int j = 0; j < columnCount; j++)
                {
                    NTableCell cell = row.CreateCell(true);
                    row.Cells.Add(cell);
                }

                Rows.Add(row);
            }

            InvalidateGrid(true);
        }

        public NTable(NTable other)
            : base(other)
        {
            Initialize();

            Title = (NTableTitle)other.Title.Clone();

            foreach (NTableRow row in other.Rows)
            {
                var clone = (NTableRow)row.Clone();
                if (row.Group == other.Head)
                    clone.Group = this.Head;
                if (row.Group == other.Body)
                    clone.Group = this.Body;
                if (row.Group == other.Foot)
                    clone.Group = this.Foot;
                Rows.Add(clone);
            }

            foreach (NTableColumn column in other.Columns)
            {
                var clone = (NTableColumn)column.Clone();
                clone.Table = this;
                Columns.Add(clone);
            }

            InvalidateGrid(true);
        }

        void Initialize()
        {
            _Visuals = new VisualCollection(this);
            _CellsVisual = new ContainerVisual();
            _Visuals.Add(_CellsVisual);
            _BordersDrawing = new DrawingVisual();
            _Visuals.Add(_BordersDrawing);
            _Selection = new Selection<Visual>();
            _Selection.SelectionChanged += Selection_Changed;

            _Head = new NTableRowGroup(this, HTMLElements.THEAD);
            _Body = new NTableRowGroup(this, HTMLElements.TBODY);
            _Foot = new NTableRowGroup(this, HTMLElements.TFOOT);

            Title = new NTableTitle();
            Title.Visibility = Visibility.Hidden;
            _Grid = new NTableCell[0, 0];
            Rows = new List<NTableRow>();
            Columns = new List<NTableColumn>();

            SnapsToDevicePixels = true;
            Cursor = Cursors.Arrow;
            Focusable = true;
            FocusVisualStyle = null;
            SelectionAdorner = new TableAdorner(this);

            LoadCommandBindings();
        }

        #endregion

        #region Properties

        #region UndoStack

        new public static readonly DependencyProperty UndoStackProperty = DocumentElement.UndoStackProperty.AddOwner(
            typeof(NTable),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, UndoStackProperty_PropertyChanged));

        static void UndoStackProperty_PropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            NTable table = (NTable)sender;
            table.Title.UndoStack = (UndoStack)e.NewValue;
            foreach (var cell in table.Cells)
            {
                cell.UndoStack = (UndoStack)e.NewValue;
            }
        }

        #endregion

        #region Background

        public static readonly new DependencyProperty BackgroundProperty = DocumentElement.BackgroundProperty.AddOwner(
            typeof(NTable),
            new FrameworkPropertyMetadata(Brushes.Transparent)
        );

        #endregion

        #region SelectionBackground

        public static readonly DependencyProperty SelectionBackgroundProperty = DependencyProperty.Register(
            "SelectionBackground",
            typeof(Brush),
            typeof(NTable),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectionBackgroundProperty_Changed)
        );

        private static void SelectionBackgroundProperty_Changed(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NTable table = (NTable)target;
            table.ApplySelectionBackground();
        }

        public Brush SelectionBackground
        {
            get { return (Brush)GetValue(SelectionBackgroundProperty); }
            set { SetValue(SelectionBackgroundProperty, value); }
        }

        public void ApplySelectionBackground()
        {
            Brush newBackground = SelectionBackground;

            if (newBackground == null)
            {
                newBackground = Brushes.Transparent;
            }

            Format(() =>
            {
                foreach (NTableCell cell in SelectedCells)
                {
                    cell.Background = newBackground;
                }
            });
        }

        #endregion

        #region SelectionBorderColor

        public static readonly DependencyProperty SelectionBorderColorProperty = DependencyProperty.Register(
            "SelectionBorderColor",
            typeof(Color),
            typeof(NTable),
            new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSelectionBorderColorChanged))
        );

        private static void OnSelectionBorderColorChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NTable table = (NTable)target;
            table.ApplySelectionBorderColor();
        }

        public Color SelectionBorderColor
        {
            get { return (Color)GetValue(SelectionBorderColorProperty); }
            set { SetValue(SelectionBorderColorProperty, value); }
        }

        public void ApplySelectionBorderColor()
        {
            Color newBorderColor = SelectionBorderColor;

            if (newBorderColor == null)
            {
                newBorderColor = Colors.Transparent;
            }

            BorderColor = newBorderColor;

            foreach (NTableCell cell in Cells)
            {
                cell.BorderColor = newBorderColor;
            }

            InvalidateVisual();
        }

        #endregion

        #region SelectionBorderWidth

        public static readonly DependencyProperty SelectionBorderWidthProperty = DependencyProperty.Register(
            "SelectionBorderWidth",
            typeof(double),
            typeof(NTable),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSelectionBorderWidthChanged))
        );

        private static void OnSelectionBorderWidthChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NTable table = (NTable)target;
            table.ApplySelectionBorderWidth();
        }

        public double SelectionBorderWidth
        {
            get { return (double)GetValue(SelectionBorderWidthProperty); }
            set { SetValue(SelectionBorderWidthProperty, value); }
        }

        public void ApplySelectionBorderWidth()
        {
            double newBorderWidth = SelectionBorderWidth;

            BorderWidth = newBorderWidth;

            foreach (NTableCell cell in Cells)
            {
                cell.BorderWidth = newBorderWidth;
            }

            InvalidateVisual();
        }

        #endregion

        #endregion

        #region Title

        private NTableTitle _Title = null;

        public NTableTitle Title 
        {
            get 
            { 
                return _Title; 
            }
            set
            {
                var oldValue = _Title;
                var newValue = value;

                if (newValue != oldValue)
                {
                    _Title = newValue;
                    OnTitleChanged(oldValue, newValue);
                }
            }
        }

        protected void OnTitleChanged(NTableTitle oldTitle, NTableTitle newTitle)
        {
            if (oldTitle != null)
            {
                _Visuals.Remove(oldTitle);
            }

            if (newTitle != null)
            {
                _Visuals.Add(newTitle);
            }
        }

        #endregion

        #region Cells

        /// <summary>
        /// Enumerate through all cells in this table.
        /// 
        /// Each cell is returned exactly once.
        /// </summary>
        public IEnumerable<NTableCell> Cells
        {
            get
            {
                foreach (var row in Rows)
                {
                    foreach (var cell in row)
                    {
                        yield return cell;
                    }
                }
            }
        }

        /// <summary>
        /// Get the table grid.
        /// 
        /// Grid[i,j] returns the cell at the ith row and jth column of this table.
        /// 
        /// Note that a cell can span multiple rows and/or columns.
        /// </summary>
        public NTableCell[,] Grid
        {
            get
            {
                UpdateGrid();
                return _Grid;
            }
        }

        /// <summary>
        /// Build the table grid based on the Rows collection.
        /// 
        /// Cells are automatically added to the Rows collection such that the final grid contains no null values.
        /// 
        /// This method also updates each cell's RowIndex and ColumnIndex properties.
        /// </summary>
        protected void UpdateGrid()
        {
            if (_Grid == null)
            {
                // Build the table grid based on our Rows collection

                _Grid = BuildGrid(Rows);

                // Add cells such that Grid contains no null values

                Normalize();

                // Update our _Visuals collection

                UpdateVisuals();

                // Trigger a new layout pass

                if (IsMeasureValid)
                {
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Build the table grid based on a set of NTableRows
        /// </summary>
        private static NTableCell[,] BuildGrid(IList<NTableRow> rows)
        {
            var grid = new Common.Array2D<NTableCell>(rows.Count);

            for (int i = 0; i < rows.Count; i++)
            {
                int j = NextGridColumn(grid, i, 0);

                foreach (NTableCell cell in rows[i].Cells)
                {
                    AddCellToGrid(grid, i, j, cell);

                    j = NextGridColumn(grid, i, j);
                }
            }

            return grid.ToArray();
        }

        /// <summary>
        /// Add a cell to the given grid.
        /// 
        /// This method sets the cell's RowIndex and ColumnIndex.
        /// </summary>
        /// <param name="grid">The grid to be updated</param>
        /// <param name="rowIndex">The first row to which the cell is to be added</param>
        /// <param name="columnIndex">The first column to which the cell is to be added</param>
        /// <param name="cell">The cell to be added</param>
        private static void AddCellToGrid(Common.Array2D<NTableCell> grid, int rowIndex, int columnIndex, NTableCell cell)
        {
            cell.RowIndex = rowIndex;
            cell.ColumnIndex = columnIndex;

            for (int i = 0; i < cell.ColumnSpan; i++)
            {
                for (int j = 0; j < cell.RowSpan; j++)
                {
                    grid.SetValue(rowIndex + j, columnIndex, cell);
                }

                columnIndex = NextGridColumn(grid, rowIndex, columnIndex);
            }
        }

        /// <summary>
        /// Get the next available column within the given grid
        /// </summary>
        private static int NextGridColumn(Common.Array2D<NTableCell> grid, int rowIndex, int columnIndex)
        {
            while (grid.GetValue(rowIndex, columnIndex) != null)
            {
                columnIndex++;
            }

            return columnIndex;
        }

        /// <summary>
        /// Update Rows such that the given grid has no empty slots
        /// </summary>
        private void Normalize()
        {
            // Add rows

            while (Rows.Count < Grid.GetLength(0))
            {
                NTableRow row;

                if (Rows.Count > 0)
                {
                    row = Rows.Last().Group.CreateRow();
                }
                else
                {
                    row = Body.CreateRow();
                }

                row.IsAutoGenerated = true;
                Rows.Add(row);
            }

            // Add cells

            for (int i = 0; i < Grid.GetLength(0); i++)
            {
                for (int j = 0; j < Grid.GetLength(1); j++)
                {
                    if (Grid[i, j] == null)
                    {
                        var row = Rows[i];
                        var cell = row.CreateCell(true);
                        cell.RowIndex = i;
                        cell.ColumnIndex = j;
                        row.Cells.Add(cell);
                        Grid[i, j] = cell;
                    }
                }
            }

            // Update columns

            Columns.Clear();

            for (int j = 0; j < Grid.GetLength(1); j++)
            {
                NTableColumn column = new NTableColumn(this);

                for (int i = 0; i < Grid.GetLength(0); i++)
                {
                    NTableCell cell = Grid[i, j];

                    if (cell.ColumnIndex == j)
                    {
                        column.Cells.Add(cell);
                    }
                }

                column.IsAutoGenerated = true;
                Columns.Add(column);
            }
        }

        protected void InvalidateGrid(bool updateNow = false)
        {
            _Grid = null;

            if (updateNow)
            {
                UpdateGrid();
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateGrid();
                }));
            }
        }

        private void UpdateVisuals()
        {
            NTableCell[] oldCells = _CellsVisual.Children.OfType<NTableCell>().ToArray();
            NTableCell[] newCells = Cells.ToArray();

            var removedCells = oldCells.Except(newCells);

            foreach (var cell in removedCells)
            {
                Selection.Unselect(cell);
                _CellsVisual.Children.Remove(cell);
            }

            var addedCells = newCells.Except(oldCells);

            foreach (var cell in addedCells)
            {
                _CellsVisual.Children.Add(cell);
            }
        }

        #endregion

        #region Sections

        /// <summary>
        /// Get the table head row group
        /// </summary>
        public NTableRowGroup Head
        {
            get { return _Head; }
        }

        /// <summary>
        /// Get the table body row group
        /// </summary>
        public NTableRowGroup Body
        {
            get { return _Body; }
        }

        /// <summary>
        /// Get the table foot row group
        /// </summary>
        public NTableRowGroup Foot
        {
            get { return _Foot; }
        }

        #endregion

        #region Rows

        private List<NTableRow> Rows { get; set; }

        /// <summary>
        /// Get the number of rows in this table
        /// </summary>
        public int RowCount
        {
            get { return Grid.GetLength(0); }
        }

        /// <summary>
        /// Get the row at the given index
        /// </summary>
        /// <param name="rowIndex">Index of the row to be retrieved</param>
        /// <returns>The retrieved row</returns>
        public NTableRow GetRow(int rowIndex)
        {
            return Rows[rowIndex];
        }

        /// <summary>
        /// Insert a row at the given row index.
        /// </summary>
        /// <param name="rowIndex">The index at which the row will be inserted</param>
        /// <param name="row">Row to be inserted</param>
        public void InsertRow(int rowIndex, NTableRow row)
        {
            if (UndoStack != null)
            {
                UndoStack.Push(() => RemoveRow(rowIndex));
            }

            Rows.Insert(rowIndex, row);

            InvalidateGrid();
        }

        /// <summary>
        /// Add a row to the end of this table
        /// </summary>
        /// <param name="row"></param>
        public void AppendRow(NTableRow newRow)
        {
            if (newRow.Group == Foot || newRow.Group == null)
            {
                InsertRow(Rows.Count, newRow);
                return;
            }

            NTableRow insertAfter;

            if (newRow.Group == Body)
            {
                insertAfter = Body.Rows.LastOrDefault();

                if (insertAfter != null)
                {
                    int index = Rows.IndexOf(insertAfter);
                    InsertRow(index + 1, newRow);
                    return;
                }
            }

            insertAfter = Head.Rows.LastOrDefault();

            if (insertAfter != null)
            {
                int index = Rows.IndexOf(insertAfter);
                InsertRow(index + 1, newRow);
                return;
            }

            InsertRow(Rows.Count, newRow);
        }

        /// <summary>
        /// Remove the row at the given index.
        /// </summary>
        /// <param name="rowIndex">Index of the row to be removed</param>
        public void RemoveRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= Rows.Count)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (UndoStack != null)
            {
                var row = Rows[rowIndex];
                UndoStack.Push(() => InsertRow(rowIndex, row));
            }

            Rows.RemoveAt(rowIndex);

            InvalidateGrid(true);
        }

        /// <summary>
        /// Remove the given row from this table
        /// </summary>
        /// <param name="row">The row to be removed</param>
        public void RemoveRow(NTableRow row)
        {
            int rowIndex = Rows.IndexOf(row);

            if (rowIndex != -1)
            {
                RemoveRow(rowIndex);
            }
        }

        /// <summary>
        /// Add all cells in the given row to the current selection
        /// </summary>
        /// <param name="rowIndex">Index of the row to be selected</param>
        public void SelectRow(int rowIndex)
        {
            var row = GetRow(rowIndex);

            Selection.SelectRange(row.Cells);

            foreach (var cell in row.Cells)
            {
                cell.MoveToStart();
                cell.SelectToEnd();
            }
        }

        /// <summary>
        /// Select all cells in the given row.
        /// 
        /// All other cells are un-selected.
        /// </summary>
        /// <param name="rowIndex">Index of the row to be selected</param>
        public void SelectRowOnly(int rowIndex)
        {
            var row = GetRow(rowIndex);

            Selection.SelectRangeOnly(row.Cells);

            foreach (var cell in row.Cells)
            {
                cell.MoveToStart();
                cell.SelectToEnd();
            }
        }

        #endregion

        #region Columns

        private List<NTableColumn> Columns { get; set; }

        /// <summary>
        /// Get the number of columns in this table
        /// </summary>
        public int ColumnCount
        {
            get { return Grid.GetLength(1); }
        }

        /// <summary>
        /// Get the column at the given index
        /// </summary>
        /// <param name="columnIndex">Index of the column to be retrieved</param>
        /// <returns>The retrieved column</returns>
        public NTableColumn GetColumn(int columnIndex)
        {
            return Columns[columnIndex];
        }

        /// <summary>
        /// Insert a column at the given column index.
        /// </summary>
        /// <param name="columnIndex">The index at which the column will be inserted</param>
        /// <param name="column">Column to be inserted</param>
        public void InsertColumn(int columnIndex, NTableColumn newColumn)
        {
            if (columnIndex < 0 || columnIndex > Grid.GetLength(1))
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            if (columnIndex == Grid.GetLength(1))
            {
                AppendColumn(newColumn);
                return;
            }

            // Insert a cell into each row at the given columnIndex

            int rowIndex = 0;

            foreach (NTableCell cell in newColumn.Cells)
            {
                NTableCell insertBefore = Grid[rowIndex, columnIndex];

                // Do not insert into the middle of cells that span multiple rows/columns

                if (insertBefore.ColumnIndex == columnIndex && insertBefore.RowIndex == rowIndex)
                {
                    NTableRow row = Rows[rowIndex];
                    int index = row.Cells.IndexOf(insertBefore);
                    row.Cells.Insert(index, cell);
                }

                rowIndex += cell.RowSpan;
            }

            InvalidateGrid(true);
        }

        /// <summary>
        /// Add a new (empty) column to the end of this table 
        /// </summary>
        public void AppendColumn(NTableColumn newColumn)
        {
            // Append a cell to the end of each row

            int rowIndex = 0;

            foreach (NTableCell cell in newColumn.Cells)
            {
                NTableRow row = Rows[rowIndex];
                row.Cells.Add(cell);
                rowIndex += cell.RowSpan;
            }

            InvalidateGrid(true);
        }

        /// <summary>
        /// Remove the column at the given index
        /// </summary>
        /// <param name="columnIndex">The index of the column to be removed</param>
        public void RemoveColumn(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= Grid.GetLength(1))
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            // Remove a cell from each row at the given columnIndex

            int rowIndex = 0;

            while (rowIndex < Grid.GetLength(0))
            {
                NTableCell cell = Grid[rowIndex, columnIndex];

                if (cell.ColumnSpan > 1)
                {
                    cell.ColumnSpan--;
                }
                else
                {
                    cell.Row.Cells.Remove(cell);
                }

                rowIndex += cell.RowSpan;
            }

            InvalidateGrid(true);
        }

        /// <summary>
        /// Add all cells in the given column to the current selection
        /// </summary>
        /// <param name="columnIndex">Index of the column to be selected</param>
        public void SelectColumn(int columnIndex)
        {
            var column = GetColumn(columnIndex);

            Selection.SelectRange(column.Cells);

            foreach (var cell in column.Cells)
            {
                cell.MoveToStart();
                cell.SelectToEnd();
            }
        }

        /// <summary>
        /// Select all cells in the given column.
        /// 
        /// All other cells are un-selected.
        /// </summary>
        /// <param name="columnIndex">Index of the column to be selected</param>
        public void SelectColumnOnly(int columnIndex)
        {
            var column = GetColumn(columnIndex);

            Selection.SelectRangeOnly(column.Cells);

            foreach (var cell in column.Cells)
            {
                cell.MoveToStart();
                cell.SelectToEnd();
            }
        }

        #endregion

        #region Selection

        private Selection<Visual> _Selection;

        public Selection<Visual> Selection
        {
            get
            {
                _Selection.UndoStack = this.UndoStack;

                return _Selection;
            }
        }

        /// <summary>
        /// Get the set of all currently-selected cells.
        /// </summary>
        public IEnumerable<NTableCell> SelectedCells
        {
            get { return Selection.OfType<NTableCell>(); }
        }

        /// <summary>
        /// Get the set of all currently-selected ITextElement objects.
        /// </summary>
        public IEnumerable<ITextElement> TextSelection
        {
            get { return Selection.OfType<ITextElement>(); }
        }

        /// <summary>
        /// Select all cells in the given range
        /// </summary>
        /// <param name="firstCell">The first cell to be selected</param>
        /// <param name="lastCell">The last cell to be selected</param>
        public void SelectSpan(NTableCell firstCell, NTableCell lastCell)
        {
            int firstRow = firstCell.RowIndex;
            int firstColumn = firstCell.ColumnIndex;

            int lastRow = lastCell.RowIndex;
            int lastColumn = lastCell.ColumnIndex;

            int rowTick = (lastRow > firstRow) ? 1 : -1;
            int columnTick = (lastColumn > firstColumn) ? 1 : -1;

            bool forward = (lastRow > firstRow) || (lastRow == firstRow && lastColumn > firstColumn);

            var selection = new LinkedList<NTableCell>();

            int rowIndex;
            for (rowIndex = firstRow; 
                 rowIndex != lastRow + rowTick; 
                 rowIndex += rowTick)
            {
                int columnIndex;
                for (columnIndex = firstColumn; 
                     columnIndex != lastColumn + columnTick; 
                     columnIndex += columnTick)
                {
                    if (rowIndex == firstRow && firstRow > lastRow && columnIndex > firstColumn ||
                        rowIndex == lastRow && firstRow > lastRow && columnIndex < lastColumn)
                    {
                        continue;
                    }

                    var cell = Grid[rowIndex, columnIndex];

                    if (cell.RowIndex != rowIndex || cell.ColumnIndex != columnIndex)
                    {
                        continue;
                    }

                    selection.AddLast(cell);

                    if (rowIndex == firstRow && columnIndex == firstColumn)
                    {
                        if (forward)
                            cell.SelectToEnd();
                        else
                            cell.SelectToStart();
                    }
                    else if (forward)
                    {
                        cell.MoveToStart();
                        cell.SelectToEnd();
                    }
                    else
                    {
                        cell.MoveToEnd();
                        cell.SelectToStart();
                    }
                }
            }

            Selection.SelectRangeOnly(selection);
        }

        public void SelectTo(NTableCell cell)
        {
            var target = SelectedCells.FirstOrDefault();

            if (target != null)
            {
                SelectSpan(target, cell);
            }
            else
            {
                Selection.Select(cell);
            }
        }

        public void SelectAll()
        {
            Selection.Select(Title);

            foreach (var row in Rows)
            {
                foreach (var cell in row.Cells)
                {
                    Selection.Select(cell);
                }
            }
        }

        public int TopMostSelectedRow
        {
            get
            {
                if (SelectedCells.Count() == 0)
                {
                    return -1;
                }

                int result = RowCount - 1;

                foreach (var cell in SelectedCells)
                {
                    result = Math.Min(result, cell.RowIndex);
                }

                return result;
            }
        }

        public int BottomMostSelectedRow
        {
            get
            {
                if (SelectedCells.Count() == 0)
                {
                    return -1;
                }

                int result = 0;

                foreach (var cell in SelectedCells)
                {
                    result = Math.Max(result, cell.RowIndex + cell.RowSpan - 1);
                }

                return result;
            }
        }

        public int LeftMostSelectedColumn
        {
            get
            {
                if (SelectedCells.Count() == 0)
                {
                    return -1;
                }

                int result = ColumnCount - 1;

                foreach (var cell in SelectedCells)
                {
                    result = Math.Min(result, cell.ColumnIndex);
                }

                return result;
            }
        }

        public int RightMostSelectedColumn
        {
            get
            {
                if (SelectedCells.Count() == 0)
                {
                    return -1;
                }

                int result = 0;

                foreach (var cell in SelectedCells)
                {
                    result = Math.Max(result, cell.ColumnIndex + cell.ColumnSpan - 1);
                }

                return result;
            }
        }

        bool IsLogicalFocusWithin
        {
            get
            {
                var focusScope = FocusManager.GetFocusScope(this);
                if (focusScope == null)
                {
                    return false;
                }

                var focusedElement = FocusManager.GetFocusedElement(focusScope) as DependencyObject;
                if (focusedElement == null)
                {
                    return false;
                }

                return LayoutHelper.IsDescendant(this, focusedElement);
            }
        }

        private void Selection_Changed(object sender, SelectionChangedEventArgs<Visual> e)
        {
            OnSelectionChanged(e.RemovedItems, e.AddedItems);
        }

        protected virtual void OnSelectionChanged(Visual[] removedItems, Visual[] addedItems)
        {
            // Normally, each table cell manages setting focus to its appropriate
            // child when its selection changes; however, we need to handle the case
            // here where the previously-selected table cell is unselected and focus
            // needs to shift to the appropriate cell.

            if (removedItems != null && removedItems.Length > 0)
            {
                var selectedItem = Selection.LastOrDefault();

                var tableCell = selectedItem as NTableCell;
                if (tableCell != null)
                {
                    var element = tableCell.Selection.LastOrDefault();
                    if (element != null)
                    {
                        element.Focus();
                    }
                }
                else
                {
                    var element = selectedItem as UIElement;
                    if (element != null)
                    {
                        element.Focus();
                    }
                }
            }
        }

        #endregion

        #region Navigation

        public NTableCell NextCell(NTableCell target)
        {
            int rowIndex = target.RowIndex;
            int columnIndex = target.ColumnIndex;

            if ((columnIndex += target.ColumnSpan) >= ColumnCount)
            {
                columnIndex = 0;

                if ((rowIndex += target.RowSpan) >= Rows.Count)
                {
                    return null;
                }
            }

            return Grid[rowIndex, columnIndex];
        }

        public NTableCell PreviousCell(NTableCell target)
        {
            int rowIndex = target.RowIndex;
            int columnIndex = target.ColumnIndex;

            if (--columnIndex < 0)
            {
                columnIndex = Columns.Count - 1;

                if (--rowIndex < 0)
                {
                    return null;
                }
            }

            return Grid[rowIndex, columnIndex];
        }

        public bool MoveUpByCell(NTableCell target)
        {
            int rowIndex = target.RowIndex - 1;

            if (rowIndex < 0)
            {
                return false;
            }

            var cellAbove = Grid[rowIndex, target.ColumnIndex];

            Point navigationOffset = target.NavigationOffset;
            Selection.SelectOnly(cellAbove);
            cellAbove.NavigationOffset = navigationOffset;
            cellAbove.MoveToBottom();

            return true;
        }

        public bool MoveDownByCell(NTableCell target)
        {
            int rowIndex = target.RowIndex + target.RowSpan;

            if (rowIndex >= Rows.Count)
            {
                return false;
            }

            var cellBelow = Grid[rowIndex, target.ColumnIndex];

            Point navigationOffset = target.NavigationOffset;
            Selection.SelectOnly(cellBelow);
            cellBelow.NavigationOffset = navigationOffset;
            cellBelow.MoveToTop();

            return true;
        }

        public bool MoveLeftByCell(NTableCell target)
        {
            var previous = PreviousCell(target);

            if (previous == null)
            {
                return false;
            }

            Selection.SelectOnly(previous);
            previous.MoveToEnd();

            return true;
        }

        public bool MoveRightByCell(NTableCell target)
        {
            var next = NextCell(target);

            if (next == null)
            {
                return false;
            }

            Selection.SelectOnly(next);
            next.MoveToStart();

            return true;
        }

        public bool SelectUpByCell(NTableCell target)
        {
            int rowIndex = target.RowIndex - 1;

            if (rowIndex < 0)
            {
                return false;
            }

            var cellAbove = Grid[rowIndex, target.ColumnIndex];

            Point navigationOffset = target.NavigationOffset;
            SelectTo(cellAbove);
            cellAbove.NavigationOffset = navigationOffset;
            cellAbove.SelectToBottom();

            return true;
        }

        public bool SelectDownByCell(NTableCell target)
        {
            int rowIndex = target.RowIndex + target.RowSpan;

            if (rowIndex >= Rows.Count)
            {
                return false;
            }

            var cellBelow = Grid[rowIndex, target.ColumnIndex];

            Point navigationOffset = target.NavigationOffset;
            SelectTo(cellBelow);
            cellBelow.NavigationOffset = navigationOffset;
            cellBelow.SelectToTop();

            return true;
        }

        public bool SelectLeftByCell(NTableCell target)
        {
            var previous = PreviousCell(target);

            if (previous == null)
            {
                return false;
            }

            SelectTo(previous);
            previous.SelectToEnd();

            return true;
        }

        public bool SelectRightByCell(NTableCell target)
        {
            var next = NextCell(target);

            if (next == null)
            {
                return false;
            }

            SelectTo(next);
            next.SelectToStart();

            return true;
        }

        #endregion

        #region Operations

        /// <summary>
        /// Insert a new row above the currently-selected row
        /// </summary>
        public void InsertRowAbove()
        {
            using (var undo = new UndoScope(UndoStack, "Insert row above"))
            {
                if (RowCount == 0)
                {
                    AppendRow(CreateDefaultRow());
                    return;
                }

                // Determine where to insert the new row

                int rowIndex = TopMostSelectedRow;

                if (rowIndex == -1)
                {
                    rowIndex = 0;
                }

                // Create a new row

                NTableRow newRow = Rows[rowIndex].Group.CreateRow();

                // Populate the new row

                NTableCell cell;

                for (int columnIndex = 0; columnIndex < Grid.GetLength(1); columnIndex += cell.ColumnSpan)
                {
                    cell = Grid[rowIndex, columnIndex];

                    // can't insert into a multi-row cell - increment span instead

                    if (cell.RowSpan > 1 && cell.RowIndex < rowIndex)
                    {
                        cell.RowSpan++;
                        continue;
                    }

                    // otherwise, create a new cell

                    NTableCell newCell = newRow.CreateCell(true);
                    CopyStyle(cell, newCell);
                    newCell.ColumnSpan = cell.ColumnSpan;
                    newRow.Cells.Add(newCell);
                }

                // Insert the new row

                InsertRow(rowIndex, newRow);

                // Move focus to new row

                if (SelectedCells.Count() == 1)
                {
                    int columnIndex = SelectedCells.Last().ColumnIndex;
                    NTableCell target = Grid[rowIndex, columnIndex];
                    Selection.SelectOnly(target);
                    target.MoveToStart();
                    target.Focus();
                }
            }
        }

        /// <summary>
        /// Insert a new row below the currently-selected row
        /// </summary>
        public void InsertRowBelow()
        {
            using (var undo = new UndoScope(UndoStack, "Insert row below"))
            {
                if (RowCount == 0)
                {
                    AppendRow(CreateDefaultRow());
                    return;
                }

                // Determine where to insert the new row

                int rowIndex = BottomMostSelectedRow;

                if (rowIndex == -1)
                {
                    rowIndex = RowCount - 1;
                }

                // Create a new row

                NTableRow newRow = Rows[rowIndex].Group.CreateRow();

                // Populate the new row

                NTableCell cell;
                for (int columnIndex = 0; columnIndex < Grid.GetLength(1); columnIndex += cell.ColumnSpan)
                {
                    cell = Grid[rowIndex, columnIndex];

                    // can't insert into a multi-row cell - increment span instead

                    if (cell.RowIndex + cell.RowSpan > rowIndex + 1)
                    {
                        cell.RowSpan++;
                        continue;
                    }

                    // otherwise, create a new cell

                    NTableCell newCell = newRow.CreateCell(true);
                    CopyStyle(cell, newCell);
                    newCell.ColumnSpan = cell.ColumnSpan;
                    newRow.Cells.Add(newCell);
                }

                // Insert the new row

                InsertRow(rowIndex + 1, newRow);

                // Move focus to new row

                if (SelectedCells.Count() == 1)
                {
                    int columnIndex = SelectedCells.Last().ColumnIndex;
                    NTableCell target = Grid[rowIndex + 1, columnIndex];
                    Selection.SelectOnly(target);
                    target.MoveToStart();
                    target.Focus();
                }
            }
        }

        /// <summary>
        /// Insert a new column to the left of the currently-selected column
        /// </summary>
        public void InsertColumnLeft()
        {
            using (var undo = new UndoScope(UndoStack, "Insert column left"))
            {
                if (ColumnCount == 0)
                {
                    AppendColumn(CreateDefaultColumn());
                    return;
                }

                // Determine where to insert the new column

                int columnIndex = LeftMostSelectedColumn;

                if (columnIndex == -1)
                {
                    columnIndex = 0;
                }

                // Populate the new column

                NTableCell cell;
                for (int rowIndex = 0; rowIndex < Grid.GetLength(0); rowIndex += cell.RowSpan)
                {
                    cell = Grid[rowIndex, columnIndex];

                    // can't insert into a multi-column cell - increment span instead

                    if (cell.ColumnIndex < columnIndex)
                    {
                        cell.ColumnSpan++;
                        continue;
                    }

                    // otherwise, create a new cell

                    NTableCell newCell = cell.Row.CreateCell(true);
                    CopyStyle(cell, newCell);
                    newCell.RowSpan = cell.RowSpan;
                    newCell.ColumnSpan = 1;

                    int cellIndex = cell.Row.Cells.IndexOf(cell);
                    cell.Row.Cells.Insert(cellIndex, newCell);
                }

                InvalidateGrid(true);

                // Move focus to new column

                if (SelectedCells.Count() == 1)
                {
                    int rowIndex = SelectedCells.Last().RowIndex;
                    NTableCell target = Grid[rowIndex, columnIndex];
                    Selection.SelectOnly(target);
                    target.MoveToStart();
                    target.Focus();
                }
            }
        }

        /// <summary>
        /// Insert a new column to the right of the currently-selected column
        /// </summary>
        public void InsertColumnRight()
        {
            using (var undo = new UndoScope(UndoStack, "Insert column right"))
            {
                if (ColumnCount == 0)
                {
                    AppendColumn(CreateDefaultColumn());
                    return;
                }

                // Determine where to insert the new column

                int columnIndex = RightMostSelectedColumn;

                if (columnIndex == -1)
                {
                    columnIndex = ColumnCount - 1;
                }

                // Populate the new column

                NTableCell cell;
                for (int rowIndex = 0; rowIndex < Grid.GetLength(0); rowIndex += cell.RowSpan)
                {
                    cell = Grid[rowIndex, columnIndex];

                    // can't insert into a multi-column cell - increment span instead

                    if (cell.ColumnIndex + cell.ColumnSpan > columnIndex + 1)
                    {
                        cell.ColumnSpan++;
                        continue;
                    }

                    // otherwise, create a new cell

                    NTableCell newCell = cell.Row.CreateCell(true);
                    CopyStyle(cell, newCell);
                    newCell.RowSpan = cell.RowSpan;
                    newCell.ColumnSpan = 1;

                    int cellIndex = cell.Row.Cells.IndexOf(cell);
                    cell.Row.Cells.Insert(cellIndex + 1, newCell);
                }

                InvalidateGrid(true);

                // Move focus to new column

                if (SelectedCells.Count() == 1)
                {
                    int rowIndex = SelectedCells.Last().RowIndex;
                    NTableCell target = Grid[rowIndex, columnIndex + 1];
                    Selection.SelectOnly(target);
                    target.MoveToStart();
                    target.Focus();
                }
            }
        }

        private static void CopyStyle(NTableCell copyFrom, NTableCell copyTo)
        {
            // Background
            copyTo.Background = copyFrom.Background;
            // BorderStyle
            copyTo.BorderLeftStyle = copyFrom.BorderLeftStyle;
            copyTo.BorderTopStyle = copyFrom.BorderTopStyle;
            copyTo.BorderRightStyle = copyFrom.BorderRightStyle;
            copyTo.BorderBottomStyle = copyFrom.BorderBottomStyle;
            // BorderColor
            copyTo.BorderLeftColor = copyFrom.BorderLeftColor;
            copyTo.BorderTopColor = copyFrom.BorderTopColor;
            copyTo.BorderRightColor = copyFrom.BorderRightColor;
            copyTo.BorderBottomColor = copyFrom.BorderBottomColor;
            // BorderWidth
            copyTo.BorderLeftWidth = copyFrom.BorderLeftWidth;
            copyTo.BorderTopWidth = copyFrom.BorderTopWidth;
            copyTo.BorderRightWidth = copyFrom.BorderRightWidth;
            copyTo.BorderBottomWidth = copyFrom.BorderBottomWidth;
            // Paragraph
            var paragraph = copyFrom.Children.OfType<TextParagraph>().FirstOrDefault();
            if (paragraph != null)
            {
                var clone = (TextParagraph)paragraph.Clone();
                clone.Delete(0, clone.Length);
                copyTo.RemoveAll();
                copyTo.Append(clone);
            }        
        }

        /// <summary>
        /// Delete all selected rows
        /// </summary>
        public void DeleteSelectedRows()
        {
            using (var undo = new UndoScope(UndoStack, "Delete row(s)"))
            {
                while (SelectedCells.Count() > 0)
                {
                    RemoveRow(SelectedCells.Last().RowIndex);
                }
            }
        }

        /// <summary>
        /// Delete all selected columns
        /// </summary>
        public void DeleteSelectedColumns()
        {
            using (var undo = new UndoScope(UndoStack, "Delete column(s)"))
            {
                while (SelectedCells.Count() > 0)
                {
                    RemoveColumn(SelectedCells.Last().ColumnIndex);
                }
            }
        }

        /// <summary>
        /// Select all cells in each row that contains a selected cell
        /// </summary>
        public void SelectRows()
        {
            using (var undo = new UndoScope(UndoStack, "Select row(s)"))
            {
                var rowIndices = SelectedCells.Select(cell => cell.RowIndex).Distinct().ToArray();

                Selection.UnselectAll();
                foreach (int rowIndex in rowIndices)
                {
                    SelectRow(rowIndex);
                }
            }
        }

        /// <summary>
        /// Select all cells in each column that contains a selected cell
        /// </summary>
        public void SelectColumns()
        {
            using (var undo = new UndoScope(UndoStack, "Select column(s)"))
            {
                var columnIndices = SelectedCells.Select(cell => cell.ColumnIndex).Distinct().ToArray();

                Selection.UnselectAll();
                foreach (int columnIndex in columnIndices)
                {
                    SelectColumn(columnIndex);
                }
            }
        }

        // Select the table itself
        public void SelectTable()
        {
            Selection.UnselectAll();
            Focus();
        }

        /// <summary>
        /// Merge all selected cells
        /// </summary>
        public void MergeCells()
        {
            int top = TopMostSelectedRow;
            int bottom = BottomMostSelectedRow;
            int rowSpan = 1 + (bottom - top);

            int left = LeftMostSelectedColumn;
            int right = RightMostSelectedColumn;
            int columnSpan = 1 + (right - left);

            NTableCell cell = Grid[top, left];
            cell.RowSpan = rowSpan;
            cell.ColumnSpan = columnSpan;

            for (int rowIndex = top; rowIndex <= bottom; rowIndex++)
            {
                for (int columnIndex = left; columnIndex <= right; columnIndex++)
                {
                    if (rowIndex != top || columnIndex != left)
                    {
                        cell = Grid[rowIndex, columnIndex];

                        if (cell.RowIndex == rowIndex && cell.ColumnIndex == columnIndex)
                        {
                            cell.Row.Cells.Remove(cell);
                        }
                    }
                }
            }

            InvalidateGrid(true);
        }

        /// <summary>
        /// Split all selected cells
        /// </summary>
        public void SplitCells()
        {
            foreach (NTableCell cell in SelectedCells)
            {
                for (int i = 0; i < cell.RowSpan; i++)
                {
                    NTableRow row = Rows[cell.RowIndex + i];
                    
                    int cellIndex = row.Cells.Count((item) => item.ColumnIndex <= cell.ColumnIndex);

                    for (int j = 0; j < cell.ColumnSpan; j++)
                    {
                        if (i == 0 && j == 0)
                        {
                            continue;
                        }

                        NTableCell newCell = row.CreateCell(true);
                        CopyStyle(cell, newCell);
                        row.Cells.Insert(cellIndex, newCell);
                    }
                }

                cell.RowSpan = 1;
                cell.ColumnSpan = 1;
            }

            InvalidateGrid(true);
        }

        private NTableRow CreateDefaultRow()
        {
            NTableRow newRow = Body.CreateRow();

            for (int i = 0; i < ColumnCount; i++)
            {
                NTableCell newCell = newRow.CreateCell(true);

                // BorderStyle
                newCell.BorderLeftStyle = BorderLeftStyle;
                newCell.BorderTopStyle = BorderTopStyle;
                newCell.BorderRightStyle = BorderRightStyle;
                newCell.BorderBottomStyle = BorderBottomStyle;
                // BorderColor
                newCell.BorderLeftColor = BorderLeftColor;
                newCell.BorderTopColor = BorderTopColor;
                newCell.BorderRightColor = BorderRightColor;
                newCell.BorderBottomColor = BorderBottomColor;
                // BorderWidth
                newCell.BorderLeftWidth = BorderLeftWidth;
                newCell.BorderTopWidth = BorderTopWidth;
                newCell.BorderRightWidth = BorderRightWidth;
                newCell.BorderBottomWidth = BorderBottomWidth;

                newRow.Cells.Add(newCell);
            }

            return newRow;
        }

        private NTableColumn CreateDefaultColumn()
        {
            NTableColumn newColumn = new NTableColumn(this);

            for (int i = 0; i < RowCount; i++)
            {
                NTableCell newCell = Rows[i].CreateCell(true);

                // BorderStyle
                newCell.BorderLeftStyle = BorderLeftStyle;
                newCell.BorderTopStyle = BorderTopStyle;
                newCell.BorderRightStyle = BorderRightStyle;
                newCell.BorderBottomStyle = BorderBottomStyle;
                // BorderColor
                newCell.BorderLeftColor = BorderLeftColor;
                newCell.BorderTopColor = BorderTopColor;
                newCell.BorderRightColor = BorderRightColor;
                newCell.BorderBottomColor = BorderBottomColor;
                // BorderWidth
                newCell.BorderLeftWidth = BorderLeftWidth;
                newCell.BorderTopWidth = BorderTopWidth;
                newCell.BorderRightWidth = BorderRightWidth;
                newCell.BorderBottomWidth = BorderBottomWidth;

                newColumn.Cells.Add(newCell);
            }

            return newColumn;
        }

        #endregion

        #region Commands

        private void LoadCommandBindings()
        {
            CommandBindings.AddRange(new [] {
                // Insert
                new CommandBinding(NTableCommands.InsertRowAbove, InsertRowAboveCommand_Executed),
                new CommandBinding(NTableCommands.InsertRowBelow, InsertRowBelowCommand_Executed),
                new CommandBinding(NTableCommands.InsertColumnLeft, InsertColumnLeftCommand_Executed),
                new CommandBinding(NTableCommands.InsertColumnRight, InsertColumnRightCommand_Executed),
                // Delete
                new CommandBinding(NTableCommands.DeleteRow, DeleteRowCommand_Executed),
                new CommandBinding(NTableCommands.DeleteColumn, DeleteColumnCommand_Executed),
                new CommandBinding(NTableCommands.DeleteTable, DeleteTableCommand_Executed),
                // Select
                new CommandBinding(NTableCommands.SelectRow, SelectRowCommand_Executed),
                new CommandBinding(NTableCommands.SelectColumn, SelectColumnCommand_Executed),
                new CommandBinding(NTableCommands.SelectTable, SelectTableCommand_Executed),
            });
        }   

        void InsertRowAboveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertRowAbove();

            e.Handled = true;
        }

        void InsertRowBelowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertRowBelow();

            e.Handled = true;
        }

        void InsertColumnLeftCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertColumnLeft();

            e.Handled = true;
        }

        void InsertColumnRightCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertColumnRight();

            e.Handled = true;
        }

        void DeleteRowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeleteSelectedRows();

            e.Handled = true;
        }

        void DeleteColumnCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeleteSelectedColumns();

            e.Handled = true;
        }

        void DeleteTableCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditingPanel.DeleteCommand.Execute(this, this);

            e.Handled = true;
        }

        void SelectRowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectRows();

            e.Handled = true;
        }

        void SelectColumnCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectColumns();

            e.Handled = true;
        }

        void SelectTableCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectTable();

            e.Handled = true;
        }

        #endregion

        #region IEditable

        public override IList<object> Cut()
        {
            if (!IsLogicalFocusWithin)
            {
                return new object[] { this };
            }

            using (var undo = new UndoScope(UndoStack, "Cut"))
            {
                var results = Copy();
                Delete();
                return results;
            }
        }

        public override IList<object> Copy()
        {
            if (!IsLogicalFocusWithin)
            {
                return new object[] { Clone() };
            }

            var results = new List<object>();

            using (var undo = new UndoScope(UndoStack, "Copy"))
            {
                var selection = SelectedCells.OrderBy(i => i.RowIndex * ColumnCount + i.ColumnIndex);

                foreach (var cell in selection)
                {
                    var items = cell.Copy();

                    results.AddRange(items);
                }
            }

            return results;
        }

        public override IList<object> Paste(IList<object> items)
        {
            if (Selection.Count == 0)
            {
                return items;
            }

            var results = new List<object>();

            using (var undo = new UndoScope(UndoStack, "Paste"))
            {
                Delete();

                var target = Selection.Last();

                items = Editable.Paste(target, items);

                results.Add(items);
            }

            return results;
        }

        public override bool Delete()
        {
            if (!IsLogicalFocusWithin)
            {
                return false;
            }

            using (var undo = new UndoScope(UndoStack))
            {
                foreach (var item in Selection.OfType<IEditable>())
                {
                    item.Delete();
                }

                // After a delete, the user expects a single item to be selected:

                if (Selection.Count > 0)
                {
                    Selection.SelectOnly(Selection.Last());
                }
            }

            return true;
        }

        #endregion

        #region INavigable

        public Point NavigationOffset
        {
            get
            {
                var cell = SelectedCells.LastOrDefault();
                if (cell != null)
                {
                    var transform = cell.TransformToAncestor(this);
                    return transform.Transform(cell.NavigationOffset);
                }
                else
                {
                    return new Point(0, 0);
                }
            }
            set
            {
                var cell = SelectedCells.LastOrDefault();
                if (cell != null)
                {
                    var transform = TransformToDescendant(cell);
                    cell.NavigationOffset = transform.Transform(value);
                }
            }
        }

        public bool MoveUp()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((Visual)target);

            if (target.MoveUp())
            {
                return true;
            }

            if (target is NTableCell)
            {
                MoveUpByCell((NTableCell)target);
            }

            return true;
        }

        public bool MoveDown()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((Visual)target);

            if (target.MoveDown())
            {
                return true;
            }

            if (target is NTableCell)
            {
                MoveDownByCell((NTableCell)target);
            }

            return true;
        }

        public bool MoveLeft()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((Visual)target);

            if (target.MoveLeft())
            {
                return true;
            }

            if (target is NTableCell)
            {
                MoveLeftByCell((NTableCell)target);
            }

            return true;
        }

        public bool MoveRight()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((Visual)target);

            if (target.MoveRight())
            {
                return true;
            }

            if (target is NTableCell)
            {
                MoveRightByCell((NTableCell)target);
            }

            return true;
        }

        public void MoveToStart()
        {
            if (Selection.LastOrDefault() == Title)
            {
                Selection.SelectOnly(Title);
                Title.MoveToStart();
            }
            else if (RowCount > 0 && ColumnCount > 0)
            {
                var firstCell = Grid[0, 0];
                Selection.SelectOnly(firstCell);
                firstCell.MoveToStart();
            }
        }

        public void MoveToEnd()
        {
            if (Selection.LastOrDefault() == Title)
            {
                Selection.SelectOnly(Title);
                Title.MoveToEnd();
            }
            else if (RowCount > 0 && ColumnCount > 0)
            {
                var lastCell = Grid[RowCount - 1, ColumnCount - 1];
                Selection.SelectOnly(lastCell);
                lastCell.MoveToEnd();
            }
        }

        public void MoveToTop()
        {
        }

        public void MoveToBottom()
        {
        }

        public void MoveToLeft()
        {
        }

        public void MoveToRight()
        {

        }

        public bool SelectUp()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectUp())
            {
                return true;
            }

            if (target is NTableCell)
            {
                SelectUpByCell((NTableCell)target);
            }

            return true;
        }

        public bool SelectDown()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectDown())
            {
                return true;
            }

            if (target is NTableCell)
            {
                SelectDownByCell((NTableCell)target);
            }

            return true;
        }

        public bool SelectLeft()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectLeft())
            {
                return true;
            }

            if (target is NTableCell)
            {
                SelectLeftByCell((NTableCell)target);
            }

            return true;
        }

        public bool SelectRight()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectRight())
            {
                return true;
            }

            if (target is NTableCell)
            {
                SelectRightByCell((NTableCell)target);
            }

            return true;
        }

        public void SelectToStart()
        {
            if (Selection.LastOrDefault() == Title)
            {
                Title.SelectToStart();
            }
            else if (RowCount > 0 && ColumnCount > 0)
            {
                var firstCell = Grid[0, 0];
                SelectTo(firstCell);
                firstCell.SelectToStart();
            }
        }

        public void SelectToEnd()
        {
            if (Selection.LastOrDefault() == Title)
            {
                Title.SelectToEnd();
            }
            else if (RowCount > 0 && ColumnCount > 0)
            {
                var lastCell = Grid[RowCount - 1, ColumnCount - 1];
                SelectTo(lastCell);
                lastCell.SelectToEnd();
            }
        }

        public void SelectToTop()
        {
        }

        public void SelectToBottom()
        {
        }

        public void SelectToLeft()
        {

        }

        public void SelectToRight()
        {

        }

        public bool TabForward()
        {
            if (RowCount == 0 && ColumnCount == 0)
            {
                return false;
            }

            NTableCell nextCell;
            NTableCell currentCell = SelectedCells.LastOrDefault();

            if (currentCell != null)
            {
                nextCell = NextCell(currentCell);
            }
            else
            {
                nextCell = Grid[0, 0];
            }

            if (nextCell != null)
            {
                Selection.SelectOnly(nextCell);
                nextCell.MoveToStart();
                nextCell.SelectToEnd();
            }

            return true;
        }

        public bool TabBackward()
        {
            if (RowCount == 0 || ColumnCount == 0)
            {
                return false;
            }

            NTableCell previousCell;
            NTableCell currentCell = SelectedCells.LastOrDefault();

            if (currentCell != null)
            {
                previousCell = PreviousCell(currentCell);
            }
            else
            {
                previousCell = Grid[RowCount - 1, ColumnCount - 1];
            }

            if (previousCell != null)
            {
                Selection.SelectOnly(previousCell);
                previousCell.MoveToEnd();
                previousCell.SelectToStart();
            }

            return true;
        }

        #endregion

        #region ISelectableText

        public bool IsTextSelected
        {
            get
            {
                if (TextSelection.Count() == 0)
                {
                    return false;
                }
                else if (TextSelection.Count() == 1)
                {
                    return TextSelection.Last().IsTextSelected;
                }
                else
                {
                    return true;
                }
            }
        }

        public string SelectedText
        {
            get
            {
                var buffer = new StringBuilder();

                foreach (var textElement in TextSelection)
                {
                    buffer.Append(textElement.SelectedText);
                }

                return buffer.ToString();
            }
        }

        #endregion

        #region ITextElement

        public void Insert(string value)
        {
            using (var undo = new UndoScope(UndoStack))
            {
                var target = TextSelection.LastOrDefault();

                if (target != null)
                {
                    target.Insert(value);
                }
            }
        }

        public void Replace(string value)
        {
            using (var undo = new UndoScope(UndoStack))
            {
                if (Selection.Count == 1 && Selection.First() is ITextElement)
                {
                    ((ITextElement)Selection.First()).Replace(value);
                }
                else
                {
                    Delete();
                    Insert(value);
                }
            }
        }

        public int Replace(string oldValue, string newValue)
        {
            using (var undo = new UndoScope(UndoStack))
            {
                int count = 0;

                foreach (var item in TextSelection)
                {
                    count += item.Replace(oldValue, newValue);
                }

                return count;
            }
        }

        public bool MoveLeftByWord()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((Visual)target);

            if (target.MoveLeftByWord())
            {
                return true;
            }

            if (target is NTableCell)
            {
                MoveLeftByCell((NTableCell)target);
            }

            return true;
        }

        public bool MoveRightByWord()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((Visual)target);

            if (target.MoveRightByWord())
            {
                return true;
            }

            if (target is NTableCell)
            {
                MoveRightByCell((NTableCell)target);
            }

            return true;
        }

        public bool MoveToLineStart()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((Visual)target);

            if (target.MoveToLineStart())
            {
                return true;
            }

            if (target is NTableCell)
            {
                MoveLeftByCell((NTableCell)target);
            }

            return true;
        }

        public bool MoveToLineEnd()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((Visual)target);

            if (target.MoveToLineEnd())
            {
                return true;
            }

            if (target is NTableCell)
            {
                MoveRightByCell((NTableCell)target);
            }

            return true;
        }

        public bool MoveToParagraphStart()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((Visual)target);

            if (target.MoveToParagraphStart())
            {
                return true;
            }

            if (target is NTableCell)
            {
                MoveLeftByCell((NTableCell)target);
            }

            return true;
        }

        public bool MoveToParagraphEnd()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((Visual)target);

            if (target.MoveToParagraphEnd())
            {
                return true;
            }

            if (target is NTableCell)
            {
                MoveRightByCell((NTableCell)target);
            }

            return true;
        }

        public bool SelectLeftByWord()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectLeftByWord())
            {
                return true;
            }

            if (target is NTableCell)
            {
                SelectLeftByCell((NTableCell)target);
            }

            return true;
        }

        public bool SelectRightByWord()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectRightByWord())
            {
                return true;
            }

            if (target is NTableCell)
            {
                SelectRightByCell((NTableCell)target);
            }

            return true;
        }

        public bool SelectToLineStart()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectToLineStart())
            {
                return true;
            }

            if (target is NTableCell)
            {
                SelectLeftByCell((NTableCell)target);
            }

            return true;
        }

        public bool SelectToLineEnd()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectToLineEnd())
            {
                return true;
            }

            if (target is NTableCell)
            {
                SelectRightByCell((NTableCell)target);
            }

            return true;
        }

        public bool SelectToParagraphStart()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectToParagraphStart())
            {
                return true;
            }

            if (target is NTableCell)
            {
                SelectLeftByCell((NTableCell)target);
            }

            return true;
        }

        public bool SelectToParagraphEnd()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectToParagraphEnd())
            {
                return true;
            }

            if (target is NTableCell)
            {
                SelectRightByCell((NTableCell)target);
            }

            return true;
        }

        public bool DeleteBack()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target != null)
            {
                return target.DeleteBack();
            }
            else
            {
                return false;
            }
        }

        public bool DeleteBackByWord()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target != null)
            {
                return target.DeleteBackByWord();
            }
            else
            {
                return false;
            }
        }

        public bool DeleteBackByParagraph()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target != null)
            {
                return target.DeleteBackByParagraph();
            }
            else
            {
                return false;
            }
        }

        public bool DeleteForward()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target != null)
            {
                return target.DeleteForward();
            }
            else
            {
                return false;
            }
        }

        public bool DeleteForwardByWord()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target != null)
            {
                return target.DeleteForwardByWord();
            }
            else
            {
                return false;
            }
        }

        public bool DeleteForwardByParagraph()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target != null)
            {
                return target.DeleteForwardByParagraph();
            }
            else
            {
                return false;
            }
        }

        public bool EnterLineBreak()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target != null)
            {
                return target.EnterLineBreak();
            }
            else
            {
                return false;
            }
        }

        public bool EnterParagraphBreak()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target != null)
            {
                return target.EnterParagraphBreak();
            }
            else
            {
                return false;
            }
        }

        public ITextElement Split()
        {
            return null;
        }

        public bool Merge(ITextElement other)
        {
            return false;
        }

        #endregion

        #region IFormattable

        public override bool HasProperty(string name)
        {
            foreach (var formattable in Selection.OfType<IFormattable>())
            {
                if (formattable.HasProperty(name))
                {
                    return true;
                }
            }

            return false;
        }

        public override void SetProperty(string name, object value)
        {
            using (var undo = new UndoScope(UndoStack, "Set Property"))
            {
                Format(() =>
                {
                    foreach (var formattable in Selection.OfType<IFormattable>())
                    {
                        formattable.SetProperty(name, value);
                    }
                });
            }
        }

        public override object GetProperty(string name)
        {
            IFormattable target;

            // If the table itself is selected (rather than its children),
            // any formatting operation applies to all cells
            if (IsSelected && Selection.Count == 0)
            {
                target = Cells.LastOrDefault();
            }
            else
            {
                target = Selection.OfType<IFormattable>().LastOrDefault();
            }

            if (target != null)
            {
                return target.GetProperty(name);
            }
            else
            {
                return null;
            }

        }

        public override void ResetProperties()
        {
            using (var undo = new UndoScope(UndoStack, "Reset Properties"))
            {
                Format(() =>
                {
                    foreach (var formattable in Selection.OfType<IFormattable>())
                    {
                        formattable.ResetProperties();
                    }
                });
            }
        }

        public override int ChangeProperty(string name, object oldValue, object newValue)
        {
            int result = 0;

            using (var undo = new UndoScope(UndoStack, "Change Property"))
            {
                Format(() =>
                {
                    foreach (var formattable in Selection.OfType<IFormattable>())
                    {
                        result += formattable.ChangeProperty(name, oldValue, newValue);
                    }
                });
            }

            return result;
        }

        /// <summary>
        /// If the table itself is selected (rather than its children),
        /// any formatting operation applies to all cells.
        /// </summary>
        /// <param name="action"></param>
        void Format(Action action)
        {
            bool isTableSelected = IsSelected && Selection.Count == 0;

            if (isTableSelected)
            {
                MoveToStart();
                SelectToEnd();
            }

            try
            {
                action();
            }
            finally
            {
                if (isTableSelected)
                {
                    SelectTable();
                }
            }
        }

        #endregion

        #region ISearchable

        private IEnumerable<Visual> SearchableElements
        {
            get
            {
                yield return Title;

                foreach (var cell in Cells)
                {
                    yield return cell;
                }
            }
        }

        public int Find(string findText)
        {
            return Searchable.Find(SearchableElements, findText);
        }

        public bool FindFirst()
        {
            var result = Searchable.FindFirst(SearchableElements);
            if (result != null)
            {
                Selection.SelectOnly((Visual)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FindLast()
        {
            var result = Searchable.FindLast(SearchableElements);
            if (result != null)
            {
                Selection.SelectOnly((Visual)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FindNext()
        {
            var result = Searchable.FindNext(SearchableElements.ToList(), Selection.LastOrDefault());
            if (result != null)
            {
                Selection.SelectOnly((Visual)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FindPrevious()
        {
            var result = Searchable.FindPrevious(SearchableElements.ToList(), Selection.LastOrDefault());
            if (result != null)
            {
                Selection.SelectOnly((Visual)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FindFirst(string pattern, RegexOptions options)
        {
            var result = Searchable.FindFirst(SearchableElements, pattern, options);
            if (result != null)
            {
                Selection.SelectOnly((Visual)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FindLast(string pattern, RegexOptions options)
        {
            var result = Searchable.FindLast(SearchableElements, pattern, options);
            if (result != null)
            {
                Selection.SelectOnly((Visual)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FindNext(string pattern, RegexOptions options)
        {
            var result = Searchable.FindNext(SearchableElements.ToList(), pattern, options, Selection.LastOrDefault());
            if (result != null)
            {
                Selection.SelectOnly((Visual)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FindPrevious(string pattern, RegexOptions options)
        {
            var result = Searchable.FindPrevious(SearchableElements.ToList(), pattern, options, Selection.LastOrDefault());
            if (result != null)
            {
                Selection.SelectOnly((Visual)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region ISelectable

        public override void Select()
        {
            Title.Visibility = Visibility.Visible;

            base.Select();
        }

        public override void Unselect()
        {
            if (Title.Length == 0)
            {
                Title.Visibility = Visibility.Hidden;
            }

            Selection.UnselectAll();

            base.Unselect();
        }

        #endregion

        #region IHasResources

        public override IEnumerable<string> ResourceNames
        {
            get
            {
                foreach (NTableCell cell in Cells)
                {
                    foreach (var fileName in cell.ResourceNames)
                    {
                        yield return fileName;
                    }
                }
            }
        }

        protected override void OnGetResource(string url, Stream stream)
        {
            foreach (NTableCell cell in Cells)
            {
                if (cell.ResourceNames.Contains(url))
                {
                    cell.GetResource(url, stream);
                    break;
                }
            }
        }

        protected override void OnSetResource(string url, Stream stream)
        {
            foreach (NTableCell cells in Cells)
            {
                if (cells.ResourceNames.Contains(url))
                {
                    cells.SetResource(url, stream);
                    break;
                }
            }
        }

        #endregion

        #region HTML

        public override string GetHTMLNodeName(NodeContext context)
        {
            return HTMLElements.TABLE;
        }

        public override IEnumerable<object> GetHTMLChildNodes(NodeContext context)
        {
            if (Title.Length > 0)
            {
                yield return Title;
            }

            foreach (var row in Rows)
            {
                yield return row;
            }
        }

        public override object CreateHTMLNode(NodeContext context)
        {
            switch (context.NodeName)
            {
                case HTMLElements.CAPTION:
                    return new NTableTitle();
                case HTMLElements.THEAD:
                    return Head;
                case HTMLElements.TFOOT:
                    return Foot;
                case HTMLElements.TBODY:
                    return Body;
                case HTMLElements.TR:
                    return Body.CreateRow();
                default:
                    return null;
            }
        }

        public override void AppendHTMLNode(NodeContext context, object newChild)
        {
            if (newChild is NTableTitle)
            {
                Title = (NTableTitle)newChild;
            }
            else if (newChild is NTableRowGroup)
            {

            }
            else if (newChild is NTableRow)
            {
                Rows.RemoveAll((row) => row.IsAutoGenerated);
                AppendRow((NTableRow)newChild);
            }
        }

        public override void InsertHTMLNode(NodeContext context, object newChild, object refChild)
        {
            if (newChild is NTableTitle)
            {
                Title = (NTableTitle)newChild;
            }
            else if (newChild is NTableRowGroup)
            {
            }
            else if (newChild is NTableRow)
            {
                int index = Rows.IndexOf((NTableRow)refChild);
                if (index != -1)
                {
                    InsertRow(index, (NTableRow)newChild);
                }
            }
        }

        public override void RemoveHTMLNode(NodeContext context, object oldChild)
        {
            if (oldChild is NTableTitle)
            {
                Title = null;
            }

            else if (oldChild is NTableRowGroup)
            {

            }

            else if (oldChild is NTableRow)
            {
                RemoveRow((NTableRow)oldChild);

            }
        }

        #endregion

        #region IEnumerable

        public IEnumerator<NTableRow> GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        #endregion

        #region ICloneable

        public virtual object Clone()
        {
            return new NTable(this);
        }

        #endregion

        #region Implementation

        #region Placing

        private bool _IsPlacing = false;

        public bool IsPlacing
        {
            get { return _IsPlacing; }
            set
            {
                if (value != _IsPlacing)
                {
                    _IsPlacing = value;
                    if (value)
                    {
                        Opacity = 0.75;
                        DocumentPanel.SetPositioning(this, Positioning.Absolute);
                        CaptureMouse();
                    }
                    else
                    {
                        ReleaseMouseCapture();
                        DocumentPanel.SetPositioning(this, Positioning.Overlapped);
                        Opacity = 1.0;
                    }
                }
            }
        }

        #endregion

        #region Mouse Input

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            // If placing, finish placing

            if (IsPlacing)
            {
                IsPlacing = false;
                e.Handled = true;
                return;
            }

            // Get clicked object

            Point hitPoint = e.GetPosition(this);

            var hitObject = InputHitTest(hitPoint) as DependencyObject;

            // Handle clicked title

            var title = LayoutHelper.GetSelfOrAncestor<NTableTitle>(hitObject);
            if (title != null && (e.ChangedButton == MouseButton.Left || !Selection.Contains(title)))
            {
                Selection.SelectOnly(title);
            }

            // Handle clicked cell

            var cell = LayoutHelper.GetSelfOrAncestor<NTableCell>(hitObject);
            if (cell != null && (e.ChangedButton == MouseButton.Left || !Selection.Contains(cell)))
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    SelectTo(cell);
                }
                else
                {
                    Selection.SelectOnly(cell);
                }
            }

            // Handle clicked table

            if (hitObject == this)
            {
                SelectTable();
            }

            Mouse.Capture(this, CaptureMode.SubTree);

            base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);

            ReleaseMouseCapture();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            e.Handled = true;
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            bool isSelecting = e.LeftButton.HasFlag(MouseButtonState.Pressed);

            if (isSelecting)
            {
                // Get clicked object

                var hitPoint = e.GetPosition(this);
                var hitObject = InputHitTest(hitPoint) as DependencyObject;

                // Handle cell click

                var cell = LayoutHelper.GetSelfOrAncestor<NTableCell>(hitObject);
                if (cell != null)
                {
                    SelectTo(cell);
                }
                // cursor is outside the table bounds - select to closest cell
                else if (hitPoint.X < 0)
                {
                    cell = Columns.First().Cells.LastOrDefault(c => e.GetPosition(c).Y > 0) ?? Columns.First().Cells.First();
                    cell.NavigationOffset = e.GetPosition(cell);
                    SelectTo(cell);
                    cell.SelectToLeft();
                }
                else if (hitPoint.X > ActualWidth)
                {
                    cell = Columns.Last().Cells.LastOrDefault(c => e.GetPosition(c).Y > 0) ?? Columns.Last().Cells.First();
                    SelectTo(cell);
                    cell.SelectToEnd();
                }
                else if (hitPoint.Y < 0)
                {
                    cell = Rows.First().Cells.LastOrDefault(c => e.GetPosition(c).X > 0) ?? Rows.First().Cells.First();
                    cell.NavigationOffset = e.GetPosition(cell);
                    SelectTo(cell);
                    cell.SelectToTop();
                }
                else if (hitPoint.Y > ActualHeight)
                {
                    cell = Rows.Last().Cells.LastOrDefault(c => e.GetPosition(c).X > 0) ?? Rows.Last().Cells.First();
                    cell.NavigationOffset = e.GetPosition(cell);
                    SelectTo(cell);
                    cell.SelectToBottom();
                }
            }

            base.OnPreviewMouseMove(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsPlacing)
            {
                Point point = e.GetPosition(DocumentPanel.FindPanel(this));
                DocumentPanel.SetLeft(this, point.X - RenderSize.Width / 2);
                DocumentPanel.SetTop(this, point.Y - RenderSize.Height / 2);
            }

            e.Handled = true;
        }

        #endregion

        #region Keyboard Input

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (Selection.Count > 1)
            {
                if (InputHelper.IsEditingStroke(e))
                {
                    Delete();

                    if (e.Key == Key.Back || e.Key == Key.Delete)
                    {
                        e.Handled = true;
                    }
                }

                if (InputHelper.IsEditingStroke(e) ||
                    InputHelper.IsNavigationKey(e.Key) && !e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    var target = LayoutHelper.ChildFromDescendant(
                        this,
                        e.OriginalSource as DependencyObject
                    ) as NTableCell;

                    if (target != null)
                    {
                        Selection.SelectOnly(target);
                    }
                }
            }

            // Tab 

            if (e.Key == Key.Tab)
            {
                if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    TabBackward();
                }
                else
                {
                    TabForward();
                }

                e.Handled = true;
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    OnDeletePressed(e);
                    break;
            }

            base.OnKeyDown(e);
        }

        protected void OnDeletePressed(KeyEventArgs e)
        {
            if (e.OriginalSource == this)
            {
                NEditingCommands.DeleteForward.Execute(null, this);
                e.Handled = true;
            }
        }

        #endregion

        #region Layout

        /// <summary>
        /// Get the number of columns whose width is set to "auto"
        /// </summary>
        private int AutoColumnCount
        {
            get { return (from column in Columns where Double.IsNaN(column.Width) select column).Count(); }
        }

        /// <summary>
        /// Get the sum of all explicitly given column widths.
        /// </summary>
        private double ExplicitColumnWidth
        {
            get 
            {
                double result = 0;

                foreach (NTableColumn column in Columns)
                {
                    if (!Double.IsNaN(column.Width))
                    {
                        result += column.Width;
                    }
                }

                return result;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            base.MeasureOverride(availableSize);

            UpdateGrid();

            Size desiredSize = new Size(0, 0);
            Size measuredSize = new Size(0, 0);

            // DETERMINE THE WIDTH OF EACH COLUMN

            // a. If the column's width is explicitly given (!= Double.NaN), 
            //    use that value; otherwise find the natural width of each 
            //    cell that spans only that column and use that value.

            for (int columnIndex = 0; columnIndex < ColumnCount; columnIndex++)
            {
                NTableColumn column = Columns[columnIndex];

                // is the width explicitly given?
                if (!Double.IsNaN(column.Width))
                {
                    column.ComputedWidth = column.Width;
                    continue;
                }

                column.ComputedWidth = 0;

                for (int rowIndex = 0; rowIndex < RowCount; rowIndex++)
                {
                    NTableCell cell = Grid[rowIndex, columnIndex];

                    if (cell.ColumnSpan == 1)
                    {
                        double measuredWidth = MeasureWidth(cell);
                        column.ComputedWidth = Math.Max(column.ComputedWidth, measuredWidth);
                    }
                }
            }

            // b. Find the maximum width of each cell that spans multiple
            //    columns. If that is greater than the sum of the currently-
            //    assigned widths of the columns it spans, expand each of
            //    those columns whose width is not explicitly set.

            foreach (NTableCell cell in Cells)
            {
                if (cell.ColumnSpan > 1)
                {
                    double measuredWidth = MeasureWidth(cell);
                    double computedWidth = ComputeWidth(cell);

                    double excessWidth = measuredWidth - computedWidth;
                    if (excessWidth > 0)
                    {
                        var autoColumns = AutoColumns(cell).ToArray();

                        foreach (var column in autoColumns)
                        {
                            column.ComputedWidth += excessWidth / autoColumns.Length;
                        }
                    }
                }
            }

            double computedTableWidth = 0;

            if (Columns.Count > 0)
            {
                computedTableWidth = Columns.Sum((column) => column.ComputedWidth);
            }

            // DETERMINE THE TABLE WIDTH

            // a. If the width is explicitly given, use that value.   

            if (!Double.IsNaN(Width))
            {
                desiredSize.Width = Width;
            }

            // b. Otherwise, use the sum of column widths computed above, 
            //    limited to the available width.

            else
            {
                desiredSize.Width = Math.Min(computedTableWidth, availableSize.Width);
            }

            // c. If the used width does not equal the sum of the computed 
            //    column widths, adjust each column width to make them fit.

            double excessTableWidth = desiredSize.Width - computedTableWidth;

            if (Math.Round(excessTableWidth) != 0)
            {
                // First adjust auto-width columns

                double totalAutoColumnWidth = Columns.Sum(column => Double.IsNaN(column.Width) ? column.ComputedWidth : 0);
                if (totalAutoColumnWidth != 0)
                {
                    // If any auto-width columns have a computed width, then scale all auto-width columns
                    // by an appropriate scale factor. This is the usual case.
                    double scaleFactor = Math.Max(totalAutoColumnWidth + excessTableWidth, 0) / totalAutoColumnWidth;
                    foreach (NTableColumn column in Columns)
                    {
                        if (Double.IsNaN(column.Width))
                        {
                            excessTableWidth += column.ComputedWidth;
                            column.ComputedWidth *= scaleFactor;
                            excessTableWidth -= column.ComputedWidth;
                        }
                    }
                }
                else
                {
                    int autoColumnCount = Columns.Count(column => Double.IsNaN(column.Width));
                    double autoColumnWidth = excessTableWidth / autoColumnCount;
                    foreach (NTableColumn column in Columns)
                    {
                        if (Double.IsNaN(column.Width))
                        {
                            column.ComputedWidth = autoColumnWidth;
                            excessTableWidth -= column.ComputedWidth;
                        }
                    }
                }

                // Then adjust fixed-width columns if needed

                if (Math.Round(excessTableWidth) != 0)
                {
                    double totalFixedColumnWidth = Columns.Sum(column => !Double.IsNaN(column.Width) ? column.ComputedWidth : 0);
                    double scaleFactor = Math.Max(totalFixedColumnWidth + excessTableWidth, 0) / totalFixedColumnWidth;

                    foreach (NTableColumn column in Columns)
                    {
                        if (!Double.IsNaN(column.Width))
                        {
                            column.ComputedWidth *= scaleFactor;
                        }
                    }
                }
            }

            // PIXEL-ALIGN COLUMNS

            double remainder = 0;

            foreach (NTableColumn column in Columns)
            {
                column.ComputedWidth += remainder;

                if (column != Columns.Last())
                {
                    double roundedWidth = Math.Round(column.ComputedWidth);
                    remainder = column.ComputedWidth - roundedWidth;
                    column.ComputedWidth = roundedWidth;
                }

                column.ComputedWidth = Math.Max(column.ComputedWidth, 0);
            }

            // DETERMINE THE HEIGHT OF THE TITLE ELEMENT

            Title.Measure(new Size(desiredSize.Width, Double.PositiveInfinity));

            measuredSize.Height += Title.DesiredSize.Height;

            // DETERMINE THE HEIGHT OF EACH ROW

            // a. Determine minimum height of each row based on the minimum
            //    height of each cell that spans only that row.

            foreach (NTableRow row in Rows)
            {
                row.ComputedHeight = row.MinHeight;

                foreach (NTableCell cell in row.Cells)
                {
                    if (cell.RowSpan == 1)
                    {
                        double measuredHeight = MeasureHeight(cell);

                        row.ComputedHeight = Math.Max(row.ComputedHeight, measuredHeight);
                    }
                }
            }

            // b. Compute the minimum height of each cell that spans multiple 
            //    rows. If that height is greater than the sum of the minimum
            //    heights of each row spanned by that cell, then those rows'
            //    minimum heights are expanded.

            foreach (NTableCell cell in Cells)
            {
                if (cell.RowSpan > 1)
                {
                    double measuredHeight = MeasureHeight(cell);
                    double computedHeight = ComputeHeight(cell);

                    double excessHeight = measuredHeight - computedHeight;

                    if (excessHeight > 0)
                    {
                        foreach (NTableRow row in GetRows(cell))
                        {
                            row.ComputedHeight += excessHeight / cell.RowSpan;
                        }
                    }
                }
            }

            if (Rows.Count > 0)
            {
                measuredSize.Height += Rows.Sum((row) => row.ComputedHeight);
            }

            // Ensure desired height = measured height

            // Get table height

            if (Double.IsNaN(Height))
            {
                desiredSize.Height = measuredSize.Height;
            }
            else
            {
                // If desired height != measured height, scale each row

                double titleHeight = Title.DesiredSize.Height;
                double desiredContentHeight = Math.Max(Height - titleHeight, 0);
                double measuredContentHeight = Math.Max(measuredSize.Height - titleHeight, 0);

                if (measuredContentHeight > 0)
                {
                    double scale = desiredContentHeight / measuredContentHeight;
                    remainder = 0;
                    foreach (var row in Rows)
                    {
                        row.ComputedHeight *= scale;
                        // Take care of rounding errors
                        row.ComputedHeight += remainder;
                        double roundedHeight = Math.Round(row.ComputedHeight);
                        remainder = row.ComputedHeight - roundedHeight;
                        row.ComputedHeight = roundedHeight;
                    }
                }
                else if (RowCount > 0)
                {
                    double rowHeight = desiredContentHeight / RowCount;
                    remainder = 0;
                    foreach (var row in Rows)
                    {
                        row.ComputedHeight = rowHeight;
                        // Take care of rounding errors
                        row.ComputedHeight += remainder;
                        double roundedHeight = Math.Round(row.ComputedHeight);
                        remainder = row.ComputedHeight - roundedHeight;
                        row.ComputedHeight = roundedHeight;
                    }
                }
            }

            return desiredSize;
        }

        /// <summary>
        /// Get the set of rows spanned by the given cell
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private IEnumerable<NTableRow> GetRows(NTableCell cell)
        {
            for (int i = 0; i < cell.RowSpan; i++)
            {
                yield return Rows[cell.RowIndex + i];
            }
        }

        /// <summary>
        /// Get the set of columns spanned by the given cell
        /// </summary>
        private IEnumerable<NTableColumn> GetColumns(NTableCell cell)
        {
            for (int i = 0; i < cell.ColumnSpan; i++)
            {
                yield return Columns[cell.ColumnIndex + i];
            }
        }


        /// <summary>
        /// Get the columns spanned by cell whose width is set to "auto"
        /// </summary>
        private IEnumerable<NTableColumn> AutoColumns(NTableCell cell)
        {
            for (int i = 0; i < cell.ColumnSpan; i++)
            {
                NTableColumn column = Columns[cell.ColumnIndex + i];

                if (Double.IsNaN(column.Width))
                {
                    yield return column;
                }
            }
        }

        /// <summary>
        /// Measure the maximum width of the given cell
        /// </summary>
        private double MeasureWidth(NTableCell cell)
        {
            cell.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

            return cell.DesiredSize.Width;
        }

        /// <summary>
        /// Get the measured height of the given cell
        /// 
        /// This must be called AFTER the each column's ComputedWidth is set.
        /// </summary>
        private double MeasureHeight(NTableCell cell)
        {
            double availableCellWidth = ComputeWidth(cell);
            double availableCellHeight = Double.PositiveInfinity;

            cell.Measure(new Size(availableCellWidth, availableCellHeight));

            return cell.DesiredSize.Height;
        }

        /// <summary>
        /// Get the computed width of the given cell.
        /// 
        /// This is the sum of the computed width of each column spanned by the cell.
        /// </summary>
        private double ComputeWidth(NTableCell cell)
        {
            double result = 0;

            for (int i = 0; i < cell.ColumnSpan; i++)
            {
                NTableColumn column = Columns[cell.ColumnIndex + i];

                result += column.ComputedWidth;
            }

            return result;
        }

        /// <summary>
        /// Get the computed height of the given cell.
        /// 
        /// This is the sum of the computed height of each row spanned by the cell.
        /// </summary>
        private double ComputeHeight(NTableCell cell)
        {
            double result = 0;

            for (int i = 0; i < cell.RowSpan; i++)
            {
                NTableRow row = Rows[cell.RowIndex + i];

                result += row.ComputedHeight;
            }

            return result;
        }

        /// <summary>
        /// Get the computed size of the given cell.
        /// 
        /// This is the sum of the computed size of each row/column spanned by the cell.
        /// </summary>
        private Size ComputeSize(NTableCell cell)
        {
            double width = ComputeWidth(cell);
            double height = ComputeHeight(cell);

            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            base.ArrangeOverride(finalSize);

            Point offset = new Point(0, 0);

            // Set title size/position

            var titleRect = new Rect(0, 0, finalSize.Width, Title.DesiredSize.Height);
            Title.Arrange(titleRect);
            offset.Y += Title.RenderSize.Height;

            // Set size/position of each cell

            for (int i = 0; i < RowCount; i++)
            {
                offset.X = 0;

                for (int j = 0; j < ColumnCount; j++)
                {
                    NTableCell cell = Grid[i, j];

                    if (cell.RowIndex == i && cell.ColumnIndex == j)
                    {
                        Size cellSize = ComputeSize(cell);

                        cell.Arrange(new Rect(offset, cellSize));
                    }

                    offset.X += Columns[j].ComputedWidth;
                }

                offset.Y += Rows[i].ComputedHeight;
            }

            Size renderSize = new Size(offset.X, offset.Y);

            return renderSize;
        }

        /// <summary>
        /// Get this table's content rectangle.
        /// 
        /// A table's content rectangle is the region occupied by the table
        /// itself and does not include the table's title or menus.
        /// 
        /// This is valid only AFTER layout has completed.
        /// </summary>
        private Rect ContentRect
        {
            get
            {
                Rect rect = new Rect(RenderSize);

                rect.Y += Title.RenderSize.Height;
                rect.Height -= Title.RenderSize.Height;
                rect.Height = Math.Max(rect.Height, 0);

                return rect;
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            // Get the table geometry

            Rect rect = ContentRect;
            
            var geometry = DocumentPanel.GetBorderGeometry (
                rect,
                BorderLeftStyle, BorderLeftColor, BorderLeftWidth, BorderTopLeftRadius,
                BorderTopStyle, BorderTopColor, BorderTopWidth, BorderTopRightRadius,
                BorderRightStyle, BorderRightColor, BorderRightWidth, BorderBottomRightRadius,
                BorderBottomStyle, BorderBottomColor, BorderBottomWidth, BorderBottomLeftRadius
            );

            // Draw the table background

            dc.DrawGeometry(Background, null, geometry);

            // Draw the table borders (not individual cell borders)

            DrawBorders();

            // Clip child cells

            _CellsVisual.Clip = geometry;
        }

        private ContainerVisual _CellsVisual;

        private DrawingVisual _BordersDrawing;

        private void DrawBorders()
        {
            DrawingContext dc = _BordersDrawing.RenderOpen();

            DocumentPanel.DrawBorders(
                dc, ContentRect,
                BorderLeftStyle, BorderLeftColor, BorderLeftWidth, BorderTopLeftRadius,
                BorderTopStyle, BorderTopColor, BorderTopWidth, BorderTopRightRadius,
                BorderRightStyle, BorderRightColor, BorderRightWidth, BorderBottomRightRadius,
                BorderBottomStyle, BorderBottomColor, BorderBottomWidth, BorderBottomLeftRadius
            );

            dc.Close();
        }

        VisualCollection _Visuals;

        protected override int VisualChildrenCount
        {
            get { return _Visuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _Visuals[index];
        }

        #endregion

        #endregion
    }

    public class NTableTitle : TextParagraph
    {
        #region Constructors

        public NTableTitle()
        {
            Initialize();
        }

        public NTableTitle(NTableTitle copy)
            : base(copy)
        {
            //Initialize();
        }

        void Initialize()
        {
            Watermark = "Title";
            Padding = new Thickness(4);
            TextAlignment = TextAlignment.Center;
            SetProperty(TextProperties.FontSizeProperty, 18.667);
        }

        #endregion

        #region INodeSource

        public override string GetNodeName(NodeContext context)
        {
            return HTMLElements.CAPTION;
        }

        #endregion

        #region IStyleable

        public override CSSValue GetStyleProperty(ElementContext context, string propertyName)
        {
            switch (propertyName)
            {
                case CSSProperties.Display:
                    return CSSValues.TableCaption;
                default:
                    return base.GetStyleProperty(context, propertyName);
            }
        }

        #endregion

        #region ICloneable

        public override object Clone()
        {
            return new NTableTitle(this);
        }

        #endregion
    }

    public class NTableRowGroup : INodeSource
    {
        #region Fields

        private NTable _Table;
        private string _Name;

        #endregion

        #region Constructors

        public NTableRowGroup(NTable table, string name)
        {
            _Table = table;
            _Name = name;
        }

        #endregion

        #region Properties

        public NTable Table
        {
            get { return _Table; }
        }

        public IEnumerable<NTableRow> Rows
        {
            get { return Table.Where((row) => row.Group == this); }
        }

        #endregion

        #region Methods

        public NTableRow CreateRow()
        {
            return new NTableRow(this);
        }

        #endregion

        #region INodeSource

        public virtual NodeType GetNodeType(NodeContext context)
        {
            return NodeType.ELEMENT_NODE;
        }

        public virtual string GetNodeName(NodeContext context)
        {
            return _Name;
        }

        static readonly string[] _NodeAttributes = new string[0];

        public virtual IList<string> GetNodeAttributes(ElementContext context)
        {
            return _NodeAttributes;
        }

        public virtual string GetNodeAttribute(ElementContext context, string name)
        {
            return null;
        }

        public virtual void SetNodeAttribute(ElementContext context, string name, string value)
        {
        }

        public virtual void ResetNodeAttribute(ElementContext context, string name)
        {
        }

        public virtual object GetParentNode(NodeContext context)
        {
            return Table;
        }

        public virtual IEnumerable<object> GetChildNodes(NodeContext context)
        {
            return Rows;
        }

        public virtual object CreateNode(NodeContext context)
        {
            switch (context.NodeName)
            {
                case HTMLElements.TR:
                    return new NTableRow(this);
                default:
                    return null;
            }
        }

        public virtual void AppendNode(NodeContext context, object newChild)
        {
            Table.AppendNode(context, newChild);
        }

        public virtual void InsertNode(NodeContext context, object newChild, object refChild)
        {
            Table.InsertNode(context, newChild, refChild);
        }

        public virtual void RemoveNode(NodeContext context, object oldChild)
        {
            Table.RemoveNode(context, oldChild);
        }

        public event NodeEventHandler NodeEvent;

        protected void RaiseNodeEvent(IEventSource evt)
        {
            if (NodeEvent != null)
            {
                NodeEvent(evt);
            }
        }

        #endregion

    }

    public class NTableRow: INodeSource, IEnumerable<NTableCell>, ICloneable
    {
        #region Constructors

        public NTableRow(NTableRowGroup group)
        {
            Initialize(group);
        }

        public NTableRow(NTableRow other)
        {
            Initialize(other.Group);

            MinHeight = other.MinHeight;

            foreach (NTableCell cell in other.Cells)
            {
                var clone = (NTableCell)cell.Clone();
                clone.Row = this;
                Cells.Add(clone);
            }
        }

        void Initialize(NTableRowGroup group)
        {
            Group = group;
            MinHeight = 0;
            ComputedHeight = Double.NaN;
            Cells = new List<NTableCell>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The table row group to which this row belongs
        /// </summary>
        public NTableRowGroup Group { get; set; }

        /// <summary>
        /// True iff this is an auto-generated row
        /// </summary>
        public bool IsAutoGenerated { get; set; }

        /// <summary>
        /// This row's requested minimum height
        /// </summary>
        public double MinHeight { get; set; }

        /// <summary>
        /// The set of all cells within this row
        /// </summary>
        public List<NTableCell> Cells { get; set; }

        /// <summary>
        /// This row's computed height
        /// </summary>
        public double ComputedHeight { get; set; }

        #endregion

        #region Methods

        public NTableCell CreateCell(bool createParagraph)
        {
            return new NTableCell(this, createParagraph);
        }

        #endregion

        #region INodeSource

        public virtual NodeType GetNodeType(NodeContext context)
        {
            return NodeType.ELEMENT_NODE;
        }

        public virtual string GetNodeName(NodeContext context)
        {
            return HTMLElements.TR;
        }

        private readonly string[] _NodeAttributes = new string[] { };

        public virtual IList<string> GetNodeAttributes(ElementContext context)
        {
            return _NodeAttributes;
        }

        public virtual string GetNodeAttribute(ElementContext context, string name)
        {
            return null;
        }

        public virtual void SetNodeAttribute(ElementContext context, string name, string value)
        {
        }

        public virtual void ResetNodeAttribute(ElementContext context, string name)
        {
        }

        public virtual object GetParentNode(NodeContext context)
        {
            return Group.Table;
        }

        public virtual IEnumerable<object> GetChildNodes(NodeContext context)
        {
            return Cells;
        }

        public virtual object CreateNode(NodeContext context)
        {
            switch (context.NodeName)
            {
                case HTMLElements.TH:
                case HTMLElements.TD:
                    return CreateCell(false);
                default:
                    return null;
            }
        }

        public virtual void AppendNode(NodeContext context, object newChild)
        {
            Cells.Add((NTableCell)newChild);
        }

        public virtual void InsertNode(NodeContext context, object newChild, object refChild)
        {
            int index = Cells.IndexOf((NTableCell)refChild);
            if (index != -1)
            {
                Cells.Insert(index, (NTableCell)newChild);
            }
            else
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }
        }

        public virtual void RemoveNode(NodeContext context, object oldChild)
        {
            if (!Cells.Remove((NTableCell)oldChild))
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }
        }

        public event NodeEventHandler NodeEvent;

        protected void RaiseNodeEvent(IEventSource evt)
        {
            if (NodeEvent != null)
            {
                NodeEvent(evt);
            }
        }

        #endregion

        #region IEnumerable

        public IEnumerator<NTableCell> GetEnumerator()
        {
            return Cells.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Cells.GetEnumerator();
        }

        #endregion

        #region ICloneable

        public object Clone()
        {
            return new NTableRow(this);
        }

        #endregion
    }

    public class NTableColumn : ICloneable
    {
        #region Constructors

        public NTableColumn(NTable table)
        {
            Initialize(table);
        }

        public NTableColumn(NTableColumn other)
        {
            Initialize(other.Table);

            Width = other.Width;

            foreach (NTableCell cell in other.Cells)
            {
                var clone = (NTableCell)cell.Clone();
                Cells.Add(clone);
            }
        }

        void Initialize(NTable table)
        {
            Table = table;
            Width = double.NaN;
            Cells = new List<NTableCell>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The table to which this column belongs
        /// </summary>
        public NTable Table { get; set; }

        /// <summary>
        /// True iff this is an auto-generated row
        /// </summary>
        public bool IsAutoGenerated { get; set; }

        /// <summary>
        /// This column's requested width
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// The set of all cells within this row
        /// </summary>
        public List<NTableCell> Cells { get; set; }

        /// <summary>
        /// This column's computed width
        /// </summary>
        public double ComputedWidth { get; set; }

        #endregion

        #region ICloneable

        public object Clone()
        {
            return new NTableColumn(this);
        }

        #endregion
    }

    public class NTableCell : InteractivePanel, ICloneable
    {
        #region Constructors

        public NTableCell(NTableRow row, bool createParagraph = false)
        {
            Initialize(row);

            if (createParagraph)
            {
                Append(new TextParagraph());
            }
        }

        public NTableCell(NTableCell copy)
            : base(copy)
        {
            Initialize(copy.Row);

            ColumnIndex = copy.ColumnIndex;
            RowSpan = copy.RowSpan;
            ColumnSpan = copy.ColumnSpan;
        }

        void Initialize(NTableRow row)
        {
            Row = row;
            ColumnIndex = -1;
            RowSpan = 1;
            ColumnSpan = 1;

            Cursor = Cursors.IBeam;
            Padding = new Thickness(4);
            UndoStack = row.Group.Table.UndoStack;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The table row to which this cell belongs.
        /// </summary>
        public NTableRow Row { get; set; }

        /// <summary>
        /// The number of rows occupied by this cell
        /// </summary>
        public int RowSpan { get; set; }

        /// <summary>
        /// The number of columns occupied by this cell
        /// </summary>
        public int ColumnSpan { get; set; }

        /// <summary>
        /// The index of the row to which this cell belongs
        /// 
        /// Maintained by NTable.UpdateGrid() (by way of AddCellToGrid() and Normalize())
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// The index of the column to which this cell belongs
        /// 
        /// Maintained by NTable.UpdateGrid() (by way of AddCellToGrid() and Normalize())
        /// </summary>
        public int ColumnIndex { get; set; }

        #endregion

        #region ISelectable

        public override void Unselect()
        {
            // TODO: hide the selection decorator without actually unselecting it
            Selection.UnselectAll();
        }

        #endregion

        #region INodeSource

        public override string GetHTMLNodeName(NodeContext context)
        {
            return HTMLElements.TD;
        }

        private readonly string[] _HTMLAttributes = new string[] 
        { 
            HTMLAttributes.ROWSPAN, 
            HTMLAttributes.COLSPAN 
        };

        public override IList<string> GetHTMLAttributes(ElementContext context)
        {
            return base.GetHTMLAttributes(context).Concat(_HTMLAttributes).ToList();
        }

        public override string GetHTMLAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case HTMLAttributes.ROWSPAN:
                    if (RowSpan != 1)
                        return SafeConvert.ToString(RowSpan);
                    else
                        return null;
                case HTMLAttributes.COLSPAN:
                    if (ColumnSpan != 1)
                        return SafeConvert.ToString(ColumnSpan);
                    else
                        return null;
                default:
                    return base.GetHTMLAttribute(context, name);
            }
        }

        public override void SetHTMLAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case HTMLAttributes.ROWSPAN:
                    RowSpan = SafeConvert.ToInt32(value, 1);
                    break;
                case HTMLAttributes.COLSPAN:
                    ColumnSpan = SafeConvert.ToInt32(value, 1);
                    break;
                default:
                    base.SetHTMLAttribute(context, name, value);
                    break;
            }
        }

        public override void ResetHTMLAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case HTMLAttributes.ROWSPAN:
                    RowSpan = 1;
                    break;
                case HTMLAttributes.COLSPAN:
                    ColumnSpan = 1;
                    break;
                default:
                    base.ResetHTMLAttribute(context, name);
                    break;
            }
        }

        #endregion

        #region Implementation

        public static readonly new DependencyProperty BackgroundProperty = DocumentPanel.BackgroundProperty.AddOwner(
            typeof(NTableCell),
            new FrameworkPropertyMetadata(Brushes.Transparent)
        );

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (InternalChildren.Count == 0)
            {
                Append(new TextParagraph());
            }

            InternalChildren[0].Focus();

            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            e.Handled = true;
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            Point point = hitTestParameters.HitPoint;

            double borderWidth = 2;
            Rect bounds = new Rect(
                Math.Min(borderWidth, RenderSize.Width),
                Math.Min(borderWidth, RenderSize.Height),
                Math.Max(RenderSize.Width - 2 * borderWidth, 0),
                Math.Max(RenderSize.Height - 2 * borderWidth, 0)
            );
            if (bounds.Contains(point))
                return base.HitTestCore(hitTestParameters);
            else
                return null;
        }

        #endregion

        #region ICloneable

        public override object Clone()
        {
            return new NTableCell(this);
        }

        #endregion
    }

    public class TableAdorner : ResizingAdorner<NTable>
    {
        public TableAdorner(NTable adornedElement)
            : base(adornedElement)
        {

        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }
        
    }
}

