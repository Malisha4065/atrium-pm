using AtriumPM.Billing.API.Application.DTOs;
using AtriumPM.Billing.API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtriumPM.Billing.API.Controllers;

[ApiController]
[Authorize]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IBillingService _billingService;

    public InvoicesController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvoiceDto>>> GetAll()
    {
        return Ok(await _billingService.GetInvoicesAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceDto>> GetById(Guid id)
    {
        var result = await _billingService.GetInvoiceByIdAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> Create([FromBody] CreateInvoiceRequest request)
    {
        var result = await _billingService.CreateInvoiceAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
