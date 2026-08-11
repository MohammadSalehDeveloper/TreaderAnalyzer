using Core.Application.Common.Exceptions;
using Core.Contracts.Interfaces.Persistence;
using Core.Contracts.Interfaces.Security;
using MediatR;

namespace Core.Application.Users.Commands.ChangeUserPassword;

public sealed class ChangeUserPasswordCommandHandler : IRequestHandler<ChangeUserPasswordCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public ChangeUserPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);

        user.ChangePassword(_passwordHasher.Hash(request.NewPassword));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
