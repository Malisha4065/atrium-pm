using AtriumPM.Billing.API.Application.DTOs;
using AtriumPM.Billing.API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtriumPM.Billing.API.Controllers;

[ApiController]
[Authorize]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IBillingService _billingService;

    public PaymentsController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Record([FromBody] CreatePaymentRequest request)
    {
        var result = await _billingService.RecordPaymentAsync(request);
        return Ok(result);
    }
}
