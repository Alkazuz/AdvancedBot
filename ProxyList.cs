using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using AdvancedBot.client;
using AdvancedBot.ProxyChecker;

namespace AdvancedBot
{
    public class ProxyList : IEnumerable<ProxyInfo>
    {
        public List<ProxyInfo> _list = new List<ProxyInfo>();

        private int index;

        public int Count { get { return _list.Count; } }

        public void Add(ProxyInfo proxy)
        {
            _list.Add(proxy);
        }
        public void AddRange(IEnumerable<ProxyInfo> _enum)
        {
            _list.AddRange(_enum);
        }
        public bool HasAny() { return _list.Count > 0; }

        public ProxyInfo Next()
        {
            return _list[index++ % _list.Count];
        }
        public Proxy NextProxy()
        {
            int count = _list.Count;
            if (count > 0) {
                ProxyInfo p = _list[index++ % count];
                return new Proxy(p.IP, p.Port, p.Type);
            } else {
                return null;
            }
        }

        public void Clear()
        {
            _list.Clear();
            index = 0;
        }
        public void ResetIndexes()
        {
            index = 0;
        }

        public IEnumerator<ProxyInfo> GetEnumerator()
        {
            return _list.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}
