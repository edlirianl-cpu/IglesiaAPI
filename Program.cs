using IglesiaAPI.Data;
using IglesiaAPI.Infrastructure.Auth;
using IglesiaAPI.Infrastructure.Reports;
using IglesiaAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// 🔹 1. CONFIGURACIÓN CORS (Sin cambios en políticas)
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins("https://sistemagestioneclesial.ipulrd.org", "https://localhost:5001", "http://localhost:5000", "https://localhost:7250")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 🔹 2. CONFIGURACIÓN DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<IglesiaDBContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 8))));

//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// builder.Services.AddDbContext<IglesiaDBContext>(options =>
//  options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 🔹 3. REGISTRO DE SERVICIOS
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10485760; // 10MB
});

// Dependency Injection (Sin cambios)
builder.Services.AddScoped<ILocalidadService, LocalidadService>();
builder.Services.AddScoped<ICuentaService, CuentaService>();
builder.Services.AddScoped<IMovimientoService, MovimientoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IMiembroService, MiembroService>();
builder.Services.AddScoped<IRegistroSecretariaService, RegistroSecretariaService>();
builder.Services.AddScoped<ICelulaService, CelulaService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IEventoImportanteService, EventoImportanteService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBandejaAutorizacionService, BandejaAutorizacionService>();
builder.Services.AddScoped<IConciliacionService, ConciliacionService>();
builder.Services.AddScoped<IReporteService, ReporteService>();

builder.Services.AddScoped<PdfGenerator>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton(resolver =>
    resolver.GetRequiredService<Microsoft.Extensions.Options.IOptions<JwtSettings>>().Value);

// 🔹 4. CONTROLADORES
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();

// 🔹 5. SWAGGER
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Iglesia API - Sistema Gestión Eclesial",
        Version = "v1",
        Description = "Roles: Secretario L/N, Tesorero L/N, Pastor Local, Administrador, SuperUsuario"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// 🔑 6. AUTENTICACIÓN
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey)) throw new InvalidOperationException("Falta Clave JWT");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role,
        };
    });

builder.Services.AddAuthorization();
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

var app = builder.Build();

// 🔹 7. MIDDLEWARE PIPELINE (AJUSTADO SEGÚN BITÁCORA)

// Según la bitácora, para evitar el problema de CORS, la política debe ir ANTES de cualquier redirección o Auth.
app.UseCors(MyAllowSpecificOrigins);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Iglesia API v1");
});

app.UseStaticFiles(); // Sirve el contenido general de wwwroot

var fotosPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads", "fotos");

if (!Directory.Exists(fotosPath))
{
    Directory.CreateDirectory(fotosPath);
}

// Esto permite que la URL "api/uploads/fotos/archivo.jpg" sea accesible
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

// app.UseHttpsRedirection(); // Mantener comentado si el hosting maneja el SSL de forma externa

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Usamos el nombre real de tu contexto
        var context = services.GetRequiredService<IglesiaDBContext>();

        // Verificación maestra para aplicar migraciones solo si hay cambios pendientes
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al migrar la base de datos.");
    }
}

app.Run();