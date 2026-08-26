using Microsoft.EntityFrameworkCore;
using MedPal.API.Data;
using MedPal.API.Data.Seeders;
using MedPal.API.Repositories;
using MedPal.API.Repositories.Implementations;
using MedPal.API.Repositories.Authorization;
using MedPal.API.Mapping;
using MedPal.API.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MedPal.API.Services;
using MedPal.API.Services.Implementations;
using MedPal.API.Authorization;
using MedPal.API.Middleware;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Security.Claims;

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
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
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

// Register services
builder.Services.AddHttpContextAccessor();

// Register Tenant Context Service (Phase 2 - Multi-tenancy)
// MUST be registered BEFORE DbContext to avoid circular dependency
builder.Services.AddScoped<ITenantContextService, TenantContextService>();

// Register Patient Consent Service (Phase 3 - Consent and Audit)
builder.Services.AddScoped<IPatientConsentService, ConsentService>();

// Register Audit Interceptor for automatic change tracking
builder.Services.AddScoped<AuditSaveChangesInterceptor>();

// Configure DbContext with lazy loading proxies and SQL Server
// DbContext is registered after TenantContextService to avoid circular dependency
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options.AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseLazyLoadingProxies();
}, contextLifetime: ServiceLifetime.Scoped, optionsLifetime: ServiceLifetime.Scoped);

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IClinicRepository, ClinicRepository>();
builder.Services.AddScoped<IPatientDetailsRepository, PatientDetailsRepository>();
builder.Services.AddScoped<IMedicalHistoryRepository, MedicalHistoryRepository>();
builder.Services.AddScoped<IClinicalAttachmentRepository, ClinicalAttachmentRepository>();
builder.Services.AddScoped<IAttachmentStorageService, AttachmentStorageService>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// Authorization repositories
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();

// Audit service (NOM-024 compliance)
builder.Services.AddScoped<IRoleAuditService, RoleAuditService>();

// Medical Record Access Audit (NOM-004 compliance)
builder.Services.AddScoped<IMedicalRecordAccessLogService, MedicalRecordAccessLogService>();

// Entity Change Audit Log (automatic change tracking)
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// Prescription Services
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();

// ARCO Services
builder.Services.AddScoped<IArcoService, ArcoService>();

// Invoice and Payment Services (Phase 2 Completion)
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Emergency Contact Service and Repository (Phase 4)
builder.Services.AddScoped<IEmergencyContactService, EmergencyContactService>();
builder.Services.AddScoped<IEmergencyContactRepository, EmergencyContactRepository>();

// Payment Repository (Phase 4)
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Invoice Repository (Phase 4)
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

// Notification Message Repository (Phase 4)
builder.Services.AddScoped<INotificationMessageRepository, NotificationMessageRepository>();

// Patient Portal Auth
builder.Services.AddScoped<IPatientAuthRepository, PatientAuthRepository>();

// Vital Signs Repository (Signos Vitales for NOM-035 compliance)
builder.Services.AddScoped<IVitalSignRepository, VitalSignRepository>();

// Nutrition Module repositories
builder.Services.AddScoped<IFoodItemRepository, FoodItemRepository>();
builder.Services.AddScoped<IBodyCompositionRepository, BodyCompositionRepository>();
builder.Services.AddScoped<IAnthropometryRepository, AnthropometryRepository>();
builder.Services.AddScoped<IDietPlanRepository, DietPlanRepository>();
builder.Services.AddScoped<INutritionProgressRepository, NutritionProgressRepository>();
builder.Services.AddScoped<ISupplementRepository, SupplementRepository>();

// Nutrition Service
builder.Services.AddScoped<INutritionService, NutritionService>();

// Subscription services
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IPendingRegistrationRepository, PendingRegistrationRepository>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();

// Notification Services (Phase 3 + WhatsApp)
builder.Services.Configure<WhatsAppSettings>(builder.Configuration.GetSection("WhatsApp"));
builder.Services.AddHttpClient("WhatsApp");

// Register channels as themselves (not INotificationChannel)
builder.Services.AddSingleton<MockNotificationChannel>();
builder.Services.AddSingleton<WhatsAppCloudApiChannel>();

// Dispatcher routes by NotificationType — single INotificationChannel entry point
builder.Services.AddSingleton<INotificationChannel>(sp =>
{
    var channels = new List<INotificationChannel>
    {
        sp.GetRequiredService<MockNotificationChannel>()
    };

    if (builder.Configuration.GetValue<bool>("WhatsApp:Enabled"))
    {
        channels.Insert(0, sp.GetRequiredService<WhatsAppCloudApiChannel>());
    }

    var logger = sp.GetRequiredService<ILogger<NotificationDispatcher>>();
    return new NotificationDispatcher(channels, logger);
});

// Appointment Reminder Service + Background Job
builder.Services.AddScoped<IAppointmentReminderService, AppointmentReminderService>();
builder.Services.AddScoped<IWhatsAppInteractionHandler, WhatsAppInteractionHandler>();
builder.Services.AddHostedService<AppointmentReminderJob>();

// Encryption Service (Phase 4)
builder.Services.AddSingleton<EncryptionProvider>();

// FluentValidation (Phase 4)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register Authorization Handlers
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MedicalRecordAccessHandler>();
builder.Services.AddScoped<IAuthorizationHandler, PatientAccessHandler>();

// Configure Authorization Policies using modern builder pattern
builder.Services.AddAuthorizationBuilder()
    // Patient Permissions
    .AddPolicy("Patients.ViewAll", policy => policy.Requirements.Add(new PermissionRequirement("Patients.ViewAll")))
    .AddPolicy("Patients.ViewOwn", policy => policy.Requirements.Add(new PermissionRequirement("Patients.ViewOwn")))
    .AddPolicy("Patients.ViewAssigned", policy => policy.Requirements.Add(new PermissionRequirement("Patients.ViewAssigned")))
    .AddPolicy("Patients.Create", policy => policy.Requirements.Add(new PermissionRequirement("Patients.Create")))
    .AddPolicy("Patients.Update", policy => policy.Requirements.Add(new PermissionRequirement("Patients.Update")))
    .AddPolicy("Patients.Delete", policy => policy.Requirements.Add(new PermissionRequirement("Patients.Delete")))

    // Appointment Permissions
    .AddPolicy("Appointments.ViewAll", policy => policy.Requirements.Add(new PermissionRequirement("Appointments.ViewAll")))
    .AddPolicy("Appointments.ViewOwn", policy => policy.Requirements.Add(new PermissionRequirement("Appointments.ViewOwn")))
    .AddPolicy("Appointments.Create", policy => policy.Requirements.Add(new PermissionRequirement("Appointments.Create")))
    .AddPolicy("Appointments.Update", policy => policy.Requirements.Add(new PermissionRequirement("Appointments.Update")))
    .AddPolicy("Appointments.Cancel", policy => policy.Requirements.Add(new PermissionRequirement("Appointments.Cancel")))

    // Medical Records Permissions
    .AddPolicy("MedicalRecords.ViewAll", policy => policy.Requirements.Add(new PermissionRequirement("MedicalRecords.ViewAll")))
    .AddPolicy("MedicalRecords.ViewOwn", policy => policy.Requirements.Add(new PermissionRequirement("MedicalRecords.ViewOwn")))
    .AddPolicy("MedicalRecords.ViewAssigned", policy => policy.Requirements.Add(new PermissionRequirement("MedicalRecords.ViewAssigned")))
    .AddPolicy("MedicalRecords.Read", policy => policy.Requirements.Add(new PermissionRequirement("MedicalRecords.Read")))
    .AddPolicy("MedicalRecords.Create", policy => policy.Requirements.Add(new PermissionRequirement("MedicalRecords.Create")))
    .AddPolicy("MedicalRecords.Update", policy => policy.Requirements.Add(new PermissionRequirement("MedicalRecords.Update")))

    // Billing Permissions
    .AddPolicy("Billing.View", policy => policy.Requirements.Add(new PermissionRequirement("Billing.View")))
    .AddPolicy("Billing.Manage", policy => policy.Requirements.Add(new PermissionRequirement("Billing.Manage")))

    // User Management Permissions
    .AddPolicy("Users.ViewAll", policy => policy.Requirements.Add(new PermissionRequirement("Users.ViewAll")))
    .AddPolicy("Users.Manage", policy => policy.Requirements.Add(new PermissionRequirement("Users.Manage")))
    .AddPolicy("Users.ManageRoles", policy => policy.Requirements.Add(new PermissionRequirement("Users.ManageRoles")))

    // Reports Permissions
    .AddPolicy("Reports.Generate", policy => policy.Requirements.Add(new PermissionRequirement("Reports.Generate")))
    .AddPolicy("Reports.View", policy => policy.Requirements.Add(new PermissionRequirement("Reports.View")))

    // Clinic Management Permissions
    .AddPolicy("Clinics.View", policy => policy.Requirements.Add(new PermissionRequirement("Clinics.View")))
    .AddPolicy("Clinics.Manage", policy => policy.Requirements.Add(new PermissionRequirement("Clinics.Manage")))

    // Role Management Permissions
    .AddPolicy("Roles.View", policy => policy.Requirements.Add(new PermissionRequirement("Roles.View")))
    .AddPolicy("Roles.Assign", policy => policy.Requirements.Add(new PermissionRequirement("Roles.Assign")))
    .AddPolicy("Roles.Revoke", policy => policy.Requirements.Add(new PermissionRequirement("Roles.Revoke")))
    .AddPolicy("Roles.ViewAudit", policy => policy.Requirements.Add(new PermissionRequirement("Roles.ViewAudit")))

    // Fase 2: Multi-tenancy Authorization Policies
    .AddPolicy("ViewUsersPolicy", policy =>
    {
        policy.RequireAssertion(context =>
        {
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value switch
            {
                "SuperAdmin" => true,
                "AccountAdmin" => true,
                "ClinicAdmin" => true,
                _ => false
            };
        });
    })
    .AddPolicy("ViewPatientsPolicy", policy =>
    {
        policy.RequireAssertion(context =>
        {
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value switch
            {
                "SuperAdmin" => true,
                "AccountAdmin" => true,
                "ClinicAdmin" => true,
                "HealthProfessional" => true,
                "Nurse" => true,
                "Receptionist" => true,
                _ => false
            };
        });
    })
    .AddPolicy("ViewAppointmentsPolicy", policy =>
    {
        policy.RequireAssertion(context =>
        {
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value switch
            {
                "SuperAdmin" => true,
                "AccountAdmin" => true,
                "ClinicAdmin" => true,
                "HealthProfessional" => true,
                "Receptionist" => true,
                _ => false
            };
        });
    })
    .AddPolicy("ManageUsersPolicy", policy =>
    {
        policy.RequireAssertion(context =>
        {
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value switch
            {
                "SuperAdmin" => true,
                "AccountAdmin" => true,
                "ClinicAdmin" => true,
                _ => false
            };
        });
    })
    .AddPolicy("ManagePatientsPolicy", policy =>
    {
        policy.RequireAssertion(context =>
        {
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value switch
            {
                "SuperAdmin" => true,
                "AccountAdmin" => true,
                "ClinicAdmin" => true,
                "HealthProfessional" => true,
                _ => false
            };
        });
    })
    .AddPolicy("ViewAuditLogPolicy", policy =>
    {
        policy.RequireAssertion(context =>
        {
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value switch
            {
                "SuperAdmin" => true,
                "AccountAdmin" => true,
                _ => false
            };
        });
    })
    .AddPolicy("AdministerAccountPolicy", policy =>
    {
        policy.RequireAssertion(context =>
        {
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value switch
            {
                "SuperAdmin" => true,
                "AccountAdmin" => true,
                _ => false
            };
        });
    })
    .AddPolicy("AdministerClinicPolicy", policy =>
    {
        policy.RequireAssertion(context =>
        {
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value switch
            {
                "SuperAdmin" => true,
                "AccountAdmin" => true,
                "ClinicAdmin" => true,
                _ => false
            };
        });
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins(
            "http://localhost:4200", 
            "http://localhost:4201", 
            "http://localhost:4321", 
            "https://clinicflow.com.mx", 
            "https://app.clinicflow.com.mx", 
            "https://api.clinicflow.com.mx", 
            "https://portal.clinicflow.com.mx")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials());
});

var app = builder.Build();

// Seed authorization data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Aplicar migraciones pendientes (y crear la base de datos si no existe)
    await context.Database.MigrateAsync();
    
    await AuthorizationSeeder.SeedAsync(context);
    await SuperAdminSeeder.SeedSuperAdminAsync(context);
    
    // Inyectar datos Dummy sólo en Desarrollo
    if (app.Environment.IsDevelopment())
    {
        await DummyDataSeeder.SeedDummyDataAsync(context);
    }

    // Seed CIE-10 diagnostic codes catalog (always, only if empty)
    await Cie10Seeder.SeedAsync(context);

    // Seed Food Catalog (always, only if empty)
    await FoodCatalogSeeder.SeedAsync(context);

    // Seed Nutrition Data (always, only if empty)
    await NutritionDataSeeder.SeedAsync(context);

    // Seed Subscription Plans (always, only if empty)
    await SubscriptionSeeder.SeedAsync(context);
}

// Configure the HTTP request pipeline.
app.UseCors("AllowSpecificOrigin");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

// Exception Handling Middleware (Phase 4)
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
