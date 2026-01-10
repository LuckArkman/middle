using AgentSaaS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgentSaaS.Infrastructure.Services;

public class HybridSearchService
{
    private readonly AppDbContext _context;
    private readonly IOpenAIClient _openAI; // Para gerar o embedding da query

    public async Task<List<string>> SearchAsync(Guid agentId, string query)
    {
        // 1. Gera o vetor da pergunta
        var queryEmbedding = await _openAI.GenerateEmbeddingAsync(query);

        // 2. SQL Híbrido: Combina Similaridade de Cosseno com Rank de Texto (BM25)
        // Essa query é complexa, mas é o segredo do sucesso.
        var sql = $@"
            WITH semantic_search AS (
                SELECT id, content, 1 - (embedding <=> @embedding) AS semantic_score
                FROM agent_memories
                WHERE agent_id = @agentId
                ORDER BY embedding <=> @embedding LIMIT 20
            ),
            keyword_search AS (
                SELECT id, content, ts_rank(to_tsvector('portuguese', content), plainto_tsquery('portuguese', @query)) AS keyword_score
                FROM agent_memories
                WHERE agent_id = @agentId AND to_tsvector('portuguese', content) @@ plainto_tsquery('portuguese', @query)
                LIMIT 20
            )
            SELECT 
                COALESCE(s.content, k.content) as content,
                (COALESCE(s.semantic_score, 0) * 0.7) + (COALESCE(k.keyword_score, 0) * 0.3) as final_score
            FROM semantic_search s
            FULL OUTER JOIN keyword_search k ON s.id = k.id
            ORDER BY final_score DESC
            LIMIT 5";
        var results = await _context.Database.SqlQueryRaw<string>(sql, ...).ToListAsync();
        return results;
    }
}