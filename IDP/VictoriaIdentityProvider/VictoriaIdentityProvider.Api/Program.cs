using Infrastructure.Presentation.GlobalMiddlewares;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using VictoriaIdentityProvider.Api.Startup;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddCustomControllers();
builder.Services.AddExceptionHandler<GlobalErrorHandlingMiddleware>(); // todo refactor
builder.Services.AddPersonalOptions(config);
builder.Services.AddLifeTimeServices(config);
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddAuditlogs();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);
// deletes the  Server headers to provide no informations about the server :D
builder.Services.AddJwtMiddleware(config); // for security(xss/xxe)  json

//()l()
var app = builder.Build();
//()l()
app.UseHttpsRedirection();



app.UseExceptionHandler();



app.UseForwardedHeaders(new ForwardedHeadersOptions
{
        ForwardedHeaders =
     ForwardedHeaders.XForwardedFor
    | ForwardedHeaders.XForwardedProto
    | ForwardedHeaders.XForwardedHost
});
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();