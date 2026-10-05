using MohammedRaouf.Domain;

namespace MohammedRaouf.UnitTests;

public class DomainAssemblyTests
{
    [Fact]
    public void Domain_assembly_loads()
    {
        var assemblyName = typeof(AssemblyMarker).Assembly.GetName().Name;

        Assert.Equal("MohammedRaouf.Domain", assemblyName);
    }
}
