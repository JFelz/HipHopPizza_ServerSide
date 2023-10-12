using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HipHopPizza_ServerSide.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageURL { get; set; }
        public double? Price { get; set; }
        public string Category { get; set; }
    }
}
