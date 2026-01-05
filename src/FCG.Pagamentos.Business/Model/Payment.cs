using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Business.Model
{
    public class Payment
    {
        public Guid OrderId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid UserId { get; set; }
        public string Currency { get; set; } = "BRL";
        public string StatusPayment { get; set; } = "Pending";
        public List<PaymentItem> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public Payment()
        {
            
        }

        public Payment(Guid orderId, Guid paymentId, Guid userId, string currency, string statusPayment, List<PaymentItem> paymentItems, decimal totalAmount, DateTime createdAt)
        {
            ValidarPropriedades(orderId, paymentId, userId, currency, statusPayment, paymentItems, totalAmount);
            ValidarValorUnitario();
            OrderId = orderId;
            PaymentId = paymentId;
            UserId = userId;
            Currency = currency;
            StatusPayment = statusPayment;
            TotalAmount = totalAmount;
            CreatedAt = createdAt;
            Items = paymentItems;
        }

        private void ValidarPropriedades(Guid orderId,Guid paymentId, Guid userId, string currency, string statusPayment, List<PaymentItem> paymentItems, decimal totalAmount)
        {
            ValidarId(paymentId);
            ValidarId(userId);
            ValidarId(orderId);
            ValidarString(currency, nameof(currency));
            ValidarString(statusPayment, nameof(statusPayment));
            ValidarTotalAmount(totalAmount);
            ValidarQuantidadeItems(paymentItems);
            ValidarMoeda(currency);
        }

        private void ValidarValorUnitario() 
        { 
            if(Items == null)
                throw new ArgumentException("Items nao pode ser nulo");

            if (Items is { Count: > 0})
            {
                foreach (var item in Items)
                {
                    if (item.UnitPrice <= 0)
                        throw new ArgumentException("UnitPrice deve ser maior que zero");
                }

                var soma = Items.Sum(x => x.TotalPrice);
                if(Math.Round(soma,2) != Math.Round(TotalAmount, 2))
                    throw new ArgumentException("TotalAmount deve ser igual a soma dos itens");
            }
        }
        private static void ValidarId(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id não pode ser vazio");

            if(!Guid.TryParse(id.ToString(), out _))
                throw new ArgumentException("Id deve ser um GUID válido");
        }
        private static void ValidarString(string valor, string nomeCampo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException($"{nomeCampo} não pode ser vazio");
        }
        private static void ValidarTotalAmount(decimal totalAmount)
        {
            if (totalAmount < 0)
                throw new ArgumentException("TotalAmount deve ser maior ou igual a zero");
        }
        private static void ValidarQuantidadeItems(List<PaymentItem> items)
        {
            if (items == null || !items.Any())
                throw new ArgumentException("A lista de items não pode ser vazia");
        }

        private static void ValidarMoeda(string currency)
        {
            if(currency == null) throw new ArgumentNullException($"{nameof(currency)} Nao pode ser nulo");
            if(currency.Length != 3)
                throw new ArgumentException("Currency deve ter exatamente 3 caracteres");
        }

    }
}
