using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace VoiceEngine.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> SignIn()
        {
            List<Claim> claims = new List<Claim>
                {
                   new Claim("AccountId", Guid.NewGuid().ToString())
                };

            ClaimsIdentity identity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            JwtSecurityToken jwt = new JwtSecurityToken(
                    issuer: "Test",
                    audience: "Test",
                    notBefore: DateTime.UtcNow,
                    claims: identity.Claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(720)),
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes("Please, don't you mind create smth more clever than this")),
                                                                                                                            SecurityAlgorithms.HmacSha256));

            string encodedJwt = "Bearer " + new JwtSecurityTokenHandler().WriteToken(jwt);

            return Ok(encodedJwt);
        }
    }
}
