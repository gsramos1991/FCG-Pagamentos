using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Business.Model
{
    public class PaymentEvent
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public string EventType { get; set; }
        public string PayLoad { get; set; }
        public int Version { get; set; }
        public DateTime EventDate { get; set; } = DateTime.UtcNow;


        public PaymentEvent()
        {
            
        }

        public PaymentEvent(Guid id, Guid paymenId, string eventType, string payLoad, int version, DateTime eventDate)
        {
            ValidarEvento(id, paymenId, eventType, payLoad, version, eventDate);

            Id = id;
            PaymentId = paymenId;
            this.EventType = eventType;
            PayLoad = payLoad;
            this.Version = version;
            EventDate = eventDate;
        }

        public void ValidarEvento(Guid id, Guid paymenId, string eventType, string payLoad, int version, DateTime eventDate) 
        {
            ValidarId(id);
            ValidarId(paymenId);
            ValidarString(eventType, nameof(eventType));
            ValidarString(payLoad, nameof(payLoad));
            ValidarVersao(version);

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
        private static void ValidarVersao(int versao)
        {
            if (versao < 0)
                throw new ArgumentException("Versão deve ser maior que zero");
        }
    }
}
