# Slide Deck Brief for AI Agent (Infographic / Presentation Generator)

## Instructions for AI Agent

You are creating a presentation slide deck (10-12 slides) about a **Thai Text Steganography** project. This is a university-level CS/security project to present to professors and general audience.

**Style**: Clean infographic style. Use diagrams, flow arrows, before/after comparisons, and tables. Minimal text per slide. Thai language for content, English for technical terms.

**Audience**: University professors (computer engineering) + general audience (non-technical)

**Tone**: Professional, educational, easy to understand

---

## Slide 1: Title

**Title**: Thai Text Steganography
**Subtitle**: Hiding Secret Messages in Thai Text using AES-256 + 4 Steganography Techniques
**Tagline**: "Text that looks normal... but carries a secret"

**Visual**: Split image - left side shows normal Thai text, right side shows the same text with hidden bits highlighted/glowing

---

## Slide 2: The Problem

**Title**: Why Steganography?

**Key message**: Encryption alone tells attackers a secret exists. Steganography hides the fact that there IS a secret.

**Visual**: 3-column comparison

| | Encryption Only | Steganography Only | Both Combined |
|---|---|---|---|
| Attacker sees | "aX9kL2m..." (gibberish) | Normal text (readable) | Normal text (readable) |
| Attacker knows secret exists? | YES | NO | NO |
| If found, can read? | NO | YES | NO |

**Bottom line**: This project combines BOTH for maximum security

---

## Slide 3: System Overview (Architecture)

**Title**: How It Works - 3 Steps

**Visual**: Horizontal flow diagram with 3 big blocks

```
[Tab 1: Encryption]          [Tab 2: Steganography]       [Tab 3: Decryption]

 Plaintext + Password         Ciphertext + Covertext       Steganotext + Password
       |                            |                            |
       v                            v                            v
   AES-256-CBC               Choose technique              Extract bits
       |                     (4 options)                        |
       v                            |                            v
   Ciphertext (Base64)              v                       Ciphertext
       |                      Steganotext                       |
       +--- auto sync --->    (looks normal!)                   v
                                                           AES Decrypt
                                                               |
                                                               v
                                                           Plaintext
```

---

## Slide 4: The 4 Techniques - Overview

**Title**: 4 Ways to Hide Data in Thai Text

**Visual**: 4 cards/panels, each with icon + name + one-line description + visual example

**Card 1 - Homoglyph**
- Swap visually similar Thai characters
- Example: swap characters that look almost identical (show two Thai chars side by side)
- Capacity: Medium

**Card 2 - Misspelling**
- Use common Thai misspellings
- Example: correct spelling = bit 0, common misspelling = bit 1
- Capacity: Low

**Card 3 - Zero-Width Space (ZWSP)**
- Insert invisible Unicode characters between letters
- Example: text looks 100% identical, but invisible chars carry data
- Capacity: HIGH (best)

**Card 4 - Synonym**
- Swap words with same meaning
- Example: "stated" vs "said", "approved" vs "agreed"
- Capacity: Low (but hardest to detect)

---

## Slide 5: How Bits Are Hidden (Technical Detail)

**Title**: From Secret Message to Hidden Bits

**Visual**: Step-by-step transformation diagram

```
"Hi"                          <- Secret message
  |
  v
[0x48, 0x69]                  <- UTF-8 bytes
  |
  v  AES-256-CBC (password + random salt + HMAC)
  |
"p38/shk...Zz0="              <- Ciphertext (88 chars)
  |
  v  Payload encoding
  |
[00000000...01011000]          <- 32-bit length header (88)
[10100011 11110001]            <- 16-bit CRC-16 (integrity check)
[01110000 00110011...]         <- 704 data bits
  |
  = 752 bits total to hide
  |
  v  Hide in covertext using chosen technique
  |
"Normal Thai text..."          <- Steganotext (looks unchanged!)
```

---

## Slide 6: ZWSP Deep Dive (Best Technique)

**Title**: Zero-Width Space - The Invisible Technique

**Visual**: Before/After comparison with hex view

**What you see:**
```
BEFORE: "Hello in Thai"
AFTER:  "Hello in Thai"      <- Looks 100% identical!
```

**What's actually there (hex view):**
```
BEFORE: ...E0B881 E0B8A3 E0B8B8 E0B887...
AFTER:  ...E0B881 E2808B E0B8A3 E0B8B8 E0B887...
                  ^^^^^^
                  ZWSP (invisible character!)
                  This is bit = 1
```

**Stats box:**
- Capacity: covertext characters - 1
- Detection: Scan for U+200B
- Strength: Completely invisible to human eyes

---

## Slide 7: Practical Example (End-to-End)

**Title**: Real Scenario - Sending a Secret Meeting Time

**Visual**: Chat/messaging app mockup

**Sender does:**
1. Types secret: "Meet at 9 PM"
2. Types password: "mykey"
3. Pastes a news article as covertext
4. Clicks "ZWSP" technique
5. Gets steganotext (looks like normal news)
6. Sends via LINE/Email

**What others see:**
```
[Chat bubble]: "Bangkok is the capital of Thailand. It is the
center of economy, politics and culture of Southeast Asia.
With over 10 million residents..."

Observer thinks: "Just sharing a news article"
```

**Receiver does:**
1. Pastes received text
2. Types same password: "mykey"
3. Clicks "ZWSP" technique
4. Gets: "Meet at 9 PM"

---

## Slide 8: Security Architecture

**Title**: Multi-Layer Security

**Visual**: Concentric circles or stacked layers diagram

```
Layer 5 (outermost): Steganography - hides that a secret exists
Layer 4: CRC-16 integrity check - catches wrong technique/options
Layer 3: HMAC-SHA256 - catches wrong key / data tampering
Layer 2: AES-256-CBC encryption - makes data unreadable
Layer 1 (core): Plaintext secret message
```

**Key specs table:**
| Component | Detail |
|---|---|
| Encryption | AES-256-CBC |
| Key Derivation | PBKDF2, 10,000 iterations |
| Salt | Random 16 bytes (new each time) |
| Authentication | HMAC-SHA256 (Encrypt-then-MAC) |
| Integrity | CRC-16/CCITT in payload header |
| Encoding | UTF-8 throughout |

---

## Slide 9: Security Audit Results

**Title**: Found & Fixed - Security Audit

**Visual**: Before/After table with severity colors

| Severity | Issue | Fix |
|---|---|---|
| CRITICAL | Fixed AES salt (same password = same output) | Random 16-byte salt per encryption |
| CRITICAL | No HMAC (vulnerable to bit-flipping) | Added HMAC-SHA256, verify before decrypt |
| HIGH | No payload integrity check | Added CRC-16 in payload header |
| HIGH | Max payload 1MB (DoS risk) | Reduced to 100KB |
| MEDIUM | Homoglyph pairs change word meaning | Removed problematic pairs (kept 3 safe) |
| MEDIUM | Misspelling substring overlap | Added longer-word containment check |
| MEDIUM | Synonym short words match inside compounds | Added Thai word boundary heuristic |

---

## Slide 10: Capacity Comparison

**Title**: How Much Text Do You Need?

**Visual**: Bar chart or comparison infographic

**To hide "Hi" (752 bits):**

| Technique | Covertext needed | Visual scale |
|---|---|---|
| ZWSP | ~3 sentences | [====] |
| Homoglyph | ~2 pages | [========================] |
| Misspelling | ~50+ pages | [==========================================...] |
| Synonym | ~50+ pages | [==========================================...] |

**Bottom line**: ZWSP is the most practical for any message length. Other techniques are best for very short secrets in very long documents.

---

## Slide 11: Test Results

**Title**: Verification & Test Cases

**Visual**: Test matrix with pass/fail indicators

**Roundtrip Tests (all 4 techniques):**
- Encrypt -> Embed -> Extract -> Decrypt = original message (PASS)
- Thai + English + numbers + symbols (PASS)
- Any password length/language (PASS)

**Security Tests:**
- Same plaintext + same password -> different ciphertext each time (PASS - random salt)
- Wrong password -> "HMAC mismatch" (PASS - caught before AES)
- Wrong technique -> "CRC mismatch" (PASS - caught before AES)
- Tampered steganotext -> "CRC mismatch" (PASS)

**Bug Fix Tests:**
- Short synonym "do" not matched inside "doing" (PASS - M4 fix)
- "civil-servant" not matched as "servant" substring (PASS - M3 fix)
- Homoglyph only shows 3 safe pairs (PASS - M1+M2 fix)

---

## Slide 12: Conclusion & Future Work

**Title**: Summary & Next Steps

**What this project demonstrates:**
1. Thai text has unique properties ideal for steganography
2. Combining cryptography + steganography provides defense-in-depth
3. 4 techniques with different tradeoffs (capacity vs detectability)
4. Security audit process: find vulnerabilities, fix them, verify

**Potential applications:**
- Digital watermarking for document tracking
- Covert communication channels
- Copyright protection for Thai text content
- Security awareness training

**Future improvements:**
- Thai word segmentation library for better synonym/misspelling boundary detection
- Support for image + text hybrid steganography
- Statistical analysis of detection resistance per technique
- Mobile app version

**Disclaimer**: This project is for educational purposes only.

---

## Design Notes for AI Agent

- Use a dark theme with accent colors: blue (encryption), green (steganography), red (security)
- Slide 5 and 6 are the most important technical slides - make the diagrams clear
- Slide 7 should feel like a real messaging app - make it relatable
- For Thai text examples, use actual Thai characters (the project is Thai-specific)
- Keep each slide to 1 key idea maximum
- Use icons: lock (encryption), eye-slash (steganography), shield (security), magnifying glass (audit)
- Total: 12 slides, approximately 15-20 minute presentation
- Add slide numbers and project title in footer
