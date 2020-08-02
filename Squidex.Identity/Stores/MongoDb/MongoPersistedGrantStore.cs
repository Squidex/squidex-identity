// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Squidex.Identity.Stores.MongoDb
{
    public sealed class MongoPersistedGrantStore : IPersistedGrantStore
    {
        private static readonly ReplaceOptions UpsertReplace = new ReplaceOptions { IsUpsert = true };
        private readonly IMongoCollection<PersistedGrant> collection;

        static MongoPersistedGrantStore()
        {
            BsonClassMap.RegisterClassMap<PersistedGrant>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.Key));
            });
        }

        public MongoPersistedGrantStore(IMongoDatabase database)
        {
            collection = database.GetCollection<PersistedGrant>("Identity_PersistedGrants");

            collection.Indexes.CreateOne(
                new CreateIndexModel<PersistedGrant>(
                    Builders<PersistedGrant>.IndexKeys
                        .Ascending(x => x.SubjectId)
                        .Ascending(x => x.ClientId)
                        .Ascending(x => x.Type)));
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            return await collection.Find(x => x.SubjectId == subjectId).ToListAsync();
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            return await collection.Find(CreateFilter(filter)).ToListAsync();
        }

        public Task<PersistedGrant> GetAsync(string key)
        {
            return collection.Find(x => x.Key == key).FirstOrDefaultAsync();
        }

        public Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            return collection.DeleteManyAsync(CreateFilter(filter));
        }

        public Task RemoveAsync(string key)
        {
            return collection.DeleteManyAsync(x => x.Key == key);
        }

        public Task StoreAsync(PersistedGrant grant)
        {
            return collection.ReplaceOneAsync(x => x.Key == grant.Key, grant, UpsertReplace);
        }

        private static FilterDefinition<PersistedGrant> CreateFilter(PersistedGrantFilter filter)
        {
            var fb = Builders<PersistedGrant>.Filter;

            var filters = new List<FilterDefinition<PersistedGrant>>();

            if (!string.IsNullOrWhiteSpace(filter.ClientId))
            {
                filters.Add(fb.Eq(x => x.ClientId, filter.ClientId));
            }

            if (!string.IsNullOrWhiteSpace(filter.SessionId))
            {
                filters.Add(fb.Eq(x => x.SessionId, filter.SessionId));
            }

            if (!string.IsNullOrWhiteSpace(filter.SubjectId))
            {
                filters.Add(fb.Eq(x => x.SubjectId, filter.SubjectId));
            }

            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                filters.Add(fb.Eq(x => x.Type, filter.Type));
            }

            if (filters.Count > 0)
            {
                return fb.And(filters);
            }

            return new BsonDocument();
        }
    }
}
