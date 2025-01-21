using H1bConnect_Backend.Models.Entities;
using MongoDB.Driver;

namespace H1bConnect_Backend.Data
{
    public class QuadrantDbContext
    {
        public readonly IMongoDatabase Database;

        public QuadrantDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            Database = client.GetDatabase("H1BConnectDb");
        }

        public IMongoCollection<ResourceDetails> ResourceDetails => Database.GetCollection<ResourceDetails>("ResourceDetails");
        public IMongoCollection<Counter> Counters => Database.GetCollection<Counter>("Counters");
        public IMongoCollection<User> Users => Database.GetCollection<User>("Users");
    }
}
