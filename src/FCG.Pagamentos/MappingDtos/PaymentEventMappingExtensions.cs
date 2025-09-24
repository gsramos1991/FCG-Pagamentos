using FCG.Pagamentos.API.Models;
using FCG.Pagamentos.Business.Model;
using System;
using System.Data;

namespace FCG.Pagamentos.API.MappingDtos
{
    public static class PaymentEventMappingExtensions
    {
        public static PaymentEvent convertToDomain(Payment? Dto, int version, string eventType)
        {
            if (Dto == null) 
            {
               
                return PaymentNotFound(Dto, version);

            }

            return AddNewEvent(Dto, eventType);



        }
        public static PaymentEvent convertToDomain(Payment Dto, string eventType)
        {
            var evtType = PaymentEventTypeMapper.FromToken(eventType);
            return new PaymentEvent
            {
                EventDate = DateTime.UtcNow,
                Id = Guid.NewGuid(),
                PayLoad = System.Text.Json.JsonSerializer.Serialize(Dto),
                PaymentId = Dto.PaymentId == Guid.Empty ? Guid.Empty : Dto.PaymentId,
                EventType = evtType,
            };



        }

        private static PaymentEvent PaymentNotFound(Payment dto, int lastVersion)
        {
            return new PaymentEvent
            (
                id: Guid.NewGuid(),
                paymenId: dto?.PaymentId ?? Guid.Empty,
                eventType: "Payment_Not_Found",
                payLoad: $"Pagamento com ID {(dto?.PaymentId == Guid.Empty ? "" : dto?.PaymentId.ToString())} não encontrado.",
                version: lastVersion +1,
                eventDate: DateTime.UtcNow
            );
        }

        private static PaymentEvent AddNewEvent(Payment payment, string eventType)
        {
            var evtType = PaymentEventTypeMapper.FromToken(eventType);
            return new PaymentEvent()
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.PaymentId,
                EventDate = DateTime.UtcNow,
                PayLoad = System.Text.Json.JsonSerializer.Serialize(payment),
                EventType = evtType
                            
            };
        }

        public static string TipoFinalPagamento(int version)
        {
            if(version <= 10)
            {
                return "PENDING";
            }
            if(version > 10 && version <= 15)
            {
                return "ANALISE";
            }
            if(version > 15 && version <= 20)
            {
                return "REJECTED";
            }

            return "SUCESSO";
        }
    }
}
