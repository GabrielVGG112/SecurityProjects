namespace VictoriaIdentityProvider.Application.Configuration;

/// <summary>
/// Provides constant string values that represent standard purposes for security tokens, such as email confirmation and
/// password recovery.
/// </summary>
/// <remarks>These constants are intended to be used as identifiers for different token purposes in authentication
/// and user management scenarios. Using predefined values helps ensure consistency and reduces the risk of errors when
/// generating or validating tokens for specific actions.</remarks>
    public static class TokenPurposeSettings
    {

        public const string ForPurpose = "purpose";
  
       //left
       
        public const string EmailConfirmation = "email_confirmation";
        public const string PasswordRecovery = "password_recovery";
        public const string ChangeEmail = "change_email";
        public const string PhoneConfirmation = "phone_confirmation";
    //right
}


