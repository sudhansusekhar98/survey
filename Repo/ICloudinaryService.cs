using CloudinaryDotNet.Actions;

namespace SurveyApp.Repo
{
    /// <summary>
    /// Service interface for Cloudinary operations
    /// </summary>
    public interface ICloudinaryService
    {
        /// <summary>
        /// Upload an image to Cloudinary from a base64 string
        /// </summary>
        /// <param name="base64Image">Base64 encoded image string</param>
        /// <param name="folder">Optional folder path in Cloudinary</param>
        /// <returns>Upload result containing URL and public ID</returns>
        Task<ImageUploadResult> UploadImageAsync(string base64Image, string folder = "survey-images");

        /// <summary>
        /// Upload an image to Cloudinary from a file stream
        /// </summary>
        /// <param name="fileStream">File stream</param>
        /// <param name="fileName">File name</param>
        /// <param name="folder">Optional folder path in Cloudinary</param>
        /// <returns>Upload result containing URL and public ID</returns>
        Task<ImageUploadResult> UploadImageAsync(Stream fileStream, string fileName, string folder = "survey-images");

        /// <summary>
        /// Delete an image from Cloudinary
        /// </summary>
        /// <param name="publicId">Public ID of the image to delete</param>
        /// <returns>Deletion result</returns>
        Task<DeletionResult> DeleteImageAsync(string publicId);

        /// <summary>
        /// Delete multiple images from Cloudinary
        /// </summary>
        /// <param name="publicIds">List of public IDs to delete</param>
        /// <returns>Deletion result</returns>
        Task<DelResResult> DeleteImagesAsync(List<string> publicIds);
    }
}
