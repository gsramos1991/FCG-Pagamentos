using FCG.Pagamentos.Business.Interfaces;
using FCG.Pagamentos.Business.Model;
using FCG.Pagamentos.Business.Services;
using FCG.Pagamentos.Business.Services.Interface;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace FCG.Pagamentos.Tests.Tests;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _repo = new();
    private readonly Mock<IPaymentEventService> _eventService = new();

    [Fact]
    public async Task Adicionar_Should_Save_And_Return_Response()
    {
        var service = new PaymentService(_repo.Object, _eventService.Object, NullLogger<PaymentService>.Instance);
        var payment = new Payment { PaymentId = Guid.NewGuid(), UserId = Guid.NewGuid(), StatusPayment = "PENDING" };
        _repo.Setup(r => r.Adicionar(It.IsAny<Payment>())).ReturnsAsync(payment);
        _eventService.Setup(e => e.ObterUltimoEventoPorPagamento(It.IsAny<Guid>())).ReturnsAsync((PaymentEvent)null!);

        var resp = await service.Adicionar(payment);

        resp.Success.Should().BeTrue();
        resp.PaymentId.Should().Be(payment.PaymentId);
        resp.StatusPayment.Should().Be(payment.StatusPayment);
    }

    [Fact]
    public async Task AtualizarStatusPagamento_Should_Call_Repo_And_Return_Response()
    {
        var service = new PaymentService(_repo.Object, _eventService.Object, NullLogger<PaymentService>.Instance);
        var req = new PaymentRequest { PaymentId = Guid.NewGuid(), UserId = Guid.NewGuid(), StatusPayment = "ANALISE" };

        _repo.Setup(r => r.AtualizarStatusPagamento(It.IsAny<PaymentRequest>())).Returns(Task.CompletedTask);

        var resp = await service.AtualizarStatusPagamento(req);

        resp.Success.Should().BeTrue();
        resp.PaymentId.Should().Be(req.PaymentId);
        resp.StatusPayment.Should().Be("ANALISE");
    }

    [Fact]
    public async Task CancelarPagamento_Should_Call_Repo_And_Return_Canceled()
    {
        var service = new PaymentService(_repo.Object, _eventService.Object, NullLogger<PaymentService>.Instance);
        var req = new PaymentRequest { PaymentId = Guid.NewGuid(), UserId = Guid.NewGuid() };

        _repo.Setup(r => r.CancelarPagamento(req.PaymentId, req.UserId)).Returns(Task.CompletedTask);

        var resp = await service.CancelarPagamento(req);

        resp.Success.Should().BeTrue();
        resp.StatusPayment.Should().Be("CANCELED");
        resp.PaymentId.Should().Be(req.PaymentId);
    }
}
