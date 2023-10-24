using HipHopPizza_ServerSide.Models;
using System.ComponentModel.DataAnnotations;

namespace HipHopPizza_ServerSide.DTO
{
    public class CreateOrderDto
    {
        public int Id { get; set; }
        [Required]
        public int CashierId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public long CustomerPhoneNumber { get; set; }
        public string PaymentType { get; set; }
        public double TipAmount { get; set; }
        public double Revenue { get; set; }
        public bool OrderStatus { get; set; }
        public bool Review { get; set; }
        public List<Product> ProductList { get; set; }
    }
}
