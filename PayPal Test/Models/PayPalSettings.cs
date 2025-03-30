using System.ComponentModel.DataAnnotations;

namespace PayPal_Test.Models
{
    public class PayPalSettings
    {
        [Key]
        public int Id { get; set; }
        public required string Key { get; set; }
        public required string Value { get; set; }
    }
}
