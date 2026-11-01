using System.Reflection;
using Xunit;

namespace InvoiceApp.Tests;

public class DomainArchitectureTests
{
    private static readonly string[] ForbiddenAssemblies =
    [
        "System.Net.Http",
        "Microsoft.AspNetCore",
        "EntityFramework",
        "Microsoft.EntityFrameworkCore",
        "SqlClient",
        "Azure.Core",
        "Azure.Identity",
        "Azure.Messaging.ServiceBus",
        "Azure.Storage",
        "Azure.Security.KeyVault"
    ];

    [Fact]
    public void DomainAssembly_MustNotReferenceForbiddenInfrastructureAssemblies()
    {
        var domainAssembly = typeof(InvoiceApp.Domain.Invoice).Assembly;
        var referenced = domainAssembly.GetReferencedAssemblies();

        var forbidden = referenced
            .Where(a => ForbiddenAssemblies.Any(f => a.Name != null && a.Name.StartsWith(f, StringComparison.OrdinalIgnoreCase)))
            .Select(a => a.FullName)
            .ToList();

        Assert.True(forbidden.Count == 0,
            $"Domain assembly references forbidden infrastructure assemblies: {string.Join(", ", forbidden)}");
    }

    [Fact]
    public void DomainTypes_MustNotUseForbiddenInfrastructureTypesInMembers()
    {
        var domainAssembly = typeof(InvoiceApp.Domain.Invoice).Assembly;

        var violatingTypes = domainAssembly
            .GetTypes()
            .Where(type => !type.IsGenericParameter)
            .Where(type => TypeUsesForbiddenInfrastructure(type))
            .ToList();

        Assert.True(violatingTypes.Count == 0,
            $"Domain types use forbidden infrastructure types: {string.Join(", ", violatingTypes.Select(t => t.FullName))}");
    }

    private static bool TypeUsesForbiddenInfrastructure(Type type)
    {
        if (UsesForbiddenType(type))
        {
            return true;
        }

        foreach (var member in type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
        {
            switch (member)
            {
                case FieldInfo field when UsesForbiddenType(field.FieldType):
                    return true;
                case PropertyInfo property when UsesForbiddenType(property.PropertyType):
                    return true;
                case ConstructorInfo ctor when ctor.GetParameters().Any(parameter => UsesForbiddenType(parameter.ParameterType)):
                    return true;
                case MethodInfo method when method.GetParameters().Any(parameter => UsesForbiddenType(parameter.ParameterType)) || UsesForbiddenType(method.ReturnType):
                    return true;
            }
        }

        return false;
    }

    private static bool UsesForbiddenType(Type type)
    {
        if (type == null)
        {
            return false;
        }

        if (type == typeof(System.Net.Http.HttpClient) || type == typeof(System.Net.Http.HttpRequestMessage) || type == typeof(System.Net.Http.HttpResponseMessage))
        {
            return true;
        }

        if (type.FullName != null && (
            type.FullName.StartsWith("Microsoft.AspNetCore.", StringComparison.OrdinalIgnoreCase) ||
            type.FullName.StartsWith("Microsoft.EntityFrameworkCore.", StringComparison.OrdinalIgnoreCase) ||
            type.FullName.StartsWith("System.Data.SqlClient", StringComparison.OrdinalIgnoreCase) ||
            type.FullName.StartsWith("Azure.", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        var ns = type.Namespace ?? string.Empty;
        if (ns.StartsWith("Microsoft.AspNetCore", StringComparison.OrdinalIgnoreCase) ||
            ns.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase) ||
            ns.StartsWith("System.Data.SqlClient", StringComparison.OrdinalIgnoreCase) ||
            ns.StartsWith("Azure", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}
