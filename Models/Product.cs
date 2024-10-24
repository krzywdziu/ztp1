using System.ComponentModel.DataAnnotations;

namespace Projekt1.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa produktu jest wymagana.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Nazwa produktu musi mieć od 3 do 20 znaków.")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Nazwa produktu może zawierać jedynie litery i cyfry.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Opis produktu nie może przekraczać 500 znaków.")]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Cena produktu musi być większa niż 0,01 PLN.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Ilość dostępnych produktów nie może być ujemna.")]
        public int StockQuantity { get; set; }

        // Status produktu: "Dostępny" lub "Niedostępny"
        public string Status => StockQuantity > 0 ? "Dostępny" : "Niedostępny";

        // Historia zmian
        public List<ChangeLog> ChangeHistory { get; set; } = new List<ChangeLog>();
    }

    public class ChangeLog
    {
        public int Id { get; set; }
        public DateTime ChangeDate { get; set; } = DateTime.Now;
        public string ChangeDescription { get; set; }
    }
}
