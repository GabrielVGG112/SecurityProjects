namespace VictoriaIdentityProvider.Application.Configuration
{
    public class JwtKeysOptions 
    {
        public string UserActionTokenKey { get; set; } = string.Empty;
        public string VictoriaIdpClientSecret { get; set; } = string.Empty;


        public string IdpPrivateKey { get; set; } = string.Empty;
        public string IdpPublicKey { get; set; } = string.Empty;
    }
}
