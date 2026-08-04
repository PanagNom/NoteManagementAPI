namespace NoteManagementAPI.Configuration
{
    internal sealed class JwtAuthenticationSettings
    {
        public const string SectionName = "Authentication";

        public string Issuer { get; private init; } = string.Empty;
        public string Audience { get; private init; } = string.Empty;
        public string SecretForKey { get; private init; } = string.Empty;
        public int AccessTokenLifetimeMinutes { get; private init; }
        public int RefreshTokenLifetimeDays { get; private init; }

        public static JwtAuthenticationSettings FromConfiguration(IConfiguration configuration)
        {
            var section = configuration.GetSection(SectionName);
            var settings = new JwtAuthenticationSettings
            {
                Issuer = GetRequiredValue(section, nameof(Issuer)),
                Audience = GetRequiredValue(section, nameof(Audience)),
                SecretForKey = GetRequiredValue(section, nameof(SecretForKey)),
                AccessTokenLifetimeMinutes = section.GetValue<int?>(nameof(AccessTokenLifetimeMinutes)) ?? 15,
                RefreshTokenLifetimeDays = section.GetValue<int?>(nameof(RefreshTokenLifetimeDays)) ?? 7
            };

            settings.Validate();
            return settings;
        }

        public byte[] GetSigningKeyBytes()
        {
            return Convert.FromBase64String(SecretForKey);
        }

        private static string GetRequiredValue(IConfigurationSection section, string key)
        {
            var value = section[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"{SectionName}:{key} is not configured.");
            }

            return value;
        }

        private void Validate()
        {
            byte[] signingKey;
            try
            {
                signingKey = GetSigningKeyBytes();
            }
            catch (FormatException exception)
            {
                throw new InvalidOperationException(
                    $"{SectionName}:{nameof(SecretForKey)} must be a Base64-encoded key.",
                    exception);
            }

            if (signingKey.Length < 32)
            {
                throw new InvalidOperationException(
                    $"{SectionName}:{nameof(SecretForKey)} must contain at least 32 bytes.");
            }

            if (AccessTokenLifetimeMinutes is < 5 or > 60)
            {
                throw new InvalidOperationException(
                    $"{SectionName}:{nameof(AccessTokenLifetimeMinutes)} must be between 5 and 60.");
            }

            if (RefreshTokenLifetimeDays is < 1 or > 30)
            {
                throw new InvalidOperationException(
                    $"{SectionName}:{nameof(RefreshTokenLifetimeDays)} must be between 1 and 30.");
            }
        }
    }
}
