global using FastEndpoints;
global using FastEndpoints.Security;
global using FastEndpoints.Validation;
global using Web.Auth;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.OpenApi.Models;
using Web.Services;

var builder = WebApplication.CreateBuilder();
builder.Services.AddFastEndpoints();
builder.Services.AddAuthenticationJWTBearer(builder.Configuration["TokenKey"]);
builder.Services.AddAuthorization(o => o.AddPolicy("AdminOnly", b => b.RequireRole(Role.Admin)));
builder.Services.AddCors();
builder.Services.AddResponseCaching();
builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.CustomSchemaIds(type => type.FullName);
    o.TagActionsBy(d => new[] { d.RelativePath?.Split('/').First() });
    o.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "bearerAuth"
                }
            },
            Array.Empty<string>()
        }
    });
}).AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseCors(b => b.AllowAnyOrigin());
app.UseResponseCaching();
app.UseFastEndpoints();
app.Run();