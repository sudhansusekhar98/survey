using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace SurveyApp.Repo
{
    
    // Service for handling Cloudinary image upload and deletion operations
   
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IOptions<CloudinarySettings> config, ILogger<CloudinaryService> logger)
        {
            _logger = logger;
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }

        /// Upload an image to Cloudinary from a base64 string
        public async Task<ImageUploadResult> UploadImageAsync(string base64Image, string folder = "survey-images")
        {
            try
            {
                // Remove data URL prefix if present (e.g., "data:image/png;base64,")
                if (base64Image.Contains(","))
                {
                    base64Image = base64Image.Split(',')[1];
                }

                var bytes = Convert.FromBase64String(base64Image);
                using var stream = new MemoryStream(bytes);

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription($"survey_{Guid.NewGuid()}", stream),
                    Folder = folder,
                    // Removed auto transformations - images are pre-compressed on client side
                    Overwrite = false,
                    UseFilename = false,
                    UniqueFilename = true
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", result.Error.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Cloudinary");
                throw;
            }
        }

        /// <summary>
        /// Upload an image to Cloudinary from a file stream
        /// </summary>
        public async Task<ImageUploadResult> UploadImageAsync(Stream fileStream, string fileName, string folder = "survey-images")
        {
            try
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, fileStream),
                    Folder = folder,
                    // Removed auto transformations for faster uploads
                    Overwrite = false,
                    UseFilename = false,
                    UniqueFilename = true
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", result.Error.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Cloudinary from stream");
                throw;
            }
        }

        /// <summary>
        /// Delete an image from Cloudinary
        /// </summary>
        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            try
            {
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary deletion error: {Error}", result.Error.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from Cloudinary: {PublicId}", publicId);
                throw;
            }
        }

        /// <summary>
        /// Delete multiple images from Cloudinary
        /// </summary>
        public async Task<DelResResult> DeleteImagesAsync(List<string> publicIds)
        {
            try
            {
                var deleteParams = new DelResParams
                {
                    PublicIds = publicIds
                };

                var result = await _cloudinary.DeleteResourcesAsync(deleteParams);

                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary bulk deletion error: {Error}", result.Error.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting multiple images from Cloudinary");
                throw;
            }
        }
    }
}
