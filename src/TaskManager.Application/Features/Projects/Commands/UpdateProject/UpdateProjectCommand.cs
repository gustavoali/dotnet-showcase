using MediatR;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Features.Projects.Commands.UpdateProject;

/// <summary>
/// Command to update an existing project.
/// </summary>
public record UpdateProjectCommand(
    Guid Id,
    string Name,
    string Description,
    ProjectStatus Status) : IRequest<ProjectDto>;
