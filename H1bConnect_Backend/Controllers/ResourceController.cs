using H1bConnect_Backend.Models.Entities;
using H1bConnect_Backend.Models.Entities.DTO;
using H1bConnect_Backend.Repository.IServices;
using Microsoft.AspNetCore.Mvc;

namespace H1bConnect_Backend.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ResourceController: ControllerBase
    {
        private readonly IResourceService _resourceService;

        public ResourceController(IResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        // CRUD Operations
        [HttpGet]
        public async Task<ActionResult<List<ResourceDetails>>> GetAllResources()
        {
            var resources = await _resourceService.GetAllResourcesAsync();
            return Ok(resources);
        }

        [HttpGet("{resourceId}")]
        public async Task<ActionResult<ResourceDetails>> GetResourceById(int resourceId)
        {
            var resource = await _resourceService.GetResourceByIdAsync(resourceId);
            if (resource == null)
                return NotFound($"Resource with ID {resourceId} not found.");
            return Ok(resource);
        }

        [HttpGet("latest-resource-id")]
        public async Task<ActionResult<int>> GetLatestResourceId()
        {
            try
            {
                var latestResourceId = await _resourceService.GetLatestResourceIdAsync();
                if (latestResourceId == null)
                {
                    return NotFound("No resources found.");
                }
                return Ok(latestResourceId);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }


        [HttpPost("create-resource")]
        public async Task<IActionResult> CreateResource([FromBody] CreateResourceDTO resourceDetails)
        {
            try
            {
                // Map DTO to ResourceDetails
                var resource = new ResourceDetails
                {
                    Id = resourceDetails.Id,
                    ResourceID = resourceDetails.ResourceID,
                    FirstName = resourceDetails.FirstName,
                    LastName = resourceDetails.LastName,
                    MiddleName = resourceDetails.MiddleName,
                    EmailID = resourceDetails.EmailID,
                    ExperienceYears = resourceDetails.ExperienceYears,
                    TechnicalSkills = resourceDetails.TechnicalSkills,
                    KeySummary = resourceDetails.KeySummary,
                    CurrentIndiaAddress = resourceDetails.CurrentIndiaAddress,
                    CurrentUSAddress = resourceDetails.CurrentUSAddress,
                    PhoneNumber = resourceDetails.PhoneNumber,
                    CountryOfOrigin = resourceDetails.CountryOfOrigin,
                    UserType = resourceDetails.UserType,
                    WorkStatus = resourceDetails.WorkStatus,
                    YearOfFiling = resourceDetails.YearOfFiling,
                    StartDate = resourceDetails.StartDate,
                    EndDate = resourceDetails.EndDate,
                    EducationDetails = resourceDetails.EducationDetails,
                    JobDetails = resourceDetails.JobDetails,
                    StatusDetails = resourceDetails.StatusDetails,
                    ReferredBy = resourceDetails.ReferredBy
                };

                // Save the resource details
                await _resourceService.CreateResourceAsync(resource);

                return CreatedAtAction(nameof(GetResourceById), new { resourceId = resource.ResourceID }, resource);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("upload-files/{resourceId}")]
        public async Task<IActionResult> UploadFiles(
    int resourceId,
    [FromForm] List<IFormFile>? resumeUploads = null,
    [FromForm] List<IFormFile>? educationDocuments = null,
    [FromForm] List<IFormFile>? offerLetters = null,
    [FromForm] List<IFormFile>? relievingLetters = null)
        {
            try
            {
                // Handle file uploads
                if (resumeUploads != null && resumeUploads.Count > 0)
                {
                    await _resourceService.UploadFilesToMongoAsync(resourceId, resumeUploads, "resume");
                }

                if (educationDocuments != null && educationDocuments.Count > 0)
                {
                    await _resourceService.UploadFilesToMongoAsync(resourceId, educationDocuments, "educationDocument");
                }

                if (offerLetters != null && offerLetters.Count > 0)
                {
                    await _resourceService.UploadFilesToMongoAsync(resourceId, offerLetters, "offerLetter");
                }

                if (relievingLetters != null && relievingLetters.Count > 0)
                {
                    await _resourceService.UploadFilesToMongoAsync(resourceId, relievingLetters, "relievingLetter");
                }

                return Ok("Files uploaded successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpGet("{resourceId}/download/{fileType}")]
        public async Task<IActionResult> DownloadFiles(int resourceId, string fileType)
        {
            try
            {
                var resource = await _resourceService.GetResourceByIdAsync(resourceId);
                if (resource == null)
                {
                    return NotFound($"Resource with ID {resourceId} not found.");
                }

                List<string> fileIds = fileType.ToLower() switch
                {
                    "resume" => resource.ResumeUploads,
                    "educationdocument" => resource.EducationDetails.SelectMany(e => e.EducationDocuments).ToList(),
                    "offerletter" => resource.JobDetails.SelectMany(j => j.OfferLetters).ToList(),
                    "relievingletter" => resource.JobDetails.SelectMany(j => j.RelievingLetters).ToList(),
                    _ => throw new ArgumentException("Invalid file type specified.")
                };

                if (fileIds == null || fileIds.Count == 0)
                {
                    return NotFound($"No files found for file type '{fileType}' in resource ID {resourceId}.");
                }

                if (fileIds.Count == 1)
                {
                    // Single file: return it directly
                    var fileStream = await _resourceService.DownloadFileFromMongoAsync(fileIds.First());
                    return File(fileStream, "application/octet-stream", $"{fileIds.First()}.file");
                }

                // Multiple files: create a ZIP
                var zipStream = new MemoryStream();
                using (var archive = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    foreach (var fileId in fileIds)
                    {
                        var fileStream = await _resourceService.DownloadFileFromMongoAsync(fileId);
                        var zipEntry = archive.CreateEntry(fileId); // Use meaningful names if available
                        using var entryStream = zipEntry.Open();
                        fileStream.CopyTo(entryStream);
                    }
                }

                zipStream.Position = 0; // Reset stream position for reading
                return File(zipStream, "application/zip", $"{fileType}_files.zip");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpPut("{resourceId}")]
        public async Task<IActionResult> UpdateResourceDetails(int resourceId, [FromBody] UpdateResourceDTO resourceDto)
        {
            // Ensure the ResourceID from the URL matches the body
            if (resourceDto.ResourceID != resourceId)
            {
                return BadRequest("ResourceID in the URL does not match the body.");
            }

            await _resourceService.UpdateResourceDetailsAsync(resourceDto);
            return Ok("Resource updated successfully.");
        }

        [HttpDelete("{resourceId}")]
        public async Task<IActionResult> DeleteResourceById(int resourceId)
        {
            var result = await _resourceService.DeleteResourceByIdAsync(resourceId);
            if (!result)
                return NotFound("Resource not found.");
            return Ok("Resource deleted successfully.");
        }

        // Resource Search and Retrieval
        [HttpGet("search")]
        public async Task<ActionResult<List<ResourceDetails>>> GetResourceByNameAndSkill( [FromQuery] string? firstName = null, [FromQuery] string? lastName = null, [FromQuery] string? skill = null, [FromQuery] DateTime? joiningDate = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string? userType = null, [FromQuery] int yearOfFiling = 0)
        {
            var resources = await _resourceService.GetResourceByNameAndSkillAsync(firstName, lastName, skill, joiningDate, startDate, endDate, userType, yearOfFiling);
            if (resources == null || resources.Count == 0)
                return NotFound("No resources found.");

            return Ok(resources);
        }

        // Resource Search by UserType
        [HttpGet("search-by-usertype")]
        public async Task<ActionResult<List<ResourceDetails>>> GetResourceByUserType([FromQuery] string userType)
        {
            if (string.IsNullOrWhiteSpace(userType))
                return BadRequest("UserType is required.");

            var resources = await _resourceService.GetResourceByUserTypeAsync(userType);
            if (resources == null || resources.Count == 0)
                return NotFound($"No resources found for UserType: {userType}");

            return Ok(resources);
        }


        // Status Management
        [HttpGet("{resourceId}/status")]
        public async Task<ActionResult<List<StatusDetails>>> GetStatusOfResource(int resourceId)
        {
            var statuses = await _resourceService.GetStatusOfResourceAsync(resourceId);
            if (statuses == null || statuses.Count == 0)
                return NotFound("No statuses found for Resource ID.");
            return Ok(statuses);
        }

        [HttpGet("{resourceId}/current-status")]
        public async Task<ActionResult<StatusDetails>> GetResourceCurrentStatus(int resourceId)
        {
            var status = await _resourceService.GetResourceCurrentStatusAsync(resourceId);
            if (status == null)
                return NotFound("Current status not found.");
            return Ok(status);
        }

        [HttpPost("{resourceId}/update-status")]
        public async Task<IActionResult> UpdateResourceCurrentStatus(int resourceId, [FromBody] StatusDetails newStatus)
        {
            var result = await _resourceService.UpdateResourceCurrentStatusAsync(resourceId, newStatus);
            if (!result)
                return NotFound("Resource not found.");
            return Ok("Resource current status updated successfully.");
        }

        [HttpGet("status/{statusId}/{userType}")]
        public async Task<ActionResult<List<ResourceDetails>>> GetResourcesByMostRecentStatusId(int statusId, string userType, [FromQuery] int? yearOfFiling = null)
        {
            var resources = await _resourceService.GetResourcesByMostRecentStatusIdAsync(statusId, userType, yearOfFiling);
            if (resources == null || resources.Count == 0)
                return NotFound($"No resources found with the most recent status ID {statusId}.");

            return Ok(resources);
        }


        [HttpPost("{resourceId}/update-notes")]
        public async Task<IActionResult> UpdateResourceNotes(int resourceId, [FromBody] ResourceNotes newNote)
        {
            var result = await _resourceService.UpdateResourceNotesAsync(resourceId, newNote);
            if (!result)
                return NotFound("Resource not found.");
            return Ok("Resource notes updated successfully.");
        }


    }
}
