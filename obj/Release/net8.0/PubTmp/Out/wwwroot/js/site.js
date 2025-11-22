// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Change Password popup Form  --------------------------------------------------------------------------------------------------------------
function showPasswordModal() {
    const modal = new bootstrap.Modal(document.getElementById('passwordModal'));

    // Close any open dropdowns
    document.querySelectorAll('.dropdown-menu.show').forEach(menu => {
        menu.classList.remove('show');
    });

    modal.show();
}

$('#changePasswordForm').submit(function (e) {
    e.preventDefault();

    const currentPassword = $('#currentPassword').val();
    const newPassword = $('#newPassword').val();
    const confirmPassword = $('#confirmPassword').val();

    if (newPassword !== confirmPassword) {
        $('#passwordMessageArea').html(`
                <div class="alert alert-danger alert-dismissible fade show" role="alert">
                    Passwords do not match.
                    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                </div>
                `);
        return;
    }

    $.ajax({
        url: '/Users/ChangePassword',
        type: 'POST',
        data: {
            currentPassword,
            newPassword
        },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            $('#passwordMessageArea').html(`
                    <div class="alert alert-${response.success ? 'success' : 'danger'} alert-dismissible fade show" role="alert">
                        ${response.message}
                        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                    </div>
                    `);

            if (response.success) {
                window.setTimeout(() => {
                    bootstrap.Modal.getInstance(document.getElementById('passwordModal')).hide();
                }, 2000);
            }
        },
        error: function () {
            $('#passwordMessageArea').html(`
                        <div class="alert alert-danger alert-dismissible fade show" role="alert">
                            Something went wrong.
                            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                        </div>
                    `);
        }
    });
});

//  --------------------------------------------------------------------------------------------------------------

// back Conform popup  --------------------------------------------------------------------------------------------------------------
document.querySelectorAll('.form-custom-floating .form-control').forEach(input => {
    // Initialize (for server-rendered values)
    if (input.value) input.classList.add('has-value');

    // On focus / blur
    input.addEventListener('focus', () => input.classList.add('focused'));
    input.addEventListener('blur', () => input.classList.remove('focused'));

    // On input
    input.addEventListener('input', () => {
        if (input.value) input.classList.add('has-value');
        else input.classList.remove('has-value');
    });
});

//  --------------------------------------------------------------------------------------------------------------
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".form-custom-floating .form-control, .form-custom-floating .form-select")
        .forEach(function (el) {

            function toggleHasValue() {
                if (el.value && el.value.trim() !== "") {
                    el.classList.add("has-value");
                } else {
                    el.classList.remove("has-value");
                }
            }

            // Initial check (model binding, autofill, password managers, etc.)
            toggleHasValue();

            // Listen for user input/change
            el.addEventListener("input", toggleHasValue);
            el.addEventListener("change", toggleHasValue);

            // Handle browser autofill (delayed check)
            window.setTimeout(toggleHasValue, 500);
        });
});
// validations --------------------------------------------------------------------------------------------------------------

// Example starter JavaScript for disabling form submissions if there are invalid fields
(function () {
    'use strict'

    // Fetch all the forms we want to apply custom Bootstrap validation styles to
    var forms = document.querySelectorAll('.needs-validation')

    // Loop over them and prevent submission
    Array.prototype.slice.call(forms)
        .forEach(function (form) {
            form.addEventListener('submit', function (event) {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                }

                form.classList.add('was-validated')
            }, false)
        })
})()

function allowOnlyNumbers(input) {
    input.value = input.value.replace(/[^0-9]/g, ''); // remove non-digits
}

function allowOnlyDecimal(input) {
    // Remove invalid characters (allow digits and one dot)
    input.value = input.value.replace(/[^0-9.]/g, '');

    // Prevent multiple dots
    const parts = input.value.split('.');
    if (parts.length > 2) {
        input.value = parts[0] + '.' + parts[1]; // Keep only first dot
    }

    // Limit to 3 decimal places
    if (parts.length === 2) {
        parts[1] = parts[1].substring(0, 3); // Trim to 3 digits
        input.value = parts[0] + '.' + parts[1];
    }
}


function allowOnlySignedNumbers(input) {
    input.value = input.value.replace(/[^0-9-]/g, ''); // allow numbers + minus
    if ((input.value.match(/-/g) || []).length > 1 || input.value.indexOf('-') > 0) {
        input.value = input.value.replace(/-/g, ''); // only allow one minus at start
    }
}

function calculateConsumption(curReadId, prvReadId, consumptionId, mfId) {
    const curRead = parseFloat(document.getElementById(curReadId)?.value) || 0;
    const prvRead = parseFloat(document.getElementById(prvReadId)?.value) || 0;
    const mf = parseFloat(document.getElementById(mfId)?.value) || 1;

    const result = (curRead - prvRead) * mf;
    const consumptionCtrl = document.getElementById(consumptionId);
    if (consumptionCtrl) {
        consumptionCtrl.value = result.toFixed(3);
    }
}

function validateRequiredRange(input) {
    const val = parseFloat(input.value);
    if (!input.value || isNaN(val) || val < 0.20 || val > 1.00) {
        input.classList.add("input-validation-error");
    } else {
        input.classList.remove("input-validation-error");
    }
}

function calculateWeight(grossId, tareId, netId) {
    const grossWeight = parseFloat(document.getElementById(grossId)?.value) || 0;
    const tareWeight = parseFloat(document.getElementById(tareId)?.value) || 0;
   

    const result = (grossWeight - tareWeight);
    const consumptionCtrl = document.getElementById(netId);
    if (consumptionCtrl) {
        consumptionCtrl.value = result.toFixed(2);
    }
}