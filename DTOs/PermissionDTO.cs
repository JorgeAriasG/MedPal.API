namespace MedPal.API.DTOs
{
    /// <summary>
    /// DTO for reading permission information
    /// </summary>
    public class PermissionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
