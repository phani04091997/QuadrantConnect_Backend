namespace H1bConnect_Backend.Models.Entities.DTO
{
    public class FileUploadDTO
    {
        // File uploads
        public List<IFormFile> ResumeUploads { get; set; } = new();
        public List<IFormFile> EducationDocuments { get; set; } = new();
        public List<IFormFile> OfferLetters { get; set; } = new();
        public List<IFormFile> RelievingLetters { get; set; } = new();
    }
}
