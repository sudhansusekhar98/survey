# Cloudinary Hierarchical Folder Structure - Implementation

## Overview
Images are now organized in Cloudinary using a hierarchical folder structure based on Survey ID, Location ID, and Item ID, matching the organizational pattern: `survey-{SurveyID}/location-{LocID}/item-{ItemID}`

## Folder Structure

### Pattern
```
survey-{SurveyID}/
  └── location-{LocID}/
      └── item-{ItemID}/
          ├── survey_abc123.jpg
          ├── survey_def456.jpg
          └── survey_ghi789.jpg
```

### Example
For the screenshot provided:
```
survey-2025117001/
  └── location-124/
      └── item-1000000/
          ├── survey_unique_guid_1.jpg
          ├── survey_unique_guid_2.jpg
          └── survey_unique_guid_3.jpg
```

## Implementation Details

### 1. **View Changes** (`ItemMasterSelection.cshtml`)

#### Added Context Variables
```javascript
const surveyId = @surveyId;    // From ViewBag
const locId = @locId;          // From ViewBag
```

#### Added Item ID Fields
```html
<input type="hidden" class="item-id-field" data-index="@i" value="@item.ItemID" />
```
- Added to both "Existing Items" and "New Items" sections
- Allows JavaScript to access item ID dynamically

#### Updated Upload Function
```javascript
async function uploadToCloudinary(base64Image, previewContainer, itemId) {
    // Create hierarchical folder structure
    const folder = `survey-${surveyId}/location-${locId}/item-${itemId}`;
    
    const response = await fetch(cloudinaryUploadUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            base64Image: base64Image,
            folder: folder  // e.g., "survey-2025117001/location-124/item-1000000"
        })
    });
}
```

#### Updated Camera Capture Handler
```javascript
const itemIndex = preview.dataset.itemIndex;
const itemId = document.querySelector(`input.item-id-field[data-index="${itemIndex}"]`)?.value || '0';
uploadToCloudinary(canvas.toDataURL('image/png'), preview, itemId);
```

#### Updated Gallery Upload Handler
```javascript
const itemIndex = preview.dataset.itemIndex;
const itemId = document.querySelector(`input.item-id-field[data-index="${itemIndex}"]`)?.value || '0';
Array.from(uploadInput.files).forEach(file => {
    if (file.type.startsWith('image/')) {
        let reader = new FileReader();
        reader.onload = function (e) {
            uploadToCloudinary(e.target.result, preview, itemId);
        };
        reader.readAsDataURL(file);
    }
});
```

### 2. **Backend (No Changes Required)**
- CloudinaryController already accepts `folder` parameter
- CloudinaryService already uses the provided folder path
- No stored procedure changes needed

## Benefits

### 1. **Organization**
- Easy to browse images in Cloudinary dashboard
- Navigate: Survey → Location → Item
- Clear visual hierarchy

### 2. **Management**
- Delete all images for a specific item
- Delete all images for a location
- Delete entire survey's images
- Easier bulk operations

### 3. **Performance**
- Smaller folder listings (100s vs 1000s of images)
- Faster media library browsing
- Better Cloudinary dashboard performance

### 4. **Debugging**
- Quickly locate images for specific survey/location/item
- Verify upload counts per item
- Troubleshoot missing images easily

### 5. **Cleanup**
- Archive old surveys by moving folders
- Delete test data without affecting production
- Clear images for specific locations

## Usage Examples

### Example 1: Camera Capture
```
Survey: 2025117001
Location: 124
Item: 1000000

Folder: survey-2025117001/location-124/item-1000000
File: survey_a1b2c3d4.jpg
Full Path: survey-2025117001/location-124/item-1000000/survey_a1b2c3d4.jpg
```

### Example 2: Gallery Upload
```
Survey: 2025117002
Location: 125
Item: 1000001

Folder: survey-2025117002/location-125/item-1000001
Files:
  - survey_e5f6g7h8.jpg
  - survey_i9j0k1l2.jpg
Full Paths:
  - survey-2025117002/location-125/item-1000001/survey_e5f6g7h8.jpg
  - survey-2025117002/location-125/item-1000001/survey_i9j0k1l2.jpg
```

## Cloudinary Dashboard View

### Navigation Path:
1. Open Cloudinary Media Library
2. Browse folders:
   ```
   Media Library
   └── survey-2025117001
       └── location-124
           └── item-1000000
               ├── Image 1
               ├── Image 2
               └── Image 3
   ```

### Operations:
- **Select Folder**: Click to view all images in that item
- **Delete Folder**: Remove all images for an item at once
- **Move Folder**: Reorganize if needed
- **Download Folder**: Backup images for specific item

## Database Storage (Unchanged)

Images are still stored in database as comma-separated strings:

### ImgPath
```
https://res.cloudinary.com/.../survey-2025117001/location-124/item-1000000/survey_abc.jpg,https://res.cloudinary.com/.../survey-2025117001/location-124/item-1000000/survey_def.jpg
```

### ImgID (Public IDs)
```
survey-2025117001/location-124/item-1000000/survey_abc,survey-2025117001/location-124/item-1000000/survey_def
```

## Migration Notes

### Existing Images
- Old images remain in `survey-images/` folder
- New images use hierarchical structure
- Both formats work correctly
- Can manually move old images in Cloudinary if desired

### Backward Compatibility
- System supports both folder structures
- Old images load and display correctly
- New images use new structure automatically

## Testing Checklist

✅ Upload image from camera
✅ Verify folder structure in Cloudinary: `survey-{id}/location-{id}/item-{id}`
✅ Upload image from gallery
✅ Multiple images for same item go to same folder
✅ Different items get different folders
✅ Delete image removes from correct folder
✅ Images load correctly on page reload
✅ Save form preserves correct folder paths

## Troubleshooting

### Images Not Organizing by Folder
- Check JavaScript console for errors
- Verify `surveyId`, `locId`, `itemId` have values
- Check network tab for folder parameter in upload request

### Can't Find Images in Cloudinary
- Search by public ID in Cloudinary
- Check folder path matches: `survey-{id}/location-{id}/item-{id}`
- Verify upload was successful (check browser console)

### Wrong Folder Structure
- Ensure hidden `item-id-field` inputs exist
- Check `data-index` matches preview container index
- Verify JavaScript selector finds correct item ID

## Future Enhancements

1. **Folder Naming Options**: Allow custom folder patterns
2. **Date-based Subfolders**: Add timestamp folders for archiving
3. **Tagging**: Add metadata tags to images
4. **Automatic Cleanup**: Delete images when survey is deleted
5. **Batch Download**: Download all images for a survey
