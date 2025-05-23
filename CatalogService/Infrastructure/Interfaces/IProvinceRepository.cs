﻿using CatalogService.Infrastructure.Models;

namespace CatalogService.Infrastructure.Interfaces
{
    public interface IProvinceRepository : IGenericRepository<Province>
    {
        Task<IEnumerable<Province>> GetByCountryIdAsync(int countryId);
    }
}
