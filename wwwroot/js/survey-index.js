/* Survey Index Page JavaScript Functions */

function checkAndEdit(surveyId) {
    $.ajax({
        url: surveyUrls.getSubmissionStatus,
        type: 'GET',
        data: { surveyId: surveyId },
        success: function(response) {
            if (response.success) {
                if (response.canEdit) {
                    window.location.href = surveyUrls.edit + '/' + surveyId;
                } else {
                    var status = response.submission?.submissionStatus || 'Submitted';
                    Swal.fire({
                        icon: 'warning',
                        title: 'Survey Locked',
                        html: `<p>This survey has been submitted and is locked for editing.</p>
                               <p><strong>Status:</strong> <span class="badge bg-warning">${status}</span></p>`,
                        confirmButtonText: 'OK',
                        confirmButtonColor: '#667eea'
                    });
                }
            } else {
                window.location.href = surveyUrls.edit + '/' + surveyId;
            }
        },
        error: function() {
            window.location.href = surveyUrls.edit + '/' + surveyId;
        }
    });
}

function submitSurvey(surveyId, surveyName) {
    // First check survey completion status
    $.ajax({
        url: surveyUrls.checkCompletion,
        type: 'GET',
        data: { surveyId: surveyId },
        success: function(checkResponse) {
            if (checkResponse.success && checkResponse.data) {
                var data = checkResponse.data;
                
                if (!data.isComplete) {
                    // Show incomplete locations
                    var incompleteList = data.incompleteLocationNames
                        .map(loc => `<li class="text-start">${loc}</li>`)
                        .join('');
                    
                    Swal.fire({
                        icon: 'error',
                        title: 'Survey Not Ready',
                        html: `
                            <div class="text-start">
                                <p><strong>${data.message}</strong></p>
                                <p class="mb-2">Completion Progress: 
                                    <span class="badge bg-info">${data.completedLocations}/${data.totalLocations} Completed</span>
                                </p>
                                <p class="mb-1"><strong>Incomplete Locations:</strong></p>
                                <ul class="text-danger">${incompleteList}</ul>
                                <p class="mt-2 text-muted small">
                                    <i class="bi bi-info-circle me-1"></i>
                                    Please complete all locations before submitting the survey.
                                </p>
                            </div>
                        `,
                        confirmButtonText: 'OK',
                        confirmButtonColor: '#dc3545',
                        width: '600px'
                    });
                    return;
                }
                
                // All locations complete, directly submit the survey (cable counts are now in Global Location)
                submitSurveyDirectly(surveyId, surveyName, data.totalLocations);
            }
        },
        error: function() {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to check survey completion status.',
                confirmButtonColor: '#667eea'
            });
        }
    });
}

// Show cable count modal before survey submission
function showCableCountModal(surveyId, surveyName, totalLocations) {
    // Fetch cable items and existing counts
    Promise.all([
        $.ajax({ url: surveyUrls.getCableItems, type: 'GET' }),
        $.ajax({ url: surveyUrls.getSurveyCableCounts, type: 'GET', data: { surveyId: surveyId } })
    ]).then(function([itemsResponse, countsResponse]) {
        var cableItems = itemsResponse.success ? itemsResponse.items : [];
        var savedCounts = countsResponse.success ? countsResponse.cableCounts : [];
        
        // Build saved counts lookup
        var savedCountsMap = {};
        savedCounts.forEach(function(c) {
            savedCountsMap[c.itemId] = { quantity: c.quantity, remarks: c.remarks };
        });
        
        // Build cable items form
        var cableFormHtml = '';
        if (cableItems.length === 0) {
            cableFormHtml = '<p class="text-muted">No cable items configured in the system.</p>';
        } else {
            cableFormHtml = `
                <div class="table-responsive">
                    <table class="table table-sm table-hover">
                        <thead class="table-light">
                            <tr>
                                <th>Cable Type</th>
                                <th style="width: 120px;">Quantity</th>
                                <th style="width: 150px;">Remarks</th>
                            </tr>
                        </thead>
                        <tbody>
            `;
            
            cableItems.forEach(function(item) {
                var saved = savedCountsMap[item.itemId] || { quantity: 0, remarks: '' };
                cableFormHtml += `
                    <tr>
                        <td>
                            <strong>${item.itemName}</strong>
                            <br><small class="text-muted">${item.itemCode} (${item.itemUOM})</small>
                        </td>
                        <td>
                            <input type="number" class="form-control form-control-sm cable-qty-input" 
                                   data-item-id="${item.itemId}" 
                                   value="${saved.quantity}" min="0" placeholder="0">
                        </td>
                        <td>
                            <input type="text" class="form-control form-control-sm cable-remarks-input" 
                                   data-item-id="${item.itemId}" 
                                   value="${saved.remarks}" placeholder="Optional">
                        </td>
                    </tr>
                `;
            });
            
            cableFormHtml += '</tbody></table></div>';
        }
        
        Swal.fire({
            icon: 'info',
            title: 'Global Cable Count',
            html: `
                <div class="text-start">
                    <p class="mb-3">Please enter the <strong>survey-level cable quantities</strong> for <strong>"${surveyName}"</strong>:</p>
                    <div class="alert alert-info py-2">
                        <i class="bi bi-info-circle me-1"></i>
                        These are total cable quantities for the entire survey, not per location.
                    </div>
                    ${cableFormHtml}
                    <div class="alert alert-success mt-3 mb-0">
                        <i class="bi bi-check-circle me-2"></i>
                        All ${totalLocations} location(s) are completed
                    </div>
                </div>
            `,
            showCancelButton: true,
            confirmButtonText: '<i class="bi bi-send me-1"></i> Save & Submit Survey',
            cancelButtonText: 'Cancel',
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            width: '700px',
            reverseButtons: true,
            preConfirm: () => {
                // Collect cable counts
                var cableCounts = [];
                $('.cable-qty-input').each(function() {
                    var itemId = parseInt($(this).data('item-id'));
                    var qty = parseInt($(this).val()) || 0;
                    var remarks = $(`.cable-remarks-input[data-item-id="${itemId}"]`).val() || '';
                    cableCounts.push({ itemId: itemId, quantity: qty, remarks: remarks });
                });
                return cableCounts;
            }
        }).then((result) => {
            if (result.isConfirmed) {
                saveCableCountsAndSubmit(surveyId, surveyName, result.value);
            }
        });
    }).catch(function() {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Failed to load cable items.',
            confirmButtonColor: '#dc3545'
        });
    });
}

// Save cable counts and then submit the survey
function saveCableCountsAndSubmit(surveyId, surveyName, cableCounts) {
    var token = $('input[name="__RequestVerificationToken"]').val();
    
    Swal.fire({
        title: 'Submitting...',
        text: 'Saving cable counts and submitting survey...',
        allowOutsideClick: false,
        allowEscapeKey: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
    
    // First save cable counts
    $.ajax({
        url: surveyUrls.saveCableCounts,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            surveyId: surveyId,
            cableCounts: cableCounts
        }),
        success: function(saveResponse) {
            if (saveResponse.success) {
                // Now submit the survey
                performSubmission(surveyId, surveyName);
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: saveResponse.message || 'Failed to save cable counts.',
                    confirmButtonColor: '#dc3545'
                });
            }
        },
        error: function() {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to save cable counts.',
                confirmButtonColor: '#dc3545'
            });
        }
    });
}

// Direct submission without cable count modal (cable counts are now in Global Location)
function submitSurveyDirectly(surveyId, surveyName, totalLocations) {
    Swal.fire({
        title: 'Submit Survey?',
        html: `<p>All <strong>${totalLocations}</strong> location(s) are complete for survey: <strong>${surveyName}</strong></p>
               <p>Are you sure you want to submit this survey?</p>`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#28a745',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, Submit',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            performSubmission(surveyId, surveyName);
        }
    });
}

function performSubmission(surveyId, surveyName) {
    var token = $('input[name="__RequestVerificationToken"]').val();
    
    Swal.fire({
        title: 'Submitting...',
        text: 'Please wait while we submit your survey.',
        allowOutsideClick: false,
        allowEscapeKey: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
    
    $.ajax({
        url: surveyUrls.submitSurvey,
        type: 'POST',
        data: {
            surveyId: surveyId,
            __RequestVerificationToken: token
        },
        success: function(response) {
            if (response.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success!',
                    text: response.message || 'Survey submitted successfully.',
                    confirmButtonColor: '#28a745'
                }).then(() => {
                    location.reload();
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Submission Failed',
                    text: response.message || 'Failed to submit survey.',
                    confirmButtonColor: '#dc3545'
                });
            }
        },
        error: function(xhr) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'An error occurred while submitting the survey.',
                confirmButtonColor: '#dc3545'
            });
        }
    });
}

function openEditModal(id) {
    var url = surveyUrls.edit + '/' + id;
    $.ajax({
        url: url,
        type: 'GET',
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function(html) {
            $('#editModalContent').html(html);
            var modalEl = document.getElementById('editModal');
            var modal = new bootstrap.Modal(modalEl);
            modal.show();
        },
        error: function() {
            alert('Failed to load edit form.');
        }
    });
}

function openSurveyDetailsModal(id) {
    var url = surveyUrls.viewDetails + '/' + id;
    $.ajax({
        url: url,
        type: 'GET',
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function(html) {
            $('#editModalContent').html(html);
            var modalEl = document.getElementById('editModal');
            var modal = new bootstrap.Modal(modalEl);
            modal.show();
        },
        error: function() {
            alert('Failed to load survey details.');
        }
    });
}

function confirmDelete(surveyId, surveyName) {
    Swal.fire({
        icon: 'warning',
        title: 'Delete Survey?',
        html: `<p>Are you sure you want to delete the survey:</p>
               <p><strong>"${surveyName}"</strong>?</p>
               <p class="text-danger mt-3">
                   <i class="bi bi-exclamation-triangle me-1"></i>
                   This action cannot be undone!
               </p>`,
        showCancelButton: true,
        confirmButtonText: 'Yes, Delete',
        cancelButtonText: 'Cancel',
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            var token = $('input[name="__RequestVerificationToken"]').val();
            
            Swal.fire({
                title: 'Deleting...',
                text: 'Please wait',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });
            
            $.ajax({
                type: 'POST',
                url: surveyUrls.deleteSurvey,
                data: {
                    id: surveyId,
                    __RequestVerificationToken: token
                },
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Deleted!',
                            text: response.message,
                            confirmButtonColor: '#28a745'
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
                error: function (xhr, status, error) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'An error occurred while deleting the survey: ' + (xhr.responseJSON?.message || error),
                        confirmButtonColor: '#dc3545'
                    });
                }
            });
        }
    });
}

function confirmUnlockSurvey(surveyId, surveyName) {
    Swal.fire({
        icon: 'question',
        title: 'Unlock Survey?',
        html: `<p>Do you want to unlock the survey:</p>
               <p><strong>"${surveyName}"</strong>?</p>
               <p class="text-warning mt-3">
                   <i class="bi bi-unlock me-1"></i>
                   This will allow you to edit locations. The survey will be returned to draft status.
               </p>`,
        showCancelButton: true,
        confirmButtonText: 'Yes, Unlock',
        cancelButtonText: 'Cancel',
        confirmButtonColor: '#17a2b8',
        cancelButtonColor: '#6c757d',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            var token = $('input[name="__RequestVerificationToken"]').val();
            
            Swal.fire({
                title: 'Unlocking...',
                text: 'Please wait',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });
            
            $.ajax({
                type: 'POST',
                url: surveyUrls.unlockSurvey,
                data: {
                    surveyId: surveyId,
                    __RequestVerificationToken: token
                },
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Unlocked!',
                            text: response.message,
                            confirmButtonColor: '#28a745'
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
                error: function (xhr, status, error) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'An error occurred while unlocking the survey: ' + (xhr.responseJSON?.message || error),
                        confirmButtonColor: '#dc3545'
                    });
                }
            });
        }
    });
}

function handleStatusChange(selectElement) {
    const selectedValue = selectElement.value;
    const missedInput = document.getElementById('missedInput');
    
    if (selectedValue === 'Missed Deadline') {
        // Set missed to true and clear status
        missedInput.value = 'true';
        selectElement.name = ''; // Don't send status parameter
    } else {
        // Clear missed and use status
        missedInput.value = '';
        selectElement.name = 'status'; // Send status parameter
    }
}
