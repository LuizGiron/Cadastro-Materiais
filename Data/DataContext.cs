using Microsoft.EntityFrameworkCore;
using CadastroMateriais.Models;

namespace CadastroMateriais.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Material> Materiais { get; set; } // Tabela de Materiais

        public DbSet<Usuario> Usuarios { get; set; }

        public async Task<bool> TestConnection()
        {
            try
            {
                return await this.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao conectar ao banco de dados: {ex.Message}");
                return false;
            }
        }
    }
}
