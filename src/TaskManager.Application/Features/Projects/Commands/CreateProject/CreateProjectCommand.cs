using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Projects.Commands.CreateProject;

/// <summary>
/// Command to create a new project.
/// </summary>
public record CreateProjectCommand(
    string Name,
    string Description) : IRequest<ProjectDto>;
