using MediatR;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Projects.Queries.GetProjects;

/// <summary>
/// Query to retrieve a paginated list of projects.
/// </summary>
public record GetProjectsQuery(
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<ProjectDto>>;
