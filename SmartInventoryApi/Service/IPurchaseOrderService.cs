﻿using SmartInventoryApi.DTOs;

namespace SmartInventoryApi.Services
{
    public interface IPurchaseOrderService
    {
        Task<PaginatedResponseDto<PurchaseOrderDto>> GetAllPurchaseOrdersAsync(
            PurchaseOrderQueryParameters queryParameters,
            int requestingUserId,
            string requestingUserRole);

        Task<PurchaseOrderDto?> GetPurchaseOrderByIdAsync(
            int id,
            int requestingUserId,
            string requestingUserRole);

        Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderDto purchaseOrderDto, int createdByUserId);
        Task<PurchaseOrderDto> UpdatePurchaseOrderAsync(int id, UpdatePurchaseOrderDto purchaseOrderDto, int updatedByUserId);
        Task<PurchaseOrderDto> ApprovePurchaseOrderAsync(int id, int approvedByUserId);
        Task<PurchaseOrderDto> CancelPurchaseOrderAsync(int id, int cancelledByUserId);
    }
}