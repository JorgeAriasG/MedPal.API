# Copilot Instructions - Medical Scheduling App Backend

> .NET 8 | Entity Framework Core | JWT + RBAC | Clean Architecture | SQL Server

## Quick Context

- **Project Type:** Medical scheduling API backend (appointments, patients, prescriptions, clinics, users, audit, consent)
- **Architecture:** Clean Architecture with Repository pattern, Service layer, Entity Framework Core
- **Key Tech:** .NET 8, Entity Framework Core 8.0.8, JWT Bearer, FluentValidation, AutoMapper, BCrypt
- **Database:** SQL Server with migrations
- **Status:** Phase 1-2 complete (auth, base models); ready for CRUD endpoints & business logic
- **Goal:** Complete API layer aligned with frontend contracts + implement all services

---

## Code Style & Conventions

### Language & Formatting
- **C#**: .NET 8, Nullable reference types enabled (`#nullable enable`)
- **Indentation:** 4 spaces (Visual Studio default)
- **Line length:** 120 chars max
- **Naming:** camelCase (variables/params), PascalCase (classes/interfaces/methods), CONSTANT_CASE (constants)
- **Async:** Always use `async/await`, no sync-over-async `.Result`
- **Namespaces:** Organized by folder structure: `MedPal.API.Controllers`, `MedPal.API.Services`, etc.

### Class Patterns

#### Controller (REST API)
```csharp
namespace MedPal.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using MedPal.API.Services;
using MedPal.API.DTOs.Request;
using MedPal.API.DTOs.Response;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IMapper _mapper;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(
        IPatientService patientService,
        IMapper mapper,
        ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all patients for the current tenant
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PatientResponse>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var patients = await _patientService.GetAllPatientsAsync(pageNumber, pageSize);
        return Ok(new ApiResponse<IEnumerable<PatientResponse>>
        {
            Data = patients,
            StatusCode = 200,
            Message = "Patients retrieved successfully"
        });
    }

    /// <summary>
    /// Get patient by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PatientResponse>>> GetById(int id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient == null)
            return NotFound(new ApiResponse<PatientResponse>
            {
                StatusCode = 404,
                Message = "Patient not found"
            });

        return Ok(new ApiResponse<PatientResponse>
        {
            Data = patient,
            StatusCode = 200,
            Message = "Patient retrieved successfully"
        });
    }

    /// <summary>
    /// Create new patient
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Doctor,Receptionist")]
    public async Task<ActionResult<ApiResponse<PatientResponse>>> Create(
        [FromBody] CreatePatientRequest request)
    {
        var result = await _patientService.CreatePatientAsync(request);
        
        _logger.LogInformation("Patient created with ID {PatientId}", result.Id);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            new ApiResponse<PatientResponse>
            {
                Data = result,
                StatusCode = 201,
                Message = "Patient created successfully"
            });
    }
}
```

#### Service (Business Logic)
```csharp
namespace MedPal.API.Services.Implementations;

using FluentValidation;
using AutoMapper;
using MedPal.API.Repositories;
using MedPal.API.Models;
using MedPal.API.DTOs.Request;
using MedPal.API.DTOs.Response;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _repository;
    private readonly IValidator<CreatePatientRequest> _validator;
    private readonly IMapper _mapper;
    private readonly ILogger<PatientService> _logger;
    private readonly ITenantContext _tenantContext;

    public PatientService(
        IPatientRepository repository,
        IValidator<CreatePatientRequest> validator,
        IMapper mapper,
        ILogger<PatientService> logger,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<PatientResponse>> GetAllPatientsAsync(int pageNumber, int pageSize)
    {
        var patients = await _repository.GetAllAsync(_tenantContext.AccountId, pageNumber, pageSize);
        return _mapper.Map<IEnumerable<PatientResponse>>(patients);
    }

    public async Task<PatientResponse> GetPatientByIdAsync(int id)
    {
        var patient = await _repository.GetByIdAsync(id);
        
        // Verify tenant access
        if (patient == null || patient.AccountId != _tenantContext.AccountId)
            return null;

        return _mapper.Map<PatientResponse>(patient);
    }

    public async Task<PatientResponse> CreatePatientAsync(CreatePatientRequest request)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Business logic
        var patient = new Patient
        {
            Name = request.Name,
            Email = request.Email.ToLowerInvariant(),
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address,
            Allergies = string.Join(",", request.Allergies ?? []),
            AccountId = _tenantContext.AccountId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _tenantContext.UserId
        };

        await _repository.AddAsync(patient);
        await _repository.SaveChangesAsync();

        _logger.LogInformation(
            "Patient created: {PatientId} by user {UserId} in account {AccountId}",
            patient.Id, _tenantContext.UserId, _tenantContext.AccountId);

        return _mapper.Map<PatientResponse>(patient);
    }
}
```

#### Entity Model (EF Core)
```csharp
namespace MedPal.API.Models;

public class Patient
{
    public int Id { get; set; }
    
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? Allergies { get; set; }
    
    // Multi-tenancy
    public int AccountId { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; } // Soft delete
    
    // Navigation
    public virtual ICollection<Appointment> Appointments { get; set; } = [];
    public virtual ICollection<Prescription> Prescriptions { get; set; } = [];
}
```

#### Validator (FluentValidation)
```csharp
namespace MedPal.API.Validation;

using FluentValidation;
using MedPal.API.DTOs.Request;

public class CreatePatientValidator : AbstractValidator<CreatePatientRequest>
{
    private readonly IPatientRepository _repository;

    public CreatePatientValidator(IPatientRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters")
            .MustAsync(BeUniqueEmail).WithMessage("Email already exists");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .Must(BeValidAge).WithMessage("Patient must be at least 18 years old");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[0-9\-\s()]+$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Invalid phone format");
    }

    private bool BeValidAge(DateTime dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        return age >= 18;
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return !await _repository.EmailExistsAsync(email.ToLowerInvariant());
    }
}
```

---

## Architecture & Project Structure

```
MedPal.API/
├── Controllers/              - HTTP endpoints
│   ├── AppointmentsController.cs
│   ├── AuthController.cs
│   ├── ClinicsController.cs
│   ├── PatientsController.cs
│   ├── PrescriptionsController.cs
│   ├── RolesController.cs
│   ├── UsersController.cs
│   └── HealthController.cs
├── Models/                  - Entity Framework entities
│   ├── User.cs
│   ├── Patient.cs
│   ├── Clinic.cs
│   ├── Appointment.cs
│   ├── Prescription.cs
│   ├── Role.cs
│   ├── Permission.cs
│   └── AuditLog.cs
├── DTOs/                    - Data Transfer Objects
│   ├── Request/
│   │   ├── CreatePatientRequest.cs
│   │   ├── UpdatePatientRequest.cs
│   │   └── ...
│   └── Response/
│       ├── PatientResponse.cs
│       ├── ApiResponse.cs
│       └── ...
├── Services/                - Service interfaces
│   └── IPatientService.cs
├── Services/Implementations/ - Service implementations
│   └── PatientService.cs
├── Repositories/            - Data access interfaces
│   └── IPatientRepository.cs
├── Repositories/Implementations/ - Repository implementations
│   └── PatientRepository.cs
├── Data/                    - Entity Framework context
│   ├── ApplicationDbContext.cs
│   ├── Configuration/       - Entity configurations
│   └── Seeders/             - Initial data
├── Migrations/              - EF Core migrations
├── Validation/              - FluentValidation validators
│   └── CreatePatientValidator.cs
├── Mapping/                 - AutoMapper profiles
│   └── AutoMapperProfile.cs
├── Authorization/           - Custom auth handlers
│   ├── PermissionHandler.cs
│   └── PermissionRequirement.cs
├── Middleware/              - Custom middleware
│   ├── ExceptionHandlingMiddleware.cs
│   ├── TenantResolverMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Enums/                   - Enum definitions
│   ├── UserRole.cs
│   └── Permission.cs
├── Interfaces/              - Shared interfaces
│   └── ITenantContext.cs
├── Program.cs               - Dependency injection setup
├── appsettings.json
├── appsettings.Development.json
└── MedPal.API.csproj
```

---

## Database & Entity Framework

### DbContext Pattern
```csharp
public sealed class ApplicationDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    // ... more

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters for soft delete & tenant isolation
        modelBuilder.Entity<Patient>()
            .HasQueryFilter(p => p.DeletedAt == null && p.AccountId == _tenantContext.AccountId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<IAuditable>();
        var now = DateTime.UtcNow;
        var userId = _tenantContext.UserId;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId;
            }
        }
    }
}
```

### Entity Configuration (Fluent API)
```csharp
namespace MedPal.API.Data.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedPal.API.Models;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.Phone)
            .HasMaxLength(20);

        // Indexes for performance
        builder.HasIndex(p => new { p.AccountId, p.Email })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");

        builder.HasIndex(p => new { p.AccountId, p.CreatedAt });

        // Relationships
        builder.HasMany(p => p.Appointments)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### Migrations
```bash
# Add new migration
dotnet ef migrations add AddPatientTable

# Apply migrations
dotnet ef database update

# View generated SQL
dotnet ef migrations script

# Revert migration
dotnet ef database update PreviousMigration
```

---

## Dependency Injection & Configuration

### Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* JWT config */ });

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireDoctor", policy => 
        policy.RequireRole("Doctor", "ClinicAdmin"));
});

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Mapping
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseExceptionHandling();
app.UseTenantResolver();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## Multi-Tenancy Implementation

### Tenant Context
```csharp
public interface ITenantContext
{
    int AccountId { get; }
    int ClinicId { get; }
    int UserId { get; }
    void SetTenant(int accountId, int clinicId, int userId);
}

public class TenantContext : ITenantContext
{
    public int AccountId { get; private set; }
    public int ClinicId { get; private set; }
    public int UserId { get; private set; }

    public void SetTenant(int accountId, int clinicId, int userId)
    {
        AccountId = accountId;
        ClinicId = clinicId;
        UserId = userId;
    }
}
```

### Tenant Resolver Middleware
```csharp
public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var accountId = context.User.FindFirst("accountId")?.Value;
        var clinicId = context.Request.Headers["X-Clinic-Id"].ToString();
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(accountId, out var accountIdInt) &&
            int.TryParse(clinicId, out var clinicIdInt) &&
            int.TryParse(userId, out var userIdInt))
        {
            tenantContext.SetTenant(accountIdInt, clinicIdInt, userIdInt);
        }

        await _next(context);
    }
}
```

---

## Error Handling

### Global Exception Handler
```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            StatusCode = context.Response.StatusCode,
            Message = GetMessage(exception),
            Data = null
        };

        if (exception is ValidationException validationEx)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            response.StatusCode = 400;
            response.Message = "Validation failed";
        }
        else if (exception is KeyNotFoundException)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            response.StatusCode = 404;
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            response.StatusCode = 500;
        }

        return context.Response.WriteAsJsonAsync(response);
    }

    private static string GetMessage(Exception ex) =>
        ex switch
        {
            ValidationException => "Please check input and try again",
            KeyNotFoundException => "Resource not found",
            _ => "An error occurred while processing your request"
        };
}
```

---

## API Response Format

```csharp
public class ApiResponse<T>
{
    public T? Data { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public IEnumerable<string>? Errors { get; set; }
}
```

---

## Build & Run Commands

```bash
# Build
dotnet build

# Run
dotnet run

# Run watch mode (hot reload)
dotnet watch run

# Run tests
dotnet test

# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# View SQL
dotnet ef migrations script
```

---

## Project Conventions

### Naming
- **Classes**: PascalCase (`PatientService`, `CreatePatientValidator`)
- **Methods**: PascalCase (`GetPatientByIdAsync`, `CreatePatientAsync`)
- **Variables/Parameters**: camelCase (`patientId`, `pageSize`)
- **Constants**: CONSTANT_CASE (`DEFAULT_PAGE_SIZE = 10`)
- **Interfaces**: Prefix `I` (`IPatientService`, `ITenantContext`)

### Async Patterns
- **ALWAYS use async/await**: No `.Result` or `.Wait()`
- **Task return**: Async void only for event handlers
- **Naming**: `*Async` suffix on async methods
- **Cancellation**: Accept `CancellationToken cancellationToken`

### Logging
```csharp
_logger.LogInformation("Patient created with ID {PatientId}", patientId);
_logger.LogWarning("Patient not found: {PatientId}", patientId);
_logger.LogError(ex, "Error creating patient {PatientName}", patientName);
```

### Security
- **ALWAYS**: Validate input (FluentValidation)
- **ALWAYS**: Check authorization before operations
- **ALWAYS**: Filter by tenant (AccountId, ClinicId)
- **ALWAYS**: Hash passwords (BCrypt)
- **NEVER**: Log sensitive data (passwords, tokens)
- **NEVER**: Return stack traces to client

---

## Testing Setup

```bash
# Create test project
dotnet new xunit -n MedPal.API.Tests

# Run tests
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

---

## Integration with Frontend

### API Contract
- **Base URL**: `http://localhost:5126/api/`
- **Authentication**: Bearer JWT in `Authorization` header
- **Response Format**: Always `{ data: T, statusCode: number, message: string }`
- **Error Format**: `{ statusCode: number, message: string, errors?: string[] }`

### Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
X-Clinic-Id: 1
X-Account-Id: 1
Content-Type: application/json
```

---

## Security & Auth

### Key Principles
1. **Validate everything**: FluentValidation on all inputs
2. **Check permissions**: [Authorize] + [Authorize(Policy = "...")] on endpoints
3. **Filter tenant data**: All queries must filter by AccountId
4. **Hash passwords**: BCrypt with workFactor >= 10
5. **No sensitive logs**: Never log passwords, tokens, PII

---

## Next Steps for Completion

1. **Create all DTOs**: Request/Response for each entity
2. **Create all Validators**: FluentValidation for each request
3. **Create all Repositories**: Data access layer
4. **Create all Services**: Business logic implementations
5. **Create all Controllers**: HTTP endpoints with auth/validation
6. **Add Migrations**: Database schema
7. **Seed Data**: Initial test data
8. **Write Tests**: Unit + integration tests

---

## Quick Reference Links

- **Entity Framework Core**: https://docs.microsoft.com/ef/core/
- **FluentValidation**: https://fluentvalidation.net/
- **AutoMapper**: https://automapper.org/
- **JWT**: https://jwt.io/
- **Clean Architecture**: https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html

---

**Version**: 1.0  
**Last Updated**: March 22, 2026  
**Specialized Agents**: @backendagent, @secopsagent, @qaagent
