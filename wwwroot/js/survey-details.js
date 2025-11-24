// Survey Details - JavaScript Module
// Handles preview modal, location submission, status updates, and unlock functionality

let currentSurveyId = 0;
let currentLocId = 0;

/**
 * Initialize the survey details page
 * @param {string} getPreviewUrl - URL for getting location preview
 * @param {string} submitCompletionUrl - URL for submitting location completion
 * @param {string} unlockUrl - URL for unlocking location
 * @param {boolean} isCompleted - Whether the location is completed
 * @param {string} antiForgeryToken - Anti-forgery token value
 */
function initSurveyDetails(getPreviewUrl, submitCompletionUrl, unlockUrl, isCompleted, antiForgeryToken) {
    // Store URLs in global scope for use by other functions
    window.surveyDetailsConfig = {
        getPreviewUrl: getPreviewUrl,
        submitCompletionUrl: submitCompletionUrl,
        unlockUrl: unlockUrl,
        antiForgeryToken: antiForgeryToken
    };

    // Disable item edit links if location is completed
    if (isCompleted) {
        $('a[href*="UpdateItem"]').each(function() {
            $(this).addClass('disabled').css('pointer-events', 'none').css('opacity', '0.5');
            $(this).attr('title', 'Location is completed. Unlock to edit.');
        });
    }
}

/**
 * Show the preview modal for a location
 * @param {number} surveyId - Survey ID
 * @param {number} locId - Location ID
 */
function showPreviewModal(surveyId, locId) {
    currentSurveyId = surveyId;
    currentLocId = locId;
    
    // Show modal
    var modal = new bootstrap.Modal(document.getElementById('previewModal'));
    modal.show();
    
    // Load preview data
    $.ajax({
        url: window.surveyDetailsConfig.getPreviewUrl,
        type: 'GET',
        data: { surveyId: surveyId, locId: locId },
        success: function(html) {
            $('#previewModalContent').html(html);
        },
        error: function(xhr, status, error) {
            $('#previewModalContent').html(
                '<div class="alert alert-danger"><i class="bi bi-exclamation-triangle me-2"></i>Failed to load preview. Please try again.</div>'
            );
        }
    });
}

/**
 * Submit location completion and mark as completed
 */
function submitLocationCompletion() {
    if (!confirm('Are you sure you want to submit and mark this location as completed? This action cannot be undone.')) {
        return;
    }
    
    $('#submitLocationBtn').prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Submitting...');
    
    $.ajax({
        url: window.surveyDetailsConfig.submitCompletionUrl,
        type: 'POST',
        data: { 
            surveyId: currentSurveyId, 
            locId: currentLocId,
            __RequestVerificationToken: window.surveyDetailsConfig.antiForgeryToken
        },
        success: function(response) {
            if (response.success) {
                // Close modal
                var modal = bootstrap.Modal.getInstance(document.getElementById('previewModal'));
                modal.hide();
                
                // Show success message
                var alertHtml = '<div class="alert alert-success alert-dismissible fade show" role="alert">' +
                    '<strong>Success!</strong> ' + response.message +
                    '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
                    '</div>';
                $('.card-body').prepend(alertHtml);
                
                // Reload the page to show updated status
                setTimeout(function() {
                    location.reload();
                }, 2000);
            } else {
                alert('Error: ' + response.message);
                $('#submitLocationBtn').prop('disabled', false).html('<i class="bi bi-check-circle me-2"></i>Submit & Mark as Completed');
            }
        },
        error: function(xhr, status, error) {
            alert('Failed to submit. Please try again.');
            $('#submitLocationBtn').prop('disabled', false).html('<i class="bi bi-check-circle me-2"></i>Submit & Mark as Completed');
        }
    });
}

/**
 * Unlock location for editing (changes status back to In Progress)
 * @param {number} surveyId - Survey ID
 * @param {number} locId - Location ID
 */
function unlockLocationForEditing(surveyId, locId) {
    if (!confirm('Are you sure you want to unlock this location for editing? This will change the status back to In Progress.')) {
        return;
    }
    
    $.ajax({
        url: window.surveyDetailsConfig.unlockUrl,
        type: 'POST',
        data: {
            surveyId: surveyId,
            locId: locId
        },
        success: function(response) {
            if (response.success) {
                alert('Success: ' + response.message);
                location.reload();
            } else {
                alert('Error: ' + response.message);
            }
        },
        error: function(xhr, status, error) {
            alert('Network error: ' + error);
        }
    });
}

/**
 * Handle location status updates (In Progress, Completed, Verified)
 * @param {number} surveyId - Survey ID
 * @param {number} locId - Location ID
 * @param {string} status - Status to update to (InProgress, Completed, Verified)
 */
function handleStatusUpdate(surveyId, locId, status) {
    const statusActions = {
        'InProgress': {
            url: '/SurveyCreation/MarkLocationAsInProgress',
            message: 'Mark this location as In Progress?'
        },
        'Completed': {
            url: '/SurveyCreation/MarkLocationAsCompleted',
            message: 'Mark this location as Completed?'
        },
        'Verified': {
            url: '/SurveyCreation/MarkLocationAsVerified',
            message: 'Mark this location as Verified?'
        }
    };
    
    const action = statusActions[status];
    if (!action) {
        alert('Invalid status action');
        return;
    }
    
    if (!confirm(action.message)) {
        return;
    }
    
    $.ajax({
        url: action.url,
        type: 'POST',
        data: {
            surveyId: surveyId,
            locId: locId,
            remarks: ''
        },
        success: function(response) {
            if (response.success) {
                alert('Success: ' + response.message);
                location.reload();
            } else {
                alert('Error: ' + response.message);
            }
        },
        error: function(xhr, status, error) {
            console.error('AJAX Error:', error);
            console.error('Status:', status);
            console.error('Response:', xhr.responseText);
            alert('Network error: Failed to update status. Please check console for details.');
        }
    });
}

// Initialize when document is ready
$(document).ready(function() {
    // Any additional initialization can go here
});
