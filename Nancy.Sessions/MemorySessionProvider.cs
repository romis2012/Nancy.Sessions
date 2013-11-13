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

            string sessionId;

            if (CookiePassed(context))
            {
                sessionId = SessionIdFromCookie(context);
            }
            else
            {
                sessionId = NewSessionId();
                var cookie = new NancyCookie(CookieName, sessionId, true);
                context.Response.AddCookie(cookie);
            }

            //todo: consider use custom ISession implementation
            var dict = new Dictionary<string, object>(session.Count);
            foreach (var kvp in session)
            {
                dict.Add(kvp.Key, kvp.Value);
            }
            SaveToStore(sessionId, dict);
        }

        public ISession Load(NancyContext context)
        {
            if (!CookiePassed(context))
            {
                return new Session();
            }

            var sessionId = SessionIdFromCookie(context);
            var session = LoadFromStore(sessionId);
            return new Session(session);
        }

        public bool Expired(NancyContext context)
        {
            return !CookiePassed(context) || _cache[SessionIdFromCookie(context)] == null;
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


        private bool CookiePassed(NancyContext context)
        {
            return context.Request.Cookies.ContainsKey(CookieName);
        }

        private string SessionIdFromCookie(NancyContext context)
        {
            return context.Request.Cookies[CookieName];
        }

        private string NewSessionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}