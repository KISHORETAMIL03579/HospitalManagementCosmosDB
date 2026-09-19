using System.Text;
using HospitalManagement.Application;
using HospitalManagement.Infrastructure.Injection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

// Load .env file if present
DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Register Services
builder.Services.AddControllers();

// Configure Swagger with JWT Bearer security definition
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hospital Management API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme. Enter token directly.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(
        (doc) =>
            new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() },
            }
    );
});

builder.Services.AddProblemDetails();

// Configure Authentication (JWT Bearer)
var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key configuration is missing.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "JWT Key must be at least 256 bits (32 bytes) long for HMAC SHA256 security."
    );
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "HospitalAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "HospitalUsers";

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureCosmos(builder.Configuration);

var app = builder.Build();

// Configure Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

// Initialize Cosmos DB
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var cosmosOptions = services.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
        var cosmosClient = services.GetRequiredService<CosmosClient>();

        logger.LogInformation("Database: {Database}", cosmosOptions.DatabaseId);

        foreach (var container in cosmosOptions.Containers)
        {
            logger.LogInformation(
                "Container: {Container}, Partition Key: {PartitionKey}",
                container.ContainerId,
                container.PartitionKeyPath
            );
        }

        await CosmosInitializer.InitializeAsync(cosmosClient, cosmosOptions);

        logger.LogInformation("Cosmos DB initialized successfully.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Failed to initialize Cosmos DB.");
        throw;
    }
}

app.MapControllers();

app.Run();
