namespace MohammedRaouf.Application.Common;

public static class Paging
{
    public static (int Page, int PageSize) Normalize(int page, int pageSize, int maxPageSize = 100)
    {
        var safePage = page < 1 ? 1 : page;
        var safeSize = pageSize < 1 ? 12 : Math.Min(pageSize, maxPageSize);
        return (safePage, safeSize);
    }
}
