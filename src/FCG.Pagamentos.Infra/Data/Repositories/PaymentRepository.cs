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
    public class PaymentRepository : IPaymentRepository
    {
        protected readonly ApplicationDbContext _db;
        public PaymentRepository(ApplicationDbContext context)
        {
            _db = context;
        }

        public async Task<Payment> Adicionar(Payment payment)
        {
            _db.Payment.Add(payment);
            await _db.SaveChangesAsync();
            return payment;
        }

        public async Task AtualizarStatusPagamento(PaymentRequest payment)
        {
            var ExistingPayment = await _db.Payment.FirstOrDefaultAsync(p => p.PaymentId == payment.PaymentId && p.UserId == payment.UserId);

            if (ExistingPayment == null)
                throw new Exception("Pagamento nao encontrado");

            ExistingPayment.StatusPayment = payment.StatusPayment ?? "PENDING";
            await _db.SaveChangesAsync();
            return;
        }

        public async Task CancelarPagamento(Guid PaymentId, Guid UserId)
        {
            var ExistingPayment = await _db.Payment.FirstOrDefaultAsync(p => p.PaymentId == PaymentId && p.UserId == UserId);
            if (ExistingPayment == null)
                throw new Exception("Pagamento nao encontrado");

            ExistingPayment.StatusPayment = "CANCELED";
            await _db.SaveChangesAsync();
            return;
        }

        public async Task<IEnumerable<Payment>> ListarPagamentos(string statusPagamento)
        {
            var pagamento = await _db.Payment.Where(p => p.StatusPayment == statusPagamento)
                                        .AsNoTracking()
                                        .ToListAsync();
            return pagamento;
        }

        public async Task<Payment?> ObterCompras(PaymentRequest request)
        {
            var pagamento = await _db.Payment.AsNoTracking()
                                        .Include(p => p.Items)
                                        .FirstOrDefaultAsync(p => p.PaymentId == request.PaymentId && p.UserId == request.UserId);
            return pagamento;
        }

        public async Task<List<Payment>> ObterComprasPorUsuario(Guid UserId)
        {
            var pagamento = await _db.Payment.Where(w => w.UserId == UserId)
                                        .AsNoTracking()
                                        .Include(p => p.Items)
                                        .ToListAsync();

            return pagamento;
        }

        public async Task<string> ObterUsuarioPorPagamento(Guid Payment)
        {
            var result = await _db.Payment.Where(w => w.PaymentId == Payment)
                                        .Select(s => s.UserId.ToString())
                                        .FirstOrDefaultAsync();
            return result ?? string.Empty;
        }
    }
}
