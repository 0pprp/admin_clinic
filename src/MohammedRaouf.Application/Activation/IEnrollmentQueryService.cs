using MohammedRaouf.Contracts.Activation;

namespace MohammedRaouf.Application.Activation;

public interface IEnrollmentQueryService
{
    Task<IReadOnlyList<StudentEnrollmentResponse>> ListMineAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
