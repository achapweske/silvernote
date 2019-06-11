/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using SilverNote.Server;

namespace SilverNote.Client
{
    public class NoteClientThread : IDisposable
    {
        #region Fields

        Thread _Thread;
        ManualResetEvent _StartEvent;   // signalled when initialization complete
        protected ManualResetEvent _StopEvent;  // signalled when thread is to terminate
        ManualResetEvent _ResumeEvent;
        RequestQueue _Queue;
        Dictionary<string, string> _UriMap;
        ObservableCollection<Request> _Log;

        #endregion

        #region Constructors

        public NoteClientThread()
        {
            _StartEvent = new ManualResetEvent(false);
            _StopEvent = new ManualResetEvent(false);
            _ResumeEvent = new ManualResetEvent(false);
            _Queue = new RequestQueue();
            _UriMap = new Dictionary<string, string>();
            _Log = new ObservableCollection<Request>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Return true iff started
        /// </summary>
        public bool IsStarted
        {
            get { return _StartEvent.WaitOne(0) & !_StopEvent.WaitOne(0); }
        }

        /// <summary>
        /// Set the underlying thread priority
        /// </summary>
        public ThreadPriority Priority
        {
            get { return _Thread.Priority; }
            set { _Thread.Priority = value; }
        }

        /// <summary>
        /// Enable/disable logging
        /// </summary>
        public bool IsLogging { get; set; }

        /// <summary>
        /// Uri translation map
        /// </summary>
        public Dictionary<string, string> UriMap
        {
            get { return _UriMap; }
        }

        /// <summary>
        /// A log of all sent requests
        /// </summary>
        public ObservableCollection<Request> Log
        {
            get { return _Log; }
        }

        #endregion

        #region Operations

        /// <summary>
        /// Start the underlying thread
        /// </summary>
        public virtual void Start(object parameter)
        {
            if (_Thread != null)
            {
                if (_StopEvent.WaitOne(0))
                {
                    _Thread.Join();
                    _Thread = null;
                }
            }

            if (_Thread == null)
            {
                _StartEvent.Reset();
                _StopEvent.Reset();
                _ResumeEvent.Reset();
                _Thread = new Thread(this.ThreadFunc);
                _Thread.Priority = ThreadPriority.Highest;
                _Thread.Start(parameter);
                _StartEvent.WaitOne();
            }
        }

        /// <summary>
        /// Stop the underlying thread
        /// </summary>
        public virtual void Stop()
        {
            if (!_StopEvent.WaitOne(0))
            {
                _Thread.Priority = ThreadPriority.Highest;
                _StopEvent.Set();
                _Thread.Join();
                _Thread = null;
            }
        }

        /// <summary>
        /// Resume the underlying thread following an error
        /// </summary>
        public virtual void Resume(bool retry = false)
        {
            if (!retry && !_StopEvent.WaitOne(0))
            {
                _Queue.Pop();
            }

            _ResumeEvent.Set();
        }

        /// <summary>
        /// Send a request
        /// </summary>
        /// <param name="request"></param>
        public void Request(Request request)
        {
            if (IsLogging)
            {
                Log.Add(request);
            }

            _Queue.Push(request);
        }

        /// <summary>
        /// Cancel a request
        /// </summary>
        /// <param name="request"></param>
        public void Cancel(Request request)
        {
            _Queue.Remove(request.Method, request.Uri);
        }

        /// <summary>
        /// Determine if the given request is pending
        /// </summary>
        public bool IsPending(Request request)
        {
            return _Queue.Contains(request.Method, request.Uri);
        }

        /// <summary>
        /// Send a request
        /// </summary>
        public IAsyncResult BeginRequest(Request request, AsyncCallback callback, object state)
        {
            var result = new RequestResult(request, callback, state);

            Request(request);

            return result;
        }

        /// <summary>
        /// Get a response
        /// </summary>
        public Response EndRequest(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            var requestResult = asyncResult as RequestResult;
            if (requestResult == null)
            {
                throw new ArgumentException("The IAsyncResult object was not created by this class.", "asyncResult");
            }

            if (!requestResult.IsCompleted)
            {
                requestResult.AsyncWaitHandle.WaitOne();
            }

            requestResult.Dispose();

            var response = requestResult.Request.Response;

            if (!response.IsSuccess)
            {
                throw new NoteClientException(response);
            }

            return response;
        }

        #endregion

        #region Implementation

        private void ThreadFunc(object o)
        {
            // Start

            if (!OnStart(o))
            {
                _StopEvent.Set();
            }

            _StartEvent.Set();

            _Thread.Priority = ThreadPriority.Normal;

            // Request loop

            var waitHandles = new[] { _StopEvent, _Queue.WaitHandle };

            while (WaitHandle.WaitAny(waitHandles) != 0)
            {
                var request = _Queue.Peek();
                var response = ProcessRequest(request);

                _ResumeEvent.Reset();

                request.Complete(response);

                if (response.IsSuccess)
                {
                    _Queue.Pop();
                }
                else
                {   
                    WaitHandle.WaitAny(new [] { _ResumeEvent, _StopEvent });
                }
            }

            // Stop

            OnStop();
        }

        Stopwatch _RequestTimer = new Stopwatch();

        protected Response ProcessRequest(Request request)
        {
            Uri originalUri = request.Uri;

            // Keep track of how long it takes to process this request
            _RequestTimer.Restart();

            try
            {
                // Check that the request is valid

                if (request.Method == RequestMethod.Any || request.Uri == null)
                {
                    return Response.BadRequest();
                }
                
                request.Uri = TranslateUri(request.Uri);

                // Now actually process the request
                //
                // OnRequest() returns a response if the request was processed, or null if it was not

                var response = OnRequest(request);
                
                if (response == null)
                {
                    response = Response.NotImplemented();
                }

                return response;
            }
            finally
            {
                _RequestTimer.Stop();
                request.RoundTripTime = _RequestTimer.ElapsedMilliseconds;
                request.Uri = originalUri;
            }
        }

        protected Uri TranslateUri(Uri uri)
        {
            string oldUri = uri.ToString();

            foreach (var entry in UriMap)
            {
                string newUri = Regex.Replace(oldUri, entry.Key, entry.Value);
                if (newUri != oldUri)
                {
                    return new Uri(newUri);
                }
            }

            return uri;
        }

        protected virtual bool OnStart(object o)
        {
            return true;
        }

        protected virtual void OnStop()
        {
            Request request;

            while ((request = _Queue.Peek()) != null)
            {
                ProcessRequest(request);

                _Queue.Pop();
            }
        }

        protected virtual Response OnRequest(Request request)
        {
            return null;
        }

        #endregion

        #region IDisposable

        private bool _IsDisposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (_IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                Stop();
                _StartEvent.Dispose();
                _StopEvent.Dispose();
                _ResumeEvent.Dispose();
            }

            _IsDisposed = true;
        }

        #endregion
    }

    public class RequestQueue
    {
        #region Fields

        LinkedList<Request> _Queue = new LinkedList<Request>();
        ManualResetEvent _WaitHandle = new ManualResetEvent(false);

        #endregion

        #region Constructors

        public RequestQueue()
        {

        }

        #endregion

        #region Properties

        /// <summary>
        /// Signaled when the queue is NOT empty
        /// </summary>
        public WaitHandle WaitHandle
        {
            get { return _WaitHandle; }
        }

        #endregion

        #region Operations

        public void Push(Request request)
        {
            lock (this)
            {
                _Queue.AddFirst(request);
                _WaitHandle.Set();
            }
        }

        public Request Peek()
        {
            lock (this)
            {
                if (_Queue.Count() > 0)
                {
                    return _Queue.Last();
                }
                else
                {
                    return null;
                }
            }
        }

        public void Pop()
        {
            lock (this)
            {
                if (_Queue.Count() > 0)
                {
                    _Queue.RemoveLast();

                    if (_Queue.Count() == 0)
                    {
                        _WaitHandle.Reset();
                    }
                }
            }
        }

        public bool Contains(RequestMethod method, Uri uri)
        {
            lock (this)
            {
                foreach (var request in _Queue)
                {
                    if (request.Method == method && Uri.Equals(request.Uri, uri))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void Remove(RequestMethod method, Uri uri)
        {
            lock (this)
            {
                var node = _Queue.First;

                while (node != _Queue.Last)
                {
                    var nextNode = node.Next;

                    if (node.Value.Method == method && Uri.Equals(node.Value.Uri, uri))
                    {
                        _Queue.Remove(node);
                    }
                    node = nextNode;
                }
            }
        }

        public void Clear()
        {
            lock (this)
            {
                _Queue.Clear();
                _WaitHandle.Reset();
            }
        }

        #endregion
    }
}
