using System;

namespace MedPal.API.DTOs
{
    public class AnthropometryReadDTO
    {
        public int Id { get; set; }
        public int PatientDetailsId { get; set; }
        public DateTime RecordedAt { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? Bmi { get; set; }
        public decimal? Waist { get; set; }
        public decimal? Hip { get; set; }
        public decimal? WaistHipRatio { get; set; }
        public decimal? WaistHeightRatio { get; set; }
        public decimal? MidArmCircumference { get; set; }
        public decimal? BodyFatPercentageEstimated { get; set; }
        public decimal? Neck { get; set; }
        public decimal? ShoulderBreadth { get; set; }
        public decimal? Chest { get; set; }
        public decimal? Arm { get; set; }
        public decimal? Forearm { get; set; }
        public decimal? Wrist { get; set; }
        public decimal? Thigh { get; set; }
        public decimal? Calf { get; set; }
        public decimal? TricepsSkinfold { get; set; }
        public decimal? BicepsSkinfold { get; set; }
        public decimal? SubscapularSkinfold { get; set; }
        public decimal? SuprailiacSkinfold { get; set; }
        public decimal? CalfSkinfold { get; set; }
        public decimal? ThighSkinfold { get; set; }
        public decimal? AbdominalSkinfold { get; set; }
        public decimal? PectoralSkinfold { get; set; }
        public decimal? AxillarySkinfold { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
