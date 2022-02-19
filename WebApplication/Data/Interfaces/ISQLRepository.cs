using System.Linq;

namespace WebApplication.Data.Interfaces
{
    public interface ISQLRepository<T>
    {
        public IQueryable<T> GetAll();

        public void Insert(T newEntity);
    }
}
