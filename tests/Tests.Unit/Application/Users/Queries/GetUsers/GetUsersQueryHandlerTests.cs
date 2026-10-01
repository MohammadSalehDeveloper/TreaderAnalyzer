using AutoMapper;
using Core.Application.Users.Queries.GetUsers;
using Core.Contracts.DTOs.Users;
using Core.Contracts.Interfaces.Persistence;
using Core.Domain.Entities;
using Core.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Tests.Unit.Application.Users.Queries.GetUsers;

public class GetUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly GetUsersQueryHandler _sut;

    public GetUsersQueryHandlerTests()
    {
        _sut = new GetUsersQueryHandler(_users.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_WhenUsersMatchFilters_ReturnsMappedDtos()
    {
        var trader = User.CreateTrader("ada@example.com", "ada", "hash", "Ada", "Lovelace", "USD");
        var dto = CreateDto(trader.Id);
        _users.Setup(repository => repository.ListAsync(UserRole.Trader, UserStatus.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { trader });
        _mapper.Setup(mapper => mapper.Map<IReadOnlyList<UserDto>>(It.IsAny<object>()))
            .Returns(new List<UserDto> { dto });

        var result = await _sut.Handle(new GetUsersQuery(UserRole.Trader, UserStatus.Pending), CancellationToken.None);

        result.Should().ContainSingle().Which.Should().BeSameAs(dto);
        _users.Verify(
            repository => repository.ListAsync(UserRole.Trader, UserStatus.Pending, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsNone_ReturnsEmptyList()
    {
        _users.Setup(repository => repository.ListAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());
        _mapper.Setup(mapper => mapper.Map<IReadOnlyList<UserDto>>(It.IsAny<object>()))
            .Returns(new List<UserDto>());

        var result = await _sut.Handle(new GetUsersQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    private static UserDto CreateDto(Guid id) =>
        new(
            id,
            "ada@example.com",
            "ada",
            UserRole.Trader,
            UserStatus.Pending,
            "Ada",
            "Lovelace",
            "Ada Lovelace",
            null,
            null,
            "Ada Lovelace",
            "USD",
            DateTime.UtcNow,
            null,
            null,
            null);
}
