// Cloudinary API Examples - Quick Reference

// ============================================
// 1. Upload Image from Camera (Base64) with Folder Structure
// ============================================
async function uploadFromCamera(base64ImageData, surveyId, locationId, itemId) {
    // Create hierarchical folder structure
    const folder = `survey-${surveyId}/location-${locationId}/item-${itemId}`;
    
    const response = await fetch('/Cloudinary/UploadBase64Image', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            base64Image: base64ImageData,  // e.g., "data:image/png;base64,iVBORw0KG..."
            folder: folder                  // e.g., "survey-2025117001/location-124/item-1000000"
        })
    });
    
    const result = await response.json();
    
    if (result.success) {
        console.log('Image URL:', result.url);
        console.log('Public ID:', result.publicId);
        console.log('Dimensions:', result.width, 'x', result.height);
    } else {
        console.error('Upload failed:', result.message);
    }
    
    return result;
}

// ============================================
// 2. Upload Single File from Input
// ============================================
async function uploadSingleFile(fileInputElement) {
    const formData = new FormData();
    formData.append('file', fileInputElement.files[0]);
    formData.append('folder', 'survey-images');
    
    const response = await fetch('/Cloudinary/UploadFile', {
        method: 'POST',
        body: formData
    });
    
    const result = await response.json();
    return result;
}

// ============================================
// 3. Upload Multiple Files
// ============================================
async function uploadMultipleFiles(fileInputElement) {
    const formData = new FormData();
    
    for (let i = 0; i < fileInputElement.files.length; i++) {
        formData.append('files', fileInputElement.files[i]);
    }
    formData.append('folder', 'survey-images');
    
    const response = await fetch('/Cloudinary/UploadMultiple', {
        method: 'POST',
        body: formData
    });
    
    const result = await response.json();
    
    if (result.success) {
        console.log('Uploaded', result.count, 'images');
        result.images.forEach(img => {
            console.log('Image:', img.url);
        });
    }
    
    return result;
}

// ============================================
// 4. Delete Image
// ============================================
async function deleteImage(publicId) {
    const response = await fetch('/Cloudinary/DeleteImage', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            publicId: publicId  // e.g., "survey-images/survey_abc123"
        })
    });
    
    const result = await response.json();
    
    if (result.success) {
        console.log('Image deleted:', result.message);
    } else {
        console.error('Delete failed:', result.message);
    }
    
    return result;
}

// ============================================
// 5. Camera Capture Example
// ============================================
async function captureFromCamera() {
    const video = document.createElement('video');
    const stream = await navigator.mediaDevices.getUserMedia({ video: true });
    video.srcObject = stream;
    await video.play();
    
    // Capture after user clicks
    const canvas = document.createElement('canvas');
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    canvas.getContext('2d').drawImage(video, 0, 0);
    
    const base64Image = canvas.toDataURL('image/png');
    
    // Upload to Cloudinary
    const result = await uploadFromCamera(base64Image);
    
    // Stop camera
    stream.getTracks().forEach(track => track.stop());
    
    return result;
}

// ============================================
// 6. File Selection Example
// ============================================
function setupFileUpload() {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    input.multiple = true;
    
    input.onchange = async (e) => {
        const result = await uploadMultipleFiles(e.target);
        console.log('Upload complete:', result);
    };
    
    input.click();
}

// ============================================
// 7. Cloudinary URL Transformations
// ============================================
function getTransformedUrl(originalUrl, transformations) {
    // Example: originalUrl = "https://res.cloudinary.com/dzkkf0c05/image/upload/v123/survey-images/survey_abc.jpg"
    
    const urlParts = originalUrl.split('/upload/');
    
    // Common transformations:
    const transforms = {
        thumbnail: 'w_150,h_150,c_fill',
        medium: 'w_500,h_500,c_limit',
        watermark: 'l_watermark,o_50',
        quality: 'q_auto',
        format: 'f_auto'
    };
    
    const transform = transformations || 'w_300,h_300,c_fill,q_auto,f_auto';
    
    return `${urlParts[0]}/upload/${transform}/${urlParts[1]}`;
}

// Usage:
// const thumbnailUrl = getTransformedUrl(imageUrl, 'w_150,h_150,c_fill');
// const mediumUrl = getTransformedUrl(imageUrl, 'w_500,h_500,c_limit,q_80');

// ============================================
// 8. Display Image with Fallback
// ============================================
function createImageElement(cloudinaryUrl, altText = 'Survey Image') {
    const img = document.createElement('img');
    img.src = cloudinaryUrl;
    img.alt = altText;
    img.className = 'img-fluid';
    
    img.onerror = function() {
        this.src = '/img/placeholder.jpg'; // Fallback image
        console.error('Failed to load image:', cloudinaryUrl);
    };
    
    return img;
}

// ============================================
// 9. Batch Delete (Custom Implementation)
// ============================================
async function deleteMultipleImages(publicIds) {
    const deletePromises = publicIds.map(id => deleteImage(id));
    const results = await Promise.all(deletePromises);
    
    const successful = results.filter(r => r.success).length;
    console.log(`Deleted ${successful} of ${publicIds.length} images`);
    
    return results;
}

// ============================================
// 10. Form Integration Example
// ============================================
class CloudinaryImageManager {
    constructor(containerId) {
        this.container = document.getElementById(containerId);
        this.images = [];
    }
    
    async addImage(base64OrFile) {
        let result;
        
        if (typeof base64OrFile === 'string') {
            result = await uploadFromCamera(base64OrFile);
        } else {
            const formData = new FormData();
            formData.append('file', base64OrFile);
            const response = await fetch('/Cloudinary/UploadFile', {
                method: 'POST',
                body: formData
            });
            result = await response.json();
        }
        
        if (result.success) {
            this.images.push({
                url: result.url,
                publicId: result.publicId
            });
            this.render();
        }
        
        return result;
    }
    
    async removeImage(index) {
        const image = this.images[index];
        await deleteImage(image.publicId);
        this.images.splice(index, 1);
        this.render();
    }
    
    render() {
        this.container.innerHTML = '';
        this.images.forEach((img, index) => {
            const wrapper = document.createElement('div');
            wrapper.className = 'image-item';
            wrapper.innerHTML = `
                <img src="${img.url}" />
                <button onclick="manager.removeImage(${index})">Remove</button>
                <input type="hidden" name="ImageUrls[]" value="${img.url}" />
                <input type="hidden" name="ImagePublicIds[]" value="${img.publicId}" />
            `;
            this.container.appendChild(wrapper);
        });
    }
    
    getFormData() {
        return {
            urls: this.images.map(img => img.url),
            publicIds: this.images.map(img => img.publicId)
        };
    }
}

// Usage:
// const manager = new CloudinaryImageManager('image-container');
// await manager.addImage(base64Image);
