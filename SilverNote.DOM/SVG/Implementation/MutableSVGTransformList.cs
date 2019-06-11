/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.SVG.Internal
{
    public class MutableSVGTransformList : SVGTransformList
    {
        #region Fields

        List<SVGTransform> _Items;

        #endregion

        #region Constructors

        public MutableSVGTransformList()
        {
            _Items = new List<SVGTransform>();
        }

        #endregion

        #region SVGTransformList

        public virtual int NumberOfItems
        {
            get { return _Items.Count; }
        }

        public virtual void Clear()
        {
            _Items.Clear();
        }

        public SVGTransform Initialize(SVGTransform newItem)
        {
            Clear();
            AppendItem(newItem);
            return newItem;
        }

        public virtual SVGTransform GetItem(int index)
        {
            if (index < 0 || index >= _Items.Count)
            {
                throw new DOMException(DOMException.INDEX_SIZE_ERR);
            }

            return _Items[index];
        }

        public virtual SVGTransform InsertItemBefore(SVGTransform newItem, int index)
        {
            if (index >= _Items.Count)
            {
                _Items.Add(newItem);
            }
            else if (index < 0)
            {
                _Items.Insert(0, newItem);
            }
            else
            {
                _Items.Insert(index, newItem);
            }

            return newItem;
        }

        public SVGTransform ReplaceItem(SVGTransform newItem, int index)
        {
            RemoveItem(index);
            InsertItemBefore(newItem, index);
            return newItem;
        }

        public virtual SVGTransform RemoveItem(int index)
        {
            if (index < 0 || index >= _Items.Count)
            {
                throw new DOMException(DOMException.INDEX_SIZE_ERR);
            }

            var removedItem = _Items[index];
            _Items.RemoveAt(index);

            return removedItem;
        }

        public virtual SVGTransform AppendItem(SVGTransform newItem)
        {
            _Items.Add(newItem);
            return newItem;
        }

        public SVGTransform CreateSVGTransformFromMatrix(SVGMatrix matrix)
        {
            var transform = new MutableSVGTransform();
            transform.SetMatrix(matrix);
            return transform;
        }

        public SVGTransform Consolidate()
        {
            SVGMatrix matrix = null;

            for (int i = 0; i < NumberOfItems; i++)
            {
                if (matrix == null)
                {
                    matrix = GetItem(i).Matrix;
                }
                else
                {
                    matrix = matrix.Multiply(GetItem(i).Matrix);
                }
            }

            SVGTransform result = CreateSVGTransformFromMatrix(matrix);
            Initialize(result);
            return result;
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return SVGFormatter.FormatTransformList(this);
        }

        #endregion
    }
}
