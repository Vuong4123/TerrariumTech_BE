using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Repositories.Repositories;
using TerrariumGardenTech.Service.DTOs;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service
{
    public class UserService : IUserService
    {
        private readonly GenericRepository<User> _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(GenericRepository<User> userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<bool> RegisterUserAsync(UserRegisterDto userDto)
        {
            var existingUser = await _userRepository.FindOneAsync(u => u.Username == userDto.Username || u.Email == userDto.Email, false);
            if (existingUser != null)
                return false;

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            var newUser = new User
            {
                Username = userDto.Username,
                Password = hashedPassword,
                Email = userDto.Email,
                FullName = userDto.FullName,
                PhoneNumber = userDto.PhoneNumber,
                DateOfBirth = userDto.DateOfBirth,
                Gender = userDto.Gender,
                CreatedAt = DateTime.UtcNow,
                Status = "Active"
            };

            await _userRepository.CreateAsync(newUser);
            return true;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _userRepository.FindOneAsync(u => u.Username == username, false);
            if (user == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                return null;

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings.GetValue<string>("SecretKey");
            var issuer = jwtSettings.GetValue<string>("Issuer");
            var audience = jwtSettings.GetValue<string>("Audience");
            var expiryMinutes = jwtSettings.GetValue<int>("ExpiryMinutes");

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User")
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

