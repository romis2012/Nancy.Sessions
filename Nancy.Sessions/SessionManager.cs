using System;
using Nancy.Bootstrapper;

namespace Nancy.Session
{
    public class SessionManager
    {
        private readonly ISessionStore _store;
        public event Action<ISession> SessionStart;

        public SessionManager(ISessionStore store)
        {
            _store = store;
        }

        public void Run(IPipelines pipelines)
        {
            pipelines.BeforeRequest.AddItemToStartOfPipeline(LoadSession);
            pipelines.AfterRequest.AddItemToEndOfPipeline(SaveSession);
            if (SessionStart != null)
            {
                pipelines.BeforeRequest.AddItemToEndOfPipeline(context =>
                {
                    if (_store.Expired(context))
                    {
                        SessionStart(context.Request.Session);
                    }
                    return null;
                });
            }
        }

        private void SaveSession(NancyContext context)
        {
            _store.Save(context);
        }

        private Response LoadSession(NancyContext context)
        {
            if (context.Request == null)
            {
                return null;
            }

            context.Request.Session = _store.Load(context);

            return null;
        }
    }
}