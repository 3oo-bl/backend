using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProfitableViewApp.Interfaces;
using ProfitableViewCore;
using ProfitableViewInfra;
using ProfitableViewDataInfra.gRPC;
using ProfitableViewDataInfra.Searchers;
using ProfitableViewDataInfra.Services;
using ProfitableViewInfra.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insert token"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
        }
    });
});

if (string.IsNullOrEmpty(builder.Configuration["jwt:Key"]))
    builder.Configuration["jwt:Key"] = File.ReadAllText("/run/secrets/jwt_key");

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidAudience = "ProfitableViewAPI",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwt:Key"]!)),
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine("AUTH FAILED: " + ctx.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                Console.WriteLine("TOKEN OK");
                return Task.CompletedTask;
            }
        };
    });

var connection = builder.Configuration.GetConnectionString("DefaultConnection");

var cs = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine(cs);

builder.Services.AddSingleton<WbGrpcClient>();
builder.Services.AddScoped<AuthentificationService>();
builder.Services.AddDbContext<DBContext>(options => options.UseNpgsql(connection));
builder.Services.AddSingleton<PasswordHasher<string>>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ILogger, Logger<AuthentificationService>>();
builder.Services.AddScoped<ISearcher, WbSearcher>();
builder.Services.AddScoped<HttpClient>();
builder.Services.BindClientFactory();
builder.Services.BindParsers();
builder.Services.BindInfrastructureServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

if (args.Contains("--fake"))
{
    app.MapFakeEndpoints(app.Logger);
}
else
{
    app.MapEndpoints();
}

app.Run();
