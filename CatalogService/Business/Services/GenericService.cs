using CatalogService.Business.Interfaces;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using System.Diagnostics;

namespace CatalogService.Business.Services
{
    public abstract class GenericService<T, TDto, TCreateDto> : IGenericService<T, TDto, TCreateDto>
            where T : BaseEntity
    {
        protected readonly IGenericRepository<T> _repository;
        public GenericService(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TDto>> GetAllAsync()
        {
            var entities = await GetAllWithIncludes();
            return entities.Select(MapToDTO).ToList();
        }
        public async Task<TDto> GetByIdAsync(int id)
        {
            var entity = await GetWithIncludes(id);
            return entity != null ? MapToDTO(entity) : default(TDto);
        }
        public async Task<TDto> CreateAsync(TCreateDto model)
        {

            var validationError = await ValidateBeforeSave(model);
            if (validationError != null)
            {
                throw new Exception(validationError);
            }


            var entity = MapToDomain(model);
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
            return MapToDTO(await GetWithIncludes(entity.Id));
        }
        public async Task<TDto> UpdateAsync(int id, TCreateDto model)
        {
            var entity = await GetWithIncludes(id);
            if (entity == null)
            {
                return default(TDto);
            }
            UpdateDomain(entity, model);
            _repository.Update(entity);
            await _repository.SaveChangesAsync();
            return MapToDTO(await GetWithIncludes(id));
        }
        public async Task<bool> UpdateStatusAsync(int id, bool isActive)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;
            entity.IsActive = isActive;
            _repository.Update(entity);
            await _repository.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;
            
            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();
            return true;
        }

        public virtual async Task<string?> ValidateBeforeSave(TCreateDto model) => null;



        protected virtual Task<IEnumerable<T>> GetAllWithIncludes() => _repository.GetAllAsync();
        protected virtual Task<T> GetWithIncludes(int id) => _repository.GetByIdAsync(id);
        protected abstract TDto MapToDTO(T entity);
        protected abstract T MapToDomain(TCreateDto dto);
        protected abstract void UpdateDomain(T entity, TCreateDto dto);
    }
}
