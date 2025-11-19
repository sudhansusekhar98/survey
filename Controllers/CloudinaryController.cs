using Microsoft.AspNetCore.Mvc;
using SurveyApp.Repo;

namespace SurveyApp.Controllers
{
    /// <summary>
    /// Controller for handling Cloudinary image uploads
    /// </summary>
    public class CloudinaryController : Controller
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<CloudinaryController> _logger;

        public CloudinaryController(ICloudinaryService cloudinaryService, ILogger<CloudinaryController> logger)
        {
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        /// <summary>
        /// Upload image from base64 string
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UploadBase64Image([FromBody] UploadBase64Request request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Base64Image))
                {
                    return BadRequest(new { success = false, message = "No image data provided" });
                }

                var result = await _cloudinaryService.UploadImageAsync(request.Base64Image, request.Folder ?? "survey-images");

                if (result.Error != null)
                {
                    return BadRequest(new { success = false, message = result.Error.Message });
                }

                return Ok(new
                {
                    success = true,
                    url = result.SecureUrl.ToString(),
                    publicId = result.PublicId,
                    width = result.Width,
                    height = result.Height
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading base64 image to Cloudinary");
                return StatusCode(500, new { success = false, message = "Error uploading image" });
            }
        }

        /// <summary>
        /// Upload image from file
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, string? folder = "survey-images")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No file uploaded" });
                }

                using var stream = file.OpenReadStream();
                var result = await _cloudinaryService.UploadImageAsync(stream, file.FileName, folder ?? "survey-images");

                if (result.Error != null)
                {
                    return BadRequest(new { success = false, message = result.Error.Message });
                }

                return Ok(new
                {
                    success = true,
                    url = result.SecureUrl.ToString(),
                    publicId = result.PublicId,
                    width = result.Width,
                    height = result.Height
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Cloudinary");
                return StatusCode(500, new { success = false, message = "Error uploading image" });
            }
        }

        /// <summary>
        /// Upload multiple images
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UploadMultiple(List<IFormFile> files, string? folder = "survey-images")
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest(new { success = false, message = "No files uploaded" });
                }

                var uploadedImages = new List<object>();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        using var stream = file.OpenReadStream();
                        var result = await _cloudinaryService.UploadImageAsync(stream, file.FileName, folder ?? "survey-images");

                        if (result.Error == null)
                        {
                            uploadedImages.Add(new
                            {
                                url = result.SecureUrl.ToString(),
                                publicId = result.PublicId,
                                width = result.Width,
                                height = result.Height
                            });
                        }
                    }
                }

                return Ok(new
                {
                    success = true,
                    images = uploadedImages,
                    count = uploadedImages.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading multiple files to Cloudinary");
                return StatusCode(500, new { success = false, message = "Error uploading images" });
            }
        }

        /// <summary>
        /// Delete an image
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteImage([FromBody] DeleteImageRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.PublicId))
                {
                    return BadRequest(new { success = false, message = "No public ID provided" });
                }

                var result = await _cloudinaryService.DeleteImageAsync(request.PublicId);

                if (result.Error != null)
                {
                    return BadRequest(new { success = false, message = result.Error.Message });
                }

                return Ok(new
                {
                    success = true,
                    message = "Image deleted successfully",
                    result = result.Result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from Cloudinary");
                return StatusCode(500, new { success = false, message = "Error deleting image" });
            }
        }
    }

    // Request models
    public class UploadBase64Request
    {
        public string Base64Image { get; set; } = string.Empty;
        public string? Folder { get; set; }
    }

    public class DeleteImageRequest
    {
        public string PublicId { get; set; } = string.Empty;
    }
}
