using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VictoriaIdentityProvider.Domain.Enums;

namespace VictoriaIdentityProvider.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsEmailConfirmed { get; set; }
        

        public DateTime? LockoutEnd { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LastLogInAt { get; set; }
       public UserStatusEnum UserStatus { get; set; } 
        
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool IsLocked => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;


        //relation & nav
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();
        public ICollection<UserSession> UserSessions { get; set; } = new HashSet<UserSession>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new HashSet<AuditLog>();
    }
}
