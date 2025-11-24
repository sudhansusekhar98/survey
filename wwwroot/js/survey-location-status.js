// SurveyLocationStatus JavaScript Helper
// Include this in views that need location status management

const SurveyLocationStatus = {
    /**
     * Mark a location as completed
     * @param {number} surveyId - Survey ID
     * @param {number} locId - Location ID
     * @param {string} remarks - Optional remarks
     * @param {function} callback - Callback function(success, message)
     */
    markAsCompleted: function(surveyId, locId, remarks, callback) {
        $.ajax({
            url: '/SurveyCreation/MarkLocationAsCompleted',
            type: 'POST',
            data: {
                surveyId: surveyId,
                locId: locId,
                remarks: remarks || ''
            },
            success: function(response) {
                if (callback) callback(response.success, response.message);
            },
            error: function(xhr, status, error) {
                if (callback) callback(false, 'Network error: ' + error);
            }
        });
    },

    /**
     * Mark a location as in progress
     * @param {number} surveyId - Survey ID
     * @param {number} locId - Location ID
     * @param {string} remarks - Optional remarks
     * @param {function} callback - Callback function(success, message)
     */
    markAsInProgress: function(surveyId, locId, remarks, callback) {
        $.ajax({
            url: '/SurveyCreation/MarkLocationAsInProgress',
            type: 'POST',
            data: {
                surveyId: surveyId,
                locId: locId,
                remarks: remarks || ''
            },
            success: function(response) {
                if (callback) callback(response.success, response.message);
            },
            error: function(xhr, status, error) {
                if (callback) callback(false, 'Network error: ' + error);
            }
        });
    },

    /**
     * Mark a location as verified
     * @param {number} surveyId - Survey ID
     * @param {number} locId - Location ID
     * @param {string} remarks - Optional remarks
     * @param {function} callback - Callback function(success, message)
     */
    markAsVerified: function(surveyId, locId, remarks, callback) {
        $.ajax({
            url: '/SurveyCreation/MarkLocationAsVerified',
            type: 'POST',
            data: {
                surveyId: surveyId,
                locId: locId,
                remarks: remarks || ''
            },
            success: function(response) {
                if (callback) callback(response.success, response.message);
            },
            error: function(xhr, status, error) {
                if (callback) callback(false, 'Network error: ' + error);
            }
        });
    },

    /**
     * Get status for a specific location
     * @param {number} surveyId - Survey ID
     * @param {number} locId - Location ID
     * @param {function} callback - Callback function(success, data)
     */
    getLocationStatus: function(surveyId, locId, callback) {
        $.ajax({
            url: '/SurveyCreation/GetLocationStatus',
            type: 'GET',
            data: {
                surveyId: surveyId,
                locId: locId
            },
            success: function(response) {
                if (callback) callback(response.success, response.data);
            },
            error: function(xhr, status, error) {
                if (callback) callback(false, null);
            }
        });
    },

    /**
     * Get all location statuses for a survey
     * @param {number} surveyId - Survey ID
     * @param {function} callback - Callback function(success, data)
     */
    getSurveyLocationStatuses: function(surveyId, callback) {
        $.ajax({
            url: '/SurveyCreation/GetSurveyLocationStatuses',
            type: 'GET',
            data: {
                surveyId: surveyId
            },
            success: function(response) {
                if (callback) callback(response.success, response.data);
            },
            error: function(xhr, status, error) {
                if (callback) callback(false, null);
            }
        });
    },

    /**
     * Get status badge HTML based on status text
     * @param {string} status - Status text (Pending, In Progress, Completed, Verified)
     * @returns {string} HTML badge element
     */
    getStatusBadge: function(status) {
        const badgeMap = {
            'Pending': '<span class="badge bg-secondary">Pending</span>',
            'In Progress': '<span class="badge bg-info">In Progress</span>',
            'Completed': '<span class="badge bg-success">Completed</span>',
            'Verified': '<span class="badge bg-primary">Verified</span>'
        };
        return badgeMap[status] || '<span class="badge bg-secondary">Unknown</span>';
    },

    /**
     * Show status update buttons for a location
     * @param {number} surveyId - Survey ID
     * @param {number} locId - Location ID
     * @param {string} currentStatus - Current status of the location
     * @returns {string} HTML buttons
     */
    getStatusButtons: function(surveyId, locId, currentStatus) {
        let buttons = '';
        
        if (currentStatus !== 'In Progress' && currentStatus !== 'Completed' && currentStatus !== 'Verified') {
            buttons += `<button type="button" class="btn btn-sm btn-info" onclick="SurveyLocationStatus.markAsInProgress(${surveyId}, ${locId}, null, function(success, msg) { if(success) { alert(msg); location.reload(); } else { alert('Error: ' + msg); } })">
                <i class="bi bi-play-circle"></i> Start
            </button> `;
        }
        
        if (currentStatus !== 'Completed' && currentStatus !== 'Verified') {
            buttons += `<button type="button" class="btn btn-sm btn-success" onclick="SurveyLocationStatus.markAsCompleted(${surveyId}, ${locId}, null, function(success, msg) { if(success) { alert(msg); location.reload(); } else { alert('Error: ' + msg); } })">
                <i class="bi bi-check-circle"></i> Complete
            </button> `;
        }
        
        if (currentStatus === 'Completed') {
            buttons += `<button type="button" class="btn btn-sm btn-primary" onclick="SurveyLocationStatus.markAsVerified(${surveyId}, ${locId}, null, function(success, msg) { if(success) { alert(msg); location.reload(); } else { alert('Error: ' + msg); } })">
                <i class="bi bi-shield-check"></i> Verify
            </button> `;
        }
        
        return buttons;
    }
};

// Export for use in modules if needed
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SurveyLocationStatus;
}
