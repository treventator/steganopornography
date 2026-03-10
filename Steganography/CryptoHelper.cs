using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Steganography
{
    /// <summary>
    /// AES-256-CBC encrypt/decrypt สำหรับซ่อนข้อความ (salt คงที่เพื่อการศึกษา)
    /// </summary>
    public static class CryptoHelper
    {
        // salt คงที่ = "Stegano2025Educa" ในงานจริงควร random salt ทุกครั้ง
        // ref: https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.rfc2898derivebytes
        private static readonly byte[] AesSalt = new byte[]
        {
            0x53, 0x74, 0x65, 0x67, 0x61, 0x6E, 0x6F, 0x32,
            0x30, 0x32, 0x35, 0x45, 0x64, 0x75, 0x63, 0x61
        };

        // PBKDF2 iterations ยิ่งเยอะยิ่ง brute-force ยาก (NIST แนะนำขั้นต่ำ 10,000)
        private const int Pbkdf2Iterations = 10_000;

        private const int KeySizeBytes = 32;
        private const int IvSizeBytes = 16;

        public static string AesEncrypt(string plaintext, string password)
        {
            // สร้าง key + IV จาก password ผ่าน PBKDF2
            using (var keyGen = new Rfc2898DeriveBytes(password, AesSalt, Pbkdf2Iterations))
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Key = keyGen.GetBytes(KeySizeBytes);
                aes.IV = keyGen.GetBytes(IvSizeBytes);

                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] b = Encoding.UTF8.GetBytes(plaintext);
                    cs.Write(b, 0, b.Length);
                    cs.Close();

                    // encrypt แล้ว convert เป็น Base64 เพราะ ciphertext เป็น binary ส่งเป็น text ไม่ได้
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        // ถอดรหัส — ขั้นตอนย้อนกลับจาก Encrypt
        public static string AesDecrypt(string cipherBase64, string password)
        {
            byte[] cipherBytes;
            try { cipherBytes = Convert.FromBase64String(cipherBase64); }
            catch { throw new InvalidOperationException("ข้อมูลที่ถอดออกมาไม่ใช่ Base64 — เลือกเทคนิค/คู่ผิด หรือ Steganotext เสียหาย"); }

            using (var keyGen = new Rfc2898DeriveBytes(password, AesSalt, Pbkdf2Iterations))
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Key = keyGen.GetBytes(KeySizeBytes);
                aes.IV = keyGen.GetBytes(IvSizeBytes);

                try
                {
                    using (var ms = new MemoryStream(cipherBytes))
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs, Encoding.UTF8))
                        return sr.ReadToEnd();
                }
                catch
                {
                    // padding ผิด = key ผิดหรือ ciphertext โดนแก้
                    throw new InvalidOperationException("ถอดรหัสไม่สำเร็จ — Key ผิด หรือข้อมูลเสียหาย");
                }
            }
        }
    }
}
