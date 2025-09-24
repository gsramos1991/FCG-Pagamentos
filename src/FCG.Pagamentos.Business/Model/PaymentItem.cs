using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Business.Model
{
    public class PaymentItem
    {
        public Guid PaymentItemId { get; set; } 
        public Guid PaymentId { get; set; }
        public Guid JogoId { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;

        public PaymentItem()
        {

        }

        public PaymentItem(Guid paymentItemId, Guid paymentId, Guid jogoId, string description, decimal unitPrice, int quantity)
        {
            ValidarPropriedades(paymentItemId, paymentId, jogoId, description, unitPrice, quantity);
            PaymentItemId = paymentItemId;
            PaymentId = paymentId;
            JogoId = jogoId;
            Description = description;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }
        private void ValidarPropriedades(Guid paymentItemId, Guid paymentId, Guid jogoId, string description, decimal unitPrice, int quantity)
        {
            ValidarId(paymentItemId);
            ValidarId(paymentId);
            ValidarId(jogoId);
            ValidarString(description, nameof(description));
            ValidarUnitPrice(unitPrice);
            ValidarQuantity(quantity);
        }
        private static void ValidarId(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id não pode ser vazio");
        }
        private static void ValidarString(string valor, string nomeCampo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException($"{nomeCampo} não pode ser vazio");
        }
        private static void ValidarUnitPrice(decimal unitPrice)
        {
            if (unitPrice < 0)
                throw new ArgumentException("UnitPrice deve ser maior ou igual a zero");
        }
        private static void ValidarQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity deve ser maior que zero");
        }


    }
}
