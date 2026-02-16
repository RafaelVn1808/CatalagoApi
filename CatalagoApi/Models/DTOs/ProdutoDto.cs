namespace CatalagoApi.Models.DTOs;

public record ProdutoListDto(
    int Id,
    string Nome,
    string? Descricao,
    decimal Preco,
    string? ImagemUrl,
    string? Codigo,
    string CategoriaNome,
    IEnumerable<DisponibilidadeLojaDto> LojasDisponiveis
);

public record ProdutoDetalheDto(
    int Id,
    string Nome,
    string? Descricao,
    decimal Preco,
    string? ImagemUrl,
    string? Codigo,
    CategoriaDto Categoria,
    IEnumerable<DisponibilidadeLojaDto> LojasDisponiveis
);

public record ProdutoCreateDto(
    string Nome,
    string? Descricao,
    decimal Preco,
    string? Codigo,
    int CategoriaId,
    IEnumerable<EstoqueLojaDto> Estoques
);

public record ProdutoUpdateDto(
    string Nome,
    string? Descricao,
    decimal Preco,
    string? ImagemUrl,
    string? Codigo,
    bool Ativo,
    int CategoriaId
);

public record DisponibilidadeLojaDto(int LojaId, string LojaNome, string? LojaWhatsApp, int Quantidade);

public record EstoqueLojaDto(int LojaId, int Quantidade);
