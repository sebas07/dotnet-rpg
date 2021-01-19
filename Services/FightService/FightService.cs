using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Fight;
using dotnet_rpg.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.FightService
{
    public class FightService : IFightService
    {

        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public FightService(DataContext dataContext, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._dataContext = dataContext;
            this._mapper = mapper;
        }

        public async Task<ServiceResponse<FightResultDto>> Fight(FightRequestDto request)
        {
            ServiceResponse<FightResultDto> response = new ServiceResponse<FightResultDto>
            {
                Data = new FightResultDto()
            };
            try
            {
                List<Character> characters = await this._dataContext.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.CharacterSkills).ThenInclude(cs => cs.Skill)
                    .Where(c => request.CharacterIds.Contains(c.Id)).ToListAsync();
                bool defeated = false;
                while (!defeated)
                {
                    foreach (Character attacker in characters)
                    {
                        List<Character> opponents = characters.Where(t => t.Id != attacker.Id).ToList();
                        Character opponent = opponents[new Random().Next(opponents.Count)];

                        int damage = 0;
                        string attackUsed = string.Empty;
                        bool useWeapon = new Random().Next(2) == 0;
                        if (useWeapon && attacker.Weapon != null)
                        {
                            attackUsed = attacker.Weapon.Name;
                            damage = FightService.ExecuteWeaponAttack(attacker, opponent);
                        }
                        else
                        {
                            if (attacker.CharacterSkills.Count > 0)
                            {
                                int randomSkill = new Random().Next(attacker.CharacterSkills.Count);
                                attackUsed = attacker.CharacterSkills[randomSkill].Skill.Name;
                                damage = FightService.ExecuteSkillAttack(attacker, opponent, attacker.CharacterSkills[randomSkill]);
                            }
                        }
                        response.Data.FightLog.Add($"{ attacker.Name } attacked { opponent.Name } using { attackUsed } with { (damage >= 0 ? damage : 0) } damage.");
                        if (opponent.HitPoints <= 0)
                        {
                            defeated = true;
                            attacker.Victories++;
                            opponent.Defeats++;
                            response.Data.FightLog.Add($"{ opponent.Name } has been defeated.");
                            response.Data.FightLog.Add($"{ attacker.Name } wins with { attacker.HitPoints } HP left.");
                            break;
                        }
                    }
                }
                characters.ForEach(c => {
                    c.Fights++;
                    c.HitPoints = 100;
                });

                this._dataContext.Characters.UpdateRange(characters);
                await this._dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }

        public async Task<ServiceResponse<AttackResultDto>> SkillAttack(SkillAttackDto request)
        {
            ServiceResponse<AttackResultDto> response = new ServiceResponse<AttackResultDto>();
            try
            {
                Character attacker = await this._dataContext.Characters
                    .Include(c => c.CharacterSkills).ThenInclude(cs => cs.Skill)
                    .FirstOrDefaultAsync(c => c.Id == request.AttackerId && c.User.Id == this.GetUserId());
                Character opponent = await this._dataContext.Characters
                    .FirstOrDefaultAsync(c => c.Id == request.OpponentId);
                if (attacker == null || opponent == null)
                {
                    response.Success = false;
                    response.Message = "Character not found.";
                    return response;
                }
                CharacterSkill attackSkill = attacker.CharacterSkills
                    .FirstOrDefault(cs => cs.Skill.Id == request.SkillId);
                if (attackSkill == null)
                {
                    response.Success = false;
                    response.Message = $"{ attacker.Name } doesn't know that skill.";
                    return response;
                }
                int damage = FightService.ExecuteSkillAttack(attacker, opponent, attackSkill);
                if (opponent.HitPoints <= 0)
                    response.Message = $"{ opponent.Name } has been defeated.";

                this._dataContext.Characters.Update(opponent);
                await this._dataContext.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    Attacker = attacker.Name,
                    AttackerHitPoint = attacker.HitPoints,
                    Opponent = opponent.Name,
                    OpponentHitPoints = opponent.HitPoints,
                    Damage = damage
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        private static int ExecuteSkillAttack(Character attacker, Character opponent, CharacterSkill attackSkill)
        {
            int damage = attackSkill.Skill.Damage + (new Random().Next(attacker.Intelligence));
            damage -= new Random().Next(opponent.Defense);
            if (damage > 0)
                opponent.HitPoints -= damage;
            return damage;
        }

        public async Task<ServiceResponse<AttackResultDto>> WeaponAttack(WeaponAttackDto request)
        {
            ServiceResponse<AttackResultDto> response = new ServiceResponse<AttackResultDto>();
            try
            {
                Character attacker = await this._dataContext.Characters.Include(c => c.Weapon)
                    .FirstOrDefaultAsync(c => c.Id == request.AttackerId && c.User.Id == this.GetUserId());
                Character opponent = await this._dataContext.Characters
                    .FirstOrDefaultAsync(c => c.Id == request.OpponentId);
                if (attacker == null || opponent == null)
                {
                    response.Success = false;
                    response.Message = "Character not found.";
                    return response;
                }
                int damage = FightService.ExecuteWeaponAttack(attacker, opponent);
                if (opponent.HitPoints <= 0)
                    response.Message = $"{ opponent.Name } has been defeated.";

                this._dataContext.Characters.Update(opponent);
                await this._dataContext.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    Attacker = attacker.Name,
                    AttackerHitPoint = attacker.HitPoints,
                    Opponent = opponent.Name,
                    OpponentHitPoints = opponent.HitPoints,
                    Damage = damage
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        private static int ExecuteWeaponAttack(Character attacker, Character opponent)
        {
            int damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));
            damage -= new Random().Next(opponent.Defense);
            if (damage > 0)
                opponent.HitPoints -= damage;
            return damage;
        }

        private int GetUserId() => int.Parse(this._httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<List<HighScoreDto>>> GetHighScore()
        {
            List<Character> characters = await this._dataContext.Characters
                .Where(c => c.Fights > 0)
                .OrderByDescending(c => c.Victories).ThenBy(c => c.Defeats)
                .ToListAsync();
            
            ServiceResponse<List<HighScoreDto>> response = new ServiceResponse<List<HighScoreDto>>
            {
                Data = characters.Select(c => this._mapper.Map<HighScoreDto>(c)).ToList()
            };

            return response;
        }
    }
}