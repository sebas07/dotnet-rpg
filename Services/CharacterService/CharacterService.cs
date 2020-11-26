using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {

        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public CharacterService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            this._context = context;
            this._httpContextAccessor = httpContextAccessor;
            this._mapper = mapper;
        }

        private int GetUserId() => int.Parse(this._httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<List<GetCharacterDto>>> AddNewCharacter(AddCharacterDto newCharacter)
        {
            ServiceResponse<List<GetCharacterDto>> serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            Character dbCharacter = this._mapper.Map<Character>(newCharacter);
            dbCharacter.User = await this._context.Users.FirstOrDefaultAsync(u => u.Id == this.GetUserId());
            await this._context.Characters.AddAsync(dbCharacter);
            await this._context.SaveChangesAsync();

            List<Character> dbCharacters = await this._context.Characters.Where(c => c.User.Id == this.GetUserId()).ToListAsync();
            serviceResponse.Data = dbCharacters.Select(c => this._mapper.Map<GetCharacterDto>(c)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            ServiceResponse<List<GetCharacterDto>> serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            List<Character> dbCharacters = await this._context.Characters.Where(c => c.User.Id == this.GetUserId()).ToListAsync();
            serviceResponse.Data = dbCharacters.Select(c => this._mapper.Map<GetCharacterDto>(c)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            ServiceResponse<GetCharacterDto> serviceResponse = new ServiceResponse<GetCharacterDto>();
            Character dbCharacter = await this._context.Characters
                .FirstOrDefaultAsync(c => c.Id == id && c.User.Id == this.GetUserId());
            serviceResponse.Data = this._mapper.Map<GetCharacterDto>(dbCharacter);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            ServiceResponse<GetCharacterDto> serviceResponse = new ServiceResponse<GetCharacterDto>();
            try
            {
                Character dbCharacter = await this._context.Characters.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == updatedCharacter.Id);
                if (dbCharacter.User.Id == this.GetUserId())
                {
                    dbCharacter.Name = updatedCharacter.Name;
                    dbCharacter.HitPoints = updatedCharacter.HitPoints;
                    dbCharacter.Strength = updatedCharacter.Strength;
                    dbCharacter.Defense = updatedCharacter.Defense;
                    dbCharacter.Intelligence = updatedCharacter.Intelligence;
                    dbCharacter.Class = updatedCharacter.Class;

                    this._context.Characters.Update(dbCharacter);
                    await this._context.SaveChangesAsync();

                    serviceResponse.Data = this._mapper.Map<GetCharacterDto>(dbCharacter);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Character not found.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
            ServiceResponse<List<GetCharacterDto>> serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            try
            {
                Character dbCharacter = await this._context.Characters
                    .FirstOrDefaultAsync(c => c.Id == id && c.User.Id == this.GetUserId());
                if (dbCharacter != null)
                {
                    this._context.Characters.Remove(dbCharacter);
                    await this._context.SaveChangesAsync();

                    List<Character> dbCharacters = await this._context.Characters
                        .Where(c => c.User.Id == this.GetUserId()).ToListAsync();
                    serviceResponse.Data = dbCharacters.Select(c => this._mapper.Map<GetCharacterDto>(c)).ToList();
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Character not found.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

    }
}