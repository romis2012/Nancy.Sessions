using System;

namespace Nancy.Session
{
    public interface ISessionProvider
    {
        void Save(NancyContext context);
        ISession Load(NancyContext context);
        event Action<ISession> SessionStart;
        event Action<ISession> SessionEnd;
    }
}