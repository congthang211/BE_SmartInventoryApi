using SmartInventoryApi.DTOs;
using SmartInventoryApi.Models;
using SmartInventoryApi.Repositories;

namespace SmartInventoryApi.Services
{
    public class PartnerService : IPartnerService
    {
        private readonly IPartnerRepository _partnerRepository;

        public PartnerService(IPartnerRepository partnerRepository)
        {
            _partnerRepository = partnerRepository;
        }

        public async Task<PartnerDto> CreatePartnerAsync(CreatePartnerDto partnerDto)
        {
            if (await _partnerRepository.NameExistsAsync(partnerDto.Name))
            {
                throw new InvalidOperationException("Partner name already exists.");
            }

            var partner = new Partner
            {
                Name = partnerDto.Name,
                Type = partnerDto.Type,
                ContactPerson = partnerDto.ContactPerson,
                Phone = partnerDto.Phone,
                Email = partnerDto.Email,
                Address = partnerDto.Address,
                TaxCode = partnerDto.TaxCode,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var createdPartner = await _partnerRepository.CreateAsync(partner);
            return MapToDto(createdPartner);
        }

        public async Task DeletePartnerAsync(int id)
        {
            var partner = await _partnerRepository.GetByIdAsync(id);
            if (partner == null)
            {
                throw new KeyNotFoundException("Partner not found.");
            }
            await _partnerRepository.DeleteAsync(partner);
        }

        public async Task<IEnumerable<PartnerDto>> GetAllPartnersAsync(string? type)
        {
            var partners = await _partnerRepository.GetAllAsync(type);
            return partners.Select(MapToDto);
        }

        public async Task<PartnerDto?> GetPartnerByIdAsync(int id)
        {
            var partner = await _partnerRepository.GetByIdAsync(id);
            return partner == null ? null : MapToDto(partner);
        }

        public async Task UpdatePartnerAsync(int id, UpdatePartnerDto partnerDto)
        {
            var partner = await _partnerRepository.GetByIdAsync(id);
            if (partner == null)
            {
                throw new KeyNotFoundException("Partner not found.");
            }

            if (await _partnerRepository.NameExistsAsync(partnerDto.Name, id))
            {
                throw new InvalidOperationException("Partner name already exists.");
            }

            partner.Name = partnerDto.Name;
            partner.Type = partnerDto.Type;
            partner.ContactPerson = partnerDto.ContactPerson;
            partner.Phone = partnerDto.Phone;
            partner.Email = partnerDto.Email;
            partner.Address = partnerDto.Address;
            partner.TaxCode = partnerDto.TaxCode;
            partner.IsActive = partnerDto.IsActive;

            await _partnerRepository.UpdateAsync(partner);
        }

        private static PartnerDto MapToDto(Partner partner)
        {
            return new PartnerDto
            {
                Id = partner.Id,
                Name = partner.Name,
                Type = partner.Type,
                ContactPerson = partner.ContactPerson,
                Phone = partner.Phone,
                Email = partner.Email,
                Address = partner.Address,
                TaxCode = partner.TaxCode,
                IsActive = partner.IsActive,
                CreatedDate = partner.CreatedDate
            };
        }
    }
}