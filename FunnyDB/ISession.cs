using System.Data;

namespace FunnyDB
{
    public interface ISession
    {
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; }
        void Commit();
        void Rollback();
    }
}