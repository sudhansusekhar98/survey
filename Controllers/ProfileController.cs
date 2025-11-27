using AnalyticaDocs.Models;
using AnalyticaDocs.Repo;
using AnalyticaDocs.Repository;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Repo;
using System.Diagnostics;

namespace AnalyticaDocs.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IAdmin _repository;
        private readonly ICommonUtil _util;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IAdmin repository, ICommonUtil util, ICloudinaryService cloudinaryService, ILogger<ProfileController> logger)
        {
            _repository = repository;
            _util = util;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Check if user is logged in
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Index", "UserLogin");
            }

            int userId = Convert.ToInt32(userIdStr);
            var user = _repository.GetUserById(userId);

            if (user == null)
            {
                TempData["ResultType"] = "danger";
                TempData["ResultMessage"] = "<strong>Error!</strong> User profile not found.";
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        /// <summary>
        /// Upload profile picture
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture([FromBody] UploadProfilePictureRequest request)
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserID");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                int userId = Convert.ToInt32(userIdStr);

                if (string.IsNullOrEmpty(request.Base64Image))
                {
                    return Json(new { success = false, message = "No image data provided" });
                }

                // Get current user to check for existing profile picture
                var currentUser = _repository.GetUserById(userId);
                
                // Delete old profile picture if exists
                if (currentUser != null && !string.IsNullOrEmpty(currentUser.ProfilePicturePublicId))
                {
                    try
                    {
                        await _cloudinaryService.DeleteImageAsync(currentUser.ProfilePicturePublicId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old profile picture: {PublicId}", currentUser.ProfilePicturePublicId);
                    }
                }

                // Upload new profile picture to Cloudinary
                var uploadResult = await _cloudinaryService.UploadImageAsync(request.Base64Image, "profile-pictures");

                if (uploadResult.Error != null)
                {
                    return Json(new { success = false, message = uploadResult.Error.Message });
                }

                // Update user profile with new image URL using the dedicated method
                bool isUpdated = _repository.UpdateProfilePicture(
                    userId, 
                    uploadResult.SecureUrl.ToString(), 
                    uploadResult.PublicId
                );

                if (isUpdated)
                {
                    // Store profile picture URL in session for immediate header update
                    HttpContext.Session.SetString("ProfilePictureUrl", uploadResult.SecureUrl.ToString());

                    return Json(new
                    {
                        success = true,
                        message = "Profile picture updated successfully",
                        url = uploadResult.SecureUrl.ToString(),
                        publicId = uploadResult.PublicId
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update profile picture in database" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile picture");
                return Json(new { success = false, message = "Error uploading profile picture" });
            }
        }

        /// <summary>
        /// Delete profile picture
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserID");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                int userId = Convert.ToInt32(userIdStr);
                var user = _repository.GetUserById(userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Delete from Cloudinary if exists
                if (!string.IsNullOrEmpty(user.ProfilePicturePublicId))
                {
                    try
                    {
                        await _cloudinaryService.DeleteImageAsync(user.ProfilePicturePublicId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete profile picture from Cloudinary: {PublicId}", user.ProfilePicturePublicId);
                    }
                }

                // Update user to remove profile picture using the dedicated method
                bool isUpdated = _repository.UpdateProfilePicture(userId, null, null);

                if (isUpdated)
                {
                    // Remove from session
                    HttpContext.Session.Remove("ProfilePictureUrl");

                    return Json(new { success = true, message = "Profile picture deleted successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete profile picture from database" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile picture");
                return Json(new { success = false, message = "Error deleting profile picture" });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // Request model for profile picture upload
    public class UploadProfilePictureRequest
    {
        public string Base64Image { get; set; } = string.Empty;
    }
}
