using FCG.Pagamentos.API.Models;
using FCG.Pagamentos.Business.Model;

namespace FCG.Pagamentos.API.MappingDtos
{
    public static class PaymentMappingExtensions
    {
        public static Payment convertToDomain(this PaymentDto Dto)
        {
            Guid IdPayment = Guid.NewGuid();
            var paymentItems = new List<PaymentItem>();
            if (Dto.Items != null && Dto.Items.Count > 0)
            {
                foreach (var item in Dto.Items)
                {
                    paymentItems.Add(ProcessarItems(IdPayment, item));
                }
            }

            Payment payment = ProcessarPedido(Dto, IdPayment, paymentItems);
            
            return CalcularTotal(payment);
        }

        private static Payment ProcessarPedido(PaymentDto Dto, Guid IdPayment, List<PaymentItem> paymentItems)
        {
            return new Payment
                        (
                            paymentId: IdPayment,
                            userId: Dto.UserId,
                            currency: Dto.Currency,
                            statusPayment: "PENDING",
                            paymentItems: paymentItems,
                            totalAmount: 0,
                            createdAt: DateTime.UtcNow
                        );
        }

        private static PaymentItem ProcessarItems(Guid IdPayment, PaymentItemDto item)
        {
            return new PaymentItem(
                paymentItemId: Guid.NewGuid(),
                paymentId: IdPayment,
                jogoId: item.JogoId,
                description: item.Description,
                unitPrice: item.UnitPrice,
                quantity: item.Quantity);
        }

        private static Payment CalcularTotal(Payment payment)
        {
            if (payment.Items == null || payment.Items.Count == 0)
                throw new ArgumentException("O pagamento deve conter pelo menos um item.");
            decimal total = 0;
            foreach (var item in payment.Items)
            {
                total += item.TotalPrice;
            }

            payment.TotalAmount = total;
            return payment;
        }

        public static PaymentRequest convertToDomain(this PaymentRequestDto dto)
        {
            return new PaymentRequest
            {
                PaymentId = dto.PaymentId,
                UserId = dto.UserId,
                StatusPayment = dto.StatusPayment ?? "PENDING"

            };
        }

        public static PaymentRequest convertToDomain(Guid PaymentId, Guid UserId)
        {
            return new PaymentRequest
            {
                PaymentId = PaymentId,
                UserId = UserId
            };
        }
    }
}
