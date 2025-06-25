using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Filters;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.Service;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using DotNetEnv;

internal class Program
{
    private static void Main(string[] args)
    {
        Env.Load(); // Tải biến môi trường từ file .env nếu có

        var builder = WebApplication.CreateBuilder(args);

        // Thêm dịch vụ CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin", policy =>
            {
                // Chỉ cho phép yêu cầu từ địa chỉ cụ thể (ví dụ: frontend đang chạy trên localhost:5173)
                policy.WithOrigins(
                    "http://localhost:5173",
                    "https://terra-tech-garden.vercel.app")  // Địa chỉ của frontend
                      .AllowAnyMethod()     // Cho phép bất kỳ phương thức HTTP nào (GET, POST, PUT, DELETE, ...)
                      .AllowAnyHeader();    // Cho phép bất kỳ header nào trong yêu cầu
            });

            // Hoặc bạn có thể cho phép tất cả các nguồn:
            // options.AddPolicy("AllowAll", policy =>
            // {
            //     policy.AllowAnyOrigin()
            //           .AllowAnyMethod()
            //           .AllowAnyHeader();
            // });
        });


        var dsads = builder.Configuration["ConnectionStrings:DefaultConnectionString"];

        // Lấy cấu hình JWT
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings.GetValue<string>("SecretKey");
        builder.Services.AddDbContext<TerrariumGardenTechDBContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");
            options.UseSqlServer(connectionString);
        });


        // Đăng ký Repository và UnitOfWork
        builder.Services.AddScoped(typeof(GenericRepository<>));
        builder.Services.AddScoped<UnitOfWork>();

        // Đăng ký Service
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ITerrariumService, TerrariumService>();
        builder.Services.AddScoped<IAccessoryService, AccessoryService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IBlogService, BlogService>();
        builder.Services.AddScoped<IBlogCategoryService, BlogCategoryService>();
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IMembershipService, MembershipService>();
        builder.Services.AddScoped<IPersonalizeService, PersonalizeService>();

        // Đăng ký thêm service quản lý tài khoản Staff/Manager cho Admin CRUD
        builder.Services.AddScoped<IAccountService, AccountService>();

        // Đăng ký cấu hình SMTP
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

        // Thêm các dịch vụ vào container DI
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
        })
        .AddCookie()
        .AddGoogle(options =>
        {
            options.ClientId = builder.Configuration["GoogleKeys:ClientId"];
            options.ClientSecret = builder.Configuration["GoogleKeys:ClientSecret"];
        });

        // Cấu hình Authentication với JWT Bearer và logging
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
                ValidAudience = jwtSettings.GetValue<string>("Audience"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(context.Exception, "Authentication failed.");
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("Unauthorized request: {Path}", context.HttpContext.Request.Path);

                    // Trả về JSON rõ ràng khi lỗi 401
                    context.HandleResponse();
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new { message = "Chưa xác thực, vui lòng đăng nhập." });
                    return context.Response.WriteAsync(result);
                },
                OnForbidden = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("Forbidden request: {Path} - User does not have permission.", context.HttpContext.Request.Path);

                    // Trả về JSON rõ ràng khi lỗi 403
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new { message = "Bạn không có quyền truy cập tài nguyên này." });
                    return context.Response.WriteAsync(result);
                }
            };
        });

        // Đăng ký Controller
        builder.Services.AddControllers();

        //// Cấu hình Swagger/OpenAPI
        //builder.Services.AddSwaggerGen(c =>
        //{
        //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TerrariumGardenTech API", Version = "v1" });

        //    // Cấu hình JWT trong Swagger để có nút Authorize
        //    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        //    {
        //        Description = @"JWT Authorization header using the Bearer scheme. 
        //                        Enter 'Bearer' [space] and then your token in the text input below.
        //                        Example: 'Bearer abcdef12345'",
        //        Name = "Authorization",
        //        In = ParameterLocation.Header,
        //        Type = SecuritySchemeType.ApiKey,
        //        Scheme = "Bearer"
        //    });

        //    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
        //    {
        //        {
        //            new OpenApiSecurityScheme
        //            {
        //                Reference = new OpenApiReference
        //                {
        //                    Type = ReferenceType.SecurityScheme,
        //                    Id = "Bearer"
        //                },
        //                Scheme = "oauth2",
        //                Name = "Bearer",
        //                In = ParameterLocation.Header,
        //            },
        //            new List<string>()
        //        }
        //    });
        //});

        // Cấu hình Swagger/OpenAPI
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "TerrariumGardenTech API", Version = "v1" });

            // Thêm OperationFilter để hiển thị Authorization cho refresh-token
            c.OperationFilter<AddAuthorizationHeaderOperationFilter>();
            // Cấu hình JWT trong Swagger để có nút Authorize
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. 
                        Enter 'Bearer' [space] and then your token in the text input below.
                        Example: 'Bearer abcdef12345'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
            });


        });

        var app = builder.Build();

        //// Middleware trả về JSON khi lỗi 401 hoặc 403
        //app.Use(async (context, next) =>
        //{
        //    await next();

        //    if (context.Response.StatusCode == 401)
        //    {
        //        context.Response.ContentType = "application/json";
        //        await context.Response.WriteAsync("{\"message\":\"Chưa xác thực, vui lòng đăng nhập.\"}");
        //    }
        //    else if (context.Response.StatusCode == 403)
        //    {
        //        context.Response.ContentType = "application/json";
        //        await context.Response.WriteAsync("{\"message\":\"Bạn không có quyền truy cập tài nguyên này.\"}");
        //    }
        //});

        // Middleware trả về JSON khi lỗi 401 hoặc 403
        app.Use(async (context, next) =>
        {
            await next();

            // Kiểm tra nếu response đã bắt đầu thì không thực hiện ghi dữ liệu thêm
            if (!context.Response.HasStarted)
            {
                if (context.Response.StatusCode == 401)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"message\":\"Chưa xác thực, vui lòng đăng nhập.\"}");
                }
                else if (context.Response.StatusCode == 403)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"message\":\"Bạn không có quyền truy cập tài nguyên này.\"}");
                }
            }
        });



        // Áp dụng middleware CORS
        app.UseCors("AllowSpecificOrigin");  // Hoặc "AllowAll" nếu bạn cấu hình chính sách AllowAnyOrigin


        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TerrariumGardenTech API V1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.MapControllers();

        app.Run();
    }
}