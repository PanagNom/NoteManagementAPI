using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NoteManagementAPI.Controllers
{
    /// <summary>
    /// Manages user authentication, including login and token generation.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class Authentication : ControllerBase
    {
        /// <summary>
        /// Represents a request to the login action, containing username and password.
        /// </summary>
        public class LoginRequest
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        /// <summary>
        /// Represents a user in the NotePad system.
        /// </summary>
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
        /// <summary>
        /// Initializes a new instance of the <see cref="Authentication"/> controller.
        /// </summary>
        /// <param name="configuration">Application configuration used for authentication settings.</param>
        public Authentication(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Authenticates the user by validating credentials and generating a JWT token.
        /// </summary>
        /// <param name="request">The login request containing username and password.</param>
        /// <returns>A JWT token as a string if authentication is successful; otherwise, an Unauthorized result.</returns>
        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate([FromBody] LoginRequest? request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var user = ValidateCredentials(request.Username, request.Password);

            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(GetRequiredConfigurationValue("Authentication:SecretForKey")));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claimsForToken = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName)
            };

            var jwtSecurityToken = new JwtSecurityToken(
                GetRequiredConfigurationValue("Authentication:Issuer"),
                GetRequiredConfigurationValue("Authentication:Audience"),
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                signingCredentials);

            var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return Ok(tokenToReturn);
        }

        private NotePadUser? ValidateCredentials(string username, string password)
        {
            var configuredUsername = _configuration["Authentication:Username"];
            var configuredPassword = _configuration["Authentication:Password"];

            if (string.IsNullOrWhiteSpace(configuredUsername) || string.IsNullOrEmpty(configuredPassword))
            {
                return null;
            }

            if (!string.Equals(username, configuredUsername, StringComparison.Ordinal) ||
                !string.Equals(password, configuredPassword, StringComparison.Ordinal))
            {
                return null;
            }

            return new NotePadUser(
                1,
                configuredUsername,
                _configuration["Authentication:FirstName"] ?? configuredUsername,
                _configuration["Authentication:LastName"] ?? string.Empty);
        }

        private string GetRequiredConfigurationValue(string key)
        {
            return _configuration[key]
                ?? throw new InvalidOperationException($"{key} is not configured.");
        }
    }
}
