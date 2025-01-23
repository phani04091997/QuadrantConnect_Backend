namespace H1bConnect_Backend.Models.Entities.DTO
{
    public class CreateResourceDTO
    {
        public string Id { get; set; }
        public int ResourceID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string EmailID { get; set; }
        public float ExperienceYears { get; set; }
        public List<string> TechnicalSkills { get; set; } = new();
        public string KeySummary { get; set; }
        public string CurrentIndiaAddress { get; set; }
        public string CurrentUSAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryOfOrigin { get; set; }
        public string UserType { get; set; }
        public string WorkStatus { get; set; }
        public int YearOfFiling { get; set; }
        public List<EducationDetails> EducationDetails { get; set; } = new();
        public List<JobDetails> JobDetails { get; set; } = new();
        public List<StatusDetails> StatusDetails { get; set; } = new();
    }
}
