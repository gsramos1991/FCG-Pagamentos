using FCG.Pagamentos.Business.Model;
using FluentAssertions;
using Xunit;

namespace FCG.Pagamentos.Tests.Tests;

public class PaymentItemTests
{
    [Fact]
    public void TotalPrice_Should_Be_UnitPrice_Multiplied_By_Quantity()
    {
        var item = new PaymentItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Item", 10m, 3);
        item.TotalPrice.Should().Be(30m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Should_Throw_When_Quantity_Invalid(int qty)
    {
        Action act = () => new PaymentItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Item", 10m, qty);
        act.Should().Throw<ArgumentException>().WithMessage("*Quantity*");
    }

    [Theory]
    [InlineData(-0.01)]
    public void Constructor_Should_Throw_When_UnitPrice_Invalid(decimal unitPrice)
    {
        Action act = () => new PaymentItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Item", unitPrice, 1);
        act.Should().Throw<ArgumentException>().WithMessage("*UnitPrice*");
    }
}
