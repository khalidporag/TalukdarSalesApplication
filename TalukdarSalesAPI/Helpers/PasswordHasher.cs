using System.Security.Cryptography;

namespace TalukdarSalesAPI.Helpers
{
    public class PasswordHasher
    {
        private const string V2Prefix = "v2$";
        private const int SaltSize = 16;
        private const int V2HashSize = 32;
        private const int V2Iterations = 100000;

        // Legacy format: base64(salt[16] + PBKDF2-SHA1(10000 iterations)[20])
        private const int LegacyHashSize = 20;
        private const int LegacyIterations = 10000;

        public static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, V2Iterations, HashAlgorithmName.SHA256, V2HashSize);

            var hashBytes = new byte[SaltSize + V2HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, V2HashSize);

            return V2Prefix + Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            try
            {
                if (storedHash.StartsWith(V2Prefix))
                {
                    var bytes = Convert.FromBase64String(storedHash.Substring(V2Prefix.Length));
                    if (bytes.Length != SaltSize + V2HashSize)
                        return false;
                    var salt = bytes[..SaltSize];
                    var expected = bytes[SaltSize..];
                    var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, V2Iterations, HashAlgorithmName.SHA256, V2HashSize);
                    return CryptographicOperations.FixedTimeEquals(expected, actual);
                }

                var legacy = Convert.FromBase64String(storedHash);
                if (legacy.Length != SaltSize + LegacyHashSize)
                    return false;
                var legacySalt = legacy[..SaltSize];
                var legacyExpected = legacy[SaltSize..];
                var legacyActual = Rfc2898DeriveBytes.Pbkdf2(password, legacySalt, LegacyIterations, HashAlgorithmName.SHA1, LegacyHashSize);
                return CryptographicOperations.FixedTimeEquals(legacyExpected, legacyActual);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool NeedsRehash(string storedHash)
            => string.IsNullOrEmpty(storedHash) || !storedHash.StartsWith(V2Prefix);
    }
}
