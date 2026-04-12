using Mapster;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Common.Mappings;

/// <summary>
/// Configures Mapster type adapter mappings for the application.
/// </summary>
public static class MappingConfig
{
    /// <summary>
    /// Registers all entity-to-DTO mappings with Mapster.
    /// </summary>
    public static void RegisterMappings()
    {
        TypeAdapterConfig<Project, ProjectDto>.NewConfig()
            .Map(dest => dest.OwnerName, src => src.Owner != null
                ? src.Owner.FirstName + " " + src.Owner.LastName
                : string.Empty)
            .Map(dest => dest.TaskCount, src => src.Tasks != null ? src.Tasks.Count : 0);

        TypeAdapterConfig<TaskItem, TaskItemDto>.NewConfig()
            .Map(dest => dest.AssigneeName, src => src.Assignee != null
                ? src.Assignee.FirstName + " " + src.Assignee.LastName
                : null)
            .Map(dest => dest.CommentCount, src => src.Comments != null ? src.Comments.Count : 0)
            .Map(dest => dest.Tags, src => src.Tags != null
                ? src.Tags.Select(t => t.Name).ToList()
                : new List<string>());

        TypeAdapterConfig<Comment, CommentDto>.NewConfig()
            .Map(dest => dest.AuthorName, src => src.Author != null
                ? src.Author.FirstName + " " + src.Author.LastName
                : string.Empty);

        TypeAdapterConfig<User, UserDto>.NewConfig();
    }
}
