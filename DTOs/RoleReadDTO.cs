// RoleReadDTO.cs
using System;
using System.Collections.Generic;

namespace MedPal.API.DTOs
{
    public class RoleReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// List of permissions assigned to this role
        /// </summary>
        public ICollection<PermissionDTO> Permissions { get; set; } = new List<PermissionDTO>();
    }
}