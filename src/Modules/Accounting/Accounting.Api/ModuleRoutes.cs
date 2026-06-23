using FastEndpoints;

namespace Accounting.Api;

public sealed class ModuleRoutes : Group
{
    public ModuleRoutes()
    {
        Configure("accounting", _ => { });
    }
}
