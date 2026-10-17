# Spec: Admin sign-in and protection of every page

**Feature:** 014-admin-sign-in
**Branch:** feat/014-admin-sign-in
**Source:** R002/BL-02
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-17

## Context

The system has no way for the Admin to sign in yet, and constitution P5 requires a signed-in Admin session for every page and operation except the sign-in page and the sign-in operation. Every R001 product feature depends on this. Today the health check and, in Development, the OpenAPI document answer without a session. This feature lets the Admin sign in with the sign-in name and password set up in 013-admin-account-setup (R002 BL-01) and land on the product management page. Until R001 BL-02 fills it in, that page is an empty, protected layout. Every other page and operation is closed to anyone without an Admin session. Every request must use an encrypted connection. This spec comes before the lockout (015), sign-out (016) and session expiry (017) specs, and it must be built before R001 spec 001-add-product. Until 015-sign-in-lockout is merged, sign-in has no brute-force protection, so this feature alone does not meet constitution P5 and must not be released or deployed before 015 is merged.

## User stories

### US1 — Sign in and reach the product management page (Priority: P1)

As the Admin, I want to sign in with my sign-in name and password so that I reach the product management page and can manage products.

**Why P1:** Without signing in, the Admin can use nothing in the system, because every page and operation needs an Admin session (US2).

**Independent test:** On a system with an Admin account from Admin account setup, open the sign-in page. Sign in with the correct sign-in name and password and check that the product management page appears. Then try a wrong password, an unknown sign-in name and an empty field, and check the messages and that no Admin session was started.

**Acceptance scenarios:**
1. **Given** the Admin account "admin" with the password "correct horse battery", and no Admin session in this browser, **When** the Admin enters "admin" and "correct horse battery" on the sign-in page and submits, **Then** an Admin session starts and the product management page is shown (BR-009, BR-011; timing: NFR-001).
2. **Given** the Admin account "admin", **When** the Admin enters the sign-in name " Admin " with the correct password, **Then** sign-in succeeds (BR-006).
3. **Given** the Admin account with the password "correct horse battery", **When** the Admin enters the correct sign-in name and "Correct horse battery", **Then** sign-in is refused with the message "Sign-in name or password is incorrect" (BR-006, BR-010).
4. **Given** the sign-in page, **When** the Admin submits the unknown sign-in name "someone" with any password, **Then** the same message "Sign-in name or password is incorrect" is shown as for a wrong password, the sign-in name "someone" stays in its field, the password field is empty and no Admin session starts (BR-010, QE-04).
5. **Given** the sign-in page, **When** the Admin submits an empty password, or a password of 129 characters, **Then** the submission is rejected with a message on the password field, not the message of BR-010, and the sign-in name and password are not checked (BR-035, QE-07; counting: 015 FR-002).
6. **Given** the sign-in page, **When** the Admin submits a sign-in name that is empty or only spaces, or that has 101 characters after leading and trailing spaces are removed, **Then** the submission is rejected with a message on the sign-in name field, not the message of BR-010, and the sign-in name and password are not checked (BR-005, BR-035, QE-07; counting: 015 FR-002).
7. **Given** the sign-in page was opened with a destination, inside or outside the system (for example a link to another site), **When** the Admin signs in successfully, **Then** the product management page is shown and the destination is ignored (BR-012).
8. **Given** a signed-in Admin, **When** the Admin opens the sign-in page, **Then** the product management page is shown instead (BR-013).
9. **Given** the Admin is signed in on browser A, **When** the Admin signs in on browser B, **Then** both Admin sessions stay active and each can use the product management page (BR-015).
10. **Given** the sign-in page with correct details entered, **When** the sign-in is submitted twice in quick succession (double click), **Then** at most one Admin session is started and the product management page is shown once (BR-041).
11. **Given** a successful sign-in, **When** the log is inspected, **Then** it contains one successful sign-in event with its time, and does not contain the entered sign-in name or password (BR-019, QE-03).
12. **Given** a browser with an active Admin session and a second tab of it still showing the sign-in page, **When** the Admin signs in successfully in that tab, **Then** a new Admin session starts for that browser, the previous one ends and its session value is refused from then on, every tab of that browser continues with the new Admin session, and the Admin sessions of other browsers are unchanged (BR-015).

### US2 — Nothing is reachable without an Admin session (Priority: P1)

As the Admin, I want every page and operation, except signing in, to be closed to anyone without an Admin session so that no unauthorised person can see or change shop data.

**Why P1:** This is constitution P5 and BG-02. No R001 product feature can ship without it.

**Independent test:** With no Admin session, open every page and call every operation of the system, including the health check and the OpenAPI document in Development. Every page must show the sign-in page, and every operation must be refused with no data or internal detail. This is checked automatically for each page and operation.

**Acceptance scenarios:**
1. **Given** no Admin session, **When** someone opens the product management page, **Then** the sign-in page is shown and no protected content is shown (BR-014).
2. **Given** no Admin session, **When** someone opens a page at an address other than the product management page, **Then** the sign-in page is shown. After a successful sign-in, the product management page is shown, not the page originally opened (BR-011, BR-014).
3. **Given** no Admin session, **When** someone calls any operation other than sign-in, **Then** it is refused with an indication that sign-in is required, and the reply contains no data and no internal detail (BR-014, QE-02).
4. **Given** no Admin session, **When** someone requests the health check, **Then** it is refused like every other protected operation (BR-014).
5. **Given** the system runs in Development and there is no Admin session, **When** someone requests the OpenAPI document, **Then** it is refused like every other protected operation (BR-014).
6. **Given** a signed-in Admin, **When** the Admin requests the health check, or the OpenAPI document in Development, **Then** it is answered (BR-014).
7. **Given** a signed-in Admin, **When** the Admin views the product management page, **Then** it shows the empty layout with no product content (R002 Constraints).
8. **Given** a request carrying a made-up or altered Admin session value, **When** any page or operation is requested, **Then** it is treated exactly as a request with no Admin session (BR-014).
9. **Given** a signed-in Admin who visits a page of another website, **When** that page makes the Admin's browser send an operation to the system (for example a sign-out or a product change), **Then** the operation is refused and not processed, the Admin session is unchanged, and the reply contains no data (constitution P5).
10. **Given** a page of another website that tries to show a page of the system inside itself, the sign-in page included, **When** the browser loads it, **Then** the system's page is not shown inside the other website's page (constitution P5).
11. **Given** no Admin session, **When** someone requests, by its address, the application code, layout or other file of any page other than the sign-in page (for example the product management page's code), **Then** it is refused and nothing of that page is delivered. Only what the sign-in page itself needs is delivered without an Admin session (constitution P5).

### US3 — Sign-in and sessions only over an encrypted connection (Priority: P1)

As the Admin, I want the system to accept requests only over an encrypted connection so that my password and my Admin session never travel in plain text.

**Why P1:** Constitution P4 forbids sending passwords in plain text. Without this story, signing in (US1) could leak the password.

**Independent test:** Over an unencrypted connection, open the sign-in page, call the sign-in operation with the correct password and call a protected operation. Check that the page is sent to the encrypted address with no data in the reply, and that both operations are refused without being processed.

**Acceptance scenarios:**
1. **Given** any visitor, **When** the sign-in page is opened over an unencrypted connection, **Then** the browser is sent to the same page over an encrypted connection, and the reply contains no data (BR-034, QE-08).
2. **Given** the correct sign-in name and password, **When** the sign-in operation is sent over an unencrypted connection, **Then** it is refused and not processed: the password is not checked and no Admin session starts (BR-034, QE-08; counting: 015 FR-002).
3. **Given** a protected operation, **When** it is sent over an unencrypted connection, with or without an Admin session, **Then** it is refused and not processed, and the reply contains no data (BR-034, QE-08).
4. **Given** any reply over an encrypted connection, **When** the browser receives it, **Then** the reply tells the browser to use only encrypted connections to the system from then on (BR-034).

## Edge cases

- Sign-in name with leading or trailing spaces, or in a different letter case → it matches the Admin account (BR-006). Spaces inside the name are kept and compared.
- Sign-in name of exactly 100 characters after trimming → accepted for checking. 101 characters → rejected at the boundary (BR-005).
- Password of exactly 128 characters → accepted for checking. 129 → rejected at the boundary. A non-empty password shorter than 12 characters is not rejected at the boundary: it is checked and fails as a wrong password, because the 12-character minimum of BR-007 applies to setup, change and recovery, not to sign-in.
- Password made only of spaces → not empty, so it is checked and fails as a wrong password (BR-006: spaces are kept).
- Both fields empty → both fields show a message, and it is not a failed sign-in attempt (BR-035).
- Unknown sign-in name → the same message, outcome and response time as a wrong password, with an average difference of less than 100 ms (BR-010, QE-04).
- Correct details double-submitted → one Admin session (BR-041). Whether wrong details double-submitted count once or twice toward the lockout is decided in 015-sign-in-lockout (its Open question 1).
- The connection is lost, or the system fails unexpectedly during sign-in → no Admin session starts. The sign-in page says the sign-in could not be completed and to try again, and shows no internal detail. The sign-in name is kept and the password field is cleared (constitution P7; assumption).
- Back button after a successful sign-in, returning to the sign-in page → the product management page is shown (BR-013).
- Sign-in page opened with a destination pointing outside the system → ignored, no redirect to another site (BR-012).
- Two tabs of the same browser both show the sign-in page, and the Admin signs in on one, then on the other → the second sign-in starts a new Admin session for that browser and ends the previous one (FR-013). A failed sign-in in the second tab does not end the Admin session (Assumptions).
- A request to an operation that does not exist, without an Admin session → refused the same way as a protected operation, so it does not reveal which operations exist (BR-014; assumption).
- A page address that matches no page → without an Admin session, the sign-in page is shown (BR-014). With one, see Assumptions.
- Files the sign-in page shares with other pages (for example the styles or the font) → may be delivered without an Admin session, because the sign-in page needs them; they must not contain shop data, Admin account information or the code of another page (FR-015).
- Code of a page other than the sign-in page that the browser kept from an earlier Admin session (for example in its cache) → this feature only controls what the system delivers; after the session ends, no data operation is answered (FR-016). Caching rules after sign-out belong to 016-sign-out.
- A request carrying a made-up, altered or unknown Admin session value → treated exactly as no Admin session (BR-014; US2 scenario 8).
- An attempt to reach protected content over an unencrypted connection while an Admin session exists → refused or redirected the same way, with no data (BR-034).
- A sign-in sent from a page of another website (for example to make failed sign-in attempts count toward a lockout) → refused and not processed like any other cross-site operation; it is not a failed sign-in attempt (FR-025).
- A link from another website that simply opens a page of the system (a normal navigation, not an operation) → handled as usual: the sign-in page or the requested page is shown (FR-025 concerns operations only).

## Functional requirements

- **FR-001:** The system MUST provide a sign-in page with a sign-in name field, a password field and a sign-in action. It is the only page available without an Admin session (BR-005, BR-014). The two fields MUST be identifiable to the browser and to password managers as a sign-in name and a current password, so that they can offer to save and fill them.
- **FR-002:** The password field MUST always be masked, with no option to show its value. The entered password MUST never be shown, logged or returned in readable form (BR-008).
- **FR-003:** The system MUST validate sign-in input at the boundary before checking the sign-in name and password. The sign-in name must be 1 to 100 characters after leading and trailing spaces are removed, and the password 1 to 128 characters. Invalid input MUST be rejected with a message on the field concerned (BR-005, QE-07). Lengths are counted the same way as at Admin account setup (013-admin-account-setup, Assumptions), so any sign-in name and password that setup accepts passes this check.
- **FR-004:** Input rejected at the boundary MUST NOT be treated as a failed sign-in attempt (BR-035).
- **FR-005:** The system MUST match the sign-in name ignoring letter case and leading and trailing spaces, and MUST match the password exactly: case-sensitive, with spaces kept (BR-006).
- **FR-006:** The system MUST check the entered password only against the stored salted slow hash, as constitution P4 requires, including its constant-time comparison (BR-008).
- **FR-007:** A sign-in MUST succeed only when the sign-in name and the password both match the Admin account and sign-in is not locked (BR-009). The lockout itself is delivered by 015-sign-in-lockout (BL-03).
- **FR-008:** On a successful sign-in from the sign-in page, the system MUST start an Admin session for that browser and show the product management page, whatever page was being opened when the sign-in page appeared (BR-011).
- **FR-009:** The sign-in page MUST accept no destination for after sign-in. Any destination given to it, inside or outside the system, MUST be ignored (BR-012).
- **FR-010:** A failed sign-in attempt MUST show the same message, "Sign-in name or password is incorrect", whether the sign-in name is unknown or the password is wrong. It MUST give no hint about which one was wrong. It MUST keep the entered sign-in name and clear the password field (BR-010).
- **FR-011:** An unknown sign-in name and a wrong password MUST lead to the same outcome. Nothing in the reply or its timing may show whether the sign-in name exists (BR-010, QE-04).
- **FR-012:** A signed-in Admin who opens the sign-in page MUST be shown the product management page; the sign-in form is not shown, even briefly (BR-013).
- **FR-013:** The Admin MUST be able to be signed in from several browsers at the same time, each with its own Admin session. A sign-in in one browser MUST NOT end the Admin session of another (BR-015). A successful sign-in in a browser that already has an active Admin session MUST start a new Admin session for that browser and end the previous one, whose session value is refused from then on; the new session's timers start at this sign-in (017-session-expiry).
- **FR-014:** One sign-in submission MUST start at most one Admin session, even if it is sent twice (BR-041).
- **FR-015:** Every page other than the sign-in page MUST require an Admin session. When a page is opened, reloaded or navigated to without an Admin session, the sign-in page MUST be shown, and no protected content may be shown (BR-014). Protected content is shop data, Admin account information **and the application code, layout and other files of every page other than the sign-in page**: without an Admin session the system MUST deliver only what the sign-in page itself needs, and MUST refuse a direct request for anything else (constitution P5, "every admin page MUST require an authenticated admin session"). What happens on a view that is already open when its Admin session ends is delivered by 017-session-expiry.
- **FR-016:** Every operation other than the sign-in operation MUST require an Admin session. Without one, it MUST be refused with an indication that sign-in is required, and the reply MUST contain no data and no internal detail (BR-014).
- **FR-017:** FR-016 MUST also apply to the existing health check and to the OpenAPI document available in Development (BR-014).
- **FR-018:** The system MUST provide the product management page as an empty, protected layout with no product content, which R001 BL-02 (002-product-list) fills in later (R002 Constraints).
- **FR-019:** The system MUST log every successful sign-in with its time and kind. It MUST NOT log the entered sign-in name or the entered password, in this event or in any other log entry this feature writes, including boundary rejections and unexpected errors. A double-submitted successful sign-in is logged once (BR-019, BR-041, QE-03).
- **FR-020:** A page opened over an unencrypted connection MUST be answered only by sending the browser to the same page over an encrypted connection, with no data in the reply (BR-034).
- **FR-021:** An operation sent over an unencrypted connection, the sign-in operation included, MUST be refused and not processed. A password sent this way MUST never be checked or accepted, and no Admin session may start (BR-034).
- **FR-022:** The system MUST tell the browser to use only encrypted connections to it from then on (BR-034).
- **FR-023:** If sign-in fails unexpectedly or its reply is lost, the browser MUST NOT be left signed in. The page says the sign-in could not be completed and to try again, with no internal detail; the sign-in name is kept and the password field is cleared. It is not a failed sign-in attempt. The error is logged with context but without the entered sign-in name or password (constitution P7, P9; BR-019; assumption).
- **FR-024:** Replies to refused or failed requests MUST NOT contain stack traces, internal details, the stored password hash or any session secret (constitution P7, QE-03).
- **FR-025:** Every operation, the sign-in operation and sign-out (016) included, MUST be refused and not processed when a page of another website made the browser send it, even if the browser has an Admin session. The reply MUST contain no data, the Admin session MUST be unchanged, and a refused sign-in is not a failed sign-in attempt (constitution P5; cross-site request forgery; R002 BR to be added).
- **FR-026:** No page of the system, the sign-in page included, MAY be shown inside a page of another website (constitution P5; clickjacking; R002 BR to be added).

## Non-functional requirements

- **NFR-001:** 95% of sign-in attempts, successful or failed, are answered within 2 seconds of submitting, measured in the browser. *(QE-01)*
- **NFR-002:** 100% of pages and operations other than the sign-in page and the sign-in operation refuse access without an Admin session, including the health check, the OpenAPI document in Development and the application code and files of every page other than the sign-in page. This is checked automatically for each one. *(QE-02)*
- **NFR-003:** 0 occurrences of a password, a password hash or a session secret in logs, error replies or committed configuration, and 0 occurrences of an entered sign-in name in logs. *(QE-03)*
- **NFR-004:** Over 100 sign-ins with an unknown sign-in name and 100 with a wrong password, the message and outcome are identical, and the average response times differ by less than 100 ms. Measured in runs of at most 4 failed sign-in attempts, each run followed by a successful sign-in that resets the count (BR-018), so the lockout of 015 never answers in place of a check. *(QE-04)*
- **NFR-006:** On the sign-in page, every field and button can be reached and used with the keyboard alone and has a visible label, and error messages are shown next to the form. *(QE-06; the change password form is in 018-change-password.)*
- **NFR-007:** 100% of invalid sign-in inputs (empty, too long) are rejected at the boundary with a message, before the sign-in name and password are checked. *(QE-07)*
- **NFR-008:** 100% of operations over an unencrypted connection are refused, and 100% of pages opened that way are sent to the encrypted address without data. This is checked automatically for the sign-in operation, a protected operation and the sign-in page. *(QE-08)*

## Key entities

- **Admin account:** The only account in the system (BR-001). Holds the sign-in name and the password, stored only as a salted slow hash. It is created by Admin account setup (013-admin-account-setup). This feature only reads it.
- **Admin session:** The time during which the Admin is signed in, in one browser. It starts at a successful sign-in and belongs to the Admin account. Several can exist at once, one per browser (BR-015). Its time of start is needed later for the maximum session length (017-session-expiry). How it ends is delivered by 016, 017, 018 and 019.
- **Failed sign-in attempt:** A sign-in, outside a lockout, where the sign-in name or the password does not match. This feature produces it. Counting it, locking, and logging it are delivered by 015-sign-in-lockout.

## Success criteria

- **SC-001:** With the correct sign-in name and password, the Admin gets from the sign-in page to the product management page in under 30 seconds. *(BG-01)*
- **SC-002:** 0 pages or operations of the system, other than the sign-in page and the sign-in operation, can be reached without an Admin session, as checked by the automatic check of NFR-002. *(BG-02)*
- **SC-003:** 0 readable Admin passwords in stored data, logs, error replies or committed configuration after a test run that includes successful, failed and rejected sign-ins. *(BG-04)*

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BG-01 | SC-001; US1 scenario 1 |
| BG-02 | SC-002; US2; FR-015, FR-016, FR-017 |
| BG-04 | SC-003; FR-002, FR-006, FR-019, FR-023, FR-024 |
| BR-005 | FR-001, FR-003; US1 scenario 6; edge cases (100/101 characters) |
| BR-006 | FR-005; US1 scenarios 2, 3; edge cases (spaces, case, spaces-only password) |
| BR-008 | FR-002, FR-006; Key entities (Admin account) |
| BR-009 | FR-007; US1 scenario 1 |
| BR-010 | FR-010, FR-011; US1 scenarios 3, 4 |
| BR-011 | FR-008; US1 scenario 1; US2 scenario 2 |
| BR-012 | FR-009; US1 scenario 7; edge case (destination outside the system) |
| BR-013 | FR-012; US1 scenario 8; edge case (Back button) |
| BR-014 | FR-001, FR-015, FR-016, FR-017; US2 scenarios 1–6, 11; edge cases (shared files, cached code) |
| BR-015 | FR-013; US1 scenarios 9, 12; edge case (two tabs sign in) |
| BR-019 | FR-019; US1 scenario 11 (successful sign-in only; the rest of BR-019 is under Out of scope → 015/016/018/019) |
| BR-034 | FR-020, FR-021, FR-022; US3 scenarios 1–4 |
| BR-035 | FR-004; US1 scenarios 5, 6; edge case (both fields empty) |
| BR-041 | FR-014; US1 scenario 10 |
| QE-01 | NFR-001 |
| QE-02 | NFR-002; US2 scenario 3 |
| QE-03 | NFR-003; FR-019, FR-023, FR-024; US1 scenario 11 |
| QE-04 | NFR-004; FR-011; US1 scenario 4 |
| QE-06 | NFR-006 (sign-in page only) |
| QE-07 | NFR-007; FR-003; US1 scenarios 5, 6 |
| QE-08 | NFR-008; US3 scenarios 1–3 |
| Constitution P5 (cross-site operations, embedding) | FR-025, FR-026; US2 scenarios 9, 10; edge cases (cross-site sign-in, plain links). R002 has no BR for this yet: add one through `/sdd-clarify R002`, then cite it here. |

## Assumptions

- The empty product management page shows its title and a layout with no product content and no placeholder data. R001 BL-02 adds the product list, and 016-sign-out adds the sign-out control.
- The sign-in page has the heading "Sign in" and no other content beyond the two fields, the sign-in action and messages.
- Boundary messages are plain and field-specific, for example "Enter your sign-in name", "Sign-in name must be 100 characters or fewer", "Enter your password" and "Password must be 128 characters or fewer".
- Pasting into the password field is allowed, and so is filling both fields from the browser or a password manager (FR-001).
- There is no "forgot password" link on the sign-in page. Recovery happens outside the application through the system operator (BR-033, 019-password-recovery).
- When the connection is lost or the system fails unexpectedly during sign-in, the sign-in name is kept and the password field is cleared, as for a failed sign-in attempt.
- An operation that does not exist is refused without an Admin session in the same way as a protected operation.
- A page address that matches no page, opened with an Admin session, shows the product management page.
- A failed sign-in from a browser that already has an active Admin session (for example a stale second tab) does not end that Admin session. A successful one replaces it (FR-013).
- The system is reached over an encrypted connection from end to end, including local development (R002 Assumptions).
- How the application code is split and served, so that only the sign-in page's part is delivered without an Admin session, is decided in the plan. It may need a change to `docs/architecture.md` through `/sdd-architecture`.
- The Admin account always exists when the system runs, because 013-admin-account-setup stops the system from starting without one.
- NFR numbers repeat the number of the R002 quality expectation they come from. QE-05 (requests with an ended session) belongs to 016, 017, 018 and 019, so there is no NFR-005 here.

## Out of scope

- Creating the Admin account from setup values: 013-admin-account-setup (BL-01).
- Counting failed sign-in attempts, the lockout, the lockout message, double-submitted wrong sign-ins, and logging failed sign-in attempts, attempts refused during a lockout and lockouts: 015-sign-in-lockout (BL-03; BR-016–BR-018, BR-040, BR-044, BR-045). This feature only logs successful sign-ins (BR-019). This feature must not be released or deployed before 015 is merged (constitution P5).
- Signing out, the sign-out control on the product management page, and Back after sign-out: 016-sign-out (BL-04).
- The idle timeout, the maximum session length, ending the Admin session when the browser is closed, the absence of "keep me signed in", the sign-in prompt over the current view, and refusing ended sessions: 017-session-expiry (BL-05; BR-020–BR-025, BR-038, BR-043, BR-045).
- Changing the password: 018-change-password (BL-06).
- Recovering a forgotten password: 019-password-recovery (BL-07).
- Any product content on the product management page: R001 (002-product-list and later).
- Self-registration, extra accounts and roles (constitution P1); changing the sign-in name; two-factor authentication, external sign-in providers and CAPTCHA (R002 Out of scope).

## Open questions

- [x] 1. May the browser offer to save and fill the sign-in name and password on the sign-in page? Options: A) allowed, B) the sign-in page tells the browser not to save or fill them. Recommended: A, because it encourages long, unique passwords (BR-007). The risk on a shared computer is limited by sign-out (BL-04) and session expiry (BL-05). (affects: FR-001, FR-002, NFR-006)
- [x] 2. When a browser that already has an active Admin session signs in again successfully (for example from a second tab still showing the sign-in page), is that Admin session A) kept as it is, or B) replaced by a new one that ends the old one? Recommended: B, because it is safer and matches 017 (signing in again in the sign-in prompt starts a new Admin session, and the 12-hour maximum of BR-021 counts from it). (affects: FR-013, FR-014, edge cases, spec 017 US2 scenario 10)
- [x] 3. In a single-page application the application code, including the empty product management layout, is downloaded before sign-in; only the data is protected. Should FR-015 say "protected content means shop data and Admin account information; the application code the browser downloads is not protected content"? Recommended: yes. This is a reading of constitution P5 ("every admin page MUST require an authenticated admin session"); if you disagree, the constitution needs `/sdd-constitution`. (affects: FR-015, NFR-002)
- [x] 4. R002 has no rule against (a) operations started from another website through the Admin's signed-in browser (cross-site request forgery), including a forced sign-out, or (b) the sign-in page being embedded in another site's page to trick the Admin (clickjacking). Should both be refused? Recommended: yes, through `/sdd-amend requirements R002` (new BRs covered by this item), since security is the constitution's first priority. (affects: FR-016, FR-001, specs 016 and 018)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|
| 2026-10-17 | Refuse cross-site request forgery and clickjacking? | Refuse both: operations sent from another website's page are refused and not processed (sign-in and sign-out included), and no page of the system may be shown inside another website's page. Based on constitution P5; matching BRs to be added to R002. |
| 2026-10-17 | A browser with an active Admin session signs in again: keep or replace its session? | Replace: a new Admin session starts for that browser, the previous one ends and its value is refused; other browsers are unaffected. Matches 017 (a new sign-in starts new timers). |
| 2026-10-17 | In a single-page app, is the downloaded application code protected content? | Yes: protected content includes the code, layout and files of every page other than the sign-in page. Without an Admin session, only what the sign-in page needs is delivered; how is a plan decision, possibly needing `/sdd-architecture`. |
| 2026-10-17 | May the browser save and fill the sign-in name and password? | Allowed: the fields are marked as sign-in name and current password so browsers and password managers can save and fill them; this encourages long, unique passwords (BR-007). |

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-17 | Initial version | — |
