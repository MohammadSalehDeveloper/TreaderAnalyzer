using Core.Application.Common.Exceptions;
using Core.Contracts.Interfaces.Persistence;
using MediatR;

namespace Core.Application.Users.Commands.SoftDeleteUser;

public sealed class SoftDeleteUserCommandHandler : IRequestHandler<SoftDeleteUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public SoftDeleteUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SoftDeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);

        user.SoftDelete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
