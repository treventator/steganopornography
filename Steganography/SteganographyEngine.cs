using System;
using System.Collections.Generic;
using System.Text;

namespace Steganography
{
    /// <summary>
    /// เครื่องมือหลักสำหรับฝังและดึงข้อความลับในข้อความปกปิด
    /// รองรับ 5 เทคนิค: Homoglyph, Misspelling, ZWSP, NBSP, Synonym
    ///
    /// รูปแบบ payload ที่ฝัง:
    ///   [32-bit length header][16-bit CRC-16][ข้อมูล bits]
    ///   [H1 FIX] เพิ่ม CRC-16 เพื่อตรวจสอบ integrity ก่อน AES decrypt
    /// </summary>
    public static class SteganographyEngine
    {
        // ============================================================
        //  HOMOGLYPH
        //  [M1 FIX] ลบคู่ เ↔แ (เปลี่ยนความหมาย: เก่า→แก่า)
        //  [M2 FIX] ลบคู่ ด↔ต (เปลี่ยนความหมาย: ดี→ตี)
        //  คงเหลือ 3 คู่ที่ glyph คล้ายกันจริงๆ
        // ============================================================

        public static readonly (char Original, char Glyph)[] HomoglyphPairs = new[]
        {
            ('ฎ', 'ฏ'),   // pair 0  — ดอชะดา / ปะตัก (glyph คล้ายมาก)
            ('ข', 'ฃ'),   // pair 1  — ขอไข่ / ขอขวด (ฃ เลิกใช้แล้ว, glyph คล้าย)
            ('ช', 'ซ'),   // pair 2  — ชอช้าง / ซอโซ่ (glyph คล้าย)
        };

        public static string HomoglyphEmbed(string coverText, string cipherText, IList<int> activePairs)
        {
            if (activePairs == null || activePairs.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกคู่ตัวอักษร Homoglyph อย่างน้อย 1 คู่");

            var originalToGlyph = new Dictionary<char, char>();
            var glyphToOriginal = new Dictionary<char, char>();
            foreach (int idx in activePairs)
            {
                var p = HomoglyphPairs[idx];
                originalToGlyph[p.Original] = p.Glyph;
                glyphToOriginal[p.Glyph] = p.Original;
            }

            bool[] bits = StringToPayloadBits(cipherText);

            var result = new StringBuilder(coverText);
            int bitIdx = 0;

            for (int i = 0; i < result.Length && bitIdx < bits.Length; i++)
            {
                char c = result[i];
                char original = glyphToOriginal.ContainsKey(c) ? glyphToOriginal[c] : c;

                if (originalToGlyph.ContainsKey(original))
                {
                    result[i] = bits[bitIdx] ? originalToGlyph[original] : original;
                    bitIdx++;
                }
            }

            if (bitIdx < bits.Length)
                throw new InvalidOperationException(
                    $"Covertext มีตัวอักษร Homoglyph ไม่เพียงพอ (ต้องการ {bits.Length} ตำแหน่ง แต่ได้ {bitIdx})");

            return result.ToString();
        }

        public static string HomoglyphExtract(string steganotextInput, IList<int> activePairs)
        {
            if (activePairs == null || activePairs.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกคู่ตัวอักษร Homoglyph อย่างน้อย 1 คู่");

            var originalSet = new HashSet<char>();
            var glyphSet = new HashSet<char>();

            foreach (int idx in activePairs)
            {
                var p = HomoglyphPairs[idx];
                originalSet.Add(p.Original);
                glyphSet.Add(p.Glyph);
            }

            var extractedBits = new List<bool>();
            foreach (char c in steganotextInput)
            {
                if (originalSet.Contains(c))
                    extractedBits.Add(false);
                else if (glyphSet.Contains(c))
                    extractedBits.Add(true);
            }

            return PayloadBitsToString(extractedBits.ToArray());
        }

        // ============================================================
        //  MISSPELLING
        //  [M3 FIX] เพิ่ม IsPartOfLongerPairWord() ป้องกัน substring overlap
        // ============================================================

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

                bool wantBit1 = bits[bitIdx];
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

        public static string MisspellingExtract(string steganotext, IList<int> activePairIndices)
        {
            if (activePairIndices == null || activePairIndices.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกคู่คำ Misspelling อย่างน้อย 1 คู่");

            var candidates = FindMisspellingCandidates(steganotext, activePairIndices);
            var extractedBits = new List<bool>();

            foreach (var (_, pairIdx, isCorrect, _) in candidates)
            {
                extractedBits.Add(!isCorrect);
            }

            return PayloadBitsToString(extractedBits.ToArray());
        }

        // ============================================================
        //  ZWSP (Zero-Width Space  U+200B)
        // ============================================================

        private const char ZWSP = '\u200B';

        public static string ZWSPEmbed(string coverText, string cipherText)
        {
            bool[] bits = StringToPayloadBits(cipherText);

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

        public static string ZWSPExtract(string steganotext)
        {
            var bits = new List<bool>();
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
                        bits.Add(prevHadZWSP);
                    }
                    prevWasNormal = true;
                    prevHadZWSP = false;
                }
            }

            return PayloadBitsToString(bits.ToArray());
        }

        // ============================================================
        //  NBSP (Non-Breaking Space  U+00A0)
        //  แทนที่ space ปกติ (U+0020) ด้วย NBSP (U+00A0)
        //  space ปกติ = bit 0, NBSP = bit 1
        // ============================================================

        private const char NBSP = '\u00A0';

        public static string NBSPEmbed(string coverText, string cipherText)
        {
            bool[] bits = StringToPayloadBits(cipherText);

            // นับจำนวน space ปกติใน covertext
            int spaceCount = 0;
            foreach (char c in coverText)
                if (c == ' ') spaceCount++;

            if (spaceCount < bits.Length)
                throw new InvalidOperationException(
                    $"Covertext มี space ไม่เพียงพอ (ต้องการ {bits.Length} ช่อง แต่มี space {spaceCount} ตัว)");

            var sb = new StringBuilder(coverText.Length);
            int bitIdx = 0;

            foreach (char c in coverText)
            {
                if (c == ' ' && bitIdx < bits.Length)
                {
                    sb.Append(bits[bitIdx] ? NBSP : ' ');
                    bitIdx++;
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static string NBSPExtract(string steganotext)
        {
            var bits = new List<bool>();

            foreach (char c in steganotext)
            {
                if (c == ' ')
                    bits.Add(false);
                else if (c == NBSP)
                    bits.Add(true);
            }

            return PayloadBitsToString(bits.ToArray());
        }

        // ============================================================
        //  SYNONYM
        //  [M4 FIX] เพิ่ม word boundary check สำหรับคำสั้น
        // ============================================================

        public static readonly string[][] SynonymGroups = new[]
        {
            new[] { "กล่าว",        "พูด" },
            new[] { "ได้รับ",       "ได้มา" },
            new[] { "ทำ",           "กระทำ" },
            new[] { "ใช้",          "นำมาใช้" },
            new[] { "มอง",          "เฝ้ามอง" },
            new[] { "เดิน",         "ย่าง" },
            new[] { "คิด",          "ไตร่ตรอง" },
            new[] { "หยุด",         "หยุดชะงัก" },
            new[] { "ต้องการ",      "ประสงค์" },
            new[] { "เริ่ม",        "เริ่มต้น" },
            new[] { "เสร็จ",        "เสร็จสิ้น" },
            new[] { "ช่วย",         "ให้ความช่วยเหลือ" },
            new[] { "บอก",          "แจ้ง" },
            new[] { "ถาม",          "สอบถาม" },
            new[] { "ตอบ",          "ตอบรับ" },
            new[] { "รู้จัก",        "ทราบ" },
            new[] { "เข้าใจ",       "ตระหนัก" },
            new[] { "ส่ง",          "จัดส่ง" },
            new[] { "รับทราบ",      "รับมอบ" },
            new[] { "สร้าง",        "ก่อสร้าง" },
            new[] { "แก้ไข",        "ปรับปรุง" },
            new[] { "ลบออก",        "ขจัด" },
            new[] { "เพิ่ม",        "เพิ่มเติม" },
            new[] { "ลดทอน",        "ลดลง" },
            new[] { "เปลี่ยน",      "เปลี่ยนแปลง" },
            new[] { "ยืนยัน",       "รับรอง" },
            new[] { "ปฏิเสธ",       "ไม่ยอมรับ" },
            new[] { "อนุมัติ",      "เห็นชอบ" },
            new[] { "ตรวจสอบ",      "พิจารณา" },
            new[] { "รายงาน",       "แจ้งรายงาน" },
            new[] { "ประชุม",       "ประชุมหารือ" },
            new[] { "ตัดสิน",       "วินิจฉัย" },
            new[] { "เลือก",        "คัดเลือก" },
            new[] { "จัดเตรียม",    "จัดการ" },
            new[] { "ควบคุม",       "กำกับดูแล" },
            new[] { "พัฒนา",        "ปรับพัฒนา" },
            new[] { "วางแผน",       "กำหนดแผน" },
            new[] { "ดำเนิน",       "ดำเนินการ" },
            new[] { "ปรึกษา",       "หารือ" },
            new[] { "สรุป",         "สรุปผล" },
            new[] { "นำเสนอ",       "นำออกเสนอ" },
            new[] { "อธิบาย",       "ชี้แจง" },
            new[] { "แสดง",         "แสดงให้เห็น" },
            new[] { "ระบุ",         "ชี้บ่ง" },
            new[] { "กำหนด",        "ตั้งกำหนด" },
            new[] { "สนับสนุน",     "ให้การสนับสนุน" },
            new[] { "คัดค้าน",      "โต้แย้ง" },
            new[] { "เสนอ",         "ยื่นข้อเสนอ" },
            new[] { "ขออนุญาต",     "ร้องขอ" },
            new[] { "ขอบคุณ",       "ขอบพระคุณ" },
            new[] { "ขอโทษ",        "ขอประทานโทษ" },
            new[] { "ยินดี",        "มีความยินดี" },
            new[] { "เห็นด้วย",     "เห็นพ้อง" },
            new[] { "ไม่เห็นด้วย",  "ขัดแย้ง" },
            new[] { "รอคอย",        "คอยท่า" },
            new[] { "รีบ",          "รีบด่วน" },
            new[] { "สำเร็จ",       "บรรลุผล" },
            new[] { "ล้มเหลว",      "ไม่สำเร็จ" },
            new[] { "ยาก",          "ลำบาก" },
            new[] { "ง่าย",         "สะดวก" },
            new[] { "ดีมาก",        "เป็นประโยชน์" },
            new[] { "เลว",          "ไม่ดี" },
            new[] { "สำคัญ",        "มีความสำคัญ" },
            new[] { "จำเป็น",       "มีความจำเป็น" },
        };

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

        public static string SynonymExtract(string steganotext, IList<int> activeGroupIndices)
        {
            if (activeGroupIndices == null || activeGroupIndices.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกกลุ่มคำพ้องอย่างน้อย 1 กลุ่ม");

            var candidates = FindSynonymCandidates(steganotext, activeGroupIndices);
            var extractedBits = new List<bool>();

            foreach (var (_, _, wordVariantIdx, _) in candidates)
                extractedBits.Add(wordVariantIdx == 1);

            return PayloadBitsToString(extractedBits.ToArray());
        }

        // ============================================================
        //  UTILITY: Payload Bits Encoding/Decoding
        //  [H1 FIX] เพิ่ม CRC-16 ใน payload header
        //  [H2 FIX] ลด max length จาก 1M เป็น 100K
        //
        //  Format: [32-bit length (big-endian)][16-bit CRC-16][data bits]
        // ============================================================

        public static bool[] StringToPayloadBits(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            ushort crc = ComputeCrc16(data);
            int totalBits = 32 + 16 + data.Length * 8; // +16 for CRC

            bool[] bits = new bool[totalBits];

            // Length header (32 bits, big-endian)
            int len = data.Length;
            for (int i = 0; i < 32; i++)
                bits[i] = ((len >> (31 - i)) & 1) == 1;

            // CRC-16 (16 bits, big-endian)
            for (int i = 0; i < 16; i++)
                bits[32 + i] = ((crc >> (15 - i)) & 1) == 1;

            // Data bits
            for (int b = 0; b < data.Length; b++)
                for (int bit = 0; bit < 8; bit++)
                    bits[48 + b * 8 + bit] = ((data[b] >> (7 - bit)) & 1) == 1;

            return bits;
        }

        public static string PayloadBitsToString(bool[] bits)
        {
            if (bits.Length < 48)
                throw new InvalidOperationException("Steganotext มีข้อมูลไม่เพียงพอ (น้อยกว่า 48 bits)");

            // อ่าน length header
            int len = 0;
            for (int i = 0; i < 32; i++)
                len = (len << 1) | (bits[i] ? 1 : 0);

            // [H2] ลด max จาก 1M → 100K
            if (len <= 0 || len > 100_000)
                throw new InvalidOperationException("ไม่พบข้อมูลที่ซ่อนอยู่ หรือเลือกเทคนิคผิด");

            // อ่าน CRC-16
            ushort storedCrc = 0;
            for (int i = 0; i < 16; i++)
                storedCrc = (ushort)((storedCrc << 1) | (bits[32 + i] ? 1 : 0));

            int requiredBits = 48 + len * 8;
            if (bits.Length < requiredBits)
                throw new InvalidOperationException(
                    $"Steganotext มีข้อมูลไม่ครบ (ต้องการ {requiredBits} bits แต่ได้ {bits.Length})");

            byte[] data = new byte[len];
            for (int b = 0; b < len; b++)
            {
                byte val = 0;
                for (int bit = 0; bit < 8; bit++)
                    val = (byte)((val << 1) | (bits[48 + b * 8 + bit] ? 1 : 0));
                data[b] = val;
            }

            // [H1] ตรวจ CRC-16
            ushort computedCrc = ComputeCrc16(data);
            if (storedCrc != computedCrc)
                throw new InvalidOperationException(
                    "ข้อมูลที่ซ่อนไว้เสียหาย หรือเลือกเทคนิค/คู่ผิด (CRC ไม่ตรง)");

            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// CRC-16/CCITT-FALSE
        /// </summary>
        private static ushort ComputeCrc16(byte[] data)
        {
            ushort crc = 0xFFFF;
            foreach (byte b in data)
            {
                crc ^= (ushort)(b << 8);
                for (int i = 0; i < 8; i++)
                    crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1);
            }
            return crc;
        }

        // ============================================================
        //  PRIVATE HELPERS
        // ============================================================

        /// <summary>
        /// [M3 FIX] ตรวจว่า match ที่ตำแหน่ง pos เป็น substring ของคำที่ยาวกว่าใน MisspellingPairs หรือไม่
        /// ป้องกัน: "ราชการ" match ภายใน "ข้าราชการ"
        /// </summary>
        private static bool IsMisspellingSubstringOfLongerPair(string text, int pos, string matchedWord)
        {
            for (int idx = 0; idx < MisspellingPairs.Length; idx++)
            {
                var pair = MisspellingPairs[idx];
                foreach (var longerWord in new[] { pair.Correct, pair.Wrong })
                {
                    if (longerWord.Length <= matchedWord.Length) continue;

                    // หา matchedWord ใน longerWord
                    int subIdx = longerWord.IndexOf(matchedWord, StringComparison.Ordinal);
                    while (subIdx >= 0)
                    {
                        // ตรวจว่า longerWord ปรากฏจริงใน text ที่ตำแหน่งครอบคลุม pos
                        int textStart = pos - subIdx;
                        if (textStart >= 0 && textStart + longerWord.Length <= text.Length &&
                            string.Compare(text, textStart, longerWord, 0, longerWord.Length, StringComparison.Ordinal) == 0)
                            return true;

                        subIdx = longerWord.IndexOf(matchedWord, subIdx + 1, StringComparison.Ordinal);
                    }
                }
            }
            return false;
        }

        private static List<(int pos, int pairIdx, bool isCorrect, string word)>
            FindMisspellingCandidates(string text, IList<int> activePairIndices)
        {
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

                // [M3] ข้ามถ้าเป็น substring ของคำที่ยาวกว่าใน MisspellingPairs
                if (IsMisspellingSubstringOfLongerPair(text, bestPos, bestWord))
                {
                    searchFrom = bestPos + bestWord.Length;
                    continue;
                }

                results.Add((bestPos, bestPairIdx, bestIsCorrect, bestWord));
                searchFrom = bestPos + bestWord.Length;
            }

            return results;
        }

        /// <summary>
        /// [M4 FIX] ตรวจว่าตัวอักษรเป็น Thai character (พยัญชนะ, สระ, วรรณยุกต์, ตัวเลข)
        /// </summary>
        private static bool IsThaiChar(char c)
        {
            return c >= '\u0E01' && c <= '\u0E5B';
        }

        /// <summary>
        /// [M4 FIX] ตรวจ word boundary สำหรับคำสั้น
        /// คำสั้น (≤ 3 chars) ที่ถูกขนาบด้วย Thai chars ทั้งซ้ายขวา → น่าจะเป็น substring ของ compound word
        /// เช่น "ทำ" ใน "ทำงาน" → ซ้ายไม่มี, ขวามี "ง" → ข้าม
        /// </summary>
        private static bool IsSynonymLikelyCompoundSubstring(string text, int pos, string word)
        {
            // เฉพาะคำสั้นเท่านั้นที่ต้องตรวจ (คำยาวมักจะ match ถูกต้อง)
            if (word.Length > 3) return false;

            int endPos = pos + word.Length;

            // ตรวจว่ามี Thai char ต่อท้ายทันที → น่าจะเป็นส่วนหนึ่งของคำรวม
            if (endPos < text.Length && IsThaiChar(text[endPos]))
                return true;

            // ตรวจว่ามี Thai char นำหน้าทันที (ไม่ใช่ space/เครื่องหมาย/ขึ้นบรรทัดใหม่)
            if (pos > 0 && IsThaiChar(text[pos - 1]))
                return true;

            return false;
        }

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

                // [M4] ข้ามคำสั้นที่น่าจะเป็น substring ของ compound word
                if (IsSynonymLikelyCompoundSubstring(text, bestPos, bestWord))
                {
                    searchFrom = bestPos + bestWord.Length;
                    continue;
                }

                results.Add((bestPos, bestGroupIdx, bestVariantIdx, bestWord));
                searchFrom = bestPos + bestWord.Length;
            }

            return results;
        }
    }
}
