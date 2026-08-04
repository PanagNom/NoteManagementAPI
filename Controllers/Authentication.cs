using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NoteManagementAPI.Controllers
{
    /// <summary>
    /// Manages user registration, authentication, and token generation.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class Authentication : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Authentication"/> controller.
        /// </summary>
        public Authentication(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        /// <summary>
        /// Creates a new local user account.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Username.Trim(),
                Email = request.Email.Trim(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim()
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return ValidationProblem(ModelState);
            }

            return StatusCode(StatusCodes.Status201Created);
        }

        /// <summary>
        /// Validates local user credentials and returns a JWT access token.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<string>> Authenticate([FromBody] LoginRequestDTO request)
        {
            var user = await _userManager.FindByNameAsync(request.Username.Trim());
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

            if (!signInResult.Succeeded)
            {
                return Unauthorized("Invalid username or password.");
            }

            var securityKey = new SymmetricSecurityKey(
                Convert.FromBase64String(GetRequiredConfigurationValue("Authentication:SecretForKey")));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var roles = await _userManager.GetRolesAsync(user);

            var claimsForToken = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName!),
                new(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claimsForToken.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var now = DateTime.UtcNow;
            var jwtSecurityToken = new JwtSecurityToken(
                GetRequiredConfigurationValue("Authentication:Issuer"),
                GetRequiredConfigurationValue("Authentication:Audience"),
                claimsForToken,
                now,
                now.AddHours(1),
                signingCredentials);

            return Ok(new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken));
        }

        private string GetRequiredConfigurationValue(string key)
        {
            return _configuration[key]
                ?? throw new InvalidOperationException($"{key} is not configured.");
        }
    }
}
