namespace MedPal.API.DTOs
{
    public class MedicalHistoryWriteDTO
    {
        public int PatientDetailsId { get; set; }
        public string ConditionName { get; set; }
        public string Treatment { get; set; }
        public string Medications { get; set; }
        public string DoctorNotes { get; set; }
        public DateTime DiagnosisDate { get; set; }
    }
}