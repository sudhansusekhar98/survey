/**
 * Camera Remarks Management - v2
 * This version provides a clear function to update the UI based on item quantity.
 */

// Global store for remarks data to persist it when inputs are re-rendered
let cameraRemarksDataStore = {};

/**
 * Main function to initialize or update the camera remarks UI for a specific item.
 * This is the single entry point called from other scripts.
 * @param {number} itemIndex The index of the item form.
 */
function updateCameraRemarksUI(itemIndex) {
    const itemForm = document.querySelector(`.item-form-instance[data-item-id-container="${itemIndex}"]`);
    if (!itemForm) return;

    const isCamera = itemForm.querySelector(`.is-camera-item[data-index="${itemIndex}"]`)?.value === 'true';
    if (!isCamera) return;

    const reqQtyInput = itemForm.querySelector(`.cam-qty-req[data-index="${itemIndex}"]`);
    const newQty = parseInt(reqQtyInput?.value || 0);

    const container = document.getElementById(`cameraRemarksInputs_${itemIndex}`);
    if (!container) return;

    // Load existing remarks from the hidden JSON field into the store if not already there
    if (cameraRemarksDataStore[itemIndex] === undefined) {
        const hiddenField = itemForm.querySelector(`.camera-remarks-json[data-item-index="${itemIndex}"]`);
        if (hiddenField && hiddenField.value) {
            try {
                cameraRemarksDataStore[itemIndex] = JSON.parse(hiddenField.value);
            } catch {
                cameraRemarksDataStore[itemIndex] = [];
            }
        } else {
            cameraRemarksDataStore[itemIndex] = [];
        }
    }

    generateInlineCameraInputs(itemIndex, newQty, container);
}

/**
 * Generates the individual text inputs for each camera's remarks.
 * @param {number} itemIndex The index of the item.
 * @param {number} quantity The number of inputs to generate.
 * @param {HTMLElement} container The container to fill with inputs.
 */
function generateInlineCameraInputs(itemIndex, quantity, container) {
    container.innerHTML = ''; // Clear previous inputs

    if (quantity === 0) {
        container.style.display = 'none';
        cameraRemarksDataStore[itemIndex] = [];
        updateHiddenJsonField(itemIndex);
        return;
    }

    container.style.display = 'block';
    const existingRemarks = cameraRemarksDataStore[itemIndex] || [];

    for (let i = 1; i <= quantity; i++) {
        const remarkValue = existingRemarks[i - 1] || '';
        const inputHtml = `
            <div class="mb-2">
                <label class="form-label small mb-1">
                    <i class="bi bi-camera-fill text-info me-1"></i>
                    <strong>#Cam${i} Installation Remarks:</strong>
                </label>
                <input type="text"
                       class="form-control form-control-sm camera-remark-input"
                       data-item-index="${itemIndex}"
                       data-camera-num="${i}"
                       placeholder="e.g., Towards main entrance, North-facing wall"
                       value="${remarkValue}"
                       oninput="handleRemarkInputChange(${itemIndex}, ${i - 1}, this.value)">
            </div>
        `;
        container.insertAdjacentHTML('beforeend', inputHtml);
    }
}

/**
 * Called when a remark input changes. Updates the in-memory store.
 * @param {number} itemIndex The index of the item.
 * @param {number} remarkIndex The index of the remark within the item.
 * @param {string} value The new value of the input.
 */
function handleRemarkInputChange(itemIndex, remarkIndex, value) {
    if (cameraRemarksDataStore[itemIndex]) {
        cameraRemarksDataStore[itemIndex][remarkIndex] = value.trim();
        updateHiddenJsonField(itemIndex);
    }
}

/**
 * Updates the hidden input field with the JSON stringified remarks from the store.
 * @param {number} itemIndex The index of the item.
 */
function updateHiddenJsonField(itemIndex) {
    const hiddenField = document.querySelector(`.camera-remarks-json[data-item-index="${itemIndex}"]`);
    if (hiddenField) {
        const remarks = cameraRemarksDataStore[itemIndex] || [];
        hiddenField.value = JSON.stringify(remarks.filter(r => r)); // Filter out empty strings
    }
}

/**
 * A function to be called to initialize the remarks UI for all items on the page.
 * This is useful for initial page load.
 */
function initializeAllCameraRemarks() {
    const itemForms = document.querySelectorAll('.item-form-instance');
    itemForms.forEach(form => {
        const itemIndex = form.querySelector('.is-camera-item')?.dataset.index;
        if (itemIndex !== undefined) {
            updateCameraRemarksUI(parseInt(itemIndex));
        }
    });
}