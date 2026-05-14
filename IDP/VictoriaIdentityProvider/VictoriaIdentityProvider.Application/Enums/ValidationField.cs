using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VictoriaIdentityProvider.Application.Enums;

public enum ValidationField
{

    [Display(Name = "Email Address")]

    Email,

    [Display(Name = "Password")]

    Password,

    [Display(Name = "First Name")]

    FirstName,

    [Display(Name = "Last Name")]

    LastName,

    [Display(Name = "Phone Number")]

    PhoneNumber,

    [Display(Name = "Refresh Token")]
    RefreshToken,

    [Display(Name = "Session")]
    Session,
    [Display(Name = "Jwt Token")]
    JwtToken
}
