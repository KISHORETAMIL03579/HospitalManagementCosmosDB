namespace HospitalManagementCosmosDB.Application.DTO
{
    public class CreatePatientDTO
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Disease { get; set; } = string.Empty;
    }

    // DTO used for returning patient data
    public class PatientDTO
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Disease { get; set; } = string.Empty;
    }

    // DTO used for updating a patient
    public class UpdatePatientDTO
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Disease { get; set; } = string.Empty;
    }
}
