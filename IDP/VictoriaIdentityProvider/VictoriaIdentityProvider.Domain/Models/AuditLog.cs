using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Domain.Models
{
public class AuditLog
{
    public int Id { get; set; }

    public string EventType { get; set; } = default!;
         
 
    public Guid? UserId { get; set; }
    public User? User { get; set; }


    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
    public Guid? SessionId { get; set; }
    public Guid? TokenId { get; set; }
   

    public string Message { get; set; } = default!;


    public DateTime CreatedAtUtc { get; set; }
}
}
