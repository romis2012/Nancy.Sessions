﻿using System;
using Nancy.Bootstrapper;

namespace Nancy.Session
{
    public class SessionManager
    {
        private readonly ISessionProvider _provider;
        public event Action<ISession> SessionStart;

        public SessionManager(ISessionProvider provider)
        {
            _provider = provider;
        }

        public void Run(IPipelines pipelines)
        {
            pipelines.BeforeRequest.AddItemToStartOfPipeline(LoadSession);
            pipelines.AfterRequest.AddItemToEndOfPipeline(SaveSession);
            if (SessionStart != null)
            {
                pipelines.BeforeRequest.AddItemToEndOfPipeline(context =>
                {
                    if (_provider.Expired(context))
                    {
                        SessionStart(context.Request.Session);
                    }
                    return null;
                });
            }
        }

        private void SaveSession(NancyContext context)
        {
            _provider.Save(context);
        }

        private Response LoadSession(NancyContext context)
        {
            if (context.Request == null)
            {
                return null;
            }

            context.Request.Session = _provider.Load(context);

            return null;
        }
    }
}