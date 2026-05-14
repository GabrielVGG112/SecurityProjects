using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Domain.Enums
{
    public enum UserStatusEnum
    {
        Active,
        Inactive,
        Suspended,
        PendingVerification,
        Verified
    }
}
