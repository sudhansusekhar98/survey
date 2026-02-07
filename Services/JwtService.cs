using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SurveyApp.Models.Api;
using AnalyticaDocs.Models;
using SurveyApp.Models;

namespace SurveyApp.Services
{
    /// <summary>
    /// JWT Settings configuration
    /// </summary>
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int TokenExpirationMinutes { get; set; } = 60;
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }

    /// <summary>
    /// JWT Token Service Interface
    /// </summary>
    public interface IJwtService
    {
        LoginResponse GenerateTokens(UserLoginModel user);
        LoginResponse GenerateTokens(UserModel user);
        ClaimsPrincipal? ValidateToken(string token);
        string GenerateRefreshToken();
        bool ValidateRefreshToken(int userId, string refreshToken);
        void SaveRefreshToken(int userId, string refreshToken, DateTime expiration);
        void RevokeRefreshToken(int userId);
    }

    /// <summary>
    /// JWT Token Service Implementation
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtService> _logger;
        
        // In-memory store for refresh tokens (in production, use database)
        private static readonly Dictionary<int, (string Token, DateTime Expiration)> _refreshTokens = new();
        private static readonly object _lock = new();

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() 
                ?? new JwtSettings 
                { 
                    SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
                    Issuer = "SurveyApp",
                    Audience = "SurveyAppMobile",
                    TokenExpirationMinutes = 60,
                    RefreshTokenExpirationDays = 7
                };
            _logger = logger;
        }

        /// <summary>
        /// Generate JWT access token and refresh token for UserLoginModel (login response)
        /// </summary>
        public LoginResponse GenerateTokens(UserLoginModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationMinutes);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.LoginName),
                new Claim("LoginId", user.LoginId),
                new Claim(ClaimTypes.Role, GetRoleName(user.RoleId)),
                new Claim("RoleId", user.RoleId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();
            var refreshExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            SaveRefreshToken(user.UserId, refreshToken, refreshExpiration);

            return new LoginResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Expiration = expiration,
                User = new UserDto
                {
                    UserId = user.UserId,
                    LoginId = user.LoginId,
                    UserName = user.LoginName,
                    RoleId = user.RoleId,
                    RoleName = GetRoleName(user.RoleId),
                    ProfilePictureUrl = user.ProfilePictureUrl
                }
            };
        }

        /// <summary>
        /// Generate JWT access token and refresh token for full UserModel
        /// </summary>
        public LoginResponse GenerateTokens(UserModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationMinutes);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId?.ToString() ?? "0"),
                new Claim(ClaimTypes.Name, user.LoginName),
                new Claim("LoginId", user.LoginId),
                new Claim(ClaimTypes.Role, GetRoleName(user.RoleId)),
                new Claim("RoleId", user.RoleId.ToString()),
                new Claim(ClaimTypes.Email, user.EmailID ?? string.Empty),
                new Claim("MobileNo", user.MobileNo ?? string.Empty),
                new Claim("EmpId", user.EmpID?.ToString() ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();
            var refreshExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            SaveRefreshToken(user.UserId ?? 0, refreshToken, refreshExpiration);

            return new LoginResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Expiration = expiration,
                User = new UserDto
                {
                    UserId = user.UserId ?? 0,
                    LoginId = user.LoginId,
                    UserName = user.LoginName,
                    RoleId = user.RoleId,
                    RoleName = GetRoleName(user.RoleId),
                    Email = user.EmailID,
                    MobileNo = user.MobileNo,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    EmpId = user.EmpID
                }
            };
        }

        /// <summary>
        /// Validate JWT token
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return null;
            }
        }

        /// <summary>
        /// Generate secure refresh token
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Validate refresh token
        /// </summary>
        public bool ValidateRefreshToken(int userId, string refreshToken)
        {
            lock (_lock)
            {
                if (_refreshTokens.TryGetValue(userId, out var stored))
                {
                    return stored.Token == refreshToken && stored.Expiration > DateTime.UtcNow;
                }
                return false;
            }
        }

        /// <summary>
        /// Save refresh token
        /// </summary>
        public void SaveRefreshToken(int userId, string refreshToken, DateTime expiration)
        {
            lock (_lock)
            {
                _refreshTokens[userId] = (refreshToken, expiration);
            }
        }

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        public void RevokeRefreshToken(int userId)
        {
            lock (_lock)
            {
                _refreshTokens.Remove(userId);
            }
        }

        private static string GetRoleName(int roleId) => roleId switch
        {
            101 => "SuperAdmin",
            102 => "Admin",
            _ => "User"
        };
    }
}
