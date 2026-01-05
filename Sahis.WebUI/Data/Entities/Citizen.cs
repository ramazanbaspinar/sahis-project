using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Orion.Data.Entities
{
    [Table("citizens")]
    public class Citizen
    {
        [Key]
        [Column("tc")]
        public long Tc { get; set; }

        [Column("ad")]
        public string? Ad { get; set; }

        [Column("soyad")]
        public string? Soyad { get; set; }

        [Column("baba_adi")]
        public string? BabaAdi { get; set; }

        [Column("baba_tc")]
        public long? BabaTc { get; set; }

        [Column("anne_adi")]
        public string? AnneAdi { get; set; }

        [Column("anne_tc")]
        public long? AnneTc { get; set; }

        [Column("dogum_tarihi")]
        public DateOnly? DogumTarihi { get; set; }

        [Column("olum_tarihi")]
        public DateOnly? OlumTarihi { get; set; }

        [Column("dogum_yeri")]
        public string? DogumYeri { get; set; }

        [Column("nufus_il")]
        public string? NufusIl { get; set; }

        [Column("nufus_ilce")]
        public string? NufusIlce { get; set; }

        [Column("nufus_koy")]
        public string? NufusKoy { get; set; }

        [Column("uyruk")]
        public string? Uyruk { get; set; }

        [Column("cinsiyet")]
        public string? Cinsiyet { get; set; }

        [Column("medeni_hal")]
        public string? MedeniHal { get; set; }

        [Column("gsm_listesi")]
        public string? GsmListesi { get; set; }

        [Column("ikametgah")]
        public string? Ikametgah { get; set; }

        [Column("vergi_no")]
        public string? VergiNo { get; set; }

        [Column("source_flags")]
        public string? SourceFlags { get; set; }
    }
}