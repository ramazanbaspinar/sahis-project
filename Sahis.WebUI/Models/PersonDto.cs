namespace Orion.Models
{
    public class PersonDto
    {
        public long Tc { get; set; }
        public long? AnneTc { get; set; }
        public long? BabaTc { get; set; }
        public string FullName { get; set; } = "";
        public string AnneBaba { get; set; } = "";
        public string DogumBilgisi { get; set; } = "";
        public string OlumTarihi { get; set; } = "";
        public string Lokasyon { get; set; } = "";
        public string Cinsiyet { get; set; } = "";
        public string MedeniHal { get; set; } = "";
        public List<string> Telefonlar { get; set; } = new();
        public string Adres { get; set; } = "";
        public string Uyruk { get; set; } = "";
        public string VergiNo { get; set; } = "";
        public string DogumYeri { get; set; } = "";
        public string NufusIl { get; set; } = "";
        public string NufusIlce { get; set; } = "";
        public string NufusKoy { get; set; } = "";
    }
}