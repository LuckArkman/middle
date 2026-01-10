using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgentSaaS.Web.Controllers;
[Authorize]
public class KnowledgeController : Controller
{
    private readonly IKernelMemory _memory;

    [HttpPost]
    public async Task<IActionResult> UploadPdf(IFormFile file, Guid agentId)
    {
        using var stream = file.OpenReadStream();
        
        // Importa o documento e vetoriza automaticamente
        await _memory.ImportDocumentAsync(stream, 
            fileName: file.FileName, 
            documentId: Guid.NewGuid().ToString(),
            tags: new TagCollection
            {
                {
                    "agentId", agentId.ToString()
                }
            }
        );

        return Ok("Documento processado e aprendido.");
    }
}