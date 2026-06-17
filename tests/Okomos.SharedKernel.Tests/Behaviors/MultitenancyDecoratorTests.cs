using FluentAssertions;
using Okomos.SharedKernel.Abstractions.Multitenancy;
using Okomos.SharedKernel.Behaviors.Multitenancy;
using Okomos.SharedKernel.Tests.TestHelpers;
using NSubstitute;

namespace Okomos.SharedKernel.Tests.Behaviors;

public class MultitenancyDecoratorTests
{
    [Fact]
    public async Task CommandDecorator_Should_Throw_When_Tenant_Is_Missing()
    {
        var inner = new TestCommandHandler();
        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.CurrentTenantId.Returns((Guid?)null);

        var decorator = new MultitenancyCommandDecorator<TestCommand, string>(inner, tenantProvider);

        var act = () => decorator.HandleAsync(new TestCommand("x"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Tenant context is required*");
    }

    [Fact]
    public async Task CommandDecorator_Should_Call_Inner_Handler_When_Tenant_Exists()
    {
        var inner = new TestCommandHandler();
        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.CurrentTenantId.Returns(Guid.NewGuid());

        var decorator = new MultitenancyCommandDecorator<TestCommand, string>(inner, tenantProvider);

        var result = await decorator.HandleAsync(new TestCommand("tenant"));

        result.Should().Be("handled:tenant");
    }

    [Fact]
    public async Task QueryDecorator_Should_Throw_When_Tenant_Is_Missing()
    {
        var inner = new TestQueryHandler();
        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.CurrentTenantId.Returns((Guid?)null);

        var decorator = new MultitenancyQueryDecorator<TestQuery, string?>(inner, tenantProvider);

        var act = () => decorator.HandleAsync(new TestQuery(Guid.NewGuid()));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
