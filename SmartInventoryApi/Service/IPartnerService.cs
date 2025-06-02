using SmartInventoryApi.DTOs;

namespace SmartInventoryApi.Services
{
    public interface IPartnerService
    {
        Task<IEnumerable<PartnerDto>> GetAllPartnersAsync(string? type);
        Task<PartnerDto?> GetPartnerByIdAsync(int id);
        Task<PartnerDto> CreatePartnerAsync(CreatePartnerDto partnerDto);
        Task UpdatePartnerAsync(int id, UpdatePartnerDto partnerDto);
        Task DeletePartnerAsync(int id);
    }
}