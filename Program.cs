using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;
using GloboClimaAPI.Data;
using System.Net; // Para tratar o status de erro
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json; // Para capturar exceções no middleware;

var builder = WebApplication.CreateBuilder(args);

// Carregar as configurações de JWT do appsettings.json
var jwtSettings = builder.Configuration.GetSection("Jwt");

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp",
        builder =>
        {
            builder.WithOrigins("https://localhost:7217") // Porta do Blazor Server
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Configurar Entity Framework para SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Identity para gerenciar LoginData (usuários) e adicionar suporte para o Identity
builder.Services.AddIdentity<LoginData, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configurar o Swagger para suportar JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GloboClimaAPI", Version = "v1" });

    // Configuração para suportar Bearer Token no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insira o token JWT desta forma: Bearer {seu_token_jwt}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Configurar Autenticação JWT usando as configurações do appsettings.json
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SigningKey"]))
        };

        // Lidar com o token no cabeçalho de requisição
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;  // Associa o token à requisição
                }
                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                context.NoResult();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "text/plain";
                return context.Response.WriteAsync("Falha na autenticação. Verifique o token.");
            },

            OnChallenge = context =>
            {
                if (context.AuthenticateFailure != null)
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    var result = JsonConvert.SerializeObject(new { message = "Token inválido ou expirado" });
                    return context.Response.WriteAsync(result);
                }
                return Task.CompletedTask;
            }
        };
    });


// Adicionar HttpClient para consumir APIs externas (como OpenWeather)
builder.Services.AddHttpClient();

// Adicionar suporte para usar Controllers
builder.Services.AddControllers();

// Adicionar autorização
builder.Services.AddAuthorization();

var app = builder.Build();

// Configurar tratamento global de exceções
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            await context.Response.WriteAsync(new
            {
                StatusCode = context.Response.StatusCode,
                Message = "An error occurred while processing your request.",
                Detailed = contextFeature.Error.Message
            }.ToString());
        }
    });
});

// Configurar o pipeline de requisição HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GloboClimaAPI v1");
    });
}

app.UseHttpsRedirection();

// Middleware de CORS
app.UseCors("AllowBlazorApp");

// Middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

// Mapeia todas as controllers
app.MapControllers();

app.Run();
