# CLAUDE.md — เอกสารออกแบบระบบ Steganography

> ไฟล์นี้เป็น context สำหรับ AI assistant (Claude/Antigravity) ที่ช่วยพัฒนาโปรเจคนี้
> อ่านก่อนเริ่มทำงานทุกครั้ง

---

## 1. ภาพรวมโปรเจค

**ชื่อ:** Steganography Desktop Application  
**เทคโนโลยี:** C#, Windows Forms, .NET Framework 4.8  
**วัตถุประสงค์:** เข้ารหัส (AES-256) และซ่อนข้อความในข้อความภาษาไทย เพื่อการศึกษาเท่านั้น  
**IDE เดิม:** Visual Studio บน Windows (path `D:/Project/...`)  
**Platform ปัจจุบัน:** macOS (cross-develop, build บน Windows)

---

## 2. โครงสร้างไฟล์

```
Steganography/
├── Program.cs                    # Entry point → Application.Run(new SteganographyForm())
├── SteganographyForm.cs          # Main form: handlers ทุกปุ่ม + tab logic + Drag&Drop
├── SteganographyForm.Designer.cs # Auto-generated UI layout (ห้ามแก้ด้วยมือถ้าใช้ Designer)
├── CryptoHelper.cs               # AES-256-CBC encrypt/decrypt (แยกจาก form)
├── SteganographyEngine.cs        # คลาสกลาง: Embed/Extract ทุกเทคนิค + payload encoding
├── OptionStaganogryphy.cs        # Sub-form: Homoglyph (ฎ/ฏ, ข/ฃ, ช/ซ)
├── OptionStaganogryphy.Designer.cs
├── Misspelling.cs                # Sub-form: Misspelling (64 คู่คำภาษาไทย)
├── Misspelling.Designer.cs
├── Space.cs                      # Sub-form: Zero-Width Space (U+200B)
├── Space.Designer.cs
├── Nbsp.cs                       # Sub-form: Non-Breaking Space (U+00A0)
├── Nbsp.Designer.cs
├── Synonym.cs                    # Sub-form: Synonym (64 กลุ่มคำพ้อง)
├── Synonym.Designer.cs
├── Steganography.csproj          # MSBuild project (include ทุกไฟล์แล้ว)
└── Properties/
    ├── AssemblyInfo.cs
    ├── Resources.Designer.cs
    └── Settings.Designer.cs
```

---

## 3. Architecture และ Data Flow

### 3.1 Encode Flow (ซ่อนข้อความ)

```
[Tab: Encryption]
  Plaintext + Key
    → AesEncrypt(plaintext, key)           [salt สุ่ม 16 bytes]
    → Base64 Ciphertext → InputPayload + sync ไปที่ textBox1 อัตโนมัติ

[Tab: Steganography]
  textBox1 (Ciphertext) + InputCovertext (Covertext จาก Encryption tab)
    → เลือกเทคนิค (BtHm / BtMs / BtSp / BtNb / BtSnn)
    → เปิด sub-form → ผู้ใช้เลือก options → กด ตกลง
    → SteganographyEngine.Xxx Embed(coverText, cipherText, options)
    → TbStegano (Steganotext)  → Save / Copy
```

### 3.2 Decode Flow (ถอดข้อความ)

```
[Tab: Decryption]
  InputChipertext (Steganotext) + InputkeyDecry (Key)
    → เลือกเทคนิค (BtHmD / BtMsD / BtSpD / BtNbD / BtSnnD)
    → เปิด sub-form → เลือก options เดิม → กด ตกลง
    → SteganographyEngine.Xxx Extract(steganotext, options)
    → Base64 Ciphertext
    → AesDecrypt(cipherBase64, key)
    → OutputPlaintext  → Copy / Save Plaintext
```

---

## 4. SteganographyEngine.cs — หัวใจของระบบ

### 4.1 Payload Format (ใช้กับทุกเทคนิค)

```
[32-bit length header (big-endian)] [16-bit CRC-16] [data bits]
```

- **Length**: จำนวน bytes ของ UTF-8 encoded ciphertext string
- **CRC-16**: CCITT-FALSE checksum สำหรับตรวจสอบ integrity
- **Data bits**: แต่ละ byte → 8 bits, MSB first
- ตัวอย่าง: ciphertext = "ABC" (3 bytes) → 32 + 16 + 24 = 72 bits ทั้งหมด

```csharp
bool[] bits = SteganographyEngine.StringToPayloadBits(cipherText);
string result = SteganographyEngine.PayloadBitsToString(extractedBits);
```

### 4.2 Homoglyph Technique

| bit | ตัวอักษรที่ใช้ |
|-----|----------------|
| 0   | Original (ซ้าย) |
| 1   | Glyph (ขวา) |

**คู่ที่รองรับ** (index → (Original, Glyph)):
```
0: ฎ → ฏ   (ChkbxDochada)
1: ข → ฃ   (ChkbxKhoKhai)
2: ช → ซ   (ChkbxChoChang)
```

[M1+M2 FIX] ลบคู่ เ↔แ และ ด↔ต ที่เปลี่ยนความหมาย คงเหลือ 3 คู่
ก่อน embed จะ normalize glyph กลับเป็น original ก่อน เพื่อป้องกัน double-encoding

### 4.3 Misspelling Technique

- **64 คู่คำ** ใน `SteganographyEngine.MisspellingPairs`
- คำ**ถูก** = bit 0, คำ**ผิด** = bit 1
- ค้นหาแบบ greedy left-to-right, non-overlapping (`StringComparison.Ordinal`)
- ลำดับ index ต้องตรงกับ `checkedListBox1.Items` ใน Misspelling.Designer.cs

### 4.4 ZWSP Technique

- ใช้ `U+200B` แทรกระหว่างตัวอักษรใน covertext
- slot i = ช่องระหว่าง char[i] และ char[i+1]: มี ZWSP = bit 1, ไม่มี = bit 0
- ความจุ = `coverText.Length - 1` bits
- **ข้อดี**: ไม่ต้องเลือก options ใดๆ ใช้ได้กับทุก covertext

### 4.5 NBSP Technique

- ใช้ `U+00A0` (Non-Breaking Space) แทนที่ space ปกติ `U+0020` ใน covertext
- space ปกติ = bit 0, NBSP = bit 1
- ความจุ = จำนวน space ใน covertext
- **ข้อดี**: ไม่ต้องเลือก options ใดๆ มองไม่เห็นด้วยตาเปล่า
- **ข้อเสีย**: ความจุต่ำกว่า ZWSP (ขึ้นกับจำนวน space)

### 4.6 Synonym Technique

- **64 กลุ่มคำพ้อง** ใน `SteganographyEngine.SynonymGroups`
- คำ index 0 ในกลุ่ม = bit 0, คำ index 1 = bit 1
- ค้นหาแบบ greedy left-to-right, non-overlapping (`StringComparison.Ordinal`)
- ลำดับ index ต้องตรงกับ `checkedListBox1.Items` ใน `Synonym.Designer.cs`
- ตัวอย่างกลุ่ม: `"กล่าว"` / `"พูด"`, `"ทำ"` / `"กระทำ"`, `"อนุมัติ"` / `"เห็นชอบ"` ฯลฯ
- **ข้อสำคัญ**: คำในแต่ละกลุ่มต้องไม่ซ้ำกับกลุ่มอื่น และไม่ควรเป็นคำสั้นเกินไปที่จะ match เป็น substring ของคำอื่น

---

## 5. AES-256 Encryption

**Algorithm**: AES-256-CBC  
**Key Derivation**: PBKDF2 (`Rfc2898DeriveBytes`), 10,000 iterations  
**Salt**: สุ่มใหม่ 16 bytes ทุกครั้ง (RandomNumberGenerator)
**HMAC**: HMAC-SHA256 Encrypt-then-MAC
**Output**: `Base64( salt[16] + cipherBytes[N] + hmac[32] )`

```csharp
// Salt สุ่มใหม่ทุกครั้ง
byte[] salt = new byte[16];
RandomNumberGenerator.Create().GetBytes(salt);
// Key = PBKDF2(password, salt, 10000).GetBytes(32)
// IV  = ต่อเนื่องจาก Key → .GetBytes(16)
// HmacKey = ต่อเนื่อง → .GetBytes(32)
```

---

## 6. UI Controls Reference

### SteganographyForm (Main)

| Control | Type | แท็บ | หน้าที่ |
|---------|------|------|---------|
| `InputPlaintext` | TextBox | Encryption | รับ plaintext |
| `InputKey` | TextBox | Encryption | รับ password |
| `InputPayload` | TextBox | Encryption | แสดง ciphertext (ReadOnly) |
| `InputCovertext` | TextBox | Encryption | รับ covertext (ใช้ร่วมกับ Stegano tab) |
| `CopyButton1` | Button | Encryption | Copy Key |
| `CopyButton2` | Button | Encryption | Copy Ciphertext |
| `textBox1` | TextBox | Steganography | รับ ciphertext สำหรับฝัง (sync จาก InputPayload) |
| `TbStegano` | TextBox | Steganography | แสดง steganotext (ReadOnly) |
| `BtRs3` | Button | Steganography | Reset Ciphertext + Steganotext |
| `InputChipertext` | TextBox | Decryption | รับ steganotext |
| `InputkeyDecry` | TextBox | Decryption | รับ password |
| `OutputPlaintext` | TextBox | Decryption | แสดง plaintext |
| `CopyButton3` | Button | Decryption | Copy Plaintext |
| `BtRs5` | Button | Decryption | Reset Key |
| `BtRs6` | Button | Decryption | Reset Plaintext output |
| `BtSavePlain` | Button | Decryption | บันทึก Plaintext ลงไฟล์ |
| `DecrytionBotton10` | Button | Decryption | ถอดรหัส AES โดยตรง (ไม่ผ่าน extract) |
| `tsslLabel` | StatusLabel | ทุก tab | แสดงสถานะ + bit capacity |

### Sub-form Properties Pattern

```csharp
var form = new XxxForm
{
    IsEmbedMode      = true/false,
    CipherText       = "...",        // Embed mode
    CoverText        = "...",        // Embed mode
    SteganotextInput = "...",        // Extract mode
};
if (form.ShowDialog() == DialogResult.OK)
    string result = form.ResultText;
```

---

## 7. Features ที่ implement แล้วทั้งหมด

| Feature | สถานะ |
|---------|--------|
| AES-256 Encrypt/Decrypt | ✅ สมบูรณ์ |
| Homoglyph Embed/Extract | ✅ สมบูรณ์ |
| Misspelling Embed/Extract | ✅ สมบูรณ์ |
| ZWSP Embed/Extract | ✅ สมบูรณ์ |
| NBSP Embed/Extract | ✅ สมบูรณ์ |
| Synonym Embed/Extract | ✅ สมบูรณ์ (64 กลุ่มคำพ้อง) |
| Drag & Drop (InputPlaintext + InputChipertext) | ✅ สมบูรณ์ |
| Sync Ciphertext อัตโนมัติ | ✅ สมบูรณ์ |
| Browse file, Copy, Save, Reset ทุกปุ่ม | ✅ สมบูรณ์ |
| Bit Capacity ใน Status Bar | ✅ สมบูรณ์ |

---

## 8. ข้อควรระวังเมื่อพัฒนาต่อ

1. **ห้ามแก้ `.Designer.cs` โดยตรงเมื่อใช้ Visual Studio Designer** — จะถูก overwrite
2. **MisspellingPairs index ต้องตรงกับ checkedListBox1.Items** — ถ้าเพิ่ม/ลบคู่ต้องอัปเดตทั้งสองที่
3. **SynonymGroups index ต้องตรงกับ checkedListBox1.Items ใน Synonym.Designer.cs** — ถ้าเพิ่ม/ลบกลุ่มต้องอัปเดตทั้งสองที่
4. **HomoglyphPairs index ต้องตรงกับ CheckBox ใน OptionStaganogryphy** — 0=ChkbxDochada, 1=ChkbxKhoKhai, 2=ChkbxChoChang
5. **Salt สุ่มใหม่ทุกครั้ง** — ฝังใน ciphertext output แล้ว (AES logic อยู่ใน `CryptoHelper.cs`)
6. **UTF-8 encoding** — ทุกอย่างใช้ UTF-8 ทั้ง plaintext, ciphertext string, และ bit conversion
7. **Covertext อยู่ใน Encryption tab** — `InputCovertext` อยู่ใน TabEncrytion แต่ใช้ร่วมกันผ่าน `GetCovertext()`
8. **Drag & Drop** — ผูกใน `SteganographyForm_Load` ด้วย lambda; ใช้ `LoadFileIntoTextBox()` helper
