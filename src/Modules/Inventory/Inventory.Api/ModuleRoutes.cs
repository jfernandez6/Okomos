using FastEndpoints;

namespace Inventory.Api;

public sealed class ModuleRoutes : Group
{
    public ModuleRoutes()
    {
        Configure("inventory", _ => { });
    }
}
