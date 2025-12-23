using System.Linq.Expressions;

namespace SoruCevapPortalı.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // İlişkili tabloları getirebilmek için 'includeProperties' eklendi
        Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities); // Çoklu silme için
    }
}