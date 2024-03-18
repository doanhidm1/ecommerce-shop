using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace Persistence
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly ShopDBContext _context;
        public EfUnitOfWork(ShopDBContext context)
        {
            _context = context;
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            if (_context.Database.CurrentTransaction != null)
            {
                return null;
            }
            return await _context.Database.BeginTransactionAsync();
        }

        //public async Task CommitAsync()
        //{
        //    if (_context.Database.CurrentTransaction == null)
        //    {
        //        return;
        //    }
        //    await _context.Database.CommitTransactionAsync();
        //}

        //public async Task RollbackAsync()
        //{
        //    if (_context.Database.CurrentTransaction == null)
        //    {
        //        return;
        //    }
        //    await _context.Database.RollbackTransactionAsync();
        //}

        //public async ValueTask DisposeAsync()
        //{
        //    await _context.DisposeAsync();
        //}
    }
}
