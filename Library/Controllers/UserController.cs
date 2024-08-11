using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Library.Data;
using Serilog;
using Microsoft.Extensions.Caching.Memory;

namespace Library.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")] // Specify the API version
    public class UserController(JwtOptions jwtOptions, ApplicationDbContext dbContext, IMemoryCache memoryCache) : ControllerBase
    {
        [HttpPost("auth")] // Route becomes api/v1/user/auth
        public ActionResult<string> AuthenticateUser(AuthenticationRequest request)
        {
            var user = dbContext.Set<User>().FirstOrDefault(x => x.Name == request.Username && x.Password == request.Password);
            if (user == null)
            {
                Log.Warning("Authentication failed for user {Username}", request.Username);
                return Unauthorized();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = jwtOptions.Issuer,
                Audience = jwtOptions.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)), SecurityAlgorithms.HmacSha256Signature),
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name)
                }),
            };
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            // Cache 
            memoryCache.Set(accessToken, user.Id, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(jwtOptions.Lifetime)
            });

            Log.Information("User {Username} authenticated successfully", user.Name);
            return Ok(accessToken);
        }
    }
}
