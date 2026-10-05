using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Application.Authorization;

public static class AuthorizationPolicies
{
    public const string RequireAdmin = "RequireAdmin";
    public const string ManageCourses = "ManageCourses";
    public const string ManageContent = "ManageContent";
    public const string ManageStudents = "ManageStudents";
    public const string ManagePayments = "ManagePayments";
    public const string ManageActivations = "ManageActivations";
    public const string ManageConsultations = "ManageConsultations";
    public const string ManageUsers = "ManageUsers";
    public const string ViewAuditLogs = "ViewAuditLogs";
    public const string AccessAdminPanel = "AccessAdminPanel";
    public const string ManageBusinessSettings = "ManageBusinessSettings";

    public static IReadOnlyDictionary<string, string[]> RoleMap { get; } =
        new Dictionary<string, string[]>
        {
            [RequireAdmin] = [RoleNames.Admin],
            [ManageCourses] = [RoleNames.Admin, RoleNames.ContentManager],
            [ManageContent] = [RoleNames.Admin, RoleNames.ContentManager],
            [ManageStudents] = [RoleNames.Admin, RoleNames.Support],
            [ManagePayments] = [RoleNames.Admin, RoleNames.Support],
            [ManageActivations] = [RoleNames.Admin, RoleNames.Support],
            [ManageConsultations] = [RoleNames.Admin, RoleNames.Support],
            [ManageUsers] = [RoleNames.Admin],
            [ViewAuditLogs] = [RoleNames.Admin],
            [AccessAdminPanel] = [RoleNames.Admin, RoleNames.Support, RoleNames.ContentManager],
            [ManageBusinessSettings] = [RoleNames.Admin]
        };
}
