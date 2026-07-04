using System.ComponentModel.DataAnnotations;

namespace HospitalManagementCosmosDB.Application.DTO
{
    public class CreatePatientDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 120)]
        public int Age { get; set; }

        [Required]
        [StringLength(200)]
        public string Disease { get; set; } = string.Empty;
    }

    // DTO used for returning patient data
    public class PatientDTO
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }

        public string Disease { get; set; } = string.Empty;
    }

    // DTO used for updating a patient
    public class UpdatePatientDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 120)]
        public int Age { get; set; }

        [Required]
        [StringLength(200)]
        public string Disease { get; set; } = string.Empty;
    }
}
