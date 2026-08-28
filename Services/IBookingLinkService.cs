namespace MedPal.API.Services
{
    public interface IBookingLinkService
    {
        string Issue(int clinicId, int doctorId);
        (int ClinicId, int DoctorId)? Validate(string token);
    }
}