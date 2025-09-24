using FCG.Pagamentos.API.MappingDtos;
using FCG.Pagamentos.Business.Model;
using FluentAssertions;
using Xunit;

namespace FCG.Pagamentos.Tests.Tests;

public class PaymentEventMappingExtensionsTests
{
    [Fact]
    public void ConvertToDomain_Should_Create_Consult_Event()
    {
        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Currency = "BRL",
            StatusPayment = "PENDING",
            Items = new(),
            TotalAmount = 0m,
            CreatedAt = DateTime.UtcNow
        };

        var evt = PaymentEventMappingExtensions.convertToDomain(payment, "CONSULTA");
        evt.EventType.Should().Be("Payment_Consulted");
        evt.PaymentId.Should().Be(payment.PaymentId);
        evt.PayLoad.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(5, "PENDING")]
    [InlineData(12, "ANALISE")]
    [InlineData(18, "REJECTED")]
    [InlineData(25, "SUCESSO")]
    public void TipoFinalPagamento_Should_Map_By_Version(int version, string expected)
    {
        PaymentEventMappingExtensions.TipoFinalPagamento(version).Should().Be(expected);
    }
}
