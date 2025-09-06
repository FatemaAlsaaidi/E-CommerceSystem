using System;
using System.Text;

using E_CommerceSystem.Repositories;
using E_CommerceSystem.Services;
using E_CommerceSystem.Services.Email;
using E_CommerceSystem.Middleware;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using AutoMapper;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

namespace E_CommerceSystem
{
    public class Program
    {
        // --- Serilog bootstrap happens before builder creation ---
        static Program()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .WriteTo.File("Logs/app-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger();
        }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Replace default logging pipeline with Serilog
            builder.Host.UseSerilog();

            builder.Services.AddControllers();

            // ----------------------
            // Repositories & Services
            // ----------------------
            builder.Services.AddScoped<IUserRepo, UserRepo>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<IProductRepo, ProductRepo>();
            builder.Services.AddScoped<IProductService, ProductService>();

            builder.Services.AddScoped<IOrderProductsRepo, OrderProductsRepo>();
            builder.Services.AddScoped<IOrderProductsService, OrderProductsService>();

            builder.Services.AddScoped<IOrderRepo, OrderRepo>();
            builder.Services.AddScoped<IOrderService, OrderService>();

            builder.Services.AddScoped<IReviewRepo, ReviewRepo>();
            builder.Services.AddScoped<IReviewService, ReviewService>();

            builder.Services.AddScoped<ICategoryRepo, CategoryRepo>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();

            builder.Services.AddScoped<ISupplierRepo, SupplierRepo>();
            builder.Services.AddScoped<ISupplierService, SupplierService>();

            builder.Services.AddScoped<IOrderSummaryService, OrderSummaryService>();

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
            builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
            builder.Services.AddTransient<IInvoiceService, InvoiceService>();

            builder.Services.AddScoped<IRefreshTokenRepo, RefreshTokenRepo>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // AutoMapper (scan all profiles in current app domain)
            builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);

            // ----------------------
            // DbContext
            // ----------------------
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseLazyLoadingProxies()
                       .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ----------------------
            // JWT Authentication
            // ----------------------
            var jwt = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwt["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey missing");

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };

                // Allow JWT from cookie if Authorization header is absent
                opt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        if (string.IsNullOrEmpty(ctx.Token))
                        {
                            var cookie = ctx.Request.Cookies["access_token"];
                            if (!string.IsNullOrEmpty(cookie))
                                ctx.Token = cookie;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization(o =>
            {
                o.AddPolicy("Reports.Read", p => p.RequireRole("Admin", "Manager"));
            });

            // ----------------------
            // Swagger
            // ----------------------
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "E-Commerce API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Paste ONLY your JWT below. Swagger will add 'Bearer ' automatically.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
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

            var app = builder.Build();

            // ----------------------
            // Pipeline
            // ----------------------
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Serilog request logging (place EARLY)
            app.UseSerilogRequestLogging(opts =>
            {
                opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();

            // Centralized error handling (your middleware)
            app.UseErrorHandling();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
