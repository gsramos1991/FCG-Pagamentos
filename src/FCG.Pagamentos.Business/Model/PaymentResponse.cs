using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Business.Model
{
    public class PaymentResponse
    {
        public Guid? PaymentId { get; set; }
        public string? StatusPayment { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    }
}
