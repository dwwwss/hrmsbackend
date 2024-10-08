using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using hrms_backend.Models;
using hrms_backend.Controllers;
using Microsoft.OpenApi.Models;
using hrms_backend;
using Microsoft.Extensions.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DB");
builder.Services.AddScoped<PermissionService>();
builder.Services.AddSingleton<BlacklistService>();
builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration"));
builder.Services.AddLogging(builder => builder.AddConsole());
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "dotnetClaimAuthorization", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert Token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
{
 new OpenApiSecurityScheme{
 Reference =new OpenApiReference{
     Type = ReferenceType.SecurityScheme,
     Id = "Bearer"
}
},
new string[]{}
}
});
});
builder.Services.AddDbContext<HrmsDbContext>(options =>
    options.UseSqlServer(connectionString)
);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("corspolicy", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddControllers();
var app = builder.Build();
var jwtKey = AuthController.JwtKeyGenerator.GenerateJwtKey();
app.Configuration["Jwt:Key"] = jwtKey;
app.UseStaticFiles();
app.UseCors("corspolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
    // Add a custom UI configuration to include a field for token input
    c.ConfigObject.AdditionalItems["syntaxHighlight"] = false; // Disable syntax highlighting for better layout
    c.ConfigObject.AdditionalItems["oauth"] = new OpenApiOAuth
    {
        Type = "apiKey",
        Name = "Authorization",
        Description = "Bearer token",
        In = "header"
    };
    // Add the "Authorize" button to the Swagger UI
    c.OAuthClientId("swagger-ui");
    c.OAuthClientSecret("swagger-ui-secret");
    c.OAuthRealm("swagger-ui-realm");
    c.OAuthAppName("Swagger UI");
    c.OAuthUsePkce();
});
app.Run();
internal class OpenApiOAuth
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string In { get; set; }
}

