// Profile Picture Upload Functionality

$(document).ready(function () {
    // Click overlay to trigger file input
    $('#avatarUploadOverlay').on('click', function (e) {
        e.stopPropagation();
        $('#profilePictureInput').click();
    });

    // Click upload button to trigger file input
    $('#uploadPhotoBtn').on('click', function (e) {
        e.preventDefault();
        $('#profilePictureInput').click();
    });

    // Handle file selection
    $('#profilePictureInput').on('change', function (e) {
        const file = e.target.files[0];
        if (!file) return;

        // Validate file type
        if (!file.type.startsWith('image/')) {
            alert('Please select a valid image file');
            return;
        }

        // Validate file size (max 5MB)
        if (file.size > 5 * 1024 * 1024) {
            alert('Image size should not exceed 5MB');
            return;
        }

        // Read file as base64
        const reader = new FileReader();
        reader.onload = function (event) {
            const base64Image = event.target.result;

            // Show loading state
            $('#avatarUploadOverlay').html('<div class="spinner-border spinner-border-sm text-white" role="status"><span class="visually-hidden">Uploading...</span></div>');

            // Get the upload URL from the button's data attribute or construct it
            const uploadUrl = $('#uploadPhotoBtn').data('upload-url') || '/Profile/UploadProfilePicture';

            // Upload to server
            $.ajax({
                url: uploadUrl,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ base64Image: base64Image }),
                success: function (response) {
                    if (response.success) {
                        // Update avatar display
                        updateAvatarDisplay(response.url);
                        
                        // Show success message
                        showToast('success', 'Profile picture updated successfully!');
                        
                        // Reload page to update header
                        setTimeout(function() {
                            location.reload();
                        }, 1000);
                    } else {
                        alert('Error: ' + response.message);
                        resetUploadOverlay();
                    }
                },
                error: function () {
                    alert('Failed to upload profile picture. Please try again.');
                    resetUploadOverlay();
                }
            });
        };

        reader.readAsDataURL(file);
    });

    // Handle delete profile picture
    $('#deleteAvatarBtn').on('click', function (e) {
        e.stopPropagation();
        
        if (!confirm('Are you sure you want to delete your profile picture?')) {
            return;
        }

        // Get the delete URL from the button's data attribute or construct it
        const deleteUrl = $('#deleteAvatarBtn').data('delete-url') || '/Profile/DeleteProfilePicture';

        $.ajax({
            url: deleteUrl,
            type: 'POST',
            success: function (response) {
                if (response.success) {
                    // Reset to default avatar
                    resetToDefaultAvatar();
                    
                    // Show success message
                    showToast('success', 'Profile picture deleted successfully!');
                    
                    // Reload page to update header
                    setTimeout(function() {
                        location.reload();
                    }, 1000);
                } else {
                    alert('Error: ' + response.message);
                }
            },
            error: function () {
                alert('Failed to delete profile picture. Please try again.');
            }
        });
    });

    function updateAvatarDisplay(imageUrl) {
        const container = $('#profileAvatarContainer');
        const icon = $('#profileAvatarIcon');
        
        if (icon.length) {
            icon.remove();
        }

        const existingImg = $('#profileAvatarImg');
        if (existingImg.length) {
            existingImg.attr('src', imageUrl);
        } else {
            const img = $('<img>').attr({
                src: imageUrl,
                alt: 'Profile Picture',
                id: 'profileAvatarImg'
            });
            container.prepend(img);
        }

        // Add delete button if not exists
        if ($('#deleteAvatarBtn').length === 0) {
            const deleteBtn = $('<button>')
                .addClass('delete-avatar-btn')
                .attr({
                    id: 'deleteAvatarBtn',
                    type: 'button',
                    title: 'Delete profile picture'
                })
                .html('<i class="bi bi-x"></i>')
                .on('click', function(e) {
                    e.stopPropagation();
                    if (confirm('Are you sure you want to delete your profile picture?')) {
                        // Call delete function
                        $('#deleteAvatarBtn').trigger('click');
                    }
                });
            container.append(deleteBtn);
        }

        resetUploadOverlay();
    }

    function resetToDefaultAvatar() {
        const container = $('#profileAvatarContainer');
        const img = $('#profileAvatarImg');
        const deleteBtn = $('#deleteAvatarBtn');
        
        if (img.length) img.remove();
        if (deleteBtn.length) deleteBtn.remove();
        
        const icon = $('<i>').addClass('bi bi-person-circle').attr('id', 'profileAvatarIcon');
        container.prepend(icon);
    }

    function resetUploadOverlay() {
        $('#avatarUploadOverlay').html('<i class="bi bi-camera-fill"></i><span>Change Photo</span>');
    }

    function showToast(type, message) {
        const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
        const toast = $('<div>')
            .addClass('alert ' + alertClass + ' alert-dismissible fade show position-fixed')
            .css({
                top: '20px',
                right: '20px',
                zIndex: 9999,
                minWidth: '300px'
            })
            .html(message + '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>');
        
        $('body').append(toast);
        
        setTimeout(function() {
            toast.alert('close');
        }, 3000);
    }
});
