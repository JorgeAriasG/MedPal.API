using MedPal.API.Models;

namespace MedPal.API.DTOs
{
    public class PatientReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime Dob { get; set; }
        public string Gender { get; set; }
        public string EmergencyContact { get; set; }
        public ClinicReadDTO Clinic { get; set; }
        // Add other necessary properties
    }
}