using FCG.Pagamentos.Business.Model;
using FCG.Pagamentos.Infra.Data.Repositories;
using FCG.Pagamentos.Tests.Infra;
using FluentAssertions;
using Xunit;

namespace FCG.Pagamentos.Tests.Tests;

public class PaymentEventRepositoryTests
{
    [Fact]
    public async Task ObterUltimoEventoPorPagamento_Should_Return_Most_Recent_By_Date()
    {
        using var db = DbContextFactory.CreateInMemory(nameof(ObterUltimoEventoPorPagamento_Should_Return_Most_Recent_By_Date));
        var repo = new PaymentEventRepository(db);
        var paymentId = Guid.NewGuid();

        var older = new PaymentEvent { Id = Guid.NewGuid(), PaymentId = paymentId, EventType = "X", PayLoad = "{}", Version = 1, EventDate = DateTime.UtcNow.AddMinutes(-5)};
        var newer = new PaymentEvent { Id = Guid.NewGuid(), PaymentId = paymentId, EventType = "Y", PayLoad = "{}", Version = 2, EventDate = DateTime.UtcNow};

        await repo.Adicionar(older);
        await repo.Adicionar(newer);

        var last = await repo.ObterUltimoEventoPorPagamento(paymentId);
        last.Should().NotBeNull();
        last!.Id.Should().Be(newer.Id);
    }
}
