# MEMORIES.md — บันทึกการตัดสินใจและข้อควรจำ

> ไฟล์นี้บันทึกการตัดสินใจทางเทคนิค, สิ่งที่แก้ไข, และข้อสังเกตสำคัญ
> อัปเดตทุกครั้งที่มีการเปลี่ยนแปลงสำคัญ

---

## Session: 2026-03-05 (ครั้งที่ 1) — Implement Steganography Logic ทั้งระบบ

### งานที่ทำ
1. สร้าง `SteganographyEngine.cs` — คลาสกลาง Embed/Extract ทุกเทคนิค
2. แก้ `OptionStaganogryphy.cs` + Designer → Homoglyph form สมบูรณ์ (เพิ่มปุ่ม OK/Cancel)
3. แก้ `Misspelling.cs` + Designer → Misspelling form สมบูรณ์ (เพิ่ม SelectAll/ClearAll)
4. แก้ `Space.cs` + Designer → ZWSP form สมบูรณ์ (เขียน UI ใหม่ทั้งหมด)
5. เขียน `SteganographyForm.cs` ใหม่ — ผูก handler ทุกปุ่ม Embed/Extract/Decrypt
6. แก้ AES: เปลี่ยน salt จาก `plaintext bytes` → salt คงที่ `"Stegano2025Educa"`
7. เพิ่ม `SteganographyEngine.cs` ใน `Steganography.csproj`

---

## Session: 2026-03-05 (ครั้งที่ 3) — Synonym sub-form + Drag & Drop

### งานที่ทำ
1. สร้าง `Synonym.cs` — sub-form สำหรับเทคนิค Synonym (Embed/Extract)
   - Pattern เดียวกับ Misspelling.cs: properties IsEmbedMode/CipherText/CoverText/SteganotextInput/ResultText
   - `ExecuteSynonym()` เรียก `SteganographyEngine.SynonymEmbed` หรือ `SynonymExtract`
   - `GetActiveGroupIndices()` อ่านจาก `checkedListBox1`
2. สร้าง `Synonym.Designer.cs` — UI layout (800×490 px)
   - `CheckedListBox` 64 รายการ รูปแบบ `"คำซ้าย / คำขวา"` (bit 0 / bit 1)
   - ปุ่ม: เลือกทั้งหมด, ยกเลิกทั้งหมด, ตกลง (เขียว), ยกเลิก (แดง)
   - Label อธิบาย: `"คำซ้าย = bit 0  |  คำขวา = bit 1"`
3. เพิ่ม `Synonym.cs` + `Synonym.Designer.cs` ใน `Steganography.csproj`
4. แก้ `BtSnn_Click` ใน `SteganographyForm.cs` → เปิด Synonym sub-form (Embed)
5. แก้ `BtSnnD_Click` ใน `SteganographyForm.cs` → เปิด Synonym sub-form (Extract)
6. Implement Drag & Drop ใน `SteganographyForm_Load`:
   - `InputPlaintext.AllowDrop = true` → drag .txt เข้า Encryption tab
   - `InputChipertext.AllowDrop = true` → drag .txt เข้า Decryption tab
   - ใช้ `LoadFileIntoTextBox()` helper method (แทน inline code)

---

### งานที่ทำ
1. ผูก handlers ที่ยังขาดทั้งหมดใน `SteganographyForm.Designer.cs`:
   - `TabControl1.SelectedIndexChanged` → sync Ciphertext อัตโนมัติ
   - `CopyButton2_Click` (copy ciphertext), `CopyButton3_Click` (copy plaintext)
   - `BtRs3_Click` (reset stegano tab), `BtRs5_Click` (reset key), `BtRs6_Click` (reset plaintext)
   - `BtSnn_Click`, `BtSnnD_Click` (Synonym — แสดง MessageBox ยังไม่รองรับ)
   - `textBox1_TextChanged`, `InputCovertext_TextChanged` → อัปเดต bit capacity ใน status bar
   - `steganographyInfoToolStripMenuItem_Click`
2. เพิ่ม `BtSavePlain` button ใน Decryption tab → บันทึก plaintext ลงไฟล์
3. เพิ่ม `SteganographyForm_Load` — ตั้งค่า ToolTip และ status bar
4. เพิ่ม `UpdateSteganoBitCapacity()` — แสดงจำนวน bits ที่ต้องการ vs ZWSP slots
5. Sync Ciphertext อัตโนมัติเมื่อ: กด Encryption button + เปลี่ยนมา Stegano tab
6. เพิ่ม `steganographyInfoToolStripMenuItem_Click` handler แสดงรายละเอียดเทคนิค

---

## การตัดสินใจทางเทคนิคที่สำคัญ

### [AES Salt — คงที่]
**ปัญหา:** โค้ดเดิมใช้ `plaintext` เป็น PBKDF2 salt → ตอนถอดรหัสไม่รู้ plaintext → ถอดไม่ได้  
**แก้ไข:** salt คงที่ 16 bytes = `"Stegano2025Educa"`  
**ผลกระทบ:** ciphertext จากโค้ดเดิม incompatible กับโค้ดใหม่  
**ไฟล์:** `SteganographyForm.cs:AesSalt`

---

### [Payload Format — 32-bit header]
**รูปแบบ:** `[32 bits: byte count][8 bits/byte × N bytes]` (UTF-8, MSB first, big-endian)  
**เหตุผล:** ต้องรู้จำนวน bytes แน่นอนเพื่อ extract ไม่อ่านเกิน  
**ไฟล์:** `SteganographyEngine.cs:StringToPayloadBits`, `PayloadBitsToString`

---

### [Homoglyph — normalize ก่อน embed]
ถ้า covertext มี glyph character อยู่แล้ว → normalize กลับเป็น original ก่อน แล้วค่อยใส่ bit  
ป้องกัน double-encoding  
**ไฟล์:** `SteganographyEngine.cs:HomoglyphEmbed`

---

### [Misspelling — greedy left-to-right, non-overlapping]
เลือกคำที่อยู่ซ้ายสุดก่อนเสมอ → deterministic → embed กับ extract ผลตรงกันถ้าใช้ active pairs เดิม  
**ไฟล์:** `SteganographyEngine.cs:FindMisspellingCandidates`

---

### [ZWSP — slot = ช่องระหว่างตัวอักษร]
slot[i] = ช่องระหว่าง char[i] และ char[i+1]: มี U+200B = bit 1, ไม่มี = bit 0  
ความจุ = `coverText.Length - 1` bits  
**ไฟล์:** `SteganographyEngine.cs:ZWSPEmbed`, `ZWSPExtract`

---

### [Sub-form Properties Pattern]
ทุก sub-form ใช้ properties แทน constructor parameters:
```csharp
form.IsEmbedMode = true/false;
form.CipherText = ...;   // embed
form.CoverText = ...;    // embed
form.SteganotextInput = ...;  // extract
// หลัง ShowDialog() → form.ResultText
```

---

### [Covertext อยู่ใน Encryption tab]
`InputCovertext` ถูกวางใน `TabEncrytion` ตั้งแต่ต้น (auto-generated Designer)  
ใช้ method `GetCovertext()` ใน SteganographyForm เพื่อดึงค่า → ไม่ต้องย้าย control  
ถ้าต้องการย้ายไป Steganography tab ต้องแก้ Designer และ TabSteganography.Controls.Add

---

### [Bit Capacity Display]
`UpdateSteganoBitCapacity()` คำนวณ bits ที่ต้องการจาก ciphertext และ ZWSP slots จาก covertext  
แสดงใน `tsslLabel` (status bar ล่าง) — ช่วยผู้ใช้รู้ว่า covertext ยาวพอหรือไม่  
เรียกเมื่อ: textBox1 เปลี่ยน, InputCovertext เปลี่ยน, เปลี่ยนมา Stegano tab

---

## ข้อควรระวัง (Gotchas)

| เรื่อง | รายละเอียด |
|--------|------------|
| **MisspellingPairs index** | ต้องตรงกับ items ใน `checkedListBox1` เสมอ — ถ้าเพิ่ม/ลบคู่ต้องอัปเดตทั้ง Engine และ Designer |
| **HomoglyphPairs index** | index 0=ฎ/ฏ, 1=เ/แ, 2=ด/ต, 3=ข/ฃ, 4=ช/ซ — ห้ามสลับ |
| **SynonymGroups index** | ต้องตรงกับ checkedListBox1.Items ใน Synonym.Designer.cs — ถ้าเพิ่ม/ลบกลุ่มต้องอัปเดตทั้งสองที่ |
| **Designer.cs** | ถ้า build บน Visual Studio อย่าแก้ด้วยมือ — จะถูก overwrite |
| **UTF-8 encoding** | payload ใช้ UTF-8 เสมอ — เปลี่ยนเป็น encoding อื่น = incompatible |
| **ZWSP กับ text editor** | อย่าเปิด steganotext ด้วย editor ที่ strip invisible chars — จะลบ U+200B ออก |
| **Covertext ใน Encryption tab** | `InputCovertext` อยู่ใน TabEncrytion ตาม Designer เดิม ใช้ GetCovertext() แทน |

---

## สถานะ Features ปัจจุบัน (2026-03-05)

| Feature | สถานะ |
|---------|--------|
| AES-256 Encrypt/Decrypt | ✅ สมบูรณ์ |
| Homoglyph Embed/Extract | ✅ สมบูรณ์ |
| Misspelling Embed/Extract | ✅ สมบูรณ์ |
| ZWSP Embed/Extract | ✅ สมบูรณ์ |
| Synonym Embed/Extract | ✅ สมบูรณ์ (64 กลุ่มคำพ้อง) |
| Sync Ciphertext อัตโนมัติ | ✅ สมบูรณ์ |
| Copy Key/Ciphertext/Plaintext/Steganotext | ✅ สมบูรณ์ |
| Save Steganotext/Plaintext | ✅ สมบูรณ์ |
| Reset ทุกปุ่ม | ✅ สมบูรณ์ |
| Bit Capacity ใน Status Bar | ✅ สมบูรณ์ |
| Steganography Info menu | ✅ สมบูรณ์ |
| Browse file (.txt) | ✅ สมบูรณ์ |
| Drag & Drop (InputPlaintext + InputChipertext) | ✅ สมบูรณ์ |
