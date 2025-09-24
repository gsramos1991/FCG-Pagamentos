using FCG.Pagamentos.Business.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Business.Interfaces
{
    public interface IPaymentEventRepository
    {
        Task Adicionar(PaymentEvent paymentEvent);
        Task<IEnumerable<PaymentEvent>> ListarEventosPorPagamento(Guid paymentId);
        Task<IEnumerable<PaymentEvent>> ListarTodosEventos();
        Task<IEnumerable<PaymentEvent>> ListarEventosPorTipo(string eventType);
        Task<PaymentEvent> ObterUltimoEventoPorPagamento(Guid paymentId);
    }
}
