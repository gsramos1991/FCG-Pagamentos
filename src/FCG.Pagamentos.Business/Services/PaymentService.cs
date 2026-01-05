using FCG.Pagamentos.Business.Interfaces;
using FCG.Pagamentos.Business.Model;
using FCG.Pagamentos.Business.Services.Interface;
using Microsoft.Extensions.Logging;
using FCG.Pagamentos.Shared.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Business.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentEventService _paymentEventService;
        private readonly ILogger<PaymentService> _logger;
        public PaymentService(IPaymentRepository paymentRepository, IPaymentEventService paymentEventService, ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _paymentEventService = paymentEventService;
            _logger = logger;
        }
        public async Task<PaymentResponse> Adicionar(Payment payment)
        {
            var response = new PaymentResponse();
            try
            {
                using (_logger.BeginPaymentScope(nameof(PaymentService), nameof(Adicionar), payment.PaymentId, payment.UserId))
                {
                    _logger.LogInformation("Adicionando pagamento");
                }

                var pagamentoExistente = await _paymentRepository.VerificaCompraExistente(payment.OrderId, payment.UserId);
                if (pagamentoExistente != Guid.Empty)
                {
                    response = MontarResposta(payment.OrderId, Guid.Empty, 0, "Existing Order", -2, "Ordem de compra já existente", true);
                    
                    payment.StatusPayment = "EXISTING_ORDER";
                    payment.PaymentId = Guid.Empty;
                    await _paymentEventService.Adicionar(payment);
                    return response;
                }

                var result = await _paymentRepository.Adicionar(payment);
                var LastPayment = await _paymentEventService.ObterUltimoEventoPorPagamento(result.PaymentId);

                response = MontarResposta(payment.OrderId, result.PaymentId, payment.TotalAmount, result.StatusPayment, 1, "Pagamento em analise", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro em Adicionar pagamento");
                response = MontarResposta(payment.OrderId, Guid.Empty, payment.TotalAmount, "ERROR", -1, "Falha no getway", false);

            }
            await _paymentEventService.Adicionar(payment);
            return response;

        }

        public async Task<PaymentResponse> AtualizarStatusPagamento(PaymentRequest paymentRequest)
        {
            var response = new PaymentResponse();
            try
            {
                using (_logger.BeginPaymentScope(nameof(PaymentService), nameof(AtualizarStatusPagamento), paymentRequest.PaymentId, paymentRequest.UserId))
                {
                    _logger.LogInformation("Atualizando status do pagamento para {Status}", paymentRequest.StatusPayment);
                }
                await _paymentRepository.AtualizarStatusPagamento(paymentRequest);
                response.PaymentId = paymentRequest.PaymentId;
                response.StatusPayment = paymentRequest.StatusPayment ?? "PENDING";
                response.Success = true;
                response.Message = "Status do pagamento atualizado com sucesso";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status do pagamento");
                response.Success = false;
                response.Message = $"{ex.Message}";


            }

            return response;

        }

        public async Task<PaymentResponse> CancelarPagamento(PaymentRequest paymentCancel)
        {
            var response = new PaymentResponse();
            try
            {
                using (_logger.BeginPaymentScope(nameof(PaymentService), nameof(CancelarPagamento), paymentCancel.PaymentId, paymentCancel.UserId))
                {
                    _logger.LogInformation("Cancelando pagamento");
                }
                await _paymentRepository.CancelarPagamento(paymentCancel.PaymentId, paymentCancel.UserId);
                response.PaymentId = paymentCancel.PaymentId;
                response.StatusPayment = "CANCELED";
                response.Success = true;
                response.Message = "Pagamento Cancelado com sucesso";

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar pagamento");
                response.PaymentId = paymentCancel.PaymentId;
                response.StatusPayment = "Error";
                response.Success = false;
                response.Message = $"{ex.Message}";
            }

            return response;
        }

        public async Task<IEnumerable<Payment>> ListarPagamentos(string statusPagamento)
        {
            List<Payment> pagamentos = new List<Payment>();
            var lstTpPag = statusPagamento.Split(',');
            foreach (var tp in lstTpPag)
            {
                var lstPag = await _paymentRepository.ListarPagamentos(tp);
                pagamentos.AddRange(lstPag);
            }

            return pagamentos;
        }

        public async Task<Payment> ObterCompras(PaymentRequest request)
        {
            return await _paymentRepository.ObterCompras(request);
        }

        public async Task<List<Payment>> ObterComprasPorUsuario(Guid UserId)
        {
            var result = await _paymentRepository.ObterComprasPorUsuario(UserId);
            return result;
        }

        public async Task<string> ObterUsuarioPorPagamento(Guid Payment)
        {
            return await _paymentRepository.ObterUsuarioPorPagamento(Payment);
        }


        private PaymentResponse MontarResposta(Guid orderId, Guid PaymentId, decimal valorTotal, string StatusPayment, int statusProcesso, string message, bool sucesso)
        {
            var paymentResponse = new PaymentResponse()
            {
                OrderId = orderId,
                PaymentId = PaymentId,
                ProcessedAt = DateTime.Now,
                StatusPayment = StatusPayment,
                statusProcesso = statusProcesso,
                TotalValue = valorTotal,
                Message = message,
                Success = sucesso,
            };

            return paymentResponse;
        }
    }
}
