# Spec: Admin account setup

**Feature:** 013-admin-account-setup
**Branch:** feat/013-admin-account-setup
**Source:** R002/BL-01
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-17

## Context

The system has no Admin account yet, so nobody can sign in, and every R001 product feature depends on a signed-in Admin. Constitution P1 forbids self-registration, so the system operator creates the single Admin account outside the application. When the system starts and no Admin account exists, it creates the account from setup values that the system operator puts in the environment the system runs in. The account must never be created with a default password, and the password must never be readable anywhere (P4, P7). This is the first R002 item. Sign-in (BL-02, spec 014) builds on it.

## User stories

### US1 — Create the Admin account at first start (Priority: P1)

As the system operator, I want the system to create the single Admin account from the setup values when it starts for the first time, so that the Admin can sign in without a default password, a setup page or a separate setup command.

**Why P1:** Without an Admin account, no other R002 or R001 item can be used.

**Independent test:** Start a system that has no Admin account and has valid setup values. Then check the stored data: exactly one Admin account exists, with the given sign-in name and a stored password hash that matches the initial password (recomputed with the stored salt and iteration count). The initial password appears nowhere in the stored data, the logs or the error output.

**Acceptance scenarios:**
1. **Given** no Admin account exists and the setup values hold the sign-in name "admin" and a 16-character initial password, **When** the system starts, **Then** exactly one Admin account exists with the sign-in name "admin", and its password is stored only as a salted slow hash that matches the initial password (BR-001, BR-002, BR-008).
2. **Given** two systems with no Admin account, **When** both are set up with the same initial password, **Then** their stored salts differ and their stored hashes differ (BR-008).
3. **Given** no Admin account exists and the sign-in name in the setup values is "  Shop Admin  ", **When** the system starts, **Then** the Admin account is created with the sign-in name "Shop Admin": the leading and trailing spaces are removed and the letter case is kept (BR-005).
4. **Given** no Admin account exists and the sign-in name is "owner", which is not an email address, **When** the system starts, **Then** the Admin account is created. No email format is checked (BR-005).
5. **Given** no Admin account exists, the sign-in name is exactly 100 characters long and the initial password is exactly 12 characters long, **When** the system starts, **Then** the Admin account is created (BR-005, BR-007).
6. **Given** no Admin account exists and the initial password is exactly 128 characters long, **When** the system starts, **Then** the Admin account is created (BR-007).
7. **Given** no Admin account exists and the initial password is "  my shop pass  " (16 characters, with leading and trailing spaces), **When** the system starts, **Then** the stored hash matches that exact password with its spaces, and does not match "my shop pass" (BR-007; R002 BR-006).
8. **Given** no Admin account exists and the initial password is "café-shop-2026" written with "e" followed by a combining accent (15 code points before normalization, 14 after), **When** the system starts, **Then** the Admin account is created, and the stored hash matches "café-shop-2026" written with the single precomposed letter "é" (FR-015).
9. **Given** no Admin account exists and the initial password is 11 emoji followed by one letter (12 code points), **When** the system starts, **Then** the Admin account is created (FR-015, BR-007).
10. **Given** no Admin account exists and the setup values are valid, **When** the system starts, **Then** the log records, with its time, that the Admin account was created and that the initial password should now be removed from the setup values. The log contains neither the sign-in name, nor the initial password, nor the hash.

### US2 — Refuse to start without valid setup values (Priority: P1)

As the system operator, I want the system to refuse to start when it has no Admin account and the setup values are missing or invalid, so that no account is ever created with a built-in, default or weak password, and so that I know exactly which setting to fix.

**Why P1:** This is the only protection against a default or invalid Admin account (BR-003).

**Independent test:** Start a system that has no Admin account, once for each kind of missing or invalid setup value. Each time, check that the system did not start, that no Admin account exists, and that the log names the setting and the rule it breaks without its value.

**Acceptance scenarios:**
1. **Given** no Admin account exists and there are no setup values at all, **When** the system starts, **Then** it does not start, no Admin account exists, and the log names both the missing sign-in name setting and the missing initial password setting (BR-003).
2. **Given** no Admin account exists, the sign-in name is set and the initial password is missing, **When** the system starts, **Then** it does not start, no Admin account exists, and the log names the initial password setting as missing (BR-003).
3. **Given** no Admin account exists and the initial password is 11 characters long, **When** the system starts, **Then** it does not start, no Admin account exists, and the log says the initial password is shorter than 12 characters. The log contains neither the value nor its actual length (BR-003, BR-007).
4. **Given** no Admin account exists and the initial password is 129 characters long, **When** the system starts, **Then** it does not start, no Admin account exists, and the log says the initial password is longer than 128 characters, without the value (BR-003, BR-007).
5. **Given** no Admin account exists and the sign-in name holds only spaces, **When** the system starts, **Then** it does not start, and the log says the sign-in name is empty (BR-003, BR-005).
6. **Given** no Admin account exists and the sign-in name is 101 characters long after the leading and trailing spaces are removed, **When** the system starts, **Then** it does not start, and the log says the sign-in name is longer than 100 characters, without the value (BR-003, BR-005).
7. **Given** no Admin account exists and the sign-in name and initial password are present only in the configuration delivered with the system, **When** the system starts, **Then** they are treated as missing: the system does not start and no Admin account exists (BR-002).
8. **Given** a start has failed because the setup values were invalid, **When** the system operator corrects them and starts the system again, **Then** the Admin account is created as in US1 (BR-003).
9. **Given** no Admin account exists and the initial password is a valid 16-character password followed by a line break, **When** the system starts, **Then** it does not start, no Admin account exists, and the log says the initial password contains a control character, without the value or the position of the character (BR-003, BR-007).
10. **Given** no Admin account exists and the initial password contains a tab between its characters, **When** the system starts, **Then** it does not start and no Admin account exists, as in scenario 9 (BR-003, BR-007).

### US3 — Leave the existing Admin account unchanged (Priority: P1)

As the system operator, I want the setup values to have no effect once the Admin account exists, so that a restart, or a value left behind in the environment, never changes or replaces the Admin's password.

**Why P1:** Without this, every restart could overwrite the Admin account (BR-004).

**Independent test:** Create the Admin account, then restart the system with different, missing and invalid setup values. Each time, check that the stored Admin account is identical to before and that there is still exactly one account.

**Acceptance scenarios:**
1. **Given** the Admin account exists, and the setup values now hold a different sign-in name and a different valid password, **When** the system starts, **Then** the Admin account is unchanged: same sign-in name, same stored hash and salt, and the stored hash still matches the original password (BR-004).
2. **Given** the Admin account exists and there are no setup values, **When** the system starts, **Then** it starts normally and the Admin account is unchanged (BR-004).
3. **Given** the Admin account exists and the setup values hold a 5-character initial password, **When** the system starts, **Then** it starts normally, the Admin account is unchanged and the setup values are not checked (BR-004; BR-003 applies only when no Admin account exists). The log warns, without the value, that the initial password should be removed (BR-047).
4. **Given** the Admin account exists, **When** the system is restarted 3 times, **Then** exactly one Admin account exists each time (BR-001).

### US4 — Warn about an initial password left in the setup values (Priority: P2)

As the system operator, I want a warning at each start while the initial password is still in the setup values, so that I remember to remove it from the environment.

**Why P2:** It reduces the risk that the initial password stays readable in the environment. The account is already safe without it (US3). US4 still ships with this feature, because BR-047 is in BL-01's Covers; P2 only orders the build.

**Independent test:** With an existing Admin account, start the system once with an initial password in the setup values and once without one. Check that the warning appears only in the first case, and never contains the value.

**Acceptance scenarios:**
1. **Given** the Admin account exists and an initial password is present in the setup values, **When** the system starts, **Then** it starts normally and the log warns that the initial password should be removed from the setup values. The warning does not contain the value (BR-047).
2. **Given** the Admin account exists and the initial password present in the setup values differs from the current password, **When** the system starts, **Then** the same warning is logged, without the value (BR-047).
3. **Given** the Admin account exists, the initial password is not in the setup values and the sign-in name still is, **When** the system starts, **Then** no warning about the initial password is logged (BR-047).
4. **Given** the Admin account exists and the initial password stays in the setup values, **When** the system is started twice, **Then** the warning is logged at both starts (BR-047).

## Edge cases

- No Admin account exists and both setup values are invalid (for example, an empty sign-in name and a 5-character password): every problem is logged during the same start, so the system operator can fix them all at once. The system does not start (BR-003).
- An initial password made only of spaces, or containing any other printable characters, counts like any other password. It is accepted if it is 12 to 128 characters long (BR-007: no composition rules). Only control characters are refused (FR-007).
- The initial password ends with a line break picked up from the environment (for example, a value copied from a file): it is refused, not silently trimmed, so the stored hash always matches exactly what the Admin will type (FR-007).
- The initial password setting is present but empty: treated as missing (BR-003). When the Admin account exists, an empty value does not trigger the warning of US4.
- The system stops part-way through creating the Admin account (for example, the process is killed): no partial Admin account remains, and the next start creates it as in US1 (FR-012).
- Two starts of the system against the same stored data happen at the same time with no Admin account: exactly one Admin account ends up existing (FR-001).
- The stored data cannot be read at start, or holds more than one Admin account, or an Admin account whose stored password cannot be used: the system does not start and logs the problem without any value (FR-014; constitution P9). Exception: if the only problem is an unusable stored password and the setup values hold a valid recovery, the recovery (019) replaces the password and the system starts.
- The setup values hold a reset indication while no Admin account exists: handled by 019-password-recovery (its FR-016). Until 019 is built, this feature does not know the reset indication and ignores it like any other unknown setting.
- The sign-in name in the setup values changes after the Admin account exists: it is ignored. Changing the sign-in name is out of scope in R002.

## Functional requirements

- **FR-001:** The system MUST have exactly one Admin account. No page, operation or command may register, create another account or assign roles. This holds even when two starts happen at the same time (BR-001).
- **FR-002:** When the system starts and no Admin account exists, it MUST create the Admin account from the setup values (sign-in name and initial password) before it accepts any request (BR-002).
- **FR-003:** The system MUST read the setup values only from the environment it runs in (environment variables, or the system operator's local secret store outside the system's source, as docs/architecture.md "Configuration" allows), never from the configuration delivered with the system (the configuration files that are part of its source). A setup value present only in the delivered configuration counts as missing. There MUST be no setup page and no separate setup command (BR-002).
- **FR-004:** The system MUST NOT create an Admin account with a built-in or default password. When no Admin account exists and a setup value is missing or invalid, the system MUST NOT start and MUST NOT create an Admin account (BR-003).
- **FR-005:** When the system does not start under FR-004, it MUST log every missing or invalid setting, by its name and with the rule it breaks. The log MUST NOT contain the value or its length (BR-003).
- **FR-006:** The sign-in name MUST be 1 to 100 characters long after leading and trailing spaces are removed. No email format is checked. It is stored without the leading and trailing spaces, with its letter case kept (BR-005).
- **FR-015:** Before any length check, hashing or storing, the system MUST normalize the sign-in name and the initial password to Unicode Normalization Form C (NFC). Lengths MUST be counted in Unicode code points of the normalized value (an emoji or an accented letter counts as 1). Specs 014, 018 and 019 MUST apply the same normalization and counting, so the same visible password always matches whatever device it is typed on.
- **FR-007:** The initial password MUST be 12 to 128 characters long. Any characters are allowed, there are no composition rules, and the password is used exactly as given, spaces included (BR-007). The one exception: an initial password containing a control character (a line break, a tab or any other Unicode control character) is invalid, because the single-line sign-in field can never reproduce it. The system then does not start, as FR-004 and FR-005 require (BR-007 as amended, see Open question 3).
- **FR-008:** The system MUST store the password only as a salted slow hash, as constitution P4 requires. It MUST never store, show, log or return the password in readable form (BR-008).
- **FR-009:** Once the Admin account exists, the system MUST NOT change its sign-in name or password from the setup values, whether they are present, absent, valid or invalid. The only exception is the recovery of BL-07 (BR-004).
- **FR-010:** Once the Admin account exists, the system MUST start without requiring any setup values (BR-004).
- **FR-011:** At every start where the Admin account already exists and an initial password is present in the setup values, the system MUST start normally and log a warning, without the value, that the initial password should be removed from the setup values (BR-047).
- **FR-012:** Creating the Admin account MUST be all-or-nothing. An interrupted creation leaves no Admin account, and the next start creates it (assumption).
- **FR-013:** At the start that creates the Admin account, the system MUST log the creation with its time, and remind the system operator to remove the initial password from the setup values. The log MUST NOT contain the sign-in name, the password or the hash (assumption).
- **FR-014:** When the stored data cannot be read, holds more than one Admin account, or holds an Admin account whose stored password cannot be used, the system MUST NOT start, and MUST log the problem without any value (assumption; constitution P9). A stored password cannot be used when its record is missing or unreadable, its algorithm is not PBKDF2-HMAC-SHA256, its salt is shorter than 16 bytes or its iteration count is below 600,000 (constitution P4). The one exception: when exactly one Admin account exists and only its stored password cannot be used, a valid recovery in the setup values (BL-07, spec 019) replaces that password at the same start, and the system starts. Applying the recovery is 019's job; this feature MUST NOT refuse to start before 019 has had the chance to do so.

## Non-functional requirements

- **NFR-001:** 100% of stored Admin passwords use PBKDF2 with HMAC-SHA256, a unique random salt of at least 16 bytes and at least 600,000 iterations, and the stored data records the algorithm, salt and iteration count (constitution P4; BR-008).
- **NFR-002:** Across at least 10 setups with the same initial password, 0 stored salts or hashes are the same (constitution P4; BR-008).
- **NFR-003:** In every start (successful, refused or with a warning), the logs and error output contain 0 occurrences of the initial password, the password hash or the sign-in name. The configuration delivered with the system contains 0 setup values. Checked with a sign-in name and an initial password that cannot occur in ordinary log text (for example `op-7Qz2k` and `Kx9!vT3m#pQ2wL`) (QE-03; BR-003, BR-047).

## Key entities

- **Admin account:** The only account in the system. It holds the sign-in name (trimmed, case kept) and the password, stored only as a salted slow hash together with its salt and iteration count. There is exactly one, created by Admin account setup.
- **Setup values:** What the system operator puts in the environment the system runs in: the sign-in name and the initial password. For recovery (BL-07), they can also hold the reset indication and the new password. They are read at system start and are never in committed files.

## Success criteria

- **SC-001:** After Admin account setup, a search of the stored data, the logs, the error output and the committed files for the initial password used in the test finds 0 readable occurrences (BG-04).
- **SC-002:** 100% of starts with no Admin account and missing or invalid setup values end with the system not running and no Admin account stored (BR-003).
- **SC-003:** 0 pages, operations or commands in the system can register, create a second account or assign roles (BR-001).
- **SC-004:** The system operator sets up the Admin account with no step other than providing the 2 setup values and starting the system (BR-002).

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BG-04 | SC-001, NFR-003, FR-008 |
| BR-001 | FR-001, SC-003, US1 scenario 1, US3 scenario 4 |
| BR-002 | FR-002, FR-003, SC-004, US1 scenario 1, US2 scenario 7 |
| BR-003 | FR-004, FR-005, SC-002, US2 scenarios 1–8, edge cases (several invalid, empty value) |
| BR-004 | FR-009, FR-010, US3 scenarios 1–3 |
| BR-005 | FR-006, US1 scenarios 3–5, US2 scenarios 5–6 |
| BR-007 | FR-007, FR-015, US1 scenarios 5–9, US2 scenarios 3–4 and 9–10, edge cases (only spaces, trailing line break) |
| BR-008 | FR-008, NFR-001, NFR-002, US1 scenarios 1–2 |
| BR-047 | FR-011, US4 scenarios 1–4 |
| QE-03 | NFR-003 |

## Assumptions

- The sign-in name is stored without leading and trailing spaces and with its letter case kept. Matching it while ignoring case is BL-02's job (R002 BR-006).
- The log for an invalid setup value gives the rule that was broken (for example "shorter than 12 characters"), never the actual length or the value.
- Every problem in the setup values is logged during the same refused start, rather than only the first one.
- A setup value that is present but empty counts as missing.
- The start that creates the Admin account logs the creation and a reminder to remove the initial password (FR-013). BR-047 applies to later starts.
- The system does not log the sign-in name from the setup values. R002 QE-03 forbids logging an entered sign-in name, and the same caution is applied here.
- When the stored data is damaged or holds more than one Admin account, refusing to start is the safe default (FR-014; constitution P9).
- No warning is logged when the sign-in name in the setup values differs from the stored one. Only BR-047 asks for a warning.
- NFR numbers are local to this spec; QE-03 is NFR-003.

## Out of scope

- Signing in, checking an entered password (including the constant-time comparison) and the sign-in page: BL-02 (spec 014-admin-sign-in).
- Protecting pages and operations, the health check and the OpenAPI document: BL-02 (spec 014).
- Lockout after failed sign-in attempts: BL-03 (spec 015-sign-in-lockout).
- Signing out: BL-04 (spec 016-sign-out).
- Session expiry and signing in again: BL-05 (spec 017-session-expiry).
- Changing the password: BL-06 (spec 018-change-password).
- Recovering a forgotten password, the reset indication and the new password in the setup values: BL-07 (spec 019-password-recovery), including what happens when the reset indication is present while no Admin account exists (019 FR-016).
- Logging of sign-in, sign-out, password change and recovery events (R002 BR-019): BL-02, BL-03, BL-04, BL-06 and BL-07.
- Changing the sign-in name; a setup page or setup command; more accounts or roles; two-factor authentication or email (R002 "Out of scope"; constitution P1).
- Any product feature (R001).

## Open questions

- [x] 1. When the stored Admin password cannot be used (FR-014) and the setup values also hold a valid recovery (019), may that recovery repair it at the same start? → Yes (FR-014; spec 019 needs a matching scenario).
- [x] 2. Should an initial password (and a recovery's new password, spec 019) that contains a line break, a tab or another control character be refused? → Refuse (FR-007, US2 scenarios 9–10).
- [ ] 3. **Blocking approval:** the answer to question 2 narrows R002 BR-007 ("any characters are allowed"), which is Approved and takes precedence over this spec. Before `/sdd-approve spec`, run `/sdd-amend requirements R002 "BR-007: a password must not contain control characters (line break, tab or other Unicode control characters); applies at setup, change and recovery"`. Specs 018 and 019 must then apply the same rule. (affects: FR-007, R002 BR-007, specs 018, 019)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|
| 2026-10-17 | May a valid recovery (019) repair an unusable stored password at the same start? | Yes, when exactly one Admin account exists and only its password is unusable; otherwise FR-014 refuses to start. 019 must add a matching scenario. |
| 2026-10-17 | Refuse an initial password containing control characters? | Refuse: the system does not start and logs the rule without the value. Requires amending R002 BR-007 (Open question 3); 018 and 019 apply the same rule. |
| 2026-10-17 | How are lengths counted, and is Unicode normalized? | Normalize to NFC before checking, hashing and storing; count code points of the normalized value (FR-015). 014, 018 and 019 apply the same rule. |

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-17 | Initial version | — |
