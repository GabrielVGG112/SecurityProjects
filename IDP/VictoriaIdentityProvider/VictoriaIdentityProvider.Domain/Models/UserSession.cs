using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Interfaces;

namespace VictoriaIdentityProvider.Domain.Models
{
    public class UserSession :IUserDependency
    {
        public Guid Id { get; set; }
       
       
        // sensitive data over sessions  
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string ? DeviceId { get; set; } 
        public string ? DeviceName {  get; set; }
        public string ? Location { get; set; }
        public string SessionToken { get; set; } = string.Empty;  // hashed
        public Guid? RefreshTokenId { get; set; }



        public DateTime LastActivityAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        // status 
       
        public DateTime? RevokedAt { get; set; }
        public  RevokedReasonEnum? RevokedReason { get; set; }
        
        
        //computed
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !RevokedReason.HasValue && !RevokedAt.HasValue && !IsExpired;

        //relation & nav
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        
        public ICollection<RefreshToken> RefreshTokens { get; set; }  = new HashSet<RefreshToken>();
    }
}
