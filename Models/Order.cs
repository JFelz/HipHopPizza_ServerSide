namespace HipHopPizza_ServerSide.Models
{
    public class Order
    {
        #region Identifier
        public int Id { get; set; }
        public int CashierId { get; set; }
        #endregion

        #region Navigation
        public List<Product> OrderItems { get; set; }
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
