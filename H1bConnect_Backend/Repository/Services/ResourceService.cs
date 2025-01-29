using H1bConnect_Backend.Data;
using H1bConnect_Backend.Models.Entities;
using H1bConnect_Backend.Models.Entities.DTO;
using H1bConnect_Backend.Repository.IServices;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.ComponentModel.Design;
using System.Linq.Expressions;
using IResourceService = H1bConnect_Backend.Repository.IServices.IResourceService;

namespace H1bConnect_Backend.Repository.Services
{
    public class ResourceService: IResourceService
    {
        private readonly IMongoCollection<ResourceDetails> _resourceCollection;
        private readonly ICounterService _counterService;
        private readonly GridFSBucket _gridFsBucket;

        public ResourceService(QuadrantDbContext context, CounterService counterService)
        {
            _resourceCollection = context.ResourceDetails;
            _counterService = counterService;
            _gridFsBucket = new GridFSBucket(context.Database);
        }

        // CRUD Operations
        public async Task<List<ResourceDetails>> GetAllResourcesAsync()
        {
            return await _resourceCollection.Find(_ => true).ToListAsync();
        }

        public async Task<ResourceDetails> GetResourceByIdAsync(int resourceId)
        {
            return await _resourceCollection.Find(r => r.ResourceID == resourceId).FirstOrDefaultAsync();
        }

        public async Task<int?> GetLatestResourceIdAsync()
        {
            var latestResource = await _resourceCollection
                .Find(FilterDefinition<ResourceDetails>.Empty)
                .SortByDescending(r => r.ResourceID)
                .Limit(1)
                .FirstOrDefaultAsync();

            return latestResource?.ResourceID;
        }


        public async Task CreateResourceAsync(ResourceDetails resource)
        {
            resource.ResourceID = await _counterService.GetNextSequenceValue("ResourceDetails");
            resource.Id = null;
            await _resourceCollection.InsertOneAsync(resource);
        }

        public async Task UpdateResourceDetailsAsync(UpdateResourceDTO resourceDto)
        {
            var filter = Builders<ResourceDetails>.Filter.Eq(r => r.ResourceID, resourceDto.ResourceID);

            var update = Builders<ResourceDetails>.Update
                .Set(r => r.FirstName, resourceDto.FirstName)
                .Set(r => r.LastName, resourceDto.LastName)
                .Set(r => r.MiddleName, resourceDto.MiddleName)
                .Set(r => r.EmailID, resourceDto.EmailID)
                .Set(r => r.ExperienceYears, resourceDto.ExperienceYears)
                .Set(r => r.TechnicalSkills, resourceDto.TechnicalSkills)
                .Set(r => r.KeySummary, resourceDto.KeySummary)
                .Set(r => r.CurrentIndiaAddress, resourceDto.CurrentIndiaAddress)
                .Set(r => r.CurrentUSAddress, resourceDto.CurrentUSAddress)
                .Set(r => r.PhoneNumber, resourceDto.PhoneNumber)
                .Set(r => r.CountryOfOrigin, resourceDto.CountryOfOrigin)
                .Set(r => r.UserType, resourceDto.UserType)
                .Set(r => r.WorkStatus, resourceDto.WorkStatus)
                .Set(r => r.YearOfFiling, resourceDto.YearOfFiling)
                .Set(r => r.StartDate, resourceDto.StartDate)
                .Set(r => r.EndDate, resourceDto.EndDate)
                .Set(r => r.JoiningDate, resourceDto.JoiningDate)
                .Set(r => r.ExitDate, resourceDto.ExitDate)
                .Set(r => r.EducationDetails, resourceDto.EducationDetails.Select(e => new EducationDetails
                {
                    EducationID = e.EducationID,
                    GraduationYear = e.GraduationYear,
                    InstitutionName = e.InstitutionName,
                    GPA = e.GPA,
                    Percentage = e.Percentage,
                    Grade = e.Grade,
                    Marks = e.Marks,
                    EducationDocuments = e.EducationDocuments
                }).ToList())
                .Set(r => r.JobDetails, resourceDto.JobDetails.Select(j => new JobDetails
                {
                    Company = j.Company,
                    StartDate = j.StartDate,
                    EndDate = j.EndDate,
                    RolesAndResponsibility = j.RolesAndResponsibility,
                    LastDesignation = j.LastDesignation,
                    OfferLetters = j.OfferLetters,
                    RelievingLetters = j.RelievingLetters
                }).ToList())
                .Set(r => r.ResumeUploads, resourceDto.ResumeUploads)
                .Set(r => r.DepartureDate, resourceDto.DepartureDate)
                .Set(r => r.ArrivalDate, resourceDto.ArrivalDate)
                .Set(r => r.DepartureCity, resourceDto.DepartureCity)
                .Set(r => r.ArrivalCity, resourceDto.ArrivalCity)
                .Set(r => r.ReferredBy, resourceDto.ReferredBy);

            var result = await _resourceCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                throw new KeyNotFoundException($"Resource with ID {resourceDto.ResourceID} not found.");
            }
        }


        public async Task<bool> DeleteResourceByIdAsync(int resourceId)
        {
            var filter = Builders<ResourceDetails>.Filter.Eq(r => r.ResourceID, resourceId);

            var result = await _resourceCollection.DeleteOneAsync(filter);

            if (result.DeletedCount > 0)
            {
                await _counterService.DecrementCounter("ResourceDetails");
                return true;
            }

            return false;
        }

        public async Task<List<ResourceDetails>> GetResourceByNameAndSkillAsync(string firstName, string lastName, string skill, DateTime? joiningDate, DateTime? startDate, DateTime? endDate, string userType, int yearOfFiling)
        {
            var filters = new List<FilterDefinition<ResourceDetails>>();
            var builder = Builders<ResourceDetails>.Filter;

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                filters.Add(builder.Regex(r => r.FirstName, new BsonRegularExpression($"^{firstName}", "i")));
            }

            if (!string.IsNullOrWhiteSpace(lastName))
            {
                filters.Add(builder.Regex(r => r.LastName, new BsonRegularExpression($"^{lastName}", "i")));
            }

            if (!string.IsNullOrWhiteSpace(skill))
            {
                filters.Add(builder.ElemMatch(r => r.TechnicalSkills, skillValue => skillValue.ToLower().StartsWith(skill.ToLower())));
            }

            if (joiningDate.HasValue)
            {
                var startOfMonth = new DateTime(joiningDate.Value.Year, joiningDate.Value.Month, 1);
                var startOfNextMonth = startOfMonth.AddMonths(1);

                filters.Add(builder.Gte(r => r.JoiningDate, startOfMonth));
                filters.Add(builder.Lt(r => r.JoiningDate, startOfNextMonth));
            }

            if (startDate.HasValue)
            {
                var startOfMonth = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
                var startOfNextMonth = startOfMonth.AddMonths(1);

                filters.Add(builder.Gte(r => r.StartDate, startOfMonth));
                filters.Add(builder.Lt(r => r.StartDate, startOfNextMonth));
            }

            if (endDate.HasValue)
            {
                var startOfMonth = new DateTime(endDate.Value.Year, endDate.Value.Month, 1);
                var startOfNextMonth = startOfMonth.AddMonths(1);

                filters.Add(builder.Gte(r => r.EndDate, startOfMonth));
                filters.Add(builder.Lt(r => r.EndDate, startOfNextMonth));
            }

            if (!string.IsNullOrWhiteSpace(userType))
            {
                filters.Add(builder.Eq(r => r.UserType, userType)); 
            }

            if (yearOfFiling > 0)
            {
                filters.Add(builder.Eq(r => r.YearOfFiling, yearOfFiling)); 
            }

            var combinedFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            return await _resourceCollection.Find(combinedFilter).ToListAsync();
        }

        public async Task<List<ResourceDetails>> GetResourceByUserTypeAsync(string userType)
        {
            var builder = Builders<ResourceDetails>.Filter;
            var filter = builder.Eq(r => r.UserType, userType);

            return await _resourceCollection.Find(filter).ToListAsync();
        }


        public async Task<List<StatusDetails>> GetStatusOfResourceAsync(int resourceId)
        {
            var resource = await GetResourceByIdAsync(resourceId);
            return resource?.StatusDetails ?? new List<StatusDetails>();
        }

        public async Task<StatusDetails> GetResourceCurrentStatusAsync(int resourceId)
        {
            var resource = await GetResourceByIdAsync(resourceId);
            return resource?.StatusDetails?.OrderByDescending(s => s.DateTimeStamp).FirstOrDefault();
        }

        public async Task<bool> UpdateResourceCurrentStatusAsync(int resourceId, StatusDetails newStatus)
        {
            var filter = Builders<ResourceDetails>.Filter.Eq(r => r.ResourceID, resourceId);
            var update = Builders<ResourceDetails>.Update.Push(r => r.StatusDetails, newStatus);

            var result = await _resourceCollection.UpdateOneAsync(filter, update);
            return result.MatchedCount > 0;
        }

        public async Task<bool> UpdateResourceNotesAsync(int resourceId, ResourceNotes newNote)
        {
            var filter = Builders<ResourceDetails>.Filter.Eq(r => r.ResourceID, resourceId);

            var initializeNotes = Builders<ResourceDetails>.Update.Set(r => r.ResourceNotes, new List<ResourceNotes>());
            var initializeResult = await _resourceCollection.UpdateOneAsync(
                filter & Builders<ResourceDetails>.Filter.Eq(r => r.ResourceNotes, null),
                initializeNotes
            );

            var pushNote = Builders<ResourceDetails>.Update.Push(r => r.ResourceNotes, newNote);
            var pushResult = await _resourceCollection.UpdateOneAsync(filter, pushNote);

            return initializeResult.MatchedCount > 0 || pushResult.MatchedCount > 0;
        }


        public async Task<List<ResourceDetails>> GetResourcesByMostRecentStatusIdAsync(int statusId, string userType, int? yearOfFiling = null)
        {
            var builder = Builders<ResourceDetails>.Filter;
            var filters = new List<FilterDefinition<ResourceDetails>>();

            if (!string.IsNullOrWhiteSpace(userType))
                filters.Add(builder.Eq(r => r.UserType, userType));

            if (yearOfFiling.HasValue)
                filters.Add(builder.Eq(r => r.YearOfFiling, yearOfFiling.Value));

            var allResources = await _resourceCollection.Find(builder.And(filters)).ToListAsync();

            var filteredResources = allResources
                .Where(resource =>
                    resource.StatusDetails != null &&
                    resource.StatusDetails.Count > 0 &&
                    resource.StatusDetails.OrderByDescending(s => s.DateTimeStamp).FirstOrDefault()?.StatusId == statusId
                )
                .ToList();

            return filteredResources;
        }



        public async Task UploadFilesToMongoAsync(int resourceId, List<IFormFile> files, string fileType)
        {
            var resource = await GetResourceByIdAsync(resourceId);
            if (resource == null)
            {
                throw new KeyNotFoundException($"Resource with ID {resourceId} not found.");
            }

            switch (fileType)
            {
                case "resume":
                    if (resource.ResumeUploads == null)
                    {
                        await InitializeField(resourceId, r => r.ResumeUploads, new List<string>());
                    }
                    break;
                case "educationDocument":
                    foreach (var education in resource.EducationDetails ?? new List<EducationDetails>())
                    {
                        if (education.EducationDocuments == null)
                        {
                            await InitializeField(resourceId, "EducationDetails.$[].EducationDocuments", new List<string>());
                        }
                    }
                    break;
                case "offerLetter":
                    foreach (var job in resource.JobDetails ?? new List<JobDetails>())
                    {
                        if (job.OfferLetters == null)
                        {
                            await InitializeField(resourceId, "JobDetails.$[].OfferLetters", new List<string>());
                        }
                    }
                    break;
                case "relievingLetter":
                    foreach (var job in resource.JobDetails ?? new List<JobDetails>())
                    {
                        if (job.RelievingLetters == null)
                        {
                            await InitializeField(resourceId, "JobDetails.$[].RelievingLetters", new List<string>());
                        }
                    }
                    break;
                default:
                    throw new ArgumentException("Invalid file type specified.");
            }

            // Remove placeholder "string" values
            UpdateDefinition<ResourceDetails> cleanupUpdate = fileType switch
            {
                "resume" => Builders<ResourceDetails>.Update.Pull(r => r.ResumeUploads, "string"),
                "educationDocument" => Builders<ResourceDetails>.Update.Pull("EducationDetails.$[].EducationDocuments", "string"),
                "offerLetter" => Builders<ResourceDetails>.Update.Pull("JobDetails.$[].OfferLetters", "string"),
                "relievingLetter" => Builders<ResourceDetails>.Update.Pull("JobDetails.$[].RelievingLetters", "string"),
                _ => null
            };

            if (cleanupUpdate != null)
            {
                await _resourceCollection.UpdateOneAsync(r => r.ResourceID == resourceId, cleanupUpdate);
            }

            foreach (var file in files)
            {
                using var stream = file.OpenReadStream();
                var fileId = await _gridFsBucket.UploadFromStreamAsync(file.FileName, stream);

                var update = fileType switch
                {
                    "resume" => Builders<ResourceDetails>.Update.AddToSet(r => r.ResumeUploads, fileId.ToString()),
                    "educationDocument" => Builders<ResourceDetails>.Update.AddToSet("EducationDetails.$[].EducationDocuments", fileId.ToString()),
                    "offerLetter" => Builders<ResourceDetails>.Update.AddToSet("JobDetails.$[].OfferLetters", fileId.ToString()),
                    "relievingLetter" => Builders<ResourceDetails>.Update.AddToSet("JobDetails.$[].RelievingLetters", fileId.ToString()),
                    _ => throw new ArgumentException("Invalid file type specified.")
                };

                await _resourceCollection.UpdateOneAsync(r => r.ResourceID == resourceId, update);
            }
        }

        private async Task InitializeField<TField>(int resourceId, Expression<Func<ResourceDetails, TField>> field, TField defaultValue)
        {
            var initializeUpdate = Builders<ResourceDetails>.Update.Set(field, defaultValue);
            await _resourceCollection.UpdateOneAsync(r => r.ResourceID == resourceId, initializeUpdate);
        }

        private async Task InitializeField(int resourceId, string fieldPath, object defaultValue)
        {
            var initializeUpdate = Builders<ResourceDetails>.Update.Set(fieldPath, defaultValue);
            await _resourceCollection.UpdateOneAsync(r => r.ResourceID == resourceId, initializeUpdate);
        }

        public async Task<Stream> DownloadFileFromMongoAsync(string fileId)
        {
            var objectId = new MongoDB.Bson.ObjectId(fileId);
            var fileStream = new MemoryStream();
            await _gridFsBucket.DownloadToStreamAsync(objectId, fileStream);

            fileStream.Position = 0; 
            return fileStream;
        }

    }
}
