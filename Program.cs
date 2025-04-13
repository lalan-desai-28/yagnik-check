using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using ProductCatalog.IRepositories;
using ProductCatalog.Repositories;
using System.Reflection;
using System.Text;
using Task8Indentity.Models;
using Task8Indentity.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);
builder.Services.AddSingleton(mongoDatabase);

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
        configuration["MongoDB:ConnectionString"],
        configuration["MongoDB:DatabaseName"])
    .AddDefaultTokenProviders();

var jwtSettings = configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();


builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
  options.RequireHttpsMetadata = false;
  options.SaveToken = true;
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidIssuer = jwtSettings["Issuer"],
    ValidAudience = jwtSettings["Audience"],
    ClockSkew = TimeSpan.Zero
  };
});


builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "Task8Indentity",
    Version = "v1"
  });

  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JWT Authorization header using the Bearer scheme."
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
            new string[] { }
        }
    });

  var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
  var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
  options.IncludeXmlComments(xmlPath);
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
  var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
  await SeedRolesAsync(roleManager);
}

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
{
  string[] roleNames = { "Admin", "User", "Manager" };

  foreach (var roleName in roleNames)
  {
    var roleExists = await roleManager.RoleExistsAsync(roleName);
    if (!roleExists)
    {
      await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
    }
  }
}
