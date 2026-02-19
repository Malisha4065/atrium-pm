using AtriumPM.Billing.API.Application.DTOs;
using AtriumPM.Billing.API.Application.Interfaces;
using AtriumPM.Billing.API.Domain.Entities;
using AtriumPM.Billing.API.Domain.Enums;
using AtriumPM.Billing.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AtriumPM.Billing.API.Application.Services;

public class BillingService : IBillingService
{
    private readonly BillingDbContext _db;

    public BillingService(BillingDbContext db)
    {
        _db = db;
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request)
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            LeaseId = request.LeaseId,
            UnitId = request.UnitId,
            BaseAmount = request.BaseAmount,
            LateFeeAmount = 0m,
            PaidAmount = 0m,
            DueDate = request.DueDate,
            Status = InvoiceStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        return MapInvoice(invoice);
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync()
    {
        return await _db.Invoices
            .AsNoTracking()
            .OrderByDescending(i => i.DueDate)
            .Select(i => new InvoiceDto(
                i.Id,
                i.TenantId,
                i.LeaseId,
                i.UnitId,
                i.BaseAmount,
                i.LateFeeAmount,
                i.PaidAmount,
                i.DueDate,
                i.Status.ToString(),
                i.CreatedAt))
            .ToListAsync();
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(Guid id)
    {
        var invoice = await _db.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        return invoice is null ? null : MapInvoice(invoice);
    }

    public async Task<PaymentDto> RecordPaymentAsync(CreatePaymentRequest request)
    {
        if (!Enum.TryParse<PaymentMethod>(request.Method, true, out var method))
        {
            throw new InvalidOperationException("Invalid payment method.");
        }

        var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == request.InvoiceId)
            ?? throw new InvalidOperationException("Invoice not found.");

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            TenantId = invoice.TenantId,
            Amount = request.Amount,
            Method = method,
            PaidAt = request.PaidAt,
            CreatedAt = DateTime.UtcNow
        };

        _db.Payments.Add(payment);

        invoice.PaidAmount += payment.Amount;
        var totalDue = invoice.BaseAmount + invoice.LateFeeAmount;
        invoice.Status = invoice.PaidAmount >= totalDue ? InvoiceStatus.Paid : InvoiceStatus.Open;

        await _db.SaveChangesAsync();

        return new PaymentDto(
            payment.Id,
            payment.InvoiceId,
            payment.TenantId,
            payment.Amount,
            payment.Method.ToString(),
            payment.PaidAt,
            payment.CreatedAt);
    }

    private static InvoiceDto MapInvoice(Invoice invoice)
    {
        return new InvoiceDto(
            invoice.Id,
            invoice.TenantId,
            invoice.LeaseId,
            invoice.UnitId,
            invoice.BaseAmount,
            invoice.LateFeeAmount,
            invoice.PaidAmount,
            invoice.DueDate,
            invoice.Status.ToString(),
            invoice.CreatedAt);
    }
}
