# Requirements: Admin sign-in

**Requirements:** R002-admin-sign-in
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-16
**Requested by:** alannguyen2003 (shop Admin / product owner)
**Input:** [input.md](input.md)

## Context

**Stated by the requester (input.md):** "I want to create a feature to let the user can sign in the system in order to perform action in Requirement R001-product-management". Nothing else is stated: no credentials, session rules, lockout figures, sign-out, password change or recovery.

**Interpretation:** "the user" means the single Admin (constitution P1; R001 glossary). R001 lists Admin sign-in as out of scope and assigns it to this document. It also says every R001 page and operation needs a signed-in Admin (R001 BR-015, QE-03), so R002 must be built before R001 BL-01. The constitution sets the rest of the frame: exactly one Admin account and no self-registration (P1), a slow salted password hash (P4), authentication by default with brute-force protection (P5), validation at the boundary (P6) and no secrets in code, logs or error responses (P7). Success means the Admin signs in quickly and reaches the product management page, nobody else can reach any product page or operation, and guessing the password online is impractical.

## Stakeholders and actors

| Actor / stakeholder | Role | Goals | Interaction with the system |
|---------------------|------|-------|-----------------------------|
| Admin | The only user of the system (constitution P1); signs in to manage products (R001) | Reach the product management page quickly; stay signed in while working; not lose work when a session ends | Uses |
| System operator | The person who installs and runs the system and sets up the Admin account outside the application. Assumed to be the same person as the Admin | Set up the Admin account once, safely; regain access if the password is forgotten | Uses (outside the application) |
| Shop owner / product owner (alannguyen2003) | Requester; same person as the Admin | Get a working, safe sign-in before the product features | Approves |
| Unauthorised person | Anyone else who can reach the sign-in page or use the Admin's browser | (Threat) reach product data or lock the Admin out | None (must be kept out) |

## Business goals

- **BG-01:** With the correct sign-in name and password, the Admin gets from the sign-in page to the product management page in under 30 seconds. *(Target is an assumption.)*
- **BG-02:** 0 product pages or product operations can be reached without a signed-in Admin session. *(Input "in order to perform action in R001"; constitution P5.)*
- **BG-03:** Online password guessing is limited to at most 20 attempts per hour, from all sources together. This follows from BR-016 (5 attempts, then 15 minutes locked). *(Constitution P5.)*
- **BG-04:** 0 readable Admin passwords in stored data, logs, error responses or committed configuration. *(Constitution P4, P7.)*

## Scope

### In scope

- Setting up the single Admin account outside the application, by the system operator.
- Signing in with a sign-in name and a password, and landing on the product management page.
- Requiring a signed-in Admin session for every page and operation except the sign-in page and the sign-in operation.
- Locking sign-in after repeated failed sign-in attempts.
- Signing out. *(Assumption: not in the input, nor required by P5; recommended for a shared computer.)*
- Ending an Admin session after an idle timeout and a maximum session length, and letting the Admin sign in again without losing an open form (R001 BR-049).
- Changing the Admin password while signed in. *(Pending Open question 2.)*
- Recovering access when the password is forgotten, through the system operator and the setup values.

### Out of scope

- Self-registration, more user accounts, roles or permissions: forbidden by constitution P1.
- Changing the sign-in name: not requested.
- Two-factor authentication, sign-in with external providers, CAPTCHA: not requested.
- Password reset by email or security questions: not requested, and the allowed dependency list has no way to send mail (see Constraints). Recovery is through the system operator (BR-033).
- A list of active sessions or signing out other devices on request: not requested. A password change ends the other sessions (BR-031).
- An audit trail of Admin actions beyond failed sign-ins and lockouts (BR-019): not requested.
- Any product feature: covered by R001.

## Glossary

| Term | Definition | Do not call it |
|------|------------|----------------|
| Admin | The single person who signs in and manages products (constitution P1). Same as R001 | user, manager, operator, staff |
| Admin account | The only account in the system: the Admin's sign-in name and password | user account, profile, login, user |
| Sign-in name | What the Admin types to say who they are when signing in. [NEEDS CLARIFICATION: a username chosen at setup, or an email address?] | username, login, user ID, email (until decided) |
| Password | The secret the Admin types with the sign-in name. Stored only as a salted slow hash (P4) | passcode, PIN, credentials |
| Initial password | The password given at Admin account setup | default password, temporary password |
| Admin account setup | The one-time creation of the Admin account at system start, from setup values the system operator provides, outside the application | registration, sign-up, onboarding |
| Setup values | The sign-in name and initial password the system operator puts in the environment the system runs in (never in committed files), read at system start. For recovery, also the reset indication and the new password | default credentials, seed data, config file |
| Reset indication | An explicit setup value that tells the system, at start, to replace the Admin password with the new password in the setup values (BR-033) | reset flag (in user-facing text), override |
| Sign in | Prove identity with the sign-in name and password, which starts an Admin session | log in, login, log on, authenticate |
| Sign-in page | The page where the Admin signs in. It is the only page available without an Admin session | login page, login screen, home page |
| Admin session | The time during which the Admin is signed in, in one browser. It starts at sign-in and ends at sign-out, at expiry or at a password change | login, token, cookie, connection |
| Signed-in Admin | The Admin while an Admin session is active. Same as R001 | logged-in user, authenticated user |
| Sign out | End the current Admin session on purpose | log out, log off, disconnect |
| Failed sign-in attempt | A sign-in where the sign-in name or the password does not match | wrong login, bad attempt, error |
| Lockout | A period after repeated failed sign-in attempts during which every sign-in is refused | block, ban, suspension, account disabled |
| Admin activity | An operation the Admin starts (opening a page, saving, signing out). Automatic requests made by the page itself do not count (BR-043) | usage, traffic |
| Idle timeout | How long an Admin session survives with no Admin activity | inactivity limit, session timeout (alone) |
| Maximum session length | The longest an Admin session can last after sign-in, even with activity | absolute timeout, session lifetime |
| Expired session | An Admin session ended by the idle timeout or the maximum session length. Same as R001 BR-049 | timed-out login, stale session |
| Ended session | An Admin session that is no longer valid for any reason: signed out, expired or ended by a password change | dead session, invalid login |
| Product management page | The single-page interface where the Admin does all product operations. Same as R001 | dashboard, admin panel, back office |
| System operator | The person who installs and runs the system and does Admin account setup | administrator (to avoid confusion with Admin), IT, host |

## Business processes

### PR-01 — Set up the Admin account

1. The system operator provides the sign-in name and the initial password in the setup values of the environment where the system runs, outside the application and never in committed files (BR-002).
2. The system starts and finds no Admin account.
3. The system checks the setup values (BR-005, BR-007) and creates the Admin account. It stores the password only as a salted slow hash (BR-008).
4. The system operator removes the initial password from the setup values (recommended).

**Exceptions:**
- No Admin account exists and the setup values are missing or invalid → the system does not start. It logs which setting is missing or invalid, never its value (BR-003).
- An Admin account already exists → the setup values are ignored and the account is unchanged (BR-004), unless the operator is doing PR-07.

### PR-02 — Sign in

1. The Admin opens the system without an Admin session.
2. The system shows the sign-in page.
3. The Admin enters the sign-in name and password and submits.
4. The system checks the input at the boundary, then checks the sign-in name and password and that sign-in is not locked.
5. The system starts an Admin session and shows the page the Admin was trying to open, or the product management page (BR-011, BR-012).

**Exceptions:**
- A field is empty or too long → rejected at the boundary with a message on the field. It does not count as a failed sign-in attempt (BR-035).
- The same sign-in is submitted twice (double click) → at most one Admin session is started (BR-041).
- Unknown sign-in name or wrong password → the same message either way. The sign-in name stays in the field, the password field is cleared, and the attempt counts as a failed sign-in attempt (BR-010, BR-016).
- Sign-in is locked → refused even with the correct password. The page says when to try again (BR-016, BR-017).
- The Admin is already signed in and opens the sign-in page → the product management page is shown (BR-013).
- The requested destination is outside the system → ignored, and the product management page is shown (BR-012).

### PR-03 — Reach a page or operation without an Admin session

1. Someone without an Admin session opens a page of the system or calls an operation.
2. For a page, the system shows the sign-in page and remembers the requested page (BR-011). For an operation, the system refuses it and returns no data (BR-014).

**Exceptions:**
- An ended session is used → treated as no session (BR-023).

### PR-04 — Sign out

1. The signed-in Admin chooses sign out on any page.
2. The system ends the Admin session at once and shows the sign-in page (BR-026, BR-027).

**Exceptions:**
- Another tab of the same browser is still open → its next action is refused, and it offers to sign in again (BR-024).

### PR-05 — Admin session ends while working

1. The Admin's session expires (idle timeout or maximum session length), or it is ended from another tab.
2. The Admin does something, for example saves the add form (R001 PR-01).
3. The system refuses the operation and says the Admin session has ended.
4. The system offers to sign in again without leaving the current view. The open form keeps its values (R001 BR-049; BR-024).
5. The Admin signs in again and stays on the same view, then retries the operation (BR-025).

**Exceptions:**
- The sign-in again fails or sign-in is locked → same as PR-02. The form stays open with its values.
- The Admin closes the sign-in prompt instead of signing in → the Admin stays on the same view and the form stays open with its values. The next refused action prompts again (BR-038).

### PR-06 — Change the password

1. The signed-in Admin opens the change password form.
2. The Admin enters the current password and the new password twice, then saves.
3. The system checks the input, changes the password, ends every other Admin session and confirms (BR-028, BR-031).

**Exceptions:**
- Wrong current password → refused. The password is unchanged, and the attempt counts as a failed sign-in attempt (BR-029, BR-030).
- The two new entries differ, the new password breaks BR-007, or it equals the current password → refused with the reason on the field. The password is unchanged (BR-029).
- Sign-in is locked (BR-016, BR-030) → the change is refused until the lockout ends (BR-036).
- The same change is submitted twice (double click) → it is processed once; the repeated submission is not counted as a failed sign-in attempt (BR-042).

### PR-07 — Recover a forgotten password

1. The Admin cannot sign in because the password is forgotten.
2. The system operator puts a new password and an explicit reset indication in the setup values, and restarts the system.
3. At start, the system replaces the password, clears any lockout and ends every Admin session (BR-033).
4. The system operator removes the reset indication and the new password from the setup values (recommended).
5. The Admin signs in with the new password (PR-02).

**Exceptions:**
- The new password breaks BR-007 → the system does not start, and it logs the reason without the value. The old password stays valid (BR-037).

## Business rules

- **BR-001:** The system has exactly one Admin account. There is no way to register, create another account or assign roles. *(Source: constitution P1)*
- **BR-002:** The Admin account is created by Admin account setup, outside the application: when the system starts and no Admin account exists, it creates the account from setup values (sign-in name and initial password) that the system operator puts in the environment the system runs in. Setup values never come from committed files, and there is no setup page or separate setup command. *(Source: constitution P1, P7; clarification 2026-10-16)*
- **BR-003:** The system never creates an Admin account with a built-in or default password. If no Admin account exists and the setup values are missing or invalid, the system does not start, and it logs which setting is missing without its value. *(Source: constitution P7, P9; "does not start" is an assumption)*
- **BR-004:** Once the Admin account exists, the setup values do not change it. The only exception is the explicit recovery of BR-033. *(Source: assumption)*
- **BR-005:** The Admin signs in with a sign-in name and a password. The sign-in name is 1 to 100 characters long. [NEEDS CLARIFICATION: a username or an email address?] *(Source: input "sign in"; length is an assumption)*
- **BR-006:** Matching the sign-in name ignores letter case and leading/trailing spaces. The password must match exactly: case-sensitive, with spaces kept. *(Source: assumption)*
- **BR-007:** A password is at least [NEEDS CLARIFICATION: minimum length, recommended 12] and at most 128 characters long. Any characters are allowed, and there are no composition rules. This applies at setup, at a change and at recovery. *(Source: assumption)*
- **BR-008:** The password is stored only as a salted slow hash, as constitution P4 requires. It is never stored, shown, logged or returned in readable form, and the password field is always masked. *(Source: constitution P4, P7)*
- **BR-009:** A sign-in succeeds only when the sign-in name and the password both match the Admin account and sign-in is not locked (BR-016). *(Source: input; constitution P5)*
- **BR-010:** A failed sign-in attempt shows the same message, for example "Sign-in name or password is incorrect", whether the sign-in name is unknown or the password is wrong. It does not say which one was wrong. The entered sign-in name stays and the password field is cleared. *(Source: assumption, security)*
- **BR-011:** After a successful sign-in, the Admin sees the page they were trying to open when the sign-in page appeared. Otherwise they see the product management page. *(Source: assumption)*
- **BR-012:** The page shown after sign-in is always a page of this system. A destination outside the system is ignored, and the product management page is shown instead. *(Source: assumption, security)*
- **BR-013:** A signed-in Admin who opens the sign-in page is taken to the product management page. *(Source: assumption)*
- **BR-014:** Every page and every operation requires a signed-in Admin session, except the sign-in page and the sign-in operation. Without a session, an operation is refused and returns no data or internal detail, and a page shows the sign-in page. This includes the health check and the OpenAPI document available in Development: they require a signed-in Admin session like every other operation. *(Source: constitution P5, P7; input "in order to perform action in R001"; R001 BR-015; clarification 2026-10-16)*
- **BR-015:** The Admin may be signed in from several browsers at the same time, and each one has its own Admin session. *(Source: assumption; Open question 12)*
- **BR-016:** After 5 failed sign-in attempts in a row, sign-in is locked for 15 minutes. The count and the lockout apply to the whole Admin account, whatever sign-in name was entered and wherever the attempts come from. During a lockout, every sign-in attempt is refused, even with the correct password. *(Source: constitution P5; clarification 2026-10-16)*
- **BR-017:** During a lockout, the sign-in page says sign-in is temporarily locked and in how many minutes it can be tried again. *(Source: assumption)*
- **BR-018:** The count of failed sign-in attempts in a row goes back to 0 after a successful sign-in and when a lockout ends. *(Source: assumption)*
- **BR-019:** Every failed sign-in attempt and every lockout is logged with its time. The entered password is never logged. *(Source: constitution P7, P9; assumption)*
- **BR-020:** An Admin session ends after [NEEDS CLARIFICATION: idle timeout, recommended 30 minutes] with no Admin activity. *(Source: assumption)*
- **BR-021:** An Admin session ends at the latest [NEEDS CLARIFICATION: maximum session length, recommended 12 hours] after sign-in, even with activity. *(Source: assumption)*
- **BR-022:** There is no "keep me signed in" option. An Admin session also ends when the browser is closed. [NEEDS CLARIFICATION: confirm no "keep me signed in"] *(Source: assumption)*
- **BR-023:** An ended session can never be used again. Every request made with it is refused, as in BR-014. *(Source: constitution P5)*
- **BR-024:** When an operation is refused because the Admin session has ended, the Admin is told so and can sign in again without leaving the current view, so that an open form keeps its values as R001 BR-049 requires. [NEEDS CLARIFICATION: confirm a sign-in prompt over the current view rather than going to the sign-in page] *(Source: R001 BR-049; presentation is an assumption)*
- **BR-025:** After signing in again (BR-024), the Admin stays on the same view. The refused operation is not repeated automatically; the Admin retries it. *(Source: R001 BR-049 "so the Admin can retry"; assumption)*
- **BR-026:** The Admin can sign out from every page of the system except the sign-in page. *(Source: assumption)*
- **BR-027:** Signing out ends the Admin session at once, with no confirmation, and shows the sign-in page. Other open tabs of the same browser are refused on their next action (BR-024). *(Source: assumption)*
- **BR-028:** The signed-in Admin can change the password by entering the current password and the new password twice. *(Source: assumption; Open question 2)*
- **BR-029:** A password change is refused, the password is unchanged and the reason is shown when any of these is true: the current password is wrong, the two new entries differ, the new password breaks BR-007, or the new password is the same as the current one. *(Source: assumption)*
- **BR-030:** A wrong current password in a password change counts as a failed sign-in attempt under BR-016. *(Source: assumption, security)*
- **BR-031:** After a successful password change, every other Admin session ends and the current one stays active. *(Source: assumption)*
- **BR-032:** [NEEDS CLARIFICATION: must the Admin replace the initial password at the first sign-in before reaching any other page? Recommended: no] *(Source: Open question 2)*
- **BR-033:** There is no self-service password reset (no email, no security questions). A forgotten password is recovered only by the system operator: at system start, when the setup values contain an explicit reset indication and a new password, the system replaces the stored password, clears any lockout and ends every Admin session. *(Source: clarification 2026-10-16)*
- **BR-034:** Every request, including the sign-in page and the sign-in operation, is accepted only over an encrypted connection, so neither passwords nor Admin sessions travel in plain text. A request over an unencrypted connection is refused. *(Source: constitution P4 "MUST NOT be transmitted in plain text", P5, P7)*
- **BR-035:** Input rejected at the boundary (an empty or too-long field) is not a failed sign-in attempt and does not count toward a lockout. *(Source: assumption)*
- **BR-036:** During a lockout, a password change is refused as well, even with the correct current password. *(Source: assumption, security)*
- **BR-037:** A recovery (BR-033) with a new password that breaks BR-007 is not applied: the system does not start, it logs the reason without the value, and the old password stays valid. *(Source: assumption)*
- **BR-038:** If the Admin closes the sign-in prompt of BR-024 without signing in, the Admin stays on the same view, and the next refused operation shows the prompt again. *(Source: assumption)*
- **BR-039:** After sign-out, going Back in the browser shows no product data or other protected content; the sign-in page is shown instead. *(Source: constitution P5; assumption)*
- **BR-040:** The lockout limit of BR-016 holds even when many sign-in attempts arrive at the same time: no more credential checks than the limit are made before sign-in is locked. *(Source: constitution P5)*
- **BR-041:** One sign-in submission starts at most one Admin session, even if it is sent twice. *(Source: assumption)*
- **BR-042:** One password-change submission is processed once. A repeated submission of the same change is not counted as a failed sign-in attempt. *(Source: assumption, security)*
- **BR-043:** Only Admin activity (an operation the Admin starts) resets the idle timeout of BR-020. Automatic requests made by the page itself, such as a check of whether the Admin is signed in, do not. *(Source: assumption)*

## Quality expectations

- **QE-01:** 95% of sign-in attempts, successful or failed, are answered within 2 seconds of submitting, measured in the browser. *(Assumption. The slow hash of P4 uses part of this budget.)*
- **QE-02:** 100% of pages and operations other than the sign-in page and the sign-in operation refuse access without a signed-in Admin session, checked automatically for each one. *(Constitution P5.)*
- **QE-03:** 0 occurrences of a password, a password hash or a session secret in logs, error responses or committed configuration. *(Constitution P4, P7.)*
- **QE-04:** A sign-in with an unknown sign-in name and a sign-in with a wrong password give the same message and outcome. Over 100 attempts of each, their average response times differ by less than 100 ms. *(Assumption, security.)*
- **QE-05:** 100% of requests made with an ended session are refused: after sign-out, after expiry, after a password change and after recovery. *(Constitution P5.)*
- **QE-06:** On the sign-in page and the change password form, every field and button can be reached and used with the keyboard alone and has a visible label. Error messages are shown next to the form. *(Assumption.)*
- **QE-07:** 100% of invalid sign-in and password change inputs (empty, too long) are rejected at the boundary with a message, before the sign-in name and password are checked. *(Constitution P6.)*
- **QE-08:** 100% of requests over an unencrypted connection are refused, checked automatically for the sign-in operation and for a protected operation. *(Constitution P4, P5, P7.)*

## Constraints

- Exactly one Admin account; no registration, no extra accounts and no roles. *(Source: constitution P1.)*
- Passwords are hashed with PBKDF2-HMAC-SHA256, a unique random salt of at least 16 bytes and at least 600,000 iterations, and compared in constant time. They are never stored or transmitted in plain text. *(Source: constitution P4.)*
- Every endpoint and page requires an authenticated Admin session except the sign-in endpoint and the sign-in page, and sign-in is protected against brute force. *(Source: constitution P5.)*
- All input is validated at the boundary. *(Source: constitution P6.)*
- No secrets in source code, committed configuration, logs or error responses. Secrets come from the environment the system runs in. *(Source: constitution P7; docs/architecture.md "Configuration".)*
- The system is a single-page application. *(Source: R001 constraint; docs/architecture.md.)*
- When a save fails because the session expired, the form stays open in the same tab with its values, and the values are not stored anywhere else in the browser. *(Source: R001 BR-049.)*
- All R002 P1 items (BL-01 to BL-05) are built before R001 BL-01. R002 BL-02 delivers the product management page as an empty, protected layout that R001 BL-02 fills in, and R002 scenarios use only that page. Checking that an open form keeps its values when a session ends (R001 BR-049) belongs to R001 BL-01, BL-04 and BL-08. *(Source: R001 "Risks and dependencies"; clarification 2026-10-16.)*
- The allowed dependency list has nothing that can send mail, so an email-based feature would need an architecture change. *(Source: docs/architecture.md "Allowed dependencies".)*

## Assumptions

- "The user" in the input means the single Admin (constitution P1).
- The system operator and the Admin are the same person, so setup values and recovery are trusted. Whoever controls the environment the system runs in controls the Admin account.
- Attempts with empty or too-long fields, rejected at the boundary, do not count as failed sign-in attempts.
- The Admin finds out that a session expired at the next action. There is no countdown or warning before expiry.
- Signing out with an open form does not warn about unsaved values.
- The system is reached over an encrypted connection from end to end, including local development.
- Targets of BG-01 (30 s), QE-01 (2 s) and QE-04 (100 ms) are defaults to confirm.

## Risks and dependencies

- **Lockout as denial of service:** a lockout for the whole account lets anyone who can reach the sign-in page keep the Admin locked out. → Accepted for the account-wide lockout (BR-016): it is short (15 minutes), and recovery through BL-07 clears it. Counting per account also hides whether a sign-in name exists and cannot be bypassed by changing source.
- **Initial password left in the setup values:** it may stay readable in the environment after setup. → BR-004 ignores it once the account exists, the operator is told to remove it, and the Admin can change the password (BL-06). Open question 2 decides whether a change is forced.
- **Forgotten password with no recovery:** without BL-07, a forgotten password locks the Admin out for good. → BL-07 (operator reset through the setup values, BR-033).
- **Reset indication left in the setup values:** every restart would reset the password again and end all sessions. → The operator is told to remove it after recovery (PR-07); the spec for BL-07 decides whether the system warns about it.
- **Slow hash adds time to sign-in:** the hashing P4 requires takes noticeable time. → QE-01 allows 2 s.
- **Existing health check and OpenAPI document answer without a session:** the health check (`.AllowAnonymous()`) and the OpenAPI document mapped in Development conflict with P5, which allows only the sign-in page and endpoint. → Both require a signed-in Admin session (BR-014), handled in BL-02. Monitoring that needs an anonymous health check would require a constitution amendment.
- **Build-order cycle with R001:** R001 needs R002 first, while the session-ending behavior matters most on R001 forms. → R002 P1 items are built first against an empty, protected product management page; R001 BL-01, BL-04 and BL-08 check that forms keep their values (R001 BR-049). Spec numbers (013+) do not imply build order.
- **R001 spec 001-add-product is already Approved** and its scenarios expect refusal without a session. → R002 BL-01 to BL-05 are built before spec 001.
- **Encrypted connection in deployment:** a deployment that removes encryption before the system receives the request would hide BR-034 violations. → Recorded as an assumption; check in the plan.

## Backlog

### Summary

| ID | Feature | Priority | Size | Depends on | Covers | Spec |
|----|---------|----------|------|------------|--------|------|
| BL-01 | Admin account setup | P1 | S | — | BG-04, BR-001, BR-002, BR-003, BR-004, BR-005, BR-007, BR-008, QE-03 | — |
| BL-02 | Sign in and protect every page | P1 | M | BL-01 | BG-01, BG-02, BG-04, BR-005, BR-006, BR-008, BR-009, BR-010, BR-011, BR-012, BR-013, BR-014, BR-015, BR-034, BR-035, BR-041, QE-01, QE-02, QE-03, QE-04, QE-06, QE-07, QE-08 | — |
| BL-03 | Lock sign-in after failed attempts | P1 | S | BL-02 | BG-03, BR-016, BR-017, BR-018, BR-019, BR-040, QE-03, QE-04 | — |
| BL-04 | Sign out | P1 | S | BL-02 | BR-023, BR-026, BR-027, BR-039, QE-02, QE-05 | — |
| BL-05 | Session expiry and signing in again | P1 | M | BL-02 | BR-020, BR-021, BR-022, BR-023, BR-024, BR-025, BR-038, BR-043, QE-05 | — |
| BL-06 | Change password | P2 | S | BL-02, BL-03 | BG-04, BR-007, BR-008, BR-028, BR-029, BR-030, BR-031, BR-032, BR-034, BR-036, BR-042, QE-03, QE-05, QE-06, QE-07 | — |
| BL-07 | Recover a forgotten password | P2 | S | BL-01, BL-02, BL-03 | BR-004, BR-007, BR-008, BR-033, BR-037, QE-03, QE-05 | — |

Size: `S` ≤ 1 day, `M` 1–3 days. The `Spec` column holds the reserved `NNN-slug` once `/sdd-ba-spec` creates the spec; `—` until then.

The P1 items BL-01 to BL-05 form the MVP, the minimum that meets constitution P5: an account, sign-in, protection by default, brute-force lockout, sign-out and session expiry. BL-01 to BL-05 are all built before R001 BL-01 (see Constraints). Spec numbers 001–012 are reserved by R001, so specs for this backlog start at 013. BL-06 may change priority or leave scope after Open question 2.

### BL-01 — Admin account setup

**Value:** The system operator creates the single Admin account safely, so the Admin can sign in. No account is ever created with a default password.

**Actors:** System operator

**Spec brief:** When the system starts and no Admin account exists, it creates the one Admin account (BR-001) from the setup values the system operator provides outside the application, never from committed files (BR-002). The sign-in name and initial password are checked (BR-005, BR-007), and the password is stored only as a salted slow hash (BR-008, P4). If setup values are missing or invalid, the system does not start, and it logs which setting is wrong without its value (BR-003). Once the account exists, the setup values are ignored (BR-004). There is no setup page and no separate setup command.

**Key scenarios:**
- No account, valid setup values → account created; the stored password is a hash with its own salt and the required iterations.
- Same password set up twice on fresh systems → the two stored hashes differ.
- No account, setup values missing → the system does not start; the log names the missing setting and contains no password.
- Initial password shorter than the minimum → the system does not start; the log gives the reason without the value.
- Account exists and the setup values have a different password → account unchanged; the old password still works.

**Not included:** Signing in (BL-02); changing the password (BL-06); recovery (BL-07).

### BL-02 — Sign in and protect every page

**Value:** The Admin signs in and reaches the product management page, and nobody without an Admin session can reach any page or operation.

**Actors:** Admin, Unauthorised person

**Spec brief:** This item delivers the product management page as an empty, protected layout that R001 BL-02 fills in later. The sign-in page asks for the sign-in name and password, which are validated at the boundary (QE-07); rejected input is not a failed sign-in attempt (BR-035). A correct sign-in name and password start an Admin session and show the page that was requested or the product management page, never a destination outside the system (BR-009, BR-011, BR-012). A failed sign-in attempt shows one generic message and gives no hint about whether the sign-in name exists (BR-010, QE-04). Every other page and operation, including the existing health check and the OpenAPI document in Development, requires an Admin session: pages show the sign-in page, and operations are refused without data (BR-014, QE-02). Every request is accepted only over an encrypted connection (BR-034, QE-08). A double-submitted sign-in starts one Admin session (BR-041).

**Key scenarios:**
- Correct credentials → product management page shown within 2 seconds (QE-01).
- Sign-in name "Admin " entered for the account "admin" → accepted (BR-006); password "Secret" entered for "secret" → refused.
- Unknown sign-in name, and wrong password → the same message for both; sign-in name kept, password cleared.
- Product management page opened without a session → sign-in page; after sign-in, the product management page is shown.
- Sign-in page opened with a destination on another site → after sign-in, the product management page is shown.
- Any protected operation without a session → refused, no data in the response.
- Health check, or the OpenAPI document in Development, requested without a session → refused.
- Empty password, or a 129-character password → rejected at the boundary; not counted as a failed sign-in attempt.

**Not included:** Lockout (BL-03); sign-out (BL-04); expiry and signing in again (BL-05); any product content on the product management page (R001).

### BL-03 — Lock sign-in after failed attempts

**Value:** Guessing the Admin password online becomes impractical (P5).

**Actors:** Admin, Unauthorised person

**Spec brief:** The system counts failed sign-in attempts in a row for the whole Admin account, whatever sign-in name was entered and wherever the attempts come from. After 5 failures, sign-in is locked for 15 minutes, and every attempt is refused, even with the correct password (BR-016). The sign-in page says sign-in is locked and in how many minutes to try again (BR-017). The count goes back to 0 after a successful sign-in or when the lockout ends (BR-018). Every failure and lockout is logged with its time and never with the password (BR-019). The limit holds when many attempts arrive at the same time (BR-040).

**Key scenarios:**
- 4 failures, then the correct password → signed in; the count goes back to 0.
- 5 failures → locked; the correct password is refused with a "locked, try again in 15 minutes" message.
- 15 minutes after the lockout began → the correct password works.
- Failures with an unknown sign-in name → counted the same way as wrong passwords.
- Failures from two different browsers → counted together toward the same lockout.
- The log shows 5 failures and 1 lockout, with no password text.

**Not included:** Wrong current password in a password change (BL-06 reuses this count); clearing a lockout through recovery (BL-07).

### BL-04 — Sign out

**Value:** The Admin can end the session on purpose, for example on a shared computer.

**Actors:** Admin

**Spec brief:** *(Assumption: sign-out was not in the input.)* From every page except the sign-in page, the signed-in Admin signs out. The Admin session ends at once with no confirmation, and the sign-in page is shown (BR-026, BR-027). The ended session can never be used again (BR-023, QE-05), and going Back shows no protected content (BR-039). Other open tabs of the same browser are refused at their next action and offered to sign in again, which is the behavior BL-05 delivers.

**Key scenarios:**
- Admin signs out from the product management page → sign-in page shown; Back button does not show protected content.
- A request made after sign-out with the old session → refused.
- Two tabs open, sign-out in one → the other tab's next action is refused.

**Not included:** The sign-in prompt in the other tab (BL-05); signing out other browsers (out of scope, except BL-06).

### BL-05 — Session expiry and signing in again

**Value:** A session left unattended stops being usable, and the Admin does not lose work when it ends (R001 BR-049).

**Actors:** Admin

**Spec brief:** An Admin session ends after [NEEDS CLARIFICATION: 30 minutes] without activity and at the latest [NEEDS CLARIFICATION: 12 hours] after sign-in (BR-020, BR-021). There is no "keep me signed in" option (BR-022). When an operation is refused because the session has ended, the Admin is told so and can sign in again without leaving the current view. An open form keeps its values as R001 BR-049 requires (BR-024). After signing in again, the Admin stays on the same view and retries; nothing is resent automatically (BR-025). Closing the prompt keeps the view, and the next refused operation prompts again (BR-038). Only Admin activity resets the idle timeout (BR-043). [NEEDS CLARIFICATION: sign-in prompt over the current view, Open question 10]

**Key scenarios:**
- 31 minutes idle, then the Admin starts an operation on the product management page → refused with "session ended"; sign-in prompt shown over the same view; after signing in, the Admin retries it successfully.
- Active the whole time, 12 hours after sign-in → the next action is refused with "session ended".
- Wrong password in the sign-in prompt → generic message; the Admin stays on the same view.
- Browser closed and reopened → sign-in page shown.

**Not included:** A warning or countdown before expiry (assumption: none); checking that a product form keeps its values (R001 BL-01, BL-04, BL-08, per R001 BR-049).

### BL-06 — Change password

**Value:** The Admin can replace the initial password or a password that may be known to others.

**Actors:** Admin

**Spec brief:** The signed-in Admin opens the change password form and enters the current password and the new password twice (BR-028). The change is refused, with the reason and the password unchanged, if the current password is wrong, the new entries differ, the new password breaks BR-007, or it equals the current one (BR-029). A wrong current password counts toward the lockout (BR-030). On success, the new hash is stored (BR-008), every other Admin session ends, and the current one stays active (BR-031). During a lockout the change is refused (BR-036); a double-submitted change is processed once and not counted as a failure (BR-042). [NEEDS CLARIFICATION: forced change at first sign-in, BR-032]

**Key scenarios:**
- Correct current password and a valid new password entered twice → changed; the old password no longer signs in.
- Wrong current password → refused; counts as a failed sign-in attempt.
- New entries differ, or new password too short → refused on the field; password unchanged.
- Signed in on two browsers, password changed in one → the other's next action is refused.

**Not included:** Recovery of a forgotten password (BL-07).

### BL-07 — Recover a forgotten password

**Value:** The Admin can get back in after forgetting the password without losing any data.

**Actors:** System operator, Admin

**Spec brief:** There is no self-service reset. The system operator sets a new password through the setup values with an explicit reset indication (BR-033). At startup, the system checks the new password (BR-007), replaces the stored hash (BR-008), clears any lockout and ends every Admin session (QE-05). An invalid new password is not applied and the old one stays valid (BR-037). Without the reset indication, the setup values are ignored (BR-004).

**Key scenarios:**
- Reset with a valid new password → the new password signs in; the old one is refused.
- Reset while sign-in is locked → lockout cleared.
- Reset with a too-short password → the system does not start; the log gives the reason without the value; the old password stays valid.

**Not included:** Email reset, security questions (out of scope).

## Traceability

| Goal / rule / quality | Backlog items |
|-----------------------|---------------|
| BG-01 | BL-02 |
| BG-02 | BL-02 |
| BG-03 | BL-03 |
| BG-04 | BL-01, BL-02, BL-06 |
| BR-001 | BL-01 |
| BR-002 | BL-01 |
| BR-003 | BL-01 |
| BR-004 | BL-01, BL-07 |
| BR-005 | BL-01, BL-02 |
| BR-006 | BL-02 |
| BR-007 | BL-01, BL-06, BL-07 |
| BR-008 | BL-01, BL-02, BL-06, BL-07 |
| BR-009 | BL-02 |
| BR-010 | BL-02 |
| BR-011 | BL-02 |
| BR-012 | BL-02 |
| BR-013 | BL-02 |
| BR-014 | BL-02 |
| BR-015 | BL-02 |
| BR-016 | BL-03 |
| BR-017 | BL-03 |
| BR-018 | BL-03 |
| BR-019 | BL-03 |
| BR-020 | BL-05 |
| BR-021 | BL-05 |
| BR-022 | BL-05 |
| BR-023 | BL-04, BL-05 |
| BR-024 | BL-05 |
| BR-025 | BL-05 |
| BR-026 | BL-04 |
| BR-027 | BL-04 |
| BR-028 | BL-06 |
| BR-029 | BL-06 |
| BR-030 | BL-06 |
| BR-031 | BL-06 |
| BR-032 | BL-06 |
| BR-033 | BL-07 |
| BR-034 | BL-02, BL-06 |
| BR-035 | BL-02 |
| BR-036 | BL-06 |
| BR-037 | BL-07 |
| BR-038 | BL-05 |
| BR-039 | BL-04 |
| BR-040 | BL-03 |
| BR-041 | BL-02 |
| BR-042 | BL-06 |
| BR-043 | BL-05 |
| QE-01 | BL-02 |
| QE-02 | BL-02, BL-04 |
| QE-03 | BL-01, BL-02, BL-03, BL-06, BL-07 |
| QE-04 | BL-02, BL-03 |
| QE-05 | BL-04, BL-05, BL-06, BL-07 |
| QE-06 | BL-02, BL-06 |
| QE-07 | BL-02, BL-06 |
| QE-08 | BL-02 |

## Open questions

- [x] 1. How is the single Admin account first created, given that self-registration is forbidden (P1)? A) The system creates it at startup from setup values the system operator puts in the environment. B) A first-run setup page, which the first visitor could claim and which is an extra page without authentication. C) A separate command run outside the application. (affects: BR-002, BR-003, BR-004, BL-01)
- [ ] 2. Is changing the password in scope, and must the initial password be replaced at the first sign-in? (affects: BR-028–BR-032, BL-06 priority, BL-02 if the change is forced)
- [x] 3. How does the Admin recover a forgotten password? A) The system operator resets it through the setup values. B) Not supported. C) Email reset, which needs an architecture change. (affects: BR-033, BL-07)
- [x] 4. The existing health check, and the OpenAPI document mapped in Development, answer without a session, which conflicts with P5. Require a session for them, remove them, or amend the constitution to allow them? (affects: BR-014, BL-02)
- [x] 5. Lockout: how many failed sign-in attempts, how long a lockout, and does it apply to the whole account or per source of the attempts? (affects: BR-016, BG-03, BL-03)
- [ ] 6. Idle timeout and maximum session length? (affects: BR-020, BR-021, BL-05)
- [ ] 7. Is a "keep me signed in" option wanted, or does closing the browser end the session? (affects: BR-022, BL-05)
- [ ] 8. Is the sign-in name a username chosen at setup or an email address? (affects: BR-005, glossary, BL-01, BL-02)
- [ ] 9. Minimum password length (recommended 12, maximum 128, no composition rules)? (affects: BR-007, BL-01, BL-06, BL-07)
- [ ] 10. When the session has ended mid-work, should signing in again happen in a prompt over the current view, keeping the form (R001 BR-049), or on the sign-in page? (affects: BR-024, BR-025, BL-05)
- [ ] 11. After sign-in, show the requested page or always the product management page? (affects: BR-011, BL-02)
- [ ] 12. May the Admin be signed in from several browsers at once? (affects: BR-015, BR-031, BL-02, BL-06)
- [x] 13. Build order with R001 (review finding, critical). R001 needs R002 first, but some R002 scenarios rely on R001 pages and forms. Recommended: R002 BL-02 delivers an empty, protected product management page (layout with sign-out) that R001 BL-02 fills in; R002 scenarios use only that page; checking that "the add form keeps its values" stays with R001 BL-01, BL-04 and BL-08 (R001 BR-049); and all R002 P1 items (BL-01 to BL-05) are built before R001 BL-01. Alternative: build only R002 BL-01 and BL-02 first, and BL-03 to BL-05 after R001 BL-01. (affects: Constraints, Risks, Backlog note, BL-02, BL-04, BL-05 scenarios)
- [ ] 14. Attempts during a lockout: are they counted, do they extend the lockout, are they logged, and does the attempt that triggers the lockout already show the lockout message? Recommended: not counted, no extension, logged; the Nth failure shows the lockout message. Should a wrong current password in a password change be part of the "Failed sign-in attempt" definition? (affects: BR-016, BR-018, BG-03, glossary, BL-03)
- [ ] 15. What does a system restart do to Admin sessions and to an active lockout? Recommended: the lockout survives a restart (only recovery clears it); state whether sessions survive. (affects: BR-016, BR-020, BR-021, BR-033, R001 BR-049, BL-03, BL-05, BL-07)
- [ ] 16. Logging: may the entered sign-in name be logged with a failed sign-in attempt (it may contain a password typed in the wrong field)? Are successful sign-ins, sign-outs, password changes and recoveries logged too? (affects: BR-019, QE-03, BL-03)
- [ ] 17. Should the system log a warning (without the value) at startup when the Admin account exists but the initial password is still present in the setup values? (affects: BR-004, BG-04, BL-01)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|
| 2026-10-16 | Q13: build order with R001 | All R002 P1 items (BL-01–BL-05) before R001 BL-01; R002 BL-02 delivers an empty, protected product management page; R002 scenarios use only that page; form-keeps-values checks stay in R001 |
| 2026-10-16 | Q1: how the Admin account is first created | At system start, when no Admin account exists, from setup values in the environment; no setup page, no separate command |
| 2026-10-16 | Q4: health check and Development OpenAPI document vs. P5 | Both require a signed-in Admin session; no constitution amendment |
| 2026-10-16 | Q5: lockout threshold, duration and scope | 5 failed attempts in a row lock sign-in for 15 minutes, for the whole Admin account regardless of source |
| 2026-10-16 | Q3: recovering a forgotten password | The system operator puts a reset indication and a new password in the setup values and restarts; the system replaces the password, clears any lockout and ends every Admin session; no self-service or email reset |

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-16 | Initial version | — |
