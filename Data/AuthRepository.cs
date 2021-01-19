using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using dotnet_rpg.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace dotnet_rpg.Data
{
    public class AuthRepository : IAuthRepository
    {

        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;

        public AuthRepository(DataContext dataContext, IConfiguration configuration)
        {
            this._configuration = configuration;
            this._dataContext = dataContext;
        }

        public async Task<ServiceResponse<string>> Login(string username, string password)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();

            User dbUser = await this._dataContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));
            if (dbUser == null)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "User not found.";
                return serviceResponse;
            }
            if (!this.VerifyPasswordHash(password, dbUser.PasswordHash, dbUser.PasswordSalt))
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Invalid password.";
                return serviceResponse;
            }

            serviceResponse.Data = this.CreateToken(dbUser);
            return serviceResponse;
        }

        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();

            if (await this.UserExists(user.Username))
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "User already exists.";
                return serviceResponse;
            }

            this.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await this._dataContext.Users.AddAsync(user);
            await this._dataContext.SaveChangesAsync();

            serviceResponse.Data = user.Id;
            return serviceResponse;
        }

        public async Task<bool> UserExists(string username)
        {
            if (await this._dataContext.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
                return true;
            return false;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computed = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computed.Length; i++)
                {
                    if (computed[i] != passwordHash[i])
                        return false;
                }
                return true;
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
                new Claim(ClaimTypes.Name, user.Username), 
                new Claim(ClaimTypes.Role, user.Role)
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(this._configuration.GetSection("AppSettings:Token").Value)
            );
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor 
            {
                Subject = new ClaimsIdentity(claims), 
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

    }
}