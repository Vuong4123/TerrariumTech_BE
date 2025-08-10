using DotNetEnv;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

//FirebaseApp.Create(new AppOptions
//{
//    Credential = GoogleCredential.FromFile("notification-terrariumtech-firebase-adminsdk.json")
//});
// Thêm dịch vụ CORS
const string CorsPolicy = "AllowFrontend";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(CorsPolicy, policy =>
        {
            policy
                .WithOrigins(
                    "https://terra-tech-garden.vercel.app", // FE prod
                    "http://localhost:5173"                 // FE dev
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();                       // Nếu FE gửi cookie/Authorization
        });
    });
// ---------- Core services ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(cfg =>
{ 
    cfg.AddProfile<MappingProfile>(); 
    cfg.AddProfile<FeedbackProfile>(); 
});
// Thêm AutoMapper

var dsads = builder.Configuration["ConnectionStrings:DefaultConnectionString"];


builder.Services.AddDbContext<TerrariumGardenTechDBContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");
    options.UseSqlServer(connectionString);
});

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
// Đăng ký Repository và UnitOfWork
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddHttpClient();
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
builder.Services.AddScoped<ITransportService, TransportService>();
//membership
builder.Services.AddScoped<IMembershipPackageService, MembershipPackageService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
//
builder.Services.AddScoped<IShapeService, ShapeService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IEnvironmentService, EnvironmentService>();
builder.Services.AddScoped<IFirebaseStorageService, FirebaseStorageService>();
builder.Services.AddScoped<IFirebasePushService, FirebasePushService>();
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
builder.Services.AddScoped<IWalletServices, WalletServices>();
builder.Services.AddScoped<IMomoServices, MomoServices>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

// Đăng ký SignalR cho real-time chat
builder.Services.AddSignalR();

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

    // Cho phép cả User (và các role đặc quyền) truy cập GET /api/order/{id},
    // sau đó handler OrderAccessRequirement sẽ tiếp tục kiểm tra xem
    // nếu là User thì phải là chủ đơn (order.UserId == User.GetUserId()).
    opt.AddPolicy("Order.AccessSpecific", p =>
    {
        p.RequireRole("Staff", "Manager", "Admin", "Shipper", "User");
        p.AddRequirements(new OrderAccessRequirement());
    });
});

// Handler DI
builder.Services.AddScoped<IAuthorizationHandler, OrderAccessHandler>();

// Lấy cấu hình JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings.GetValue<string>("SecretKey");

// Cấu hình Authentication với JWT Bearer và logging
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
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

                // Bỏ qua nếu là SignalR/WebSocket connection
                if (context.HttpContext.Request.Path.StartsWithSegments("/chatHub"))
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }

                // Trả về JSON rõ ràng khi lỗi 401 cho REST API
                context.HandleResponse();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 401;
                var result = JsonSerializer.Serialize(new { message = "Chưa xác thực, vui lòng đăng nhập." });
                return context.Response.WriteAsync(result);
            },
            OnForbidden = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Forbidden request: {Path} - User does not have permission.",
                    context.HttpContext.Request.Path);

                // Skip response body for WebSocket requests (SignalR)
                if (context.HttpContext.Request.Path.StartsWithSegments("/chatHub"))
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                }

                // Trả về JSON rõ ràng khi lỗi 403 cho REST API
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 403;
                var result = JsonSerializer.Serialize(new { message = "Bạn không có quyền truy cập tài nguyên này." });
                return context.Response.WriteAsync(result);
            }
        };
    });

//// Đăng ký Controller
//builder.Services.AddControllers()
//    .AddJsonOptions(options =>
//    {
//        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
//    });
// Đăng ký Controller

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 1. Viết các enum ra JSON dưới dạng camel-case (ví dụ: "pending")
        // 2. Khi đọc, cho phép case-insensitive và cả giá trị số (1, 2, 3…)
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(
                JsonNamingPolicy.CamelCase,
                allowIntegerValues: true
            )
        );

        // (Tuỳ chọn) nếu bạn muốn tất cả các property name cũng dùng camel-case:
        // options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

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
app.UseCors(CorsPolicy); // Hoặc "AllowAll" nếu bạn cấu hình chính sách AllowAnyOrigin

app.UseGlobalExceptionHandler();
app.UseHttpsRedirection();

// Enable static files (for wwwroot folder)
app.UseStaticFiles();

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

// Map SignalR ChatHub
app.MapHub<TerrariumGardenTech.API.Hubs.ChatHub>("/chatHub");

app.Run();