using FCG.Pagamentos.Business.Model;

namespace FCG.Pagamentos.API.Models
{
    public class PaymentDto
    {
        public Guid UserId { get; set; }
        public string Currency { get; set; } = "BRL";
        public List<PaymentItemDto> Items { get; set; } = new();
    }
}
