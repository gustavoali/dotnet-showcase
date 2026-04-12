using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Projects.Commands.CreateProject;

/// <summary>
/// Handles the creation of a new project.
/// </summary>
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    /// <inheritdoc/>
    public Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        // TODO: Implement project creation logic
        throw new NotImplementedException();
    }
}
