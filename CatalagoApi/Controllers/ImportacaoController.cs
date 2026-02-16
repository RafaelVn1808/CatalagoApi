using CatalagoApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalagoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportacaoController : ControllerBase
{
    private readonly ImportacaoCsvService _importacaoCsv;

    public ImportacaoController(ImportacaoCsvService importacaoCsv) => _importacaoCsv = importacaoCsv;

    /// <summary>
    /// Importa produtos e estoque a partir de um arquivo CSV.
    /// Formato esperado (separador ; ou ,): Codigo;Nome;Descricao;Preco;CategoriaId;Estoque_1;Estoque_2;...
    /// Estoque_N = quantidade na loja de Id N. Cabeçalho na primeira linha.
    /// </summary>
    [HttpPost("csv")]
    [ProducesResponseType(typeof(ImportacaoCsvResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImportacaoCsvResult>> ImportarCsv(IFormFile? file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Envie um arquivo CSV (form-data: file)" });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".csv" && ext != ".txt")
            return BadRequest(new { message = "Arquivo deve ser .csv ou .txt" });

        await using var stream = file.OpenReadStream();
        var result = await _importacaoCsv.ImportarAsync(stream, ct);

        if (result.Erros.Count > 0 && result.LinhasProcessadas == 0)
            return BadRequest(result);

        return Ok(result);
    }
}
