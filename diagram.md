# Steganography System — Diagrams

## 1. Overall System Flow

```mermaid
flowchart TD
    subgraph Encryption["แท็บ 1: Encryption"]
        A[/"Plaintext<br/>(ข้อความลับ)"/] --> B["AES-256-CBC Encrypt<br/>PBKDF2 + Salt คงที่"]
        K1[/"Key (รหัสผ่าน)"/] --> B
        B --> C[/"Ciphertext (Base64)"/]
        CV[/"Covertext<br/>(ข้อความปกปิดภาษาไทย)"/]
    end

    subgraph Steganography["แท็บ 2: Steganography"]
        C -- sync อัตโนมัติ --> D{"เลือกเทคนิค"}
        CV --> D
        D -->|Homoglyph| E1["สลับตัวอักษร<br/>ฎ↔ฏ ข↔ฃ ช↔ซ"]
        D -->|Misspelling| E2["สลับคำถูก/ผิด<br/>65 คู่คำ"]
        D -->|ZWSP| E3["แทรก U+200B<br/>ระหว่างตัวอักษร"]
        D -->|NBSP| E4["แทนที่ Space<br/>ด้วย U+00A0"]
        D -->|Synonym| E5["สลับคำพ้อง<br/>64 กลุ่ม"]
        E1 & E2 & E3 & E4 & E5 --> F[/"Steganotext<br/>(ข้อความที่ซ่อนข้อมูลแล้ว)"/]
    end

    subgraph Decryption["แท็บ 3: Decryption"]
        F --> G{"เลือกเทคนิคเดิม"}
        G --> H["Extract bits<br/>จาก Steganotext"]
        H --> I[/"Ciphertext (Base64)"/]
        I --> J["AES-256-CBC Decrypt"]
        K2[/"Key เดิม"/] --> J
        J --> L[/"Plaintext<br/>(ข้อความลับเดิม)"/]
    end

    style Encryption fill:#e8f5e9,stroke:#2e7d32
    style Steganography fill:#e3f2fd,stroke:#1565c0
    style Decryption fill:#fff3e0,stroke:#e65100
```

---

## 2. Payload Format (ใช้ร่วมกันทุกเทคนิค)

```mermaid
block-beta
    columns 8
    block:header:4
        h1["Bit 0"] h2["Bit 1"] h3["..."] h4["Bit 31"]
    end
    block:data:4
        d1["Bit 32"] d2["Bit 33"] d3["..."] d4["Bit N"]
    end
    header --> data

    style header fill:#ffcdd2,stroke:#c62828
    style data fill:#c8e6c9,stroke:#2e7d32
```

```mermaid
flowchart LR
    A["Ciphertext<br/>(Base64 string)"] -->|"UTF-8 Encode"| B["byte[]<br/>[0x41, 0x42, ...]"]
    B -->|"แต่ละ byte → 8 bits<br/>MSB first"| C["Data Bits"]
    D["จำนวน bytes"] -->|"เขียนเป็น 32-bit<br/>big-endian"| E["Length Header<br/>(32 bits)"]
    E --> F["Payload Bits"]
    C --> F
    F -->|"ฝังลง covertext<br/>ด้วยเทคนิคที่เลือก"| G["Steganotext"]

    style E fill:#ffcdd2
    style C fill:#c8e6c9
    style F fill:#fff9c4
```

**ตัวอย่าง:** Ciphertext = `"AB"` (2 bytes)

| ส่วน | Bits | จำนวน |
|------|------|-------|
| Header | `00000000 00000000 00000000 00000010` | 32 bits |
| Data 'A' (0x41) | `01000001` | 8 bits |
| Data 'B' (0x42) | `01000010` | 8 bits |
| **รวม** | | **48 bits** |

---

## 3. Homoglyph Technique

```mermaid
flowchart TD
    subgraph Embed
        A["Covertext ภาษาไทย"] --> B["Normalize<br/>(แปลง glyph กลับเป็น original)"]
        B --> C["สแกนทีละตัวอักษร"]
        C --> D{"ตัวอักษรนี้<br/>อยู่ในคู่ที่เลือก?"}
        D -->|ไม่ใช่| E["ข้ามไป"]
        D -->|ใช่| F{"bit = ?"}
        F -->|"0"| G["คงเป็น Original<br/>เช่น ฎ"]
        F -->|"1"| H["แทนด้วย Glyph<br/>เช่น ฏ"]
        E & G & H --> I["ตัวอักษรถัดไป"]
        I --> C
    end

    subgraph "คู่ตัวอักษร (3 คู่)"
        P0["ฎ ↔ ฏ"]
        P1["ข ↔ ฃ"]
        P2["ช ↔ ซ"]
    end

    style Embed fill:#f3e5f5,stroke:#6a1b9a
```

```mermaid
flowchart LR
    subgraph "ตัวอย่าง: ฝัง bits [1, 0, 1]"
        direction LR
        T1["ก"] --> T2["ฎ → <b>ฏ</b><br/>(bit 1)"]
        T2 --> T3["ร"]
        T3 --> T4["ฎ → <b>ฎ</b><br/>(bit 0)"]
        T4 --> T5["า"]
        T5 --> T6["ช → <b>ซ</b><br/>(bit 1)"]
    end

    style T2 fill:#ffcdd2
    style T4 fill:#c8e6c9
    style T6 fill:#ffcdd2
```

**ความจุ:** จำนวนตัวอักษร ฎ, ข, ช (+ glyph) ที่ปรากฏใน covertext

---

## 4. Misspelling Technique

```mermaid
flowchart TD
    subgraph Embed
        A["Covertext ภาษาไทย"] --> B["Greedy Search<br/>(ซ้ายไปขวา, ไม่ซ้อนทับ)"]
        B --> C["พบคำที่อยู่ในคู่ที่เลือก"]
        C --> D{"bit = ?"}
        D -->|"0"| E["ใช้คำสะกด<b>ถูก</b><br/>เช่น สังเกต"]
        D -->|"1"| F["ใช้คำสะกด<b>ผิด</b><br/>เช่น สังเกตุ"]
        E & F --> G["เลื่อนหาคำถัดไป<br/>(offset tracking)"]
        G --> B
    end

    style Embed fill:#e8eaf6,stroke:#283593
```

```mermaid
flowchart LR
    subgraph "ตัวอย่าง: ฝัง bits [0, 1, 0]"
        direction LR
        W1["...สังเกต..."] -->|"bit 0 → คำถูก"| R1["...สังเกต..."]
        W2["...กฎหมาย..."] -->|"bit 1 → คำผิด"| R2["...กฏหมาย..."]
        W3["...อนุญาต..."] -->|"bit 0 → คำถูก"| R3["...อนุญาต..."]
    end

    style R1 fill:#c8e6c9
    style R2 fill:#ffcdd2
    style R3 fill:#c8e6c9
```

**ความจุ:** จำนวนคำจากคู่ที่เลือกที่ปรากฏใน covertext (65 คู่คำ)

---

## 5. ZWSP (Zero-Width Space) Technique

```mermaid
flowchart TD
    subgraph Embed
        A["Covertext"] --> B["Normalize<br/>(ลบ ZWSP/NBSP เดิม)"]
        B --> C["วนทีละตัวอักษร"]
        C --> D["เพิ่มตัวอักษรลง result"]
        D --> E{"ยังมี bit<br/>ที่ต้องฝัง?"}
        E -->|"bit = 1"| F["แทรก U+200B<br/>(มองไม่เห็น)"]
        E -->|"bit = 0"| G["ไม่แทรกอะไร"]
        E -->|"หมดแล้ว"| G
        F & G --> H["ตัวอักษรถัดไป"]
        H --> C
    end

    style Embed fill:#e0f7fa,stroke:#006064
```

```mermaid
flowchart LR
    subgraph "ตัวอย่าง: ฝัง bits [1, 0, 1] ลงใน 'สวัสดี'"
        direction LR
        C1["ส"] -->|"bit 1"| Z1["<b>​</b><br/>(ZWSP)"]
        Z1 --> C2["ว"]
        C2 -->|"bit 0"| N1["(ไม่มี)"]
        N1 --> C3["ั"]
        C3 -->|"bit 1"| Z2["<b>​</b><br/>(ZWSP)"]
        Z2 --> C4["ส"]
        C4 --> C5["ด"]
        C5 --> C6["ี"]
    end

    style Z1 fill:#ffcdd2,stroke:#c62828
    style N1 fill:#c8e6c9,stroke:#2e7d32
    style Z2 fill:#ffcdd2,stroke:#c62828
```

**ความจุ:** `covertext.Length - 1` bits (ช่องระหว่างตัวอักษรทุกคู่)

---

## 6. NBSP (Non-Breaking Space) Technique

```mermaid
flowchart TD
    subgraph Embed
        A["Covertext"] --> B["Normalize<br/>(ลบ ZWSP, แปลง NBSP → space)"]
        B --> C["วนทีละตัวอักษร"]
        C --> D{"ตัวนี้เป็น<br/>space ปกติ?"}
        D -->|ไม่ใช่| E["เพิ่มตามเดิม"]
        D -->|ใช่| F{"bit = ?"}
        F -->|"0"| G["คง Space ปกติ<br/>(U+0020)"]
        F -->|"1"| H["แทนด้วย NBSP<br/>(U+00A0)"]
        E & G & H --> I["ตัวอักษรถัดไป"]
        I --> C
    end

    style Embed fill:#fce4ec,stroke:#880e4f
```

```mermaid
flowchart LR
    subgraph "ตัวอย่าง: ฝัง bits [1, 0] ลงใน 'สวัสดี ครับ ผม'"
        direction LR
        W1["สวัสดี"] -->|"bit 1"| S1["<b>NBSP</b><br/>(U+00A0)"]
        S1 --> W2["ครับ"]
        W2 -->|"bit 0"| S2["Space<br/>(U+0020)"]
        S2 --> W3["ผม"]
    end

    style S1 fill:#ffcdd2,stroke:#c62828
    style S2 fill:#c8e6c9,stroke:#2e7d32
```

**ความจุ:** จำนวน space ที่มีใน covertext

---

## 7. Synonym Technique

```mermaid
flowchart TD
    subgraph Embed
        A["Covertext ภาษาไทย"] --> B["Greedy Search<br/>(ซ้ายไปขวา, ไม่ซ้อนทับ)"]
        B --> C["พบคำที่อยู่ในกลุ่มที่เลือก"]
        C --> D{"bit = ?"}
        D -->|"0"| E["ใช้คำที่ 1 ในกลุ่ม<br/>เช่น กล่าว"]
        D -->|"1"| F["ใช้คำที่ 2 ในกลุ่ม<br/>เช่น พูด"]
        E & F --> G["เลื่อนหาคำถัดไป<br/>(offset tracking)"]
        G --> B
    end

    style Embed fill:#f1f8e9,stroke:#33691e
```

```mermaid
flowchart LR
    subgraph "ตัวอย่าง: ฝัง bits [1, 0, 1]"
        direction LR
        W1["...กล่าว..."] -->|"bit 1 → คำที่ 2"| R1["...<b>พูด</b>..."]
        W2["...ตรวจสอบ..."] -->|"bit 0 → คำที่ 1"| R2["...<b>ตรวจสอบ</b>..."]
        W3["...สำเร็จ..."] -->|"bit 1 → คำที่ 2"| R3["...<b>บรรลุผล</b>..."]
    end

    style R1 fill:#ffcdd2
    style R2 fill:#c8e6c9
    style R3 fill:#ffcdd2
```

**ความจุ:** จำนวนคำพ้องจากกลุ่มที่เลือกที่ปรากฏใน covertext (64 กลุ่ม)

---

## 8. AES-256-CBC Encryption Flow

```mermaid
flowchart LR
    subgraph "Key Derivation (PBKDF2)"
        P["Password"] --> KDF["Rfc2898DeriveBytes<br/>10,000 iterations"]
        S["Salt คงที่<br/>'Stegano2025Educa'"] --> KDF
        KDF -->|"32 bytes"| KEY["AES Key<br/>(256 bits)"]
        KDF -->|"16 bytes"| IV["IV<br/>(128 bits)"]
    end

    subgraph "Encrypt"
        PT["Plaintext"] -->|"UTF-8"| BYTES["byte[]"]
        BYTES --> AES["AES-256-CBC<br/>+ PKCS7 padding"]
        KEY --> AES
        IV --> AES
        AES --> CT["Ciphertext bytes"]
        CT -->|"Base64 Encode"| B64["Base64 String"]
    end

    subgraph "Decrypt"
        B64D["Base64 String"] -->|"Base64 Decode"| CTD["Ciphertext bytes"]
        CTD --> AESD["AES-256-CBC<br/>Decrypt"]
        KEY -.-> AESD
        IV -.-> AESD
        AESD -->|"UTF-8"| PTD["Plaintext"]
    end

    style KEY fill:#fff9c4,stroke:#f57f17
    style IV fill:#fff9c4,stroke:#f57f17
```

---

## 9. เปรียบเทียบ 5 เทคนิค

```mermaid
quadrantChart
    title เปรียบเทียบ Steganography 5 เทคนิค
    x-axis "ความจุต่ำ" --> "ความจุสูง"
    y-axis "สังเกตง่าย" --> "สังเกตยาก"
    quadrant-1 "ดี: จุเยอะ+ซ่อนดี"
    quadrant-2 "ซ่อนดีแต่จุน้อย"
    quadrant-3 "ไม่ค่อยดี"
    quadrant-4 "จุเยอะแต่เสี่ยง"
    ZWSP: [0.85, 0.80]
    NBSP: [0.35, 0.75]
    Homoglyph: [0.30, 0.65]
    Synonym: [0.40, 0.55]
    Misspelling: [0.45, 0.30]
```

| เทคนิค | ซ่อน bit ด้วย | ความจุ | จุดแข็ง | จุดอ่อน |
|--------|---------------|--------|---------|---------|
| **Homoglyph** | ตัวอักษรคล้าย (ฎ↔ฏ) | ขึ้นกับตัวอักษรในคู่ | ตาเปล่าแยกไม่ออก | ความจุต่ำ ขึ้นกับ covertext |
| **Misspelling** | คำถูก/ผิด (65 คู่) | ขึ้นกับคำในคู่ | เป็นธรรมชาติ (คนสะกดผิดบ่อย) | ผู้อ่านอาจสังเกตคำผิด |
| **ZWSP** | แทรก U+200B | `length - 1` | ความจุสูงมาก มองไม่เห็น | บาง editor ตัด ZWSP ออก |
| **NBSP** | แทนที่ space ด้วย U+00A0 | จำนวน space | ไม่เปลี่ยนจำนวนอักขระ | ความจุขึ้นกับจำนวน space |
| **Synonym** | คำพ้อง (64 กลุ่ม) | ขึ้นกับคำพ้องในกลุ่ม | ข้อความอ่านเป็นธรรมชาติที่สุด | ความจุต่ำ ขึ้นกับ covertext |

---

## 10. Class Diagram

```mermaid
classDiagram
    class SteganographyEngine {
        <<static>>
        +HomoglyphPairs : (char, char)[]
        +MisspellingPairs : (string, string)[]
        +SynonymGroups : string[][]
        +HomoglyphEmbed(coverText, cipherText, activePairs) string
        +HomoglyphExtract(steganotext, activePairs) string
        +MisspellingEmbed(coverText, cipherText, indices) string
        +MisspellingExtract(steganotext, indices) string
        +ZWSPEmbed(coverText, cipherText) string
        +ZWSPExtract(steganotext) string
        +NBSPEmbed(coverText, cipherText) string
        +NBSPExtract(steganotext) string
        +SynonymEmbed(coverText, cipherText, indices) string
        +SynonymExtract(steganotext, indices) string
        +StringToPayloadBits(text) bool[]
        +PayloadBitsToString(bits) string
        -NormalizeCovertext(coverText) string
        -FindMisspellingCandidates(text, indices) List
        -FindSynonymCandidates(text, indices) List
    }

    class CryptoHelper {
        <<static>>
        -AesSalt : byte[]
        -Pbkdf2Iterations : int
        +AesEncrypt(plaintext, password) string
        +AesDecrypt(cipherBase64, password) string
    }

    class SteganographyForm {
        -MaxFileSizeBytes : long
        -tt : ToolTip
        +SteganographyForm()
        -ValidateSteganographyInput() bool
        -ValidateDecryptionInput() bool
        -TriggerDecryption(cipherBase64)
        -UpdateSteganoBitCapacity()
        -GetCovertext() string
        -LoadFileIntoTextBox(path, target, label)
    }

    class OptionStaganography {
        +IsEmbedMode : bool
        +CipherText : string
        +CoverText : string
        +SteganotextInput : string
        +ResultText : string
        +ExecuteHomoglyph()
        +GetActivePairIndices() List~int~
    }

    class Misspelling {
        +IsEmbedMode : bool
        +CipherText : string
        +CoverText : string
        +SteganotextInput : string
        +ResultText : string
        +ExecuteMisspelling()
        +GetActivePairIndices() List~int~
    }

    class ChkbxZWSP {
        +IsEmbedMode : bool
        +CipherText : string
        +CoverText : string
        +SteganotextInput : string
        +ResultText : string
    }

    class NbspForm {
        +IsEmbedMode : bool
        +CipherText : string
        +CoverText : string
        +SteganotextInput : string
        +ResultText : string
    }

    class Synonym {
        +IsEmbedMode : bool
        +CipherText : string
        +CoverText : string
        +SteganotextInput : string
        +ResultText : string
        +ExecuteSynonym()
        +GetActiveGroupIndices() List~int~
    }

    SteganographyForm --> CryptoHelper : เข้ารหัส/ถอดรหัส
    SteganographyForm --> OptionStaganography : ShowDialog()
    SteganographyForm --> Misspelling : ShowDialog()
    SteganographyForm --> ChkbxZWSP : ShowDialog()
    SteganographyForm --> NbspForm : ShowDialog()
    SteganographyForm --> Synonym : ShowDialog()
    OptionStaganography --> SteganographyEngine : Embed/Extract
    Misspelling --> SteganographyEngine : Embed/Extract
    ChkbxZWSP --> SteganographyEngine : Embed/Extract
    NbspForm --> SteganographyEngine : Embed/Extract
    Synonym --> SteganographyEngine : Embed/Extract
```

---

## 11. Greedy Search Algorithm (Misspelling & Synonym)

```mermaid
flowchart TD
    START["เริ่ม: searchFrom = 0"] --> LOOP{"searchFrom < text.Length?"}
    LOOP -->|ไม่| DONE["ส่งคืนผลลัพธ์"]
    LOOP -->|ใช่| SCAN["วนทุกคู่/กลุ่มที่เลือก<br/>ค้นหาคำถูก+คำผิด ตั้งแต่ searchFrom"]
    SCAN --> BEST{"พบคำไหม?"}
    BEST -->|ไม่พบ| DONE
    BEST -->|พบ| SELECT["เลือกคำที่:<br/>1. ตำแหน่งน้อยที่สุด<br/>2. ถ้าตำแหน่งเท่ากัน → ยาวที่สุด"]
    SELECT --> RECORD["บันทึก (ตำแหน่ง, คู่, ถูก/ผิด, คำ)"]
    RECORD --> ADVANCE["searchFrom = ตำแหน่ง + ความยาวคำ<br/>(non-overlapping)"]
    ADVANCE --> LOOP

    style SELECT fill:#fff9c4,stroke:#f57f17
    style ADVANCE fill:#e8eaf6,stroke:#283593
```
