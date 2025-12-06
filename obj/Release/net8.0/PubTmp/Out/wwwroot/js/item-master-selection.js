// Item Master Selection - Quantity and Image Management
// This script handles quantity increment/decrement, camera capture, and image upload functionality

function applyWatermark(ctx, watermarkText) {
    const canvas = ctx.canvas;
    // Calculate font size based on canvas width (responsive sizing)
    const fontSize = Math.max(20, canvas.width * 0.025);
    ctx.font = `bold ${fontSize}px Arial`;
    
    // Measure text width
    const textMetrics = ctx.measureText(watermarkText);
    const textWidth = textMetrics.width;
    const textHeight = fontSize;
    
    // Position at bottom center with padding
    const x = (canvas.width - textWidth) / 2;
    const y = canvas.height - 30;
    const padding = 15;
    
    // Draw semi-transparent background for text
    ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
    ctx.fillRect(
        x - padding, 
        y - textHeight - padding/2, 
        textWidth + (padding * 2), 
        textHeight + padding
    );
    
    // Draw white text
    ctx.fillStyle = '#FFFFFF';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'bottom';
    ctx.fillText(watermarkText, canvas.width / 2, canvas.height - 20);
    
    // Add a thin black stroke for better visibility
    ctx.strokeStyle = '#000000';
    ctx.lineWidth = 1;
    ctx.strokeText(watermarkText, canvas.width / 2, canvas.height - 20);
}

// Global variables for Cloudinary URLs (will be set from view)
let cloudinaryUploadUrl = '';
let cloudinaryDeleteUrl = '';
let surveyId = 0;
let locId = 0;

// Initialize the module
function initItemMasterSelection(uploadUrl, deleteUrl, sid, lid) {
    cloudinaryUploadUrl = uploadUrl;
    cloudinaryDeleteUrl = deleteUrl;
    surveyId = sid;
    locId = lid;
}

// Quantity increment/decrement buttons
document.addEventListener('click', function (e) {
    // Use closest to ensure button click works even if icon is clicked
    const plusBtn = e.target.closest('.cam-qty-plus');
    const minusBtn = e.target.closest('.cam-qty-minus');
    if (!plusBtn && !minusBtn) return;

    const row = e.target.closest('.cam-device-row');
    if (!row) return;
    
    // Determine which input to target based on data-target attribute
    const targetType = plusBtn ? plusBtn.dataset.target : minusBtn.dataset.target;
    const input = row.querySelector(`.cam-qty-${targetType}`);
    if (!input) return;

    let current = parseInt(input.value, 10);
    if (isNaN(current) || current < 0) current = 0;

    if (plusBtn) {
        current++;
        input.value = current;
        updateExtraSection(input);
    }
    if (minusBtn) {
        if (current > 0) current--;
        input.value = current;
        updateExtraSection(input);
    }
});

// Handle manual input changes
document.addEventListener('input', function (e) {
    if (e.target.classList.contains('cam-qty-input')) {
        let val = parseInt(e.target.value, 10);
        if (isNaN(val) || val < 0) val = 0;
        e.target.value = val;
        updateExtraSection(e.target);
    }
});

// Update extra section visibility based on quantities
function updateExtraSection(input) {
    if (!input) return;
    const card = input.closest('.card');
    const existInput = card.querySelector('.cam-qty-exist');
    const reqInput = card.querySelector('.cam-qty-req');
    const extraSection = card.querySelector('.cam-extra-section');
    
    if (extraSection) {
        const existQty = parseInt(existInput?.value, 10) || 0;
        const reqQty = parseInt(reqInput?.value, 10) || 0;
        // Show extra section if either quantity is greater than 0
        extraSection.style.display = (existQty > 0 || reqQty > 0) ? 'block' : 'none';
    }
}

// Camera and Gallery logic
let currentFacingMode = 'environment'; // Default to back camera

document.addEventListener('click', function (e) {
    // Take Photo
    if (e.target.closest('.cam-take-photo-btn')) {
        const btn = e.target.closest('.cam-take-photo-btn');
        const section = btn.closest('.cam-extra-section');
        const preview = section.querySelector('.cam-preview');
        const modal = new bootstrap.Modal(document.getElementById('camDeviceVideoModal'));
        const video = document.getElementById('camDeviceCaptureVideo');
        const captureBtn = document.getElementById('camDeviceCaptureBtn');
        const flipBtn = document.getElementById('camFlipBtn');
        let stream = null;
        video.srcObject = null;
        
        // Function to start camera with current facing mode
        async function startCamera() {
            try {
                // Stop existing stream if any
                if (stream) {
                    stream.getTracks().forEach(track => track.stop());
                }
                
                const constraints = {
                    video: { 
                        facingMode: currentFacingMode,
                        width: { ideal: 1920 },
                        height: { ideal: 1080 }
                    }
                };
                
                stream = await navigator.mediaDevices.getUserMedia(constraints);
                video.srcObject = stream;
            } catch (err) {
                console.error('Camera error:', err);
                let errorMessage = 'Camera access denied or not available.';
                
                // Provide specific error messages
                if (err.name === 'NotAllowedError' || err.name === 'PermissionDeniedError') {
                    errorMessage = 'Camera permission denied. Please allow camera access in your browser settings.';
                } else if (err.name === 'NotFoundError' || err.name === 'DevicesNotFoundError') {
                    errorMessage = 'No camera found on this device.';
                } else if (err.name === 'NotReadableError' || err.name === 'TrackStartError') {
                    errorMessage = 'Camera is already in use by another application.';
                } else if (err.name === 'OverconstrainedError') {
                    errorMessage = 'Camera does not support the requested settings.';
                } else if (err.name === 'NotSupportedError') {
                    errorMessage = 'Camera not supported. HTTPS connection required for camera access.';
                } else if (err.name === 'TypeError') {
                    errorMessage = 'Camera not supported. Please use HTTPS connection or enable camera permissions.';
                }
                
                alert(errorMessage);
                modal.hide();
            }
        }
        
        // Check for secure context (HTTPS, localhost, or file://)
        const isSecureContext = window.isSecureContext || location.protocol === 'https:' || location.hostname === 'localhost' || location.hostname === '127.0.0.1';
        
        if (!isSecureContext) {
            alert('Camera access requires HTTPS connection.\n\nCurrent URL: ' + location.protocol + '//' + location.host + '\n\nPlease access the application via HTTPS or localhost for camera functionality.');
            return;
        }
        
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            alert('Camera not supported. Your browser or device does not support camera access.\n\nPlease use a modern browser (Chrome, Firefox, Edge, Safari) and ensure camera permissions are enabled.');
            return;
        }
        
        startCamera();
        modal.show();
            
        // Flip camera button handler
        flipBtn.onclick = function() {
            currentFacingMode = currentFacingMode === 'environment' ? 'user' : 'environment';
            startCamera();
        };
        
        captureBtn.onclick = function () {
            let canvas = document.createElement('canvas');
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            const ctx = canvas.getContext('2d');
            
            // Draw video frame
            ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
            
            // Add watermark if text is provided
            const watermarkText = document.getElementById('camWatermarkText').value.trim();
            if (watermarkText) {
                applyWatermark(ctx, watermarkText);
            }
            
            // Upload to Cloudinary with folder structure
            const itemIndex = preview.dataset.itemIndex;
            const itemId = document.querySelector(`input.item-id-field[data-index=\"${itemIndex}\"]`)?.value || '0';
            uploadToCloudinary(canvas.toDataURL('image/png'), preview, itemId);
            
            // Clear watermark input for next capture
            document.getElementById('camWatermarkText').value = '';
            
            modal.hide();
            if (stream) stream.getTracks().forEach(track => track.stop());
        };
        
        document.getElementById('camDeviceVideoModal').addEventListener('hidden.bs.modal', function () {
            if (stream) stream.getTracks().forEach(track => track.stop());
        }, { once: true });
    }
    
    // Gallery Upload
    if (e.target.closest('.cam-gallery-btn')) {
        const btn = e.target.closest('.cam-gallery-btn');
        const section = btn.closest('.cam-extra-section');
        const uploadInput = section.querySelector('.cam-upload-input');
        const preview = section.querySelector('.cam-preview');
        uploadInput.click();
        uploadInput.onchange = async function () {
            const itemIndex = preview.dataset.itemIndex;
            const itemId = document.querySelector(`input.item-id-field[data-index="${itemIndex}"]`)?.value || '0';
            
            // Ask for watermark once for all images
            const watermarkText = prompt('Enter watermark text for all selected images (optional, leave empty for no watermark):');
            
            if (watermarkText !== null) { // User didn't cancel
                const files = Array.from(uploadInput.files).filter(file => file.type.startsWith('image/'));
                
                if (files.length === 0) return;
                
                // Show progress indicator
                const progressWrapper = document.createElement('div');
                progressWrapper.className = 'cam-preview-wrapper position-relative d-inline-block';
                progressWrapper.innerHTML = `<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Processing...</span></div><div class="small text-center mt-2">Processing 0/${files.length}</div>`;
                preview.appendChild(progressWrapper);
                
                let completedCount = 0;
                const updateProgress = () => {
                    completedCount++;
                    progressWrapper.querySelector('div.small').textContent = `Uploading ${completedCount}/${files.length}`;
                };
                
                // Process images in parallel batches of 3 for optimal performance
                const batchSize = 3;
                for (let i = 0; i < files.length; i += batchSize) {
                    const batch = files.slice(i, i + batchSize);
                    const uploadPromises = batch.map(file => {
                        return new Promise((resolve) => {
                            let reader = new FileReader();
                            reader.onload = async function (e) {
                                try {
                                    if (watermarkText.trim()) {
                                        // Add watermark to gallery image
                                        addWatermarkToImage(e.target.result, watermarkText.trim(), async function(watermarkedImage) {
                                            await uploadToCloudinary(watermarkedImage, preview, itemId);
                                            updateProgress();
                                            resolve();
                                        });
                                    } else {
                                        // No watermark, upload as-is
                                        await uploadToCloudinary(e.target.result, preview, itemId);
                                        updateProgress();
                                        resolve();
                                    }
                                } catch (error) {
                                    console.error('Error processing image:', error);
                                    updateProgress();
                                    resolve();
                                }
                            };
                            reader.readAsDataURL(file);
                        });
                    });
                    
                    // Wait for current batch to complete before processing next batch
                    await Promise.all(uploadPromises);
                }
                
                // Remove progress indicator
                progressWrapper.remove();
            }
            uploadInput.value = ''; // Clear input for next use
        };
    }
});

// Function to compress image before upload
function compressImage(base64Image, maxWidth = 1920, quality = 0.85) {
    return new Promise((resolve) => {
        const img = new Image();
        img.onload = function() {
            const canvas = document.createElement('canvas');
            let width = img.width;
            let height = img.height;
            
            // Resize if image is too large
            if (width > maxWidth) {
                height = (height * maxWidth) / width;
                width = maxWidth;
            }
            
            canvas.width = width;
            canvas.height = height;
            
            const ctx = canvas.getContext('2d');
            ctx.drawImage(img, 0, 0, width, height);
            
            // Compress to JPEG for smaller file size
            resolve(canvas.toDataURL('image/jpeg', quality));
        };
        img.src = base64Image;
    });
}

// Function to upload image to Cloudinary with compression
async function uploadToCloudinary(base64Image, previewContainer, itemId) {
    try {
        // Show loading indicator
        const loadingWrapper = document.createElement('div');
        loadingWrapper.className = 'cam-preview-wrapper position-relative d-inline-block';
        loadingWrapper.innerHTML = '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Uploading...</span></div>';
        previewContainer.appendChild(loadingWrapper);

        // Compress image before upload
        const compressedImage = await compressImage(base64Image);

        // Create hierarchical folder structure: survey-{id}/location-{id}/item-{id}
        const folder = `survey-${surveyId}/location-${locId}/item-${itemId}`;

        const response = await fetch(cloudinaryUploadUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                base64Image: compressedImage,
                folder: folder
            })
        });

        const result = await response.json();
        
        // Remove loading indicator
        loadingWrapper.remove();

        if (result.success) {
            // Create image preview with Cloudinary URL
            createImagePreview(result.url, result.publicId, previewContainer);
        } else {
            alert('Upload failed: ' + result.message);
        }
    } catch (error) {
        console.error('Error uploading to Cloudinary:', error);
        alert('Error uploading image. Please try again.');
    }
}

// Function to create image preview
function createImagePreview(imageUrl, publicId, previewContainer) {
    const itemIndex = previewContainer.dataset.itemIndex;
    const itemType = previewContainer.dataset.itemType;
    
    let wrapper = document.createElement('div');
    wrapper.className = 'cam-preview-wrapper position-relative d-inline-block';
    wrapper.dataset.publicId = publicId;
    
    let img = document.createElement('img');
    img.src = imageUrl;
    img.className = 'cam-preview-img';
    img.style.objectFit = 'contain'; // Ensure full image is visible
    img.style.background = '#f8f9fa';
    wrapper.appendChild(img);
    
    // Add hidden input to store URL for form submission with proper naming
    let hiddenInput = document.createElement('input');
    hiddenInput.type = 'hidden';
    hiddenInput.name = `ItemLists[${itemIndex}].CloudinaryUrls`;
    hiddenInput.value = imageUrl;
    wrapper.appendChild(hiddenInput);

    let hiddenPublicId = document.createElement('input');
    hiddenPublicId.type = 'hidden';
    hiddenPublicId.name = `ItemLists[${itemIndex}].CloudinaryPublicIds`;
    hiddenPublicId.value = publicId;
    wrapper.appendChild(hiddenPublicId);
    
    let previewBtn = document.createElement('button');
    previewBtn.type = 'button';
    previewBtn.className = 'btn btn-sm btn-info position-absolute cam-preview-btn';
    previewBtn.innerHTML = '<i class="bi bi-eye"></i>';
    previewBtn.style.top = '2px';
    previewBtn.style.right = '42px';
    previewBtn.dataset.url = imageUrl;
    wrapper.appendChild(previewBtn);

    let cancelBtn = document.createElement('button');
    cancelBtn.type = 'button'; // Prevent form submission
    cancelBtn.className = 'btn btn-sm btn-danger position-absolute';
    cancelBtn.innerHTML = '<i class="bi bi-x"></i>';
    cancelBtn.style.top = '2px'; // Align with preview button
    cancelBtn.style.right = '2px'; // Position at far right
    cancelBtn.onclick = async function (e) {
        e.preventDefault(); // Prevent any default behavior
        e.stopPropagation(); // Stop event bubbling
        
        // Delete from Cloudinary
        try {
            await fetch(cloudinaryDeleteUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ publicId: publicId })
            });
        } catch (error) {
            console.error('Error deleting from Cloudinary:', error);
        }
        wrapper.remove();
    };
    wrapper.appendChild(cancelBtn);
    
    previewContainer.appendChild(wrapper);
}

// Handle deletion of existing (pre-loaded) images
document.addEventListener('click', function(e) {
    if (e.target.closest('.cam-delete-existing')) {
        const btn = e.target.closest('.cam-delete-existing');
        const wrapper = btn.closest('.cam-preview-wrapper');
        const publicId = btn.dataset.publicId;
        
        if (confirm('Are you sure you want to delete this image?')) {
            // Delete from Cloudinary
            if (publicId) {
                fetch(cloudinaryDeleteUrl, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ publicId: publicId })
                }).then(response => response.json())
                .then(result => {
                    if (result.success) {
                        wrapper.remove();
                    } else {
                        console.error('Failed to delete image:', result.message);
                        alert('Failed to delete image from cloud storage.');
                    }
                })
                .catch(error => {
                    console.error('Error deleting from Cloudinary:', error);
                    // Still remove from UI even if delete fails
                    wrapper.remove();
                });
            } else {
                // No public ID, just remove from UI
                wrapper.remove();
            }
        }
    }
});

// Helper function to add watermark to an image from gallery
function addWatermarkToImage(base64Image, watermarkText, callback) {
    const img = new Image();
    img.onload = function() {
        const canvas = document.createElement('canvas');
        canvas.width = img.width;
        canvas.height = img.height;
        const ctx = canvas.getContext('2d');
        
        // Draw original image
        ctx.drawImage(img, 0, 0);
        
        // Apply the watermark
        if (watermarkText) {
            applyWatermark(ctx, watermarkText);
        }
        
        // Return watermarked image as base64
        callback(canvas.toDataURL('image/png'));
    };
    img.src = base64Image;
}

// Handle preview button click
document.addEventListener('click', function(e) {
    const previewBtn = e.target.closest('.cam-preview-btn');
    if (previewBtn) {
        const imageUrl = previewBtn.dataset.url;
        const modal = new bootstrap.Modal(document.getElementById('imagePreviewModal'));
        const previewImage = document.getElementById('previewImage');
        
        if (imageUrl && previewImage) {
            previewImage.src = imageUrl;
            modal.show();
        }
    }
});

// Add styles for preview button hover effect
const style = document.createElement('style');
style.innerHTML = `
    .cam-preview-wrapper:hover .cam-preview-btn {
        display: block !important;
    }
    .cam-preview-btn {
        display: none;
    }
`;
document.head.appendChild(style);

document.getElementById('cameraDevicesForm').addEventListener('submit', function(e) {
    let warningMessage = '';
    const items = document.querySelectorAll('.card.shadow-sm'); // Each item is in a card

    items.forEach((item, index) => {
        const urlInputs = item.querySelectorAll(`input[name="ItemLists[${index}].CloudinaryUrls"]`);
        if (urlInputs.length > 0) {
            const urls = Array.from(urlInputs).map(input => input.value);
            const urlString = urls.join(',');
            
            // Check length against a reasonable threshold
            if (urlString.length > 1000) {
                const itemName = item.querySelector('h6.fw-bold').textContent || `Item #${index + 1}`;
                warningMessage += `Warning: Item "${itemName}" has ${urlInputs.length} images, and the total URL length (${urlString.length} characters) is very long. This may cause data to be truncated when saving. Please consider reducing the number of images or contacting your system administrator to increase the database field size.\n\n`;
            }
        }
    });

    if (warningMessage) {
        alert("Potential Data Length Issue\n--------------------------------\n" + warningMessage);
    }
});

