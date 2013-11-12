namespace Nancy.Session
{
    public interface ISessionStore
    {
        void Save(NancyContext context);
        ISession Load(NancyContext context);
        bool Expired(NancyContext context);
    }
}