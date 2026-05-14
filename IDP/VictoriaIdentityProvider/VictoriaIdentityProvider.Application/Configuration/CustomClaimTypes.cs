namespace VictoriaIdentityProvider.Application.Configuration
{
    public static class CustomClaimTypes
    {
  
        public const string EmailVerified = "email_verified";

       
        public const string PreferredUsername = "preferred_username";

       
        public const string Permissions = "permissions";

    
        public const string SessionId = "session_id";
        public const string DeviceId = "device_id";

        public const string Amr = "amr";                 // Authentication Method Reference
        public const string MfaVerified = "mfa_verified";

        public const string RiskScore = "risk_score";


        public const string TenantId = "tenant_id";
    }
}