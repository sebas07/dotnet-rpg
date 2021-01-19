using System.Threading.Tasks;
using dotnet_rpg.Dtos.Fight;
using dotnet_rpg.Services.FightService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class FightController : ControllerBase
    {

        private readonly IFightService _fightService;

        public FightController(IFightService fightService)
        {
            this._fightService = fightService;
        }

        [HttpPost("Weapon")]
        public async Task<ActionResult> WeaponAttack(WeaponAttackDto request)
        {
            return Ok(await this._fightService.WeaponAttack(request));
        }

        [HttpPost("Skill")]
        public async Task<ActionResult> SkillAttack(SkillAttackDto request)
        {
            return Ok(await this._fightService.SkillAttack(request));
        }

        [HttpPost]
        public async Task<ActionResult> Fight(FightRequestDto request)
        {
            return Ok(await this._fightService.Fight(request));
        }

        [HttpGet]
        public async Task<ActionResult> GetHighScore()
        {
            return Ok(await this._fightService.GetHighScore());
        }

    }
}