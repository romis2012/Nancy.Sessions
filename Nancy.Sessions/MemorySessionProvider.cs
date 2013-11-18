using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Nancy.Cookies;

namespace Nancy.Session
{
    public class MemorySessionProvider : ISessionProvider
    {
        private readonly ObjectCache _cache = MemoryCache.Default;
        public string CookieName { get; set; }
        public int Timeout { get; set; }

        public event Action<ISession> SessionStart;

        protected virtual void OnSessionStart(ISession obj)
        {
            Action<ISession> handler = SessionStart;
            if (handler != null)
            {
                handler(obj);
            }
        }

        public event Action<ISession> SessionEnd;
        protected virtual void OnSessionEnd(ISession obj)
        {
            Action<ISession> handler = SessionEnd;
            if (handler != null)
            {
                handler(obj);
            }
        }

        public MemorySessionProvider(string cookieName, int timeout)
        {
            CookieName = cookieName;
            Timeout = timeout;
        }

        public MemorySessionProvider(): this("_sessionId", 20)
        {
        }

        public void Save(NancyContext context)
        {
            var session = context.Request.Session;
            if (session == null || !session.HasChanged)
            {
                return;
            }

            var sessionId = SessionIdFromCookie(context);

            if (String.IsNullOrEmpty(sessionId))
            {
                sessionId = NewSessionId();
                var cookie = new NancyCookie(CookieName, sessionId, true);
                context.Response.AddCookie(cookie);
            }

            var dict = new Dictionary<string, object>(session.Count);
            foreach (var kvp in session)
            {
                dict.Add(kvp.Key, kvp.Value);
            }
            SaveToStore(sessionId, dict);
        }

        public ISession Load(NancyContext context)
        {
            var sessionId = SessionIdFromCookie(context);

            if (String.IsNullOrEmpty(sessionId) || !InStore(sessionId))
            {
                var session = new Session();
                OnSessionStart(session);
                return session;
            }

            return new Session(LoadFromStore(sessionId));
        }

        private IDictionary<string, object> LoadFromStore(string key)
        {
            var val = _cache[key] as IDictionary<string, object>;
            return val ?? new Dictionary<string, object>();
        }

        private void SaveToStore(string key, IDictionary<string, object> value)
        {
            var policy = new CacheItemPolicy {SlidingExpiration = new TimeSpan(0, 0, Timeout, 0)};
            policy.RemovedCallback += args =>
            {
                if (args.RemovedReason == CacheEntryRemovedReason.Expired)
                {
                    var item = args.CacheItem.Value as IDictionary<string, object>;
                    if (item != null)
                    {
                        OnSessionEnd(new Session(item));
                    }
                }
            };
            _cache.Set(key, value, policy);
        }


        private bool InStore(string sessionId)
        {
            return _cache.Contains(sessionId) && _cache[sessionId] != null;
        }

        private string SessionIdFromCookie(NancyContext context)
        {
            if (context.Request.Cookies.ContainsKey(CookieName))
            {
                return context.Request.Cookies[CookieName];
            }
            return null;
        }

        private string NewSessionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}