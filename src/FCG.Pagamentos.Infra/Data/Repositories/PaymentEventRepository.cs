using FCG.Pagamentos.Business.Interfaces;
using FCG.Pagamentos.Business.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Pagamentos.Infra.Data.Repositories
{
    public class PaymentEventRepository : IPaymentEventRepository
    {
        protected readonly ApplicationDbContext _db;
        public PaymentEventRepository(ApplicationDbContext context)
        {
            _db = context;
        }
        public async Task Adicionar(PaymentEvent paymentEvent)
        {
            _db.PaymentEvents.Add(paymentEvent);
            await _db.SaveChangesAsync();
            
        }

        public async Task<IEnumerable<PaymentEvent>> ListarEventosPorPagamento(Guid paymentId)
        {
            var events = await _db.PaymentEvents.Where(e => e.PaymentId == paymentId)
                                            .AsNoTracking()
                                            .ToListAsync();

            return events;
        }

        public async Task<IEnumerable<PaymentEvent>> ListarTodosEventos()
        {
            var events = await _db.PaymentEvents
                            .AsNoTracking()
                            .ToListAsync();
            return events;
        }

        public async Task<IEnumerable<PaymentEvent>> ListarEventosPorTipo(string eventType)
        {
            var events = await _db.PaymentEvents
                            .AsNoTracking()
                            .Where(e => e.EventType == eventType)
                            .ToListAsync();
            return events;
        }

        public async Task<PaymentEvent> ObterUltimoEventoPorPagamento(Guid paymentId)
        {
            var evento = await _db.PaymentEvents
                           .AsNoTracking()
                           .Where(e => e.PaymentId == paymentId)
                           .OrderByDescending(e => e.EventDate)
                           .FirstOrDefaultAsync();

            return evento;
        }
    }
}
