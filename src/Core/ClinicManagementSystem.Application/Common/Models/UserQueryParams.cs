namespace ClinicManagementSystem.Application.DTOs.Auth.Users;

public class UserQueryParams
{
    private const int MaxPageSize = 50; // الحد الأقصى لحجم الصفحة لحماية السيرفر

    public int PageNumber { get; set; } = 1; // رقم الصفحة الافتراضي

    private int _pageSize = 10; // حجم الصفحة الافتراضي (10 مستخدمين)
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public string? SearchTerm { get; set; } // نص البحث (اسم، بريد، هاتف)
    public string? SortBy { get; set; } // الحقل المراد الترتيب بناءً عليه (مثل Username, CreatedAt)
    public bool IsDescending { get; set; } = false; // هل الترتيب تنازلي؟
    public bool? IsActive { get; set; } // فلترة المستخدمين بناءً على حالتهم (نشط/موقف)
}