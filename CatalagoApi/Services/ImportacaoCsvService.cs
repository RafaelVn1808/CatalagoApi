using System.Globalization;
using System.Text;
using CatalagoApi.Data;
using CatalagoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalagoApi.Services;

public class ImportacaoCsvResult
{
    public int LinhasProcessadas { get; set; }
    public int ProdutosCriados { get; set; }
    public int ProdutosAtualizados { get; set; }
    public IList<string> Erros { get; } = new List<string>();
    public bool Sucesso => Erros.Count == 0;
}

public class ImportacaoCsvService
{
    private readonly AppDbContext _db;

    public ImportacaoCsvService(AppDbContext db) => _db = db;

    /// <summary>
    /// Importa produtos e estoque a partir de um CSV.
    /// Formato: Codigo;Nome;Descricao;Preco;CategoriaId;Estoque_1;Estoque_2;...
    /// (Estoque_N = quantidade na loja de Id N). Cabeçalho obrigatório na primeira linha.
    /// Codigo vazio = novo produto (será gerado ou usado Nome). Codigo existente = atualiza produto e estoques.
    /// </summary>
    public async Task<ImportacaoCsvResult> ImportarAsync(Stream csvStream, CancellationToken ct = default)
    {
        var result = new ImportacaoCsvResult();
        using var reader = new StreamReader(csvStream, Encoding.UTF8);

        var headerLine = await reader.ReadLineAsync(ct);
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            result.Erros.Add("Arquivo CSV vazio ou sem cabeçalho.");
            return result;
        }

        var colunas = ParseCsvLine(headerLine);
        if (colunas.Count < 5)
        {
            result.Erros.Add("Cabeçalho deve ter ao menos: Codigo, Nome, Descricao, Preco, CategoriaId");
            return result;
        }

        var idxCodigo = ObterIndice(colunas, "Codigo", "codigo");
        var idxNome = ObterIndice(colunas, "Nome", "nome");
        var idxDescricao = ObterIndice(colunas, "Descricao", "descricao");
        var idxPreco = ObterIndice(colunas, "Preco", "preco");
        var idxCategoriaId = ObterIndice(colunas, "CategoriaId", "categoriaid");

        if (idxNome < 0 || idxPreco < 0 || idxCategoriaId < 0)
        {
            result.Erros.Add("Cabeçalho deve conter: Nome, Preco, CategoriaId (e opcionalmente Codigo, Descricao)");
            return result;
        }

        var estoqueColunas = new Dictionary<int, int>(); // LojaId -> índice da coluna
        for (var i = 5; i < colunas.Count; i++)
        {
            var col = colunas[i].Trim();
            if (col.StartsWith("Estoque_", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(col.AsSpan(8), NumberStyles.None, CultureInfo.InvariantCulture, out var lojaId))
                estoqueColunas[lojaId] = i;
        }

        var lojasIds = estoqueColunas.Keys.ToList();
        var lojasExistentes = await _db.Lojas
            .Where(l => lojasIds.Contains(l.Id))
            .Select(l => l.Id)
            .ToListAsync(ct);
        var categoriasExistentes = await _db.Categorias.Select(c => c.Id).ToListAsync(ct);

        var produtosPorCodigo = await _db.Produtos
            .Include(p => p.ProdutosLoja)
            .ToDictionaryAsync(p => p.Codigo ?? "", p => p, StringComparer.OrdinalIgnoreCase);

        var linhaNum = 1;
        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            linhaNum++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var valores = ParseCsvLine(line);
            if (valores.Count < 5) continue;

            var codigo = idxCodigo >= 0 && idxCodigo < valores.Count ? valores[idxCodigo].Trim() : null;
            var nome = idxNome < valores.Count ? valores[idxNome].Trim() : "";
            var descricao = idxDescricao >= 0 && idxDescricao < valores.Count ? valores[idxDescricao].Trim() : null;
            if (string.IsNullOrEmpty(nome))
            {
                result.Erros.Add($"Linha {linhaNum}: Nome é obrigatório.");
                continue;
            }

            if (!decimal.TryParse(idxPreco < valores.Count ? valores[idxPreco].Replace(",", ".") : "", NumberStyles.Number, CultureInfo.InvariantCulture, out var preco) || preco < 0)
            {
                result.Erros.Add($"Linha {linhaNum}: Preco inválido.");
                continue;
            }

            if (!int.TryParse(idxCategoriaId < valores.Count ? valores[idxCategoriaId] : "", NumberStyles.None, CultureInfo.InvariantCulture, out var categoriaId) || !categoriasExistentes.Contains(categoriaId))
            {
                result.Erros.Add($"Linha {linhaNum}: CategoriaId inválido ou inexistente.");
                continue;
            }

            var produto = produtosPorCodigo.GetValueOrDefault(codigo ?? "");
            if (produto != null)
            {
                produto.Nome = nome;
                produto.Descricao = descricao;
                produto.Preco = preco;
                produto.CategoriaId = categoriaId;
                produto.Ativo = true;
                result.ProdutosAtualizados++;
            }
            else
            {
                produto = new Produto
                {
                    Codigo = string.IsNullOrEmpty(codigo) ? $"IMP-{linhaNum}" : codigo,
                    Nome = nome,
                    Descricao = descricao,
                    Preco = preco,
                    CategoriaId = categoriaId,
                    Ativo = true
                };
                _db.Produtos.Add(produto);
                await _db.SaveChangesAsync(ct);
                produtosPorCodigo[produto.Codigo ?? ""] = produto;
                result.ProdutosCriados++;
            }

            foreach (var lojaId in lojasIds)
            {
                if (!lojasExistentes.Contains(lojaId)) continue;
                if (!estoqueColunas.TryGetValue(lojaId, out var colIdx) || colIdx >= valores.Count) continue;

                var qtdStr = valores[colIdx].Trim();
                if (!int.TryParse(qtdStr, NumberStyles.None, CultureInfo.InvariantCulture, out var quantidade))
                    quantidade = 0;
                if (quantidade < 0) quantidade = 0;

                var pl = produto.ProdutosLoja.FirstOrDefault(pl => pl.LojaId == lojaId);
                if (pl != null)
                    pl.Quantidade = quantidade;
                else
                    _db.ProdutosLoja.Add(new ProdutoLoja { ProdutoId = produto.Id, LojaId = lojaId, Quantidade = quantidade });
            }

            result.LinhasProcessadas++;
        }

        await _db.SaveChangesAsync(ct);
        return result;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var list = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }
            if (!inQuotes && (c == ';' || c == ','))
            {
                list.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }
            current.Append(c);
        }
        list.Add(current.ToString().Trim());
        return list;
    }

    private static int ObterIndice(List<string> colunas, params string[] nomes)
    {
        for (var i = 0; i < colunas.Count; i++)
        {
            var c = colunas[i].Trim();
            foreach (var n in nomes)
                if (c.Equals(n, StringComparison.OrdinalIgnoreCase))
                    return i;
        }
        return -1;
    }
}
