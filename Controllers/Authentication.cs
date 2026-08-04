using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NoteManagementAPI.Configuration;
using NoteManagementAPI.DTOs;
using NoteManagementAPI.Models;
using NoteManagementAPI.Services;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuthenticationTokenService _tokenService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Authentication"/> controller.
        /// </summary>
        public Authentication(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAuthenticationTokenService tokenService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        /// <summary>
        /// Creates a new local user account.
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting(AuthenticationRateLimitPolicies.Registration)]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
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
        [EnableRateLimiting(AuthenticationRateLimitPolicies.Login)]
        [HttpPost("authenticate")]
        [ProducesResponseType(typeof(TokenResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<TokenResponseDTO>> Authenticate([FromBody] LoginRequestDTO request)
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

            var tokenResponse = await _tokenService.IssueTokenPairAsync(
                user,
                HttpContext.RequestAborted);
            return Ok(tokenResponse);
        }

        /// <summary>
        /// Rotates a valid refresh token and returns a new access/refresh token pair.
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting(AuthenticationRateLimitPolicies.Refresh)]
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(TokenResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<TokenResponseDTO>> Refresh(
            [FromBody] RefreshTokenRequestDTO request)
        {
            var tokenResponse = await _tokenService.RotateRefreshTokenAsync(
                request.RefreshToken,
                HttpContext.RequestAborted);

            if (tokenResponse == null)
            {
                return Unauthorized("Invalid refresh token.");
            }

            return Ok(tokenResponse);
        }

        /// <summary>
        /// Revokes the refresh-token family for the authenticated session.
        /// </summary>
        [Authorize]
        [HttpPost("revoke")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequestDTO request)
        {
            await _tokenService.RevokeRefreshTokenFamilyAsync(
                request.RefreshToken,
                GetCurrentUserId(),
                HttpContext.RequestAborted);

            return NoContent();
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new InvalidOperationException("The authenticated user id claim is missing.");
        }
    }
}
