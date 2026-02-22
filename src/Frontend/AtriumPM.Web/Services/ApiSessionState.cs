namespace AtriumPM.Web.Services;

public class ApiSessionState
{
    public Guid TenantId { get; private set; }
    public string AccessToken { get; private set; } = string.Empty;
    public string RefreshToken { get; private set; } = string.Empty;

    public bool IsAuthenticated => TenantId != Guid.Empty && !string.IsNullOrWhiteSpace(AccessToken);

    public void SetSession(Guid tenantId, string accessToken, string refreshToken)
    {
        TenantId = tenantId;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public void Clear()
    {
        TenantId = Guid.Empty;
        AccessToken = string.Empty;
        RefreshToken = string.Empty;
    }
}
