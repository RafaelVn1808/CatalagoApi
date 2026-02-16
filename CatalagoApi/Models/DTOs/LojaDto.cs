namespace CatalagoApi.Models.DTOs;

public record LojaDto(
    int Id,
    string Nome,
    string? Endereco,
    string? Telefone,
    string? WhatsApp,
    string? Horario
);

public record LojaCreateDto(
    string Nome,
    string? Endereco,
    string? Telefone,
    string? WhatsApp,
    string? Horario
);

public record LojaUpdateDto(
    string Nome,
    string? Endereco,
    string? Telefone,
    string? WhatsApp,
    string? Horario
);
