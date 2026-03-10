// นำเข้า namespace System ซึ่งมี class พื้นฐาน เช่น Exception, InvalidOperationException ที่ใช้ throw ข้อผิดพลาด
using System;
// นำเข้า namespace Collections.Generic เพื่อใช้ List<T>, Dictionary<TKey,TValue>, HashSet<T> สำหรับเก็บข้อมูลแบบ generic
using System.Collections.Generic;
// นำเข้า namespace Text เพื่อใช้ StringBuilder (ต่อ string อย่างมีประสิทธิภาพ) และ Encoding.UTF8 (แปลง string ↔ bytes)
using System.Text;

// ประกาศ namespace Steganography เพื่อจัดกลุ่มคลาสทั้งหมดของโปรเจคนี้ไว้ด้วยกัน
namespace Steganography
{
    /// <summary>
    /// คลาส SteganographyEngine เป็นเครื่องมือหลัก (core engine) สำหรับฝังและดึงข้อความลับในข้อความปกปิด
    /// รองรับ 5 เทคนิค: Homoglyph, Misspelling, ZWSP, NBSP, Synonym
    ///
    /// ========== หลักการทำงานโดยรวม ==========
    ///
    /// การฝัง (Embed Flow):
    ///   1. รับ ciphertext (ข้อความที่เข้ารหัส AES-256 แล้ว อยู่ในรูป Base64 string)
    ///   2. แปลง ciphertext เป็น byte array ด้วย UTF-8 encoding
    ///   3. แปลง bytes เป็น bit array (แต่ละ byte = 8 bits) พร้อม header 32 bits ที่บอกความยาว
    ///   4. ฝัง bits เหล่านั้นลงใน covertext (ข้อความปกปิด) ด้วยเทคนิคที่เลือก
    ///   5. ได้ steganotext (ข้อความที่มีข้อความลับซ่อนอยู่)
    ///
    /// การดึง (Extract Flow):
    ///   1. รับ steganotext ที่มีข้อความลับซ่อนอยู่
    ///   2. อ่าน bits จาก steganotext ด้วยเทคนิคเดียวกัน
    ///   3. อ่าน 32-bit header เพื่อรู้ความยาวของ ciphertext
    ///   4. อ่าน data bits ตามจำนวนที่ระบุ แล้วแปลงกลับเป็น bytes
    ///   5. แปลง bytes กลับเป็น ciphertext string ด้วย UTF-8
    ///
    /// รูปแบบ payload ที่ฝัง:
    ///   [32-bit length header (big-endian)] [data bits ของ ciphertext bytes]
    ///   - length = จำนวน bytes ของ ciphertext (ไม่ใช่จำนวน bits)
    ///   - เก็บเป็น big-endian 32 บิต = 32 ตำแหน่งแรก
    ///   - ตัวอย่าง: ciphertext "ABC" = 3 bytes → header = 32 bits + data = 24 bits = รวม 56 bits
    /// </summary>
    // ประกาศคลาสเป็น static เพราะทุก method เป็น utility function ที่ไม่ต้องสร้าง instance (ไม่มี state ของ object)
    public static class SteganographyEngine
    {
        // ============================================================
        //  HOMOGLYPH — เทคนิคแรก
        //  หลักการ: ใช้ตัวอักษรที่หน้าตาคล้ายกันมาก (homoglyph) เช่น ฎ กับ ฏ
        //  มนุษย์อ่านแล้วแทบแยกไม่ออก แต่คอมพิวเตอร์อ่าน Unicode ต่างกัน
        //  จึงใช้ความแตกต่างนี้ในการซ่อน bit: ตัวจริง = bit 0, ตัวแทน = bit 1
        // ============================================================

        /// <summary>
        /// อาร์เรย์คู่ตัวอักษร Homoglyph ที่รองรับในระบบ
        /// แต่ละคู่ประกอบด้วย (Original = ตัวจริง, Glyph = ตัวแทนที่หน้าตาคล้ายกัน)
        /// เมื่อฝังข้อมูล: bit 0 → ใช้ตัว Original (index 0), bit 1 → ใช้ตัว Glyph (index 1)
        /// ผู้ใช้สามารถเลือกว่าจะใช้คู่ไหนบ้างผ่าน CheckBox ใน sub-form OptionStaganogryphy
        /// </summary>
        // ประกาศเป็น public static readonly เพราะเป็นข้อมูลคงที่ที่ sub-form ต้องเข้าถึงได้ และไม่ต้องการให้ถูกเปลี่ยนแปลง
        // ใช้ ValueTuple (char Original, char Glyph) เพื่อให้เข้าถึงด้วยชื่อ field ได้ชัดเจน
        public static readonly (char Original, char Glyph)[] HomoglyphPairs = new[]
        {
            ('ฎ', 'ฏ'),   // pair 0 — ฎ (ชฎา) กับ ฏ (ปฏัก) — ตรงกับ ChkbxDochada ใน sub-form
            ('ข', 'ฃ'),   // pair 1 — ข (ไข่) กับ ฃ (ขวด, ตัวที่เลิกใช้แล้ว) — ตรงกับ ChkbxKhoKhai
            ('ช', 'ซ'),   // pair 2 — ช (ช้าง) กับ ซ (โซ่) — ตรงกับ ChkbxChoChang
        };

        /// <summary>
        /// ฝัง ciphertext ลงใน covertext ด้วยเทคนิค Homoglyph
        ///
        /// ขั้นตอนการทำงาน:
        ///   1. สร้าง lookup dictionary สำหรับคู่ตัวอักษรที่ผู้ใช้เลือกใช้
        ///   2. แปลง ciphertext เป็น payload bits (32-bit header + data bits)
        ///   3. วนผ่าน covertext ทีละตัวอักษร ถ้าตัวนั้นเป็นตัวอักษรในคู่ที่เลือก
        ///      ก็แทนที่ด้วย original (bit=0) หรือ glyph (bit=1)
        ///   4. ถ้า covertext มีตัวอักษร homoglyph ไม่พอก็ throw exception
        /// </summary>
        /// <param name="coverText">ข้อความปกปิด ที่จะใช้เป็นตัวพาข้อมูล (ต้องมีตัวอักษรใน HomoglyphPairs เพียงพอ)</param>
        /// <param name="cipherText">ข้อความเข้ารหัสในรูป Base64 string ที่จะฝังเข้าไป</param>
        /// <param name="activePairs">รายการ index ของคู่ตัวอักษรที่ผู้ใช้เลือก เช่น {0, 2} หมายถึงใช้คู่ ฎ/ฏ และ ช/ซ</param>
        /// <returns>steganotext — ข้อความที่มี ciphertext ซ่อนอยู่แล้ว</returns>
        public static string HomoglyphEmbed(string coverText, string cipherText, IList<int> activePairs)
        {
            // ตรวจสอบว่าผู้ใช้เลือกคู่ตัวอักษรอย่างน้อย 1 คู่ เพราะถ้าไม่เลือกเลยจะไม่มีที่ฝังข้อมูล
            if (activePairs == null || activePairs.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกคู่ตัวอักษร Homoglyph อย่างน้อย 1 คู่");

            // สร้าง Dictionary สำหรับแปลงตัวอักษร: original → glyph (ใช้ตอน embed เพื่อแทนที่ตัวอักษร)
            var originalToGlyph = new Dictionary<char, char>();
            // สร้าง Dictionary สำหรับแปลงกลับ: glyph → original (ใช้ normalize ก่อน embed เพื่อป้องกัน double-encoding)
            var glyphToOriginal = new Dictionary<char, char>();
            // วนลูปผ่าน index ของคู่ที่ผู้ใช้เลือก เพื่อสร้าง lookup table ทั้งสองทิศทาง
            foreach (int idx in activePairs)
            {
                // ดึงคู่ตัวอักษรจากอาร์เรย์ HomoglyphPairs ตาม index ที่ผู้ใช้เลือก
                var p = HomoglyphPairs[idx];
                // ใส่ mapping: ตัวจริง → ตัวแทน (ใช้ตอนต้องการเปลี่ยนเป็น bit 1)
                originalToGlyph[p.Original] = p.Glyph;
                // ใส่ mapping ย้อนกลับ: ตัวแทน → ตัวจริง (ใช้ตอน normalize เพื่อคืนค่าเดิม)
                glyphToOriginal[p.Glyph] = p.Original;
            }

            // แปลง ciphertext เป็น payload bits: 32-bit header บอกความยาว + data bits ของทุก byte
            // ฟังก์ชัน StringToPayloadBits จะจัดการ encoding ทั้งหมดให้
            bool[] bits = StringToPayloadBits(cipherText);

            // สร้าง StringBuilder จาก covertext เพื่อให้แก้ไขตัวอักษรแต่ละตำแหน่งได้โดยตรง (O(1) per replacement)
            // StringBuilder ดีกว่าการต่อ string ธรรมดาเพราะ string ใน C# เป็น immutable — ทุกครั้งที่แก้จะสร้าง object ใหม่
            var result = new StringBuilder(coverText);
            // ตัวนับตำแหน่ง bit ปัจจุบันที่กำลังจะฝัง เริ่มจาก bit แรก (index 0)
            int bitIdx = 0;

            // === ขั้นตอนหลัก: วนผ่าน covertext ทีละตัวอักษร ===
            // เงื่อนไข: i < result.Length (ยังไม่ถึงท้าย covertext) AND bitIdx < bits.Length (ยังมี bit ที่ต้องฝัง)
            for (int i = 0; i < result.Length && bitIdx < bits.Length; i++)
            {
                // อ่านตัวอักษรที่ตำแหน่ง i ใน covertext
                char c = result[i];

                // === ขั้นตอน Normalize ===
                // ถ้าตัวอักษร c เป็น glyph อยู่แล้ว (เช่น มีคนเคยใส่ ฏ แทน ฎ ไว้ก่อนหน้า)
                // ให้แปลงกลับเป็น original ก่อน เพื่อป้องกันปัญหา double-encoding
                // เพราะถ้าไม่ normalize แล้ว covertext มี glyph อยู่แล้ว จะทำให้อ่าน bit ผิดตอน extract
                char original = glyphToOriginal.ContainsKey(c) ? glyphToOriginal[c] : c;

                // ตรวจว่าตัวอักษรนี้ (หลัง normalize) เป็นตัวที่สามารถใช้ฝัง bit ได้หรือไม่
                // คือต้องเป็นตัว original ของคู่ใดคู่หนึ่งที่ผู้ใช้เลือก
                if (originalToGlyph.ContainsKey(original))
                {
                    // === ฝัง 1 bit ===
                    // ถ้า bit ปัจจุบันเป็น true (bit 1) → แทนที่ด้วย glyph (ตัวแทน)
                    // ถ้า bit ปัจจุบันเป็น false (bit 0) → คงไว้เป็น original (ตัวจริง)
                    result[i] = bits[bitIdx] ? originalToGlyph[original] : original;
                    // เลื่อนไปฝัง bit ถัดไป
                    bitIdx++;
                }
                // ถ้าตัวอักษรนี้ไม่อยู่ในคู่ที่เลือก → ข้ามไป ไม่ฝัง bit (ตัวอักษรนี้เป็นแค่ "พื้นหลัง")
            }

            // === ตรวจสอบว่าฝังครบทุก bit หรือไม่ ===
            // ถ้า bitIdx < bits.Length หมายความว่า covertext มีตัวอักษร homoglyph น้อยกว่าจำนวน bit ที่ต้องฝัง
            // ผู้ใช้ต้องเปลี่ยน covertext ที่ยาวกว่า หรือเลือกคู่ตัวอักษรเพิ่ม
            if (bitIdx < bits.Length)
                throw new InvalidOperationException(
                    $"Covertext มีตัวอักษร Homoglyph ไม่เพียงพอ (ต้องการ {bits.Length} ตำแหน่ง แต่ได้ {bitIdx})");

            // แปลง StringBuilder กลับเป็น string แล้วส่งคืนเป็น steganotext
            return result.ToString();
        }

        /// <summary>
        /// ดึง ciphertext จาก steganotext ด้วยเทคนิค Homoglyph
        ///
        /// ขั้นตอนการทำงาน:
        ///   1. สร้าง HashSet ของตัวอักษร original และ glyph ที่ต้องสนใจ
        ///   2. วนผ่าน steganotext ทีละตัวอักษร
        ///   3. ถ้าเจอ original → บันทึก bit 0, ถ้าเจอ glyph → บันทึก bit 1
        ///   4. แปลง extracted bits กลับเป็น ciphertext string ผ่าน PayloadBitsToString
        /// </summary>
        /// <param name="steganotextInput">ข้อความ steganotext ที่มีข้อมูลลับซ่อนอยู่</param>
        /// <param name="activePairs">index ของคู่ตัวอักษรที่ใช้ตอน embed (ต้องเลือกเหมือนกัน)</param>
        /// <returns>ciphertext ที่ถูกดึงออกมา (Base64 string)</returns>
        public static string HomoglyphExtract(string steganotextInput, IList<int> activePairs)
        {
            // ตรวจสอบว่าผู้ใช้เลือกคู่ตัวอักษรอย่างน้อย 1 คู่ (ต้องตรงกับที่ใช้ตอน embed)
            if (activePairs == null || activePairs.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกคู่ตัวอักษร Homoglyph อย่างน้อย 1 คู่");

            // สร้าง HashSet เก็บตัวอักษร original ทั้งหมดที่ต้องสนใจ (ใช้ HashSet เพราะ lookup O(1))
            var originalSet = new HashSet<char>();
            // สร้าง HashSet เก็บตัวอักษร glyph ทั้งหมดที่ต้องสนใจ
            var glyphSet = new HashSet<char>();
            // สร้าง Dictionary สำหรับแปลง glyph → original (ใช้ในอนาคตถ้าต้องการ normalize)
            var glyphToOriginal = new Dictionary<char, char>();

            // วนลูปผ่าน index ของคู่ที่ผู้ใช้เลือก เพื่อเติมข้อมูลใน set ทั้งสอง
            foreach (int idx in activePairs)
            {
                // ดึงคู่ตัวอักษรจากอาร์เรย์ตาม index
                var p = HomoglyphPairs[idx];
                // เพิ่มตัว original ลงใน set (ใช้ตรวจว่าตัวอักษรไหนคือ bit 0)
                originalSet.Add(p.Original);
                // เพิ่มตัว glyph ลงใน set (ใช้ตรวจว่าตัวอักษรไหนคือ bit 1)
                glyphSet.Add(p.Glyph);
                // เก็บ mapping glyph → original ไว้ด้วย
                glyphToOriginal[p.Glyph] = p.Original;
            }

            // สร้าง list สำหรับเก็บ bits ที่ดึงออกมา (ยังไม่รู้จำนวนแน่นอนจึงใช้ List แทน array)
            var extractedBits = new List<bool>();
            // === ขั้นตอนหลัก: วนผ่าน steganotext ทีละตัวอักษร ===
            foreach (char c in steganotextInput)
            {
                // ถ้าตัวอักษรนี้เป็น original (ตัวจริง) → หมายถึง bit 0 ถูกฝังไว้ที่ตำแหน่งนี้
                if (originalSet.Contains(c))
                    extractedBits.Add(false); // false = bit 0
                // ถ้าตัวอักษรนี้เป็น glyph (ตัวแทน) → หมายถึง bit 1 ถูกฝังไว้ที่ตำแหน่งนี้
                else if (glyphSet.Contains(c))
                    extractedBits.Add(true);  // true = bit 1
                // ถ้าไม่ใช่ทั้ง original และ glyph → ข้ามไป ไม่เกี่ยวกับการฝัง
            }

            // แปลง extracted bits กลับเป็น ciphertext string
            // PayloadBitsToString จะอ่าน 32-bit header เพื่อรู้ความยาว แล้วอ่าน data bits ตามนั้น
            return PayloadBitsToString(extractedBits.ToArray());
        }

        // ============================================================
        //  MISSPELLING — เทคนิคที่ 2
        //  หลักการ: ใช้คู่คำที่สะกดถูกกับสะกดผิดในภาษาไทย
        //  คำถูก = bit 0, คำผิด = bit 1
        //  คนอ่านอาจไม่สังเกตคำผิดเล็กน้อย แต่คอมพิวเตอร์สามารถตรวจจับได้
        //  เหมาะกับ covertext ที่มีคำเหล่านี้ปรากฏอยู่หลายตำแหน่ง
        // ============================================================

        /// <summary>
        /// อาร์เรย์คู่คำ Misspelling 65 คู่ — เป็นคู่ (คำถูก, คำผิด) ภาษาไทย
        /// ใช้ในเทคนิค Misspelling Steganography
        ///
        /// หลักการ:
        ///   - bit 0 = ใช้คำถูก (Correct), bit 1 = ใช้คำผิด (Wrong)
        ///   - ลำดับ index ต้องตรงกับ checkedListBox1.Items ใน Misspelling.Designer.cs
        ///     เพราะ sub-form จะส่ง index ของรายการที่ผู้ใช้เลือกมาให้ engine
        ///   - คำถูก/ผิดจะถูกสลับกันใน covertext ตาม bit ที่ต้องฝัง
        ///
        /// ที่มา: คำที่คนไทยมักสะกดผิดบ่อย โดยเฉพาะศัพท์ราชการและกฎหมาย
        /// ข้อดี: ดูเป็นธรรมชาติ เพราะคนสะกดผิดกันบ่อยอยู่แล้ว
        /// ข้อเสีย: ความจุขึ้นกับจำนวนคำเหล่านี้ที่ปรากฏใน covertext
        /// </summary>
        // ประกาศเป็น public static readonly เพราะ sub-form Misspelling.cs ต้องเข้าถึงข้อมูลนี้ได้
        // ใช้ ValueTuple (string Correct, string Wrong) เพื่อให้โค้ดอ่านง่าย
        public static readonly (string Correct, string Wrong)[] MisspellingPairs = new[]
        {
            ("กบฏ",                      "กบฎ"),
            ("กฎหมาย",                   "กฏหมาย"),
            ("คณะรัฐมนตรี",             "คณะรัฐมลตรี"),
            ("คำนวณ",                    "คำนวน"),
            ("ข้อตกลง",                  "ข้อตกลง์"),
            ("ข้าราชการ",               "ข้าราชการ์"),
            ("งบการเงิน",               "งบการเงิน์"),
            ("งบประมาณ",                "งบประมาน"),
            ("จดทะเบียน",               "จดทะเบียณ"),
            ("จรรยาบรรณ",               "จรรยาบรรณ์"),
            ("ฉบับ",                     "ฉบับ์"),
            ("ฉันทามติ",                "ฉันทามต"),
            ("ชอบด้วยกฎหมาย",           "ชอบด้วยกฏหมาย"),
            ("ชุมนุม",                   "ชุมนุม์"),
            ("ญาณ",                      "ญาน"),
            ("ญัตติ",                    "ญัตต"),
            ("ฎีกา",                     "ฎีการ"),
            ("ฎีกาเลือกตั้ง",            "ฎีการเลือกตั้ง"),
            ("ฐิติ",                     "ฐิต"),
            ("ดุลพินิจ",                "ดุลพินิด"),
            ("ดุลยภาพ",                  "ดุลยภาภ"),
            ("ตราสาร",                   "ตราสาร์"),
            ("ตราประทับ",               "ตราประทับ์"),
            ("ถ้อยคำ",                   "ถ้อยคัม"),
            ("ถาวร",                     "ถาวรณ์"),
            ("ทรัพย์สิน",               "ทรัพสิน"),
            ("ทรัพยากร",                "ทรัพยากรณ์"),
            ("ธรรมจักร",                "ธรรมจักร์"),
            ("ธรรมาภิบาล",              "ธรรมาภิบาล์"),
            ("นโยบาย",                   "นโยบาย์"),
            ("นิติบุคคล",               "นิติบุคคล์"),
            ("บรรพชิต",                  "บรรพชิด"),
            ("บรรพบุรุษ",               "บรรพบุรษ"),
            ("ประชาธิปไตย",              "ประชาธิปไต"),
            ("ประกาศพระบรมราชโองการ",   "ประกาศพระบรมราชโองการ์"),
            ("ปรากฏ",                    "ปรากฎ"),
            ("ปฏิเสธ",                   "ปฎิเสธ"),
            ("พระราชบัญญัติ",            "พระราชบัญญัต"),
            ("พระราชกำหนด",             "พระราชกำหนด์"),
            ("พระราชกฤษฎีกา",            "พระราชกฤษฎีการ"),
            ("ภารกิจ",                   "ภาระกิจ"),
            ("รัฐธรรมนูญ",              "รัฐธรรมนูญ์"),
            ("ราชการ",                   "ราชการ์"),
            ("ราชกิจจานุเบกษา",         "ราชกิจจานุเบกษ์"),
            ("วาระ",                     "วาระ์"),
            ("วุฒิสภา",                  "วุฒิสภ์"),
            ("ศาลรัฐธรรมนูญ",           "ศาลรัฐธรรมนูญ์"),
            ("ศีลธรรม",                  "ศีลธรรณ์"),
            ("สังเกต",                   "สังเกตุ"),
            ("สร้างสรรค์",              "สร้างสรร"),
            ("สภาผู้แทนราษฎร",          "สภาผู้แทนราษฎร์"),
            ("อภิสิทธิ์",               "อภิสิทธิ์์"),
            ("อัธยาศัย",                "อัธยาสัย"),
            ("อัศจรรย์",                "อัศจรร"),
            ("อนุญาต",                   "อนุญาติ"),
            ("อนุมัติ",                  "อนุมัต"),
            ("อนุสัญญา",                "อนุสัญญ์"),
            ("อัตโนมัติ",               "อัตโนมัต"),
            ("อุปกรณ์",                  "อุปกรณ์์"),
            ("ละแวก",                    "ระแวก"),
            ("ละเว้น",                   "ระแว้น"),
            ("เซ็นชื่อ",                "เซ็นต์ชื่อ"),
            ("เบรก",                     "เบรค"),
            ("ทยอย",                     "ทะยอย"),
            ("ผูกพัน",                   "ผูกพันธ์"),
        };

        /// <summary>
        /// ฝัง ciphertext ลงใน covertext ด้วยเทคนิค Misspelling
        ///
        /// ขั้นตอนการทำงาน:
        ///   1. แปลง ciphertext เป็น payload bits
        ///   2. ค้นหาตำแหน่งทุกคำ (ถูก/ผิด) ใน covertext ที่อยู่ในคู่ที่ผู้ใช้เลือก
        ///   3. วนผ่านตำแหน่งที่พบ แทนที่คำด้วยคำถูก (bit=0) หรือคำผิด (bit=1)
        ///   4. ใช้ offset tracking เพราะคำถูก/ผิดอาจมีความยาวต่างกัน ทำให้ตำแหน่งเลื่อน
        /// </summary>
        /// <param name="coverText">ข้อความปกปิดที่จะใช้ฝัง (ต้องมีคำจากคู่ที่เลือกเพียงพอ)</param>
        /// <param name="cipherText">ข้อความเข้ารหัสในรูป Base64 string</param>
        /// <param name="activePairIndices">index ของคู่คำที่ผู้ใช้เลือก (ต้องมีอยู่ใน covertext จึงจะใช้ได้)</param>
        /// <returns>steganotext — ข้อความที่มี ciphertext ซ่อนอยู่แล้ว</returns>
        public static string MisspellingEmbed(string coverText, string cipherText, IList<int> activePairIndices)
        {
            // ตรวจสอบว่าผู้ใช้เลือกคู่คำอย่างน้อย 1 คู่ เพราะถ้าไม่เลือกเลยจะไม่มีที่ฝังข้อมูล
            if (activePairIndices == null || activePairIndices.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกคู่คำ Misspelling อย่างน้อย 1 คู่");

            // แปลง ciphertext เป็น payload bits (32-bit header + data bits)
            bool[] bits = StringToPayloadBits(cipherText);

            // สร้าง StringBuilder จาก covertext เพื่อให้สามารถ Remove/Insert คำได้อย่างมีประสิทธิภาพ
            var sb = new StringBuilder(coverText);
            // ค้นหาตำแหน่งของทุกคำ (ถูก/ผิด) ใน covertext ที่ตรงกับคู่ที่ผู้ใช้เลือก
            // ฟังก์ชัน FindMisspellingCandidates จะค้นหาแบบ greedy left-to-right, non-overlapping
            var candidates = FindMisspellingCandidates(coverText, activePairIndices);

            // offset ใช้ติดตามการเลื่อนตำแหน่งหลังจากแทนที่คำ
            // เพราะคำถูกกับคำผิดอาจมีความยาวต่างกัน ทำให้ตำแหน่งของคำถัดไปเลื่อนจากเดิม
            int offset = 0;
            // ตัวนับ bit ปัจจุบันที่กำลังจะฝัง
            int bitIdx = 0;

            // === ขั้นตอนหลัก: วนผ่านทุกตำแหน่งที่พบคำ แล้วแทนที่ตาม bit ===
            // ใช้ tuple deconstruction: pos = ตำแหน่งใน text ดั้งเดิม, pairIdx = index ของคู่คำ,
            // isCorrect = คำที่เจอเป็นคำถูกหรือไม่, word = คำที่เจอจริงๆ
            foreach (var (pos, pairIdx, isCorrect, word) in candidates)
            {
                // ถ้าฝังครบทุก bit แล้ว ไม่ต้องแทนที่คำอีก
                if (bitIdx >= bits.Length) break;

                // อ่าน bit ปัจจุบัน: true = ต้องการ bit 1 (คำผิด), false = ต้องการ bit 0 (คำถูก)
                bool wantBit1 = bits[bitIdx];
                // เลือกคำเป้าหมายตาม bit: bit 1 → คำผิด (Wrong), bit 0 → คำถูก (Correct)
                string target = wantBit1
                    ? MisspellingPairs[pairIdx].Wrong
                    : MisspellingPairs[pairIdx].Correct;

                // คำนวณตำแหน่งจริงใน StringBuilder โดยบวก offset ที่สะสมจากการแทนที่ครั้งก่อนๆ
                int adjustedPos = pos + offset;
                // ลบคำเดิมออกจาก StringBuilder (ลบตั้งแต่ตำแหน่ง adjustedPos เป็นจำนวน word.Length ตัวอักษร)
                sb.Remove(adjustedPos, word.Length);
                // แทรกคำเป้าหมาย (ถูกหรือผิดตาม bit) เข้าที่ตำแหน่งเดิม
                sb.Insert(adjustedPos, target);
                // อัปเดต offset: ถ้าคำเป้าหมายยาวกว่าคำเดิม offset จะเพิ่ม, สั้นกว่าจะลด
                offset += target.Length - word.Length;
                // เลื่อนไปฝัง bit ถัดไป
                bitIdx++;
            }

            // ตรวจสอบว่าฝังครบทุก bit หรือไม่ — ถ้าไม่ครบแสดงว่า covertext มีคำไม่เพียงพอ
            if (bitIdx < bits.Length)
                throw new InvalidOperationException(
                    $"Covertext มีคำ Misspelling ไม่เพียงพอ (ต้องการ {bits.Length} คำ แต่ได้ {bitIdx})");

            // แปลง StringBuilder กลับเป็น string แล้วส่งคืนเป็น steganotext
            return sb.ToString();
        }

        /// <summary>
        /// ดึง ciphertext จาก steganotext ด้วยเทคนิค Misspelling
        ///
        /// ขั้นตอนการทำงาน:
        ///   1. ค้นหาตำแหน่งทุกคำ (ถูก/ผิด) ใน steganotext
        ///   2. คำถูก = bit 0, คำผิด = bit 1
        ///   3. แปลง extracted bits กลับเป็น ciphertext string
        /// </summary>
        /// <param name="steganotext">ข้อความ steganotext ที่มีข้อมูลลับซ่อนอยู่</param>
        /// <param name="activePairIndices">index ของคู่คำที่ใช้ตอน embed (ต้องเลือกเหมือนกัน)</param>
        /// <returns>ciphertext ที่ถูกดึงออกมา (Base64 string)</returns>
        public static string MisspellingExtract(string steganotext, IList<int> activePairIndices)
        {
            // ตรวจสอบว่าผู้ใช้เลือกคู่คำอย่างน้อย 1 คู่ (ต้องตรงกับที่ใช้ตอน embed)
            if (activePairIndices == null || activePairIndices.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกคู่คำ Misspelling อย่างน้อย 1 คู่");

            // ค้นหาตำแหน่งของทุกคำ (ถูก/ผิด) ใน steganotext ที่ตรงกับคู่ที่ผู้ใช้เลือก
            var candidates = FindMisspellingCandidates(steganotext, activePairIndices);
            // สร้าง list สำหรับเก็บ bits ที่ดึงออกมา
            var extractedBits = new List<bool>();

            // วนผ่านทุกตำแหน่งที่พบคำ แล้วอ่าน bit
            // ใช้ _ (discard) สำหรับ pos และ word เพราะไม่ต้องใช้ในการ extract
            foreach (var (_, pairIdx, isCorrect, _) in candidates)
            {
                // isCorrect = true → คำถูก → bit 0 (false)
                // isCorrect = false → คำผิด → bit 1 (true)
                // กลับค่า isCorrect ด้วย ! เพื่อให้ได้ bit ที่ถูกต้อง
                extractedBits.Add(!isCorrect);
            }

            // แปลง extracted bits กลับเป็น ciphertext string ผ่าน PayloadBitsToString
            return PayloadBitsToString(extractedBits.ToArray());
        }

        // ============================================================
        //  ZWSP (Zero-Width Space U+200B) — เทคนิคที่ 3
        //  หลักการ: แทรกอักขระ Zero-Width Space (ความกว้างเป็นศูนย์ มองไม่เห็น)
        //           ระหว่างตัวอักษรแต่ละคู่ใน covertext
        //  ช่องระหว่างตัวอักษร char[i] กับ char[i+1]:
        //    - มี ZWSP = bit 1
        //    - ไม่มี ZWSP = bit 0
        //  ความจุ = coverText.Length - 1 bits
        //  ข้อดี: ไม่ต้องเลือก options, ความจุสูง, มองไม่เห็นด้วยตาเปล่า
        //  ข้อเสีย: copy-paste บางโปรแกรมอาจตัด ZWSP ออก
        // ============================================================

        // ค่าคงที่ ZWSP (Zero-Width Space) — Unicode U+200B — ตัวอักษรที่ไม่แสดงบนหน้าจอ ใช้เป็นตัวพา bit
        private const char ZWSP = '\u200B';
        // ค่าคงที่ NBSP (Non-Breaking Space) — Unicode U+00A0 — เหมือน space แต่ไม่ให้ตัดบรรทัด
        // ใช้ทั้งในเทคนิค NBSP และใน NormalizeCovertext
        private const char NBSP = '\u00A0';

        /// <summary>
        /// Normalize covertext ก่อน embed — ทำความสะอาดตัวอักษรพิเศษที่อาจรบกวนการฝัง
        ///
        /// เหตุผล: ถ้า covertext มี ZWSP หรือ NBSP อยู่แล้วก่อนฝัง
        ///         จะทำให้ตอน extract อ่าน bit ผิดเพราะมี invisible char ปลอมปน
        ///         จึงต้องลบ ZWSP ออก และแปลง NBSP เป็น space ปกติก่อน
        /// </summary>
        /// <param name="coverText">ข้อความปกปิดดั้งเดิม</param>
        /// <returns>ข้อความที่ทำความสะอาดแล้ว (ไม่มี ZWSP, NBSP ถูกแปลงเป็น space ปกติ)</returns>
        private static string NormalizeCovertext(string coverText)
        {
            // สร้าง StringBuilder ขนาดเท่ากับ covertext เดิม เพื่อประสิทธิภาพ (หลีกเลี่ยง resize)
            var sb = new StringBuilder(coverText.Length);
            // วนผ่านทุกตัวอักษรใน covertext
            foreach (char c in coverText)
            {
                // ถ้าเป็น ZWSP → ข้ามไป ไม่เอาเข้า result (ลบออก)
                // เพราะ ZWSP จะถูกเพิ่มกลับมาตอน embed ตาม bit ที่ต้องฝัง
                if (c == ZWSP) continue;
                // ถ้าเป็น NBSP → แปลงเป็น space ปกติ (U+0020) แล้วเอาเข้า result
                // เพราะ NBSP จะถูกใช้ในเทคนิค NBSP embed ไม่ควรปนมาก่อน
                if (c == NBSP) { sb.Append(' '); continue; }
                // ตัวอักษรปกติ → เอาเข้า result ตามปกติ
                sb.Append(c);
            }
            // ส่งคืนข้อความที่ทำความสะอาดแล้ว
            return sb.ToString();
        }

        /// <summary>
        /// ฝัง ciphertext ลงใน covertext ด้วยเทคนิค Zero-Width Space (ZWSP)
        ///
        /// ขั้นตอนการทำงาน:
        ///   1. Normalize covertext (ลบ ZWSP/NBSP เดิมออก)
        ///   2. แปลง ciphertext เป็น payload bits
        ///   3. วนผ่าน covertext: หลังตัวอักษรแต่ละตัว
        ///      ถ้า bit=1 → แทรก ZWSP, ถ้า bit=0 → ไม่แทรก
        ///   4. ตำแหน่งที่เกินจำนวน bits → ไม่แทรก ZWSP (ปล่อยไว้เหมือนเดิม)
        /// </summary>
        /// <param name="coverText">ข้อความปกปิด</param>
        /// <param name="cipherText">ข้อความเข้ารหัสในรูป Base64 string</param>
        /// <returns>steganotext ที่มี ZWSP ซ่อนอยู่ระหว่างตัวอักษร</returns>
        public static string ZWSPEmbed(string coverText, string cipherText)
        {
            // ทำความสะอาด covertext ก่อน: ลบ ZWSP และแปลง NBSP เป็น space ปกติ
            coverText = NormalizeCovertext(coverText);
            // แปลง ciphertext เป็น payload bits (32-bit header + data bits)
            bool[] bits = StringToPayloadBits(cipherText);

            // คำนวณจำนวน "ช่อง" (slot) ที่สามารถฝัง bit ได้
            // ช่อง = ระหว่างตัวอักษรแต่ละคู่ = จำนวนตัวอักษร - 1
            // เช่น "ABCD" มี 4 ตัวอักษร → 3 ช่อง (AB, BC, CD)
            int slots = coverText.Length - 1;
            // ตรวจสอบว่าช่องเพียงพอสำหรับจำนวน bits ที่ต้องฝัง
            if (slots < bits.Length)
                throw new InvalidOperationException(
                    $"Covertext สั้นเกินไป (ต้องการ {bits.Length + 1} ตัวอักษร แต่ได้ {coverText.Length})");

            // สร้าง StringBuilder สำหรับสร้าง steganotext (ไม่กำหนดขนาดเริ่มต้นเพราะอาจยาวกว่าเดิม)
            var sb = new StringBuilder();
            // === ขั้นตอนหลัก: วนผ่าน covertext ทีละตัวอักษร ===
            for (int i = 0; i < coverText.Length; i++)
            {
                // เพิ่มตัวอักษรปัจจุบันเข้าไปใน result ก่อน
                sb.Append(coverText[i]);
                // ตรวจว่ายังมี bit ที่ต้องฝังอยู่ (i < bits.Length)
                // ช่อง i = ระหว่าง char[i] กับ char[i+1]
                if (i < bits.Length)
                {
                    // ถ้า bit เป็น true (bit 1) → แทรก ZWSP หลังตัวอักษรนี้
                    // ถ้า bit เป็น false (bit 0) → ไม่แทรกอะไร (ไม่มี ZWSP = bit 0)
                    if (bits[i]) sb.Append(ZWSP);
                }
                // ถ้า i >= bits.Length → ไม่มี bit ที่ต้องฝังแล้ว → แค่เพิ่มตัวอักษรเฉยๆ
            }
            // แปลง StringBuilder เป็น string แล้วส่งคืนเป็น steganotext
            return sb.ToString();
        }

        /// <summary>
        /// ดึง ciphertext จาก steganotext ด้วยเทคนิค ZWSP
        ///
        /// ขั้นตอนการทำงาน:
        ///   1. วนผ่าน steganotext ทีละตัวอักษร
        ///   2. ถ้าเจอ ZWSP → จำไว้ว่าช่องนี้มี ZWSP
        ///   3. ถ้าเจอตัวอักษรปกติ (ไม่ใช่ ZWSP) และก่อนหน้ามีตัวอักษรปกติแล้ว
        ///      → บันทึก bit: มี ZWSP = bit 1, ไม่มี ZWSP = bit 0
        ///   4. แปลง extracted bits กลับเป็น ciphertext string
        ///
        /// สำคัญ: ZWSP ไม่นับเป็น "ตัวอักษรปกติ" — เป็นแค่ marker ระหว่างตัวอักษร
        /// </summary>
        /// <param name="steganotext">ข้อความ steganotext ที่อาจมี ZWSP ซ่อนอยู่</param>
        /// <returns>ciphertext ที่ถูกดึงออกมา (Base64 string)</returns>
        public static string ZWSPExtract(string steganotext)
        {
            // สร้าง list สำหรับเก็บ bits ที่ดึงออกมา
            var bits = new List<bool>();
            // flag บอกว่าตัวอักษรก่อนหน้า (ที่ไม่ใช่ ZWSP) มีหรือไม่
            // ใช้เพื่อรู้ว่าเรากำลังอยู่ "ระหว่าง" ตัวอักษรสองตัว → ถึงจะนับเป็น 1 bit
            bool prevWasNormal = false;
            // flag บอกว่าช่องระหว่างตัวอักษรคู่ก่อนหน้ามี ZWSP หรือไม่
            bool prevHadZWSP = false;

            // === ขั้นตอนหลัก: วนผ่าน steganotext ทีละตัวอักษร ===
            for (int i = 0; i < steganotext.Length; i++)
            {
                // อ่านตัวอักษรที่ตำแหน่ง i
                char c = steganotext[i];
                // ตรวจว่าตัวอักษรนี้เป็น ZWSP หรือไม่
                if (c == ZWSP)
                {
                    // เจอ ZWSP → จำไว้ว่าช่องปัจจุบันมี ZWSP (จะถูกบันทึกเป็น bit 1 เมื่อเจอตัวอักษรปกติตัวถัดไป)
                    prevHadZWSP = true;
                }
                else
                {
                    // เจอตัวอักษรปกติ (ไม่ใช่ ZWSP)
                    // ถ้าก่อนหน้านี้มีตัวอักษรปกติอยู่แล้ว → แสดงว่าเราเพิ่งผ่าน "ช่อง" ระหว่างสองตัวอักษร
                    if (prevWasNormal)
                    {
                        // บันทึก 1 bit: ถ้ามี ZWSP ในช่องนี้ = true (bit 1), ไม่มี = false (bit 0)
                        bits.Add(prevHadZWSP);
                    }
                    // ตั้ง flag ว่าตัวอักษรปกติตัวนี้คือ "ตัวก่อนหน้า" สำหรับช่องถัดไป
                    prevWasNormal = true;
                    // reset flag ZWSP สำหรับช่องถัดไป
                    prevHadZWSP = false;
                }
            }

            // แปลง extracted bits กลับเป็น ciphertext string ผ่าน PayloadBitsToString
            return PayloadBitsToString(bits.ToArray());
        }

        // ============================================================
        //  NBSP (Non-Breaking Space U+00A0) — เทคนิคที่ 4
        //  หลักการ: แทนที่ space ปกติ (U+0020) ด้วย Non-Breaking Space (U+00A0)
        //  ทั้งสองตัวแสดงผลเหมือน space ปกติ แต่ Unicode ต่างกัน
        //    - space ปกติ (U+0020) = bit 0
        //    - NBSP (U+00A0) = bit 1
        //  ความจุ = จำนวน space ใน covertext
        //  ข้อดี: มองไม่เห็นความแตกต่างด้วยตาเปล่า ไม่ต้องเลือก options
        //  ข้อเสีย: ความจุต่ำ (ขึ้นกับจำนวน space ใน covertext)
        // ============================================================

        /// <summary>
        /// ฝัง ciphertext ลงใน covertext ด้วยเทคนิค Non-Breaking Space (NBSP)
        ///
        /// ขั้นตอนการทำงาน:
        ///   1. Normalize covertext (ลบ ZWSP, แปลง NBSP เป็น space ปกติ)
        ///   2. แปลง ciphertext เป็น payload bits
        ///   3. นับจำนวน space ใน covertext เพื่อตรวจความจุ
        ///   4. วนผ่าน covertext: ถ้าเจอ space → แทนที่ด้วย space ปกติ (bit=0) หรือ NBSP (bit=1)
        /// </summary>
        /// <param name="coverText">ข้อความปกปิด</param>
        /// <param name="cipherText">ข้อความเข้ารหัสในรูป Base64 string</param>
        /// <returns>steganotext ที่มี NBSP ซ่อนอยู่แทน space บางตัว</returns>
        public static string NBSPEmbed(string coverText, string cipherText)
        {
            // ทำความสะอาด covertext ก่อน: ลบ ZWSP และแปลง NBSP เป็น space ปกติ
            // เพื่อให้ทุก space เป็น U+0020 ก่อนเริ่มฝัง
            coverText = NormalizeCovertext(coverText);
            // แปลง ciphertext เป็น payload bits (32-bit header + data bits)
            bool[] bits = StringToPayloadBits(cipherText);

            // นับจำนวน space ปกติ (U+0020) ทั้งหมดใน covertext
            // เพื่อตรวจว่ามี "ช่อง" เพียงพอสำหรับฝัง bits ทั้งหมดหรือไม่
            int spaceCount = 0;
            foreach (char c in coverText)
                if (c == ' ') spaceCount++; // นับเฉพาะ space ปกติ (U+0020)

            // ตรวจสอบว่าจำนวน space เพียงพอสำหรับจำนวน bits ที่ต้องฝัง
            if (spaceCount < bits.Length)
                throw new InvalidOperationException(
                    $"Covertext มี space ไม่เพียงพอ (ต้องการ {bits.Length} ช่อง แต่มี space {spaceCount} ตัว)");

            // สร้าง StringBuilder ขนาดเท่ากับ covertext เดิม (NBSP กับ space มีขนาดเท่ากัน)
            var sb = new StringBuilder(coverText.Length);
            // ตัวนับ bit ปัจจุบันที่กำลังจะฝัง
            int bitIdx = 0;

            // === ขั้นตอนหลัก: วนผ่าน covertext ทีละตัวอักษร ===
            foreach (char c in coverText)
            {
                // ตรวจว่าตัวอักษรนี้เป็น space ปกติ AND ยังมี bit ที่ต้องฝัง
                if (c == ' ' && bitIdx < bits.Length)
                {
                    // bit เป็น true (bit 1) → ใช้ NBSP (U+00A0) แทน space
                    // bit เป็น false (bit 0) → คง space ปกติ (U+0020) ไว้
                    sb.Append(bits[bitIdx] ? NBSP : ' ');
                    // เลื่อนไปฝัง bit ถัดไป
                    bitIdx++;
                }
                else
                {
                    // ตัวอักษรไม่ใช่ space หรือฝัง bit ครบแล้ว → เพิ่มตัวอักษรตามเดิม
                    sb.Append(c);
                }
            }

            // แปลง StringBuilder เป็น string แล้วส่งคืนเป็น steganotext
            return sb.ToString();
        }

        /// <summary>
        /// ดึง ciphertext จาก steganotext ด้วยเทคนิค NBSP
        ///
        /// ขั้นตอนการทำงาน:
        ///   1. วนผ่าน steganotext ทีละตัวอักษร
        ///   2. ถ้าเจอ space ปกติ (U+0020) → bit 0
        ///   3. ถ้าเจอ NBSP (U+00A0) → bit 1
        ///   4. ตัวอักษรอื่นๆ → ข้ามไป ไม่เกี่ยว
        ///   5. แปลง extracted bits กลับเป็น ciphertext string
        /// </summary>
        /// <param name="steganotext">ข้อความ steganotext ที่อาจมี NBSP ซ่อนอยู่</param>
        /// <returns>ciphertext ที่ถูกดึงออกมา (Base64 string)</returns>
        public static string NBSPExtract(string steganotext)
        {
            // สร้าง list สำหรับเก็บ bits ที่ดึงออกมา
            var bits = new List<bool>();

            // วนผ่าน steganotext ทีละตัวอักษร
            foreach (char c in steganotext)
            {
                // ถ้าเป็น space ปกติ (U+0020) → bit 0 ถูกฝังไว้ที่ตำแหน่งนี้
                if (c == ' ')
                    bits.Add(false); // false = bit 0
                // ถ้าเป็น NBSP (U+00A0) → bit 1 ถูกฝังไว้ที่ตำแหน่งนี้
                else if (c == NBSP)
                    bits.Add(true);  // true = bit 1
                // ตัวอักษรอื่น → ข้ามไป ไม่ใช่ส่วนที่ฝังข้อมูล
            }

            // แปลง extracted bits กลับเป็น ciphertext string ผ่าน PayloadBitsToString
            return PayloadBitsToString(bits.ToArray());
        }

        // ============================================================
        //  UTILITY: Payload Bits Encoding/Decoding
        //  ส่วนนี้เป็นฟังก์ชันแปลง string ↔ bits ที่ใช้ร่วมกันทุกเทคนิค
        //
        //  รูปแบบ payload:
        //    [32-bit length header (big-endian)] [data bits]
        //    - length = จำนวน bytes ของ ciphertext (ไม่ใช่จำนวน bits)
        //    - big-endian = bit แรกคือ bit ที่มีน้ำหนักมากที่สุด (MSB)
        //    - data bits = แต่ละ byte ของ ciphertext → 8 bits, MSB first
        //
        //  เหตุผลที่ต้องมี 32-bit header:
        //    เพราะตอน extract เราไม่รู้ว่า ciphertext ยาวเท่าไหร่
        //    header จะบอกความยาวเพื่อให้อ่าน bits ได้ถูกจำนวน
        // ============================================================

        /// <summary>
        /// แปลง string (ciphertext) → payload bits
        ///
        /// ขั้นตอนการทำงาน:
        ///   1. แปลง string เป็น byte array ด้วย UTF-8 encoding
        ///   2. สร้าง bit array ขนาด 32 + (จำนวน bytes × 8)
        ///   3. เขียน 32-bit length header (big-endian): จำนวน bytes ของ data
        ///   4. เขียน data bits: แต่ละ byte → 8 bits, MSB (Most Significant Bit) ก่อน
        ///
        /// ตัวอย่าง: text = "AB" → UTF-8 = [0x41, 0x42] → 2 bytes
        ///   header (32 bits): 00000000 00000000 00000000 00000010 (= 2 ในเลขฐานสอง)
        ///   data (16 bits):   01000001 01000010 (= 'A' 'B')
        ///   รวม 48 bits
        /// </summary>
        /// <param name="text">string ที่จะแปลงเป็น payload bits (ปกติคือ ciphertext ในรูป Base64)</param>
        /// <returns>อาร์เรย์ bool[] ที่เป็น payload bits พร้อม 32-bit header</returns>
        public static bool[] StringToPayloadBits(string text)
        {
            // แปลง string เป็น byte array ด้วย UTF-8 encoding
            // UTF-8 รองรับทุกภาษา แต่ Base64 ciphertext จะใช้แค่ ASCII → 1 byte ต่อตัวอักษร
            byte[] data = Encoding.UTF8.GetBytes(text);
            // คำนวณจำนวน bits ทั้งหมด: 32 bits สำหรับ header + จำนวน bytes × 8 bits/byte
            int totalBits = 32 + data.Length * 8;
            // สร้าง bit array ขนาดที่คำนวณได้ (ค่าเริ่มต้นทุกช่องเป็น false = bit 0)
            bool[] bits = new bool[totalBits];

            // === เขียน 32-bit length header (big-endian) ===
            // len = จำนวน bytes ของ data (ใช้เป็นค่าที่จะถูก encode เข้า header)
            int len = data.Length;
            // วนลูป 32 รอบ (bit 0 ถึง bit 31) เพื่อเขียน header
            for (int i = 0; i < 32; i++)
                // ดึง bit ที่ตำแหน่ง (31-i) จาก len ด้วย bit shift + masking
                // i=0 → ดึง bit 31 (MSB), i=1 → ดึง bit 30, ..., i=31 → ดึง bit 0 (LSB)
                // ถ้า bit นั้นเป็น 1 → true, เป็น 0 → false
                bits[i] = ((len >> (31 - i)) & 1) == 1;

            // === เขียน data bits ===
            // วนลูปผ่านทุก byte ใน data array
            for (int b = 0; b < data.Length; b++)
                // วนลูป 8 รอบสำหรับแต่ละ bit ใน byte (MSB first)
                for (int bit = 0; bit < 8; bit++)
                    // คำนวณตำแหน่งใน bits array: 32 (ข้าม header) + b*8 (byte ที่ b) + bit (bit ที่เท่าไหร่ใน byte)
                    // ดึง bit ที่ตำแหน่ง (7-bit) จาก data[b]: bit=0 → ดึง bit 7 (MSB), bit=7 → ดึง bit 0 (LSB)
                    bits[32 + b * 8 + bit] = ((data[b] >> (7 - bit)) & 1) == 1;

            // ส่งคืน bit array ที่มีทั้ง header และ data
            return bits;
        }

        /// <summary>
        /// แปลง payload bits → string (ciphertext)
        ///
        /// ขั้นตอนการทำงาน:
        ///   1. ตรวจสอบว่ามี bits อย่างน้อย 32 ตัว (สำหรับ header)
        ///   2. อ่าน 32-bit length header (big-endian) เพื่อรู้ว่า data มีกี่ bytes
        ///   3. ตรวจสอบว่า length อยู่ในช่วงที่สมเหตุสมผล (> 0 และ ≤ 1,000,000)
        ///   4. ตรวจสอบว่ามี bits เพียงพอสำหรับ data ทั้งหมด
        ///   5. อ่าน data bits: ทุกๆ 8 bits → 1 byte
        ///   6. แปลง byte array กลับเป็น string ด้วย UTF-8
        ///
        /// ถ้า length ไม่สมเหตุสมผลหรือ bits ไม่พอ → throw exception
        /// เพราะอาจหมายถึงผู้ใช้เลือกเทคนิคผิด หรือ steganotext ถูกแก้ไข
        /// </summary>
        /// <param name="bits">อาร์เรย์ bits ที่ดึงมาจาก steganotext</param>
        /// <returns>ciphertext string ที่แปลงกลับได้</returns>
        public static string PayloadBitsToString(bool[] bits)
        {
            // ตรวจสอบว่ามี bits อย่างน้อย 32 ตัว เพราะ header เพียงอย่างเดียวต้องการ 32 bits
            if (bits.Length < 32)
                throw new InvalidOperationException("Steganotext มีข้อมูลไม่เพียงพอ (น้อยกว่า 32 bits)");

            // === อ่าน 32-bit length header (big-endian) ===
            // เริ่มจาก 0 แล้ว shift ซ้ายทีละ 1 bit พร้อมกับ OR เข้ากับ bit ปัจจุบัน
            int len = 0;
            for (int i = 0; i < 32; i++)
                // len << 1 = shift ซ้าย 1 bit (คูณ 2)
                // | (bits[i] ? 1 : 0) = ใส่ bit ปัจจุบันที่ตำแหน่ง LSB
                // ผลลัพธ์: สร้างเลขจำนวนเต็ม 32-bit จาก bits ที่อ่านมา
                len = (len << 1) | (bits[i] ? 1 : 0);

            // ตรวจสอบว่า length อยู่ในช่วงที่สมเหตุสมผล
            // len ≤ 0 = ไม่มีข้อมูล (อาจเลือกเทคนิคผิดทำให้ header อ่านผิด)
            // len > 1,000,000 = ข้อมูลใหญ่เกินไป (อาจเป็น garbage data)
            if (len <= 0 || len > 1_000_000)
                throw new InvalidOperationException("ไม่พบข้อมูลที่ซ่อนอยู่ หรือเลือกเทคนิคผิด");

            // คำนวณจำนวน bits ที่ต้องการทั้งหมด: 32 (header) + len × 8 (data)
            int requiredBits = 32 + len * 8;
            // ตรวจสอบว่ามี bits เพียงพอสำหรับ data ทั้งหมดตามที่ header ระบุ
            if (bits.Length < requiredBits)
                throw new InvalidOperationException(
                    $"Steganotext มีข้อมูลไม่ครบ (ต้องการ {requiredBits} bits แต่ได้ {bits.Length})");

            // สร้าง byte array ขนาดตามที่ header ระบุ เพื่อเก็บ data ที่จะอ่าน
            byte[] data = new byte[len];
            // === อ่าน data bits: แปลง bits → bytes ===
            // วนลูปผ่านทุก byte ที่ต้องอ่าน
            for (int b = 0; b < len; b++)
            {
                // เริ่มต้นค่า byte เป็น 0 (00000000 ในเลขฐานสอง)
                byte val = 0;
                // วนลูป 8 รอบสำหรับแต่ละ bit ใน byte (MSB first)
                for (int bit = 0; bit < 8; bit++)
                    // val << 1 = shift ซ้าย 1 bit (คูณ 2)
                    // | (bits[...] ? 1 : 0) = ใส่ bit ปัจจุบันที่ตำแหน่ง LSB
                    // ตำแหน่ง bit ใน array: 32 (ข้าม header) + b*8 (byte ที่ b) + bit (bit ที่เท่าไหร่)
                    val = (byte)((val << 1) | (bits[32 + b * 8 + bit] ? 1 : 0));
                // เก็บ byte ที่ประกอบเสร็จแล้วลงใน data array
                data[b] = val;
            }

            // แปลง byte array กลับเป็น string ด้วย UTF-8 encoding แล้วส่งคืนเป็น ciphertext
            return Encoding.UTF8.GetString(data);
        }

        // ============================================================
        //  PRIVATE HELPERS — ฟังก์ชันช่วยภายในที่ใช้เฉพาะในคลาสนี้
        // ============================================================

        /// <summary>
        /// ค้นหาตำแหน่งของคำ Correct/Wrong ทั้งหมดใน text ตามลำดับที่ปรากฏ (left-to-right)
        ///
        /// อัลกอริทึม (Greedy Left-to-Right, Non-Overlapping):
        ///   1. เริ่มค้นจากตำแหน่ง 0 ของ text
        ///   2. วนผ่านทุกคู่คำที่ผู้ใช้เลือก ค้นหาทั้งคำถูกและคำผิด
        ///   3. เลือกคำที่พบเร็วที่สุด (ตำแหน่งน้อยที่สุด) — ถ้าตำแหน่งเท่ากัน เลือกคำที่ยาวกว่า
        ///   4. บันทึกผลลัพธ์ แล้วเลื่อน searchFrom ไปหลังคำที่พบ (non-overlapping)
        ///   5. ทำซ้ำจนค้นหมดทั้ง text
        ///
        /// เหตุผลที่ใช้ greedy: เพื่อให้ได้ผลลัพธ์เหมือนกันทั้ง embed และ extract
        /// เหตุผลที่เลือกคำยาวกว่าเมื่อตำแหน่งเท่ากัน: เพื่อป้องกัน substring match ซ้ำซ้อน
        /// </summary>
        /// <param name="text">ข้อความที่จะค้นหา (อาจเป็น covertext ตอน embed หรือ steganotext ตอน extract)</param>
        /// <param name="activePairIndices">index ของคู่คำที่ผู้ใช้เลือก</param>
        /// <returns>รายการ tuple: (ตำแหน่งในtext, index คู่คำ, เป็นคำถูกหรือไม่, คำที่เจอ)</returns>
        private static List<(int pos, int pairIdx, bool isCorrect, string word)>
            FindMisspellingCandidates(string text, IList<int> activePairIndices)
        {
            // สร้าง list สำหรับเก็บผลลัพธ์ทั้งหมดที่พบ
            var results = new List<(int pos, int pairIdx, bool isCorrect, string word)>();
            // ตำแหน่งเริ่มต้นในการค้นหา — เริ่มจาก 0 (ต้น text)
            int searchFrom = 0;

            // === วนลูปค้นหาจนกว่าจะหมด text ===
            while (searchFrom < text.Length)
            {
                // ตัวแปรเก็บ "ผลลัพธ์ที่ดีที่สุด" ในรอบนี้ (ตำแหน่งเร็วที่สุด, คำยาวที่สุด)
                int bestPos = -1;        // ตำแหน่งที่ดีที่สุด (-1 = ยังไม่พบ)
                int bestPairIdx = -1;    // index ของคู่คำที่ดีที่สุด
                bool bestIsCorrect = false; // คำที่ดีที่สุดเป็นคำถูกหรือผิด
                string bestWord = null;  // คำที่ดีที่สุดที่เจอ

                // วนผ่านทุกคู่คำที่ผู้ใช้เลือก เพื่อค้นหาทั้งคำถูกและคำผิดใน text
                foreach (int idx in activePairIndices)
                {
                    // ดึงคู่คำจากอาร์เรย์ตาม index
                    var pair = MisspellingPairs[idx];

                    // ค้นหาคำถูก (Correct) ใน text ตั้งแต่ตำแหน่ง searchFrom
                    // ใช้ StringComparison.Ordinal เพื่อเปรียบเทียบ byte-by-byte (เร็วและแม่นยำ)
                    int pc = text.IndexOf(pair.Correct, searchFrom, StringComparison.Ordinal);
                    // ตรวจว่าพบหรือไม่ (pc >= 0) และตำแหน่งดีกว่าที่เคยพบ:
                    // - bestPos < 0 = ยังไม่เคยพบเลย → ใช้เลย
                    // - pc < bestPos = พบเร็วกว่า (ตำแหน่งน้อยกว่า)
                    // - pc == bestPos && ยาวกว่า bestWord = ตำแหน่งเท่ากัน เลือกคำยาวกว่า (ป้องกัน substring issue)
                    if (pc >= 0 && (bestPos < 0 || pc < bestPos || (pc == bestPos && pair.Correct.Length > bestWord.Length)))
                    {
                        // อัปเดตผลลัพธ์ที่ดีที่สุด
                        bestPos = pc; bestPairIdx = idx; bestIsCorrect = true; bestWord = pair.Correct;
                    }

                    // ค้นหาคำผิด (Wrong) ใน text ตั้งแต่ตำแหน่ง searchFrom
                    int pw = text.IndexOf(pair.Wrong, searchFrom, StringComparison.Ordinal);
                    // ตรวจเงื่อนไขเดียวกับคำถูก
                    if (pw >= 0 && (bestPos < 0 || pw < bestPos || (pw == bestPos && pair.Wrong.Length > bestWord.Length)))
                    {
                        // อัปเดตผลลัพธ์ที่ดีที่สุด
                        bestPos = pw; bestPairIdx = idx; bestIsCorrect = false; bestWord = pair.Wrong;
                    }
                }

                // ถ้าไม่พบคำใดเลยในรอบนี้ → ค้นหมดแล้ว ออกจากลูป
                if (bestPos < 0) break;

                // บันทึกผลลัพธ์ที่ดีที่สุด (ตำแหน่ง, index คู่คำ, เป็นคำถูกหรือไม่, คำที่เจอ)
                results.Add((bestPos, bestPairIdx, bestIsCorrect, bestWord));
                // เลื่อน searchFrom ไปหลังคำที่เจอ (non-overlapping: ไม่ให้ค้นทับกับคำที่เจอแล้ว)
                searchFrom = bestPos + bestWord.Length;
            }

            // ส่งคืนรายการตำแหน่งทั้งหมดที่พบ (เรียงตามลำดับจากซ้ายไปขวา)
            return results;
        }

        // ============================================================
        //  SYNONYM — เทคนิคที่ 5
        //  หลักการ: ใช้คู่คำพ้องความหมาย (synonym) ภาษาไทย
        //  คำ index 0 ในกลุ่ม = bit 0, คำ index 1 ในกลุ่ม = bit 1
        //  ตัวอย่าง: กลุ่ม ["กล่าว", "พูด"] → ถ้าเจอ "กล่าว" = bit 0, "พูด" = bit 1
        //  ข้อดี: ข้อความดูเป็นธรรมชาติเพราะความหมายเดิมไม่เปลี่ยน
        //  ข้อเสีย: ความจุขึ้นกับจำนวนคำพ้องที่ปรากฏใน covertext
        // ============================================================

        /// <summary>
        /// อาร์เรย์ 64 กลุ่มคำพ้องภาษาไทย (Synonym Groups)
        /// แต่ละกลุ่มมี 2 คำที่ความหมายใกล้เคียงกัน
        ///
        /// หลักการเข้ารหัส:
        ///   - แต่ละกลุ่มที่ผู้ใช้เลือกจะให้ 1 bit ต่อ 1 ครั้งที่พบใน covertext
        ///   - คำที่ index 0 ในกลุ่ม = bit 0 (ใช้คำแรก)
        ///   - คำที่ index 1 ในกลุ่ม = bit 1 (ใช้คำที่สอง)
        ///   - อาจขยายเป็น n-ary encoding ในอนาคต (เช่น 4 คำต่อกลุ่ม → 2 bits) แต่ตอนนี้ใช้ binary
        ///
        /// ข้อสำคัญ:
        ///   - ลำดับ index ต้องตรงกับ checkedListBox1.Items ใน Synonym.Designer.cs
        ///   - คำในแต่ละกลุ่มต้องไม่ซ้ำกับกลุ่มอื่น เพื่อป้องกันการ match ผิดกลุ่ม
        ///   - ไม่ควรใช้คำสั้นเกินไปที่อาจเป็น substring ของคำอื่น
        /// </summary>
        // ประกาศเป็น public static readonly เพราะ sub-form Synonym.cs ต้องเข้าถึงข้อมูลนี้ได้
        // ใช้ string[][] (jagged array) เพราะแต่ละกลุ่มอาจมีจำนวนคำต่างกันในอนาคต
        public static readonly string[][] SynonymGroups = new[]
        {
            // index 0
            new[] { "กล่าว",        "พูด" },
            // index 1
            new[] { "ได้รับ",       "ได้มา" },
            // index 2
            new[] { "ทำ",           "กระทำ" },
            // index 3
            new[] { "ใช้",          "นำมาใช้" },
            // index 4
            new[] { "มอง",          "เฝ้ามอง" },
            // index 5
            new[] { "เดิน",         "ย่าง" },
            // index 6
            new[] { "คิด",          "ไตร่ตรอง" },
            // index 7
            new[] { "หยุด",         "หยุดชะงัก" },
            // index 8
            new[] { "ต้องการ",      "ประสงค์" },
            // index 9
            new[] { "เริ่ม",        "เริ่มต้น" },
            // index 10
            new[] { "เสร็จ",        "เสร็จสิ้น" },
            // index 11
            new[] { "ช่วย",         "ให้ความช่วยเหลือ" },
            // index 12
            new[] { "บอก",          "แจ้ง" },
            // index 13
            new[] { "ถาม",          "สอบถาม" },
            // index 14
            new[] { "ตอบ",          "ตอบรับ" },
            // index 15
            new[] { "รู้จัก",        "ทราบ" },
            // index 16
            new[] { "เข้าใจ",       "ตระหนัก" },
            // index 17
            new[] { "ส่ง",          "จัดส่ง" },
            // index 18
            new[] { "รับทราบ",      "รับมอบ" },
            // index 19
            new[] { "สร้าง",        "ก่อสร้าง" },
            // index 20
            new[] { "แก้ไข",        "ปรับปรุง" },
            // index 21
            new[] { "ลบออก",        "ขจัด" },
            // index 22
            new[] { "เพิ่ม",        "เพิ่มเติม" },
            // index 23
            new[] { "ลดทอน",        "ลดลง" },
            // index 24
            new[] { "เปลี่ยน",      "เปลี่ยนแปลง" },
            // index 25
            new[] { "ยืนยัน",       "รับรอง" },
            // index 26
            new[] { "ปฏิเสธ",       "ไม่ยอมรับ" },
            // index 27
            new[] { "อนุมัติ",      "เห็นชอบ" },
            // index 28
            new[] { "ตรวจสอบ",      "พิจารณา" },
            // index 29
            new[] { "รายงาน",       "แจ้งรายงาน" },
            // index 30
            new[] { "ประชุม",       "ประชุมหารือ" },
            // index 31
            new[] { "ตัดสิน",       "วินิจฉัย" },
            // index 32
            new[] { "เลือก",        "คัดเลือก" },
            // index 33
            new[] { "จัดเตรียม",    "จัดการ" },
            // index 34
            new[] { "ควบคุม",       "กำกับดูแล" },
            // index 35
            new[] { "พัฒนา",        "ปรับพัฒนา" },
            // index 36
            new[] { "วางแผน",       "กำหนดแผน" },
            // index 37
            new[] { "ดำเนิน",       "ดำเนินการ" },
            // index 38
            new[] { "ปรึกษา",       "หารือ" },
            // index 39
            new[] { "สรุป",         "สรุปผล" },
            // index 40
            new[] { "นำเสนอ",       "นำออกเสนอ" },
            // index 41
            new[] { "อธิบาย",       "ชี้แจง" },
            // index 42
            new[] { "แสดง",         "แสดงให้เห็น" },
            // index 43
            new[] { "ระบุ",         "ชี้บ่ง" },
            // index 44
            new[] { "กำหนด",        "ตั้งกำหนด" },
            // index 45
            new[] { "สนับสนุน",     "ให้การสนับสนุน" },
            // index 46
            new[] { "คัดค้าน",      "โต้แย้ง" },
            // index 47
            new[] { "เสนอ",         "ยื่นข้อเสนอ" },
            // index 48
            new[] { "ขออนุญาต",     "ร้องขอ" },
            // index 49
            new[] { "ขอบคุณ",       "ขอบพระคุณ" },
            // index 50
            new[] { "ขอโทษ",        "ขอประทานโทษ" },
            // index 51
            new[] { "ยินดี",        "มีความยินดี" },
            // index 52
            new[] { "เห็นด้วย",     "เห็นพ้อง" },
            // index 53
            new[] { "ไม่เห็นด้วย",  "ขัดแย้ง" },
            // index 54
            new[] { "รอคอย",        "คอยท่า" },
            // index 55
            new[] { "รีบ",          "รีบด่วน" },
            // index 56
            new[] { "สำเร็จ",       "บรรลุผล" },
            // index 57
            new[] { "ล้มเหลว",      "ไม่สำเร็จ" },
            // index 58
            new[] { "ยาก",          "ลำบาก" },
            // index 59
            new[] { "ง่าย",         "สะดวก" },
            // index 60
            new[] { "ดีมาก",        "เป็นประโยชน์" },
            // index 61
            new[] { "เลว",          "ไม่ดี" },
            // index 62
            new[] { "สำคัญ",        "มีความสำคัญ" },
            // index 63
            new[] { "จำเป็น",       "มีความจำเป็น" },
        };

        /// <summary>
        /// ฝัง ciphertext ลงใน covertext ด้วยเทคนิค Synonym
        ///
        /// ขั้นตอนการทำงาน:
        ///   1. แปลง ciphertext เป็น payload bits
        ///   2. ค้นหาตำแหน่งทุกคำพ้องใน covertext ที่อยู่ในกลุ่มที่ผู้ใช้เลือก
        ///   3. วนผ่านตำแหน่งที่พบ แทนที่ด้วย variant 0 (bit=0) หรือ variant 1 (bit=1)
        ///   4. ใช้ offset tracking เพราะคำพ้องอาจมีความยาวต่างกัน
        ///
        /// เทคนิคนี้คล้ายกับ Misspelling แต่ใช้คำพ้องแทนคำถูก/ผิด
        /// ข้อดีคือข้อความดูเป็นธรรมชาติมากกว่าเพราะความหมายไม่เปลี่ยน
        /// </summary>
        /// <param name="coverText">ข้อความปกปิดที่จะใช้ฝัง (ต้องมีคำจากกลุ่มที่เลือกเพียงพอ)</param>
        /// <param name="cipherText">ข้อความเข้ารหัสในรูป Base64 string</param>
        /// <param name="activeGroupIndices">index ของกลุ่มคำพ้องที่ผู้ใช้เลือก</param>
        /// <returns>steganotext — ข้อความที่มี ciphertext ซ่อนอยู่แล้ว</returns>
        public static string SynonymEmbed(string coverText, string cipherText, IList<int> activeGroupIndices)
        {
            // ตรวจสอบว่าผู้ใช้เลือกกลุ่มคำพ้องอย่างน้อย 1 กลุ่ม
            if (activeGroupIndices == null || activeGroupIndices.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกกลุ่มคำพ้องอย่างน้อย 1 กลุ่ม");

            // แปลง ciphertext เป็น payload bits (32-bit header + data bits)
            bool[] bits = StringToPayloadBits(cipherText);
            // ค้นหาตำแหน่งของทุกคำพ้องใน covertext ที่ตรงกับกลุ่มที่ผู้ใช้เลือก
            // ฟังก์ชัน FindSynonymCandidates ใช้อัลกอริทึม greedy left-to-right เหมือน FindMisspellingCandidates
            var candidates = FindSynonymCandidates(coverText, activeGroupIndices);

            // ตรวจสอบว่าจำนวนคำพ้องที่พบเพียงพอสำหรับจำนวน bits ที่ต้องฝัง
            // ตรวจที่นี่ก่อนเข้าลูปเพื่อแจ้ง error ได้เร็ว (fail fast)
            if (candidates.Count < bits.Length)
                throw new InvalidOperationException(
                    $"Covertext มีคำพ้องไม่เพียงพอ (ต้องการ {bits.Length} คำ แต่พบ {candidates.Count})");

            // สร้าง StringBuilder จาก covertext เพื่อให้สามารถ Remove/Insert คำได้
            var sb = new StringBuilder(coverText);
            // offset ใช้ติดตามการเลื่อนตำแหน่ง เหมือนกับในเทคนิค Misspelling
            int offset = 0;
            // ตัวนับ bit ปัจจุบันที่กำลังจะฝัง
            int bitIdx = 0;

            // === ขั้นตอนหลัก: วนผ่านทุกตำแหน่งที่พบคำพ้อง แล้วแทนที่ตาม bit ===
            // ใช้ tuple deconstruction: pos = ตำแหน่ง, groupIdx = index กลุ่มคำพ้อง,
            // wordVariantIdx = index ของ variant ที่เจอ (0 หรือ 1), word = คำที่เจอจริงๆ
            foreach (var (pos, groupIdx, wordVariantIdx, word) in candidates)
            {
                // ถ้าฝังครบทุก bit แล้ว ไม่ต้องแทนที่คำอีก
                if (bitIdx >= bits.Length) break;

                // อ่าน bit ปัจจุบัน: true = ต้องการ bit 1, false = ต้องการ bit 0
                bool wantBit1 = bits[bitIdx];
                // เลือกคำเป้าหมายจากกลุ่ม: bit 0 → ใช้ variant index 0 (คำแรก), bit 1 → ใช้ variant index 1 (คำที่สอง)
                // ใช้ ternary operator: wantBit1 ? 1 : 0 เพื่อเลือก index ของ variant
                string target = SynonymGroups[groupIdx][wantBit1 ? 1 : 0];

                // คำนวณตำแหน่งจริงใน StringBuilder โดยบวก offset ที่สะสมจากการแทนที่ครั้งก่อนๆ
                int adjustedPos = pos + offset;
                // ลบคำเดิมออกจาก StringBuilder
                sb.Remove(adjustedPos, word.Length);
                // แทรกคำเป้าหมาย (variant ตาม bit) เข้าที่ตำแหน่งเดิม
                sb.Insert(adjustedPos, target);
                // อัปเดต offset: ถ้าคำเป้าหมายยาว/สั้นกว่าคำเดิม ตำแหน่งคำถัดไปจะเลื่อน
                offset += target.Length - word.Length;
                // เลื่อนไปฝัง bit ถัดไป
                bitIdx++;
            }

            // ตรวจสอบอีกครั้งว่าฝังครบทุก bit (เป็น safety check เพราะตรวจข้างบนแล้วแต่ loop อาจ break ก่อน)
            if (bitIdx < bits.Length)
                throw new InvalidOperationException(
                    $"Covertext มีคำพ้องไม่เพียงพอ (ต้องการ {bits.Length} แต่ได้ {bitIdx})");

            // แปลง StringBuilder กลับเป็น string แล้วส่งคืนเป็น steganotext
            return sb.ToString();
        }

        /// <summary>
        /// ดึง ciphertext จาก steganotext ด้วยเทคนิค Synonym
        ///
        /// ขั้นตอนการทำงาน:
        ///   1. ค้นหาตำแหน่งทุกคำพ้องใน steganotext
        ///   2. คำ variant 0 = bit 0, คำ variant 1 = bit 1
        ///   3. แปลง extracted bits กลับเป็น ciphertext string
        /// </summary>
        /// <param name="steganotext">ข้อความ steganotext ที่มีข้อมูลลับซ่อนอยู่</param>
        /// <param name="activeGroupIndices">index ของกลุ่มคำพ้องที่ใช้ตอน embed (ต้องเลือกเหมือนกัน)</param>
        /// <returns>ciphertext ที่ถูกดึงออกมา (Base64 string)</returns>
        public static string SynonymExtract(string steganotext, IList<int> activeGroupIndices)
        {
            // ตรวจสอบว่าผู้ใช้เลือกกลุ่มคำพ้องอย่างน้อย 1 กลุ่ม (ต้องตรงกับที่ใช้ตอน embed)
            if (activeGroupIndices == null || activeGroupIndices.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกกลุ่มคำพ้องอย่างน้อย 1 กลุ่ม");

            // ค้นหาตำแหน่งของทุกคำพ้องใน steganotext ที่ตรงกับกลุ่มที่ผู้ใช้เลือก
            var candidates = FindSynonymCandidates(steganotext, activeGroupIndices);
            // สร้าง list สำหรับเก็บ bits ที่ดึงออกมา
            var extractedBits = new List<bool>();

            // วนผ่านทุกตำแหน่งที่พบคำพ้อง แล้วอ่าน bit
            // ใช้ _ (discard) สำหรับ pos, groupIdx, word เพราะไม่ต้องใช้ในการ extract
            foreach (var (_, _, wordVariantIdx, _) in candidates)
                // wordVariantIdx = 0 → bit 0 (false), wordVariantIdx = 1 → bit 1 (true)
                extractedBits.Add(wordVariantIdx == 1);

            // แปลง extracted bits กลับเป็น ciphertext string ผ่าน PayloadBitsToString
            return PayloadBitsToString(extractedBits.ToArray());
        }

        /// <summary>
        /// ค้นหาตำแหน่งของคำพ้องทั้งหมดใน text ตามลำดับที่ปรากฏ (left-to-right)
        ///
        /// อัลกอริทึม (Greedy Left-to-Right, Non-Overlapping):
        ///   เหมือนกับ FindMisspellingCandidates แต่ค้นจาก SynonymGroups แทน MisspellingPairs
        ///   1. เริ่มค้นจากตำแหน่ง 0 ของ text
        ///   2. วนผ่านทุกกลุ่มที่เลือก ค้นหาทุก variant ในกลุ่ม
        ///   3. เลือก match ที่ตำแหน่งเร็วที่สุด (ถ้าเท่ากัน เลือกคำยาวกว่า)
        ///   4. บันทึกผลลัพธ์ แล้วเลื่อนไปหลังคำที่พบ
        ///   5. ทำซ้ำจนค้นหมดทั้ง text
        /// </summary>
        /// <param name="text">ข้อความที่จะค้นหา</param>
        /// <param name="activeGroupIndices">index ของกลุ่มคำพ้องที่ผู้ใช้เลือก</param>
        /// <returns>รายการ tuple: (ตำแหน่งใน text, index กลุ่ม, index variant ในกลุ่ม, คำที่เจอ)</returns>
        private static List<(int pos, int groupIdx, int variantIdx, string word)>
            FindSynonymCandidates(string text, IList<int> activeGroupIndices)
        {
            // สร้าง list สำหรับเก็บผลลัพธ์ทั้งหมดที่พบ
            var results = new List<(int pos, int groupIdx, int variantIdx, string word)>();
            // ตำแหน่งเริ่มต้นในการค้นหา — เริ่มจาก 0 (ต้น text)
            int searchFrom = 0;

            // === วนลูปค้นหาจนกว่าจะหมด text ===
            while (searchFrom < text.Length)
            {
                // ตัวแปรเก็บ "ผลลัพธ์ที่ดีที่สุด" ในรอบนี้
                int bestPos = -1;          // ตำแหน่งที่ดีที่สุด (-1 = ยังไม่พบ)
                int bestGroupIdx = -1;     // index ของกลุ่มที่ดีที่สุด
                int bestVariantIdx = -1;   // index ของ variant ที่ดีที่สุด (0 หรือ 1)
                string bestWord = null;    // คำที่ดีที่สุดที่เจอ

                // วนผ่านทุกกลุ่มคำพ้องที่ผู้ใช้เลือก
                foreach (int gi in activeGroupIndices)
                {
                    // ดึงกลุ่มคำพ้องจากอาร์เรย์ตาม index
                    var group = SynonymGroups[gi];
                    // วนผ่านทุก variant ในกลุ่ม (ปกติ 2 ตัว: index 0 และ 1)
                    for (int vi = 0; vi < group.Length; vi++)
                    {
                        // ค้นหา variant นี้ใน text ตั้งแต่ตำแหน่ง searchFrom
                        // ใช้ StringComparison.Ordinal เพื่อเปรียบเทียบ byte-by-byte (เร็วและแม่นยำ)
                        int p = text.IndexOf(group[vi], searchFrom, StringComparison.Ordinal);
                        // ตรวจว่าพบ (p >= 0) และตำแหน่งดีกว่าที่เคยพบ:
                        // - bestPos < 0 = ยังไม่เคยพบเลย
                        // - p < bestPos = พบเร็วกว่า (ตำแหน่งน้อยกว่า)
                        // - p == bestPos && ยาวกว่า = ตำแหน่งเท่ากัน เลือกคำยาวกว่า (ป้องกัน substring issue)
                        if (p >= 0 && (bestPos < 0 || p < bestPos || (p == bestPos && group[vi].Length > bestWord.Length)))
                        {
                            // อัปเดตผลลัพธ์ที่ดีที่สุด
                            bestPos = p;
                            bestGroupIdx = gi;
                            bestVariantIdx = vi;
                            bestWord = group[vi];
                        }
                    }
                }

                // ถ้าไม่พบคำใดเลยในรอบนี้ → ค้นหมดแล้ว ออกจากลูป
                if (bestPos < 0) break;

                // บันทึกผลลัพธ์ที่ดีที่สุด (ตำแหน่ง, index กลุ่ม, index variant, คำที่เจอ)
                results.Add((bestPos, bestGroupIdx, bestVariantIdx, bestWord));
                // เลื่อน searchFrom ไปหลังคำที่เจอ (non-overlapping)
                searchFrom = bestPos + bestWord.Length;
            }

            // ส่งคืนรายการตำแหน่งทั้งหมดที่พบ (เรียงตามลำดับจากซ้ายไปขวา)
            return results;
        }
    }
}
