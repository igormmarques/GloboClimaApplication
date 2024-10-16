using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar appsettings baseado no ambiente
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Adicionar servi�os ao cont�iner
builder.Services.AddControllersWithViews();

// Configurar JWT para autentica��o
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
                // Captura o token JWT do cabe�alho Authorization
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }

                // Tenta recuperar o token da sess�o se ele n�o estiver no cabe�alho
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


// Configura��o de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

// Configurar sess�o para armazenamento do token
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

// Construir a aplica��o
var app = builder.Build();

// Configurar o pipeline de requisi��o HTTP
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

// Middleware de sess�o
app.UseSession();  // Ativa o uso de sess�o

// Middleware de autentica��o e autoriza��o
app.UseAuthentication();
app.UseAuthorization(); 

// Mapeamento das rotas para o MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "register",
    pattern: "{controller=Register}/{action=Index}/{id?}");

// Executar a aplica��o
app.Run();
