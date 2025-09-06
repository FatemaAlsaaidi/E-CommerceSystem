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
using E_CommerceSystem.AdminDashboard;

namespace E_CommerceSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // 1)Configure Serilog (inside the Main method to avoid "global code" and CS7022 warning)
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .WriteTo.File("Logs/app-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // 2)Replace the default logging pipeline with Serilog
                builder.Host.UseSerilog();

                builder.Services.AddControllers();

                // 3)DI – Warehousing and Services
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

                builder.Services.AddScoped<IAdminDashboardService,AdminDashboardService>();


                builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
                builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
                builder.Services.AddTransient<IInvoiceService, InvoiceService>();

                builder.Services.AddScoped<IRefreshTokenRepo, RefreshTokenRepo>();
                builder.Services.AddScoped<IAuthService, AuthService>();

                // 4) AutoMapper
                builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);

                // 5) DbContext
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseLazyLoadingProxies()
                           .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

                // 6) JWT
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

                    // Allow reading the token from the cookie if it is not present in the header.
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

                // 7) Swagger
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

                // 8) Pipeline order
                app.UseHttpsRedirection();
                app.UseStaticFiles();

                // Log all requests 
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

                // centralized error handling middleware
                app.UseErrorHandling();

                app.UseAuthorization();

                app.MapControllers();

                app.Run();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
