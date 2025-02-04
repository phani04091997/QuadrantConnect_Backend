﻿using H1bConnect_Backend.Data;
using H1bConnect_Backend.Models.Entities;
using H1bConnect_Backend.Repository.IServices;
using MongoDB.Driver;

namespace H1bConnect_Backend.Repository.Services
{
    public class CounterService : ICounterService
    {
        private readonly IMongoCollection<Counter> _counterCollection;
        private readonly IMongoCollection<ResourceDetails> _resourceCollection;

        public CounterService(QuadrantDbContext context)
        {
            _counterCollection = context.Counters;
            _resourceCollection = context.ResourceDetails;
        }


        public async Task<int> GetNextSequenceValue(string collectionName)
        {
            var filter = Builders<Counter>.Filter.Eq(c => c.CollectionName, collectionName);
            var update = Builders<Counter>.Update.Inc(c => c.SequenceValue, 1);
            var options = new FindOneAndUpdateOptions<Counter>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true 
            };

            var counter = await _counterCollection.FindOneAndUpdateAsync(filter, update, options);
            return counter.SequenceValue;
        }

 
        public async Task DecrementCounter(string collectionName)
        {
            var isCollectionEmpty = !await _resourceCollection.Find(_ => true).AnyAsync();
            if (isCollectionEmpty)
            {
                var filter = Builders<Counter>.Filter.Eq(c => c.CollectionName, collectionName);
                var update = Builders<Counter>.Update.Set(c => c.SequenceValue, 1);
                await _counterCollection.UpdateOneAsync(filter, update);
            }
            else
            {
                var filter = Builders<Counter>.Filter.Eq(c => c.CollectionName, collectionName);
                var update = Builders<Counter>.Update.Inc(c => c.SequenceValue, -1);
                await _counterCollection.UpdateOneAsync(filter, update);
            }
        }
    }
}
