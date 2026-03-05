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
├── OptionStaganogryphy.cs        # Sub-form: Homoglyph (ฎ/ฏ, เ/แ, ด/ต, ข/ฃ, ช/ซ)
├── OptionStaganogryphy.Designer.cs
├── Misspelling.cs                # Sub-form: Misspelling (64 คู่คำภาษาไทย)
├── Misspelling.Designer.cs
├── Space.cs                      # Sub-form: Zero-Width Space (U+200B)
├── Space.Designer.cs
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
    → AesEncrypt(plaintext, key)           [salt คงที่ "Stegano2025Educa"]
    → Base64 Ciphertext → InputPayload + sync ไปที่ textBox1 อัตโนมัติ

[Tab: Steganography]
  textBox1 (Ciphertext) + InputCovertext (Covertext จาก Encryption tab)
    → เลือกเทคนิค (BtHm / BtMs / BtSp)
    → เปิด sub-form → ผู้ใช้เลือก options → กด ตกลง
    → SteganographyEngine.Xxx Embed(coverText, cipherText, options)
    → TbStegano (Steganotext)  → Save / Copy
```

### 3.2 Decode Flow (ถอดข้อความ)

```
[Tab: Decryption]
  InputChipertext (Steganotext) + InputkeyDecry (Key)
    → เลือกเทคนิค (BtHmD / BtMsD / BtSpD)
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
[32-bit length header (big-endian)] [data bits]
```

- **Length**: จำนวน bytes ของ UTF-8 encoded ciphertext string  
- **Data bits**: แต่ละ byte → 8 bits, MSB first  
- ตัวอย่าง: ciphertext = "ABC" (3 bytes) → 32 + 24 = 56 bits ทั้งหมด  

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
1: เ → แ   (ChkbxSaraE)
2: ด → ต   (ChkbxDodek)
3: ข → ฃ   (ChkbxKhoKhai)
4: ช → ซ   (ChkbxChoChang)
```

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

### 4.5 Synonym Technique

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
**Salt**: คงที่ 16 bytes = `"Stegano2025Educa"`

```csharp
private static readonly byte[] AesSalt = {
    0x53,0x74,0x65,0x67,0x61,0x6E,0x6F,0x32,
    0x30,0x32,0x35,0x45,0x64,0x75,0x63,0x61
};
// Key = PBKDF2(password, AesSalt, 10000).GetBytes(32)
// IV  = ต่อเนื่องจาก Key → .GetBytes(16)
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
4. **HomoglyphPairs index ต้องตรงกับ CheckBox ใน OptionStaganogryphy** — 0=ChkbxDochada, 1=ChkbxSaraE, ฯลฯ
5. **Salt คงที่** — สำหรับงานจริงควรสุ่ม salt และฝังใน ciphertext (AES logic อยู่ใน `CryptoHelper.cs`)
6. **UTF-8 encoding** — ทุกอย่างใช้ UTF-8 ทั้ง plaintext, ciphertext string, และ bit conversion
7. **Covertext อยู่ใน Encryption tab** — `InputCovertext` อยู่ใน TabEncrytion แต่ใช้ร่วมกันผ่าน `GetCovertext()`
8. **Drag & Drop** — ผูกใน `SteganographyForm_Load` ด้วย lambda; ใช้ `LoadFileIntoTextBox()` helper
