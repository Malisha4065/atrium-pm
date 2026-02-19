using AtriumPM.Billing.API.Application.DTOs;
using AtriumPM.Billing.API.Application.Interfaces;
using AtriumPM.Leasing.API.Application.Events;
using AtriumPM.Shared.Interfaces;
using MassTransit;

namespace AtriumPM.Billing.API.Infrastructure.Consumers;

public class LeaseSignedConsumer : IConsumer<LeaseSignedEvent>
{
    private readonly ITenantContext _tenantContext;
    private readonly IBillingService _billingService;

    public LeaseSignedConsumer(ITenantContext tenantContext, IBillingService billingService)
    {
        _tenantContext = tenantContext;
        _billingService = billingService;
    }

    public async Task Consume(ConsumeContext<LeaseSignedEvent> context)
    {
        _tenantContext.TenantId = context.Message.TenantId;

        var dueDate = context.Message.StartDate.AddMonths(1);

        await _billingService.CreateInvoiceAsync(new CreateInvoiceRequest(
            context.Message.LeaseId,
            context.Message.UnitId,
            1000m,
            dueDate));
    }
}
