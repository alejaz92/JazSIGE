using AccountingService.Infrastructure.Data;
using JazSIGE.Accounting.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AccountingDbContext _ctx;
        protected readonly DbSet<T> _db;

        public GenericRepository(AccountingDbContext ctx)
        {
            _ctx = ctx;
            _db = ctx.Set<T>();
        }

        public IQueryable<T> Query() => _db.AsQueryable();

        public virtual async Task<T?> GetByIdAsync(int id) => await _db.FindAsync(id);

        public virtual async Task AddAsync(T entity) => await _db.AddAsync(entity);

        public virtual void Update(T entity) => _db.Update(entity);

        public virtual void Remove(T entity) => _db.Remove(entity);
    }
}
