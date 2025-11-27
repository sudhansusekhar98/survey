# Cloudinary Integration Guide

## Overview
This application now integrates with Cloudinary for cloud-based image storage. Images captured from camera or uploaded from gallery are automatically stored in your Cloudinary account.

## Configuration
Your Cloudinary credentials are configured in `appsettings.json`:

```json
"Cloudinary": {
  "CloudName": "dzkkf0c05",
  "ApiKey": "668214466925985",
  "ApiSecret": "HE7uA6r-lD6HPJdPsXymgYUZVNI"
}
```

## Features Implemented

### 1. **CloudinaryService** (`/Repo/CloudinaryService.cs`)
A service class that handles all Cloudinary operations:
- Upload images from base64 strings (camera captures)
- Upload images from file streams (gallery uploads)
- Delete single images
- Delete multiple images in batch
- Automatic image optimization (quality: auto, format: auto)

### 2. **CloudinaryController** (`/Controllers/CloudinaryController.cs`)
API endpoints for image operations:
- `POST /Cloudinary/UploadBase64Image` - Upload from camera/base64
- `POST /Cloudinary/UploadFile` - Upload from file input
- `POST /Cloudinary/UploadMultiple` - Batch upload multiple files
- `POST /Cloudinary/DeleteImage` - Delete an image by publicId

### 3. **Updated Models** (`/Models/SurveyDetailsUpdate.cs`)
Extended models to support Cloudinary URLs:
- `CloudinaryUrls` - List of image URLs per item
- `CloudinaryPublicIds` - List of Cloudinary public IDs for deletion

### 4. **Enhanced View** (`/Views/SurveyDetails/ItemMasterSelection.cshtml`)
Updated JavaScript to:
- Upload captured/selected images to Cloudinary automatically
- Display uploaded images with Cloudinary URLs
- Track images per survey item
- Delete images from Cloudinary when removed
- Show loading indicators during upload

## How It Works

### Image Upload Flow:
1. User captures photo or selects from gallery
2. JavaScript determines folder path: `survey-{SurveyID}/location-{LocID}/item-{ItemID}`
3. Image is converted to base64 format
4. JavaScript sends AJAX request to `/Cloudinary/UploadBase64Image` with folder path
5. CloudinaryService uploads to Cloudinary cloud in the specified folder
6. Cloudinary returns secure URL and public ID
7. Image preview displayed with Cloudinary URL
8. URLs stored as hidden inputs in form

### Form Submission:
When the form is submitted, each item includes:
- `ItemLists[0].CloudinaryUrls` - Array of image URLs
- `ItemLists[0].CloudinaryPublicIds` - Array of public IDs

### Image Deletion:
When user clicks the X button:
- JavaScript sends delete request to Cloudinary
- Image is removed from cloud storage
- Preview wrapper is removed from DOM

## Storage Structure
Images are organized in Cloudinary with a hierarchical folder structure:
- **Folder Structure**: `survey-{SurveyID}/location-{LocID}/item-{ItemID}/`
- **Example**: `survey-2025117001/location-124/item-1000000/`
- **Naming**: `survey_{guid}`
- **Transformations**: Auto quality, auto format

### Folder Organization Benefits:
1. **Easy Navigation**: Browse images by survey → location → item
2. **Logical Grouping**: All images for a specific item in one folder
3. **Batch Operations**: Delete all images for a survey/location easily
4. **Cleaner Structure**: Avoid mixing thousands of images in one folder

## Benefits

1. **No Server Storage**: Images stored in cloud, reducing server disk usage
2. **CDN Delivery**: Fast image loading from Cloudinary's global CDN
3. **Automatic Optimization**: Images optimized for web delivery
4. **Scalability**: No storage limits on your server
5. **Transformations**: Easy to apply filters, resize, crop via URL parameters

## Usage in Controller

To access uploaded images in your controller:

```csharp
[HttpPost]
public IActionResult UpdateItem(SurveyDetailsUpdate model)
{
    foreach (var item in model.ItemLists)
    {
        if (item.CloudinaryUrls != null && item.CloudinaryUrls.Count > 0)
        {
            // Store URLs in database
            string imageUrls = string.Join(",", item.CloudinaryUrls);
            string publicIds = string.Join(",", item.CloudinaryPublicIds);
            
            // Save to database fields
            item.ImgPath = imageUrls;
            item.ImgID = publicIds;
        }
    }
    
    // Continue with your existing save logic...
}
```

## Cloudinary Dashboard
Access your uploaded images at:
https://cloudinary.com/console/media_library

You can:
- Browse folder structure: survey-{id}/location-{id}/item-{id}
- View all uploaded images organized by survey and location
- Organize into folders
- Apply transformations
- Monitor usage statistics
- Delete entire folders for cleanup

## Cloudinary URL Transformations

You can manipulate images via URL parameters:
- Resize: `https://res.cloudinary.com/{cloud}/image/upload/w_300,h_200/{publicId}`
- Crop: `https://res.cloudinary.com/{cloud}/image/upload/c_fill,w_300,h_200/{publicId}`
- Quality: `https://res.cloudinary.com/{cloud}/image/upload/q_50/{publicId}`

## Security Notes

1. **API Keys**: Stored in `appsettings.json` - ensure this file is in `.gitignore`
2. **Upload Signing**: Consider implementing signed uploads for production
3. **Rate Limits**: Free tier has upload/transformation limits
4. **Public IDs**: Track these to enable deletion of old images

## Testing

To test the integration:

1. Run the application:
   ```powershell
   dotnet run
   ```

2. Navigate to survey item selection page
3. Increment quantity for any item
4. Click "Take Photo" or "Gallery" button
5. Upload an image
6. Check browser network tab - should see successful POST to `/Cloudinary/UploadBase64Image`
7. Check Cloudinary console - image should appear in `survey-images` folder

## Troubleshooting

### Upload Fails
- Check Cloudinary credentials in `appsettings.json`
- Verify internet connection
- Check browser console for errors
- Ensure CloudinaryDotNet package is installed

### Images Not Displaying
- Verify HTTPS URLs are returned
- Check CORS settings if loading from different domain
- Inspect hidden input values in form

### Deletion Not Working
- Ensure correct publicId is passed
- Check Cloudinary API key has delete permissions
- Verify network request completes successfully

## Next Steps

1. **Database Integration**: Update stored procedures to save/retrieve Cloudinary URLs
2. **Display Existing Images**: Load and display existing images when editing surveys
3. **Batch Operations**: Implement cleanup job to delete unused images
4. **Error Handling**: Add user-friendly error messages
5. **Progress Indicators**: Show upload progress for large images
6. **Image Validation**: Add size/format restrictions before upload

## Support
- Cloudinary Documentation: https://cloudinary.com/documentation
- CloudinaryDotNet GitHub: https://github.com/cloudinary/CloudinaryDotNet
