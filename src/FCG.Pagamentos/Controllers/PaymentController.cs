using FCG.Pagamentos.API.MappingDtos;
using FCG.Pagamentos.API.Models;
using FCG.Pagamentos.Business.Model;
using System.Collections.Generic;
using FCG.Pagamentos.Business.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FCG.Pagamentos.Shared.Logging;

namespace FCG.Pagamentos.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentEventService _paymentEventService;
        private readonly ILogger<PaymentController> _logger;
        public PaymentController(IPaymentService paymentService, IPaymentEventService paymentEventService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _paymentEventService = paymentEventService;
            _logger = logger;
        }

        [HttpPost("SolicitacaoCompra")]
        [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SolicitacaoCompra([FromBody] PaymentDto paymentDto)
        {
            _logger.LogInformation("SolicitacaoCompra iniciada");
            using (_logger.BeginPaymentScope(nameof(PaymentController), nameof(SolicitacaoCompra), null, paymentDto?.UserId))
                try
                {
                    _logger.LogInformation("SolicitacaoCompra iniciada");
                    var paymentDomain = paymentDto.convertToDomain();
                    var result = await _paymentService.Adicionar(paymentDomain);
                    
                    _logger.LogInformation("SolicitacaoCompra concluida");
                    return Ok(result);
                }
                catch (Exception EX)
                {
                    _logger.LogError(EX, "Erro ao processar pagamento");
                    return BadRequest($"Erro ao processar pagamento {EX.Message}");
                }

        }

        [HttpPost("CancelarPagamento")]
        [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelarPagamento([FromBody] PaymentRequestDto paymentDto)
        {
            using (_logger.BeginPaymentScope(nameof(PaymentController), nameof(CancelarPagamento), paymentDto?.PaymentId, paymentDto?.UserId))
                try
                {
                    _logger.LogInformation("CancelarPagamento iniciado");
                    var domain = paymentDto.convertToDomain();
                    var result = await _paymentService.CancelarPagamento(domain);

                    if (!result.Success)
                    {
                        _logger.LogWarning("Cancelamento com falha: {Message}", result.Message);
                        return BadRequest(result.Message);
                    }

                    var compra = await _paymentService.ObterCompras(domain);
                    if (compra != null)
                    {
                        var events = PaymentEventMappingExtensions.convertToDomain(compra, "CANCELED");
                        await _paymentEventService.Adicionar(compra, events);
                    }
                    _logger.LogInformation("CancelarPagamento concluido");
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro em CancelarPagamento");
                    return BadRequest(ex.Message);
                }
        }

        [HttpGet("ConsultarPagamento/{paymentId:guid}/{userId:guid}")]
        [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConsultarPagamento(Guid paymentId, Guid userId)
        {
            using (_logger.BeginPaymentScope(nameof(PaymentController), nameof(ConsultarPagamento), paymentId, userId))
                try
                {
                    _logger.LogInformation("ConsultarPagamento iniciado {paymentId} {userId}", paymentId, userId);
                    var paymentRequest = new PaymentRequestDto { PaymentId = paymentId, UserId = userId };
                    var domain = paymentRequest.convertToDomain();
                    var result = await _paymentService.ObterComprasAtualizacao(domain);
                    if (result == null)
                    {
                        _logger.LogInformation("Pagamento não encontrado {paymentId} {userId}", paymentId, userId);
                        return NotFound("Pagamento não encontrado");
                    }

                    _logger.LogInformation("ConsultarPagamento concluido {paymentId} {userId}", paymentId, userId);
                    var events = PaymentEventMappingExtensions.convertToDomain(result, "CONSULTA_SITUACAO");
                    await _paymentEventService.Adicionar(result, events);


                    var response = new PaymentResponse()
                    {
                        OrderId = result.OrderId,
                        PaymentId = result.PaymentId,
                        StatusPayment = result.StatusPayment,
                        Success = true,
                        Message = "Pagamento encontrado com sucesso"
                    };

                    return Ok(response);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);

                }

        }
        [HttpGet("ListarComprasUsuario/{PaymentId:guid}/{UserId:guid}")]
        [ProducesResponseType(typeof(Payment), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ListarComprasUsuario([FromRoute] Guid PaymentId, [FromRoute] Guid UserId)
        {
            using (_logger.BeginPaymentScope(nameof(PaymentController), nameof(ListarComprasUsuario), PaymentId, UserId))
                try
                {
                    _logger.LogInformation("ListarComprasUsuario iniciado");
                    var domain = PaymentMappingExtensions.convertToDomain(PaymentId, UserId);
                    var result = await _paymentService.ObterCompras(domain);

                    if (result == null)
                    {
                        _logger.LogInformation("Nenhum valor encontrado");
                        return NotFound("Nenhum valor encontrado");
                    }

                    var events = PaymentEventMappingExtensions.convertToDomain(result, "CONSULTA");
                    await _paymentEventService.Adicionar(result, events);
                    _logger.LogInformation("ListarComprasUsuario concluido");
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro em ListarComprasUsuario");
                    return BadRequest(ex.Message);
                }
        }


        
    }
}
