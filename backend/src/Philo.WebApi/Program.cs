using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Philo.Application;
using Philo.Application.Common.Interfaces;
using Philo.Infrastructure;
using Philo.Infrastructure.Services;
using Philo.WebApi.Auth;
using Philo.WebApi.Hubs;
using Philo.WebApi.Middleware;
using Philo.WebApi.Realtime;

var builder = WebApplication.CreateBuilder(args);

// Camadas
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Tempo real
builder.Services.AddSignalR();
builder.Services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();

// Autenticação/autorização — identidade sempre lida do JWT, nunca do corpo da requisição.
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var signingKey = jwtSection["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey não configurada.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        // Permite que o SignalR receba o JWT via query string na conexão do WebSocket.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs/chat"))
                    context.Token = accessToken;

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();

builder.Services.Configure<InternalApiKeyOptions>(builder.Configuration.GetSection(InternalApiKeyOptions.SectionName));
builder.Services.AddScoped<InternalApiKeyFilter>();

// Enums trafegam como texto ("Support", "Normal", ...) nos dois sentidos — é como o
// frontend já trata esses campos (union types) e como os DTOs de leitura já devolvem
// (Status.ToString() manualmente). Sem isso, o binding do corpo da requisição espera
// número por padrão e falha a desserialização antes mesmo do validador rodar.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Philo API", Version = "v1" });

    // JWT (Bearer) — usado pelos endpoints autenticados normais.
    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Informe apenas o token JWT (sem o prefixo \"Bearer \").",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
    };
    options.AddSecurityDefinition("Bearer", bearerScheme);

    // X-Internal-Api-Key — exigido pelo InternalApiKeyFilter nos endpoints de /api/internal/*.
    var internalApiKeyScheme = new OpenApiSecurityScheme
    {
        Name = "X-Internal-Api-Key",
        Description = "Chave de serviço para endpoints internos (ex.: /api/internal/sessions). Valor em appsettings: InternalApi:ApiKey.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "InternalApiKey" },
    };
    options.AddSecurityDefinition("InternalApiKey", internalApiKeyScheme);

    // Torna os dois esquemas selecionáveis no botão "Authorize" do Swagger UI,
    // sem exigir nenhum deles globalmente (cada endpoint usa o que precisa).
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { bearerScheme, Array.Empty<string>() },
        { internalApiKeyScheme, Array.Empty<string>() },
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Site", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Em dev, quem fala com o navegador é sempre o Vite (proxy em HTTP). Redirecionar pra HTTPS
// aqui faz o navegador seguir o 307 sozinho para uma origem diferente (porta/esquema), virando
// uma chamada cross-origin de verdade — e o Fetch spec não deixa "Authorization" passar por um
// Access-Control-Allow-Headers "*", então o token some antes de chegar na API (causa do 401
// "veio sem token nenhum" nos endpoints [Authorize], mesmo com o CORS liberado).
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("Site");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();

// Necessário para WebApplicationFactory em testes E2E.
public partial class Program { }
