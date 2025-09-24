using FCG.Pagamentos.API.Controllers;
using FCG.Pagamentos.API.Models;
using FCG.Pagamentos.Business.Model;
using FCG.Pagamentos.Business.Services.Interface;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FCG.Pagamentos.Tests.Tests;

public class PaymentControllerTests
{
    private readonly Mock<IPaymentService> _paymentService = new();
    private readonly Mock<IPaymentEventService> _paymentEventService = new();

    [Fact]
    public async Task SolicitacaoCompra_Should_Return_Ok_With_Response()
    {
        var controller = new PaymentController(_paymentService.Object, _paymentEventService.Object, NullLogger<PaymentController>.Instance);
        var dto = new PaymentDto
        {
            UserId = Guid.NewGuid(),
            Currency = "BRL",
            Items = new() { new PaymentItemDto { JogoId = Guid.NewGuid(), Description = "Item", UnitPrice = 10m, Quantity = 1 } }
        };

        _paymentService.Setup(s => s.Adicionar(It.IsAny<Payment>()))
            .ReturnsAsync(new PaymentResponse { PaymentId = Guid.NewGuid(), StatusPayment = "PENDING", Success = true });

        var result = await controller.SolicitacaoCompra(dto) as OkObjectResult;

        result.Should().NotBeNull();
        var response = result!.Value as PaymentResponse;
        response!.Success.Should().BeTrue();
        _paymentEventService.Verify(e => e.Adicionar(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarPagamento_Should_Return_NotFound_When_No_Payment()
    {
        var controller = new PaymentController(_paymentService.Object, _paymentEventService.Object, NullLogger<PaymentController>.Instance);
        var req = new PaymentRequestDto { PaymentId = Guid.NewGuid(), UserId = Guid.NewGuid(), StatusPayment = "PENDING" };

        _paymentService.Setup(s => s.ObterUsuarioPorPagamento(It.IsAny<Guid>()))
            .ReturnsAsync(Guid.NewGuid().ToString());
        _paymentService.Setup(s => s.ObterCompras(It.IsAny<PaymentRequest>()))
            .ReturnsAsync((Payment)null!);

        var result = await controller.AtualizarPagamento(req.PaymentId);
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ConsultarPagamento_Should_Return_Ok_When_Found()
    {
        var controller = new PaymentController(_paymentService.Object, _paymentEventService.Object, NullLogger<PaymentController>.Instance);
        var payment = new Payment { PaymentId = Guid.NewGuid(), UserId = Guid.NewGuid(), StatusPayment = "PENDING", Items = new(), TotalAmount = 0m, CreatedAt = DateTime.UtcNow };

        _paymentService.Setup(s => s.ObterCompras(It.IsAny<PaymentRequest>()))
            .ReturnsAsync(payment);

        var result = await controller.ConsultarPagamento(payment.PaymentId, payment.UserId);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CancelarPagamento_Should_Publish_Event_And_Return_Ok_When_Success()
    {
        var controller = new PaymentController(_paymentService.Object, _paymentEventService.Object, NullLogger<PaymentController>.Instance);
        var paymentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var req = new PaymentRequestDto { PaymentId = paymentId, UserId = userId };

        var payment = new Payment { PaymentId = paymentId, UserId = userId, StatusPayment = "CANCELED", Items = new(), TotalAmount = 0m, CreatedAt = DateTime.UtcNow };

        _paymentService.Setup(s => s.CancelarPagamento(It.IsAny<PaymentRequest>()))
            .ReturnsAsync(new PaymentResponse { PaymentId = paymentId, StatusPayment = "CANCELED", Success = true, Message = "Pagamento Cancelado com sucesso" });
        _paymentService.Setup(s => s.ObterCompras(It.IsAny<PaymentRequest>()))
            .ReturnsAsync(payment);

        var action = await controller.CancelarPagamento(req);

        action.Should().BeOfType<OkObjectResult>();
        _paymentEventService.Verify(e => e.Adicionar(payment, It.IsAny<PaymentEvent>()), Times.Once);
    }

    [Fact]
    public async Task ListarComprasUsuario_Should_Return_Ok_And_Publish_Event_When_Found()
    {
        var controller = new PaymentController(_paymentService.Object, _paymentEventService.Object, NullLogger<PaymentController>.Instance);
        var paymentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var payment = new Payment { PaymentId = paymentId, UserId = userId, StatusPayment = "PENDING", Items = new(), TotalAmount = 0m, CreatedAt = DateTime.UtcNow };

        _paymentService.Setup(s => s.ObterCompras(It.IsAny<PaymentRequest>()))
            .ReturnsAsync(payment);

        var action = await controller.ListarComprasUsuario(paymentId, userId) as OkObjectResult;

        action.Should().NotBeNull();
        action!.Value.Should().Be(payment);
        _paymentEventService.Verify(e => e.Adicionar(payment, It.IsAny<PaymentEvent>()), Times.Once);
    }

    [Fact]
    public async Task ListarComprasUsuario_Should_Return_NotFound_When_Not_Found()
    {
        var controller = new PaymentController(_paymentService.Object, _paymentEventService.Object, NullLogger<PaymentController>.Instance);
        var paymentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _paymentService.Setup(s => s.ObterCompras(It.IsAny<PaymentRequest>()))
            .ReturnsAsync((Payment)null!);

        var action = await controller.ListarComprasUsuario(paymentId, userId);

        action.Should().BeOfType<NotFoundObjectResult>();
        _paymentEventService.Verify(e => e.Adicionar(It.IsAny<Payment>(), It.IsAny<PaymentEvent>()), Times.Never);
    }

    [Fact]
    public async Task CancelarPagamento_Should_Return_BadRequest_When_Service_Fails()
    {
        var controller = new PaymentController(_paymentService.Object, _paymentEventService.Object, NullLogger<PaymentController>.Instance);
        var req = new PaymentRequestDto { PaymentId = Guid.NewGuid(), UserId = Guid.NewGuid() };

        _paymentService.Setup(s => s.CancelarPagamento(It.IsAny<PaymentRequest>()))
            .ReturnsAsync(new PaymentResponse { Success = false, Message = "Falha ao cancelar" });

        var action = await controller.CancelarPagamento(req);

        action.Should().BeOfType<BadRequestObjectResult>();
        _paymentEventService.Verify(e => e.Adicionar(It.IsAny<Payment>(), It.IsAny<PaymentEvent>()), Times.Never);
        _paymentService.Verify(s => s.ObterCompras(It.IsAny<PaymentRequest>()), Times.Never);
    }

    [Fact]
    public async Task CancelarPagamento_Should_Return_BadRequest_When_Service_Throws()
    {
        var controller = new PaymentController(_paymentService.Object, _paymentEventService.Object, NullLogger<PaymentController>.Instance);
        var req = new PaymentRequestDto { PaymentId = Guid.NewGuid(), UserId = Guid.NewGuid() };

        _paymentService.Setup(s => s.CancelarPagamento(It.IsAny<PaymentRequest>()))
            .ThrowsAsync(new Exception("Erro inesperado"));

        var action = await controller.CancelarPagamento(req);

        action.Should().BeOfType<BadRequestObjectResult>();
        _paymentEventService.Verify(e => e.Adicionar(It.IsAny<Payment>(), It.IsAny<PaymentEvent>()), Times.Never);
        _paymentService.Verify(s => s.ObterCompras(It.IsAny<PaymentRequest>()), Times.Never);
    }

    [Fact]
    public async Task ListarComprasUsuario_Should_Return_BadRequest_When_Service_Throws()
    {
        var controller = new PaymentController(_paymentService.Object, _paymentEventService.Object, NullLogger<PaymentController>.Instance);
        var paymentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _paymentService.Setup(s => s.ObterCompras(It.IsAny<PaymentRequest>()))
            .ThrowsAsync(new Exception("Erro de consulta"));

        var action = await controller.ListarComprasUsuario(paymentId, userId);

        action.Should().BeOfType<BadRequestObjectResult>();
        _paymentEventService.Verify(e => e.Adicionar(It.IsAny<Payment>(), It.IsAny<PaymentEvent>()), Times.Never);
    }
}
