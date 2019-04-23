using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using index.db.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace index.db.Services
{
    public interface IServiceBase<T> where T : IEntity
    {
        List<T> Get();
        T Get(string id);
        T Get(Expression<Func<T, bool>> selector);
        T Create(T entity);
        void Update(string id, T entityIn);
        void Remove(T entityIn);
        void Remove(string id);
        void RemoveAll();
    }

    public class ServiceBase<T> : IServiceBase<T> where T : IEntity
    {
        private readonly IMongoCollection<T> entities;

        protected ServiceBase(IConfiguration config, string dbCollection)
        {
            var client = new MongoClient(config.GetConnectionString("mongo"));
            var database = client.GetDatabase("indexDb");
            entities = database.GetCollection<T>(dbCollection);
        }

        public List<T> Get() => entities.Find(_ => true).ToList();

        public T Get(string id) => entities.Find(e => e.Id == id).FirstOrDefault();
        public T Get(Expression<Func<T, bool>> selector) => entities.Find(selector).FirstOrDefault();

        public T Create(T entity)
        {
            entities.InsertOne(entity);

            return entity;
        }

        public void Update(string id, T entityIn) => entities.ReplaceOne(entity => entity.Id == id, entityIn);

        public void Remove(T entityIn) => entities.DeleteOne(entity => entity.Id == entityIn.Id);

        public void Remove(string id) => entities.DeleteOne(entity => entity.Id == id);
        public void RemoveAll() => entities.DeleteMany(_ => true);
    }
}