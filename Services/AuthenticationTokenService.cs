using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NoteManagementAPI.Configuration;
using NoteManagementAPI.DTOs;
using NoteManagementAPI.Infrastructure;
using NoteManagementAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NoteManagementAPI.Services
{
    internal sealed class AuthenticationTokenService : IAuthenticationTokenService
    {
        private const int RefreshTokenByteLength = 64;

        private readonly NoteDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtAuthenticationSettings _settings;
        private readonly SigningCredentials _signingCredentials;

        public AuthenticationTokenService(
            NoteDbContext context,
            UserManager<ApplicationUser> userManager,
            JwtAuthenticationSettings settings)
        {
            _context = context;
            _userManager = userManager;
            _settings = settings;
            _signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(settings.GetSigningKeyBytes()),
                SecurityAlgorithms.HmacSha256);
        }

        public async Task<TokenResponseDTO> IssueTokenPairAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var refreshToken = CreateRefreshToken(
                user.Id,
                Guid.NewGuid(),
                now,
                now.AddDays(_settings.RefreshTokenLifetimeDays));

            var response = await CreateResponseAsync(user, refreshToken, now);
            _context.RefreshTokens.Add(refreshToken.Entity);
            await _context.SaveChangesAsync(cancellationToken);

            return response;
        }

        public async Task<TokenResponseDTO?> RotateRefreshTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken)
        {
            var tokenHash = HashRefreshToken(refreshToken);
            var currentToken = await _context.RefreshTokens
                .Include(token => token.User)
                .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

            if (currentToken == null)
            {
                return null;
            }

            var now = DateTimeOffset.UtcNow;
            if (currentToken.RevokedAtUtc != null)
            {
                await RevokeActiveFamilyAsync(
                    currentToken.UserId,
                    currentToken.FamilyId,
                    now,
                    cancellationToken);
                return null;
            }

            if (!currentToken.IsActive(now))
            {
                return null;
            }

            var replacementToken = CreateRefreshToken(
                currentToken.UserId,
                currentToken.FamilyId,
                now,
                currentToken.ExpiresAtUtc);

            currentToken.Revoke(now, replacementToken.Entity.TokenHash);
            _context.RefreshTokens.Add(replacementToken.Entity);

            var response = await CreateResponseAsync(currentToken.User, replacementToken, now);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                _context.ChangeTracker.Clear();
                await RevokeActiveFamilyAsync(
                    currentToken.UserId,
                    currentToken.FamilyId,
                    now,
                    cancellationToken);
                return null;
            }

            return response;
        }

        public async Task RevokeRefreshTokenFamilyAsync(
            string refreshToken,
            string userId,
            CancellationToken cancellationToken)
        {
            var tokenHash = HashRefreshToken(refreshToken);
            var familyId = await _context.RefreshTokens
                .Where(token => token.TokenHash == tokenHash && token.UserId == userId)
                .Select(token => (Guid?)token.FamilyId)
                .SingleOrDefaultAsync(cancellationToken);

            if (familyId == null)
            {
                return;
            }

            await RevokeActiveFamilyAsync(
                userId,
                familyId.Value,
                DateTimeOffset.UtcNow,
                cancellationToken);
        }

        private async Task<TokenResponseDTO> CreateResponseAsync(
            ApplicationUser user,
            RefreshTokenIssue refreshToken,
            DateTimeOffset now)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var accessTokenExpiresAtUtc = now.AddMinutes(_settings.AccessTokenLifetimeMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName!),
                new(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var accessToken = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: accessTokenExpiresAtUtc.UtcDateTime,
                signingCredentials: _signingCredentials);

            return new TokenResponseDTO
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
                RefreshToken = refreshToken.RawToken,
                RefreshTokenExpiresAtUtc = refreshToken.Entity.ExpiresAtUtc
            };
        }

        private static RefreshTokenIssue CreateRefreshToken(
            string userId,
            Guid familyId,
            DateTimeOffset createdAtUtc,
            DateTimeOffset expiresAtUtc)
        {
            var rawToken = Base64UrlEncoder.Encode(
                RandomNumberGenerator.GetBytes(RefreshTokenByteLength));
            var entity = new RefreshToken(
                HashRefreshToken(rawToken),
                userId,
                familyId,
                createdAtUtc,
                expiresAtUtc);

            return new RefreshTokenIssue(rawToken, entity);
        }

        private static string HashRefreshToken(string refreshToken)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
        }

        private Task<int> RevokeActiveFamilyAsync(
            string userId,
            Guid familyId,
            DateTimeOffset revokedAtUtc,
            CancellationToken cancellationToken)
        {
            return _context.RefreshTokens
                .Where(token =>
                    token.UserId == userId &&
                    token.FamilyId == familyId &&
                    token.RevokedAtUtc == null)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        token => token.RevokedAtUtc,
                        revokedAtUtc),
                    cancellationToken);
        }

        private sealed record RefreshTokenIssue(string RawToken, RefreshToken Entity);
    }
}
