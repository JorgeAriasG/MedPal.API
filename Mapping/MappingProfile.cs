using System;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Models.Authorization;
using MedPal.API.Enums;

namespace MedPal.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Patient, PatientReadDTO>()
                .ForMember(dest => dest.EmergencyContact, opt => opt.Ignore())
                .ForMember(dest => dest.PatientDetailsId, opt => opt.MapFrom(src => src.PatientDetails != null ? src.PatientDetails.Id : (int?)null))
                .ForMember(dest => dest.Clinic, opt => opt.Ignore())
                .ForMember(dest => dest.Clinics, opt => opt.Ignore());
            CreateMap<Patient, PatientWriteDTO>()
                .ForMember(dest => dest.EmergencyContact, opt => opt.Ignore());
            CreateMap<PatientWriteDTO, Patient>(MemberList.Source)
                .ForSourceMember(src => src.EmergencyContact, opt => opt.DoNotValidate());
            CreateMap<Patient, Patient>().ReverseMap();
            
            CreateMap<User, UserReadDTO>()
                .ForMember(dest => dest.Token, opt => opt.Ignore());
            CreateMap<UserReadDTO, User>(MemberList.Source)
                .ForSourceMember(src => src.Token, opt => opt.DoNotValidate());

            CreateMap<Clinic, ClinicReadDTO>().ReverseMap();
            CreateMap<ClinicWriteDTO, Clinic>(MemberList.Source);
            CreateMap<PatientDetails, PatientDetailsReadDTO>().ReverseMap();
            CreateMap<PatientDetailsWriteDTO, PatientDetails>(MemberList.Source);
            CreateMap<MedicalHistory, MedicalHistoryReadDTO>()
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.HealthcareProfessional != null ? src.HealthcareProfessional.Name : null));
            CreateMap<MedicalHistoryReadDTO, MedicalHistory>();
            CreateMap<MedicalHistoryWriteDTO, MedicalHistory>(MemberList.Source);
            CreateMap<Allergy, AllergyReadDTO>().ReverseMap();
            CreateMap<AllergyWriteDTO, Allergy>(MemberList.Source);
            CreateMap<Appointment, AppointmentReadDTO>().ReverseMap();
            CreateMap<AppointmentWriteDTO, Appointment>(MemberList.Source)
                .ForSourceMember(src => src.Status, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.PatientName, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.PatientPhone, opt => opt.DoNotValidate())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Status) ? AppointmentStatus.Scheduled : Enum.Parse<AppointmentStatus>(src.Status, true)));
            CreateMap<Clinic, ClinicBasicDTO>().ReverseMap();
            CreateMap<UserUpdateDTO, User>(MemberList.Source);
            
            // Prescription mappings (Phase 4)
            CreateMap<Prescription, PrescriptionReadDTO>()
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.Name))
                .ForMember(dest => dest.DoctorSpecialty, opt => opt.MapFrom(src => src.Doctor.Specialty))
                .ForMember(dest => dest.DoctorLicense, opt => opt.MapFrom(src => src.Doctor.ProfessionalLicenseNumber))
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.Name));
            CreateMap<PrescriptionWriteDTO, Prescription>(MemberList.Source);
            CreateMap<PrescriptionItem, PrescriptionItemDTO>().ReverseMap();

            // Custom mappings for User and UserWriteDTO
            CreateMap<UserWriteDTO, User>(MemberList.Source)
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password))
                .ForSourceMember(src => src.RoleId, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.ConfirmPassword, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.AcceptPrivacyTerms, opt => opt.DoNotValidate());

            // Custom mappings for User and UserRegisterDTO
            CreateMap<UserRegisterDTO, User>(MemberList.Source)
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password))
                .ForSourceMember(src => src.ConfirmPassword, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.AcceptPrivacyTerms, opt => opt.DoNotValidate());

            // Permission mappings
            CreateMap<Permission, PermissionDTO>();

            // Role mappings
            CreateMap<Role, RoleReadDTO>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.RolePermissions.Select(rp => rp.Permission).ToList()));
            
            // UserRole mappings
            CreateMap<UserRole, UserRoleDTO>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name))
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            // Phase 4: EmergencyContact mappings
            CreateMap<EmergencyContact, EmergencyContactReadDTO>().ReverseMap();
            CreateMap<EmergencyContactWriteDTO, EmergencyContact>(MemberList.Source);

            // Phase 4: Payment mappings
            CreateMap<Payment, PaymentReadDTO>()
                .ForMember(dest => dest.TransactionReference, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.Ignore());
            CreateMap<PaymentWriteDTO, Payment>(MemberList.Source)
                .ForSourceMember(src => src.TransactionReference, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.Notes, opt => opt.DoNotValidate());

            // Phase 4: Invoice mappings
            CreateMap<Invoice, InvoiceReadDTO>()
                .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.TotalAmount - src.PaidAmount))
                .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments.Where(p => !p.IsDeleted).ToList()));
            CreateMap<InvoiceWriteDTO, Invoice>(MemberList.Source);

            // Phase 4: NotificationMessage mappings
            CreateMap<NotificationMessage, NotificationMessageReadDTO>().ReverseMap();
            CreateMap<NotificationMessageWriteDTO, NotificationMessage>(MemberList.Source);

            // VitalSign mappings (Signos Vitales)
            CreateMap<VitalSign, VitalSignReadDTO>().ReverseMap();
            CreateMap<VitalSignWriteDTO, VitalSign>(MemberList.Source);

            // CIE-10 mappings
            CreateMap<Cie10Code, Cie10CodeDTO>().ReverseMap();

            // Consent mappings
            CreateMap<PatientConsent, ConsentReadDTO>().ReverseMap();

            // Waitlist mappings
            CreateMap<WaitlistRegisterDTO, WaitlistEntry>();

            // Nutrition Module mappings
            CreateMap<FoodItem, FoodItemReadDTO>().ReverseMap();
            CreateMap<FoodItemWriteDTO, FoodItem>(MemberList.Source);

            CreateMap<BodyComposition, BodyCompositionReadDTO>().ReverseMap();
            CreateMap<BodyCompositionWriteDTO, BodyComposition>(MemberList.Source);

            CreateMap<AnthropometryRecord, AnthropometryReadDTO>().ReverseMap();
            CreateMap<AnthropometryWriteDTO, AnthropometryRecord>(MemberList.Source);

            CreateMap<DietPlan, DietPlanReadDTO>()
                .ForMember(dest => dest.Meals, opt => opt.MapFrom(src => src.Meals.OrderBy(m => m.MealOrder)));
            CreateMap<DietPlanWriteDTO, DietPlan>(MemberList.Source)
                .ForMember(dest => dest.Meals, opt => opt.Ignore());
            CreateMap<DietPlanMeal, DietPlanMealDTO>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
            CreateMap<DietPlanMealWriteDTO, DietPlanMeal>(MemberList.Source);
            CreateMap<DietPlanMealItem, DietPlanMealItemDTO>()
                .ForMember(dest => dest.FoodItemName, opt => opt.MapFrom(src => src.FoodItem != null ? src.FoodItem.Name : src.CustomFoodName));
            CreateMap<DietPlanMealItemWriteDTO, DietPlanMealItem>(MemberList.Source);

            CreateMap<NutritionProgress, NutritionProgressReadDTO>().ReverseMap();
            CreateMap<NutritionProgressWriteDTO, NutritionProgress>(MemberList.Source);

            CreateMap<Supplement, SupplementReadDTO>().ReverseMap();
            CreateMap<SupplementWriteDTO, Supplement>(MemberList.Source);

            // Subscription mappings
            CreateMap<SubscriptionPlan, SubscriptionPlanReadDTO>();
            CreateMap<Subscription, SubscriptionReadDTO>()
                .ForMember(dest => dest.Plan, opt => opt.MapFrom(src => src.SubscriptionPlan))
                .ForMember(dest => dest.CurrentTeamMembers, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentClinics, opt => opt.Ignore());
        }
    }
}