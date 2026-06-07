using Microsoft.EntityFrameworkCore;
using IglesiaAPI.Models;

namespace IglesiaAPI.Data
{
    public class IglesiaDBContext : DbContext
    {
        public IglesiaDBContext(DbContextOptions<IglesiaDBContext> options)
            : base(options)
        {
        }

        // 🔹 DbSets para todas las entidades
        public DbSet<Localidad> Localidades { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Cuenta> Cuentas { get; set; } = null!;
        public DbSet<Movimiento> Movimientos { get; set; } = null!;
        public DbSet<Inventario> Inventarios { get; set; } = null!;
        public DbSet<Miembro> Miembros { get; set; } = null!;
        public DbSet<Celula> Celulas { get; set; } = null!;
        public DbSet<RegistroSecretaria> RegistrosSecretaria { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 Usuario
            modelBuilder.Entity<Usuario>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Localidad)
                .WithMany(l => l.Usuarios)
                .HasForeignKey(u => u.LocalidadID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Miembro)
                .WithMany(m => m.Usuarios)
                .HasForeignKey(u => u.MiembroID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // 🔹 Miembro
            modelBuilder.Entity<Miembro>()
                .HasOne(m => m.Localidad)
                .WithMany(l => l.Miembros)
                .HasForeignKey(m => m.LocalidadID)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Celula
            modelBuilder.Entity<Celula>()
                .HasOne(c => c.Localidad)
                .WithMany(l => l.Celulas) // ✅ mejor declarar colección en Localidad
                .HasForeignKey(c => c.LocalidadID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Celula>()
                .HasOne(c => c.Miembro)
                .WithMany()
                .HasForeignKey(c => c.MiembroID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // 🔹 Inventario
            modelBuilder.Entity<Inventario>()
                .HasOne(i => i.Localidad)
                .WithMany(l => l.Inventarios)
                .HasForeignKey(i => i.LocalidadID)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Movimiento
            modelBuilder.Entity<Movimiento>()
                .HasOne(m => m.Cuenta)
                .WithMany(c => c.Movimientos)
                .HasForeignKey(m => m.CuentaID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Movimiento>()
                .HasOne(m => m.Localidad)
                .WithMany(l => l.Movimientos)
                .HasForeignKey(m => m.LocalidadID)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 RegistroSecretaria
            modelBuilder.Entity<RegistroSecretaria>()
                .HasOne(r => r.Localidad)
                .WithMany(l => l.RegistrosSecretaria) // ✅ mejor declarar colección en Localidad
                .HasForeignKey(r => r.LocalidadID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RegistroSecretaria>()
                .HasOne(r => r.CreadoPor)
                .WithMany()
                .HasForeignKey(r => r.CreadoPorID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}