namespace AtriumPM.Billing.API.Application.Interfaces;

public interface ILateFeeService
{
    Task<int> ApplyLateFeesAsync(CancellationToken cancellationToken = default);
}
