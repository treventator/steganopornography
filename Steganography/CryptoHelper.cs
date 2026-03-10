// นำเข้า namespace System ซึ่งเป็น namespace หลักของ .NET ที่มี class พื้นฐาน เช่น Convert, InvalidOperationException
using System;
// นำเข้า System.IO สำหรับจัดการ stream (กระแสข้อมูล) เช่น MemoryStream ที่ใช้เก็บข้อมูลในหน่วยความจำชั่วคราวระหว่างเข้ารหัส/ถอดรหัส
using System.IO;
// นำเข้า System.Security.Cryptography ซึ่งมี class สำหรับการเข้ารหัสลับ เช่น Aes, Rfc2898DeriveBytes (PBKDF2), CryptoStream
using System.Security.Cryptography;
// นำเข้า System.Text สำหรับ Encoding.UTF8 ที่ใช้แปลง string ↔ byte[] ในรูปแบบ UTF-8 (รองรับภาษาไทย)
using System.Text;

// ประกาศ namespace "Steganography" เพื่อจัดกลุ่ม class ทั้งหมดในโปรเจคนี้ให้อยู่ภายใต้ชื่อเดียวกัน
namespace Steganography
{
    /// <summary>
    /// คลาสสำหรับเข้ารหัสและถอดรหัสด้วย AES-256-CBC พร้อม PBKDF2 key derivation
    /// Salt เป็นค่าคงที่สำหรับงานการศึกษา — ในระบบจริงควรสุ่ม salt ใหม่ทุกครั้ง
    /// </summary>
    //
    // ประกาศ class เป็น public (เข้าถึงได้จากทุกที่) และ static (ไม่ต้องสร้าง instance, เรียกใช้ผ่านชื่อ class ได้เลย)
    // เหตุผลที่ใช้ static: เพราะ class นี้เป็นเครื่องมือ (utility) ที่ไม่ต้องเก็บ state ใดๆ เป็นรายวัตถุ
    public static class CryptoHelper
    {
        // ประกาศ Salt (เกลือ) เป็น byte array ขนาด 16 bytes ที่ใช้ผสมกับ password ตอนสร้าง key
        // private: เข้าถึงได้เฉพาะใน class นี้เท่านั้น
        // static: ใช้ร่วมกันทั้ง class โดยไม่ต้องสร้าง instance
        // readonly: กำหนดค่าได้ครั้งเดียวตอนประกาศหรือใน constructor เท่านั้น ป้องกันการแก้ไขทีหลัง
        //
        // ทำไมต้องมี Salt?
        //   - Salt ทำให้แม้ password เดียวกันจะได้ key ที่ต่างกัน (ถ้า salt ต่างกัน)
        //   - ป้องกัน Rainbow Table Attack (ตารางค้นหา hash ที่คำนวณไว้ล่วงหน้า)
        //   - ในโค้ดนี้ salt คงที่ = "Stegano2025Educa" ในรูป ASCII hex เพื่อความง่ายในการศึกษา
        //   - 0x53='S', 0x74='t', 0x65='e', 0x67='g', 0x61='a', 0x6E='n', 0x6F='o', 0x32='2'
        //   - 0x30='0', 0x32='2', 0x35='5', 0x45='E', 0x64='d', 0x75='u', 0x63='c', 0x61='a'
        private static readonly byte[] AesSalt = new byte[]
        {
            0x53, 0x74, 0x65, 0x67, 0x61, 0x6E, 0x6F, 0x32, // "Stegano2" ในรูป hexadecimal
            0x30, 0x32, 0x35, 0x45, 0x64, 0x75, 0x63, 0x61  // "025Educa" ในรูป hexadecimal
        };

        // จำนวนรอบที่ PBKDF2 จะ hash ซ้ำ (10,000 รอบ)
        // ยิ่งมากยิ่งช้า → ทำให้ brute-force ยากขึ้น เพราะผู้โจมตีต้องคำนวณ 10,000 รอบต่อการลอง password 1 ตัว
        // ค่า 10,000 เป็นค่าขั้นต่ำที่แนะนำโดย NIST (สถาบันมาตรฐานของสหรัฐฯ)
        private const int Pbkdf2Iterations = 10_000;

        // ขนาด key ของ AES-256 = 32 bytes = 256 bits
        // AES รองรับ 3 ขนาด: 128, 192, 256 bits — เราเลือก 256 bits ซึ่งแข็งแรงที่สุด
        private const int KeySizeBytes = 32;

        // ขนาด IV (Initialization Vector) = 16 bytes = 128 bits
        // IV ใช้คู่กับ CBC mode เพื่อทำให้ ciphertext block แรกไม่คาดเดาได้
        // ขนาด IV = ขนาด block ของ AES ซึ่งเป็น 128 bits เสมอ (ไม่ขึ้นกับขนาด key)
        private const int IvSizeBytes = 16;

        // ฟังก์ชันเข้ารหัส AES-256-CBC
        // รับ plaintext (ข้อความดิบ) และ password (รหัสผ่าน) แล้วส่งคืน ciphertext ในรูป Base64 string
        // ใช้ public static เพื่อให้เรียกจากภายนอกได้โดยไม่ต้องสร้าง object: CryptoHelper.AesEncrypt(...)
        public static string AesEncrypt(string plaintext, string password)
        {
            // สร้าง Rfc2898DeriveBytes (PBKDF2 — Password-Based Key Derivation Function 2)
            // PBKDF2 จะนำ password + salt มา hash ซ้ำ 10,000 รอบ เพื่อสร้าง key material ที่ปลอดภัย
            // ใช้ "using" เพื่อให้ .NET เรียก Dispose() อัตโนมัติเมื่อจบ block → ปลดปล่อยทรัพยากร cryptographic
            using (var keyGen = new Rfc2898DeriveBytes(password, AesSalt, Pbkdf2Iterations))
            // สร้าง Aes object ซึ่งเป็น symmetric encryption algorithm (เข้ารหัสและถอดรหัสด้วย key เดียวกัน)
            // Aes.Create() จะสร้าง instance ของ AES provider ที่เหมาะสมกับระบบปฏิบัติการ
            using (Aes aes = Aes.Create())
            {
                // กำหนดขนาด key เป็น 256 bits (AES-256) ซึ่งเป็นระดับการเข้ารหัสที่แข็งแกร่งที่สุดของ AES
                aes.KeySize = 256;

                // ดึง 32 bytes จาก PBKDF2 output มาใช้เป็น encryption key
                // PBKDF2 จะสร้าง pseudo-random bytes จาก password+salt ที่ hash ซ้ำ 10,000 รอบ
                aes.Key = keyGen.GetBytes(KeySizeBytes);

                // ดึง 16 bytes ถัดไปจาก PBKDF2 output มาใช้เป็น IV (Initialization Vector)
                // IV ทำให้แม้เข้ารหัสข้อความเดิมด้วย key เดิม ผลลัพธ์ก็จะต่างกันได้ (ถ้า IV ต่างกัน)
                // ในที่นี้เนื่องจาก salt คงที่ → password เดียวกันจะได้ IV เดียวกันเสมอ (ข้อจำกัดเพื่อการศึกษา)
                // CBC mode: แต่ละ block ของ plaintext จะถูก XOR กับ ciphertext block ก่อนหน้า
                //           block แรกจะ XOR กับ IV แทน → ทำให้ pattern ใน plaintext ถูกกลบ
                aes.IV = keyGen.GetBytes(IvSizeBytes);

                // สร้าง MemoryStream เพื่อเก็บ ciphertext (ข้อมูลที่เข้ารหัสแล้ว) ในหน่วยความจำ
                // MemoryStream ทำงานเหมือนไฟล์แต่อยู่ใน RAM → เร็วกว่าเขียนลงดิสก์
                using (var ms = new MemoryStream())
                // สร้าง CryptoStream ครอบ MemoryStream โดยใช้ Encryptor
                // CryptoStream จะเข้ารหัสข้อมูลทุกอย่างที่เขียนเข้าไป แล้วส่งผลลัพธ์ไปยัง MemoryStream
                // CryptoStreamMode.Write = เราจะ "เขียน" plaintext เข้า stream แล้วมันจะเข้ารหัสให้
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    // แปลง plaintext (string) เป็น byte array ด้วย UTF-8 encoding
                    // UTF-8 รองรับทุกภาษา (รวมภาษาไทย) โดยใช้ 1-4 bytes ต่อตัวอักษร
                    // AES ทำงานกับ bytes ไม่ใช่ string ดังนั้นต้องแปลงก่อน
                    byte[] b = Encoding.UTF8.GetBytes(plaintext);

                    // เขียน byte array ทั้งหมดเข้า CryptoStream
                    // CryptoStream จะเข้ารหัส AES-256-CBC ทีละ block (128 bits = 16 bytes)
                    // พารามิเตอร์: buffer, offset (เริ่มที่ตำแหน่ง 0), count (จำนวน bytes ทั้งหมด)
                    cs.Write(b, 0, b.Length);

                    // ปิด CryptoStream เพื่อ:
                    // 1) Flush ข้อมูลที่ค้างอยู่ใน buffer
                    // 2) เพิ่ม PKCS7 padding ให้ block สุดท้ายครบ 16 bytes
                    //    (เช่น ถ้าข้อมูลเหลือ 10 bytes → เติม 0x06 อีก 6 bytes ให้ครบ 16)
                    cs.Close();

                    // อ่าน ciphertext bytes ทั้งหมดจาก MemoryStream แล้วแปลงเป็น Base64 string
                    // Base64 แปลง binary data เป็นตัวอักษร A-Z, a-z, 0-9, +, / ที่ปลอดภัยสำหรับ text
                    // เหตุผลที่ใช้ Base64: เพราะ ciphertext เป็น binary ที่อาจมีค่า byte ใดก็ได้ (0x00-0xFF)
                    // ถ้าเก็บเป็น string ตรงๆ อาจเสียหาย → Base64 ทำให้ส่งผ่าน text ได้อย่างปลอดภัย
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        // ฟังก์ชันถอดรหัส AES-256-CBC
        // รับ cipherBase64 (ciphertext ในรูป Base64) และ password แล้วส่งคืน plaintext เดิม
        // กระบวนการเป็นขั้นตอนย้อนกลับจาก AesEncrypt ทุกประการ
        public static string AesDecrypt(string cipherBase64, string password)
        {
            // ประกาศตัวแปรสำหรับเก็บ ciphertext ที่แปลงจาก Base64 กลับเป็น byte array
            byte[] cipherBytes;
            // ลองแปลง Base64 string กลับเป็น byte array
            // ถ้าข้อมูลไม่ใช่ Base64 ที่ถูกต้อง (เช่น มีตัวอักษรแปลกปลอม หรือข้อมูลเสียหาย)
            // Convert.FromBase64String จะ throw FormatException
            // เราจับ exception ทุกชนิดแล้ว throw ข้อความภาษาไทยที่เข้าใจง่ายแทน
            // สาเหตุที่ Base64 อาจผิด: ผู้ใช้เลือกเทคนิค steganography ผิด หรือ steganotext ถูกแก้ไข
            try { cipherBytes = Convert.FromBase64String(cipherBase64); }
            catch { throw new InvalidOperationException("ข้อมูลที่ถอดออกมาไม่ใช่ Base64 — เลือกเทคนิค/คู่ผิด หรือ Steganotext เสียหาย"); }

            // สร้าง PBKDF2 key generator ด้วย password + salt + iterations เดียวกับตอนเข้ารหัส
            // สำคัญมาก: ต้องใช้ค่าเดียวกันทุกอย่าง ไม่เช่นนั้นจะได้ key/IV คนละตัว → ถอดรหัสไม่ได้
            using (var keyGen = new Rfc2898DeriveBytes(password, AesSalt, Pbkdf2Iterations))
            // สร้าง Aes object สำหรับถอดรหัส (ใช้ algorithm เดียวกับตอนเข้ารหัส)
            using (Aes aes = Aes.Create())
            {
                // กำหนดขนาด key เป็น 256 bits เหมือนตอนเข้ารหัส
                aes.KeySize = 256;
                // สร้าง key จาก PBKDF2 — จะได้ค่าเดียวกับตอนเข้ารหัสเพราะ password+salt+iterations เหมือนกัน
                aes.Key = keyGen.GetBytes(KeySizeBytes);
                // สร้าง IV จาก PBKDF2 — ต้องเป็นค่าเดียวกับตอนเข้ารหัสจึงจะถอดรหัสได้ถูกต้อง
                aes.IV = keyGen.GetBytes(IvSizeBytes);

                // ลองถอดรหัส — ถ้า key ผิดหรือข้อมูลเสียหาย AES จะ throw CryptographicException
                // เพราะ padding ของ block สุดท้ายจะไม่ถูกต้อง (PKCS7 padding validation fails)
                try
                {
                    // สร้าง MemoryStream จาก cipherBytes เพื่อให้ CryptoStream อ่านข้อมูลจากมัน
                    // ต่างจากตอนเข้ารหัสที่สร้าง MemoryStream เปล่า — ตอนนี้ใส่ข้อมูลเข้าไปเลย
                    using (var ms = new MemoryStream(cipherBytes))
                    // สร้าง CryptoStream ในโหมด Read + Decryptor
                    // เมื่ออ่านจาก CryptoStream มันจะดึงข้อมูลจาก MemoryStream → ถอดรหัส → ส่งคืน plaintext bytes
                    // CryptoStreamMode.Read = เราจะ "อ่าน" plaintext ออกมาจาก stream ที่ถอดรหัสแล้ว
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    // สร้าง StreamReader ครอบ CryptoStream เพื่อแปลง bytes → string ด้วย UTF-8 encoding
                    // StreamReader ทำให้อ่านข้อมูลเป็น string ได้สะดวก แทนที่จะอ่านเป็น byte[] แล้วแปลงเอง
                    using (var sr = new StreamReader(cs, Encoding.UTF8))
                        // อ่านข้อมูลทั้งหมดจาก StreamReader แล้วส่งคืนเป็น plaintext string
                        // ReadToEnd() จะอ่านจนหมด stream → CryptoStream ถอดรหัสทุก block → StreamReader แปลง UTF-8 → string
                        return sr.ReadToEnd();
                }
                // ถ้าเกิด exception ใดๆ ระหว่างถอดรหัส (เช่น CryptographicException จาก padding ผิด)
                // แสดงว่า key ผิดหรือ ciphertext ถูกแก้ไข/เสียหาย
                catch
                {
                    // throw ข้อความภาษาไทยที่เข้าใจง่ายเพื่อแสดงในหน้า UI ให้ผู้ใช้ทราบ
                    throw new InvalidOperationException("ถอดรหัสไม่สำเร็จ — Key ผิด หรือข้อมูลเสียหาย");
                }
            }
        }
    }
}
