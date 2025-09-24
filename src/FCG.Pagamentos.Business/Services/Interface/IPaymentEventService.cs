using FCG.Pagamentos.Business.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Business.Services.Interface
{
    public interface IPaymentEventService
    {
        Task Adicionar(Payment payment);
        Task Adicionar(Payment payment, PaymentEvent paymentEvent);
        Task<IEnumerable<PaymentEvent>> ListarEventosPorPagamento(Guid paymentId);
        Task<IEnumerable<PaymentEvent>> ListarTodosEventos();
        Task<IEnumerable<PaymentEvent>> ListarEventosPorTipo(string eventType);
        Task<PaymentEvent> ObterUltimoEventoPorPagamento(Guid paymentId);
    }
}
