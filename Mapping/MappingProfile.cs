using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Models.Authorization;

namespace MedPal.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Patient, PatientReadDTO>().ReverseMap();
            CreateMap<Patient, PatientWriteDTO>().ReverseMap();
            CreateMap<Patient, Patient>().ReverseMap();
            CreateMap<User, UserReadDTO>().ReverseMap();
            CreateMap<User, UserWriteDTO>().ReverseMap();
            CreateMap<Clinic, ClinicReadDTO>().ReverseMap();
            CreateMap<Clinic, ClinicWriteDTO>().ReverseMap();
            CreateMap<PatientDetails, PatientDetailsReadDTO>().ReverseMap();
            CreateMap<PatientDetails, PatientDetailsWriteDTO>().ReverseMap();
            CreateMap<MedicalHistory, MedicalHistoryReadDTO>().ReverseMap();
            CreateMap<MedicalHistory, MedicalHistoryWriteDTO>().ReverseMap();
            CreateMap<Allergy, AllergyReadDTO>().ReverseMap();
            CreateMap<Allergy, AllergyWriteDTO>().ReverseMap();
            CreateMap<Appointment, AppointmentReadDTO>().ReverseMap();
            CreateMap<AppointmentWriteDTO, Appointment>().ReverseMap();

            // Permission mappings
            CreateMap<Permission, PermissionDTO>();

            // Role mappings
            CreateMap<Role, RoleReadDTO>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.RolePermissions.Select(rp => rp.Permission).ToList()));
            
            // UserRole mappings
            CreateMap<UserRole, UserRoleDTO>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));

            // Phase 4: EmergencyContact mappings
            CreateMap<EmergencyContact, EmergencyContactReadDTO>().ReverseMap();
            CreateMap<EmergencyContactWriteDTO, EmergencyContact>();

            // Phase 4: Payment mappings
            CreateMap<Payment, PaymentReadDTO>().ReverseMap();
            CreateMap<PaymentWriteDTO, Payment>();

            // Phase 4: Invoice mappings
            CreateMap<Invoice, InvoiceReadDTO>()
                .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.TotalAmount - src.PaidAmount))
                .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments.Where(p => !p.IsDeleted).ToList()));
            CreateMap<InvoiceWriteDTO, Invoice>();

            // Phase 4: NotificationMessage mappings
            CreateMap<NotificationMessage, NotificationMessageReadDTO>().ReverseMap();
            CreateMap<NotificationMessageWriteDTO, NotificationMessage>();
        }
    }
}