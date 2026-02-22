namespace AtriumPM.Shared.Interfaces;

public interface ITenantConnectionStringResolver
{
    string ResolveConnectionString(string defaultConnectionString);
}
