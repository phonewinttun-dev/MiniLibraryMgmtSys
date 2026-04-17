using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MiniLibraryMgmtSys.Domain.Features.Auth
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(TblUser user)
        {
            // Get JWT settings from configuration
            var jwt = _configuration.GetSection("Jwt");

            // claims = data inside the token, which can be used to identify the user and their permissions
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // create security key
            // key string -> byte array -> symmetric security key
            // utf8 for converting string to byte array (binary key) and vice versa 
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new Exception("No Jwt key found!"))
            );

                
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // create token
            // issuer = who created the token
            // audience = who can use the token
            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(jwt["ExpireMinutes"]!)
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
