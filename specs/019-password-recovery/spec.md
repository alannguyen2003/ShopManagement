# Spec: Recover a forgotten password

**Feature:** 019-password-recovery
**Branch:** feat/019-password-recovery
**Source:** R002/BL-07
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-17

## Context

The single Admin (constitution P1) cannot sign in after forgetting the password. Without a way back in, the Admin is locked out of product management for good (R002 "Risks and dependencies"). There is no self-service reset: no email and no security questions (R002 BR-033, out of scope). The system operator, assumed to be the same person as the Admin, recovers access instead. The system operator puts an explicit reset indication and a new password in the setup values and restarts the system. At system start, the system replaces the password, clears any lockout and ends every Admin session. No other data changes. A recovery can also replace a stored password that cannot be used (spec 013 FR-014). This item builds on Admin account setup (BL-01, spec 013), sign-in (BL-02, spec 014) and the lockout (BL-03, spec 015). Scenarios that check an Admin session is still valid after a restart also rely on Admin sessions surviving a restart (BL-05, spec 017 FR-016).

## User stories

### US1 — Recover access with a new password (Priority: P1)

As the system operator, I want to replace a forgotten Admin password through the setup values and a restart so that the Admin can sign in again without losing any data.

**Why P1:** Without it, a forgotten password means losing access for good. This is the core of the item.

**Independent test:** Set up an Admin account with a known password. Put the reset indication and a valid new password in the setup values and restart. Check that the new password signs in, the old one is refused, any lockout is gone, and earlier Admin sessions are refused.

**Acceptance scenarios:**
1. **Given** the Admin account "admin" has the password "old-password-0001", **When** the system operator sets the reset indication and the new password "new-password-0002" in the setup values and restarts the system, **Then** signing in with "admin" / "new-password-0002" shows the product management page, and signing in with "admin" / "old-password-0001" is refused with the generic failed sign-in message (BR-033, BR-010).
2. **Given** sign-in is locked because of 5 failed sign-in attempts 2 minutes ago, **When** a recovery with a valid new password is applied at start, **Then** signing in with the new password succeeds at once, without waiting for the lockout to end (BR-033).
3. **Given** the Admin is signed in in two browsers, **When** a recovery is applied at start, **Then** the next request from each browser is refused as if there were no Admin session and returns no data (BR-033, BR-023, QE-05).
4. **Given** 3 failed sign-in attempts in a row and no lockout, **When** a recovery is applied at start, **Then** 4 more failed sign-in attempts after the start do not lock sign-in (the count restarted at 0).
5. **Given** a recovery is applied, **When** the log is read, **Then** it has one recovery entry with its time and kind, and it contains neither the old nor the new password nor any password hash (BR-019, QE-03).
6. **Given** the setup values contain a new password but no reset indication, **When** the system restarts, **Then** the password is unchanged: the old password signs in and the new one is refused, and the log warns that the new password should be removed from the setup values, without its value (BR-004, FR-017).
7. **Given** a recovery whose setup values also contain a sign-in name different from the Admin account's, **When** the recovery is applied, **Then** the sign-in name is unchanged and only the stored sign-in name signs in (BR-004).
8. **Given** stored data other than the Admin account exists (for example products), **When** a recovery is applied, **Then** that data is unchanged.
9. **Given** a recovery with "new-password-0002" is applied, **When** the stored Admin account is read, **Then** the password is stored only as a salted slow hash with the algorithm, salt and iteration count of constitution P4, its salt differs from the salt stored before the recovery, it matches "new-password-0002", and the record of FR-010 does not contain "new-password-0002" in readable form (BR-008).
10. **Given** the reset indication and a new password are present only in the configuration delivered with the system, **When** the system restarts, **Then** no recovery is applied and the old password signs in (FR-018).
11. **Given** exactly one Admin account exists and its stored password cannot be used (for example, its iteration count is below 600,000), **When** the system starts with the reset indication and the new password "new-password-0002", **Then** the recovery is applied, the system starts, and "admin" / "new-password-0002" signs in (FR-002; spec 013 FR-014).
12. **Given** the reset indication and the new password "café-shop-2026" written with "e" followed by a combining accent, **When** the recovery is applied, **Then** signing in with "café-shop-2026" written with the single precomposed letter "é" succeeds (FR-003; spec 013 FR-015).

### US2 — Refuse an invalid new password (Priority: P1)

As the system operator, I want a recovery with an invalid new password to be refused without touching the current password so that a typing mistake never leaves the Admin with a password that breaks the rules.

**Why P1:** Applying a password that breaks BR-007 would violate an approved business rule, and a half-applied recovery could lock the Admin out.

**Independent test:** Restart with the reset indication and an 11-character new password. Check that the system does not start, that the log names the reason without the value, and that after removing the reset indication and restarting, the old password still signs in.

**Acceptance scenarios:**
1. **Given** the reset indication and an 11-character new password in the setup values, **When** the system starts, **Then** it does not start, and the log names the new password setting and says it is too short, without its value or its length (BR-037, BR-007).
2. **Given** the reset indication and a 129-character new password, **When** the system starts, **Then** it does not start, and the log names the setting and says it is too long, without its value or its length (BR-037, BR-007).
3. **Given** a start refused as in scenario 1, **When** the system operator removes the reset indication and restarts, **Then** the old password signs in, and an Admin session that was active before the refused start is still valid (BR-037, BR-045). *(The session part runs once spec 017 US4 is built.)*
4. **Given** the reset indication and a new password of exactly 12 characters, or of exactly 128 characters, or of 11 emoji followed by one letter (12 code points), **When** the system starts, **Then** the recovery is applied (BR-007, FR-003).
5. **Given** the reset indication and a new password made only of letters, or containing spaces and symbols, **When** the system starts, **Then** the recovery is applied; no composition rule applies (BR-007).
6. **Given** the reset indication and the new password "  spaced-pass-01  " (with leading and trailing spaces), **When** the recovery is applied, **Then** that exact value signs in, and "spaced-pass-01" without the spaces is refused (BR-006).
7. **Given** the reset indication without any new password, **When** the system starts, **Then** it does not start, and the log names the missing setting (assumption, see Assumptions).
8. **Given** no Admin account exists, and the setup values hold a valid sign-in name and initial password, the reset indication and the new password "new-password-0002", **When** the system starts, **Then** it does not start, no Admin account is created, and the log says that a recovery needs an existing Admin account, without any value (FR-016).
9. **Given** the reset indication and a 14-character new password that ends with a line break, or that contains a tab, **When** the system starts, **Then** it does not start, nothing is applied, and the log names the new password setting and says it contains a control character, without its value; the old password still signs in (BR-007, BR-037).

### US3 — Apply a recovery only once (Priority: P2)

As the system operator, I want a recovery left in the setup values to be ignored at later starts so that a routine restart does not reset the password again or sign the Admin out.

**Why P2:** US1 and US2 already give a working, safe recovery. This story protects later restarts when the system operator forgets to remove the reset (R002 PR-07 step 4 only recommends removing it).

**Independent test:** Apply a recovery. Sign in. Restart with the same reset indication and new password still in the setup values. Check that the Admin session is still valid and that the log warns that the reset indication is still present, without the value.

**Acceptance scenarios:**
1. **Given** a recovery with the new password "new-password-0002" was applied and the Admin then signed in, **When** the system restarts with the same reset indication and new password still in the setup values, **Then** the Admin's session is still valid, the system starts normally, and the log warns that the reset indication is still present, without the value (BR-046). *(The session part runs once spec 017 US4 is built.)*
2. **Given** a recovery was applied, and later sign-in was locked by 5 failed sign-in attempts, **When** the system restarts with the same reset still in the setup values, **Then** the lockout is not cleared and ends 15 minutes after it began (BR-046, BR-045).
3. **Given** a recovery with "new-password-0002" was applied and the Admin then changed the password to "changed-pass-0003" (BL-06, spec 018), **When** the system restarts with the same reset still in the setup values, **Then** "changed-pass-0003" signs in and "new-password-0002" is refused (BR-046). *(This scenario can run only once spec 018 is built.)*
4. **Given** a recovery with "new-password-0002" was applied, **When** the system operator sets the reset indication with the different new password "new-password-0004" and restarts, **Then** a new recovery is applied: "new-password-0004" signs in, any lockout is cleared and every Admin session ends (BR-046, BR-033).
5. **Given** recoveries with "new-password-0002" and then "new-password-0004" were applied, **When** the system restarts with the reset indication and "new-password-0002", **Then** the recovery is applied, because it differs from the most recently applied one; only that one is remembered (FR-010).

## Edge cases

- The reset indication is present but no Admin account exists yet → the system does not start, and the log says that a recovery needs an existing Admin account, without any value. Admin account setup does not run until the system operator removes the reset indication (FR-016).
- A new password is in the setup values without the reset indication → the new password is ignored (BR-004), and every start logs a warning, without the value, that it should be removed (FR-017).
- The reset indication has a value that is neither clearly on nor clearly off → the system does not start, and the log names the setting (assumption, constitution P6) (FR-014).
- The reset indication is present but the new password is missing or empty → handled like an invalid new password: the system does not start (FR-015).
- The new password equals the current password but differs from the most recently applied recovery → the recovery is applied normally (lockout cleared, every Admin session ended). If it equals the most recently applied recovery, it is an ignored repeat (FR-010), and to clear a lockout the system operator must provide a different new password (BR-046) (assumption).
- The new password ends with a line break picked up from the environment (for example, a value copied from a file) → it is refused, not silently trimmed, so the stored hash always matches what the Admin will type (FR-003, FR-004).
- The stored password cannot be used and a valid recovery is present → the recovery replaces it and the system starts (FR-002). If the new password equals the most recently applied recovery, it is an ignored repeat (FR-010), and the system does not start under spec 013 FR-014; the system operator must provide a different new password.
- The new password breaks BR-007 while sign-in is locked → nothing is applied, the system does not start, and the lockout keeps running on its own clock (BR-037, BR-045).
- The system stops in the middle of applying a recovery (power loss, crash) → either all of the recovery took effect or none of it. If none, the recovery is applied at the next start, because it was never recorded as applied (FR-011).
- The Admin has an open form when the recovery ends the session → the next refused operation behaves as for any ended session (BL-05, spec 017). Keeping the form values is R001's concern (R001 BR-049).
- A request reaches the system during start, before the recovery has finished → no request is accepted until the recovery is applied or refused (FR-002).
- The same reset is left in the setup values for many restarts → the recovery is applied only the first time; every later start logs the warning (BR-046).
- The system operator needs a second recovery and reuses the last applied new password → it is ignored as a repeat; the system operator must provide a different new password (BR-046).
- Someone uses the sign-in page to look for a "forgot password" path → there is none (FR-001).

## Functional requirements

- **FR-001:** The system MUST NOT offer any self-service way to reset or recover the password: no page, operation, link, email or security question. The only recovery is the one in FR-002 (BR-033).
- **FR-002:** At system start, when an Admin account exists, the reset indication is on (FR-014), and the new password is present, meets BR-007 (FR-003) and differs from the most recently applied recovery (FR-010), the system MUST apply a recovery before it accepts any request. This also applies when exactly one Admin account exists and only its stored password cannot be used (spec 013 FR-014): the recovery replaces it and the system starts. A recovery replaces the password, clears any lockout and ends every Admin session (BR-033). This applies to each new password that differs from the most recently applied one, so a second recovery needs a different new password (BR-046).
- **FR-003:** Before applying a recovery, the system MUST normalize the new password to Unicode Normalization Form C (NFC), as spec 013 FR-015 requires, and then check it against BR-007: 12 to 128 characters counted in Unicode code points of the normalized value, no control character (category Cc: a line break, a tab or any other), no composition rules. Apart from normalization, the new password MUST be taken exactly as given, with leading and trailing spaces kept (BR-007, BR-006). The normalized value is the one hashed, stored and compared under FR-010.
- **FR-004:** When the new password breaks BR-007, the system MUST NOT apply the recovery and MUST NOT start. It MUST log the name of the setting and the reason (too short, too long or contains a control character), without the value or its length. The old password stays valid, and the lockout state, the count of failed sign-in attempts in a row and the Admin sessions are unchanged (BR-037).
- **FR-005:** A recovery MUST store the new password only as a salted slow hash, as constitution P4 requires, and MUST replace the previous one, with a new unique salt, so the old password no longer signs in. The new password is never stored, shown, logged or returned in readable form (BR-008).
- **FR-006:** After a recovery, every Admin session that existed before it, in every browser, MUST be an ended session, and every request made with it MUST be refused as in BR-014 (BR-033, BR-023, BR-015).
- **FR-007:** A recovery MUST clear any lockout and set the count of failed sign-in attempts in a row back to 0 (BR-033; resetting the count without a lockout is an assumption).
- **FR-008:** A recovery MUST NOT change the sign-in name or any stored data other than the password, the lockout state, the count of failed sign-in attempts in a row, the Admin sessions and the record of FR-010. A sign-in name or initial password in the setup values does not change the Admin account (BR-004).
- **FR-009:** When the reset indication is absent or off (FR-014), the system MUST NOT change the password, the lockout, the count of failed sign-in attempts in a row or the Admin sessions because of the setup values, including any new password in them (BR-004). Warnings about values left in the setup values are FR-017 and spec 013 FR-011.
- **FR-010:** The system MUST remember only the most recently applied recovery, not earlier ones, in a form that does not reveal its new password, with the same protection as the Admin password (BR-008, constitution P4). When a later start finds the reset indication with the same new password as the most recently applied recovery, the system MUST NOT replace the password, clear a lockout or end any Admin session. It MUST start normally and log a warning, without the value, that the reset indication is still present (BR-046).
- **FR-011:** A recovery MUST take effect completely or not at all: the password, the lockout state, the count of failed sign-in attempts in a row, the Admin sessions and the record of FR-010 change together (assumption).
- **FR-012:** "The system does not start" (FR-004, FR-014, FR-015, FR-016) has the meaning of spec 013 FR-017: before it listens on any port or answers any request, the system writes every problem to its console log and the process ends with a non-zero exit code. It never keeps running in a degraded state (constitution P9).
- **FR-013:** The system MUST log, with time and kind: an applied recovery, a recovery refused under FR-004 or FR-015 (with the reason), a repeat ignored under FR-010 (with the warning), and a new password left without the reset indication (FR-017, with the warning). These entries MUST NOT contain any password, any password hash, any session secret or the sign-in name value (BR-019, QE-03).
- **FR-014:** Only an explicit "on" value of the reset indication MUST start a recovery. An absent or explicit "off" value means no recovery. Any other value MUST stop the system from starting, and the log MUST name the setting (assumption, constitution P6).
- **FR-015:** When the reset indication is on and the new password is missing or empty, the system MUST NOT apply a recovery and MUST NOT start. It MUST log the name of the missing setting (assumption, consistent with BR-003 and BR-037).
- **FR-016:** When the reset indication is on and no Admin account exists, the system MUST NOT start and MUST NOT create the Admin account, whatever the other setup values hold. It MUST log that a recovery needs an existing Admin account, without any value. Admin account setup (BL-01, spec 013) runs only at a start without the reset indication (assumption, consistent with BR-003 and BR-037).
- **FR-017:** When an Admin account exists and the setup values contain a new password without the reset indication, the system MUST start normally and, at every such start, log a warning, without the value, that the new password should be removed from the setup values, as BR-047 does for the initial password (assumption; constitution P7).
- **FR-018:** The system MUST read the reset indication and the new password only from the environment the system runs in, as for the other setup values (spec 013 FR-003). When they are present only in the configuration delivered with the system, they are treated as absent (BR-033; QE-03; constitution P7).

## Non-functional requirements

- **NFR-003:** There are 0 occurrences of a password (old or new), a password hash or a session secret in logs, error responses or committed configuration. This is checked automatically on the logs of an applied recovery, a refused recovery (too short, too long, missing), an ignored repeat and a new password left without the reset indication (QE-03).
- **NFR-005:** 100% of requests made with an Admin session that existed before an applied recovery are refused. This is checked automatically with sessions from at least two browsers (QE-05).

## Key entities

- **Admin account:** the only account (BR-001). Its sign-in name, its password (stored only as a salted slow hash), the count of failed sign-in attempts in a row, the lockout state and the record of the most recently applied recovery (kept only in a form that does not reveal its password). A recovery changes everything except the sign-in name.
- **Setup values:** values the system operator puts in the environment the system runs in, read at system start and never from committed files. For recovery, they hold the reset indication and the new password. They may also still hold the sign-in name and initial password from Admin account setup.
- **Reset indication:** the explicit setup value that asks for a recovery at system start (BR-033).
- **Admin session:** each browser's signed-in period (BR-015). Every Admin session becomes an ended session when a recovery is applied.
- **Lockout:** the account-wide period of refused sign-ins (BR-016). An applied recovery clears it.

## Success criteria

- **SC-001:** In 100% of acceptance runs, the Admin signs in with the new password on the first attempt after an applied recovery, and the old password is refused.
- **SC-002:** 0 stored records other than the Admin account's password, lockout state, count of failed sign-in attempts in a row, Admin sessions and recovery record differ before and after a recovery.
- **SC-003:** In 100% of restarts with an already applied reset indication and new password still in the setup values, no Admin session ends (once spec 017 US4 is built) and the password is not replaced.
- **SC-004:** 0 recoveries with a new password outside 12–128 characters are applied.

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BR-004 | FR-008, FR-009; US1 scenarios 6, 7 |
| BR-007 | FR-003, FR-004; US1 scenario 12; US2 scenarios 1, 2, 4, 5, 9 |
| BR-008 | FR-005, FR-010; US1 scenario 9; Key entities (Admin account) |
| BR-019 | FR-013; US1 scenario 5 |
| BR-033 | FR-001, FR-002, FR-006, FR-007; US1 scenarios 1–3; US3 scenario 4 |
| BR-037 | FR-004; US2 scenarios 1–3; Edge cases (invalid while locked) |
| BR-046 | FR-002, FR-010; US3 scenarios 1–5; Edge cases (repeated reset, reused password) |
| QE-03 | NFR-003; FR-013, FR-018; US1 scenarios 5, 10 |
| QE-05 | NFR-005; FR-006; US1 scenario 3 |

## Assumptions

- The system operator and the Admin are the same person, so the setup values and recovery are trusted (R002 Assumptions). Whoever controls the environment the system runs in controls the Admin account.
- A recovery happens during start, before any request is accepted, so no sign-in can race with it.
- A recovery also sets the count of failed sign-in attempts in a row back to 0 when there is no lockout. BR-033 only says it clears a lockout, and BR-018 resets the count when a lockout ends; doing the same for a recovery keeps the "fresh start" simple.
- A new password equal to the current one is applied like any other, unless it equals the most recently applied recovery (FR-010). The same-password refusal of BR-029 belongs to a password change, not to a recovery.
- Only an explicit "on" value of the reset indication counts. An unrecognised value stops the system, because constitution P6 requires configuration to be validated at the boundary.
- A reset indication with a missing or empty new password is handled like an invalid new password (BR-037): the system does not start.
- The new password is taken exactly as given apart from NFC normalization, and its length counts every code point of the normalized value, including spaces. This matches how the initial password is handled in spec 013 (FR-007, FR-015).
- The sign-in page shows no "forgot password" link or hint.
- The Admin gets no notice in the browser that a recovery took place. Earlier sessions are simply ended (BL-05 handles what the Admin sees).
- The system operator learns the outcome of a recovery from the log (FR-013).
- NFR numbers match the R002 QE numbers they come from. QE-01, QE-02, QE-04, QE-06, QE-07 and QE-08 belong to other backlog items, so NFR-001, 002, 004, 006, 007 and 008 are not used here.

## Out of scope

- Password reset by email, security questions or any self-service path: excluded by R002 (BR-033, "Out of scope"), and no mail capability is allowed (docs/architecture.md).
- Creating the Admin account, checking the initial setup values, and the warning about an initial password left in the setup values (BR-047): BL-01, spec 013-admin-account-setup.
- Signing in, the generic failed sign-in message and encrypted connections: BL-02, spec 014-admin-sign-in.
- Counting failed sign-in attempts, starting and showing a lockout: BL-03, spec 015-sign-in-lockout.
- What the Admin sees when a request is refused because a recovery ended the session (sign-in prompt over the current view): BL-05, spec 017-session-expiry.
- Changing the password while signed in: BL-06, spec 018-change-password.
- Changing the sign-in name: not requested (R002 "Out of scope").
- Removing the reset indication and the new password from the setup values automatically. The system operator does this (R002 PR-07 step 4).
- Keeping an open product form's values when its session ends: R001 BL-01, BL-04 and BL-08 (R001 BR-049).

## Open questions

- [x] 1. At a start where no Admin account exists and the setup values also hold the reset indication (and possibly a new password), what should the system do? A) Refuse to start, and log that a recovery needs an existing Admin account, until the system operator removes the reset indication. B) Ignore the reset, let Admin account setup (BL-01) create the account from the sign-in name and the initial password, and record the reset as already applied so that the next start does not reset the new account. C) Create the account from the new password. Recommended: A, because it is explicit, keeps no hidden state and matches the other "does not start" setup rules (BR-003, BR-037). This spec owns the rule; spec 013 points to FR-016. (affects: FR-016, Admin account setup in spec 013) → A: refuse to start (FR-016, US2 scenario 8).
- [x] 2. When an Admin account exists and the setup values hold a new password without the reset indication, should every start log a warning, without the value, that the new password should be removed, as BR-047 does for the initial password? A) Yes. B) No, ignore it silently (BR-004 only). Recommended: A, because a readable password left in the environment is the same risk BR-047 addresses (constitution P7). (affects: FR-017, NFR-003) → A: warn at every start (FR-017, US1 scenario 6).

## Clarification log

| Date | Question | Decision |
|------|----------|----------|
| 2026-10-17 | Reset indication on but no Admin account exists? | Refuse to start, create nothing, log that a recovery needs an existing Admin account (FR-016). Setup runs only without the reset indication. |
| 2026-10-17 | Warn when a new password is left without the reset indication? | Yes: every start logs a warning without the value, like BR-047 (FR-017). |
| 2026-10-17 | Align with spec 013 and R002 BR-007 rev 2? | Yes: NFC normalization and code-point length, control characters refused (FR-003, FR-004), a valid recovery repairs an unusable stored password (FR-002), and "does not start" follows spec 013 FR-017 (FR-012). |
| 2026-10-17 | After recoveries with A then B, is A again a repeat? | No: only the most recently applied recovery is remembered and compared, so A is applied (FR-010, US3 scenario 5). |

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-17 | Initial version | — |
