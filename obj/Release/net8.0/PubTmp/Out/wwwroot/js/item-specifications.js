/**
 * Item Specifications Module
 * Handles loading and saving dynamic item specifications from ItemSpecificationMaster
 */

// Global configuration object
let specConfig = {
    getSpecificationsUrl: '',
    saveSpecificationsUrl: '',
    antiForgeryToken: ''
};

/**
 * Initialize the specifications module
 * @param {string} getUrl - URL to fetch specifications
 * @param {string} saveUrl - URL to save specifications
 * @param {string} token - Anti-forgery token
 */
function initItemSpecifications(getUrl, saveUrl, token) {
    specConfig.getSpecificationsUrl = getUrl;
    specConfig.saveSpecificationsUrl = saveUrl;
    specConfig.antiForgeryToken = token;
    
    console.log('Item Specifications Module Initialized:', {
        getUrl: getUrl,
        saveUrl: saveUrl,
        hasToken: !!token
    });
    
    // Load specifications for all items when the extra section becomes visible
    initSpecificationLoading();

    // Populate legacy pole fields (server-rendered selects) from specification definitions
    populatePoleAndHeightFields();
}

/**
 * Populate server-rendered pole owner/height selects from ItemSpecificationMaster options
 */
function populatePoleAndHeightFields() {
    try {
        const surveyInput = document.querySelector('input[name="SurveyID"]');
        const locInput = document.querySelector('input[name="LocID"]');
        const surveyId = surveyInput ? surveyInput.value : (document.querySelector('.item-specifications-section')?.dataset.surveyId || '');
        const locId = locInput ? locInput.value : (document.querySelector('.item-specifications-section')?.dataset.locId || '');

        // Helper to fetch specs for an item and populate a select when matching specName found
        const fetchAndPopulate = (itemId, selectEl, matchFn) => {
            if (!itemId || !selectEl) return;
            const url = `${specConfig.getSpecificationsUrl}?surveyId=${surveyId}&locId=${locId}&itemId=${itemId}`;
            fetch(url)
                .then(r => r.json())
                .then(data => {
                    if (!data || !data.success || !Array.isArray(data.specifications)) return;
                    const specs = data.specifications;
                    const spec = specs.find(s => matchFn((s.specificationName || '').toLowerCase()));
                    if (!spec || !spec.options) return;
                    const options = spec.options.split(',').map(o => o.trim()).filter(Boolean);
                    if (options.length === 0) return;

                    // Preserve current value if present
                    const current = selectEl.value;
                    selectEl.innerHTML = '<option value="">-- Select --</option>';
                    options.forEach(opt => {
                        const optEl = document.createElement('option');
                        optEl.value = opt;
                        optEl.textContent = opt;
                        if (current && current === opt) optEl.selected = true;
                        selectEl.appendChild(optEl);
                    });
                })
                .catch(err => console.error('Error populating select for item', itemId, err));
        };

        // Populate all pole owner selects
        document.querySelectorAll('.pole-owner-select').forEach(sel => {
            const itemInstance = sel.closest('.item-form-instance');
            const itemId = itemInstance?.dataset.itemId || sel.closest('[data-item-id]')?.dataset.itemId;
            fetchAndPopulate(itemId, sel, name => name.includes('owner'));
        });

        // Populate all pole height selects
        document.querySelectorAll('.pole-height-select').forEach(sel => {
            const itemInstance = sel.closest('.item-form-instance');
            const itemId = itemInstance?.dataset.itemId || sel.closest('[data-item-id]')?.dataset.itemId;
            fetchAndPopulate(itemId, sel, name => name.includes('height'));
        });
    } catch (e) {
        console.error('populatePoleAndHeightFields error:', e);
    }
}

/**
 * Initialize specification loading when item sections are shown
 */
function initSpecificationLoading() {
    // When quantity changes and section becomes visible, load specifications
    document.querySelectorAll('.cam-qty-input').forEach(input => {
        input.addEventListener('change', function() {
            const card = this.closest('.card.shadow-sm');
            if (!card) return;
            
            const extraSection = card.querySelector('.cam-extra-section');
            const specSection = card.querySelector('.item-specifications-section');
            
            if (extraSection && specSection) {
                const existQty = parseInt(card.querySelector('.cam-qty-exist')?.value) || 0;
                const reqQty = parseInt(card.querySelector('.cam-qty-req')?.value) || 0;
                
                if (existQty > 0 || reqQty > 0) {
                    extraSection.style.display = 'block';
                    loadItemSpecifications(specSection);
                } else {
                    extraSection.style.display = 'none';
                }
            }
        });
    });
    
    // Also load for already visible sections (on page load)
    // Use a slight delay to ensure DOM is ready
    setTimeout(() => {
        document.querySelectorAll('.item-specifications-section').forEach(specSection => {
            const extraSection = specSection.closest('.cam-extra-section');
            // Check if parent section is visible
            if (extraSection && extraSection.style.display !== 'none') {
                loadItemSpecifications(specSection);
            }
        });
    }, 500);
}

/**
 * Load specifications for an item
 * @param {HTMLElement} specSection - The specifications section element
 */
function loadItemSpecifications(specSection) {
    const itemId = specSection.dataset.itemId;
    const surveyId = specSection.dataset.surveyId;
    const locId = specSection.dataset.locId;
    const itemIndex = specSection.dataset.itemIndex;
    
    console.log('Loading specifications for item:', { itemId, surveyId, locId, itemIndex });
    
    if (!itemId || specSection.dataset.loaded === 'true') {
        console.log('Skipping - already loaded or no itemId');
        return; // Already loaded or no item ID
    }
    
    const loadingEl = specSection.querySelector('.specifications-loading');
    const inputsContainer = specSection.querySelector('.specifications-inputs');
    
    if (loadingEl) loadingEl.classList.remove('d-none');
    
    const url = `${specConfig.getSpecificationsUrl}?surveyId=${surveyId}&locId=${locId}&itemId=${itemId}`;
    console.log('Fetching specifications from:', url);
    
    // Fetch specifications with saved values
    fetch(url)
        .then(response => response.json())
        .then(data => {
            console.log('Specifications response:', data);
            if (loadingEl) loadingEl.classList.add('d-none');
            
            if (data.success && data.specifications && data.specifications.length > 0) {
                renderSpecificationInputs(inputsContainer, data.specifications, itemIndex, itemId);
                specSection.dataset.loaded = 'true';
            } else {
                console.log('No specifications found for this item');
            }
        })
        .catch(error => {
            console.error('Error loading specifications:', error);
            if (loadingEl) loadingEl.classList.add('d-none');
        });
}

/**
 * Render specification input fields
 * @param {HTMLElement} container - Container to render inputs into
 * @param {Array} specifications - Array of specification objects
 * @param {string} itemIndex - Index of the item in the form
 * @param {string} itemId - The item ID for the specifications
 */
function renderSpecificationInputs(container, specifications, itemIndex, itemId) {
    if (!container || !specifications || specifications.length === 0) {
        return;
    }
    
    console.log('Rendering specifications:', { itemIndex, itemId, count: specifications.length });
    
    let html = `
        <div class="specification-fields border-top pt-3 mt-2" data-item-id="${itemId}">
            <label class="form-label fw-semibold text-secondary mb-2">
                <i class="bi bi-list-check me-1"></i>Item Specifications
            </label>
            <div class="row g-2">
    `;
    
    specifications.forEach((spec, idx) => {
        const inputId = `spec_${itemIndex}_${spec.specificationID}`;
        const inputName = `ItemSpecs_${itemIndex}_${spec.specificationID}`;
        const savedValue = spec.specificationDetails || '';
        
        // Determine input type based on specification name or inputType field
        const inputType = determineInputType(spec.specificationName, spec.inputType);
        
        html += `
            <div class="col-12 col-md-6">
                <div class="specification-field" data-spec-id="${spec.specificationID}">
                    <label class="form-label small mb-1" for="${inputId}">
                        ${escapeHtml(spec.specificationName)}
                    </label>
        `;
        
        if (inputType === 'dropdown' && spec.options) {
            // Render dropdown
            const options = spec.options.split(',').map(o => o.trim());
            html += `<select class="form-select form-select-sm spec-input" 
                        id="${inputId}" 
                        name="${inputName}"
                        data-spec-id="${spec.specificationID}">
                        <option value="">-- Select --</option>`;
            options.forEach(opt => {
                const selected = savedValue === opt ? 'selected' : '';
                html += `<option value="${escapeHtml(opt)}" ${selected}>${escapeHtml(opt)}</option>`;
            });
            html += `</select>`;
        } else if (inputType === 'number') {
            // Render number input
            html += `<input type="number" 
                        class="form-control form-control-sm spec-input" 
                        id="${inputId}" 
                        name="${inputName}"
                        data-spec-id="${spec.specificationID}"
                        placeholder="Enter ${escapeHtml(spec.specificationName.toLowerCase())}..."
                        value="${escapeHtml(savedValue)}"
                        step="0.01" />`;
        } else {
            // Default: text input
            html += `<input type="text" 
                        class="form-control form-control-sm spec-input" 
                        id="${inputId}" 
                        name="${inputName}"
                        data-spec-id="${spec.specificationID}"
                        placeholder="Enter ${escapeHtml(spec.specificationName.toLowerCase())}..."
                        value="${escapeHtml(savedValue)}" />`;
        }
        
        html += `
                </div>
            </div>
        `;
    });
    
    html += `
            </div>
        </div>
    `;
    
    container.innerHTML = html;
}

/**
 * Determine the input type based on specification name or explicit inputType
 * @param {string} specName - Specification name
 * @param {string} inputType - Explicit input type if defined
 * @returns {string} - 'text', 'number', or 'dropdown'
 */
function determineInputType(specName, inputType) {
    if (inputType) {
        return inputType.toLowerCase();
    }
    
    const nameLower = (specName || '').toLowerCase();
    
    // Number fields
    if (nameLower.includes('width') || 
        nameLower.includes('height') || 
        nameLower.includes('length') ||
        nameLower.includes('count') ||
        nameLower.includes('quantity') ||
        nameLower.includes('size') ||
        nameLower.includes('distance')) {
        return 'number';
    }
    
    // Dropdown fields (common cases)
    if (nameLower.includes('owner') ||
        nameLower.includes('type') ||
        nameLower.includes('status')) {
        return 'text'; // Could be dropdown if options are provided
    }
    
    return 'text';
}

/**
 * Collect all specification values for an item
 * @param {HTMLElement} specSection - The specifications section
 * @returns {Array} - Array of {specificationID, specificationDetails, instanceNumber}
 */
function collectSpecificationValues(specSection) {
    const specs = [];
    const inputs = specSection.querySelectorAll('.spec-input');
    
    inputs.forEach(input => {
        const specId = parseInt(input.dataset.specId);
        const instanceNum = parseInt(input.dataset.instance) || 1; // Get instance number from data attribute
        const value = input.value.trim();
        
        if (specId) {
            specs.push({
                specificationID: specId,
                specificationDetails: value,
                instanceNumber: instanceNum // Include instance number in payload
            });
        }
    });
    
    console.log('Collected specifications:', specs);
    return specs;
}

/**
 * Save all specifications for an item
 * @param {long} surveyId - Survey ID
 * @param {int} locId - Location ID
 * @param {int} itemId - Item ID
 * @param {Array} specifications - Array of specification values
 * @returns {Promise}
 */
function saveItemSpecifications(surveyId, locId, itemId, specifications) {
    const payload = {
        surveyID: surveyId,
        locID: locId,
        itemID: itemId,
        specifications: specifications
    };
    
    console.log('Saving specifications:', payload);
    console.log('Save URL:', specConfig.saveSpecificationsUrl);
    
    return fetch(specConfig.saveSpecificationsUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
    })
    .then(response => {
        console.log('Save response status:', response.status);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        console.log('Save response data:', data);
        return data;
    })
    .catch(error => {
        console.error('Save error:', error);
        throw error;
    });
}

/**
 * Collect and save all specifications from the form
 * Called before form submission
 * @returns {Promise<boolean>}
 */
async function saveAllSpecifications() {
    console.log('saveAllSpecifications called');
    
    const specSections = document.querySelectorAll('.item-specifications-section[data-loaded="true"]');
    console.log('Found spec sections with data-loaded="true":', specSections.length);
    
    const promises = [];
    
    specSections.forEach(section => {
        const surveyId = parseInt(section.dataset.surveyId);
        const locId = parseInt(section.dataset.locId);
        const itemId = parseInt(section.dataset.itemId);
        const specs = collectSpecificationValues(section);
        
        console.log('Processing section:', { surveyId, locId, itemId, specsCount: specs.length });
        
        if (specs.length > 0) {
            promises.push(saveItemSpecifications(surveyId, locId, itemId, specs));
        }
    });
    
    console.log('Total save operations:', promises.length);
    
    if (promises.length === 0) {
        console.log('No specifications to save');
        return true;
    }
    
    try {
        const results = await Promise.all(promises);
        const allSuccess = results.every(r => r.success);
        return allSuccess;
    } catch (error) {
        console.error('Error saving specifications:', error);
        return false;
    }
}

/**
 * Escape HTML to prevent XSS
 * @param {string} text - Text to escape
 * @returns {string} - Escaped text
 */
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Expose functions globally
window.initItemSpecifications = initItemSpecifications;
window.loadItemSpecifications = loadItemSpecifications;
window.collectSpecificationValues = collectSpecificationValues;
window.saveAllSpecifications = saveAllSpecifications;
