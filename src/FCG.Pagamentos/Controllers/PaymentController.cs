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
            using (_logger.BeginPaymentScope(nameof(PaymentController), nameof(SolicitacaoCompra), null, paymentDto?.UserId))
                try
                {
                    _logger.LogInformation("SolicitacaoCompra iniciada");
                    var paymentDomain = paymentDto.convertToDomain();
                    var result = await _paymentService.Adicionar(paymentDomain);
                    await _paymentEventService.Adicionar(paymentDomain);


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

        [HttpPut("AtualizarPagamento")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AtualizarPagamento([FromBody] Guid paymentRequest)
        {
            using (_logger.BeginPaymentScope(nameof(PaymentController), nameof(AtualizarPagamento), paymentRequest))
                try
                {
                    _logger.LogInformation("AtualizarPagamento iniciado");
                    var user = await _paymentService.ObterUsuarioPorPagamento(paymentRequest);

                    if(string.IsNullOrEmpty(user))
                    {
                        _logger.LogInformation("Pagamento não encontrado para o usuario {UserId}", paymentRequest);
                        return NotFound("Pagamento não encontrado para o usuario");
                    }


                    var domain = new PaymentRequest { PaymentId = paymentRequest, UserId = Guid.Parse(user) };
                    var compraUser = await _paymentService.ObterCompras(domain);

                    if (compraUser == null)
                    {
                        _logger.LogInformation("Nenhum valor encontrado");
                        return NotFound("Nenhum valor encontrado");
                    }

                    var MaxEvent = await _paymentEventService.ObterUltimoEventoPorPagamento(domain.PaymentId);
                    var versao = MaxEvent == null ? 0 : MaxEvent.Version;
                    var stPagamento = PaymentEventMappingExtensions.TipoFinalPagamento(versao);
                    domain.StatusPayment = stPagamento;
                    await _paymentService.AtualizarStatusPagamento(domain);
                    var events = PaymentEventMappingExtensions.convertToDomain(compraUser, "ATUALIZACAO");
                    await _paymentEventService.Adicionar(compraUser, events);

                    _logger.LogInformation("AtualizarPagamento concluido");
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro em AtualizarPagamento");
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
                    var result = await _paymentService.ObterCompras(domain);
                    if (result == null)
                    {
                        _logger.LogInformation("Pagamento não encontrado {paymentId} {userId}", paymentId, userId);
                        return NotFound("Pagamento não encontrado");
                    }

                    _logger.LogInformation("ConsultarPagamento concluido{paymentId} {userId}", paymentId, userId);
                    var events = PaymentEventMappingExtensions.convertToDomain(result, "CONSULTA");
                    await _paymentEventService.Adicionar(result, events);


                    var response = new PaymentResponse()
                    {
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

        [HttpGet("PagamentoAnalise")]
        [ProducesResponseType(typeof(List<Payment>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ObterPagamentosPendentes()
        {
            try
            {
                string tpPagamento = "PENDING,ANALISE";
                _logger.LogInformation($"ObterPagamentosPendentes {tpPagamento} iniciado");
                var result = await _paymentService.ListarPagamentos(tpPagamento);

                if (result == null)
                {
                    var pay = new Payment { PaymentId = Guid.Empty, UserId = Guid.Empty, StatusPayment = "NOT_FOUND" };
                    var paymentEvent = PaymentEventMappingExtensions.convertToDomain(null, 0, "NOT_FOUND");
                    await _paymentEventService.Adicionar(pay, paymentEvent);
                    _logger.LogInformation("Nenhum pagamento pendente encontrado");
                    return NotFound("Nenhum pagamento pendente encontrado");

                }

                foreach (var item in result)
                {
                    var paymentEvent = PaymentEventMappingExtensions.convertToDomain(item, "CONSULTA");
                    await _paymentEventService.Adicionar(item, paymentEvent);
                }
                _logger.LogInformation("ObterPagamentosPendentes concluido");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro em ObterPagamentosPendentes");
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        [Route("ListarPagamentoUsuario/{userId:guid}")]
        [ProducesResponseType(typeof(List<Payment>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ListarPagamentoUsuario([FromRoute] Guid userId)
        {
            using (_logger.BeginPaymentScope(nameof(PaymentController), nameof(ListarPagamentoUsuario), null, userId))
                try
                {
                    _logger.LogInformation("ListarPagamentoUsuario iniciado");
                    var result = await _paymentService.ObterComprasPorUsuario(userId);
                    if (result == null || result.Count == 0)
                    {
                        _logger.LogInformation("Usuario não encontrado");
                        var purchased = new Payment { PaymentId = Guid.Empty, UserId = userId, StatusPayment = "PURCHASE_NOT_FOUND" };
                        var paymentEvent = PaymentEventMappingExtensions.convertToDomain(purchased, 0, "PURCHASE_NOT_FOUND");
                        await _paymentEventService.Adicionar(purchased, paymentEvent);
                        return NotFound("Usuario não encontrado");
                    }
                    foreach (var item in result)
                    {
                        var events = PaymentEventMappingExtensions.convertToDomain(item, "CONSULTA");
                        await _paymentEventService.Adicionar(item, events);
                    }

                    _logger.LogInformation("ListarPagamentoUsuario concluido");
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro em ListarPagamentoUsuario");
                    return BadRequest(ex.Message);
                }
        }
    }
}
