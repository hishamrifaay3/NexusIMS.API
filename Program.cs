using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NexusIMS.API.Custom_Middleware;
using NexusIMS.API.Data;
using NexusIMS.API.Entities;
using NexusIMS.API.Services;
using System.Text;

namespace NexusIMS.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

           
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

           
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                };
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("StrictTeamPolicy",
                    policy =>
                    {
                        policy.WithOrigins(
                                "https://luxora-restaurant.com", // دومين الشركة الحقيقي
                                "http://localhost:3000",        // جهاز زميلك بتاع الفرونت إند (React مثلاً)
                                "https://your-frontend-team.vercel.app" // لو رافعين الفرونت تجريبي
                        )
                              .AllowAnyMethod() // بيسمح بـ GET, POST, PUT, DELETE
                              .AllowAnyHeader() // بيسمح ببعت الـ Token في الهيدر
                              .AllowCredentials(); // لو هتبعت Cookies أو Tokens حساسة
                    });
            });


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

           
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<DbSeeder>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IWarehouseService, WarehouseService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IStockTransactionService, StockTransactionService>();
            builder.Services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
            builder.Services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();

            // -------------------------------------------------------------
            var app = builder.Build();
            // -------------------------------------------------------------

 
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(); 
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "NexusIMS API V1");
                    options.RoutePrefix = "swagger"; 
                });
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // تنفيذ كود الـ Seeding للبيانات الأولية عند بدء تشغيل السيرفر
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var seeder = services.GetRequiredService<DbSeeder>();
                    await seeder.SeedAllAsync();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            app.Run();
        }
    }
}