namespace FCG.Pagamentos.API.Models
{
    public class PaymentItemDto
    {
        public Guid JogoId { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

    }
}
