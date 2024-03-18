namespace Domain.Abstractions
{
    public interface IRepository<TEntity, TKey> where TEntity : class
    {
        IQueryable<TEntity> GetAll();
        Task Add(TEntity entity);
        Task Update(TEntity entity);
        Task Delete(TEntity entity);
        Task<TEntity?> FindById(TKey id);
    }
}
