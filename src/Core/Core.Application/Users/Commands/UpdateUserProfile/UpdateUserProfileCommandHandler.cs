using AutoMapper;
using Core.Application.Common.Exceptions;
using Core.Contracts.DTOs.Users;
using Core.Contracts.Interfaces.Persistence;
using MediatR;

namespace Core.Application.Users.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateUserProfileCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdWithBalanceAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);

        user.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.DisplayName,
            request.TimeZoneId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
