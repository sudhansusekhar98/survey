/**
 * Report OTP Access Control Module
 * Handles OTP generation, validation, and report download authorization
 */
const ReportOTPModule = (function () {
    'use strict';

    // Private variables
    let currentReportType = '';
    let currentReportParams = {};
    let currentDownloadUrl = '';
    let otpLogId = null;

    /**
     * Initialize the OTP module
     */
    function init() {
        createOTPModal();
        bindEvents();
    }

    /**
     * Create the OTP modal HTML
     */
    function createOTPModal() {
        // Remove existing modal if present
        $('#otpVerificationModal').remove();

        const modalHtml = `
            <div class="modal fade" id="otpVerificationModal" tabindex="-1" aria-labelledby="otpModalLabel" aria-hidden="true" data-bs-backdrop="static">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header bg-primary text-white">
                            <h5 class="modal-title" id="otpModalLabel">
                                <i class="bx bx-lock-alt me-2"></i>Report Download Authorization
                            </h5>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <!-- Request OTP Section -->
                            <div id="otpRequestSection">
                                <div class="text-center mb-4">
                                    <div class="avatar avatar-xl bg-label-warning mx-auto mb-3">
                                        <i class="bx bx-shield-quarter bx-lg"></i>
                                    </div>
                                    <h5 class="mb-2">OTP Verification Required</h5>
                                    <p class="text-muted mb-0">
                                        For security purposes, you need to verify your identity with a One-Time Password (OTP) to download this report.
                                    </p>
                                </div>
                                <div class="alert alert-info d-flex align-items-center">
                                    <i class="bx bx-info-circle bx-lg me-2"></i>
                                    <div>
                                        Click "Request OTP" to generate a verification code. The OTP will be sent to the Super Admin for approval.
                                    </div>
                                </div>
                                <div class="d-grid">
                                    <button type="button" class="btn btn-primary" id="btnRequestOTP">
                                        <i class="bx bx-key me-2"></i>Request OTP
                                    </button>
                                </div>
                            </div>

                            <!-- Enter OTP Section (initially hidden) -->
                            <div id="otpEnterSection" style="display: none;">
                                <div class="text-center mb-4">
                                    <div class="avatar avatar-xl bg-label-success mx-auto mb-3">
                                        <i class="bx bx-message-square-check bx-lg"></i>
                                    </div>
                                    <h5 class="mb-2">Enter OTP</h5>
                                    <p class="text-muted mb-0">
                                        OTP has been generated. Please contact a Super Admin to get the OTP code.
                                    </p>
                                </div>
                                <div class="alert alert-warning d-flex align-items-center mb-3" id="otpExpiryWarning">
                                    <i class="bx bx-time-five me-2"></i>
                                    <span id="otpExpiryText">OTP expires in 10 minutes</span>
                                </div>
                                <div class="mb-3">
                                    <label class="form-label">Enter 6-digit OTP</label>
                                    <div class="input-group input-group-lg">
                                        <span class="input-group-text"><i class="bx bx-lock"></i></span>
                                        <input type="text" class="form-control text-center" id="otpInput" 
                                               maxlength="6" placeholder="000000" 
                                               style="letter-spacing: 1rem; font-size: 1.5rem; font-weight: bold;">
                                    </div>
                                    <div class="invalid-feedback" id="otpError"></div>
                                </div>
                                <div class="d-grid gap-2">
                                    <button type="button" class="btn btn-success btn-lg" id="btnValidateOTP">
                                        <i class="bx bx-check-circle me-2"></i>Verify & Download
                                    </button>
                                    <button type="button" class="btn btn-outline-secondary" id="btnResendOTP">
                                        <i class="bx bx-refresh me-2"></i>Resend OTP
                                    </button>
                                </div>
                            </div>

                            <!-- Processing Section -->
                            <div id="otpProcessingSection" style="display: none;">
                                <div class="text-center py-4">
                                    <div class="spinner-border text-primary mb-3" role="status">
                                        <span class="visually-hidden">Processing...</span>
                                    </div>
                                    <p class="mb-0" id="processingText">Processing...</p>
                                </div>
                            </div>

                            <!-- Success Section -->
                            <div id="otpSuccessSection" style="display: none;">
                                <div class="text-center py-4">
                                    <div class="avatar avatar-xl bg-label-success mx-auto mb-3">
                                        <i class="bx bx-check-circle bx-lg"></i>
                                    </div>
                                    <h5 class="text-success mb-2">Verification Successful</h5>
                                    <p class="text-muted mb-0">Your report download should start automatically.</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        $('body').append(modalHtml);
    }

    /**
     * Bind event handlers
     */
    function bindEvents() {
        // Request OTP button
        $(document).on('click', '#btnRequestOTP', function () {
            requestOTP();
        });

        // Validate OTP button
        $(document).on('click', '#btnValidateOTP', function () {
            validateOTP();
        });

        // Resend OTP button
        $(document).on('click', '#btnResendOTP', function () {
            requestOTP();
        });

        // OTP input - only allow numbers
        $(document).on('input', '#otpInput', function () {
            this.value = this.value.replace(/[^0-9]/g, '');
        });

        // OTP input - auto submit on 6 digits
        $(document).on('keyup', '#otpInput', function (e) {
            if (this.value.length === 6) {
                if (e.key === 'Enter') {
                    validateOTP();
                }
            }
        });

        // Reset modal on close
        $(document).on('hidden.bs.modal', '#otpVerificationModal', function () {
            resetModal();
        });
    }

    /**
     * Check if user requires OTP before download
     * @param {string} reportType - Type of report
     * @param {object} params - Report parameters
     * @param {string} downloadUrl - URL to download the report
     * @param {function} callback - Optional callback for when download is allowed
     */
    function checkAndDownload(reportType, params, downloadUrl, callback) {
        currentReportType = reportType;
        currentReportParams = params || {};
        currentDownloadUrl = downloadUrl;

        // Check if OTP is required
        $.ajax({
            url: '/ReportOTP/CheckOTPRequired',
            type: 'GET',
            success: function (response) {
                if (response.isSuperAdmin || !response.requiresOTP) {
                    // Super Admin - proceed with download directly
                    proceedWithDownload(callback);
                } else {
                    // Non-super admin - check if they already have a valid OTP
                    checkExistingOTP(callback);
                }
            },
            error: function () {
                // On error, require OTP for safety
                showOTPModal();
            }
        });
    }

    /**
     * Check if user already has a valid OTP
     */
    function checkExistingOTP(callback) {
        $.ajax({
            url: '/ReportOTP/HasValidOTP',
            type: 'GET',
            success: function (response) {
                if (response.hasValidOTP) {
                    // Already has valid OTP - proceed with download
                    proceedWithDownload(callback);
                } else {
                    // Need to request new OTP
                    showOTPModal();
                }
            },
            error: function () {
                showOTPModal();
            }
        });
    }

    /**
     * Show OTP verification modal
     */
    function showOTPModal() {
        resetModal();
        $('#otpVerificationModal').modal('show');
    }

    /**
     * Request a new OTP
     */
    function requestOTP() {
        showSection('otpProcessingSection');
        $('#processingText').text('Generating OTP...');

        $.ajax({
            url: '/ReportOTP/RequestOTP',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                reportType: currentReportType,
                surveyId: currentReportParams.surveyId,
                fromDate: currentReportParams.fromDate,
                toDate: currentReportParams.toDate,
                status: currentReportParams.status,
                region: currentReportParams.region,
                type: currentReportParams.type
            }),
            success: function (response) {
                if (response.success) {
                    otpLogId = response.logId;
                    showSection('otpEnterSection');

                    // Update expiry text
                    if (response.expiresAt) {
                        updateExpiryCountdown(new Date(response.expiresAt));
                    }

                    // Show success toast
                    showToast('success', 'OTP Generated', response.message);
                } else {
                    showSection('otpRequestSection');
                    showToast('error', 'Error', response.message);
                }
            },
            error: function () {
                showSection('otpRequestSection');
                showToast('error', 'Error', 'Failed to generate OTP. Please try again.');
            }
        });
    }

    /**
     * Validate the entered OTP
     */
    function validateOTP() {
        const otp = $('#otpInput').val().trim();

        if (!otp || otp.length !== 6) {
            $('#otpInput').addClass('is-invalid');
            $('#otpError').text('Please enter a valid 6-digit OTP');
            return;
        }

        $('#otpInput').removeClass('is-invalid');
        showSection('otpProcessingSection');
        $('#processingText').text('Verifying OTP...');

        $.ajax({
            url: '/ReportOTP/ValidateOTP',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                otp: otp,
                reportType: currentReportType
            }),
            success: function (response) {
                if (response.success) {
                    showSection('otpSuccessSection');
                    showToast('success', 'Success', 'OTP verified successfully!');

                    // Proceed with download after a short delay
                    setTimeout(function () {
                        $('#otpVerificationModal').modal('hide');
                        proceedWithDownload();
                    }, 1500);
                } else {
                    showSection('otpEnterSection');
                    $('#otpInput').addClass('is-invalid');
                    $('#otpError').text(response.message);
                    showToast('error', 'Invalid OTP', response.message);
                }
            },
            error: function () {
                showSection('otpEnterSection');
                showToast('error', 'Error', 'Failed to validate OTP. Please try again.');
            }
        });
    }

    /**
     * Proceed with the report download
     */
    function proceedWithDownload(callback) {
        if (callback && typeof callback === 'function') {
            callback();
        } else if (currentDownloadUrl) {
            window.location.href = currentDownloadUrl;
        }
    }

    /**
     * Show a specific section in the modal
     */
    function showSection(sectionId) {
        $('#otpRequestSection, #otpEnterSection, #otpProcessingSection, #otpSuccessSection').hide();
        $('#' + sectionId).show();
    }

    /**
     * Reset the modal to initial state
     */
    function resetModal() {
        $('#otpInput').val('').removeClass('is-invalid');
        $('#otpError').text('');
        showSection('otpRequestSection');
    }

    /**
     * Update expiry countdown
     */
    function updateExpiryCountdown(expiresAt) {
        const updateTimer = function () {
            const now = new Date();
            const diff = expiresAt - now;

            if (diff <= 0) {
                $('#otpExpiryText').text('OTP has expired');
                $('#otpExpiryWarning').removeClass('alert-warning').addClass('alert-danger');
                return;
            }

            const minutes = Math.floor(diff / 60000);
            const seconds = Math.floor((diff % 60000) / 1000);
            $('#otpExpiryText').text(`OTP expires in ${minutes}:${seconds.toString().padStart(2, '0')}`);
        };

        updateTimer();
        const timerId = setInterval(function () {
            updateTimer();
            if (expiresAt <= new Date()) {
                clearInterval(timerId);
            }
        }, 1000);
    }

    /**
     * Show toast notification
     */
    function showToast(type, title, message) {
        // Use existing toast system if available, or fallback to alert
        if (typeof toastr !== 'undefined') {
            toastr[type](message, title);
        } else if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: type === 'success' ? 'success' : 'error',
                title: title,
                text: message,
                timer: 3000,
                showConfirmButton: false
            });
        } else {
            console.log(`${type}: ${title} - ${message}`);
        }
    }

    // Public API
    return {
        init: init,
        checkAndDownload: checkAndDownload,
        showOTPModal: showOTPModal
    };
})();

// Initialize on document ready
$(document).ready(function () {
    ReportOTPModule.init();
});
