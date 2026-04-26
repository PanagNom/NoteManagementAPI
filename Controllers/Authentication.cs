using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace NoteManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Authentication : ControllerBase
    {
        public class LoginRequest
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        private class NotePadUser
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public NotePadUser(int userId, string username, string firstName, string lastName)
            {
                UserId = userId;
                Username = username;
                FirstName = firstName;
                LastName = lastName;
            }
        }

        private readonly IConfiguration _configuration;
        public Authentication(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate([FromBody] LoginRequest request)
        {
            var user = ValidateCredentials(request.Username, request.Password);

            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["Authentication:SecretForKey"]));
            var singningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claimsForToken = new List<Claim>();
            claimsForToken.Add(new Claim("sub", user.UserId.ToString()));
            claimsForToken.Add(new Claim("given_name", user.FirstName));
            claimsForToken.Add(new Claim("family_name", user.LastName));

            var jwtSecurityToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                singningCredentials);

            var tokenToRetunn = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(jwtSecurityToken); 

            return Ok(tokenToRetunn);
        }

        private NotePadUser ValidateCredentials(string? username, string? password)
        {
            // Assuming we have a hardcoded user for demonstration purposes
            return new NotePadUser(1, "testuser", "Test", "User");
        }
    }
}
