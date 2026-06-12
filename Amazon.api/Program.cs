using Amazon.Core.CustomEntities;
using Amazon.Core.Interface;
using Amazon.Core.Services;
using Amazon.Infrastructure.Data;
using Amazon.Infrastructure.Filters;
using Amazon.Infrastructure.Mappings;
using Amazon.Infrastructure.Repositories;
using Amazon.Infrastructure.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ─── CONFIGURACIÓN (orden correcto) ──────────────────────────────────
        // Las env vars deben cargarse ANTES de leer cualquier valor de config.
        builder.Configuration.Sources.Clear();
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                optional: true, reloadOnChange: true)
            .AddEnvironmentVariables(); // ← AQUÍ, no al final

        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>();
        }
        // ─────────────────────────────────────────────────────────────────────

        #region Configurar la BD MySql
        var connectionString = builder.Configuration.GetConnectionString("ConnectionMySql");

        // Log para verificar que la connection string llega correctamente
        Console.WriteLine($"[CONFIG] ConnectionString is null: {connectionString == null}");
        if (connectionString != null)
        {
            // Loguea solo el host, nunca la password completa
            Console.WriteLine($"[CONFIG] MySQL host: {connectionString.Split(';')[0]}");
        }

        builder.Services.AddDbContext<AmazonContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        #endregion

        // ─── CORS ────────────────────────────────────────────────────────────
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:5173",
                        "http://localhost:3000",
                        builder.Configuration["Cors:AllowedOrigin"] ?? ""
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        // ─────────────────────────────────────────────────────────────────────

        builder.Services.AddAutoMapper(typeof(MappingProfile));

        builder.Services.AddTransient<IOrderService, OrderService>();
        builder.Services.AddTransient<IUserService, UserService>();
        builder.Services.AddTransient<IProductService, ProductService>();
        builder.Services.AddTransient<IPaymentService, PaymentService>();

        builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
        builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        builder.Services.AddScoped<IDapperContext, DapperContext>();
        builder.Services.AddSingleton<IPasswordService, PasswordService>();

        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<GlobalExceptionFilter>();
            options.Filters.Add<ValidationFilter>();
        }).AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        }).ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        builder.Services.Configure<PasswordOptions>(
            builder.Configuration.GetSection("PasswordOptions"));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "Backend Amazon API",
                Version = "v1",
                Description = "Documentacion de la API de Amazon - net 9",
                Contact = new()
                {
                    Name = "Equipo de Desarrollo UCB",
                    Email = "desarrollo@ucb.edu.bo"
                }
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });

        builder.Services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("x-api-version"),
                new QueryStringApiVersionReader("api-version")
            );
        });

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Authentication:Issuer"],
                ValidAudience = builder.Configuration["Authentication:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(
                        builder.Configuration["Authentication:SecretKey"]!
                    )
                )
            };
        });

        builder.Services.AddValidatorsFromAssemblyContaining<UserDtoValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<ProductDtoValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<OrderDtoValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<GetByIdRequestValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<CrearOrdenRequestValidation>();
        builder.Services.AddValidatorsFromAssemblyContaining<OrderItemDtoValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<PaymentDtoValidator>();

        builder.Services.AddScoped<IValidationService, ValidationService>();
        builder.Services.AddScoped<ISecurityServices, SecurityServices>();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend AmazonDb API v1");
            options.RoutePrefix = string.Empty;
        });

        // app.UseHttpsRedirection();

        app.UseCors("FrontendPolicy");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}