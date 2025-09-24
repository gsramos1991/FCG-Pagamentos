using System.Text.Json.Serialization;

namespace FCG.Pagamentos.API.Models
{
    public class PaymentRequestDto
    {
        public Guid PaymentId { get; set; }
        public Guid UserId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? StatusPayment { get; set; }
    }
}
