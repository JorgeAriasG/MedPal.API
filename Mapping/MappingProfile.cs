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
            CreateMap<Appointment, AppointmentReadDTO>().ReverseMap();
            CreateMap<AppointmentWriteDTO, Appointment>().ReverseMap();
        }
    }
}