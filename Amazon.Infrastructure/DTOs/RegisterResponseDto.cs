using Amazon.Core.Enum;

namespace Amazon.Infrastructure.DTOs
{
    public class RegisterResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public RoleType? Role { get; set; }
    }
}
