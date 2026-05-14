using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using VictoriaIdentityProvider.Domain.CustomErrors;

namespace Infrastructure.Presentation.GlobalMiddlewares
{
    // TODO : refactor , better errors , inheritance + logging
    public class GlobalErrorHandlingMiddleware : IExceptionHandler
    {
      
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.Headers.XFrameOptions = "DENY";
            httpContext.Response.Headers.XContentTypeOptions = "nosniff";
            httpContext.Response.Headers["Referrer-Policy"] = "no-referrer";
            httpContext.Response.Headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'; sandbox"; // maybe nonce final step

            // REMOVE SERVER HEADERS
            httpContext.Response.Headers.Server = "";
            httpContext.Response.Headers.XPoweredBy = "";
         
            var status = exception switch
            {
                ClaimException => (int)HttpStatusCode.Forbidden,
                EmailDeliveryException => (int)HttpStatusCode.BadRequest,
                EmailNotVerifiedException => (int)HttpStatusCode.Forbidden,
                InvalidCredentialsException => (int)HttpStatusCode.Forbidden,
                LockoutExpirationException => (int)HttpStatusCode.Forbidden,
                RefreshTokenException => (int)HttpStatusCode.BadRequest,
                SessionException => (int)HttpStatusCode.BadRequest,
                UserException => (int)HttpStatusCode.BadRequest,
                SessionConcurrencyException => (int)HttpStatusCode.Conflict,
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                ValidationError => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError,

            };
         
             httpContext.Response.StatusCode = status;
       
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "An error occured",
                Status = status,
                Detail = exception.Message,
                Type = exception.GetType().Name.ToString()
            }, cancellationToken);
            return true;
        }
    }
}
