using AutoMapper;
using Core.Application.Common.Exceptions;
using Core.Application.Users.Commands.CreateAdmin;
using Core.Contracts.DTOs.Users;
using Core.Contracts.Interfaces.Persistence;
using Core.Contracts.Interfaces.Security;
using Core.Domain.Entities;
using Core.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Tests.Unit.Application.Users.Commands.CreateAdmin;

public class CreateAdminCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly CreateAdminCommandHandler _sut;

    public CreateAdminCommandHandlerTests()
    {
        _unitOfWork.SetupGet(unitOfWork => unitOfWork.Users).Returns(_users.Object);
        _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _passwordHasher.Setup(hasher => hasher.Hash(It.IsAny<string>())).Returns("hashed");
        _sut = new CreateAdminCommandHandler(_unitOfWork.Object, _passwordHasher.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_WhenEmailAndUserNameAreFree_CreatesActiveAdmin()
    {
        User? saved = null;
        var dto = new UserDto(
            Guid.NewGuid(),
            "ann@example.com",
            "ann",
            UserRole.Admin,
            UserStatus.Active,
            "Ann",
            "Admin",
            "Ann Admin",
            null,
            null,
            null,
            null,
            DateTime.UtcNow,
            null,
            null,
            null);

        _users.Setup(repository => repository.EmailExistsAsync("ann@example.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _users.Setup(repository => repository.UserNameExistsAsync("ann", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _users.Setup(repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => saved = user)
            .Returns(Task.CompletedTask);
        _mapper.Setup(mapper => mapper.Map<UserDto>(It.IsAny<object>())).Returns(dto);

        var result = await _sut.Handle(
            new CreateAdminCommand("ann@example.com", "ann", "Secret123", "Ann", "Admin"),
            CancellationToken.None);

        result.Should().BeSameAs(dto);
        saved.Should().NotBeNull();
        saved!.Role.Should().Be(UserRole.Admin);
        saved.Status.Should().Be(UserStatus.Active);
        saved.PasswordHash.Should().Be("hashed");
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailExists_ThrowsConflictException()
    {
        _users.Setup(repository => repository.EmailExistsAsync("ann@example.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _sut.Handle(
            new CreateAdminCommand("ann@example.com", "ann", "Secret123", "Ann", "Admin"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        _users.Verify(repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNameExists_ThrowsConflictException()
    {
        _users.Setup(repository => repository.EmailExistsAsync("ann@example.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _users.Setup(repository => repository.UserNameExistsAsync("ann", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _sut.Handle(
            new CreateAdminCommand("ann@example.com", "ann", "Secret123", "Ann", "Admin"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        _users.Verify(repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
