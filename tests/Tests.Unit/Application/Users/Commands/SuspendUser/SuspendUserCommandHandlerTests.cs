using Core.Application.Common.Exceptions;
using Core.Application.Users.Commands.SuspendUser;
using Core.Contracts.Interfaces.Persistence;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace Tests.Unit.Application.Users.Commands.SuspendUser;

public class SuspendUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly SuspendUserCommandHandler _sut;

    public SuspendUserCommandHandlerTests()
    {
        _unitOfWork.SetupGet(unitOfWork => unitOfWork.Users).Returns(_users.Object);
        _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new SuspendUserCommandHandler(_unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsActive_SuspendsAndSaves()
    {
        var user = User.CreateAdmin("ann@example.com", "ann", "hash", "Ann", "Admin");
        _users.Setup(repository => repository.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _sut.Handle(new SuspendUserCommand(user.Id), CancellationToken.None);

        user.Status.Should().Be(UserStatus.Suspended);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsMissing_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        _users.Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _sut.Handle(new SuspendUserCommand(userId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsPending_ThrowsDomainException()
    {
        var user = User.CreateTrader("ada@example.com", "ada", "hash", "Ada", "Lovelace", "USD");
        _users.Setup(repository => repository.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = async () => await _sut.Handle(new SuspendUserCommand(user.Id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*active*");
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
