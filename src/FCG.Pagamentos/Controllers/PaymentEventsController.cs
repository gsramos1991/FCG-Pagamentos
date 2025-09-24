using FCG.Pagamentos.Business.Model;
using FCG.Pagamentos.Business.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FCG.Pagamentos.Shared.Logging;

namespace FCG.Pagamentos.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentEventsController : ControllerBase
{
    private readonly IPaymentEventService _paymentEventService;
    private readonly ILogger<PaymentEventsController> _logger;

    public PaymentEventsController(IPaymentEventService paymentEventService, ILogger<PaymentEventsController> logger)
    {
        _paymentEventService = paymentEventService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PaymentEvent>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        using (_logger.BeginPaymentScope(nameof(PaymentEventsController), nameof(GetAll)))
        {
            _logger.LogInformation("Listando todos os eventos de pagamento");
            var events = await _paymentEventService.ListarTodosEventos();
            return Ok(events);
        }
    }

    [HttpGet("type/{eventType}")]
    [ProducesResponseType(typeof(IEnumerable<PaymentEvent>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByType([FromRoute] string eventType)
    {
        using (_logger.BeginPaymentScope(nameof(PaymentEventsController), nameof(GetByType)))
        {
            _logger.LogInformation("Listando eventos por tipo {EventType}", eventType);
            var events = await _paymentEventService.ListarEventosPorTipo(eventType);
            return Ok(events);
        }
    }
}

