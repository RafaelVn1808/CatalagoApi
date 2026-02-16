namespace CatalagoApi.Models.DTOs;

public record CategoriaDto(int Id, string Nome, string? Descricao);

public record CategoriaCreateDto(string Nome, string? Descricao);

public record CategoriaUpdateDto(string Nome, string? Descricao);
