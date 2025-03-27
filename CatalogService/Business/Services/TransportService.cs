using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Transport;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class TransportService : GenericService<Transport, TransportDTO, TransportCreateDTO>, ITransportService
    {
        public TransportService(ITransportRepository repository) : base(repository)
        {
        }
        protected override TransportDTO MapToDTO(Transport entity)
        {
            return new TransportDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                TaxId = entity.TaxId,
                Address = entity.Address,
                PostalCode = entity.PostalCode.Code,
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                Comment = entity.Comment
            };
        }

        protected override Transport MapToDomain(TransportCreateDTO dto)
        {
            return new Transport
            {
                Name = dto.Name,
                TaxId = dto.TaxId,
                Address = dto.Address,
                PostalCodeId = dto.PostalCodeId,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Comment = dto.Comment
            };
        }

        protected override void UpdateDomain(Transport entity, TransportCreateDTO dto)
        {
            entity.Name = dto.Name;
            entity.TaxId = dto.TaxId;
            entity.Address = dto.Address;
            entity.PostalCodeId = dto.PostalCodeId;
            entity.PhoneNumber = dto.PhoneNumber;
            entity.Email = dto.Email;
            entity.Comment = dto.Comment;
        }

        public async Task<bool> IsTransportNameUnique(string Name)
        {
            var transports = await _repository.FindAsync(b => b.Name == Name);
            return !transports.Any();
        }

        public override async Task<string?> ValidateBeforeSave(TransportCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return "Transport name is mandatory.";
            var isUnique = await IsTransportNameUnique(model.Name);
            if (!isUnique)
                return "Transport already exits.";
            return null;
        }


        protected override Task<IEnumerable<Transport>> GetAllWithIncludes() => _repository.GetAllIncludingAsync(
            t => t.PostalCode
            );

        protected override Task<Transport> GetWithIncludes(int id) => _repository.GetIncludingAsync(
            id, 
            t => t.PostalCode);

    }
}
