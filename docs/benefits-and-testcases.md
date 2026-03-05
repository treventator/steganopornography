# Thai Text Steganography - Benefits & Test Cases

## 1. Benefits (What is this project for?)

### 1.1 Core Value - One Sentence

> Plaintext secret message is encrypted (AES-256), then hidden inside normal-looking Thai text using 5 steganography techniques. The receiver extracts and decrypts it back.

```
Cryptography alone:  Attacker knows a secret exists, but can't read it
Steganography alone: Attacker doesn't know a secret exists, but if found, can read it
Both combined:       Attacker doesn't know a secret exists + can't read it even if found
```

---

### 1.2 Academic Value (Primary Purpose)

This is an **educational project** that teaches:

| Concept | What students learn |
|---|---|
| AES-256-CBC Encryption | How symmetric encryption works in practice |
| PBKDF2 Key Derivation | Why we don't use passwords directly as keys |
| HMAC-SHA256 | Why encrypt-then-MAC prevents tampering |
| Bit Manipulation | How text becomes bits and bits become hidden data |
| Text Steganography | 5 different ways to hide data in Thai text |
| Security Auditing | How to find and fix vulnerabilities |

**Unique contribution**: Text steganography research for Thai language is very rare. Most existing work focuses on image steganography or English text.

---

### 1.3 Thai Language Characteristics Used

| Thai Language Feature | Technique |
|---|---|
| Visually similar characters (dochada/patuk) | Homoglyph |
| Common misspellings that people accept as normal | Misspelling |
| No spaces between words - invisible chars go unnoticed | ZWSP |
| Spaces between sentences/words look identical with NBSP | NBSP |
| Rich synonyms in formal/government language | Synonym |

---

### 1.4 Real-World Applications (If Developed Further)

**Digital Watermarking**
- Company sends confidential document to 10 employees
- Each copy has different watermark (recipient name hidden inside)
- If document leaks, extract watermark to identify the source

**Covert Communication**
- Send what looks like a news article via LINE/Email
- Eavesdroppers see only a normal article
- Receiver who knows key + technique extracts the secret message

**Copyright Protection**
- Writer embeds signature in their article
- If copied, extract signature to prove ownership

**Anti-Censorship**
- In countries that censor certain keywords
- Hide real message in ZWSP - censorship filters can't detect it
- Visible text is about something else entirely

**Security Awareness Training**
- Teach that normal-looking text can contain hidden data
- Teach how to detect invisible characters (ZWSP)
- Teach that spell-checking can reveal Misspelling steganography

---

### 1.5 Technique Comparison

| Technique | Human detectable? | Machine detectable? | Capacity | Best for |
|---|---|---|---|---|
| ZWSP | Not at all | Scan for U+200B | Highest (chars-1) | Any scenario |
| NBSP | Not at all | Scan for U+00A0 | High (space count) | Text with many spaces |
| Synonym | Almost impossible | Very hard | Low | Meeting reports |
| Misspelling | Looks like typos | Dictionary check | Low | Legal documents |
| Homoglyph | Difficult | Check codepoints | Medium | Formal text with target chars |

---

## 2. Input / Process / Output Examples

### 2.1 System Overview

```
ENCODE:
  Plaintext + Password --[AES-256]--> Ciphertext (Base64)
  Ciphertext --[Payload Encode]--> bits (32-bit len + 16-bit CRC + data)
  bits + Covertext --[Steganography]--> Steganotext

DECODE:
  Steganotext --[Steganography Extract]--> bits
  bits --[Payload Decode + CRC check]--> Ciphertext (Base64)
  Ciphertext + Password --[AES-256 Decrypt + HMAC verify]--> Plaintext
```

**AES Output Format**: `Base64( salt[16] + cipherBytes[N] + hmac[32] )`
**Payload Format**: `[32-bit length][16-bit CRC-16][data bits]`

---

### 2.2 Example 1: Short English Secret - "Hi"

**INPUT**
```
Plaintext : "Hi"
Password  : "secret"
Technique : ZWSP
Covertext : Thai news article (800+ characters)
```

**PROCESS**
```
Step 1 - AES Encrypt:
  "Hi" = [0x48, 0x69] (2 bytes)
  Random salt = [a7 3f b2 ... 16 random bytes]
  PBKDF2("secret", salt, 10000) -> Key(32B) + IV(16B) + HmacKey(32B)
  AES-CBC: 2 bytes + PKCS7 pad -> 16 bytes cipher
  HMAC-SHA256(hmacKey, salt + cipher) -> 32 bytes
  Output: salt[16] + cipher[16] + hmac[32] = 64 bytes
  Base64(64) = 88 characters

Step 2 - Payload Encode:
  88 ASCII bytes -> CRC-16 = 0xA3F1
  [32-bit: 00000000 00000000 00000000 01011000] (length=88)
  [16-bit: 10100011 11110001]                    (CRC=0xA3F1)
  [704 data bits]
  Total = 752 bits to hide

Step 3 - ZWSP Embed:
  Covertext chars: ก ร ุ ง เ ท พ ม ห า น ค ร ...
  Between each pair of characters:
    bit=0 -> nothing inserted
    bit=1 -> insert U+200B (invisible!)
  752 bits hidden in first 753 character gaps
```

**OUTPUT**
```
Steganotext looks 100% identical to covertext.
No visible difference at all.
But hex dump reveals U+200B (E2 80 8B) inserted at ~350 positions.
```

---

### 2.3 Example 2: Thai Secret - "Meeting at 9 PM"

**INPUT**
```
Plaintext : "Meeting at 9 PM" (in Thai)
Password  : "pass2025"
Technique : Synonym
Groups    : 0(speak/say), 10(finish), 20(fix), 27(approve), 28(inspect)
Covertext : Thai meeting report with many synonym target words
```

**PROCESS**
```
Step 1 - AES Encrypt:
  33 UTF-8 bytes -> AES 3 blocks -> 48 bytes cipher
  salt[16] + cipher[48] + hmac[32] = 96 bytes
  Base64(96) = 128 characters

Step 2 - Payload Encode:
  128 bytes -> CRC-16 computed
  Total = 32 + 16 + 1024 = 1072 bits

Step 3 - Synonym Embed:
  Scan covertext left-to-right for synonym group words:

  Found "speak" (group 0) at pos 7:
    bit[N]=0 -> keep "speak"
    bit[N]=1 -> replace with "say"

  Found "do" (group 2, 2 chars) at pos 62:
    [M4 FIX] Thai char follows immediately -> compound word -> SKIP

  Found "approve" (group 27) at pos 90:
    bit[N]=0 -> keep "approve"
    bit[N]=1 -> replace with "agree"
```

**OUTPUT**
```
BEFORE: "Chairman stated that the meeting must consider..."
AFTER:  "Chairman said that the meeting must consider..."
                   ^^^^
                   "stated" -> "said" (bit=1)

Meaning is identical. Reader notices nothing.
```

---

### 2.4 Example 3: Formal Secret - "Approved, Code A-7742"

**INPUT**
```
Plaintext : "Approved, Code A-7742" (in Thai)
Password  : "P@ss2025!"
Technique : Misspelling
Pairs     : 1(law), 3(calculate), 7(budget), 35(appear), 36(deny)
Covertext : Thai legal document with target misspelling words
```

**PROCESS**
```
Step 1 - AES Encrypt:
  52 UTF-8 bytes -> AES 4 blocks -> 64 bytes cipher
  salt[16] + cipher[64] + hmac[32] = 112 bytes
  Base64(112) = 152 characters

Step 2 - Payload Encode:
  Total = 32 + 16 + 1216 = 1264 bits

Step 3 - Misspelling Embed:
  Scan covertext for correct/wrong word pairs:

  Found "law-correct" (pair 1) at pos 25:
    bit=0 -> keep correct spelling
    bit=1 -> replace with common misspelling

  Found "civil-servant" inside "government-official":
    [M3 FIX] substring of longer pair word -> SKIP

  Found "budget-correct" (pair 7) at pos 42:
    bit=1 -> replace with misspelling (swap final consonant)
```

**OUTPUT**
```
BEFORE: "...comply with the law, calculate the budget correctly..."
AFTER:  "...comply with the law (misspelled), calculate the budget (misspelled)..."

Thai readers think: "Just normal typos, people misspell these all the time"
```

---

## 3. Test Cases

### 3.1 Roundtrip Tests (Must Pass)

| # | Plaintext | Password | Technique | Expected |
|---|---|---|---|---|
| RT-1 | "Hi" | "secret" | ZWSP | Decrypt returns "Hi" |
| RT-2 | "Hi" | "secret" | Homoglyph (pairs 0,1,2) | Decrypt returns "Hi" |
| RT-3 | "Hi" | "secret" | Misspelling (pairs 0-9) | Decrypt returns "Hi" |
| RT-4 | "Hi" | "secret" | Synonym (groups 0-9) | Decrypt returns "Hi" |
| RT-4b | "Hi" | "secret" | NBSP | Decrypt returns "Hi" |
| RT-5 | Thai text (33 bytes) | "pass" | ZWSP | Decrypt returns original |
| RT-6 | Thai+English+numbers | "P@ss!" | ZWSP | Decrypt returns original |
| RT-7 | Emoji password | "key" | ZWSP | Decrypt returns original |
| RT-8 | 1-char password "a" | "a" | ZWSP | Decrypt returns original |

### 3.2 Encryption Security Tests

| # | Test | Expected |
|---|---|---|
| ES-1 | Encrypt same plaintext + same password twice | Different ciphertext (random salt) |
| ES-2 | Decrypt with wrong password | Error: "HMAC mismatch" (not padding error) |
| ES-3 | Tamper 1 byte of ciphertext Base64 | Error: "HMAC mismatch" |
| ES-4 | Empty ciphertext to decrypt | Error: "ciphertext too short" |
| ES-5 | Non-Base64 string to decrypt | Error: "not Base64" |

### 3.3 Payload Integrity Tests (CRC-16)

| # | Test | Expected |
|---|---|---|
| PI-1 | Extract with wrong technique | Error: "CRC mismatch" |
| PI-2 | Extract with wrong pair/group selection | Error: "CRC mismatch" |
| PI-3 | Modify 1 character in steganotext then extract | Error: "CRC mismatch" |
| PI-4 | Steganotext too short (< 48 bits extractable) | Error: "insufficient data" |

### 3.4 Capacity Tests

| # | Test | Expected |
|---|---|---|
| CA-1 | ZWSP: covertext 100 chars, need 752 bits | Error: "covertext too short (need 753 chars)" |
| CA-1b | NBSP: covertext with 100 spaces, need 752 bits | Error: "covertext มี space ไม่เพียงพอ" |
| CA-2 | ZWSP: covertext 753 chars, need 752 bits | Success (just enough) |
| CA-3 | Homoglyph: covertext has 5 target chars, need 752 | Error: "insufficient Homoglyph chars (need 752, got 5)" |
| CA-4 | Misspelling: covertext has 3 target words, need 752 | Error: "insufficient Misspelling words" |
| CA-5 | Synonym: covertext has 0 target words | Error: "insufficient Synonym words" |

### 3.5 Bug Fix Verification Tests

| # | Fix | Test | Expected |
|---|---|---|---|
| BF-1 | M3: Misspelling substring | "civil-servant" in text, only short pair active | Short pair SKIPPED (not matched inside longer word) |
| BF-2 | M3: Misspelling substring | Both long+short pairs active | Long pair matches first (correct behavior) |
| BF-3 | M4: Synonym short word | "do" in "doing" (compound word) | "do" SKIPPED (Thai char follows) |
| BF-4 | M4: Synonym short word | "do" after space "...then do" | "do" MATCHED (space boundary) |
| BF-5 | M1+M2: Removed pairs | Homoglyph UI shows 3 pairs only | No sara-e/dodek checkboxes |
| BF-6 | C1: Random salt | Encrypt same input twice | Different output each time |
| BF-7 | C2: HMAC | Tamper ciphertext before decrypt | "HMAC mismatch" (caught BEFORE AES decrypt) |
| BF-8 | H1: CRC-16 | Extract with wrong options | "CRC mismatch" (caught BEFORE AES decrypt) |

### 3.6 Edge Cases

| # | Test | Expected |
|---|---|---|
| EC-1 | Plaintext = "" (empty) | UI validation: "Please enter plaintext" |
| EC-2 | Password = "" (empty) | UI validation: "Please enter key" |
| EC-3 | Covertext = "" (empty) | UI validation: "Please enter covertext" |
| EC-4 | Very long plaintext (10,000 chars) + adequate covertext | Success (if covertext large enough) |
| EC-5 | File > 5MB via drag & drop | Warning: "File too large" |
| EC-6 | Steganotext copy-pasted through app that strips ZWSP | Error on extract (bits lost) |
| EC-7 | Newlines in plaintext | Roundtrip preserves newlines |

---

## 4. Numeric Summary

### Minimum bits required (after security fixes)

```
Formula:
  aes_blocks    = ceil(plaintext_bytes / 16)
  cipher_bytes  = aes_blocks * 16
  total_bytes   = 16 (salt) + cipher_bytes + 32 (hmac)
  base64_chars  = ceil(total_bytes / 3) * 4
  payload_bits  = 32 (length) + 16 (CRC) + base64_chars * 8
```

| Plaintext | Plaintext bytes | AES output | Base64 chars | Bits to hide |
|---|---|---|---|---|
| "Hi" | 2 | 64 | 88 | 752 |
| "OK" | 2 | 64 | 88 | 752 |
| Short Thai (13 chars) | 33 | 96 | 128 | 1,072 |
| Medium Thai (20 chars) | 52 | 112 | 152 | 1,264 |
| Long Thai (100 chars) | ~250 | 304 | 408 | 3,312 |

### Covertext requirements per technique

| Technique | Covertext needed for 752 bits | For 1,072 bits |
|---|---|---|
| ZWSP | 753 characters (~3 sentences) | 1,073 characters (~1 paragraph) |
| NBSP | 752 spaces (~5-10 paragraphs) | 1,072 spaces (~10-15 paragraphs) |
| Homoglyph (3 pairs) | ~5,000+ chars (depends on frequency) | ~7,000+ chars |
| Misspelling | 752 target word occurrences (very long document) | 1,072 occurrences |
| Synonym | 752 target word occurrences (very long document) | 1,072 occurrences |

### Error detection layers

```
Layer 1: UI Validation      -> empty fields, missing covertext
Layer 2: Capacity Check     -> covertext too short (before embedding)
Layer 3: CRC-16             -> wrong technique/options (before AES)
Layer 4: HMAC-SHA256        -> wrong key / tampered data (before AES)
Layer 5: AES-CBC Padding    -> corrupted cipher (last resort)
```
