using System;

namespace Nancy.Session
{
    public interface ISessionProvider
    {
        void Save(NancyContext context);
        ISession Load(NancyContext context);
        bool Expired(NancyContext context);

        event Action<ISession> SessionEnd;
    }
}