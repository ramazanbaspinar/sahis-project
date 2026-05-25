using Microsoft.EntityFrameworkCore;
using Orion.Data.Entities;

namespace Orion.Data.Context
{
    public class SahisDbContext : DbContext
    {
        public SahisDbContext(DbContextOptions<SahisDbContext> options) : base(options)
        {
        }

        public DbSet<Citizen> Citizens { get; set; }

        // C# tarafında kullanacağımız "Sanal" metod
        // Bu metod aslında SQL'deki "tr_to_en" fonksiyonuna gidecek.
        public string TrToEn(string text) => throw new NotSupportedException();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("pg_trgm");

            // SQL Fonksiyonunu Tanıtıyoruz
            modelBuilder.HasDbFunction(typeof(SahisDbContext).GetMethod(nameof(TrToEn), new[] { typeof(string) })!)
                .HasName("tr_to_en"); // SQL'deki fonksiyon adı

            // --- PERFORMANS İNDEKSLERİ ---
            // Soy ağacı sorgularını hızlandırmak için AnneTc ve BabaTc
            modelBuilder.Entity<Citizen>().HasIndex(c => c.AnneTc);
            modelBuilder.Entity<Citizen>().HasIndex(c => c.BabaTc);
            
            // Ad, Soyad ve İl/İlçe aramaları için
            modelBuilder.Entity<Citizen>().HasIndex(c => c.Ad);
            modelBuilder.Entity<Citizen>().HasIndex(c => c.Soyad);
            modelBuilder.Entity<Citizen>().HasIndex(c => c.NufusIl);
            modelBuilder.Entity<Citizen>().HasIndex(c => c.NufusIlce);

            base.OnModelCreating(modelBuilder);
        }
    }
}