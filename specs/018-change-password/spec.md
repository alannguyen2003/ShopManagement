# Spec: Change password

**Feature:** 018-change-password
**Branch:** feat/018-change-password
**Source:** R002/BL-06
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-17

## Context

The Admin account starts with an initial password that the system operator put in the setup values (013-admin-account-setup, R002 BL-01). That password may still be readable in the environment the system runs in, or other people may come to know it. Otherwise it can be replaced only by a recovery through the system operator (019-password-recovery, R002 BL-07), if available. This feature lets the signed-in Admin change the password at any time, ends the Admin sessions in other browsers, and makes wrong guesses of the current password count toward the lockout. That way an unattended signed-in browser cannot be used to guess the password or replace it. The change is never forced (R002 BR-032).

## User stories

### US1 — Change the password (Priority: P1)

As the Admin, I want to replace my password while I am signed in, so that the initial password, or a password others may know, stops working.

**Why P1:** This is the value of the feature. Without it, the initial password stays valid until a recovery.

**Independent test:** Sign in, change the password, then try to sign in from a second browser. The old password is refused and the new one signs in.

**Acceptance scenarios:**
1. **Given** the Admin is signed in and sign-in is not locked, **When** the Admin enters the correct current password, enters a valid new password (12 to 128 characters) twice and saves, **Then** the password is changed, a confirmation is shown, the three password fields are cleared, and the Admin stays signed in on the same view (BR-028, BR-031).
2. **Given** the password has been changed, **When** anyone signs in with the old password, **Then** the sign-in is refused as a failed sign-in attempt; **When** they sign in with the new password, **Then** sign-in succeeds (BR-028).
3. **Given** the Admin is signed in on browser A and on browser B, **When** the Admin changes the password in browser A, **Then** browser A's Admin session stays active, and the next action in browser B is refused because the Admin session has ended (BR-031, QE-05).
4. **Given** the Admin has two tabs open in browser A, **When** the Admin changes the password in one tab, **Then** the next action in the other tab succeeds, because both tabs share the current Admin session (BR-031).
5. **Given** the Admin changes the password to a new password that begins and ends with spaces, **When** the Admin later signs in with exactly that value, **Then** sign-in succeeds; **When** the Admin signs in with the same value without the spaces, **Then** sign-in is refused (BR-007; R002 BR-006).
6. **Given** a successful change, **When** the stored Admin account is inspected, **Then** the password is stored only as a salted slow hash that meets constitution P4, its salt differs from the salt of the previous password, and no readable password is stored anywhere (BR-008, BG-04).
7. **Given** the Admin submits a valid change twice (double click), **When** both submissions arrive, **Then** the password is changed once, one confirmation is shown, the repeated submission is not counted as a failed sign-in attempt, and the log shows one successful password change (BR-042).
8. **Given** a successful change, **When** the log is inspected, **Then** it holds one event with the time and the kind "password changed", and it contains no password, password hash or session secret (BR-019, QE-03).
9. **Given** the Admin signs in for the first time with the initial password, **When** sign-in succeeds, **Then** the product management page is shown straight away, and nothing asks or forces the Admin to change the password (BR-032).
10. **Given** the password has been changed and the initial password is still in the setup values, **When** the system restarts, **Then** the new password still signs in, the old one is refused, and the Admin sessions ended by the change stay ended (R002 BR-004, BR-045).

### US2 — Refuse wrong or unsafe changes (Priority: P1)

As the Admin, I want a change to be refused when the entries are wrong, and wrong guesses of the current password to count toward the lockout, so that someone at my unattended browser cannot guess or replace my password.

**Why P1:** Without this story, an open Admin session would allow unlimited online password guessing, which breaks R002 BG-03 and constitution P5.

**Independent test:** With a signed-in Admin session, submit each kind of invalid change. Check that the password is unchanged, the reason is shown on the right field, and the count of failed sign-in attempts goes up only for a wrong current password.

**Acceptance scenarios:**
1. **Given** the Admin is signed in, **When** the Admin saves a change with a wrong current password and a valid new password entered twice, **Then** the change is refused with a message on the current password field, the password is unchanged, the attempt counts as a failed sign-in attempt, and the log shows a refused password change and a failed sign-in attempt, without any password (BR-029, BR-030, BR-019).
2. **Given** 3 failed sign-in attempts in a row have been made on the sign-in page, **When** the Admin saves 2 changes in a row with a wrong current password, **Then** the second refusal says sign-in is temporarily locked and can be tried again in 15 minutes, and a sign-in from another browser with the correct password is then refused (BR-030; R002 BR-016, BR-017).
3. **Given** sign-in is locked, **When** the Admin saves a change with the correct current password and a valid new password, **Then** the change is refused with a message that says it is temporarily locked and in how many minutes it can be tried again. The current password is not checked, the password is unchanged, the attempt is not counted as a failed sign-in attempt, the lockout still ends 15 minutes after it began, and the refusal is logged (BR-036, BR-019; R002 BR-044).
4. **Given** the Admin is signed in, **When** the two new password entries differ, **Then** the change is refused with a message on the confirmation field, the password is unchanged, and the attempt is not counted as a failed sign-in attempt (BR-029).
5. **Given** the Admin is signed in, **When** the new password is 11 characters long, **Then** the change is refused with a message on the new password field saying it needs at least 12 characters; the password is unchanged and the attempt is not counted (BR-029, BR-007).
6. **Given** the Admin is signed in, **When** the new password is 129 characters long, **Then** it is rejected at the boundary as too long, before the current password is checked; the password is unchanged and the attempt is not counted (BR-007, QE-07).
7. **Given** the Admin is signed in, **When** the new password is 12 lowercase letters with no digits or symbols, **Then** the change succeeds, because there are no composition rules (BR-007).
8. **Given** the Admin is signed in, **When** the new password is the same as the current password, **Then** the change is refused with a message on the new password field saying it must differ from the current password; the password is unchanged and the attempt is not counted (BR-029).
9. **Given** the Admin is signed in, **When** any of the three fields is empty, **Then** the change is rejected at the boundary with a message on that field, before the current password is checked; the password is unchanged and the attempt is not counted (QE-07).
10. **Given** the Admin is signed in, **When** a change with the correct current password is sent over an unencrypted connection, **Then** it is refused and not processed: the current password is not checked, the password is unchanged and nothing is counted (BR-034).
11. **Given** an Admin session has ended (signed out, expired, or ended by a password change in another browser), **When** a change is sent with that session, **Then** it is refused as an ended session and not processed: the current password is not checked, the password is unchanged and nothing is counted (R002 BR-023, QE-05).
12. **Given** any refused change, **When** the response to the Admin and the log are inspected, **Then** neither contains an entered password, a password hash, a session secret or internal detail (BR-008, QE-03).
13. **Given** the Admin submits a change with a wrong current password twice (double click), **When** both submissions arrive, **Then** the change is refused once and counted as one failed sign-in attempt, not two (BR-042).
14. **Given** 0 failed sign-in attempts, **When** 3 changes with a wrong current password and 5 sign-ins with a wrong password arrive at the same time, **Then** the password is checked exactly 5 times, sign-in is locked, and the rest are refused as during a lockout, are not counted and do not extend it (BR-030; R002 BR-040; 015 FR-009).

## Edge cases

- The Admin session expires (idle timeout or maximum session length) while the change password form is open → saving is refused as an ended session, and the sign-in prompt opens over the current view (017-session-expiry, R002 BL-05). The change is not processed and not counted. What happens to the typed passwords: [NEEDS CLARIFICATION: after signing in again in the sign-in prompt, does the change password form keep the three typed passwords, or does it stay open with the password fields cleared? See Open question 2.]
- The password is changed in browser B while browser A has the change password form open → A's save is refused as an ended session, not processed and not counted. After signing in again with the new password, A can change it again.
- Two changes, each with the correct current password, are submitted at almost the same moment from two browsers → they are handled one at a time. The first succeeds and ends the other session, so the second is refused as an ended session (FR-016).
- A sign-in with the old password in browser B arrives while the change in browser A is being made → it either starts before the change and its Admin session is then ended, or it is refused; no Admin session proven with the old password survives the change (FR-016). A request from another browser that arrives after the change has finished is refused; one that finished before it is not affected.
- Many changes with a wrong current password arrive at the same time (scripted) → the lockout limit holds across sign-in attempts and changes together. No more than 5 checks of the password are made before sign-in is locked (R002 BR-040; 015 FR-001 and FR-009 cover sign-ins and password changes together; US2 scenario 14).
- The wrong current password that starts a lockout is entered by a signed-in Admin → the refusal shows the lockout message, and the current Admin session continues: a lockout is not among the ways an Admin session ends (R002 glossary "Admin session"; 015 FR-012).
- A successful change is made while the count of failed sign-in attempts in a row is above 0 → [NEEDS CLARIFICATION: see Open question 3].
- A new password made only of spaces, of non-Latin letters or of symbols, 12 to 128 characters long → accepted, because any characters are allowed (BR-007). Spaces are kept and counted.
- The Admin presses Save again with the same values after seeing the result, outside the repeat window of FR-011 → not a repeat; processed normally. After a valid change was applied, the current password entered is now wrong, so it is counted.
- The new password is one the Admin used before (but not the current one) → accepted; there is no password history.
- A current password over 128 characters → rejected at the boundary as too long, not counted (it can never match, by BR-007).
- The system restarts right after a change → the new password and the ended sessions stay in force (FR-015).
- The Admin tries to change the password from the sign-in page, without an Admin session → not possible. The change requires a signed-in Admin session (FR-001, R002 BR-014).

## Functional requirements

- **FR-001:** The signed-in Admin MUST be able to open the change password form at any time while the Admin session is active (BR-028). The form MUST NOT be reachable without a signed-in Admin session (R002 BR-014). [NEEDS CLARIFICATION: where is the change password form reached, and does it open over the current view or as its own page? See Open question 1.]
- **FR-002:** The change password form MUST ask for the current password, the new password, and the new password again to confirm it (BR-028).
- **FR-003:** All three password fields MUST always be masked, and the system MUST NOT show, return or reveal any entered password in readable form, including in error messages (BR-008).
- **FR-004:** The system MUST reject a change at the boundary when any field is empty or longer than 128 characters. The rejection happens before the current password is checked, with a message on the field. The password stays unchanged and the attempt is not counted as a failed sign-in attempt (QE-07; assumption by analogy with R002 BR-035).
- **FR-005:** While sign-in is locked (R002 BR-016), the system MUST refuse every change, even one with the correct current password, without checking the current password. The refusal MUST say the change is temporarily locked and in how many minutes it can be tried again. A refused change during a lockout is not counted as a failed sign-in attempt and does not extend the lockout (BR-036; R002 BR-044).
- **FR-006:** The system MUST refuse a change, keep the password unchanged and show the reason on the field in each of these cases: the two new password entries differ; the new password is shorter than 12 or longer than 128 characters (any characters allowed, no composition rules, spaces kept and counted); or the new password equals the entered current password (BR-029, BR-007). These checks MUST be made without checking the current password, and a change refused by them is not counted as a failed sign-in attempt (assumption).
- **FR-007:** The system MUST refuse a change whose current password does not exactly match the Admin's password (case-sensitive, spaces kept; R002 BR-006). It MUST keep the password unchanged, show the reason on the current password field, and count the attempt as a failed sign-in attempt in the same count as sign-in (BR-029, BR-030). When this failure is the 5th in a row, the lockout starts (R002 BR-016), and the refusal MUST say sign-in is temporarily locked and in how many minutes it can be tried again (assumption, as R002 BR-017).
- **FR-008:** On a successful change, the system MUST replace the stored password with the new password, stored only as a salted slow hash with a new unique salt, as constitution P4 requires (BR-008, BG-04). From then on, only the new password signs in.
- **FR-009:** On a successful change, the system MUST at once end every other Admin session (every other browser), and keep the current Admin session active. Other tabs of the same browser share the current Admin session and stay signed in (BR-031; R002 BR-015). Every request later made with an ended session MUST be refused (R002 BR-023, QE-05).
- **FR-010:** On a successful change, the system MUST confirm it to the Admin and clear the three password fields. The Admin stays on the same view (assumption).
- **FR-011:** One change submission MUST be processed once (BR-042). A submission is a repeat only when it comes from the same Admin session with exactly the same three entered values as an earlier submission, within [NEEDS CLARIFICATION: how long; see Open question 4]. A repeat MUST NOT check the current password again, change the password, count as a failed sign-in attempt or be logged as another change; it gets the same outcome as the original submission. Any other submission is processed and counted normally.
- **FR-012:** The change MUST be accepted only over an encrypted connection. A change sent over an unencrypted connection MUST be refused and not processed: the current password is not checked, nothing is counted, and the password is unchanged (BR-034).
- **FR-013:** A change sent without an Admin session or with an ended session MUST be refused and not processed: the current password is not checked, nothing is counted, and the password is unchanged (R002 BR-014, BR-023).
- **FR-014:** The system MUST NOT force or require a password change at any time, including at the first sign-in with the initial password. After a successful sign-in the Admin goes straight to the product management page (BR-032).
- **FR-015:** A changed password, and the Admin sessions it ended, MUST stay in force after a system restart, even if the initial password is still in the setup values (R002 BR-004, BR-045).
- **FR-016:** Changes MUST be handled one at a time. When two changes arrive at the same time, at most one succeeds, and the other is handled after it under FR-009 and FR-013. A change and a sign-in that arrive at the same time are handled one after the other: a sign-in checked against the old password either starts its Admin session before the change, and that session is then ended under FR-009, or is refused. No Admin session started with the old password stays active after the change (BR-031; assumption).
- **FR-017:** The system MUST log every successful change and every refused change received with a valid Admin session over an encrypted connection, with its time and kind: changed, or refused with the kind of reason (rejected at the boundary, locked, rule not met, wrong current password). Requests refused for no session, an ended session or an unencrypted connection never reach the change; they are handled as in 014 (BR-014, BR-034) with no password change event. A wrong current password is also logged as a failed sign-in attempt, and as the start of a lockout when it starts one (BR-019). Logs MUST NOT contain any entered password, any password hash or any session secret (BR-019, QE-03).
- **FR-018:** A refused change MUST NOT return any entered password, password hash, session secret or internal detail (BR-008; constitution P7).

## Non-functional requirements

- **NFR-002:** US2 scenario 14 gives exactly 5 password checks in 100% of 20 repeated runs (BR-030; R002 BR-040).
- **NFR-003:** 0 occurrences of an entered password (current, new or confirmation), a password hash or a session secret in logs, responses to the Admin or committed configuration. Checked automatically after a successful change and after each kind of refused change in FR-017 (QE-03).
- **NFR-005:** 100% of requests made with an Admin session ended by a password change are refused. Checked automatically for a page and for an operation in another browser after a successful change (QE-05).
- **NFR-006:** On the change password form, 100% of the fields and buttons can be reached and used with the keyboard alone and have a visible label. Every error message is shown next to the field it concerns, or next to the form when it concerns no single field (QE-06).
- **NFR-007:** 100% of change submissions with an empty field or a field over 128 characters are rejected at the boundary with a message before the current password is checked. Checked automatically for each of the three fields (QE-07).

## Key entities

- **Admin account:** the only account (R002 BR-001). It holds the sign-in name, which a change leaves unchanged, and the password, stored only as a salted slow hash. A successful change replaces the password.
- **Password:** the secret typed with the sign-in name. In this feature it is entered as the current password and the new password (twice). It is never stored, shown or logged in readable form.
- **Admin session:** an Admin's signed-in time in one browser. A successful change ends every Admin session except the current one.
- **Failed sign-in attempt:** a sign-in, or a change, where the password does not match. The count of failed sign-in attempts in a row is shared with sign-in (015-sign-in-lockout).
- **Lockout:** the period during which sign-in is refused (015-sign-in-lockout). During a lockout a change is refused as well.
- **Password change event:** a logged event with its time and kind (changed, or refused with the kind of reason). It never contains a password, a hash or a session secret.

## Success criteria

- **SC-001:** After a successful change, 100% of sign-ins with the old password are refused and 100% of sign-ins with the new password succeed.
- **SC-002:** After a successful change, 100% of the Admin sessions in other browsers are refused at their next request, and the current one keeps working.
- **SC-003:** 100% of changes with a wrong current password count as failed sign-in attempts. A total of 5 failures in a row, from sign-in and changes together, locks sign-in.
- **SC-004:** 0 entered passwords, password hashes or session secrets appear in logs or in responses to the Admin, across all successful and refused changes in the automated tests.

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BG-04 | FR-008, FR-017, FR-018, NFR-003; US1 scenario 6; SC-004 |
| BR-007 | FR-006 (12–128, any characters, no composition rules); US1 scenario 5; US2 scenarios 5, 6, 7; Edge cases (spaces only, non-Latin) |
| BR-008 | FR-003, FR-008, FR-018; US1 scenario 6; US2 scenario 12 |
| BR-019 | FR-017; US1 scenario 8; US2 scenarios 1, 3 |
| BR-028 | FR-001, FR-002; US1 scenarios 1, 2 |
| BR-029 | FR-006, FR-007; US2 scenarios 1, 4, 5, 8 |
| BR-030 | FR-007; US2 scenarios 1, 2, 14; NFR-002; SC-003 |
| BR-031 | FR-009; US1 scenarios 3, 4; SC-002 |
| BR-032 | FR-014; US1 scenario 9 |
| BR-034 | FR-012; US2 scenario 10 |
| BR-036 | FR-005; US2 scenario 3 |
| BR-042 | FR-011; US1 scenario 7; US2 scenario 13 |
| QE-03 | NFR-003; FR-017, FR-018; US1 scenario 8; US2 scenario 12 |
| QE-05 | NFR-005; FR-009, FR-013; US1 scenario 3; US2 scenario 11 |
| QE-06 | NFR-006 |
| QE-07 | NFR-007; FR-004; US2 scenarios 6, 9 |

## Assumptions

- All checks that do not need the stored password are made before the current password is checked: empty or too-long fields, the lockout, the new password length, the two new entries matching, and the new password differing from the entered current password. A change refused by these checks is not a failed sign-in attempt, and every check of the current password is counted, so no guess goes uncounted.
- Input rejected at the boundary in a change is not a failed sign-in attempt, by analogy with R002 BR-035, which is written for sign-in.
- The refusal during a lockout, and the refusal that starts a lockout, give the same information as on the sign-in page: locked, and in how many minutes it can be tried again (R002 BR-017).
- After a successful change, the Admin stays on the same view, sees a confirmation, and the password fields are cleared.
- A change is Admin activity, so it resets the idle timeout (R002 BR-043). The maximum session length of the current Admin session keeps counting from its original sign-in.
- Every refused change that reaches the change (valid Admin session, encrypted connection) is logged, including boundary rejections, with the kind of reason and never with a value.
- Pasting into the password fields is allowed, so password managers can be used.
- There is no password history: only the current password is refused as a new password.
- No notification is sent when the password changes. Nothing in R002 asks for one, and the system cannot send mail.
- How characters are counted for the 12–128 length is the same as at Admin account setup (013-admin-account-setup).
- There is no separate response-time target for a change. R002 QE-01 covers sign-in only.
- NFR numbers repeat the number of the R002 quality expectation they come from (QE-03, QE-05, QE-06, QE-07).

## Out of scope

- Admin account setup and the initial password: R002 BL-01 (013-admin-account-setup).
- The sign-in page, the generic failure message, protection of every page and operation, and the encrypted-connection rule in general: R002 BL-02 (014-admin-sign-in). This feature only applies them to the change.
- The lockout itself (threshold, duration, count reset, the lockout message on the sign-in page, simultaneous attempts, restart behavior): R002 BL-03 (015-sign-in-lockout). This feature only adds a wrong current password to the count and refuses changes during a lockout.
- Sign-out: R002 BL-04 (016-sign-out).
- Session expiry and the sign-in prompt that other browsers see after their session ends: R002 BL-05 (017-session-expiry).
- Recovering a forgotten password through the system operator: R002 BL-07 (019-password-recovery).
- Changing the sign-in name (R002 out of scope).
- Forcing or reminding the Admin to change the initial password (R002 BR-032).
- A list of active Admin sessions, or signing out other browsers without changing the password (R002 out of scope).
- Password strength meters, composition rules and password history (R002 BR-007).
- Email or other notifications of a change, and email-based reset (R002 out of scope).
- Any product content (R001).

## Open questions

- [ ] 1. Where is the change password form reached, and does it open over the current view or as its own page? Recommended: a "Change password" action wherever sign-out is available (every page except the sign-in page, R002 BR-026), with the form opening over the current view so an open product form is not lost. (affects: FR-001, NFR-006)
- [ ] 2. When the Admin session ends while the change password form is open and the Admin signs in again in the sign-in prompt, should the form keep the three typed passwords, or stay open with the password fields cleared? Recommended: clear the password fields. Retyping is cheap, and typed passwords stay in the page for less time. R002 BR-024 / R001 BR-049 were written for product forms. (affects: Edge cases, FR-003)
- [ ] 3. Does a successful password change reset the count of failed sign-in attempts in a row to 0, as a successful sign-in does (R002 BR-018)? Recommended: yes. A successful change proves the Admin knows the current password, just as a sign-in does. R002 BR-018 names only sign-in and the end of a lockout. Spec 019 FR-007 makes the same kind of extension (a reset at recovery); both should follow one policy: either both are spec assumptions, or one `/sdd-amend requirements R002` adds both to BR-018. (affects: FR-007, FR-008, Edge cases; spec 019 FR-007)
- [ ] 4. How long does a submission with exactly the same three values from the same Admin session count as a repeat of the earlier one (FR-011)? Recommended: until the result of the earlier submission has been shown, and at most 10 seconds. Longer windows would let a repeated guess go uncounted. (affects: FR-011, US1 scenario 7, US2 scenario 13)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-17 | Initial version | — |
