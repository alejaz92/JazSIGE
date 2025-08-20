using SalesService.Business.Models.DeliveryNote;

namespace SalesService.Business.Interfaces
{
    public interface IDeliveryNoteService
    {
        Task<DeliveryNoteDTO> CreateAsync(int saleId, DeliveryNoteCreateDTO dto, int userId);
        Task<DeliveryNoteDTO> CreateQuickAsync(int userId, DeliveryNoteCreateDTO dto);
        Task<IEnumerable<DeliveryNoteDTO>> GetAllBySaleIdAsync(int saleId);
        Task<DeliveryNoteDTO> GetByIdAsync(int id);
    }
}
