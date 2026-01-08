using FCG.Pagamentos.Business.Model;
using FCG.Pagamentos.Infra.Data.Repositories;
using FCG.Pagamentos.Tests.Infra;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FCG.Pagamentos.Tests.Tests;

public class PaymentRepositoryTests
{
    [Fact]
    public async Task Adicionar_And_ObterCompras_Should_Persist_With_Items()
    {
        using var db = DbContextFactory.CreateInMemory(nameof(Adicionar_And_ObterCompras_Should_Persist_With_Items));
        var repo = new PaymentRepository(db);
        var paymentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var items = new List<PaymentItem>
        {
            new PaymentItem(Guid.NewGuid(), paymentId, Guid.NewGuid(), "Item 1", 10m, 1)
        };
        var payment = new Payment(Guid.NewGuid(),paymentId, userId, "BRL", "PENDING", items, 10m, DateTime.UtcNow);

        await repo.Adicionar(payment);

        var loaded = await repo.ObterCompras(new PaymentRequest{ PaymentId = paymentId, UserId = userId});
        loaded.Should().NotBeNull();
        loaded!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task AtualizarStatusPagamento_Should_Update_Status()
    {
        using var db = DbContextFactory.CreateInMemory(nameof(AtualizarStatusPagamento_Should_Update_Status));
        var repo = new PaymentRepository(db);
        var paymentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var items = new List<PaymentItem>
        {
            new PaymentItem(Guid.NewGuid(), paymentId, Guid.NewGuid(), "Item 1", 10m, 1)
        };
        var payment = new Payment(Guid.NewGuid(), paymentId, userId, "BRL", "PENDING", items, 10m, DateTime.UtcNow);
        await repo.Adicionar(payment);

        await repo.AtualizarStatusPagamento(new PaymentRequest{ PaymentId = paymentId, UserId = userId, StatusPayment = "ANALISE"});

        (await db.Payment.AsNoTracking().FirstAsync()).StatusPayment.Should().Be("ANALISE");
    }

    [Fact]
    public async Task CancelarPagamento_Should_Set_Canceled()
    {
        using var db = DbContextFactory.CreateInMemory(nameof(CancelarPagamento_Should_Set_Canceled));
        var repo = new PaymentRepository(db);
        var paymentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var items = new List<PaymentItem>
        {
            new PaymentItem(Guid.NewGuid(), paymentId, Guid.NewGuid(), "Item 1", 10m, 1)
        };
        var payment = new Payment(Guid.NewGuid(), paymentId, userId, "BRL", "PENDING", items, 10m, DateTime.UtcNow);
        await repo.Adicionar(payment);

        await repo.CancelarPagamento(paymentId, userId);

        (await db.Payment.AsNoTracking().FirstAsync()).StatusPayment.Should().Be("CANCELED");
    }
}
