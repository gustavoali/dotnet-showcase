using FluentAssertions;
using Moq;
using TaskManager.Application.Common.Mappings;
using TaskManager.Application.Features.Projects.Queries.GetProjects;
using TaskManager.Application.Tests.Helpers;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Tests.Features.Projects.Queries.GetProjects;

public class GetProjectsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Project>> _projectRepoMock;
    private readonly GetProjectsQueryHandler _handler;

    public GetProjectsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _projectRepoMock = new Mock<IRepository<Project>>();
        _unitOfWorkMock.Setup(u => u.Projects).Returns(_projectRepoMock.Object);

        MappingConfig.RegisterMappings();

        _handler = new GetProjectsQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnPagedResult()
    {
        // Arrange
        var owner = new User { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" };
        var projects = Enumerable.Range(1, 5).Select(i => new Project
        {
            Id = Guid.NewGuid(),
            Name = $"Project {i}",
            OwnerId = owner.Id,
            Owner = owner,
            Tasks = new List<TaskItem>(),
            CreatedAt = DateTime.UtcNow.AddDays(-i)
        }).ToList();

        var queryable = projects.AsAsyncQueryable();
        _projectRepoMock.Setup(r => r.Query()).Returns(queryable);

        var query = new GetProjectsQuery(PageNumber: 1, PageSize: 3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }
}
