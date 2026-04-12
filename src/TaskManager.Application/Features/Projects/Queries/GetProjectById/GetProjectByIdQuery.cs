using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Projects.Queries.GetProjectById;

/// <summary>
/// Query to retrieve a single project by its identifier.
/// </summary>
public record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDto>;
