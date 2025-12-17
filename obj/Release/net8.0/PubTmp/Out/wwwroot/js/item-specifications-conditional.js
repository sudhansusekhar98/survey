/**
 * Item Specifications with Conditional Display
 * Handles pole owner (ExistingQty > 0) and pole height (RequiredQty > 0) with multiple instances
 */

(function() {
    'use strict';

    const specConfig = {
        getSpecificationsUrl: '/SurveyDetails/GetItemSpecifications',
        getDetailsUrl: '/SurveyDetails/GetSpecificationDetails'
    };

    /**
     * Initialize specifications for all items on page load
     */
    function initializeAllSpecifications() {
        const containers = document.querySelectorAll('.item-specifications-container');
        containers.forEach(container => {
            const itemId = parseInt(container.dataset.itemId);
            const itemIndex = container.dataset.itemIndex;
            const existingQty = parseInt(container.dataset.existingQty) || 0;
            const requiredQty = parseInt(container.dataset.requiredQty) || 0;

            if (itemId) {
                loadAndRenderSpecifications(container, itemId, itemIndex, existingQty, requiredQty);
            }
        });
    }

    /**
     * Load specifications for an item and render based on conditional rules
     */
    async function loadAndRenderSpecifications(container, itemId, itemIndex, existingQty, requiredQty) {
        try {
            const response = await fetch(`${specConfig.getSpecificationsUrl}?itemId=${itemId}`);
            const data = await response.json();

            if (!data.success || !data.specifications || data.specifications.length === 0) {
                container.style.display = 'none';
                return;
            }

            const specifications = data.specifications;
            const surveyId = document.querySelector('input[name="SurveyID"]')?.value;
            const locId = document.querySelector('input[name="LocID"]')?.value;

            // Get saved values
            let savedValues = {};
            if (surveyId && locId) {
                try {
                    const detailsResponse = await fetch(`${specConfig.getDetailsUrl}?surveyId=${surveyId}&locId=${locId}&itemId=${itemId}`);
                    const detailsData = await detailsResponse.json();
                    if (detailsData.success && detailsData.specifications) {
                        detailsData.specifications.forEach(spec => {
                            const key = `${spec.specificationID}_${spec.instanceNumber || 1}`;
                            savedValues[key] = spec.specificationDetails;
                        });
                    }
                } catch (e) {
                    console.log('Could not load saved details:', e);
                }
            }

            // Render specifications based on conditional display rules
            renderConditionalSpecifications(container, specifications, itemIndex, existingQty, requiredQty, savedValues);

        } catch (error) {
            console.error('Error loading specifications:', error);
            container.style.display = 'none';
        }
    }

    /**
     * Render specifications with conditional display logic
     */
    function renderConditionalSpecifications(container, specifications, itemIndex, existingQty, requiredQty, savedValues) {
        let html = '<div class="row g-2">';
        let hasVisibleSpecs = false;

        specifications.forEach(spec => {
            const shouldShow = checkConditionalDisplay(spec.conditionalDisplay, existingQty, requiredQty);
            if (!shouldShow) return;

            hasVisibleSpecs = true;

            if (spec.allowMultipleInstances) {
                // Render multiple instances based on quantity
                const instanceCount = spec.conditionalDisplay === 'ExistingQtyOnly' ? existingQty : 
                                    spec.conditionalDisplay === 'RequiredQtyOnly' ? requiredQty : 
                                    Math.max(existingQty, requiredQty);

                for (let instance = 1; instance <= instanceCount; instance++) {
                    const key = `${spec.specificationID}_${instance}`;
                    const savedValue = savedValues[key] || '';
                    html += renderSpecificationInput(spec, itemIndex, instance, savedValue, instanceCount);
                }
            } else {
                // Single instance
                const key = `${spec.specificationID}_1`;
                const savedValue = savedValues[key] || '';
                html += renderSpecificationInput(spec, itemIndex, 1, savedValue, 1);
            }
        });

        html += '</div>';

        if (hasVisibleSpecs) {
            container.innerHTML = html;
            container.style.display = 'block';
        } else {
            container.style.display = 'none';
        }
    }

    /**
     * Check if specification should be displayed based on conditional rule
     */
    function checkConditionalDisplay(conditionalDisplay, existingQty, requiredQty) {
        if (!conditionalDisplay || conditionalDisplay === 'Always') return true;
        if (conditionalDisplay === 'ExistingQtyOnly') return existingQty > 0;
        if (conditionalDisplay === 'RequiredQtyOnly') return requiredQty > 0;
        if (conditionalDisplay === 'BothQty') return existingQty > 0 && requiredQty > 0;
        return false;
    }

    /**
     * Render a single specification input field
     */
    function renderSpecificationInput(spec, itemIndex, instanceNumber, savedValue, totalInstances) {
        const fieldId = `spec_${itemIndex}_${spec.specificationID}_${instanceNumber}`;
        const fieldName = `ItemSpecs_${itemIndex}_${spec.specificationID}_${instanceNumber}`;
        
        const label = totalInstances > 1 
            ? `${spec.specificationName} #${instanceNumber}` 
            : spec.specificationName;

        let inputHtml = '';

        if (spec.inputType === 'dropdown' && spec.options) {
            // Render dropdown from database options
            const options = spec.options.split(',').map(o => o.trim()).filter(Boolean);
            inputHtml = `
                <select id="${fieldId}" name="${fieldName}" class="form-select form-select-sm spec-input"
                    data-spec-id="${spec.specificationID}" data-instance="${instanceNumber}">
                    <option value="">-- Select ${spec.specificationName} --</option>
                    ${options.map(opt => {
                        const isSelected = savedValue === opt ? 'selected' : '';
                        return `<option value="${escapeHtml(opt)}" ${isSelected}>${escapeHtml(opt)}</option>`;
                    }).join('')}
                </select>
            `;
        } else if (spec.inputType === 'number') {
            inputHtml = `
                <input type="number" id="${fieldId}" name="${fieldName}" 
                    class="form-control form-control-sm spec-input"
                    data-spec-id="${spec.specificationID}" data-instance="${instanceNumber}"
                    value="${escapeHtml(savedValue)}" 
                    placeholder="Enter ${spec.specificationName}">
            `;
        } else {
            // Default: text input
            inputHtml = `
                <input type="text" id="${fieldId}" name="${fieldName}" 
                    class="form-control form-control-sm spec-input"
                    data-spec-id="${spec.specificationID}" data-instance="${instanceNumber}"
                    value="${escapeHtml(savedValue)}" 
                    placeholder="Enter ${spec.specificationName}">
            `;
        }

        return `
            <div class="col-12 col-md-6">
                <div class="specification-field mb-2">
                    <label class="form-label small mb-1 fw-semibold text-secondary" for="${fieldId}">
                        <i class="bi bi-${getIconForSpec(spec.specificationName)} me-1"></i>${escapeHtml(label)}
                    </label>
                    ${inputHtml}
                </div>
            </div>
        `;
    }

    /**
     * Get Bootstrap icon class for specification
     */
    function getIconForSpec(specName) {
        const name = (specName || '').toLowerCase();
        if (name.includes('owner')) return 'building';
        if (name.includes('height')) return 'arrows-vertical';
        if (name.includes('width') || name.includes('road')) return 'signpost-2';
        return 'list-check';
    }

    /**
     * Escape HTML to prevent XSS
     */
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text || '';
        return div.innerHTML;
    }

    /**
     * Update specifications when quantity changes
     */
    function setupQuantityChangeListeners() {
        document.querySelectorAll('.cam-qty-input').forEach(input => {
            input.addEventListener('change', function() {
                const card = this.closest('.card.shadow-sm');
                if (!card) return;

                const existInput = card.querySelector('.cam-qty-exist');
                const reqInput = card.querySelector('.cam-qty-req');
                const existingQty = parseInt(existInput?.value || 0);
                const requiredQty = parseInt(reqInput?.value || 0);

                const specContainer = card.querySelector('.item-specifications-container');
                if (specContainer) {
                    const itemId = parseInt(specContainer.dataset.itemId);
                    const itemIndex = specContainer.dataset.itemIndex;
                    
                    // Update data attributes
                    specContainer.dataset.existingQty = existingQty;
                    specContainer.dataset.requiredQty = requiredQty;

                    // Reload specifications with new quantities
                    loadAndRenderSpecifications(specContainer, itemId, itemIndex, existingQty, requiredQty);
                }
            });
        });
    }

    /**
     * Initialize on DOM ready
     */
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            initializeAllSpecifications();
            setupQuantityChangeListeners();
        });
    } else {
        initializeAllSpecifications();
        setupQuantityChangeListeners();
    }

    // Expose globally if needed
    window.reloadItemSpecifications = function(itemIndex) {
        const container = document.querySelector(`#item-specifications-${itemIndex}`);
        if (container) {
            const itemId = parseInt(container.dataset.itemId);
            const existingQty = parseInt(container.dataset.existingQty) || 0;
            const requiredQty = parseInt(container.dataset.requiredQty) || 0;
            loadAndRenderSpecifications(container, itemId, itemIndex, existingQty, requiredQty);
        }
    };

})();
