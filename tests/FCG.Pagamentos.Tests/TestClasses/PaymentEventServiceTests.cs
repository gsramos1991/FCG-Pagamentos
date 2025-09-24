using FCG.Pagamentos.Business.Interfaces;
using FCG.Pagamentos.Business.Model;
using FCG.Pagamentos.Business.Services;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace FCG.Pagamentos.Tests.Tests;

public class PaymentEventServiceTests
{
    private readonly Mock<IPaymentEventRepository> _repo = new();

    [Fact]
    public async Task Adicionar_Should_Set_Version_And_Save()
    {
        var service = new PaymentEventService(_repo.Object, NullLogger<PaymentEventService>.Instance);
        _repo.Setup(r => r.ObterUltimoEventoPorPagamento(It.IsAny<Guid>())).ReturnsAsync((PaymentEvent)null!);
        PaymentEvent? saved = null;
        _repo.Setup(r => r.Adicionar(It.IsAny<PaymentEvent>()))
            .Callback<PaymentEvent>(pe => saved = pe)
            .Returns(Task.CompletedTask);

        var payment = new Payment { PaymentId = Guid.NewGuid(), StatusPayment = "PENDING" };
        await service.Adicionar(payment);

        saved.Should().NotBeNull();
        saved!.Version.Should().Be(1);
        saved.PaymentId.Should().Be(payment.PaymentId);
        saved.EventType.Should().Be("Payment_Pending");
    }

    [Fact]
    public async Task Adicionar_With_Provided_Event_Should_Increment_Version()
    {
        var last = new PaymentEvent { PaymentId = Guid.NewGuid(), Version = 2 };
        _repo.Setup(r => r.ObterUltimoEventoPorPagamento(last.PaymentId)).ReturnsAsync(last);

        var service = new PaymentEventService(_repo.Object, NullLogger<PaymentEventService>.Instance);
        var payment = new Payment { PaymentId = last.PaymentId, StatusPayment = "PENDING" };
        var evt = new PaymentEvent { PaymentId = last.PaymentId, EventType = "Payment_Updated" };

        PaymentEvent? saved = null;
        _repo.Setup(r => r.Adicionar(It.IsAny<PaymentEvent>()))
            .Callback<PaymentEvent>(pe => saved = pe)
            .Returns(Task.CompletedTask);

        await service.Adicionar(payment, evt);

        saved.Should().NotBeNull();
        saved!.Version.Should().Be(3);
    }
}
