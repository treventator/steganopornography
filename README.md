# Thai Text Steganography

ซ่อนข้อความลับในข้อความภาษาไทยที่ดูปกติ โดยผสาน **การเข้ารหัส AES-256** กับ **4 เทคนิค Steganography** ที่ออกแบบมาเฉพาะสำหรับภาษาไทย

```
ข้อความลับ: "เจอกัน 3 ทุ่ม"
                |
                v  AES-256 + Steganography
                |
Steganotext: "ประธานกล่าวว่าที่ประชุมอนุมัติงบประมาณ..."
              (ดูเหมือนรายงานประชุมธรรมดา — ข้อความลับมองไม่เห็น)
```

## แนวคิดหลัก

```
เข้ารหัสอย่างเดียว:      คนรู้ว่ามีความลับ แต่อ่านไม่ออก
ซ่อนข้อมูลอย่างเดียว:    คนไม่รู้ว่ามีความลับ แต่ถ้าเจอก็อ่านออก
ทั้งสองรวมกัน:           คนไม่รู้ว่ามีความลับ + ถึงเจอก็อ่านไม่ออก
```

## ความสามารถ

- **เข้ารหัส AES-256-CBC** พร้อม PBKDF2 (10,000 รอบ), salt สุ่มใหม่ทุกครั้ง, และ HMAC-SHA256 ป้องกันการดัดแปลง
- **4 เทคนิค Steganography** ออกแบบสำหรับภาษาไทยโดยเฉพาะ:

| เทคนิค | วิธีการ | ตาเปล่าสังเกตได้? | ความจุ |
|---|---|---|---|
| **Homoglyph** | สลับตัวอักษรที่หน้าตาคล้ายกัน (ฎ↔ฏ, ข↔ฃ, ช↔ซ) | สังเกตยาก | ปานกลาง |
| **Misspelling** | ใช้คำที่คนไทยสะกดผิดบ่อย (กฎหมาย↔กฏหมาย) | ดูเหมือนพิมพ์ผิดปกติ | ต่ำ |
| **Zero-Width Space** | แทรกอักขระล่องหน U+200B ระหว่างตัวอักษร | มองไม่เห็นเลย | สูงสุด |
| **Synonym** | สลับคำพ้องความหมาย (กล่าว↔พูด, อนุมัติ↔เห็นชอบ) | ความหมายไม่เปลี่ยน | ต่ำ |

- **ลากวางไฟล์** (Drag & Drop) ได้ทั้ง Plaintext และ Steganotext
- **Sync อัตโนมัติ** ระหว่างแท็บ Encryption กับ Steganography
- **แสดงความจุ** (Bit Capacity) ใน Status Bar แบบ real-time
- **CRC-16** ตรวจสอบความถูกต้อง — จับได้ทันทีถ้าเลือกเทคนิค/คู่คำผิด

## วิธีทำงาน

### ซ่อนข้อความ (Encode)

```
[แท็บ 1: Encryption]
  ข้อความลับ + รหัสผ่าน  -->  AES-256-CBC  -->  Ciphertext (Base64)

[แท็บ 2: Steganography]
  Ciphertext + ข้อความปกปิด + เลือกเทคนิค  -->  Steganotext
                                                  (ข้อความที่ดูปกติแต่ซ่อนความลับไว้)
```

### ถอดข้อความ (Decode)

```
[แท็บ 3: Decryption]
  Steganotext + รหัสผ่าน + เทคนิคเดิม  -->  ดึง bits ออกมา
                                        -->  ตรวจ CRC-16
                                        -->  ตรวจ HMAC-SHA256
                                        -->  AES ถอดรหัส
                                        -->  ข้อความลับ
```

### รูปแบบ Payload

```
[32-bit ความยาว (big-endian)] [16-bit CRC-16] [data bits (UTF-8, MSB first)]
```

### รูปแบบ AES Output

```
Base64( salt_สุ่ม[16] + aes_ciphertext[N] + hmac_sha256[32] )
```

## ตัวอย่างการใช้งาน

### ซ่อนคำว่า "Hi" ด้วย ZWSP

```
1. Plaintext = "Hi", Password = "secret"
2. AES Encrypt --> Base64 ciphertext 88 ตัวอักษร
3. Payload = 32 + 16 + 704 = 752 bits
4. Covertext = บทความข่าวภาษาไทย 800+ ตัวอักษร
5. ZWSP Embed --> แทรกอักขระล่องหนระหว่างตัวอักษร
6. ผลลัพธ์: ข้อความเหมือนเดิม 100% แต่ซ่อน 752 bits ไว้
```

### ซ่อน "เจอกัน 3 ทุ่ม" ด้วย Synonym

```
1. Plaintext = "เจอกัน 3 ทุ่ม", Password = "mypass"
2. AES Encrypt --> Base64 ciphertext 128 ตัวอักษร
3. Payload = 32 + 16 + 1024 = 1,072 bits
4. Covertext = รายงานการประชุมสภา (มีคำพ้องเยอะ)
5. Synonym Embed --> "กล่าว"→"พูด", "อนุมัติ"→"เห็นชอบ"
6. ผลลัพธ์: ข้อความอ่านได้ความหมายเหมือนเดิม แต่ซ่อนข้อมูลลับไว้
```

## โครงสร้างโปรเจค

```
Steganography/
  Program.cs                        # จุดเริ่มต้นโปรแกรม
  SteganographyForm.cs              # หน้าหลัก (3 แท็บ: เข้ารหัส / ซ่อน / ถอดรหัส)
  SteganographyForm.Designer.cs     # UI layout
  CryptoHelper.cs                   # AES-256-CBC + PBKDF2 + HMAC-SHA256
  SteganographyEngine.cs            # แกนหลัก: Embed/Extract ทั้ง 4 เทคนิค
  OptionStaganogryphy.cs/.Designer  # ฟอร์มย่อย: Homoglyph (3 คู่ตัวอักษร)
  Misspelling.cs/.Designer          # ฟอร์มย่อย: Misspelling (64 คู่คำ)
  Space.cs/.Designer                # ฟอร์มย่อย: ZWSP (ไม่ต้องเลือก options)
  Synonym.cs/.Designer              # ฟอร์มย่อย: Synonym (64 กลุ่มคำพ้อง)

docs/
  benefits-and-testcases.md         # ประโยชน์, ตัวอย่าง IPO, Test Cases 35+ รายการ
  slide.md                          # Brief สำหรับทำ Slide นำเสนอ
```

## ความต้องการของระบบ

- **Runtime**: .NET Framework 4.8 (Windows)
- **IDE**: Visual Studio 2019 ขึ้นไป (เปิดไฟล์ `Steganography.sln`)
- **Platform**: Windows (แอป WinForms)

## วิธีเริ่มใช้งาน

1. Clone repository นี้
2. เปิด `Steganography.sln` ใน Visual Studio
3. Build แล้ว Run (F5)
4. **แท็บ Encryption**: ใส่ข้อความลับ + รหัสผ่าน + ข้อความปกปิด แล้วกด Encryption
5. **แท็บ Steganography**: เลือกเทคนิค → เลือก options → ได้ Steganotext
6. **แท็บ Decryption**: วาง Steganotext + ใส่รหัสผ่าน → เลือกเทคนิคเดิม + options เดิม

## ความปลอดภัย

โปรเจคนี้จัดทำ **เพื่อการศึกษาเท่านั้น**

| คุณสมบัติ | สถานะ |
|---|---|
| Salt สุ่มใหม่ทุกครั้ง | 16 bytes |
| HMAC-SHA256 (Encrypt-then-MAC) | ตรวจก่อน decrypt, constant-time comparison |
| CRC-16 ตรวจ payload | จับเทคนิค/คู่คำผิดได้ทันที |
| PBKDF2 key derivation | 10,000 รอบ |

**ข้อจำกัดสำหรับงานจริง:**
- PBKDF2 ควรใช้ 100,000+ รอบสำหรับ hardware สมัยใหม่
- Homoglyph/Misspelling ตรวจจับได้ด้วยโปรแกรมวิเคราะห์ข้อความ
- ZWSP ตรวจจับได้ด้วยการสแกนหา U+200B
- ไม่มี forward secrecy หรือระบบหมุนเวียน key

## สิทธิ์การใช้งาน

เพื่อการศึกษาเท่านั้น ผู้พัฒนาไม่รับผิดชอบต่อการนำไปใช้ในทางที่ผิด
