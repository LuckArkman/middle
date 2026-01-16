using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace IntelligentAutomation.Core.Controllers;

[ApiController]
[Route("[controller]")]
public class ModulesController : ControllerBase
{
    private readonly IMongoCollection<ModuleManifest> _manifestsCollection;

    public ModulesController(MongoDbContext mongoContext)
    {
        _manifestsCollection = mongoContext.ModuleManifests;
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog()
    {
        var manifests = await _manifestsCollection.Find(_ => true).ToListAsync();
        // Agrupa os manifestos por Área para facilitar o consumo no frontend
        var groupedCatalog = manifests
            .GroupBy(m => m.Area)
            .ToDictionary(g => g.Key, g => g.ToList());

        return Ok(groupedCatalog);
    }
}