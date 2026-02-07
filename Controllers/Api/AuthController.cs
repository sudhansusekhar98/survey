using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Models.Api;
using SurveyApp.Services;
using AnalyticaDocs.Repo;
using AnalyticaDocs.Models;
using SurveyApp.Repo;

namespace SurveyApp.Controllers.Api
{
    /// <summary>
    /// Authentication API Controller - JWT token management
    /// </summary>
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAdmin _adminRepo;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAdmin adminRepo, 
            IJwtService jwtService,
            IPasswordHasher passwordHasher,
            ILogger<AuthController> logger)
        {
            _adminRepo = adminRepo;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate user and get JWT token
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT token with user information</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<object>.Fail("Validation failed", errors));
                }

                // Authenticate user using existing repository
                var loginModel = new UserLoginModel
                {
                    LoginId = request.LoginId,
                    LoginPassword = request.Password
                };

                var user = _adminRepo.GetLoginUser(loginModel);

                if (user == null)
                {
                    _logger.LogWarning("Login failed for user: {LoginId}", request.LoginId);
                    return Unauthorized(ApiResponse<object>.Fail("Invalid credentials"));
                }

                if (user.ISActive != "Y")
                {
                    _logger.LogWarning("Login attempt for inactive user: {LoginId}", request.LoginId);
                    return Unauthorized(ApiResponse<object>.Fail("Account is deactivated. Please contact administrator."));
                }

                // Check if user must change password
                if (user.MustChangePassword)
                {
                    return Ok(ApiResponse<object>.Fail("Password change required", new List<string> { "MUST_CHANGE_PASSWORD" }));
                }

                // Generate JWT tokens using UserLoginModel overload
                var response = _jwtService.GenerateTokens(user);

                _logger.LogInformation("User {LoginId} logged in successfully", request.LoginId);
                return Ok(ApiResponse<LoginResponse>.Ok(response, "Login successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {LoginId}", request.LoginId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred during login"));
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="request">Refresh token</param>
        /// <returns>New JWT token</returns>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(ApiResponse<object>.Fail("Refresh token is required"));
                }

                // Extract user ID from expired token in header (if provided)
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(ApiResponse<object>.Fail("Authorization header is required"));
                }

                var expiredToken = authHeader.Substring("Bearer ".Length);
                
                // For refresh, we need to get user ID even from expired token
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(expiredToken);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.Fail("Invalid token"));
                }

                // Validate refresh token
                if (!_jwtService.ValidateRefreshToken(userId, request.RefreshToken))
                {
                    return Unauthorized(ApiResponse<object>.Fail("Invalid or expired refresh token"));
                }

                // Get user from database - returns full UserModel
                var user = _adminRepo.GetUserById(userId);
                if (user == null || user.ISActive != "Y")
                {
                    return Unauthorized(ApiResponse<object>.Fail("User not found or inactive"));
                }

                // Generate new tokens using UserModel overload
                var response = _jwtService.GenerateTokens(user);

                _logger.LogInformation("Token refreshed for user: {UserId}", userId);
                return Ok(ApiResponse<LoginResponse>.Ok(response, "Token refreshed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred during token refresh"));
            }
        }

        /// <summary>
        /// Logout and revoke refresh token
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    _jwtService.RevokeRefreshToken(userId);
                    _logger.LogInformation("User {UserId} logged out", userId);
                }

                return Ok(ApiResponse<object>.Ok(null, "Logout successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred during logout"));
            }
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
        public IActionResult GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.Fail("Invalid token"));
                }

                var user = _adminRepo.GetUserById(userId);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.Fail("User not found"));
                }

                var userDto = new UserDto
                {
                    UserId = user.UserId ?? 0,
                    LoginId = user.LoginId,
                    UserName = user.LoginName,
                    RoleId = user.RoleId,
                    RoleName = user.RoleId == 101 ? "SuperAdmin" : user.RoleId == 102 ? "Admin" : "User",
                    Email = user.EmailID,
                    MobileNo = user.MobileNo,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    EmpId = user.EmpID
                };

                return Ok(ApiResponse<UserDto>.Ok(userDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Change password (for users who must change password)
        /// </summary>
        [HttpPost("change-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.LoginId) || 
                    string.IsNullOrEmpty(request.CurrentPassword) || 
                    string.IsNullOrEmpty(request.NewPassword))
                {
                    return BadRequest(ApiResponse<object>.Fail("All fields are required"));
                }

                if (request.NewPassword.Length < 6)
                {
                    return BadRequest(ApiResponse<object>.Fail("Password must be at least 6 characters"));
                }

                // Authenticate with current password
                var loginModel = new UserLoginModel
                {
                    LoginId = request.LoginId,
                    LoginPassword = request.CurrentPassword
                };

                var loginUser = _adminRepo.GetLoginUser(loginModel);
                if (loginUser == null)
                {
                    return Unauthorized(ApiResponse<object>.Fail("Invalid credentials"));
                }

                // Change password
                var result = _adminRepo.ChangePassword(loginUser.UserId, request.CurrentPassword, request.NewPassword);
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Failed to change password"));
                }

                // Clear must change password flag
                _adminRepo.ClearMustChangePasswordFlag(loginUser.UserId);

                // Get updated user and return new tokens
                var user = _adminRepo.GetUserById(loginUser.UserId);
                if (user == null)
                {
                    return StatusCode(500, ApiResponse<object>.Fail("Password changed but failed to retrieve user"));
                }

                var response = _jwtService.GenerateTokens(user);

                return Ok(ApiResponse<LoginResponse>.Ok(response, "Password changed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }
    }

    /// <summary>
    /// Change password request model
    /// </summary>
    public class ChangePasswordRequest
    {
        public string LoginId { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
