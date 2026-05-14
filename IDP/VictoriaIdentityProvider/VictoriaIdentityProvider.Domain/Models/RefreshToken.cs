using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Interfaces;

namespace VictoriaIdentityProvider.Domain.Models
{
    public class RefreshToken  :IUserDependency
    {
        public Guid Id { get; set; }


        // Token Data
        public string TokenHash { get; set; } = string.Empty; 
  

        //LifeCycle
     
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public RevokedReasonEnum? RevokedReason { get; set; }
        public Guid? ReplacedByTokenId { get; set; }





        // Computed Properties
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt.HasValue;
        public bool IsActive => !IsRevoked && !IsExpired && !ReplacedByTokenId.HasValue;
    
        //relation & nav
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid UserSessionId { get; set; }
        public UserSession UserSession { get; set; }

    }


}
