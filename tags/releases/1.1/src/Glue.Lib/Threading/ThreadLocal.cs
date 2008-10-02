using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web;

namespace Glue.Lib.Threading
{
    public class ThreadLocal<T> 
    {
        private static Hashtable _threadLocalContexts = new Hashtable();

        public static T Current
        {
            get
            {
                Thread thread = Thread.CurrentThread;
                int threadId = Thread.CurrentThread.ManagedThreadId;

                Log.Debug("ThreadLocal: Retrieving " + typeof(T).Name + " in thread " + threadId);

                ThreadLocal<T> context = (ThreadLocal<T>)_threadLocalContexts[threadId];

                if (context != null)
                    return context._value;
                else
                    return default(T);
            }
            set
            {
                // remove old key
                _threadLocalContexts.Remove(Thread.CurrentThread.ManagedThreadId);

                Log.Debug("ThreadLocal: Setting " + typeof(T).Name + " in thread " + Thread.CurrentThread.ManagedThreadId);

                if (value != null)
                {
                    // set
                    ThreadLocal<T> context = ThreadLocal<T>.Create(value);
                    _threadLocalContexts[Thread.CurrentThread.ManagedThreadId] = context;
                }
            }
        }

        private T _value;

        private static ThreadLocal<T> Create(T value)
        {
            ThreadLocal<T> tl = new ThreadLocal<T>();
            tl.Init(value);
            return tl;
        }

        private void Init(T value)
        {
            _value = value;

        }
    }
}
