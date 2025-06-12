using SalesService.Business.Models.DeliveryNote;

namespace SalesService.Business.Interfaces
{
    public interface IDeliveryNoteService
    {
        Task<DeliveryNoteDTO> CreateAsync(int saleId, DeliveryNoteCreateDTO dto, int userId);
        Task<IEnumerable<DeliveryNoteDTO>> GetAllBySaleIdAsync(int saleId);
        Task<DeliveryNoteDTO> GetByIdAsync(int id);
    }
}
