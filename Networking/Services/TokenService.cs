using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using VoiceEngine.Network.Abstractions.Services;

namespace VoiceEngine.Network.Services
{
    public class TokenService : ITokenService
    {
        public string GetAccountId(string token)
        {
            token = token.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var validations = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                //TODO: Move this shit into settings
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Please, don't you mind create smth more clever than this")),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            var claims = handler.ValidateToken(token, validations, out var tokenSecure);
            return claims.Claims.FirstOrDefault(x => x.Type == "AccountId")?.Value;
        }
    }
}
