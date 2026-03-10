using System;
using System.Collections.Generic;
using System.Text;

namespace Steganography
{
    // Engine หลักสำหรับซ่อนและดึงข้อความ
    // รองรับ 5 เทคนิค: Homoglyph, Misspelling, ZWSP, NBSP, Synonym
    public static class SteganographyEngine
    {
        // --- Homoglyph ---

        // คู่ตัวอักษร homoglyph ที่ใช้ในระบบ
        public static readonly (char Original, char Glyph)[] HomoglyphPairs = new[]
        {
            ('ฎ', 'ฏ'),   // ฎ/ฏ — ตรงกับ ChkbxDochada
            ('ข', 'ฃ'),   // ข/ฃ — ตรงกับ ChkbxKhoKhai
            ('ช', 'ซ'),   // ช/ซ — ตรงกับ ChkbxChoChang
        };

        /// <summary>
        /// ฝัง ciphertext ลงใน covertext ด้วย Homoglyph (original = bit 0, glyph = bit 1)
        /// </summary>
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

                // normalize glyph กลับเป็น original ก่อน กัน double-encoding
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

        /// <summary>
        /// ดึง ciphertext จาก steganotext ด้วย Homoglyph
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
                    extractedBits.Add(false);
                else if (glyphSet.Contains(c))
                    extractedBits.Add(true);
            }

            return PayloadBitsToString(extractedBits.ToArray());
        }

        // --- Misspelling ---

        // 65 คู่คำถูก/ผิด — index ต้องตรงกับ checkedListBox ใน Misspelling.Designer.cs
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
        /// ฝัง ciphertext ด้วย Misspelling — คำถูก = bit 0, คำผิด = bit 1
        /// </summary>
        public static string MisspellingEmbed(string coverText, string cipherText, IList<int> activePairIndices)
        {
            if (activePairIndices == null || activePairIndices.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกคู่คำ Misspelling อย่างน้อย 1 คู่");

            bool[] bits = StringToPayloadBits(cipherText);
            var sb = new StringBuilder(coverText);
            var candidates = FindMisspellingCandidates(coverText, activePairIndices);

            // offset เพราะคำถูก/ผิดอาจยาวไม่เท่ากัน ทำให้ตำแหน่งเลื่อน
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

        /// <summary>
        /// ดึง ciphertext จาก steganotext ด้วย Misspelling (reverse ของ Embed)
        /// </summary>
        public static string MisspellingExtract(string steganotext, IList<int> activePairIndices)
        {
            if (activePairIndices == null || activePairIndices.Count == 0)
                throw new InvalidOperationException("กรุณาเลือกคู่คำ Misspelling อย่างน้อย 1 คู่");

            var candidates = FindMisspellingCandidates(steganotext, activePairIndices);
            var extractedBits = new List<bool>();

            foreach (var (_, pairIdx, isCorrect, _) in candidates)
                extractedBits.Add(!isCorrect);

            return PayloadBitsToString(extractedBits.ToArray());
        }

        // --- ZWSP (Zero-Width Space U+200B) ---

        private const char ZWSP = '\u200B';
        private const char NBSP = '\u00A0';

        // ลบ ZWSP ออก + แปลง NBSP เป็น space ปกติ ก่อน embed
        private static string NormalizeCovertext(string coverText)
        {
            var sb = new StringBuilder(coverText.Length);
            foreach (char c in coverText)
            {
                if (c == ZWSP) continue;
                if (c == NBSP) { sb.Append(' '); continue; }
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// ฝัง ciphertext ด้วย ZWSP — แทรก U+200B ระหว่างตัวอักษร (bit 1 = มี ZWSP)
        /// </summary>
        public static string ZWSPEmbed(string coverText, string cipherText)
        {
            coverText = NormalizeCovertext(coverText);
            bool[] bits = StringToPayloadBits(cipherText);

            // slot = ช่องระหว่างตัวอักษร = length - 1
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
        /// ดึง ciphertext จาก steganotext ด้วย ZWSP
        /// </summary>
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
                        bits.Add(prevHadZWSP);
                    prevWasNormal = true;
                    prevHadZWSP = false;
                }
            }

            return PayloadBitsToString(bits.ToArray());
        }

        // --- NBSP (Non-Breaking Space U+00A0) ---
        // ทำงานคล้ายๆ ZWSP แต่ใช้ NBSP แทน space ปกติ

        /// <summary>
        /// ฝัง ciphertext ด้วย NBSP — space ปกติ = bit 0, NBSP = bit 1
        /// </summary>
        public static string NBSPEmbed(string coverText, string cipherText)
        {
            coverText = NormalizeCovertext(coverText);
            bool[] bits = StringToPayloadBits(cipherText);

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

        /// <summary>
        /// ดึง ciphertext จาก steganotext ด้วย NBSP
        /// </summary>
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

        // --- Payload Bits (ใช้ร่วมกันทุกเทคนิค) ---
        // format: [32-bit length header (big-endian)] [data bits]

        /// <summary>
        /// แปลง string เป็น payload bits — 32-bit header + data bits (MSB first)
        /// </summary>
        public static bool[] StringToPayloadBits(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            int totalBits = 32 + data.Length * 8;
            bool[] bits = new bool[totalBits];

            // header 32 bit บอกจำนวน bytes
            int len = data.Length;
            for (int i = 0; i < 32; i++)
                bits[i] = ((len >> (31 - i)) & 1) == 1;

            // convert to bits
            for (int b = 0; b < data.Length; b++)
                for (int bit = 0; bit < 8; bit++)
                    bits[32 + b * 8 + bit] = ((data[b] >> (7 - bit)) & 1) == 1;

            return bits;
        }

        /// <summary>
        /// แปลง payload bits กลับเป็น string — อ่าน header แล้วอ่าน data
        /// </summary>
        public static string PayloadBitsToString(bool[] bits)
        {
            if (bits.Length < 32)
                throw new InvalidOperationException("Steganotext มีข้อมูลไม่เพียงพอ (น้อยกว่า 32 bits)");

            // อ่าน header
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

        // --- Private Helpers ---

        // greedy left-to-right search สำหรับ Misspelling
        // ถ้าตำแหน่งเท่ากันเลือกคำยาวกว่า กัน substring ซ้อน
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

                results.Add((bestPos, bestPairIdx, bestIsCorrect, bestWord));
                searchFrom = bestPos + bestWord.Length;
            }

            return results;
        }

        // --- Synonym ---

        // 64 กลุ่มคำพ้อง — index ต้องตรงกับ checkedListBox ใน Synonym.Designer.cs
        // variant 0 = bit 0, variant 1 = bit 1
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

        /// <summary>
        /// ฝัง ciphertext ด้วย Synonym — ทำงานคล้าย Misspelling แต่ใช้คำพ้องแทน
        /// </summary>
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

        /// <summary>
        /// ดึง ciphertext จาก steganotext ด้วย Synonym
        /// </summary>
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

        // อันนี้ logic เหมือน FindMisspellingCandidates เลย แค่เปลี่ยนมาใช้ SynonymGroups
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
