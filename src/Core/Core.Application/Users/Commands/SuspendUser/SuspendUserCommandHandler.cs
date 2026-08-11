using Core.Application.Common.Exceptions;
using Core.Contracts.Interfaces.Persistence;
using MediatR;

namespace Core.Application.Users.Commands.SuspendUser;

public sealed class SuspendUserCommandHandler : IRequestHandler<SuspendUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public SuspendUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SuspendUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);

        user.Suspend();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
