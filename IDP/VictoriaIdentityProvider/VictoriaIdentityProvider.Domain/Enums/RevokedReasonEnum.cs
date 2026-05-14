using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Domain.Enums
{
    public enum RevokedReasonEnum
    {
        Logout,
        Admin,
        Rotation,
        Corrupted,
        Expired
    }
}
