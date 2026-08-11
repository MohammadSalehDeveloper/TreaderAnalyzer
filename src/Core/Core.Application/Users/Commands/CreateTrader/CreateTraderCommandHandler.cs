using AutoMapper;
using Core.Application.Common.Exceptions;
using Core.Contracts.DTOs.Users;
using Core.Contracts.Interfaces.Persistence;
using Core.Contracts.Interfaces.Security;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Users.Commands.CreateTrader;

public sealed class CreateTraderCommandHandler : IRequestHandler<CreateTraderCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public CreateTraderCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(CreateTraderCommand request, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken: cancellationToken))
            throw new ConflictException($"Email '{request.Email}' is already registered.");

        if (await _unitOfWork.Users.UserNameExistsAsync(request.UserName, cancellationToken: cancellationToken))
            throw new ConflictException($"User name '{request.UserName}' is already taken.");

        var user = User.CreateTrader(
            request.Email,
            request.UserName,
            _passwordHasher.Hash(request.Password),
            request.FirstName,
            request.LastName,
            request.PreferredCurrency,
            request.PhoneNumber,
            request.DisplayName,
            request.TimeZoneId);

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
