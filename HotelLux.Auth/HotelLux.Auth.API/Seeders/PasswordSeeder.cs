using HotelLux.Auth.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Auth.API.Seeders;

public static class PasswordSeeder
{
    public static async Task RegenerarHashesPlaceholderAsync(AuthDbContext db, CancellationToken ct)
    {
        var usuarios = await db.UsuarioApps
            .Where(u => u.PasswordHash.StartsWith("PLACEHOLDER"))
            .ToListAsync(ct);

        foreach (var u in usuarios)
        {
            Console.WriteLine($"[Seeder] Regenerando hash de password para usuario: {u.Username}");
            u.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!", 11);
            u.PasswordSalt = string.Empty;
        }

        if (usuarios.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}
