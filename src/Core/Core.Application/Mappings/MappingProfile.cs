using AutoMapper;
using Core.Contracts.DTOs.Users;
using Core.Domain.Entities;

namespace Core.Application.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Balance, BalanceDto>();

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));
    }
}
