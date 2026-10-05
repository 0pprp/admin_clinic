namespace MohammedRaouf.Domain.Identity;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string ContentManager = "ContentManager";
    public const string Support = "Support";
    public const string Student = "Student";

    public static readonly IReadOnlyList<string> All =
    [
        Admin,
        ContentManager,
        Support,
        Student
    ];
}
