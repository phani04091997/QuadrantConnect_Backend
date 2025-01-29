using H1bConnect_Backend.Models.Entities;
using H1bConnect_Backend.Models.Entities.DTO;

namespace H1bConnect_Backend.Repository.IServices
{
    public interface IResourceService
    {
        
        // CRUD Operations for Resources
        Task<List<ResourceDetails>> GetAllResourcesAsync();  // Retrieve all resources
        Task<ResourceDetails> GetResourceByIdAsync(int resourceId);  // Get a specific resource by ID
        Task<int?> GetLatestResourceIdAsync();
        Task CreateResourceAsync(ResourceDetails resource);  // Create a new resource
        Task UpdateResourceDetailsAsync(UpdateResourceDTO resourceDto);  // Update resource details
        Task<bool> UpdateResourceNotesAsync(int resourceId, ResourceNotes newNotes);
        Task<bool> DeleteResourceByIdAsync(int resourceId);  // Delete a resource by ID

        // Resource Search and Retrieval
        Task<List<ResourceDetails>> GetResourceByNameAndSkillAsync(string firstName, string lastName, string skill, DateTime? joiningDate, DateTime? startDate, DateTime? EndDate, string userType, int yearOfFiling);  // Find a resource by name and skill
        Task<List<ResourceDetails>> GetResourceByUserTypeAsync(string userType);

        // Status Management
        Task<List<StatusDetails>> GetStatusOfResourceAsync(int resourceId);  // Get all statuses of a resource(pipeline)
        Task<StatusDetails> GetResourceCurrentStatusAsync(int resourceId);  // Get the current status of a resource
        Task<bool> UpdateResourceCurrentStatusAsync(int resourceId, StatusDetails newStatus);  // Update the current status of a resource
        Task<List<ResourceDetails>> GetResourcesByMostRecentStatusIdAsync(int statusId, string userType, int? yearOfFiling = null);//Get all resources by most recent status ids
        Task UploadFilesToMongoAsync(int resourceId, List<IFormFile> files, string fileType);
        Task<Stream> DownloadFileFromMongoAsync(string fileId);
    }
}
