using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;

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
            CreateMap<Appointment, AppointmentReadDTO>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToString("yyyy-MM-dd")))
            .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.Time.ToString("HH:mm")))
            .ReverseMap();
            CreateMap<AppointmentWriteDTO, Appointment>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => DateOnly.Parse(src.Date)))
            .ForMember(dest => dest.Time, opt => opt.MapFrom(src => TimeOnly.Parse(src.Time)));
            CreateMap<Appointment, AppointmentWriteDTO>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToString("yyyy-MM-dd")))
            .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.Time.ToString("HH:mm")));
        }
    }
}