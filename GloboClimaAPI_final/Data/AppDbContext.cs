using Microsoft.EntityFrameworkCore;

namespace GloboClimaAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet para gerenciamento de favoritos
        public DbSet<FavoritesData> Favorites { get; set; }

        // DbSet para gerenciamento de usuários (LoginData)
        public DbSet<LoginData> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração adicional (ex.: constraints, relacionamentos, etc.)

            // Evitar inserção explícita em colunas de identidade
            modelBuilder.Entity<FavoritesData>()
                .Property(f => f.Id)
                .ValueGeneratedOnAdd(); // Garante que o ID será gerado automaticamente

            // Configurar chave primária para LoginData (se necessário)
            modelBuilder.Entity<LoginData>()
                .HasKey(u => u.Id);

            // Configurações adicionais podem ser feitas aqui para qualquer relação ou restrição
        }
    }
}
