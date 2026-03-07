using CatalagoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalagoApi.Data;

public static class DbSeeder
{
    /// <param name="isDevelopment">Se true, cria também o usuário admin@teste.com / 123456 para debug.</param>
    public static async Task SeedAsync(AppDbContext db, bool isDevelopment = false, CancellationToken ct = default)
    {
        // Admin: admin@catalago.com / Admin@123 — cria apenas se não existir
        if (!await db.Usuarios.AnyAsync(u => u.Email.ToLower() == "admin@catalago.com", ct))
        {
            db.Usuarios.Add(new Usuario
            {
                Email = "admin@catalago.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Nome = "Administrador",
                Role = "Admin",
                DeveAlterarSenha = true
            });
        }

        // Debug: admin@teste.com / 123456 — apenas em Development
        if (isDevelopment && !await db.Usuarios.AnyAsync(u => u.Email.ToLower() == "admin@teste.com", ct))
        {
            db.Usuarios.Add(new Usuario
            {
                Email = "admin@teste.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Nome = "Admin Debug",
                Role = "Admin",
                DeveAlterarSenha = false
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
