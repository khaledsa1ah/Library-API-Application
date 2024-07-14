using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Day1.Data;
using Serilog;
using Microsoft.Extensions.Caching.Memory;

namespace Day1.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")] // Specify the API version
    public class UserController : ControllerBase
    {
        private readonly JwtOptions _jwtOptions;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;

        public UserController(JwtOptions jwtOptions, ApplicationDbContext dbContext, IMemoryCache memoryCache)
        {
            _jwtOptions = jwtOptions;
            _dbContext = dbContext;
            _memoryCache = memoryCache;
        }

        [HttpPost("auth")] // Route becomes api/v1/user/auth
        public ActionResult<string> AuthenticateUser(AuthenticationRequest request)
        {
            var user = _dbContext.Set<User>().FirstOrDefault(x => x.Name == request.Username && x.Password == request.Password);
            if (user == null)
            {
                Log.Warning("Authentication failed for user {Username}", request.Username);
                return Unauthorized();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtOptions.issuer,
                Audience = _jwtOptions.audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.signingKey)), SecurityAlgorithms.HmacSha256Signature),
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                    new Claim(ClaimTypes.Name, user.Name)
                }),
            };
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            // Cache 
            _memoryCache.Set(accessToken, user.ID, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_jwtOptions.lifetime)
            });

            Log.Information("User {Username} authenticated successfully", user.Name);
            return Ok(accessToken);
        }
    }
}
