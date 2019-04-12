namespace BasicBot.Infrastructure.Extensions
{
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;

    public static class TokenExtensions
    {
        public static string GetClaim(this string token, string claim)
        {
            var jwt = new JwtSecurityToken(token);
            return jwt.Payload.GetValueOrDefault(claim)?.ToString();
        }
    }
}
