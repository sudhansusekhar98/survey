-- SQL Script to add Update and Delete operations for Survey Assignments
-- Add these to your existing SpSurvey stored procedure

-- ============================================
-- SpType 18: Update Survey Assignment
-- ============================================
IF @SpType = 18
BEGIN
    UPDATE SurveyAssignment
    SET 
        EmpID = @EmpID,
        DueDate = @DueDate
    WHERE TransID = @TransID
      AND SurveyID = @SurveyID
END

-- ============================================
-- SpType 19: Delete Survey Assignment
-- ============================================
IF @SpType = 19
BEGIN
    DELETE FROM SurveyAssignment
    WHERE TransID = @TransID
END

GO

-- ============================================
-- Example usage:
-- ============================================

-- Update an assignment
-- EXEC SpSurvey @SpType = 18, @TransID = 1, @SurveyID = 123, @EmpID = 456, @DueDate = '2025-12-31'

-- Delete an assignment
-- EXEC SpSurvey @SpType = 19, @TransID = 1

GO
