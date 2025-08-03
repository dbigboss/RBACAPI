using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RBACApi.Constants;
using RBACApi.Data;
using RBACApi.Middleware;
using RBACApi.Models;
using RBACApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.Admin, Roles.SuperAdmin));
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole(Roles.SuperAdmin));
    options.AddPolicy("UserOrAbove", policy => policy.RequireRole(Roles.User, Roles.Admin, Roles.SuperAdmin));
});

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IExceptionService, ExceptionService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[]{}
        }
    });
});

builder.Configuration.AddJsonFile("appsettings.json", false, true).AddEnvironmentVariables();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

// Add global exception handling middleware (should be one of the first middleware)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    //var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    //db.Database.Migrate();
    
    // Create roles if they don't exist
    string[] roles = { Roles.User, Roles.Admin, Roles.SuperAdmin };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    
    // Create super admin user if not exists
    if (await userManager.FindByEmailAsync("superadmin@rbac.com") == null)
    {
        var superAdmin = new ApplicationUser
        {
            UserName = "superadmin@rbac.com",
            Email = "superadmin@rbac.com",
            FirstName = "Super",
            LastName = "Admin",
            EmailConfirmed = true
        };
        
        await userManager.CreateAsync(superAdmin, "SuperAdmin123!");
        await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin);
    }
    
    // Create admin users if they don't exist
    if (await userManager.FindByEmailAsync("admin1@rbac.com") == null)
    {
        var admin1 = new ApplicationUser
        {
            UserName = "admin1@rbac.com",
            Email = "admin1@rbac.com",
            FirstName = "Admin",
            LastName = "One",
            EmailConfirmed = true
        };
        
        await userManager.CreateAsync(admin1, "Admin123!");
        await userManager.AddToRoleAsync(admin1, Roles.Admin);
    }
    
    if (await userManager.FindByEmailAsync("admin2@rbac.com") == null)
    {
        var admin2 = new ApplicationUser
        {
            UserName = "admin2@rbac.com",
            Email = "admin2@rbac.com",
            FirstName = "Admin",
            LastName = "Two",
            EmailConfirmed = true
        };
        
        await userManager.CreateAsync(admin2, "Admin123!");
        await userManager.AddToRoleAsync(admin2, Roles.Admin);
    }
}

app.Run();
