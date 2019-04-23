using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using SharpGeoAPI.Models;

namespace SharpGeoAPI
{
    class Storage : IStorage
    {
        private LiteDatabase db;
        protected LiteCollection<Seed> seeds;
        protected LiteCollection<Session> sessions;

        public Storage()
        {
            db = new LiteDatabase(DBNames.DBName);
            seeds = db.GetCollection<Seed>(DBNames.SeedCollection);
            sessions = db.GetCollection<Session>(DBNames.SessionCollection);
        }

        public async Task<Session> GetSession(string sessionId)
        {
            return sessions.FindById(sessionId);
        }

        public async Task PutSession(Session session)
        {
            sessions.Insert(session.SessionId, session);
        }

        public async Task PutSeed(string sessionId, Seed seed)
        {
            seeds.Insert(seed.Id, seed);
        }

        public async Task<IEnumerable<Seed>> GetSeeds(string sessionId)
        {
            return seeds.Find(seed => seed.Id.StartsWith(sessionId));
        }

        public async Task<Seed> GetSeed(string sessionId, string seedName)
        {
            return seeds.FindOne(seed => seed.SessionId.Equals(sessionId) && seed.SeedName.Equals(seedName));
        }

        public void Dispose()
        {
            db.Dispose();
        }

        private class DBNames
        {
            public static string SeedCollection => "SeedCollection";
            public static string SessionCollection => "SessionsCollection";
            public static string DBName => "Seeds.db";
        }
    }
}