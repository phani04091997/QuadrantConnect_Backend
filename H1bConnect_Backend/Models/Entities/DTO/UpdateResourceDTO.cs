namespace H1bConnect_Backend.Models.Entities.DTO
{
    public class UpdateResourceDTO
    {
        public int ResourceID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string EmailID { get; set; }
        public float ExperienceYears { get; set; }
        public List<string> TechnicalSkills { get; set; }
        public string KeySummary { get; set; }
        public string CurrentIndiaAddress { get; set; }
        public string CurrentUSAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryOfOrigin { get; set; }
        public string UserType { get; set; }
        public string WorkStatus { get; set; }
        public int YearOfFiling { get; set; }
        public DateTime? JoiningDate { get; set; }
        public DateTime? ExitDate { get; set; }
        public List<EducationDetailsDto> EducationDetails { get; set; }
        public List<JobDetailsDto> JobDetails { get; set; }
        public List<string> ResumeUploads { get; set; }
        //public List<ResourceNotesDTO> ResourceNotes { get; set; }
        public string DepartureCity { get; set; }
        public string ArrivalCity { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
    }

    public class EducationDetailsDto
    {
        public int EducationID { get; set; }
        public int GraduationYear { get; set; }
        public string InstitutionName { get; set; }
        public float GPA { get; set; }
        public float Percentage { get; set; }
        public string Grade { get; set; }
        public double Marks { get; set; }
        public List<string> EducationDocuments { get; set; }
    }

    public class JobDetailsDto
    {
        public string Company { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string RolesAndResponsibility { get; set; }
        public string LastDesignation { get; set; }
        public List<string> OfferLetters { get; set; }
        public List<string> RelievingLetters { get; set; }
    }

    //public class ResourceNotesDTO
    //{
    //    public string Notes { get; set; }
    //    public DateTime NotesTimeStamp { get; set; }
    //}

}
