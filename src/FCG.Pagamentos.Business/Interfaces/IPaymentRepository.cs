using FCG.Pagamentos.Business.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Business.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment> Adicionar(Payment payment);
        Task AtualizarStatusPagamento(PaymentRequest payment);
        Task CancelarPagamento(Guid id, Guid UserId);
        Task<Payment> ObterCompras(PaymentRequest request);
        Task<List<Payment>> ObterComprasPorUsuario(Guid UserId);
        Task<IEnumerable<Payment>> ListarPagamentos(string statusPagamento);
        Task<string> ObterUsuarioPorPagamento(Guid Payment);
        Task<Guid> VerificaCompraExistente(Guid OrderId, Guid userId);
    }
}
