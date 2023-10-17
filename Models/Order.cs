using System.ComponentModel.DataAnnotations;

namespace HipHopPizza_ServerSide.Models
{
    public class Order
    {
        #region Identifier
        [Key]
        public int Id { get; set; }
        [Required]

        public int CashierId { get; set; }
        [Required]
        #endregion

        #region Navigation
        public List<Product> ProductList { get; set; }
        #endregion

        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string PaymentType { get; set; }
        public double Revenue { get; set; }
        public bool OrderStatus { get; set; }
        public bool Review { get; set; }
    }
}