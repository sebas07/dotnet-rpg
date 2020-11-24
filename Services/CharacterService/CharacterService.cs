using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {

        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public CharacterService(IMapper mapper, DataContext context)
        {
            this._context = context;
            this._mapper = mapper;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> AddNewCharacter(AddCharacterDto newCharacter)
        {
            ServiceResponse<List<GetCharacterDto>> serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            Character dbCharacter = this._mapper.Map<Character>(newCharacter);
            await this._context.Characters.AddAsync(dbCharacter);
            await this._context.SaveChangesAsync();

            List<Character> dbCharacters = await this._context.Characters.ToListAsync();
            serviceResponse.Data = dbCharacters.Select(c => this._mapper.Map<GetCharacterDto>(c)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            ServiceResponse<List<GetCharacterDto>> serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            List<Character> dbCharacters = await this._context.Characters.ToListAsync();
            serviceResponse.Data = dbCharacters.Select(c => this._mapper.Map<GetCharacterDto>(c)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            ServiceResponse<GetCharacterDto> serviceResponse = new ServiceResponse<GetCharacterDto>();
            Character dbCharacter = await this._context.Characters.FirstOrDefaultAsync(c => c.Id == id);
            serviceResponse.Data = this._mapper.Map<GetCharacterDto>(dbCharacter);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            ServiceResponse<GetCharacterDto> serviceResponse = new ServiceResponse<GetCharacterDto>();
            try
            {
                Character dbCharacter = await this._context.Characters.FirstOrDefaultAsync(c => c.Id == updatedCharacter.Id);
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
                Character dbCharacter = await this._context.Characters.FirstAsync(c => c.Id == id);
                this._context.Characters.Remove(dbCharacter);
                await this._context.SaveChangesAsync();

                List<Character> dbCharacters = await this._context.Characters.ToListAsync();
                serviceResponse.Data = dbCharacters.Select(c => this._mapper.Map<GetCharacterDto>(c)).ToList();
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