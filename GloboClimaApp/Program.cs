using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar appsettings baseado no ambiente
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Adicionar serviços ao contêiner
builder.Services.AddControllersWithViews();

// Configurar JWT para autenticação
var jwtSettings = builder.Configuration.GetSection("Jwt");
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

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Captura o token JWT do cabeçalho Authorization
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }

                // Tenta recuperar o token da sessão se ele não estiver no cabeçalho
                if (string.IsNullOrEmpty(context.Token))
                {
                    token = context.HttpContext.Session.GetString("JWTToken");
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }
                }

                return Task.CompletedTask;
            },
            // Desafio personalizado para token expirado
            OnChallenge = context =>
            {
                if (context.ErrorDescription != null && context.ErrorDescription.Contains("The token expired"))
                {
                    context.Response.Redirect("/Login");
                }
                return Task.CompletedTask;
            }
        };
    });


// Configuração de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

// Configurar sessão para armazenamento do token
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Adicionar HttpClient e configurar URLs para API de favoritos
builder.Services.AddHttpClient("FavoritesApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["FavoritesApiSettings:BaseUrl"]);
});

// Construir a aplicação
var app = builder.Build();

// Configurar o pipeline de requisição HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware de CORS
app.UseCors("CorsPolicy");

// Middleware de sessão
app.UseSession();  // Ativa o uso de sessão

// Middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization(); 

// Mapeamento das rotas para o MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "register",
    pattern: "{controller=Register}/{action=Index}/{id?}");

// Executar a aplicação
app.Run();
