using Microsoft.EntityFrameworkCore;
using MedPal.API.Data;
using MedPal.API.Data.Seeders;
using MedPal.API.Repositories;
using MedPal.API.Repositories.Implementations;
using MedPal.API.Repositories.Authorization;
using MedPal.API.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MedPal.API.Services;
using MedPal.API.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
// JWT Auth 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(option =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("JWT key is not configured. Please set 'Jwt:Key' in configuration.");
        }

        option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        // Events para debuggear JWT
        option.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            }
        };
    });

// Add services to the container.
builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new MedPal.API.Serialization.TimeOnlyJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new MedPal.API.Serialization.DateOnlyJsonConverter());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "MedPal API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddAutoMapper(typeof(MappingProfile)); // Ensure this line is present

// Configure DbContext with lazy loading proxies and SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseLazyLoadingProxies());

// Register services
builder.Services.AddHttpContextAccessor();

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IClinicRepository, ClinicRepository>();
builder.Services.AddScoped<IPatientDetailsRepository, PatientDetailsRepository>();
builder.Services.AddScoped<IMedicalHistoryRepository, MedicalHistoryRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();

// Authorization repositories
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();

// Register Authorization Handlers
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MedicalRecordAccessHandler>();

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Patient Permissions
    options.AddPolicy("Patients.ViewAll", policy => policy.Requirements.Add(new PermissionRequirement("Patients.ViewAll")));
    options.AddPolicy("Patients.ViewOwn", policy => policy.Requirements.Add(new PermissionRequirement("Patients.ViewOwn")));
    options.AddPolicy("Patients.Create", policy => policy.Requirements.Add(new PermissionRequirement("Patients.Create")));
    options.AddPolicy("Patients.Update", policy => policy.Requirements.Add(new PermissionRequirement("Patients.Update")));
    options.AddPolicy("Patients.Delete", policy => policy.Requirements.Add(new PermissionRequirement("Patients.Delete")));

    // Appointment Permissions
    options.AddPolicy("Appointments.ViewAll", policy => policy.Requirements.Add(new PermissionRequirement("Appointments.ViewAll")));
    options.AddPolicy("Appointments.ViewOwn", policy => policy.Requirements.Add(new PermissionRequirement("Appointments.ViewOwn")));
    options.AddPolicy("Appointments.Create", policy => policy.Requirements.Add(new PermissionRequirement("Appointments.Create")));
    options.AddPolicy("Appointments.Update", policy => policy.Requirements.Add(new PermissionRequirement("Appointments.Update")));
    options.AddPolicy("Appointments.Cancel", policy => policy.Requirements.Add(new PermissionRequirement("Appointments.Cancel")));

    // Medical Records Permissions
    options.AddPolicy("MedicalRecords.ViewAll", policy => policy.Requirements.Add(new PermissionRequirement("MedicalRecords.ViewAll")));
    options.AddPolicy("MedicalRecords.ViewOwn", policy => policy.Requirements.Add(new PermissionRequirement("MedicalRecords.ViewOwn")));
    options.AddPolicy("MedicalRecords.ViewAssigned", policy => policy.Requirements.Add(new PermissionRequirement("MedicalRecords.ViewAssigned")));
    options.AddPolicy("MedicalRecords.Create", policy => policy.Requirements.Add(new PermissionRequirement("MedicalRecords.Create")));
    options.AddPolicy("MedicalRecords.Update", policy => policy.Requirements.Add(new PermissionRequirement("MedicalRecords.Update")));

    // Billing Permissions
    options.AddPolicy("Billing.View", policy => policy.Requirements.Add(new PermissionRequirement("Billing.View")));
    options.AddPolicy("Billing.Manage", policy => policy.Requirements.Add(new PermissionRequirement("Billing.Manage")));

    // User Management Permissions
    options.AddPolicy("Users.ViewAll", policy => policy.Requirements.Add(new PermissionRequirement("Users.ViewAll")));
    options.AddPolicy("Users.Manage", policy => policy.Requirements.Add(new PermissionRequirement("Users.Manage")));
    options.AddPolicy("Users.ManageRoles", policy => policy.Requirements.Add(new PermissionRequirement("Users.ManageRoles")));

    // Reports Permissions
    options.AddPolicy("Reports.Generate", policy => policy.Requirements.Add(new PermissionRequirement("Reports.Generate")));
    options.AddPolicy("Reports.View", policy => policy.Requirements.Add(new PermissionRequirement("Reports.View")));

    // Clinic Management Permissions
    options.AddPolicy("Clinics.View", policy => policy.Requirements.Add(new PermissionRequirement("Clinics.View")));
    options.AddPolicy("Clinics.Manage", policy => policy.Requirements.Add(new PermissionRequirement("Clinics.Manage")));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:4200")
                            .AllowAnyHeader()
                            .AllowAnyMethod());
});

var app = builder.Build();

// Seed authorization data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await AuthorizationSeeder.SeedAsync(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowSpecificOrigin");
} else
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllers();

app.Run();
