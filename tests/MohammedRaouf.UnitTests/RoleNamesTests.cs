using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.UnitTests;

public class RoleNamesTests
{
    [Fact]
    public void Role_names_include_the_four_platform_roles()
    {
        Assert.Equal(
            ["Admin", "ContentManager", "Support", "Student"],
            RoleNames.All);
    }
}
