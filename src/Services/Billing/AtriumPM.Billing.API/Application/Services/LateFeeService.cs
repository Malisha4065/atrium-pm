using AtriumPM.Billing.API.Application.Interfaces;
using AtriumPM.Billing.API.Domain.Entities;
using AtriumPM.Billing.API.Domain.Enums;
using AtriumPM.Billing.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AtriumPM.Billing.API.Application.Services;

public class LateFeeService : ILateFeeService
{
    private const decimal LateFeeAmount = 25m;
    private readonly BillingDbContext _db;
    private readonly ILogger<LateFeeService> _logger;

    public LateFeeService(BillingDbContext db, ILogger<LateFeeService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<int> ApplyLateFeesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var invoices = await _db.Invoices
            .Where(i => i.Status != InvoiceStatus.Paid && i.DueDate < now)
            .ToListAsync(cancellationToken);

        var appliedCount = 0;

        foreach (var invoice in invoices)
        {
            var alreadyAppliedToday = await _db.LateFeeCharges
                .AnyAsync(
                    lf => lf.InvoiceId == invoice.Id && lf.AppliedAt.Date == now.Date,
                    cancellationToken);

            if (alreadyAppliedToday)
            {
                continue;
            }

            invoice.LateFeeAmount += LateFeeAmount;
            invoice.Status = InvoiceStatus.Overdue;

            _db.LateFeeCharges.Add(new LateFeeCharge
            {
                Id = Guid.NewGuid(),
                TenantId = invoice.TenantId,
                InvoiceId = invoice.Id,
                Amount = LateFeeAmount,
                Reason = "Nightly overdue late fee",
                AppliedAt = now
            });

            appliedCount++;
        }

        if (appliedCount > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Applied late fees to {Count} invoices.", appliedCount);
        return appliedCount;
    }
}
