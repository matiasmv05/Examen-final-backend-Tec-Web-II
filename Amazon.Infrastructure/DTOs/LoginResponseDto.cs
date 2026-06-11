using Amazon.Core.Enum;
using System;

namespace Amazon.Infrastructure.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public RoleType? Role { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
