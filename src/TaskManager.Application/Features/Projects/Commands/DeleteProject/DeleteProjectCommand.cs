using MediatR;

namespace TaskManager.Application.Features.Projects.Commands.DeleteProject;

/// <summary>
/// Command to delete an existing project.
/// </summary>
public record DeleteProjectCommand(Guid Id) : IRequest<Unit>;
