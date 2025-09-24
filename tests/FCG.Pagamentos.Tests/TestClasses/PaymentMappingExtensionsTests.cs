using FCG.Pagamentos.API.MappingDtos;
using FCG.Pagamentos.API.Models;
using FluentAssertions;
using Xunit;

namespace FCG.Pagamentos.Tests.Tests;

public class PaymentMappingExtensionsTests
{
    [Fact]
    public void ConvertToDomain_Should_Map_And_Calculate_Total()
    {
        var dto = new PaymentDto
        {
            UserId = Guid.NewGuid(),
            Currency = "BRL",
            Items = new()
            {
                new PaymentItemDto { JogoId = Guid.NewGuid(), Description = "A", UnitPrice = 10m, Quantity = 2 },
                new PaymentItemDto { JogoId = Guid.NewGuid(), Description = "B", UnitPrice = 5m, Quantity = 1 }
            }
        };

        var domain = dto.convertToDomain();

        domain.PaymentId.Should().NotBe(Guid.Empty);
        domain.Items.Should().HaveCount(2);
        domain.TotalAmount.Should().Be(25m);
        domain.StatusPayment.Should().Be("PENDING");
        domain.Currency.Should().Be("BRL");
    }

    [Fact]
    public void PaymentRequest_Mapping_Should_Default_Status_To_Pending_When_Null()
    {
        var dto = new PaymentRequestDto { PaymentId = Guid.NewGuid(), UserId = Guid.NewGuid(), StatusPayment = null };
        var req = dto.convertToDomain();
        req.StatusPayment.Should().Be("PENDING");
    }
}
