using DotNetEnv;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using System.Text.Json;
using TerrariumGardenTech.API.Authorization;
using TerrariumGardenTech.API.Middlewares;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Repositories.Repositories;
using TerrariumGardenTech.Service.Configs;
using TerrariumGardenTech.Service.Filters;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.Mappers;
using TerrariumGardenTech.Service.Service;

Env.Load(); // Tải biến môi trường từ file .env nếu có

var builder = WebApplication.CreateBuilder(args);

FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile("notification-terrariumtech-firebase-adminsdk.json")
});
// Thêm dịch vụ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        // Chỉ cho phép yêu cầu từ địa chỉ cụ thể (ví dụ: frontend đang chạy trên localhost:5173)
        policy.WithOrigins("http://localhost:5173") // Địa chỉ của frontend
            .WithOrigins("https://terra-tech-garden.vercel.app")
            .AllowAnyMethod() // Cho phép bất kỳ phương thức HTTP nào (GET, POST, PUT, DELETE, ...)
            .AllowAnyHeader(); // Cho phép bất kỳ header nào trong yêu cầu
    });

    // Hoặc bạn có thể cho phép tất cả các nguồn:
    // options.AddPolicy("AllowAll", policy =>
    // {
    //     policy.AllowAnyOrigin()
    //           .AllowAnyMethod()
    //           .AllowAnyHeader();
    // });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<MappingProfile>(); cfg.AddProfile<FeedbackProfile>(); });
// Thêm AutoMapper

var dsads = builder.Configuration["ConnectionStrings:DefaultConnectionString"];

// Lấy cấu hình JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings.GetValue<string>("SecretKey");
builder.Services.AddDbContext<TerrariumGardenTechDBContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");
    options.UseSqlServer(connectionString);
});

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
// Đăng ký Repository và UnitOfWork
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped<UnitOfWork>();

/*---------------- Repositorys ----------------*/
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<OrderItemRepository>();
builder.Services.AddScoped<MembershipPackageRepository>();
builder.Services.AddScoped<ICartService, CartService>(); // Đăng ký CartService vào DI container


// Đăng ký Service
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITerrariumService, TerrariumService>();
builder.Services.AddScoped<IAccessoryService, AccessoryService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IBlogCategoryService, BlogCategoryService>();
builder.Services.AddScoped<IRoleService, RoleService>();
//membership
builder.Services.AddScoped<IMembershipPackageService, MembershipPackageService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
//
builder.Services.AddScoped<IShapeService, ShapeService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IEnvironmentService, EnvironmentService>();
builder.Services.AddScoped<IFirebaseStorageService, FirebaseStorageService>();
builder.Services.AddScoped<ITankMethodService, TankMethodService>();
builder.Services.AddScoped<ITerrariumImageService, TerrariumImageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITerrariumVariantService, TerrariumVariantService>();
builder.Services.AddScoped<IPersonalizeService, PersonalizeService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IAccessoryImageService, AccessoryImageService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IPayOsService, PayOsService>();

builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<ICartService, CartService>();

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

/*---------------- Authorization ----------------*/
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("Order.ReadAll",
        p => p.RequireRole("Staff", "Manager", "Admin", "Shipper")); // Thêm "Shipper" vào đây

    opt.AddPolicy("Order.UpdateStatus",
        p => p.RequireRole("Staff", "Manager", "Admin", "Shipper"));

    opt.AddPolicy("Order.Delete",
        p => p.RequireRole("Manager", "Admin"));

    opt.AddPolicy("Order.AccessSpecific",
        p => p.AddRequirements(new OrderAccessRequirement())); // resource-based
});

// Handler DI
builder.Services.AddScoped<IAuthorizationHandler, OrderAccessHandler>();

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
                var result = JsonSerializer.Serialize(new { message = "Chưa xác thực, vui lòng đăng nhập." });
                return context.Response.WriteAsync(result);
            },
            OnForbidden = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Forbidden request: {Path} - User does not have permission.",
                    context.HttpContext.Request.Path);

                // Trả về JSON rõ ràng khi lỗi 403
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new { message = "Bạn không có quyền truy cập tài nguyên này." });
                return context.Response.WriteAsync(result);
            }
        };
    });

// Đăng ký Controller
builder.Services.AddControllers();
//.AddJsonOptions(options =>
//{
//    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
//    options.JsonSerializerOptions.WriteIndented = true; // (tuỳ chọn) giúp JSON đẹp hơn
//});


// Cấu hình Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TerrariumGardenTech API", Version = "v1" });

    // Thêm OperationFilter để hiển thị Authorization cho refresh-token
    c.OperationFilter<AddAuthorizationHeaderOperationFilter>();
    // Đăng ký OperationFilter cho file upload
    c.OperationFilter<FileUploadOperationFilter>();

    // Cấu hình JWT trong Swagger để có nút Authorize
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"Please enter your JWT Token in the text input below. Example: 'eyJhbGciOiJIUzI1Ni...'
                       (NOTE: Do not add 'Bearer ' prefix. Swagger will automatically add it.)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http, // Đổi từ ApiKey sang Http
        Scheme = "Bearer" // Giữ nguyên scheme là "Bearer"
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
                },
                // Scheme và Name không cần đặt lại ở đây
            },
            new List<string>() // Đây là phạm vi (scopes) - để trống nếu không có
        }
    });
});

var app = builder.Build();

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
app.UseCors("AllowSpecificOrigin"); // Hoặc "AllowAll" nếu bạn cấu hình chính sách AllowAnyOrigin

app.UseGlobalExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json",
            "TerrariumGardenTech API V1"); // Định nghĩa endpoint của Swagger
        c.RoutePrefix = "swagger"; // Swagger UI sẽ có sẵn ở /swagger
        c.DocumentTitle = "TerrariumGardenTech API"; // Tiêu đề của Swagger UI
        c.DefaultModelsExpandDepth(-1); // Tùy chọn: Tắt hiển thị model
        c.EnableDeepLinking(); // Bật liên kết sâu
        c.EnableFilter(); // Bật thanh tìm kiếm trong Swagger UI
    });
}

app.MapControllers();

app.Run();