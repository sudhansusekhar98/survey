using BCrypt.Net;

namespace SurveyApp.Repo
{
    /// Service for secure password hashing using BCrypt.
    /// Provides methods to hash passwords and verify password attempts.
    public interface IPasswordHasher
    {
        /// Hash a plain text password using BCrypt.
        /// <param name="password">The plain text password to hash</param>
        /// <returns>The BCrypt hash of the password</returns>
        string HashPassword(string password);

        /// Verify if a plain text password matches a BCrypt hash.
        /// <param name="password">The plain text password to verify</param>
        /// <param name="hashedPassword">The stored BCrypt hash</param>
        /// <returns>True if the password matches the hash, false otherwise</returns>
        bool VerifyPassword(string password, string hashedPassword);

        /// Check if a stored password is a BCrypt hash or plain text.
        /// Used for migration of existing plain text passwords.
        /// <param name="storedPassword">The password value from the database</param>
        /// <returns>True if the password appears to be a BCrypt hash</returns>
        bool IsBCryptHash(string storedPassword);
    }

    /// Implementation of password hashing using BCrypt algorithm.
    /// BCrypt includes salt by default and is designed specifically for password hashing.
    public class PasswordHasher : IPasswordHasher
    {
        // BCrypt work factor (cost). Higher = more secure but slower.
        // 11 provides a good balance of security and performance.
        private const int WorkFactor = 11;

        /// Hash a plain text password using BCrypt.
        /// The resulting hash includes the salt, making each hash unique even for identical passwords.
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password), "Password cannot be null or empty");
            }

            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        /// Verify if a plain text password matches a BCrypt hash.
        /// Handles both BCrypt hashes and legacy plain text passwords for migration.
        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            {
                return false;
            }

            try
            {
                // Check if the stored password is a BCrypt hash
                if (IsBCryptHash(hashedPassword))
                {
                    // Verify using BCrypt
                    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                }
                else
                {
                    // Legacy plain text password - direct comparison
                    // This supports migration from plain text to hashed passwords
                    return password == hashedPassword;
                }
            }
            catch (SaltParseException)
            {
                // If BCrypt fails to parse, treat as plain text
                return password == hashedPassword;
            }
            catch
            {
                return false;
            }
        }

        /// Check if a stored password is a BCrypt hash.
        /// BCrypt hashes start with "$2a$", "$2b$", or "$2y$" followed by the work factor.
        public bool IsBCryptHash(string storedPassword)
        {
            if (string.IsNullOrEmpty(storedPassword))
            {
                return false;
            }

            // BCrypt hashes are always 60 characters and start with $2a$, $2b$, or $2y$
            return storedPassword.Length == 60 &&
                   (storedPassword.StartsWith("$2a$") ||
                    storedPassword.StartsWith("$2b$") ||
                    storedPassword.StartsWith("$2y$"));
        }
    }
}
