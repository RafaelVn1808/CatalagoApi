using System.Text;
using CatalagoApi.Data;
using Microsoft.OpenApi.Models;
using CatalagoApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// User Secrets em desenvolvimento (credenciais Supabase, etc.)
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddUserSecrets<Program>();

// Banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<SupabaseSettings>(builder.Configuration.GetSection(SupabaseSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
if (!string.IsNullOrEmpty(jwtSettings?.Key))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
            };
        });
}

builder.Services.AddAuthorization();

// Serviços
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<LojaService>();
builder.Services.AddScoped<EstoqueService>();
builder.Services.AddScoped<ImportacaoCsvService>();
builder.Services.AddScoped<UploadService>();
builder.Services.AddHttpClient<SupabaseStorageService>();

// CORS (inclui origem do Swagger para evitar "Failed to fetch")
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000", "http://localhost:5173", "http://localhost:5291", "https://localhost:7171"];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CatalagoApi", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Faça login em POST /api/Auth/login e cole APENAS o token (sem Bearer)"
    });
    c.OperationFilter<CatalagoApi.Swagger.BearerAuthOperationFilter>();
});

var app = builder.Build();

// Tratamento global de exceções e logging de requisições
app.UseMiddleware<CatalagoApi.Middleware.ExceptionHandlingMiddleware>();
app.UseMiddleware<CatalagoApi.Middleware.RequestLoggingMiddleware>();

// Migrations automáticas e seed em desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CatalagoApi v1");
        c.DisplayRequestDuration();
    });
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pending = await db.Database.GetPendingMigrationsAsync();
        if (pending.Any())
            await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db);
    }
}

// Em desenvolvimento, evita redirecionar HTTP→HTTPS (evita "Failed to fetch" no Swagger)
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
