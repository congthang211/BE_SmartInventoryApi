﻿namespace SmartInventoryApi.DTOs
{
    public class ActivityLogDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; } // Tên đăng nhập của người dùng
        public string? UserFullName { get; set; } // Họ tên đầy đủ của người dùng
        public string Action { get; set; }
        public string Module { get; set; }
        public string? Description { get; set; }
        public int? EntityId { get; set; }
        public string? EntityType { get; set; }
        public string? IpAddress { get; set; }
        public DateTime LogDate { get; set; }
    }

    public class ActivityLogQueryParameters
    {
        public int? UserId { get; set; }
        public string? Module { get; set; }
        public string? Action { get; set; }
        public string? EntityType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Thuộc tính này sẽ được Service Layer sử dụng để truyền điều kiện lọc vai trò xuống Repository
        public List<string>? TargetUserRoles { get; set; }

        private const int MaxPageSize = 50;
        private int _pageNumber = 1;
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = (value < 1) ? 1 : value;
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : (value < 1 ? 1 : value);
        }

        public string SortBy { get; set; } = "LogDate";
        public string SortDirection { get; set; } = "desc";
    }
}
