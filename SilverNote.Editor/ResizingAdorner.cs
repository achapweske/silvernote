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
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

namespace SilverNote.Editor
{
    public class ResizingAdorner<T> : Adorner where T: UIElement
    {
        public ResizingAdorner(T target)
            : base(target)
        {
            Children = new VisualCollection(this);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            NWThumb = CreateHandleThumb(Cursors.SizeNWSE);
            NEThumb = CreateHandleThumb(Cursors.SizeNESW);
            SWThumb = CreateHandleThumb(Cursors.SizeNESW);
            SEThumb = CreateHandleThumb(Cursors.SizeNWSE);
            MoverThumb = CreateMoverThumb(Cursors.SizeAll);
        }

        public new T AdornedElement
        {
            get { return (T)base.AdornedElement; }
        }

        public virtual bool MoveStarted()
        {
            var e = new RoutedEventArgs(Movable.RequestingBeginMove);
            AdornedElement.RaiseEvent(e);
            return e.Handled;
        }

        public virtual bool MoveDelta(Vector delta)
        {
            var e = new MoveDeltaEventArgs(Movable.RequestingMoveDelta, delta);
            AdornedElement.RaiseEvent(e);
            return e.Handled;
        }

        public virtual bool MoveCompleted()
        {
            var e = new RoutedEventArgs(Movable.RequestingEndMove);
            AdornedElement.RaiseEvent(e);
            return e.Handled;
        }

        public virtual bool Resize(Vector delta)
        {
            IResizable resizable = AdornedElement as IResizable;
            if (resizable == null)
            {
                return false;
            }

            resizable.Resize(delta);
            return true;
        }

        private Thumb nwThumb;
        public Thumb NWThumb 
        {
            get { return nwThumb; }
            set
            {
                if (nwThumb != null)
                {
                    nwThumb.DragStarted -= new DragStartedEventHandler(NWThumb_DragStarted);
                    nwThumb.DragDelta -= new DragDeltaEventHandler(NWThumb_DragDelta);
                    nwThumb.DragCompleted -= new DragCompletedEventHandler(NWThumb_DragCompleted);

                    Children.Remove(nwThumb);
                }
                nwThumb = value;
                if (nwThumb != null)
                {
                    Children.Add(nwThumb);

                    nwThumb.DragStarted += new DragStartedEventHandler(NWThumb_DragStarted);
                    nwThumb.DragDelta += new DragDeltaEventHandler(NWThumb_DragDelta);
                    nwThumb.DragCompleted += new DragCompletedEventHandler(NWThumb_DragCompleted);
                }
            }
        }

        private Thumb neThumb;
        public Thumb NEThumb
        {
            get { return neThumb; }
            set
            {
                if (neThumb != null)
                {
                    neThumb.DragStarted -= new DragStartedEventHandler(NEThumb_DragStarted);
                    neThumb.DragDelta -= new DragDeltaEventHandler(NEThumb_DragDelta);
                    neThumb.DragCompleted -= new DragCompletedEventHandler(NEThumb_DragCompleted);

                    Children.Remove(neThumb);
                }
                neThumb = value;
                if (neThumb != null)
                {
                    Children.Add(neThumb);

                    neThumb.DragStarted += new DragStartedEventHandler(NEThumb_DragStarted);
                    neThumb.DragDelta += new DragDeltaEventHandler(NEThumb_DragDelta);
                    neThumb.DragCompleted += new DragCompletedEventHandler(NEThumb_DragCompleted);
                }
            }
        }

        private Thumb swThumb;
        public Thumb SWThumb
        {
            get { return swThumb; }
            set
            {
                if (swThumb != null)
                {
                    swThumb.DragStarted -= new DragStartedEventHandler(SWThumb_DragStarted);
                    swThumb.DragDelta -= new DragDeltaEventHandler(SWThumb_DragDelta);
                    swThumb.DragCompleted -= new DragCompletedEventHandler(SWThumb_DragCompleted);

                    Children.Remove(swThumb);
                }
                swThumb = value;
                if (swThumb != null)
                {
                    Children.Add(swThumb);

                    swThumb.DragStarted += new DragStartedEventHandler(SWThumb_DragStarted);
                    swThumb.DragDelta += new DragDeltaEventHandler(SWThumb_DragDelta);
                    swThumb.DragCompleted += new DragCompletedEventHandler(SWThumb_DragCompleted);
                }
            }
        }

        private Thumb seThumb;
        public Thumb SEThumb
        {
            get { return seThumb; }
            set
            {
                if (seThumb != null)
                {
                    seThumb.DragStarted -= new DragStartedEventHandler(SEThumb_DragStarted);
                    seThumb.DragDelta -= new DragDeltaEventHandler(SEThumb_DragDelta);
                    seThumb.DragCompleted -= new DragCompletedEventHandler(SEThumb_DragCompleted);

                    Children.Remove(seThumb);
                }
                seThumb = value;
                if (seThumb != null)
                {
                    Children.Add(seThumb);

                    seThumb.DragStarted += new DragStartedEventHandler(SEThumb_DragStarted);
                    seThumb.DragDelta += new DragDeltaEventHandler(SEThumb_DragDelta);
                    seThumb.DragCompleted += new DragCompletedEventHandler(SEThumb_DragCompleted);
                }
            }
        }

        private Thumb moverThumb;
        public Thumb MoverThumb
        {
            get { return moverThumb; }
            set
            {
                if (moverThumb != null)
                {
                    moverThumb.DragStarted -= new DragStartedEventHandler(MoverThumb_DragStarted);
                    moverThumb.DragDelta -= new DragDeltaEventHandler(MoverThumb_DragDelta);
                    moverThumb.DragCompleted -= new DragCompletedEventHandler(MoverThumb_DragCompleted);
                    
                    Children.Remove(moverThumb);
                }
                moverThumb = value;
                if (moverThumb != null)
                {
                    Children.Add(moverThumb);

                    moverThumb.DragStarted += new DragStartedEventHandler(MoverThumb_DragStarted);
                    moverThumb.DragDelta += new DragDeltaEventHandler(MoverThumb_DragDelta);
                    moverThumb.DragCompleted += new DragCompletedEventHandler(MoverThumb_DragCompleted);
                }
            }
        }
        
        protected virtual Thumb CreateHandleThumb(Cursor cursor)
        {
            Thumb result = new Thumb();
            result.SnapsToDevicePixels = true;
            result.Cursor = cursor;
            result.Width = 5;
            result.Height = 5;
            result.Background = Brushes.White;
            result.BorderThickness = new Thickness(1);
            result.BorderBrush = Brushes.Black;
            result.Template = TryFindResource("HandleThumb") as ControlTemplate;
            return result;
        }

        protected virtual Thumb CreateMoverThumb(Cursor cursor)
        {
            Thumb result = new Thumb();
            result.SnapsToDevicePixels = true;
            result.Cursor = cursor;
            result.Width = 16;
            result.Height = 16;
            result.Background = Brushes.White;
            result.BorderThickness = new Thickness(1);
            result.BorderBrush = Brushes.Black;
            result.Template = TryFindResource("MoverThumb") as ControlTemplate;
            return result;
        }

        protected virtual void ArrangeNWThumb(Size finalSize)
        {
            if (NEThumb != null)
            {
                NWThumb.Arrange(new Rect(
                    -NWThumb.DesiredSize.Width / 2,
                    -NWThumb.DesiredSize.Height / 2,
                    NWThumb.DesiredSize.Width,
                    NWThumb.DesiredSize.Height
                ));
            }
        }

        protected virtual void ArrangeNEThumb(Size finalSize)
        {
            if (NEThumb != null)
            {
                NEThumb.Arrange(new Rect(
                    finalSize.Width - NEThumb.DesiredSize.Width / 2 + 0.5,
                    -NEThumb.DesiredSize.Height / 2,
                    NEThumb.DesiredSize.Width,
                    NEThumb.DesiredSize.Height
                ));
            }
        }

        protected virtual void ArrangeSWThumb(Size finalSize)
        {
            if (SWThumb != null)
            {
                SWThumb.Arrange(new Rect(
                    -SWThumb.DesiredSize.Width / 2,
                    finalSize.Height - SWThumb.DesiredSize.Height / 2 + 0.5,
                    SWThumb.DesiredSize.Width,
                    SWThumb.DesiredSize.Height
                ));
            }
        }

        protected virtual void ArrangeSEThumb(Size finalSize)
        {
            if (SEThumb != null)
            {
                SEThumb.Arrange(new Rect(
                    finalSize.Width - SEThumb.DesiredSize.Width / 2 + 0.5,
                    finalSize.Height - SEThumb.DesiredSize.Height / 2 + 0.5,
                    SEThumb.DesiredSize.Width,
                    SEThumb.DesiredSize.Height
                ));
            }
        }

        protected virtual void ArrangeMoverThumb(Size finalSize)
        {
            if (MoverThumb != null)
            {
                MoverThumb.Arrange(new Rect(
                    finalSize.Width + 5,
                    finalSize.Height + 5,
                    MoverThumb.Width,
                    MoverThumb.Height
                ));
            }
        }

        protected void OnDragStartedNWThumb()
        {
            MoveStarted();
        }

        protected void OnDragDeltaNWThumb(Vector delta)
        {
            if (MoveDelta(delta))
            {
                Resize(-delta);
            }
        }

        protected void OnDragCompletedNWThumb()
        {
            MoveCompleted();
        }

        protected void OnDragStartedNEThumb()
        {
            MoveStarted();
        }

        protected void OnDragDeltaNEThumb(Vector delta)
        {
            if (MoveDelta(new Vector(0, delta.Y)))
            {
                Resize(new Vector(delta.X, -delta.Y));
            }
            else
            {
                Resize(new Vector(delta.X, 0));
            }
        }

        protected void OnDragCompletedNEThumb()
        {
            MoveCompleted();
        }

        protected void OnDragStartedSWThumb()
        {
            MoveStarted();
        }

        protected void OnDragDeltaSWThumb(Vector delta)
        {
            if (MoveDelta(new Vector(delta.X, 0)))
            {
                Resize(new Vector(-delta.X, delta.Y));
            }
            else
            {
                Resize(new Vector(0, delta.Y));
            }
        }

        protected void OnDragCompletedSWThumb()
        {
            MoveCompleted();
        }

        protected void OnDragStartedSEThumb()
        {
            MoveStarted();
        }

        protected void OnDragDeltaSEThumb(Vector delta)
        {
            Resize(delta);
        }

        protected void OnDragCompletedSEThumb()
        {
            MoveCompleted();
        }

        protected void OnDragStartedMoverThumb()
        {
            MoveStarted();
        }

        protected void OnDragDeltaMoverThumb(Vector delta)
        {
            MoveDelta(delta);
        }

        protected void OnDragCompletedMoverThumb()
        {
            MoveCompleted();
        }

        #region Implementation

        private void NWThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            OnDragStartedNWThumb();
        }

        private void NWThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            OnDragDeltaNWThumb(new Vector(e.HorizontalChange, e.VerticalChange));
        }

        private void NWThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            OnDragCompletedNWThumb();
        }

        private void NEThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            OnDragStartedNEThumb();
        }

        private void NEThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            OnDragDeltaNEThumb(new Vector(e.HorizontalChange, e.VerticalChange));
        }

        private void NEThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            OnDragCompletedNEThumb();
        }

        private void SWThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            OnDragStartedSWThumb();
        }

        private void SWThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            OnDragDeltaSWThumb(new Vector(e.HorizontalChange, e.VerticalChange));
        }

        private void SWThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            OnDragCompletedSWThumb();
        }

        private void SEThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            OnDragStartedSEThumb();
        }

        private void SEThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            OnDragDeltaSEThumb(new Vector(e.HorizontalChange, e.VerticalChange));
        }

        private void SEThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            OnDragCompletedSEThumb();
        }

        private void MoverThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            OnDragStartedMoverThumb();
        }

        private void MoverThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            OnDragDeltaMoverThumb(new Vector(e.HorizontalChange, e.VerticalChange));
        }

        private void MoverThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            OnDragCompletedMoverThumb();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            base.MeasureOverride(availableSize);

            return AdornedElement.RenderSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            ArrangeNWThumb(finalSize);
            ArrangeNEThumb(finalSize);
            ArrangeSWThumb(finalSize);
            ArrangeSEThumb(finalSize);
            ArrangeMoverThumb(finalSize);

            return finalSize;
        }

        protected VisualCollection Children { get; private set; }

        protected override int VisualChildrenCount
        {
            get { return Children.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return Children[index];
        }

        #endregion
    }
}
