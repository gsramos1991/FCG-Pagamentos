using FCG.Pagamentos.Business.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Business.Services.Interface
{
    public interface IPaymentService
    {
        Task<PaymentResponse> Adicionar(Payment payment);
        Task<PaymentResponse> AtualizarStatusPagamento(PaymentRequest paymentRequest);
        Task<PaymentResponse> CancelarPagamento(PaymentRequest id);
        Task<Payment> ObterCompras(PaymentRequest id);
        Task<IEnumerable<Payment>> ListarPagamentos(string statusPagamento);
        Task<List<Payment>> ObterComprasPorUsuario(Guid UserId);
        Task<string> ObterUsuarioPorPagamento(Guid Payment);

    }
}
