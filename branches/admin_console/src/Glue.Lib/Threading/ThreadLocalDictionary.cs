using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Glue.Lib.Threading
{
    public class ThreadLocalDictionary
    {
        private static Hashtable _threadLocalContexts = new Hashtable();

        public static Dictionary<string, object> Current
        {
            get
            {
                Thread thread = Thread.CurrentThread;
                int threadId = Thread.CurrentThread.ManagedThreadId;

                // Log.Debug("ThreadLocalDictionary: Retrieving dictionary in thread " + threadId);

                Dictionary<string, object> context = (Dictionary<string, object>)_threadLocalContexts[threadId];

                if (context == null)
                {
                    context = new Dictionary<string, object>();
                    _threadLocalContexts[threadId] = context;
                }

                return context;
            }
        }
    }
}
