using FCG.Pagamentos.Business.Interfaces;
using FCG.Pagamentos.Business.Model;
using FCG.Pagamentos.Business.Services.Interface;
using Microsoft.Extensions.Logging;
using FCG.Pagamentos.Shared.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Business.Services
{
  
    public class PaymentEventService : IPaymentEventService
    {
        private readonly IPaymentEventRepository _paymentEventRepository;
        private readonly ILogger<PaymentEventService> _logger;
        public PaymentEventService(IPaymentEventRepository paymentEventRepository, ILogger<PaymentEventService> logger)
        {
            _paymentEventRepository = paymentEventRepository;
            _logger = logger;
        }

        public async Task Adicionar(Payment payment)
        {
            using (_logger.BeginPaymentScope(nameof(PaymentEventService), nameof(Adicionar), payment.PaymentId, payment.UserId))
            {
                _logger.LogInformation("Registrando evento de pagamento {Status}", payment.StatusPayment);
            }
            var LastEvent = await _paymentEventRepository.ObterUltimoEventoPorPagamento(payment.PaymentId);
            var newEventType = PaymentEventTypeMapper.FromStatusString(payment.StatusPayment);
            if (LastEvent != null && LastEvent.EventType == newEventType)
                return; 
            var newVersion = LastEvent != null ? LastEvent.Version + 1 : 1;
            var paymentEvent = new PaymentEvent
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.PaymentId,
                EventType = newEventType,
                PayLoad = System.Text.Json.JsonSerializer.Serialize(payment),
                Version = newVersion,
                EventDate = DateTime.UtcNow
            };
            await _paymentEventRepository.Adicionar(paymentEvent);
            _logger.LogInformation("Evento de pagamento registrado, versao {Version}", newVersion);




        }

        public async Task Adicionar(Payment payment, PaymentEvent paymentEvent)
        {
            using (_logger.BeginPaymentScope(nameof(PaymentEventService), nameof(Adicionar), payment.PaymentId, payment.UserId))
            {
                _logger.LogInformation("Registrando evento customizado {Type}", paymentEvent.EventType);
            }
            var LastEvent = await _paymentEventRepository.ObterUltimoEventoPorPagamento(payment.PaymentId);
            var newVersion = LastEvent != null ? LastEvent.Version + 1 : 1;
            paymentEvent.Version = newVersion;


            await _paymentEventRepository.Adicionar(paymentEvent);
            _logger.LogInformation("Evento customizado registrado, versao {Version}", newVersion);





        }

        public async Task<IEnumerable<PaymentEvent>> ListarEventosPorPagamento(Guid paymentId)
        {
            var result = await _paymentEventRepository.ListarEventosPorPagamento(paymentId);
            return result;
        }

        public async Task<IEnumerable<PaymentEvent>> ListarTodosEventos()
        {
            var result = await _paymentEventRepository.ListarTodosEventos();
            return result;
        }

        public async Task<IEnumerable<PaymentEvent>> ListarEventosPorTipo(string eventType)
        {
            var result = await _paymentEventRepository.ListarEventosPorTipo(eventType);
            return result;
        }

        public async Task<PaymentEvent> ObterUltimoEventoPorPagamento(Guid paymentId)
        {
            var result = await _paymentEventRepository.ObterUltimoEventoPorPagamento(paymentId);
            return result;
        }
    }
}
