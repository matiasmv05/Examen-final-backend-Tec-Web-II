using Amazon.Core.Enum;

public class UpdateUserAdminDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
    public RoleType? Role { get; set; } // null = no cambiar el rol
}