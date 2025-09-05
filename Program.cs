
using E_CommerceSystem.Repositories;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using AutoMapper; // Add this using directive for AutoMapper
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

using E_CommerceSystem.Services.Email;


//using static E_CommerceSystem.Mapping.CategoryProfile;
//using static E_CommerceSystem.Mapping.OrderProfile;
//using static E_CommerceSystem.Mapping.ProductProfile;
//using static E_CommerceSystem.Mapping.ReviewProfile;
//using static E_CommerceSystem.Mapping.SupplierProfile;
//using static E_CommerceSystem.Mapping.UserProfile;


namespace E_CommerceSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();

            // Add services to the container.
            builder.Services.AddScoped<IUserRepo,UserRepo>();
            builder.Services.AddScoped<IUserService,UserService>();


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




            // Auto Mapper Configurations
            builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);











            //builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                  options.UseLazyLoadingProxies() //to enable lazy loading ...
                  .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add JWT Authentication
            //var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            //var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing.");


            //// Add JWT Authentication
            //var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            //var secretKey = jwtSettings["SecretKey"];

            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                            ?? throw new InvalidOperationException("JwtSettings:SecretKey missing");




            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                     .AddJwtBearer(options =>
                     {
                         options.TokenValidationParameters = new TokenValidationParameters
                         {
                             ValidateIssuer = false, // You can set this to true if you want to validate the issuer.
                             ValidateAudience = false, // You can set this to true if you want to validate the audience.
                             ValidateLifetime = true, // Ensures the token hasn't expired.
                             ValidateIssuerSigningKey = true, // Ensures the token is properly signed.
                             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)) // Match with your token generation key.
                         };
                         // Read token from cookie if Authorization header is missing
                         options.Events = new JwtBearerEvents
                         {
                             OnMessageReceived = context =>
                             {
                                 if (string.IsNullOrEmpty(context.Token))
                                 {
                                     var cookieToken = context.Request.Cookies["access_token"];
                                     if (!string.IsNullOrEmpty(cookieToken))
                                     {
                                         context.Token = cookieToken;
                                     }
                                 }
                                 return Task.CompletedTask;
                             }
                         };
                     });
            builder.Services.AddAuthorization(o =>
            {
                o.AddPolicy("Reports.Read", p => p.RequireRole("Admin", "Manager"));

                // o.AddPolicy("Reports.Read", p => p.RequireClaim("permission", "reports.read"));
            });

            builder.Services.AddEndpointsApiExplorer();



            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "E-Commerce API", Version = "v1" });
                // Use HTTP Bearer so Swagger auto-prefixes "Bearer "
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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            //new string[] {}
            Array.Empty<string>()
        }
    });
            });



            var app = builder.Build();
            app.UseHttpsRedirection(); 

            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication(); //jwt check middleware
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
