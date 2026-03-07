using System;
using System.Collections.Generic;
using System.Text;

namespace Steganography
{
    /// <summary>
    /// เครื่องมือหลักสำหรับฝังและดึงข้อความลับในข้อความปกปิด
    /// รองรับ 4 เทคนิค: Homoglyph, Misspelling, ZWSP, Synonym
    ///
    /// หลักการทำงาน (Embed):
    ///   ciphertext (Base64 string) → bytes → bits → ฝังในข้อความ covertext
    ///
    /// หลักการทำงาน (Extract):
    ///   steganotext → อ่าน bits → bytes → Base64 ciphertext string
    ///
    /// รูปแบบ payload ที่ฝัง:
    ///   [32-bit length header][ข้อมูล bits ของ ciphertext bytes]
    ///   - length = จำนวน bytes ของ ciphertext (ไม่ใช่จำนวน bits)
    ///   - เก็บเป็น big-endian 32 บิต = 32 ตำแหน่งแรก
    /// </summary>
    public static class SteganographyEngine
    {
        // ============================================================
        //  HOMOGLYPH
        // ============================================================

        /// <summary>
        /// คู่ตัวอักษร Homoglyph: (ตัวจริง, ตัวแทน)
        /// bit 0 = ใช้ตัวจริง (index 0), bit 1 = ใช้ตัวแทน (index 1)
        /// </summary>
        public static readonly (char Original, char Glyph)[] HomoglyphPairs = new[]
        {
            ('ฎ', 'ฏ'),   // pair 0  — ChkbxDochada
            ('ข', 'ฃ'),   // pair 1  — ChkbxKhoKhai
            ('ช', 'ซ'),   // pair 2  — ChkbxChoChang
        };

        /// <summary>
        /// ฝัง ciphertext ลงใน covertext ด้วยเทคนิค Homoglyph
        /// </summary>
        /// <param name="coverText">ข้อความปกปิด (ต้องมีตัวอักษรใน HomoglyphPairs เพียงพอ)</param>
        /// <param name="cipherText">Base64 ciphertext ที่จะฝัง</param>
        /// <param name="activePairs">index ของคู่ที่ผู้ใช้เลือก (เช่น {0,1,3})</param>
        /// <returns>steganotext</returns>
        public static string HomoglyphEmbed(string coverText, string cipherText, IList<int> activePairs)
        {
            if (activePairs == null || activePairs.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกคู่ตัวอักษร Homoglyph อย่างน้อย 1 คู่");

            // สร้าง lookup: ตัวอักษรใดใน covertext ที่เราสามารถแทนได้
            var originalToGlyph = new Dictionary<char, char>();
            var glyphToOriginal = new Dictionary<char, char>();
            foreach (int idx in activePairs)
            {
                var p = HomoglyphPairs[idx];
                originalToGlyph[p.Original] = p.Glyph;
                glyphToOriginal[p.Glyph] = p.Original; // คืนค่าเดิมเมื่อ extract
            }

            // แปลง ciphertext → payload bits (32-bit header + data bits)
            bool[] bits = StringToPayloadBits(cipherText);

            var result = new StringBuilder(coverText);
            int bitIdx = 0;

            for (int i = 0; i < result.Length && bitIdx < bits.Length; i++)
            {
                char c = result[i];
                // normalize: ถ้าเป็น glyph ให้แปลงกลับเป็น original ก่อนตัดสินใจ
                char original = glyphToOriginal.ContainsKey(c) ? glyphToOriginal[c] : c;

                if (originalToGlyph.ContainsKey(original))
                {
                    // bit 0 → ใช้ original, bit 1 → ใช้ glyph
                    result[i] = bits[bitIdx] ? originalToGlyph[original] : original;
                    bitIdx++;
                }
            }

            if (bitIdx < bits.Length)
                throw new InvalidOperationException(
                    $"Covertext มีตัวอักษร Homoglyph ไม่เพียงพอ (ต้องการ {bits.Length} ตำแหน่ง แต่ได้ {bitIdx})");

            return result.ToString();
        }

        /// <summary>
        /// ดึง ciphertext จาก steganotext ด้วยเทคนิค Homoglyph
        /// </summary>
        public static string HomoglyphExtract(string steganotextInput, IList<int> activePairs)
        {
            if (activePairs == null || activePairs.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกคู่ตัวอักษร Homoglyph อย่างน้อย 1 คู่");

            var originalSet = new HashSet<char>();
            var glyphSet = new HashSet<char>();
            var glyphToOriginal = new Dictionary<char, char>();

            foreach (int idx in activePairs)
            {
                var p = HomoglyphPairs[idx];
                originalSet.Add(p.Original);
                glyphSet.Add(p.Glyph);
                glyphToOriginal[p.Glyph] = p.Original;
            }

            var extractedBits = new List<bool>();
            foreach (char c in steganotextInput)
            {
                if (originalSet.Contains(c))
                    extractedBits.Add(false); // bit 0
                else if (glyphSet.Contains(c))
                    extractedBits.Add(true);  // bit 1
            }

            return PayloadBitsToString(extractedBits.ToArray());
        }

        // ============================================================
        //  MISSPELLING
        // ============================================================

        /// <summary>
        /// รายการคู่คำ (คำถูก, คำผิด) ที่ใช้ในการฝัง
        /// bit 0 = คำถูก, bit 1 = คำผิด
        /// ดึงมาจาก Misspelling.Designer.cs (ลำดับตรงกัน)
        /// </summary>
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
        /// แต่ละคำ Correct/Wrong ใน covertext = 1 bit
        /// </summary>
        /// <param name="activePairIndices">index ของคู่คำที่ผู้ใช้เลือก (ต้องมีอยู่ใน covertext)</param>
        public static string MisspellingEmbed(string coverText, string cipherText, IList<int> activePairIndices)
        {
            if (activePairIndices == null || activePairIndices.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกคู่คำ Misspelling อย่างน้อย 1 คู่");

            bool[] bits = StringToPayloadBits(cipherText);

            var sb = new StringBuilder(coverText);
            var candidates = FindMisspellingCandidates(coverText, activePairIndices);

            int offset = 0;
            int bitIdx = 0;

            foreach (var (pos, pairIdx, isCorrect, word) in candidates)
            {
                if (bitIdx >= bits.Length) break;

                bool wantBit1 = bits[bitIdx]; // true = ต้องการคำผิด, false = คำถูก
                string target = wantBit1
                    ? MisspellingPairs[pairIdx].Wrong
                    : MisspellingPairs[pairIdx].Correct;

                int adjustedPos = pos + offset;
                sb.Remove(adjustedPos, word.Length);
                sb.Insert(adjustedPos, target);
                offset += target.Length - word.Length;
                bitIdx++;
            }

            if (bitIdx < bits.Length)
                throw new InvalidOperationException(
                    $"Covertext มีคำ Misspelling ไม่เพียงพอ (ต้องการ {bits.Length} คำ แต่ได้ {bitIdx})");

            return sb.ToString();
        }

        /// <summary>
        /// ดึง ciphertext จาก steganotext ด้วยเทคนิค Misspelling
        /// </summary>
        public static string MisspellingExtract(string steganotext, IList<int> activePairIndices)
        {
            if (activePairIndices == null || activePairIndices.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกคู่คำ Misspelling อย่างน้อย 1 คู่");

            var candidates = FindMisspellingCandidates(steganotext, activePairIndices);
            var extractedBits = new List<bool>();

            foreach (var (_, pairIdx, isCorrect, _) in candidates)
            {
                // isCorrect = true → คำถูก = bit 0, isCorrect = false → คำผิด = bit 1
                extractedBits.Add(!isCorrect);
            }

            return PayloadBitsToString(extractedBits.ToArray());
        }

        // ============================================================
        //  ZWSP (Zero-Width Space  U+200B)
        // ============================================================

        private const char ZWSP = '\u200B';

        /// <summary>
        /// ฝัง ciphertext ลงใน covertext ด้วยเทคนิค Zero-Width Space
        /// แทรก ZWSP ระหว่างตัวอักษรทุกคู่ใน covertext:
        ///   bit 1 → แทรก ZWSP, bit 0 → ไม่แทรก
        /// </summary>
        public static string ZWSPEmbed(string coverText, string cipherText)
        {
            bool[] bits = StringToPayloadBits(cipherText);

            // จำนวน "ช่องว่าง" ระหว่างตัวอักษร = coverText.Length - 1
            int slots = coverText.Length - 1;
            if (slots < bits.Length)
                throw new InvalidOperationException(
                    $"Covertext สั้นเกินไป (ต้องการ {bits.Length + 1} ตัวอักษร แต่ได้ {coverText.Length})");

            var sb = new StringBuilder();
            for (int i = 0; i < coverText.Length; i++)
            {
                sb.Append(coverText[i]);
                if (i < bits.Length)
                {
                    if (bits[i]) sb.Append(ZWSP);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// ดึง ciphertext จาก steganotext ด้วยเทคนิค ZWSP
        /// </summary>
        public static string ZWSPExtract(string steganotext)
        {
            var bits = new List<bool>();
            // อ่าน bit จากช่องว่างระหว่างตัวอักษร (ไม่นับ ZWSP เป็นตัวอักษร)
            bool prevWasNormal = false;
            bool prevHadZWSP = false;

            for (int i = 0; i < steganotext.Length; i++)
            {
                char c = steganotext[i];
                if (c == ZWSP)
                {
                    prevHadZWSP = true;
                }
                else
                {
                    if (prevWasNormal)
                    {
                        // ระหว่าง normal char สองตัว → บันทึก bit
                        bits.Add(prevHadZWSP);
                    }
                    prevWasNormal = true;
                    prevHadZWSP = false;
                }
            }

            return PayloadBitsToString(bits.ToArray());
        }

        // ============================================================
        //  UTILITY: Payload Bits Encoding/Decoding
        // ============================================================

        /// <summary>
        /// แปลง string (ciphertext) → payload bits
        /// รูปแบบ: [32-bit length (big-endian)][bits ของแต่ละ byte ของ UTF8 string]
        /// </summary>
        public static bool[] StringToPayloadBits(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            int totalBits = 32 + data.Length * 8;
            bool[] bits = new bool[totalBits];

            // เขียน length header (32 bits, big-endian)
            int len = data.Length;
            for (int i = 0; i < 32; i++)
                bits[i] = ((len >> (31 - i)) & 1) == 1;

            // เขียน data bits
            for (int b = 0; b < data.Length; b++)
                for (int bit = 0; bit < 8; bit++)
                    bits[32 + b * 8 + bit] = ((data[b] >> (7 - bit)) & 1) == 1;

            return bits;
        }

        /// <summary>
        /// แปลง payload bits → string (ciphertext)
        /// อ่าน 32-bit length header ก่อน แล้วอ่าน data bytes
        /// </summary>
        public static string PayloadBitsToString(bool[] bits)
        {
            if (bits.Length < 32)
                throw new InvalidOperationException("Steganotext มีข้อมูลไม่เพียงพอ (น้อยกว่า 32 bits)");

            // อ่าน length header
            int len = 0;
            for (int i = 0; i < 32; i++)
                len = (len << 1) | (bits[i] ? 1 : 0);

            if (len <= 0 || len > 1_000_000)
                throw new InvalidOperationException("ไม่พบข้อมูลที่ซ่อนอยู่ หรือเลือกเทคนิคผิด");

            int requiredBits = 32 + len * 8;
            if (bits.Length < requiredBits)
                throw new InvalidOperationException(
                    $"Steganotext มีข้อมูลไม่ครบ (ต้องการ {requiredBits} bits แต่ได้ {bits.Length})");

            byte[] data = new byte[len];
            for (int b = 0; b < len; b++)
            {
                byte val = 0;
                for (int bit = 0; bit < 8; bit++)
                    val = (byte)((val << 1) | (bits[32 + b * 8 + bit] ? 1 : 0));
                data[b] = val;
            }

            return Encoding.UTF8.GetString(data);
        }

        // ============================================================
        //  PRIVATE HELPERS
        // ============================================================

        /// <summary>
        /// ค้นหาตำแหน่งของคำ Correct/Wrong ทั้งหมดใน text ตามลำดับที่ปรากฏ
        /// Returns: List of (position, pairIndex, isCorrect, matchedWord)
        /// </summary>
        private static List<(int pos, int pairIdx, bool isCorrect, string word)>
            FindMisspellingCandidates(string text, IList<int> activePairIndices)
        {
            // รวบรวม (word, pairIdx, isCorrect) ทั้งหมด
            // เรียงตามตำแหน่งที่พบใน text

            var results = new List<(int pos, int pairIdx, bool isCorrect, string word)>();
            int searchFrom = 0;

            while (searchFrom < text.Length)
            {
                int bestPos = -1;
                int bestPairIdx = -1;
                bool bestIsCorrect = false;
                string bestWord = null;

                foreach (int idx in activePairIndices)
                {
                    var pair = MisspellingPairs[idx];

                    int pc = text.IndexOf(pair.Correct, searchFrom, StringComparison.Ordinal);
                    if (pc >= 0 && (bestPos < 0 || pc < bestPos || (pc == bestPos && pair.Correct.Length > bestWord.Length)))
                    {
                        bestPos = pc; bestPairIdx = idx; bestIsCorrect = true; bestWord = pair.Correct;
                    }

                    int pw = text.IndexOf(pair.Wrong, searchFrom, StringComparison.Ordinal);
                    if (pw >= 0 && (bestPos < 0 || pw < bestPos || (pw == bestPos && pair.Wrong.Length > bestWord.Length)))
                    {
                        bestPos = pw; bestPairIdx = idx; bestIsCorrect = false; bestWord = pair.Wrong;
                    }
                }

                if (bestPos < 0) break;

                results.Add((bestPos, bestPairIdx, bestIsCorrect, bestWord));
                searchFrom = bestPos + bestWord.Length;
            }

            return results;
        }

        // ============================================================
        //  SYNONYM
        // ============================================================

        /// <summary>
        /// กลุ่มคำพ้องภาษาไทย (Synonym Groups)
        /// แต่ละกลุ่มมีคำหลาย 2 คำขึ้นไปที่ความหมายใกล้เคียงกัน
        ///
        /// หลักการเข้ารหัส:
        ///   - แต่ละกลุ่มที่ผู้ใช้เลือกจะให้ 1 bit
        ///   - คำที่ index 0 ในกลุ่ม = bit 0
        ///   - คำที่ index 1 ในกลุ่ม = bit 1
        ///   (ขยายได้เป็น n-ary encoding แต่ตอนนี้ใช้ binary)
        ///
        /// ลำดับ index ต้องตรงกับ checkedListBox1.Items ใน Synonym.Designer.cs
        /// </summary>
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
        /// แทนคำในกลุ่มที่เลือก: index 0 ในกลุ่ม = bit 0, index 1 = bit 1
        /// </summary>
        /// <param name="coverText">ข้อความปกปิด (ต้องมีคำจากกลุ่มที่เลือกเพียงพอ)</param>
        /// <param name="cipherText">Base64 ciphertext ที่จะฝัง</param>
        /// <param name="activeGroupIndices">index ของกลุ่มคำพ้องที่ผู้ใช้เลือก</param>
        public static string SynonymEmbed(string coverText, string cipherText, IList<int> activeGroupIndices)
        {
            if (activeGroupIndices == null || activeGroupIndices.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกกลุ่มคำพ้องอย่างน้อย 1 กลุ่ม");

            bool[] bits = StringToPayloadBits(cipherText);
            var candidates = FindSynonymCandidates(coverText, activeGroupIndices);

            if (candidates.Count < bits.Length)
                throw new InvalidOperationException(
                    $"Covertext มีคำพ้องไม่เพียงพอ (ต้องการ {bits.Length} คำ แต่พบ {candidates.Count})");

            var sb = new StringBuilder(coverText);
            int offset = 0;
            int bitIdx = 0;

            foreach (var (pos, groupIdx, wordVariantIdx, word) in candidates)
            {
                if (bitIdx >= bits.Length) break;

                bool wantBit1 = bits[bitIdx];
                // bit 0 → ใช้ variant 0, bit 1 → ใช้ variant 1
                string target = SynonymGroups[groupIdx][wantBit1 ? 1 : 0];

                int adjustedPos = pos + offset;
                sb.Remove(adjustedPos, word.Length);
                sb.Insert(adjustedPos, target);
                offset += target.Length - word.Length;
                bitIdx++;
            }

            if (bitIdx < bits.Length)
                throw new InvalidOperationException(
                    $"Covertext มีคำพ้องไม่เพียงพอ (ต้องการ {bits.Length} แต่ได้ {bitIdx})");

            return sb.ToString();
        }

        /// <summary>
        /// ดึง ciphertext จาก steganotext ด้วยเทคนิค Synonym
        /// </summary>
        public static string SynonymExtract(string steganotext, IList<int> activeGroupIndices)
        {
            if (activeGroupIndices == null || activeGroupIndices.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกกลุ่มคำพ้องอย่างน้อย 1 กลุ่ม");

            var candidates = FindSynonymCandidates(steganotext, activeGroupIndices);
            var extractedBits = new List<bool>();

            foreach (var (_, _, wordVariantIdx, _) in candidates)
                extractedBits.Add(wordVariantIdx == 1); // variant 0 = bit 0, variant 1 = bit 1

            return PayloadBitsToString(extractedBits.ToArray());
        }

        /// <summary>
        /// ค้นหาตำแหน่งคำทุกคำจากกลุ่มพ้องที่เลือก เรียงตามตำแหน่งใน text
        /// Returns: (pos, groupIdx, variantIdx, matchedWord)
        /// </summary>
        private static List<(int pos, int groupIdx, int variantIdx, string word)>
            FindSynonymCandidates(string text, IList<int> activeGroupIndices)
        {
            var results = new List<(int pos, int groupIdx, int variantIdx, string word)>();
            int searchFrom = 0;

            while (searchFrom < text.Length)
            {
                int bestPos = -1;
                int bestGroupIdx = -1;
                int bestVariantIdx = -1;
                string bestWord = null;

                foreach (int gi in activeGroupIndices)
                {
                    var group = SynonymGroups[gi];
                    for (int vi = 0; vi < group.Length; vi++)
                    {
                        int p = text.IndexOf(group[vi], searchFrom, StringComparison.Ordinal);
                        if (p >= 0 && (bestPos < 0 || p < bestPos || (p == bestPos && group[vi].Length > bestWord.Length)))
                        {
                            bestPos = p;
                            bestGroupIdx = gi;
                            bestVariantIdx = vi;
                            bestWord = group[vi];
                        }
                    }
                }

                if (bestPos < 0) break;

                results.Add((bestPos, bestGroupIdx, bestVariantIdx, bestWord));
                searchFrom = bestPos + bestWord.Length;
            }

            return results;
        }
    }
}
