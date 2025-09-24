using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Business.Model
{
    public class PaymentRequest
    {
        public PaymentRequest()
        {
            
        }
        public Guid PaymentId { get; set; }
        public Guid UserId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string StatusPayment { get; set; } = string.Empty;

        public PaymentRequest(Guid userId, Guid paymentId, string status)
        {
            ValidarId(userId, nameof(userId));
            ValidarId(paymentId, nameof(paymentId));
            UserId = userId;
            PaymentId = paymentId;
            StatusPayment = status;
        }

        private static void ValidarId(Guid id, string type)
        {
            if (id == Guid.Empty)
                throw new ArgumentException($"{type} não pode ser vazio");

            if (!Guid.TryParse(id.ToString(), out _))
                throw new ArgumentException($"{type} deve ser um GUID válido");

        }
    }

    
}
