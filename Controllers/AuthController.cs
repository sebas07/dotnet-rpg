using System.Threading.Tasks;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.User;
using dotnet_rpg.Models;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            this._authRepository = authRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(UserRegisterDto request)
        {
            ServiceResponse<int> response = await this._authRepository.Register(
                new User { Username = request.Username }, request.Password
            );
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(UserLoginDto request)
        {
            ServiceResponse<string> response = await this._authRepository.Login(
                request.Username, request.Password
            );
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

    }
}