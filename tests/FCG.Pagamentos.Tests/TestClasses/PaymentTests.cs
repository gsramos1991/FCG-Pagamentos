using FCG.Pagamentos.Business.Model;
using FluentAssertions;
using Xunit;

namespace FCG.Pagamentos.Tests.Tests;

public class PaymentTests
{
    [Fact]
    public void Constructor_Should_Validate_Basic_Fields()
    {
        var items = new List<PaymentItem>
        {
            new PaymentItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Item 1", 5m, 2)
        };

        var paymentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var act = () => new Payment(Guid.NewGuid(), paymentId, userId, "BRL", "PENDING", items, 10m, DateTime.UtcNow);
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Currency_Invalid()
    {
        var items = new List<PaymentItem>
        {
            new PaymentItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Item 1", 5m, 2)
        };

        Action act = () => new Payment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "BR", "PENDING", items, 10m, DateTime.UtcNow);
        act.Should().Throw<ArgumentException>().WithMessage("*Currency*");
    }

    [Fact]
    public void Constructor_Should_Throw_When_Items_Empty()
    {
        var items = new List<PaymentItem>();
        Action act = () => new Payment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "BR", "PENDING", items, 10m, DateTime.UtcNow);
        act.Should().Throw<ArgumentException>().WithMessage("*lista de items*");
    }
}
