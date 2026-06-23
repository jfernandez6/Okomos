using FastEndpoints;

namespace Billing.Api;

public sealed class ModuleRoutes : Group
{
    public ModuleRoutes()
    {
        Configure("billing", _ => { });
    }
}
