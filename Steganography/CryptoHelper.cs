using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Steganography
{
    /// <summary>
    /// AES-256-CBC encryption/decryption with PBKDF2 key derivation.
    ///
    /// [C1 FIX] Salt is now random 16 bytes per encryption (prepended to output).
    /// [C2 FIX] HMAC-SHA256 Encrypt-then-MAC to prevent bit-flipping / padding oracle.
    /// [L3 FIX] Specific exception types with inner exception preserved.
    ///
    /// Output format: Base64( salt[16] + cipherBytes[N] + hmac[32] )
    /// </summary>
    public static class CryptoHelper
    {
        private const int SaltSize = 16;
        private const int Pbkdf2Iterations = 10_000;
        private const int KeySizeBytes = 32;   // AES-256
        private const int IvSizeBytes = 16;    // AES block size
        private const int HmacSizeBytes = 32;  // SHA-256

        public static string AesEncrypt(string plaintext, string password)
        {
            // [C1] Random salt every time
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            using (var keyGen = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations))
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Key = keyGen.GetBytes(KeySizeBytes);
                aes.IV = keyGen.GetBytes(IvSizeBytes);
                byte[] hmacKey = keyGen.GetBytes(KeySizeBytes); // separate HMAC key

                byte[] cipherBytes;
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] b = Encoding.UTF8.GetBytes(plaintext);
                    cs.Write(b, 0, b.Length);
                    cs.Close();
                    cipherBytes = ms.ToArray();
                }

                // [C2] HMAC-SHA256 over salt + cipherBytes
                byte[] dataToMac = new byte[salt.Length + cipherBytes.Length];
                Buffer.BlockCopy(salt, 0, dataToMac, 0, salt.Length);
                Buffer.BlockCopy(cipherBytes, 0, dataToMac, salt.Length, cipherBytes.Length);

                byte[] hmac;
                using (var hmacSha = new HMACSHA256(hmacKey))
                    hmac = hmacSha.ComputeHash(dataToMac);

                // Output: salt[16] + cipherBytes[N] + hmac[32]
                byte[] result = new byte[salt.Length + cipherBytes.Length + hmac.Length];
                Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
                Buffer.BlockCopy(cipherBytes, 0, result, salt.Length, cipherBytes.Length);
                Buffer.BlockCopy(hmac, 0, result, salt.Length + cipherBytes.Length, hmac.Length);

                return Convert.ToBase64String(result);
            }
        }

        public static string AesDecrypt(string cipherBase64, string password)
        {
            // [L3] Specific exception for Base64
            byte[] raw;
            try
            {
                raw = Convert.FromBase64String(cipherBase64);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException(
                    "ข้อมูลที่ถอดออกมาไม่ใช่ Base64 — เลือกเทคนิค/คู่ผิด หรือ Steganotext เสียหาย", ex);
            }

            int minSize = SaltSize + 16 + HmacSizeBytes; // salt + 1 AES block + hmac
            if (raw.Length < minSize)
                throw new InvalidOperationException(
                    $"ข้อมูล ciphertext สั้นเกินไป (ต้องการอย่างน้อย {minSize} bytes แต่ได้ {raw.Length})");

            // Split: salt[16] + cipherBytes[N] + hmac[32]
            byte[] salt = new byte[SaltSize];
            byte[] hmac = new byte[HmacSizeBytes];
            byte[] cipherBytes = new byte[raw.Length - SaltSize - HmacSizeBytes];

            Buffer.BlockCopy(raw, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(raw, SaltSize, cipherBytes, 0, cipherBytes.Length);
            Buffer.BlockCopy(raw, raw.Length - HmacSizeBytes, hmac, 0, HmacSizeBytes);

            using (var keyGen = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations))
            {
                byte[] key = keyGen.GetBytes(KeySizeBytes);
                byte[] iv = keyGen.GetBytes(IvSizeBytes);
                byte[] hmacKey = keyGen.GetBytes(KeySizeBytes);

                // [C2] Verify HMAC first (before attempting decrypt)
                byte[] dataToMac = new byte[salt.Length + cipherBytes.Length];
                Buffer.BlockCopy(salt, 0, dataToMac, 0, salt.Length);
                Buffer.BlockCopy(cipherBytes, 0, dataToMac, salt.Length, cipherBytes.Length);

                byte[] computedHmac;
                using (var hmacSha = new HMACSHA256(hmacKey))
                    computedHmac = hmacSha.ComputeHash(dataToMac);

                if (!ConstantTimeEquals(computedHmac, hmac))
                    throw new InvalidOperationException(
                        "ถอดรหัสไม่สำเร็จ — Key ผิด หรือข้อมูลถูกดัดแปลง (HMAC ไม่ตรง)");

                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.Key = key;
                    aes.IV = iv;

                    try
                    {
                        using (var ms = new MemoryStream(cipherBytes))
                        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        using (var sr = new StreamReader(cs, Encoding.UTF8))
                            return sr.ReadToEnd();
                    }
                    catch (CryptographicException ex)
                    {
                        throw new InvalidOperationException(
                            "ถอดรหัสไม่สำเร็จ — ข้อมูลเสียหาย (padding ผิดพลาด)", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Constant-time comparison to prevent timing attacks on HMAC verification
        /// </summary>
        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
