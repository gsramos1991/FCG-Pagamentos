using FCG.Pagamentos.API.Controllers;
using FCG.Pagamentos.Business.Model;
using FCG.Pagamentos.Business.Services.Interface;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace FCG.Pagamentos.Tests.Tests;

public class PaymentEventsControllerTests
{
    private readonly Mock<IPaymentEventService> _eventService = new();

    [Fact]
    public async Task GetAll_Should_Return_All_Events()
    {
        var list = new List<PaymentEvent>
        {
            new PaymentEvent{ Id=Guid.NewGuid(), PaymentId=Guid.NewGuid(), EventType="Payment_Pending", Version=1, EventDate=DateTime.UtcNow },
            new PaymentEvent{ Id=Guid.NewGuid(), PaymentId=Guid.NewGuid(), EventType="Payment_Consulted", Version=2, EventDate=DateTime.UtcNow }
        };
        _eventService.Setup(s => s.ListarTodosEventos()).ReturnsAsync(list);

        var controller = new PaymentEventsController(_eventService.Object, NullLogger<PaymentEventsController>.Instance);
        var result = await controller.GetAll() as OkObjectResult;

        result.Should().NotBeNull();
        var payload = result!.Value as IEnumerable<PaymentEvent>;
        payload.Should().NotBeNull();
        payload!.Count().Should().Be(2);
    }

    [Fact]
    public async Task GetByType_Should_Filter_By_EventType()
    {
        var type = "Payment_Pending";
        var list = new List<PaymentEvent>
        {
            new PaymentEvent{ Id=Guid.NewGuid(), PaymentId=Guid.NewGuid(), EventType=type, Version=1, EventDate=DateTime.UtcNow }
        };
        _eventService.Setup(s => s.ListarEventosPorTipo(type)).ReturnsAsync(list);

        var controller = new PaymentEventsController(_eventService.Object, NullLogger<PaymentEventsController>.Instance);
        var result = await controller.GetByType(type) as OkObjectResult;

        result.Should().NotBeNull();
        var payload = result!.Value as IEnumerable<PaymentEvent>;
        payload.Should().NotBeNull();
        payload!.Single().EventType.Should().Be(type);
    }
}

