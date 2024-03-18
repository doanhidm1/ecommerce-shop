using Microsoft.EntityFrameworkCore.Storage;

namespace Domain.Abstractions
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
