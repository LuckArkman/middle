using IntelligentAutomation.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace IntelligentAutomation.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDbConnection");
        var mongoUrl = new MongoUrl(connectionString);
        var client = new MongoClient(mongoUrl);
        _database = client.GetDatabase(mongoUrl.DatabaseName);
    }
    public IMongoCollection<ModuleManifest> ModuleManifests => _database.GetCollection<ModuleManifest>("moduleManifests");
    public IMongoCollection<Agent> Agents => _database.GetCollection<Agent>("agents");
    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Plan> Plans => _database.GetCollection<Plan>("plans");
    public IMongoCollection<Subscription> Subscriptions => _database.GetCollection<Subscription>("subscriptions");
}