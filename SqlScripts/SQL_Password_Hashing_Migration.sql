-- =============================================
-- Password Hashing Migration Guide
-- Purpose: Instructions for migrating from plain text to BCrypt password storage
-- Created: 2025-12-27
-- =============================================

/*
===================================================================================
BCRYPT PASSWORD HASHING IMPLEMENTATION
===================================================================================

The Survey Application now uses BCrypt for secure password hashing. This document
explains the implementation and migration strategy.

BENEFITS OF BCRYPT:
- Passwords are stored as cryptographic hashes, not plain text
- Each hash includes a unique salt (protection against rainbow table attacks)
- Computational cost makes brute-force attacks impractical
- Industry-standard algorithm designed specifically for passwords

IMPLEMENTATION DETAILS:
- Work factor: 11 (each password takes ~100-200ms to hash)
- Hash format: $2a$11$... (60 characters total)
- Library: BCrypt.Net-Next NuGet package

===================================================================================
AUTOMATIC MIGRATION
===================================================================================

The application implements AUTOMATIC password migration:

1. When a user logs in with a plain text password:
   - System detects the password is NOT a BCrypt hash
   - Verifies using direct comparison (for legacy support)
   - Hashes the password and updates the database
   - Future logins will use BCrypt verification

2. This means:
   - NO downtime required for migration
   - NO manual password resets needed
   - Users can continue using their existing passwords
   - Passwords are migrated one-by-one as users log in

3. Detection method:
   - BCrypt hashes are always 60 characters
   - BCrypt hashes start with "$2a$", "$2b$", or "$2y$"
   - If password doesn't match this pattern, treated as plain text

===================================================================================
WHAT HAPPENS WITH NEW PASSWORDS
===================================================================================

All new passwords are automatically hashed with BCrypt:

1. New user creation (Users/Create)
2. Password reset by admin (Users/ResetPassword)  
3. Password change by user (Users/ChangePassword)
4. Force password change after admin reset
5. Employee sync to login (SyncEmployeesToLogin)

===================================================================================
CHECKING MIGRATION STATUS (OPTIONAL)
===================================================================================

To see how many passwords have been migrated, run this query:

SELECT 
    COUNT(*) AS TotalUsers,
    SUM(CASE WHEN LEN(LoginPassword) = 60 AND LoginPassword LIKE '$2%$%' THEN 1 ELSE 0 END) AS BCryptPasswords,
    SUM(CASE WHEN LEN(LoginPassword) <> 60 OR LoginPassword NOT LIKE '$2%$%' THEN 1 ELSE 0 END) AS PlainTextPasswords
FROM LoginMaster
WHERE LoginPassword IS NOT NULL;

===================================================================================
FORCE MIGRATION (OPTIONAL - NOT RECOMMENDED)
===================================================================================

If you want to force-migrate all passwords immediately (NOT RECOMMENDED):

CAUTION: This approach requires knowing the plain text passwords, which means
         you need to generate new temporary passwords for all users.

Option 1: Have all users change their passwords
   - This is the safest approach
   - Use the "Reset Password" feature for each user
   - They'll be prompted to change password on next login

Option 2: Keep automatic migration
   - Wait for users to log in naturally
   - Their passwords will be migrated automatically
   - Most secure - no need to handle plain text passwords

===================================================================================
IMPORTANT NOTES
===================================================================================

1. BACKWARD COMPATIBILITY:
   - Login works with BOTH plain text and BCrypt passwords
   - No changes needed to existing user accounts
   - Migration happens transparently

2. SECURITY:
   - Never log or display plain text passwords
   - BCrypt hashes are safe to store and display (they cannot be reversed)
   - The TestPassword endpoint no longer returns the password hash

3. PASSWORD COLUMN SIZE:
   - BCrypt hashes are 60 characters
   - Ensure LoginPassword column is NVARCHAR(100) or larger
   
   If needed, run:
   ALTER TABLE LoginMaster ALTER COLUMN LoginPassword NVARCHAR(100);

4. PERFORMANCE:
   - BCrypt is intentionally slow (~100-200ms per hash)
   - This is a security feature, not a bug
   - Login may be slightly slower, but more secure

===================================================================================
TESTING THE IMPLEMENTATION
===================================================================================

After deployment, you can test using:

1. Create a new user - check that password is hashed (60 chars starting with $2)
2. Login with existing user - password should be auto-migrated
3. Change password - new password should be hashed
4. Reset password - temporary password should be hashed

Use the TestPassword endpoint to verify (Admin only):
GET /Users/TestPassword?userId=123&password=testpass

Response shows:
- passwordIsBCryptHash: true/false
- passwordMatch: true/false
- No longer shows the actual password hash (security)

===================================================================================
*/

-- Just to verify column size is adequate
-- Run this to check:
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('LoginMaster') 
  AND c.name = 'LoginPassword';

-- If max_length shows 50 or less, expand it:
-- ALTER TABLE LoginMaster ALTER COLUMN LoginPassword NVARCHAR(100);
