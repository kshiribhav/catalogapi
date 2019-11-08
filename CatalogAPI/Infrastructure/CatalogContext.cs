using CatalogAPI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace CatalogAPI.Infrastructure
{
    public class CatalogContext
    {
        private IConfiguration _configuration;
        private IMongoDatabase _database;

        public CatalogContext(IConfiguration configuration)
        {
            _configuration = configuration;
            var connectionString = _configuration.GetValue<string>("MongoSettings:ConnectionString");
            //MongoClientSettings clientSettings = MongoClientSettings.FromConnectionString(connectionString);
            MongoClientSettings clientSettings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
            clientSettings.SslSettings = new SslSettings()
            {
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
            };
            MongoClient mongoClient = new MongoClient(clientSettings);
            if (mongoClient != null)
            {
                _database = mongoClient.GetDatabase(_configuration.GetValue<string>("MongoSettings:Database"));
            }
        }

        public IMongoCollection<CatalogItem> Catalog
        {
            get
            {
                return _database.GetCollection<CatalogItem>("products");
            }
        }
    }
}
