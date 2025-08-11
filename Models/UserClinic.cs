namespace MedPal.API.Models
{
    public class UserClinic
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int ClinicId { get; set; }
        public virtual Clinic Clinic { get; set; }
    }
}