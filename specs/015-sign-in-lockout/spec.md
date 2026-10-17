# Spec: Lock sign-in after failed attempts

**Feature:** 015-sign-in-lockout
**Branch:** feat/015-sign-in-lockout
**Source:** R002/BL-03
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-17

## Context

Spec 014 (R002 BL-02) lets the Admin sign in with a sign-in name and a password. Without a limit, an unauthorised person who can reach the sign-in page could keep guessing the password. Constitution P5 requires brute-force protection on sign-in. This feature counts failed sign-in attempts in a row for the whole Admin account. After 5 of them it locks sign-in for 15 minutes, tells whoever is signing in when to try again, and logs these events without any entered text. It is part of the R002 MVP (BL-01 to BL-05), which is built before R001 BL-01.

## User stories

### US1 — Lock sign-in after 5 failed attempts in a row (Priority: P1)

As the Admin, I want sign-in to be locked after repeated failed sign-in attempts so that an unauthorised person cannot guess my password online.

**Why P1:** Constitution P5 requires protection against brute force. Without it, sign-in (spec 014) does not meet the constitution.

**Independent test:** On a system with an Admin account and no lockout, submit 5 wrong passwords, then the correct password. The correct password is refused. After 15 minutes the correct password signs in.

**Acceptance scenarios:**
1. **Given** no lockout and 0 failed sign-in attempts, **When** 4 sign-ins with a wrong password are submitted and then one with the correct sign-in name and password, **Then** the 5th sign-in succeeds and the count of failed sign-in attempts goes back to 0 (BR-018).
2. **Given** a count of 0 after scenario 1, **When** 4 more sign-ins with a wrong password are submitted, **Then** sign-in is not locked, because the count restarted at 0 (BR-018).
3. **Given** no lockout and 0 failed sign-in attempts, **When** 5 sign-ins with a wrong password are submitted, **Then** sign-in is locked for 15 minutes from the 5th attempt (BR-016).
4. **Given** sign-in is locked, **When** a sign-in with the correct sign-in name and password is submitted, **Then** it is refused, no Admin session is started, and the response is the same as for a wrong password during the lockout (BR-016, FR-004).
5. **Given** no lockout, **When** 5 sign-ins with a sign-in name that does not match the Admin account are submitted, **Then** sign-in is locked exactly as after 5 wrong passwords (BR-016, QE-04).
6. **Given** no lockout, **When** 3 failed sign-in attempts are made from one browser and 2 from another browser or source, **Then** sign-in is locked after the 5th, for both (BR-016).
7. **Given** a lockout that began at time T, **When** the correct sign-in name and password are submitted at T + 15 minutes or later, **Then** the sign-in succeeds (BR-016, BR-044).
8. **Given** a lockout that began at time T, **When** the lockout ends at T + 15 minutes and 1 sign-in with a wrong password is then submitted, **Then** it is counted as the 1st failed sign-in attempt and sign-in is not locked (BR-018).
9. **Given** no lockout and 0 failed sign-in attempts, **When** 50 sign-ins with a wrong password arrive at the same time, **Then** the sign-in name and password are checked for exactly 5 of them, sign-in is locked, and the others are refused as attempts during a lockout (BR-040).
10. **Given** the Admin is signed in in one browser, **When** sign-in becomes locked by failed sign-in attempts from another browser, **Then** the Admin's existing Admin session stays active and the Admin's next operation succeeds (FR-012).

### US2 — Tell whoever is signing in when to try again (Priority: P1)

As the Admin, I want the sign-in page to say that sign-in is locked and when I can try again, so that I do not keep typing a correct password that will be refused.

**Why P1:** Without the message, a locked-out Admin cannot tell a lockout from a wrong password and may believe the password is lost.

**Independent test:** Cause a lockout, then try again 10 minutes later. The 5th attempt says 15 minutes, the later attempt says 5 minutes, and the correct password works at 15 minutes.

**Acceptance scenarios:**
1. **Given** 4 failed sign-in attempts in a row, **When** a 5th sign-in with a wrong password is submitted, **Then** the sign-in page says that sign-in is temporarily locked and can be tried again in 15 minutes, instead of the message of BR-010 (BR-017).
2. **Given** 0 to 3 failed sign-in attempts in a row, **When** another sign-in with a wrong password is submitted, **Then** the sign-in page shows the message of BR-010 (spec 014), with no lockout message and no count of remaining attempts.
3. **Given** a lockout that began 10 minutes ago, **When** any sign-in is submitted, **Then** it is refused, the sign-in page says sign-in is temporarily locked and can be tried again in 5 minutes, and the lockout still ends 15 minutes after it began (BR-017, BR-044).
4. **Given** a lockout that began 10 minutes ago, **When** 20 more sign-ins are submitted during the next 4 minutes, **Then** none of them is counted as a failed sign-in attempt and the lockout still ends 15 minutes after it began (BR-044).
5. **Given** sign-in is locked, **When** one sign-in with the correct password and one with a wrong password are submitted, **Then** both get the same message and outcome, and neither reveals whether the sign-in name or password matched (BR-010, BR-017).
6. **Given** sign-in is locked, **When** a sign-in is refused, **Then** the entered sign-in name stays in its field and the password field is cleared, as for any failed sign-in attempt (BR-010).

### US3 — Keep the lockout across a system restart (Priority: P2)

As the Admin, I want a lockout and the count of failed sign-in attempts to survive a system restart, so that restarting the system never gives an unauthorised person new guesses.

**Why P2:** US1 already limits guessing while the system runs; this closes the gap when the system restarts, which an unauthorised person rarely controls.

**Independent test:** Cause a lockout, restart the system 5 minutes later, and submit the correct password. It is refused until 15 minutes after the lockout began.

**Acceptance scenarios:**
1. **Given** a lockout that began at time T, **When** the system is restarted at T + 5 minutes and the correct sign-in name and password are submitted at T + 6 minutes, **Then** the sign-in is refused and the sign-in page says it can be tried again in 9 minutes (BR-045, BR-017).
2. **Given** a lockout that began at time T, **When** the system is stopped at T + 5 minutes and started again at T + 20 minutes, **Then** the correct sign-in name and password sign in at once (BR-045).
3. **Given** 3 failed sign-in attempts in a row, **When** the system is restarted and 2 more sign-ins with a wrong password are submitted, **Then** sign-in is locked after the 2nd of them (BR-045).

### US4 — Log failed attempts and lockouts (Priority: P2)

As the Admin (also the system operator), I want failed sign-in attempts, refused attempts and lockouts logged with their time, so that I can see that someone tried to guess the password, without the log revealing anything that was typed.

**Why P2:** The lockout protects sign-in without the log. The log shows that an attack happened and how long it lasted.

**Independent test:** Make 5 failed sign-in attempts and 2 attempts during the lockout, then read the log. It has 5 failed-attempt entries, 1 lockout-start entry and 2 refused-attempt entries, each with its time, and none contains the entered sign-in name or password.

**Acceptance scenarios:**
1. **Given** no lockout, **When** a sign-in with a wrong password is submitted, **Then** one log entry of kind "failed sign-in attempt" is written with its time (BR-019).
2. **Given** 4 failed sign-in attempts in a row, **When** a 5th is submitted, **Then** one "failed sign-in attempt" entry and one "lockout started" entry are written, each with its time (BR-019).
3. **Given** sign-in is locked, **When** a sign-in is submitted, **Then** one log entry of kind "attempt refused during a lockout" is written with its time (BR-019, BR-044).
4. **Given** sign-ins submitted with the sign-in name "Admin-Secret-123" and the password "P@ssw0rd-guess-1", **When** the log and the responses are searched for these values, **Then** neither value is found, and no password hash is found either (BR-019, QE-03).
5. **Given** a failed sign-in attempt with an unknown sign-in name and one with a wrong password, **When** the log is read, **Then** both entries have the same kind and contain no hint of which part was wrong (BR-019, FR-011).

## Edge cases

- A sign-in with an empty or too-long field is rejected at the boundary (spec 014) and is not counted (BR-035). This holds during a lockout too: it gets the boundary message, not the lockout message.
- A sign-in sent over an unencrypted connection is refused and not processed (BR-034, spec 014), so it is not counted and is not logged as a failed sign-in attempt.
- Many sign-ins arrive at the same time, near the 5th failure: the sign-in name and password are checked for no more than 5 of them before the lockout starts (BR-040).
- The correct password is among sign-ins that arrive at the same time while the count is 4: whichever is checked first decides. If the correct one is checked first, it signs in and the count goes back to 0. If a wrong one is checked first, the lockout starts and the correct one is refused (BR-016, BR-018, BR-040).
- A sign-in arrives exactly 15 minutes after the lockout began: the lockout has ended, and the sign-in is checked normally (BR-044).
- The failed sign-in attempts in a row are far apart in time (for example 4 today, 1 next week, with no successful sign-in in between): they still count together; within this feature the count goes back to 0 only after a successful sign-in or when a lockout ends (BR-018). A recovery also resets it (spec 019), and whether a successful password change does is open in spec 018.
- A lockout is active while the Admin is signed in in another browser: that Admin session is not affected (FR-012). A password change during the lockout is refused (BR-036, spec 018).
- The system is stopped during a lockout and started after it would have ended: the lockout is over (BR-045).
- A wrong sign-in is submitted twice by a double click: [NEEDS CLARIFICATION: one failed sign-in attempt or two? See FR-013 and Open question 1.]
- The failure of a sign-in cannot be recorded (unexpected error while saving the count or the lockout): the sign-in does not succeed and no check goes uncounted (FR-015).
- An unauthorised person keeps sign-in locked by sending a wrong guess every 15 minutes: this is the accepted risk "Lockout as denial of service" in R002. The system operator can clear a lockout only through recovery (BR-033, spec 019).

## Functional requirements

- **FR-001:** The system MUST keep one count of failed sign-in attempts in a row for the whole Admin account, whatever sign-in name was entered and whichever browser or source the attempts come from (BR-016). The count, the lockout and the limit of FR-009 cover every check of the Admin password that is a failed sign-in attempt in the R002 glossary, including the wrong current password of a password change that spec 018 adds (BR-030).
- **FR-002:** The system MUST count as a failed sign-in attempt every sign-in, outside a lockout, where the sign-in name or the password does not match the Admin account, and an unknown sign-in name counts the same as a wrong password (BR-016). Sign-ins rejected at the boundary (BR-035) and sign-ins refused because the connection is unencrypted (BR-034) MUST NOT be counted.
- **FR-003:** When the count reaches 5, the system MUST start a lockout at the time of that 5th failed sign-in attempt. The lockout MUST end exactly 15 minutes after it began (BR-016, BR-044).
- **FR-004:** During a lockout, the system MUST refuse every sign-in, even with the correct sign-in name and password, without checking the sign-in name or the password, and MUST NOT start an Admin session. The response MUST be the same whatever was entered (BR-016, BR-040).
- **FR-005:** A sign-in refused during a lockout MUST NOT be counted as a failed sign-in attempt and MUST NOT extend the lockout (BR-044).
- **FR-006:** The count MUST go back to 0 after a successful sign-in and when a lockout ends (BR-018).
- **FR-007:** For the failed sign-in attempt that starts a lockout and for every sign-in refused during it, the sign-in page MUST say that sign-in is temporarily locked and in how many minutes it can be tried again (BR-017). The message MUST NOT say whether the sign-in name or password matched, and the response MUST contain no internal details (BR-010, constitution P7).
- **FR-008:** For failed sign-in attempts that do not start a lockout, the sign-in page MUST show the message of BR-010 (spec 014) unchanged.
- **FR-009:** The limit MUST hold when many sign-ins arrive at the same time: the sign-in name and password MUST be checked for no more than 5 sign-ins in a row without a success before the lockout starts. A sign-in that would be the 6th check in a row without a success MUST be handled as a sign-in during a lockout, and a sign-in that arrives while the 5th check is still running waits for its result (BR-040). This holds for sign-ins and password changes together (FR-001).
- **FR-010:** A system restart MUST NOT change the count or an active lockout. A lockout MUST end 15 minutes after it began even if the system was stopped during that time (BR-045).
- **FR-011:** The system MUST log, with its time and kind, every failed sign-in attempt, every sign-in refused during a lockout, and the start of every lockout (BR-019). A failed sign-in attempt with an unknown sign-in name and one with a wrong password MUST be logged as the same kind.
- **FR-012:** A lockout MUST NOT end any Admin session: a lockout is not among the ways an Admin session ends (R002 glossary "Admin session"). Operations in an active Admin session work as usual, except a password change, which is refused during a lockout (BR-036, spec 018).
- **FR-013:** When the same sign-in with a wrong password is submitted twice (double click), the system MUST count it as [NEEDS CLARIFICATION: one failed sign-in attempt, or two? BR-041 covers only the Admin session, and BR-042 covers only a password change. See Open question 1].
- **FR-014:** Log entries and responses for these events MUST NOT contain the entered sign-in name, the entered password, a password hash or a session secret (BR-019, QE-03).
- **FR-015:** If a failed sign-in attempt or a lockout start cannot be recorded, the sign-in MUST NOT succeed and is answered like the unexpected-error case of spec 014 (its FR-023); the error is logged without entered text. No check of the sign-in name and password may go uncounted (BR-040, constitution P9). A sign-in that reached the system still counts if the connection is lost before the reply.

## Non-functional requirements

- **NFR-001:** With no successful sign-in, the sign-in name and password are checked for at most 20 sign-ins per hour from all sources together, measured over 1 hour of continuous wrong sign-ins (BG-03).
- **NFR-002:** With 0 failed sign-in attempts and 50 sign-ins with a wrong password arriving at the same time, exactly 5 are checked and counted and 45 are refused as sign-ins during a lockout, in 100% of 20 repeated runs (BR-040).
- **NFR-003:** 0 occurrences of an entered password, a password hash or a session secret in log entries or responses for failed sign-in attempts, refused sign-ins and lockout starts, and 0 occurrences of an entered sign-in name in those log entries (QE-03).
- **NFR-004:** A sign-in with an unknown sign-in name and a sign-in with a wrong password give the same message and outcome, including the attempt that starts a lockout and sign-ins during a lockout. Over 100 attempts of each kind, their average response times differ by less than 100 ms, measured separately for checked attempts (in runs of at most 4, each followed by a successful sign-in) and for attempts refused during a lockout (QE-04).

## Key entities

- **Admin account:** the only account in the system (R002 glossary; spec 013). In this feature it also holds the count of failed sign-in attempts in a row and the current lockout, if any. Both survive a system restart (BR-045).
- **Failed sign-in attempt:** a sign-in, outside a lockout, where the sign-in name or the password does not match (R002 glossary). It has a time. A wrong current password in a password change is one too (BR-030), but spec 018 delivers that.
- **Lockout:** a period after 5 failed sign-in attempts in a row during which every sign-in is refused (R002 glossary). It has a start time (the time of the 5th failed sign-in attempt) and ends 15 minutes later.
- **Logged event:** a log entry with a time and a kind: "failed sign-in attempt", "attempt refused during a lockout" or "lockout started". It never contains entered text (BR-019).

## Success criteria

- **SC-001:** In a 1-hour automated run of continuous wrong guesses from several sources, no more than 20 of them are checked against the sign-in name and password (BG-03).
- **SC-002:** 100% of sign-ins made during a lockout are refused, including those with the correct sign-in name and password.
- **SC-003:** After a lockout, the Admin signs in with the correct password on the first try at 15 minutes after the lockout began, in 100% of test runs.
- **SC-004:** After 5 failed sign-in attempts and 2 refused sign-ins, the log has exactly 5 + 1 + 2 matching entries, and a search for the entered texts finds 0 matches.

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BG-03 | NFR-001, SC-001; US1 scenario 9 |
| BR-016 | FR-001, FR-002, FR-003, FR-004; US1 scenarios 3–7 |
| BR-017 | FR-007; US2 scenarios 1, 3; US3 scenario 1 |
| BR-018 | FR-006; US1 scenarios 1, 2, 8; edge case "far apart in time" |
| BR-019 | FR-011, FR-014; US4 scenarios 1–5; SC-004 |
| BR-040 | FR-004, FR-009, FR-015, NFR-002; US1 scenario 9; edge cases on simultaneous sign-ins |
| BR-044 | FR-003, FR-005; US1 scenario 7; US2 scenarios 3, 4; US4 scenario 3 |
| BR-045 | FR-010; US3 scenarios 1–3; Key entities "Admin account" |
| QE-03 | NFR-003, FR-014; US4 scenario 4 |
| QE-04 | NFR-004; US1 scenario 5; US2 scenario 5; US4 scenario 5 |

## Assumptions

- "In how many minutes" is a whole number of minutes, rounded up, and at least 1. For example, 14 minutes 10 seconds left shows "15 minutes", and 20 seconds left shows "1 minute".
- Opening the sign-in page during a lockout without submitting shows no lockout message. The message is shown only in reply to a sign-in (BR-017 says "for every attempt").
- Before a lockout, the sign-in page does not say how many attempts are left. The requirements ask only for the BR-010 message.
- The end of a lockout is not logged as its own event. It follows from the start time (BR-019 lists only the start).
- Log entries hold only the time and kind that BR-019 requires. The source of an attempt is not logged.
- The 2-second response target of QE-01 (spec 014) also applies to sign-ins that are refused or that start a lockout.
- The sign-in prompt (spec 017) uses this same count, lockout and message, because BR-024 applies BR-016 and BR-017 to it.
- NFR numbers are local to this spec: NFR-001 comes from BG-03, NFR-002 from BR-040, NFR-003 from QE-03 and NFR-004 from QE-04.

## Out of scope

- The sign-in page, the BR-010 message, boundary validation (BR-035), encrypted connections (BR-034) and logging successful sign-ins: R002 BL-02, spec 014-admin-sign-in.
- Showing the lockout message in the sign-in prompt over the current view (BR-024): R002 BL-05, spec 017-session-expiry.
- A wrong current password in a password change counting toward the lockout (BR-030), refusing a password change during a lockout (BR-036), and double-submitted password changes (BR-042): R002 BL-06, spec 018-change-password.
- Clearing a lockout and resetting the count through recovery (BR-033): R002 BL-07, spec 019-password-recovery.
- Logging sign-outs: R002 BL-04, spec 016-sign-out.
- Lockout per source of the attempts, CAPTCHA, two-factor authentication, telling the Admin when a lockout happens, a screen of past attempts, and any other way to unlock: not requested (R002 Out of scope; BR-016).

## Open questions

- [ ] 1. When the same sign-in with a wrong password is submitted twice (double click), does it count as one failed sign-in attempt or two? Options: A) one, the same as BR-041 for sessions and BR-042 for password changes; B) two, so every submission that reaches the check counts. Recommended: A, so that a double click cannot bring the Admin closer to a lockout. Either way, the limit of BR-040 still holds. R002 has no rule on this, so the answer may need `/sdd-amend requirements R002`. (affects: FR-013, edge cases; spec 014 edge cases)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-17 | Initial version | — |
