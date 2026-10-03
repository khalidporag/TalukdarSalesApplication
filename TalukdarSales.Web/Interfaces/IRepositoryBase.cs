using System.Linq.Expressions;
using TalukdarSales.Web.Models;

namespace TalukdarSales.Web.Interfaces
{
    public interface IRepositoryBase<T> where T : class, IEntityBase, new()
    {
        IQueryable<T> AllIncluding(params Expression<Func<T, object>>[] includeProperties);
        /// <summary>Not-deleted rows. Deferred: compose filters, ordering, paging and projections on it so they run in SQL.</summary>
        IQueryable<T> GetAll();
        int Count();
        T GetSingle(int id);
        T GetSingle(Expression<Func<T, bool>> predicate);
        T GetSingle(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);
        /// <summary>Rows matching the predicate (deleted rows included, as before). Deferred.</summary>
        IQueryable<T> FindBy(Expression<Func<T, bool>> predicate);
        void Add(T entity);
        void AddRange(List<T> entities);
        void Update(T entity);
        void Delete(T entity);
        void DeleteWhere(Expression<Func<T, bool>> predicate);
        void Commit();
    }
}
