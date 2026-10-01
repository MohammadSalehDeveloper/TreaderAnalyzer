using Core.Application.Common.Exceptions;
using Core.Application.Users.Commands.ActivateUser;
using Core.Contracts.Interfaces.Persistence;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace Tests.Unit.Application.Users.Commands.ActivateUser;

public class ActivateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ActivateUserCommandHandler _sut;

    public ActivateUserCommandHandlerTests()
    {
        _unitOfWork.SetupGet(unitOfWork => unitOfWork.Users).Returns(_users.Object);
        _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new ActivateUserCommandHandler(_unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsPending_ActivatesAndSaves()
    {
        var user = User.CreateTrader("ada@example.com", "ada", "hash", "Ada", "Lovelace", "USD");
        _users.Setup(repository => repository.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _sut.Handle(new ActivateUserCommand(user.Id), CancellationToken.None);

        user.Status.Should().Be(UserStatus.Active);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsMissing_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        _users.Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _sut.Handle(new ActivateUserCommand(userId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsClosed_ThrowsDomainException()
    {
        var user = User.CreateAdmin("ann@example.com", "ann", "hash", "Ann", "Admin");
        user.Close();
        _users.Setup(repository => repository.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = async () => await _sut.Handle(new ActivateUserCommand(user.Id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Closed*");
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
