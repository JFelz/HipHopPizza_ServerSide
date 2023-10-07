using System.ComponentModel.DataAnnotations;

namespace HipHopPizza_ServerSide.Models
{
    public class Cashier
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int PhoneNumber { get; set; }
        public string? ImageURL { get; set; }
    }
}
