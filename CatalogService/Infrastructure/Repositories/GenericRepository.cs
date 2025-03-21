using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CatalogService.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository where T : BaseEntity
    {
        private readonly CatalogDbContext _context;
        private DbSet<T> _dbSet;
        private IDbContextTransaction _transaction;

        public GenericRepository(CatalogDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<T> GetByIdAsync(int id) => await _db.FindAsync(id);

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }            
        }

        public async Task UpdateStatusAsync(int id, bool status)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                entity.IsActive = status;
                await _context.SaveChangesAsync();
            }
        }

    }
}
