using System.Net.Http.Headers;
using System.Net.Http.Json;
using AtriumPM.Web.Models;
using Microsoft.Extensions.Options;

namespace AtriumPM.Web.Services;

public class AtriumApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiSessionState _session;
    private readonly ApiEndpointsOptions _options;

    public AtriumApiClient(
        IHttpClientFactory httpClientFactory,
        ApiSessionState session,
        IOptions<ApiEndpointsOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _session = session;
        _options = options.Value;
    }

    public async Task<TenantDto> RegisterTenantAsync(RegisterTenantRequest request)
    {
        return await PostAnonymousAsync<RegisterTenantRequest, TenantDto>(_options.IdentityBaseUrl, "/api/tenants/register", request);
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request)
    {
        return await PostAnonymousAsync<LoginRequest, TokenResponse>(_options.IdentityBaseUrl, "/api/auth/login", request);
    }

    public async Task<BuildingDto> CreateBuildingAsync(CreateBuildingRequest request)
    {
        return await PostAuthorizedAsync<CreateBuildingRequest, BuildingDto>(_options.PropertyBaseUrl, "/api/buildings", request);
    }

    public async Task<UnitDto> CreateUnitAsync(CreateUnitRequest request)
    {
        return await PostAuthorizedAsync<CreateUnitRequest, UnitDto>(_options.PropertyBaseUrl, "/api/units", request);
    }

    public async Task<LeaseDto> CreateLeaseAsync(CreateLeaseRequest request)
    {
        return await PostAuthorizedAsync<CreateLeaseRequest, LeaseDto>(_options.LeasingBaseUrl, "/api/leases", request);
    }

    public async Task<MaintenanceTicketDto> CreateTicketAsync(CreateMaintenanceTicketRequest request)
    {
        return await PostAuthorizedAsync<CreateMaintenanceTicketRequest, MaintenanceTicketDto>(_options.MaintenanceBaseUrl, "/api/tickets", request);
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request)
    {
        return await PostAuthorizedAsync<CreateInvoiceRequest, InvoiceDto>(_options.BillingBaseUrl, "/api/invoices", request);
    }

    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request)
    {
        return await PostAuthorizedAsync<CreatePaymentRequest, PaymentDto>(_options.BillingBaseUrl, "/api/payments", request);
    }

    public async Task<IReadOnlyList<BuildingDto>> GetBuildingsAsync()
    {
        return await GetAuthorizedAsync<IReadOnlyList<BuildingDto>>(_options.PropertyBaseUrl, "/api/buildings");
    }

    public async Task<BuildingDto> GetBuildingByIdAsync(Guid id)
    {
        return await GetAuthorizedAsync<BuildingDto>(_options.PropertyBaseUrl, $"/api/buildings/{id}");
    }

    public async Task<IReadOnlyList<UnitDto>> GetUnitsAsync()
    {
        return await GetAuthorizedAsync<IReadOnlyList<UnitDto>>(_options.PropertyBaseUrl, "/api/units");
    }

    public async Task<UnitDto> GetUnitByIdAsync(Guid id)
    {
        return await GetAuthorizedAsync<UnitDto>(_options.PropertyBaseUrl, $"/api/units/{id}");
    }

    public async Task<IReadOnlyList<UnitDto>> GetUnitsByBuildingAsync(Guid buildingId)
    {
        return await GetAuthorizedAsync<IReadOnlyList<UnitDto>>(_options.PropertyBaseUrl, $"/api/units/building/{buildingId}");
    }

    public async Task<IReadOnlyList<LeaseDto>> GetLeasesAsync()
    {
        return await GetAuthorizedAsync<IReadOnlyList<LeaseDto>>(_options.LeasingBaseUrl, "/api/leases");
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync()
    {
        return await GetAuthorizedAsync<IReadOnlyList<InvoiceDto>>(_options.BillingBaseUrl, "/api/invoices");
    }

    public async Task<IReadOnlyList<ServiceStatus>> GetServiceStatusesAsync()
    {
        var checks = new[]
        {
            ("Identity", _options.IdentityBaseUrl),
            ("Property", _options.PropertyBaseUrl),
            ("Leasing", _options.LeasingBaseUrl),
            ("Maintenance", _options.MaintenanceBaseUrl),
            ("Billing", _options.BillingBaseUrl)
        };

        var tasks = checks.Select(async check =>
        {
            var isReachable = await IsReachableAsync(check.Item2);
            return new ServiceStatus(check.Item1, check.Item2, isReachable);
        });

        return (await Task.WhenAll(tasks)).ToList();
    }

    private async Task<TResponse> PostAnonymousAsync<TRequest, TResponse>(string baseUrl, string path, TRequest request)
    {
        var client = _httpClientFactory.CreateClient();
        using var message = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}{path}")
        {
            Content = JsonContent.Create(request)
        };

        var response = await client.SendAsync(message);
        await EnsureSuccess(response);

        return (await response.Content.ReadFromJsonAsync<TResponse>())!;
    }

    private async Task<TResponse> PostAuthorizedAsync<TRequest, TResponse>(string baseUrl, string path, TRequest request)
    {
        EnsureAuthenticated();

        var client = _httpClientFactory.CreateClient();
        using var message = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}{path}")
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
        message.Headers.Add("X-Tenant-ID", _session.TenantId.ToString());

        var response = await client.SendAsync(message);
        await EnsureSuccess(response);

        return (await response.Content.ReadFromJsonAsync<TResponse>())!;
    }

    private async Task<TResponse> GetAuthorizedAsync<TResponse>(string baseUrl, string path)
    {
        EnsureAuthenticated();

        var client = _httpClientFactory.CreateClient();
        using var message = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}{path}");

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
        message.Headers.Add("X-Tenant-ID", _session.TenantId.ToString());

        var response = await client.SendAsync(message);
        await EnsureSuccess(response);

        return (await response.Content.ReadFromJsonAsync<TResponse>())!;
    }

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException($"API call failed ({(int)response.StatusCode}): {body}");
    }

    private void EnsureAuthenticated()
    {
        if (!_session.IsAuthenticated)
            throw new InvalidOperationException("Authenticate first: register/login to set tenant and token.");
    }

    private async Task<bool> IsReachableAsync(string baseUrl)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, baseUrl);
            using var response = await client.SendAsync(request);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public record ServiceStatus(string Name, string Url, bool IsReachable);

public class ApiEndpointsOptions
{
    public string IdentityBaseUrl { get; set; } = "http://localhost:5100";
    public string PropertyBaseUrl { get; set; } = "http://localhost:5200";
    public string LeasingBaseUrl { get; set; } = "http://localhost:5300";
    public string MaintenanceBaseUrl { get; set; } = "http://localhost:5400";
    public string BillingBaseUrl { get; set; } = "http://localhost:5500";
}
