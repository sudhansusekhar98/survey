/**
 * Camera Remarks Management - Inline Version
 * Handles camera installation remarks for NEW cameras only
 * Shows inline input fields instead of modal popup
 */

// Global variables
let cameraRemarksData = {}; // Store remarks per item index
let previousNewCameraQty = {}; // Track previous new camera quantities

/**
 * Initialize camera remarks functionality
 */
function initCameraRemarks() {
    console.log('Camera Remarks Module Initialized (Inline Version)');
    
    // Listen to NEW camera quantity changes to dynamically show/hide input fields
    document.addEventListener('input', function(e) {
        if (e.target.classList.contains('cam-qty-req') && e.target.dataset.type === 'req') {
            const itemIndexAttr = e.target.dataset.index;
            
            if (itemIndexAttr !== undefined) {
                const itemIndex = parseInt(itemIndexAttr);
                
                // Check if this is a camera item using the hidden field
                const isCameraField = document.querySelector(`.is-camera-item[data-index="${itemIndex}"]`);
                
                if (isCameraField && isCameraField.value === 'true') {
                    const newQty = parseInt(e.target.value || 0);
                    
                    console.log('🎥 Camera NEW qty changed for index', itemIndex, '- New qty:', newQty);
                    
                    // Show remarks inputs when new camera quantity > 0
                    if (newQty > 0) {
                        console.log('✅ Showing camera remarks inputs for', newQty, 'new cameras');
                        // Generate inline input fields for new cameras
                        generateInlineCameraInputs(itemIndex, newQty);
                    } else {
                        console.log('❌ Hiding camera remarks (new qty = 0)');
                        // Hide remarks section if no new cameras
                        const container = document.getElementById(`cameraRemarksInputs_${itemIndex}`);
                        if (container) {
                            container.innerHTML = '';
                            container.style.display = 'none';
                        }
                    }
                    
                    // Update tracking
                    previousNewCameraQty[itemIndex] = newQty;
                }
            }
        }
        
        // Listen to EXISTING camera quantity changes to hide remarks if existing qty > 0
        // We don't need to listen to existing qty changes for camera remarks
        // Camera remarks are only for NEW cameras being deployed
    });
    
    // Initialize previous quantities tracking and load existing remarks
    document.querySelectorAll('.cam-qty-req[data-type="req"]').forEach(input => {
        const itemIndex = parseInt(input.dataset.index);
        const currentQty = parseInt(input.value || 0);
        previousNewCameraQty[itemIndex] = currentQty;
        
        // Check if this is a camera and generate inputs if new quantity exists
        const isCameraField = document.querySelector(`.is-camera-item[data-index="${itemIndex}"]`);
        if (isCameraField && isCameraField.value === 'true' && currentQty > 0) {
            console.log('🔄 Initializing camera remarks for index', itemIndex, 'with', currentQty, 'new cameras');
            // Load existing remarks
            loadExistingCameraRemarks(itemIndex);
            // Generate inputs
            generateInlineCameraInputs(itemIndex, currentQty);
        }
    });
}

/**
 * Generate inline input fields for camera installation remarks
 */
function generateInlineCameraInputs(itemIndex, newQty) {
    const container = document.getElementById(`cameraRemarksInputs_${itemIndex}`);
    if (!container) {
        console.warn('⚠️ Container not found for index:', itemIndex);
        return;
    }
    
    console.log(`📝 Generating ${newQty} camera remark inputs for index ${itemIndex}`);
    
    // Clear existing inputs
    container.innerHTML = '';
    
    if (newQty === 0) {
        // No cameras, hide container
        container.style.display = 'none';
        // Clear the hidden field
        const hiddenField = document.querySelector(`.camera-remarks-json[data-item-index="${itemIndex}"]`);
        if (hiddenField) {
            hiddenField.value = '';
        }
        cameraRemarksData[itemIndex] = [];
        console.log('✓ Container hidden (qty = 0)');
        return;
    }
    
    container.style.display = 'block';
    console.log('✓ Container visible');
    
    // Get existing remarks if any
    const existingRemarks = cameraRemarksData[itemIndex] || [];
    
    // Generate input fields
    for (let i = 1; i <= newQty; i++) {
        const remarkValue = existingRemarks[i - 1] || '';
        
        const inputHtml = `
            <div class="mb-2">
                <label class="form-label small mb-1">
                    <i class="bi bi-camera-fill text-info me-1"></i>
                    <strong>#Cam${i} Remarks:</strong>
                </label>
                <input type="text" 
                       class="form-control form-control-sm camera-remark-input" 
                       data-item-index="${itemIndex}"
                       data-camera-num="${i}" 
                       placeholder="e.g., Towards main entrance, North side wall, Parking area, etc."
                       value="${remarkValue}" />
            </div>
        `;
        
        container.insertAdjacentHTML('beforeend', inputHtml);
    }
    
    console.log(`✓ Created ${newQty} input fields`);
    
    // Attach change event listeners to update hidden field
    container.querySelectorAll('.camera-remark-input').forEach(input => {
        input.addEventListener('blur', function() {
            updateCameraRemarksHiddenField(itemIndex);
        });
        
        input.addEventListener('change', function() {
            updateCameraRemarksHiddenField(itemIndex);
        });
    });
    
    console.log('✓ Event listeners attached');
}

/**
 * Update hidden field with camera remarks JSON
 */
function updateCameraRemarksHiddenField(itemIndex) {
    const container = document.getElementById(`cameraRemarksInputs_${itemIndex}`);
    if (!container) return;
    
    const inputs = container.querySelectorAll('.camera-remark-input');
    const remarks = [];
    
    inputs.forEach(input => {
        remarks.push(input.value.trim());
    });
    
    // Store in memory
    cameraRemarksData[itemIndex] = remarks;
    
    // Update hidden field
    const hiddenField = document.querySelector(`.camera-remarks-json[data-item-index="${itemIndex}"]`);
    if (hiddenField) {
        hiddenField.value = JSON.stringify(remarks);
        console.log('Updated camera remarks for index', itemIndex, ':', remarks);
    }
}

/**
 * Load existing camera remarks from hidden field
 */
function loadExistingCameraRemarks(itemIndex) {
    const hiddenField = document.querySelector(`.camera-remarks-json[data-item-index="${itemIndex}"]`);
    
    if (hiddenField && hiddenField.value) {
        try {
            const remarks = JSON.parse(hiddenField.value);
            cameraRemarksData[itemIndex] = remarks;
            console.log('Loaded existing remarks for index', itemIndex, ':', remarks);
        } catch (e) {
            console.error('Error parsing camera remarks JSON for index', itemIndex, ':', e);
        }
    }
}

// Initialize on document ready
document.addEventListener('DOMContentLoaded', function() {
    initCameraRemarks();
});

// Export for external access
window.cameraRemarksModule = {
    initCameraRemarks,
    cameraRemarksData
};
