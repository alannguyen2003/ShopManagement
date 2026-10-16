using System.Reflection;
using NetArchTest.Rules;

namespace ShopManagement.ArchitectureTests;

public class LayerDependencyTests
{
    private const string DomainNamespace = "ShopManagement.Domain";
    private const string ApplicationNamespace = "ShopManagement.Application";
    private const string InfrastructureNamespace = "ShopManagement.Infrastructure";
    private const string ApiNamespace = "ShopManagement.Api";

    private static readonly Assembly DomainAssembly = typeof(Domain.Common.Result).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;

    [Fact]
    public void Domain_HasNoProjectDependencies_DependsOnNothing()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, FailingTypes(result));
    }

    [Fact]
    public void Domain_HasNoFrameworkDependencies_DoesNotUseEfCoreOrAspNet()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore", "Microsoft.Extensions.DependencyInjection")
            .GetResult();

        Assert.True(result.IsSuccessful, FailingTypes(result));
    }

    [Fact]
    public void Application_DoesNotDependOnOuterLayers_NoInfrastructureOrApi()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace, "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, FailingTypes(result));
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnApi_NoApiReference()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, FailingTypes(result));
    }

    [Fact]
    public void Layers_AssembliesLoaded_NamespacesMatchLayerNames()
    {
        Assert.Equal(DomainNamespace, DomainAssembly.GetName().Name);
        Assert.Equal(ApplicationNamespace, ApplicationAssembly.GetName().Name);
        Assert.Equal(InfrastructureNamespace, InfrastructureAssembly.GetName().Name);
        Assert.Equal(ApiNamespace, typeof(Program).Assembly.GetName().Name);
    }

    private static string FailingTypes(TestResult result) =>
        "Violating types: " + string.Join(", ", result.FailingTypeNames ?? []);
}
