using AutoMapper;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.User;
using dotnet_rpg.Models;

namespace dotnet_rpg
{
    public class AutoMapperProfile : Profile
    {

        public AutoMapperProfile()
        {
            // Character
            CreateMap<Character, GetCharacterDto>();
            CreateMap<AddCharacterDto, Character>();
            CreateMap<UpdateCharacterDto, Character>();
        }
        
    }
}