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
    // Disable button and show loading
    $('#submitLocationBtn').prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Checking...');
    
    // First, check validation via AJAX without confirmation
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
                // Validation passed, close modal and show success
                var modal = bootstrap.Modal.getInstance(document.getElementById('previewModal'));
                modal.hide();
                
                Swal.fire({
                    icon: 'success',
                    title: 'Success!',
                    text: response.message,
                    confirmButtonColor: '#28a745',
                    timer: 2000,
                    timerProgressBar: true
                }).then(() => {
                    location.reload();
                });
            } else {
                // Validation failed, show error in modal
                var errorHtml = '<div class="alert alert-warning mb-0">';
                errorHtml += '<strong><i class="bi bi-exclamation-triangle me-2"></i>' + response.message + '</strong>';
                
                if (response.errorDetails) {
                    errorHtml += '<div class="mt-3">' + response.errorDetails + '</div>';
                }
                
                errorHtml += '</div>';
                
                Swal.fire({
                    icon: 'warning',
                    title: 'Cannot Submit',
                    html: errorHtml,
                    confirmButtonText: 'OK, I\'ll Add Items',
                    confirmButtonColor: '#ffc107',
                    width: '600px'
                });
                
                // Re-enable submit button
                $('#submitLocationBtn').prop('disabled', false).html('<i class="bi bi-check-circle me-2"></i>Submit & Mark as Completed');
            }
        },
        error: function(xhr, status, error) {
            Swal.fire({
                icon: 'error',
                title: 'Submission Failed',
                text: 'Unable to submit. Please try again.',
                confirmButtonColor: '#dc3545'
            });
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
    Swal.fire({
        title: 'Unlock Location?',
        html: 'Are you sure you want to unlock this location for editing?<br><small class="text-muted">This will change the status back to In Progress.</small>',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#ffc107',
        cancelButtonColor: '#6c757d',
        confirmButtonText: '<i class="bi bi-unlock me-2"></i>Yes, Unlock',
        cancelButtonText: '<i class="bi bi-x-circle me-2"></i>Cancel',
        customClass: {
            confirmButton: 'btn btn-warning',
            cancelButton: 'btn btn-secondary'
        },
        buttonsStyling: false
    }).then((result) => {
        if (!result.isConfirmed) {
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
                    Swal.fire({
                        icon: 'success',
                        title: 'Unlocked!',
                        text: response.message,
                        confirmButtonColor: '#28a745',
                        timer: 2000,
                        timerProgressBar: true
                    }).then(() => {
                        location.reload();
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: response.message,
                        confirmButtonColor: '#dc3545'
                    });
                }
            },
            error: function(xhr, status, error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Network Error',
                    text: 'Failed to unlock. Please try again.',
                    confirmButtonColor: '#dc3545'
                });
            }
        });
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
            message: 'Mark this location as In Progress?',
            icon: 'info',
            color: '#17a2b8'
        },
        'Completed': {
            url: '/SurveyCreation/MarkLocationAsCompleted',
            message: 'Mark this location as Completed?',
            icon: 'success',
            color: '#28a745'
        },
        'Verified': {
            url: '/SurveyCreation/MarkLocationAsVerified',
            message: 'Mark this location as Verified?',
            icon: 'success',
            color: '#28a745'
        }
    };
    
    const action = statusActions[status];
    if (!action) {
        Swal.fire({
            icon: 'error',
            title: 'Invalid Action',
            text: 'Invalid status action',
            confirmButtonColor: '#dc3545'
        });
        return;
    }
    
    Swal.fire({
        title: 'Update Status?',
        text: action.message,
        icon: action.icon,
        showCancelButton: true,
        confirmButtonColor: action.color,
        cancelButtonColor: '#6c757d',
        confirmButtonText: '<i class="bi bi-check-circle me-2"></i>Yes, Update',
        cancelButtonText: '<i class="bi bi-x-circle me-2"></i>Cancel',
        customClass: {
            confirmButton: 'btn btn-primary',
            cancelButton: 'btn btn-secondary'
        },
        buttonsStyling: false
    }).then((result) => {
        if (!result.isConfirmed) {
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
                    Swal.fire({
                        icon: 'success',
                        title: 'Success!',
                        text: response.message,
                        confirmButtonColor: '#28a745',
                        timer: 2000,
                        timerProgressBar: true
                    }).then(() => {
                        location.reload();
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: response.message,
                        confirmButtonColor: '#dc3545'
                    });
                }
            },
            error: function(xhr, status, error) {
                console.error('AJAX Error:', error);
                console.error('Status:', status);
                console.error('Response:', xhr.responseText);
                Swal.fire({
                    icon: 'error',
                    title: 'Network Error',
                    text: 'Failed to update status. Please check console for details.',
                    confirmButtonColor: '#dc3545'
                });
            }
        });
    });
}

/**
 * Submit the entire survey for approval to supervisor
 */
function submitSurveyForApproval() {
    Swal.fire({
        title: 'Submit Survey for Approval?',
        html: '<p>You are about to submit this survey for supervisor approval.</p>' +
              '<p class="text-warning mb-0"><i class="bi bi-exclamation-triangle"></i> ' +
              '<strong>Note:</strong> Once submitted, the survey will be locked and cannot be edited until reviewed.</p>',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#6f42c1',
        cancelButtonColor: '#6c757d',
        confirmButtonText: '<i class="bi bi-send-check me-2"></i>Yes, Submit for Approval',
        cancelButtonText: 'Cancel',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            // Show loading
            Swal.fire({
                title: 'Submitting Survey...',
                html: 'Please wait while we process your submission.',
                allowOutsideClick: false,
                allowEscapeKey: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            // Submit via AJAX
            $.ajax({
                url: '/SurveySubmission/SubmitForApproval',
                type: 'POST',
                data: {
                    surveyId: currentSurveyId,
                    __RequestVerificationToken: window.surveyDetailsConfig.antiForgeryToken
                },
                success: function(response) {
                    Swal.close();
                    
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Survey Submitted!',
                            html: '<p>' + response.message + '</p>' +
                                  '<p class="text-muted mb-0">Your supervisor will be notified via email.</p>',
                            confirmButtonColor: '#28a745',
                            timer: 3000,
                            timerProgressBar: true
                        }).then(() => {
                            // Close modal and redirect to dashboard or submissions page
                            var modal = bootstrap.Modal.getInstance(document.getElementById('previewModal'));
                            if (modal) modal.hide();
                            window.location.href = '/SurveySubmission/MySubmissions';
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Submission Failed',
                            text: response.message,
                            confirmButtonColor: '#dc3545'
                        });
                    }
                },
                error: function(xhr, status, error) {
                    Swal.close();
                    Swal.fire({
                        icon: 'error',
                        title: 'Network Error',
                        text: 'Failed to submit survey. Please try again.',
                        confirmButtonColor: '#dc3545'
                    });
                }
            });
        }
    });
}

// Initialize when document is ready
$(document).ready(function() {
    // Any additional initialization can go here
});
