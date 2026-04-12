using MediatR;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Projects.Queries.GetProjects;

/// <summary>
/// Handles retrieving a paginated list of projects.
/// </summary>
public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, PagedResult<ProjectDto>>
{
    /// <inheritdoc/>
    public Task<PagedResult<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        // TODO: Implement project retrieval logic
        throw new NotImplementedException();
    }
}
