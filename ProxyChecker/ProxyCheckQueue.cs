using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvancedBot.client;
using System.Threading;

namespace AdvancedBot.ProxyChecker
{
    public class ProxyCheckQueue : IDisposable
    {
        private enum ItemState
        {
            Created = 0,
            Checking = 1,
            CheckFinished = 2,
            WaitingForNewItem = 3,
            QueuedNewItem = 4,
            Finalized = 5
        }
        private class QueueItem
        {
            public ProxyInfo Proxy;
            public Thread Thread;
            public ItemState State = ItemState.Created;
            public object SyncObj = new object();
            public bool WaitForNew = true;
            public long StartTime;

            public QueueItem(ProxyInfo p)
            {
                Proxy = p;

                State = ItemState.Created;

                Thread = new Thread(ThreadDelegate);
                Thread.IsBackground = true;
                Thread.Priority = ThreadPriority.BelowNormal;
                Thread.Start();

                ResetTime();
            }
            public void ResetTime()
            {
                StartTime = Utils.GetTimestamp();
            }

            private void ThreadDelegate()
            {
                while (WaitForNew) {
                    lock (SyncObj) {
                        if (State != ItemState.Created && State != ItemState.QueuedNewItem) {
                            goto sleepAndContinue;
                        }

                        State = ItemState.Checking;
                    }

                    if (!ProxyUtils.CheckProxy(Proxy)) {
                        Proxy.Type = (ProxyType)0;
                    }
                    lock (SyncObj) State = ItemState.CheckFinished;

                sleepAndContinue:
                    Thread.Sleep(50);
                }
            }
        }

        private List<QueueItem> queue = new List<QueueItem>();
        public int QueueSize { get { return queue.Count; } }
        public int FreeThreadCount
        {
            get {
                lock (queue) {
                    int n = 0;
                    foreach (QueueItem qi in queue) {
                        lock (qi.SyncObj) {
                            if (qi.State == ItemState.WaitingForNewItem)
                                n++;
                        }
                    }
                    return n;
                }
            }
        }

        public string TestIP;
        public ushort TestPort;

        public void Add(ProxyInfo p)
        {
            lock (queue) {
                foreach (QueueItem qi in queue) {
                    lock (qi.SyncObj) {
                        if (qi.State == ItemState.WaitingForNewItem) {
                            qi.Proxy = p;
                            qi.State = ItemState.QueuedNewItem;
                            qi.ResetTime();
                            return;
                        }
                    }
                }

                queue.Add(new QueueItem(p));
            }
        }
        public List<ProxyInfo> GetFinished(int maxRuntime)
        {
            long now = Utils.GetTimestamp();
            List<ProxyInfo> dat = new List<ProxyInfo>();
            lock (queue) {
                for (int i = 0; i < queue.Count; i++) {
                    QueueItem t = queue[i];

                    lock (t.SyncObj) {
                        if (now - t.StartTime > maxRuntime && t.State == ItemState.Checking) {
                            try {
                                t.WaitForNew = false;
                                t.State = ItemState.Finalized;
                                t.Thread.Abort();
                            } catch { }
                        }

                        if (t.State == ItemState.CheckFinished || t.State == ItemState.Finalized) {
                            dat.Add(t.Proxy);
                            if (t.State == ItemState.Finalized) {
                                queue.RemoveAt(i--);
                            } else {
                                t.Proxy = null;
                                t.State = ItemState.WaitingForNewItem;
                            }
                        }
                    }
                }
            }
            return dat;
        }
        ~ProxyCheckQueue()
        {
            Dispose();
        }
        public void Dispose()
        {
            lock (queue) {
                for (int p = 0; p < 2; p++) {
                    foreach (QueueItem ci in queue) {
                        if (p == 0) {
                            ci.WaitForNew = false;
                            ci.State = ItemState.Finalized;
                        } else if (ci.Thread != null && ci.Thread.IsAlive) {
                            try {
                                ci.Thread.Join(120);
                            } catch { }
                            try {
                                ci.Thread.Abort();
                            } catch { }
                        }
                    }
                }
                queue.Clear();
            }
        }
    }
}
