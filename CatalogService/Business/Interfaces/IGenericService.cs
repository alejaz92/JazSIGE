using CatalogService.Infrastructure.Models;
using System.Collections.Generic;

namespace CatalogService.Business.Interfaces
{
    public interface IGenericService<T, TDto, TCreateDto> where T : class
    {
        Task<TDto> CreateAsync(TCreateDto model);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<TDto>> GetAllAsync();
        Task<TDto> GetByIdAsync(int id);
        Task<TDto> UpdateAsync(int id, TCreateDto model);
        Task<bool> UpdateStatusAsync(int id, bool isActive);
        Task<string?> ValidateBeforeSave(TCreateDto model);
    }
}
