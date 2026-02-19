using AtriumPM.Billing.API.Application.Interfaces;
using Quartz;

namespace AtriumPM.Billing.API.Infrastructure.Jobs;

public class NightlyLateFeeJob : IJob
{
    private readonly ILateFeeService _lateFeeService;

    public NightlyLateFeeJob(ILateFeeService lateFeeService)
    {
        _lateFeeService = lateFeeService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _lateFeeService.ApplyLateFeesAsync(context.CancellationToken);
    }
}
