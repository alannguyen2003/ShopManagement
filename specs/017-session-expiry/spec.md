# Spec: Session expiry and signing in again

**Feature:** 017-session-expiry
**Branch:** feat/017-session-expiry
**Source:** R002/BL-05
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-17

## Context

Once spec 014 (R002 BL-02) is in place, the Admin can sign in and every page and operation needs a signed-in Admin session. As long as that session lasts, though, anyone at an unattended browser can use it, which constitution P5 is meant to prevent. This feature ends an Admin session after 30 minutes without Admin activity and at the latest 12 hours after sign-in, and also when the browser is closed. Because a session can now end while the Admin is working, the Admin must be able to sign in again over the current view without losing an open form (R001 BR-049). R001 BL-01 needs all R002 P1 items, this one included, to be built first.

## User stories

### US1 — An unattended Admin session stops working (Priority: P1)

As the Admin, I want my Admin session to end after a period with no Admin activity, and in any case some hours after I signed in, so that nobody can use a browser I left unattended.

**Why P1:** Without expiry, a session left open stays usable forever. This is the security purpose of the item and part of the minimum that constitution P5 requires.

**Independent test:** Sign in, let 30 minutes pass with no Admin activity, then start an operation: it is refused. Separately, sign in and keep the session active, then start an operation 12 hours after sign-in: it is refused.

**Acceptance scenarios:**
1. **Given** a signed-in Admin whose last Admin activity was 29 minutes ago, **When** the Admin starts an operation on the product management page, **Then** the operation is accepted and the 30-minute idle timeout starts again from that moment (BR-020, BR-043).
2. **Given** a signed-in Admin with no Admin activity for 30 minutes, **When** the Admin starts an operation, **Then** it is refused because the Admin session has ended, and no data is returned (BR-020, BR-023).
3. **Given** a signed-in Admin with no Admin activity for 31 minutes, during which the page itself made automatic requests (for example checking whether the Admin is signed in), **When** the Admin starts an operation, **Then** it is refused because the Admin session has ended. The automatic requests did not reset the idle timeout (BR-043).
4. **Given** an Admin who signed in 12 hours ago and has had Admin activity at least every 10 minutes since, **When** the Admin starts an operation, **Then** it is refused because the Admin session has ended (BR-021).
5. **Given** an Admin who signed in 11 hours 59 minutes ago and has stayed active, **When** the Admin starts an operation, **Then** it is accepted (BR-021).
6. **Given** an expired session, **When** any further request is made with it, including after more Admin activity, **Then** every request is refused, and the session never becomes valid again (BR-023).
7. **Given** a signed-in Admin whose last Admin activity was 20 minutes ago, **When** the Admin reloads the page, **Then** the product management page is shown and the idle timeout starts again from the reload (BR-020, BR-043).

### US2 — Sign in again without leaving the current view (Priority: P1)

As the Admin, I want to sign in again over the view I am on when my Admin session has ended, so that I can retry what I was doing without losing an open form.

**Why P1:** Without this, an ended session sends the Admin away from the view and loses an open form, which R001 BR-049 forbids. R001 BL-01, BL-04 and BL-08 depend on it.

**Independent test:** Let the Admin session expire on the product management page and start an operation. The sign-in prompt opens over the same view. Sign in there: the Admin stays on the same view, and retrying the operation succeeds. [NEEDS CLARIFICATION: the product management page is empty in R002 (BL-02); which Admin-started operation should these scenarios use? See Open question 1.]

**Acceptance scenarios:**
1. **Given** an Admin on the product management page whose Admin session has expired, **When** the Admin starts an operation, **Then** the Admin is told the Admin session has ended and a sign-in prompt opens over the current view. The sign-in page is not shown (BR-024).
2. **Given** the sign-in prompt is open after a refused operation, **When** the Admin enters the correct sign-in name and password, **Then** a new Admin session starts, the prompt closes, the Admin stays on the same view, and the refused operation is not sent again on its own (BR-025).
3. **Given** the Admin has just signed in again in the sign-in prompt, **When** the Admin starts the refused operation again, **Then** it is accepted (BR-025).
4. **Given** the sign-in prompt is open, **When** the Admin enters a wrong password, **Then** the prompt shows the same generic message as the sign-in page, keeps the sign-in name, clears the password field, and the view behind it is unchanged (BR-024, BR-010).
5. **Given** the sign-in prompt is open, **When** the Admin closes it without signing in, **Then** the Admin stays on the same view, unchanged (BR-038).
6. **Given** the Admin closed the sign-in prompt without signing in, **When** the Admin starts another operation, **Then** it is refused because the Admin session has ended, and the sign-in prompt opens again (BR-038).
7. **Given** sign-in is locked (spec 015) and the sign-in prompt is open, **When** the Admin enters the correct sign-in name and password, **Then** the prompt shows the lockout message of BR-017, and the view behind it is unchanged (BR-024).
8. **Given** 4 failed sign-in attempts in a row and the sign-in prompt open, **When** a 5th sign-in with a wrong password fails in the prompt, **Then** the prompt shows the lockout message of BR-017 (15 minutes), and the view behind it is unchanged (BR-024).
9. **Given** the Admin has two tabs of the same browser open and the Admin session has been ended from one of them (for example by sign-out, spec 016), **When** the Admin starts an operation in the other tab, **Then** it is refused, and the sign-in prompt opens over that tab's current view (BR-024, BR-027).
10. **Given** the Admin signed in again in the sign-in prompt, **When** 29 minutes pass with no Admin activity and the Admin then starts an operation, **Then** it is accepted: the new Admin session has its own idle timeout and its own maximum session length, counted from the new sign-in (BR-020, BR-021).

### US3 — Closing the browser ends the Admin session (Priority: P1)

As the Admin, I want my Admin session to end when I close the browser, so that the next person who opens it on a shared computer cannot use my session.

**Why P1:** There is no "keep me signed in" option (BR-022), and closing the browser is the usual way to leave a shared computer.

**Independent test:** Sign in, close the browser, reopen it and open the system: the sign-in page is shown.

**Acceptance scenarios:**
1. **Given** a signed-in Admin, **When** the browser is closed, then reopened, and the system is opened, **Then** the sign-in page is shown and no protected content is shown (BR-022). [NEEDS CLARIFICATION: browsers that restore the previous session on reopen; see Open question 2.]
2. **Given** the sign-in page or the sign-in prompt, **When** the Admin looks at the form, **Then** it has no "keep me signed in" option (BR-022).
3. **Given** a signed-in Admin with two tabs of the same browser open, **When** one tab is closed and the Admin starts an operation in the other, **Then** it is accepted, because closing a tab does not close the browser.

### US4 — A system restart does not sign the Admin out (Priority: P2)

As the Admin, I want my Admin session to survive a system restart, so that a restart does not interrupt my work or make me sign in again.

**Why P2:** Restarts are rare. The MVP stays safe without this story (sessions would simply end more often), but BR-045 requires it, and it protects R001 BR-049 forms from restarts.

**Independent test:** Sign in, restart the system, then start an operation within the idle timeout: it is accepted without signing in again.

**Acceptance scenarios:**
1. **Given** a signed-in Admin whose last Admin activity was 5 minutes ago, **When** the system is restarted and the Admin starts an operation 10 minutes after that last Admin activity, **Then** it is accepted without signing in again (BR-045).
2. **Given** a signed-in Admin whose last Admin activity was 20 minutes before the system stopped, **When** the system stays stopped for 15 minutes and the Admin then starts an operation, **Then** it is refused because the Admin session has ended (35 minutes with no Admin activity), and the sign-in prompt opens (BR-045, BR-020, BR-024).
3. **Given** an active Admin who signed in 11 hours 50 minutes before the system stopped, **When** the system stays stopped for 15 minutes and the Admin then starts an operation, **Then** it is refused because the maximum session length has passed (BR-045, BR-021).

## Edge cases

- The Admin types in a form for more than 30 minutes without starting any operation, then saves. Typing is not Admin activity, so the save is refused and the sign-in prompt opens, with the form and its values kept (BR-024, R001 BR-049). Checking that a product form keeps its values belongs to R001 BL-01, BL-04 and BL-08.
- An operation is made exactly at the 30-minute or 12-hour limit. A request at or after the limit is refused; there is no grace period (FR-001, FR-004).
- An operation is refused for invalid input while the session is valid. It still counts as Admin activity and restarts the idle timeout (FR-002).
- Automatic requests made by the page after the session has ended are refused, but they do not open the sign-in prompt. The Admin finds out at the next Admin-started operation (FR-017).
- The page is reloaded, the system is opened in a new tab, or the browser is reopened while the session has ended. The sign-in page is shown instead of the sign-in prompt, because there is no view to keep (FR-015).
- The Admin presses Back or reloads while the sign-in prompt is open. The view is left, the sign-in page is shown, and unsaved values are lost. This is accepted, because R001 BR-049 only covers staying in the same view.
- The sign-in prompt's sign-in is submitted twice (double click). At most one Admin session starts (BR-041, spec 014).
- The sign-in prompt is submitted with an empty or too-long field. The input is rejected at the boundary with a message on the field and is not counted as a failed sign-in attempt (BR-035, spec 014).
- A failed sign-in in the sign-in prompt counts toward the lockout like one on the sign-in page (BR-016, spec 015).
- Two tabs of the same browser each show the sign-in prompt, and the Admin signs in again in one of them. The other tab's next operation is accepted, because both tabs share the browser's Admin session. Its prompt can be closed or used.
- The maximum session length is reached while the Admin is active. The next operation is refused with no warning beforehand (R002 assumption: no countdown).
- The Admin chooses sign out after the Admin session has already ended. What the Admin sees is decided in spec 016 (its FR-008 and Open question 1), not here.
- The connection is lost when an operation is started. That is not an ended session: the sign-in prompt is not shown, and the session's state is unchanged.
- The system is stopped for longer than 30 minutes. Every Admin session with no Admin activity in the last 30 minutes, counting the downtime, has ended at the next request (BR-045).

## Functional requirements

- **FR-001:** The system MUST end an Admin session when 30 minutes pass with no Admin activity. A request made 30 minutes or more after the session's last Admin activity is refused (BR-020).
- **FR-002:** Each Admin activity in a valid Admin session MUST restart that session's idle timeout. Admin activity is opening a page of the system or an operation the Admin starts (for example saving or signing out), whether or not the operation then succeeds (BR-020, BR-043). Loading or reloading the system in a tab, or opening it in a new tab, is Admin activity: the request made to show that view restarts the idle timeout. Requests the page makes later without an Admin action (repeated or background sign-in checks, automatic refreshes) are not Admin activity (FR-003).
- **FR-003:** Automatic requests made by the page itself, such as checking whether the Admin is signed in, MUST NOT restart the idle timeout (BR-043).
- **FR-004:** The system MUST end an Admin session at the latest 12 hours after the sign-in that started it, whatever the Admin activity. A request made 12 hours or more after that sign-in is refused (BR-021).
- **FR-005:** The sign-in page and the sign-in prompt MUST NOT offer a "keep me signed in" option, or any other way to extend an Admin session beyond FR-001, FR-004 and FR-006 (BR-022).
- **FR-006:** After the browser is closed and reopened, it MUST no longer hold the Admin session: opening the system shows the sign-in page (BR-022). The system is not told when a browser closes; that Admin session can no longer be used from that browser, and it ends for good at its idle timeout (FR-001) at the latest. [NEEDS CLARIFICATION: browsers that restore the previous session on reopen; see Open question 2.]
- **FR-007:** An expired session MUST never become valid again. Every request made with it, including later Admin activity, is refused with no data and no internal detail, as for a request without a session (BR-023, BR-014).
- **FR-008:** When an operation the Admin started from a shown view is refused because there is no valid Admin session (expiry, sign-out in another tab of the same browser (016), password change in another browser (018), recovery (019)), the system MUST tell the Admin that the Admin session has ended and open the sign-in prompt over the current view. It MUST NOT leave the view or show the sign-in page (BR-024). What the Admin sees when a sign-out is refused is decided in 016 FR-008.
- **FR-009:** The sign-in prompt MUST ask for the sign-in name and the password, and MUST use the same sign-in operation as the sign-in page. Every rule of 014-admin-sign-in and 015-sign-in-lockout for that operation applies unchanged, including BR-006, BR-008, BR-009, BR-010, BR-016–BR-019, BR-034, BR-035, BR-040, BR-041, BR-044 and QE-04 (BR-024). The only differences are FR-011 (the view is kept instead of BR-011), and BR-013 does not apply.
- **FR-010:** While the sign-in prompt is open, after it is closed and after the Admin signs in again, the current view and any open form MUST keep their state and entered values in the same tab. The system MUST NOT store those values anywhere else in the browser to make this work (BR-024, R001 BR-049). Password fields of the change password form follow spec 018 (its Open question 2).
- **FR-011:** A successful sign-in in the sign-in prompt MUST start a new Admin session, close the prompt and leave the Admin on the same view. The product management page of BR-011 is not shown instead (BR-025, BR-011).
- **FR-012:** The system MUST NOT repeat the refused operation automatically after the Admin signs in again. The Admin retries it (BR-025).
- **FR-013:** A failed sign-in in the sign-in prompt, or one refused during a lockout, MUST show its message inside the prompt and keep the prompt open over the unchanged view (BR-024; R002 PR-05 exception).
- **FR-014:** The Admin MUST be able to close the sign-in prompt without signing in. The Admin then stays on the same, unchanged view, and the next refused operation opens the prompt again (BR-038).
- **FR-015:** Opening the system without a valid Admin session (first load, reload, new tab, reopened browser) MUST show the sign-in page, not the sign-in prompt (BR-014, BR-022, BR-023; assumption on which surface to show).
- **FR-016:** A system restart MUST NOT end Admin sessions. The idle timeout and the maximum session length MUST keep counting while the system is stopped, so a session whose limit passed during the downtime is refused at its next request (BR-045).
- **FR-017:** A refused automatic request made by the page itself MUST NOT open the sign-in prompt. Only a refused operation the Admin started opens it (R002 assumption: "the Admin finds out that a session expired at the next action").

## Non-functional requirements

- **NFR-001:** 95% of sign-ins in the sign-in prompt, successful or failed, are answered within 2 seconds of submitting, measured in the browser (R002 QE-01, applied to the sign-in prompt).
- **NFR-005:** 100% of requests made with an expired session are refused. Automated checks cover the end of the idle timeout, the end of the maximum session length and limits passed during a system restart. The other ended-session cases of QE-05 (after sign-out, after a password change, after a recovery) are checked in specs 016, 018 and 019 (R002 QE-05). Closing the browser is checked through SC-003, not through request refusal.
- **NFR-006:** Every field and button of the sign-in prompt, including the way to close it, can be reached and used with the keyboard alone and has a visible label. Its messages are shown inside the prompt next to the form (R002 QE-06, applied to the sign-in prompt; assumption).

## Key entities

- **Admin session:** the time during which the Admin is signed in, in one browser. Attributes: when it started (the sign-in), the time of the last Admin activity, and whether it is active or ended. One per browser, and several may exist at once (BR-015). Once ended, it never becomes active again.
- **Admin activity:** an operation the Admin starts (opening, loading or reloading a page, saving, signing out). Restarts the idle timeout of the Admin session it is made in. Requests the page makes later on its own are not Admin activity.
- **Expired session:** an Admin session ended by the idle timeout (30 minutes) or the maximum session length (12 hours). A kind of ended session.
- **Ended session:** an Admin session that is no longer valid for any reason. Every request made with it is refused.
- **Sign-in prompt:** the sign-in form shown over the current view when an operation is refused because the Admin session has ended. It follows the sign-in page's rules and keeps the view.

## Success criteria

- **SC-001:** In automated checks, 0 requests are accepted with an Admin session past 30 minutes without Admin activity or past 12 hours after sign-in, including across a system restart.
- **SC-002:** After the Admin session ends on a view, the Admin signs in again and retries the operation without leaving the view and without re-entering anything in the view, in 100% of the checked cases.
- **SC-003:** After the browser is closed and reopened, opening the system shows the sign-in page in 100% of the checked cases.
- **SC-004:** In automated checks, 0 automatic requests made by the page extend an Admin session.

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BR-020 | FR-001, FR-002; US1 scenarios 1, 2, 7; US2 scenario 10; US4 scenario 2 |
| BR-021 | FR-004; US1 scenarios 4, 5; US2 scenario 10; US4 scenario 3 |
| BR-022 | FR-005, FR-006, FR-015; US3 scenarios 1, 2 |
| BR-023 | FR-007, FR-015; US1 scenarios 2, 6 |
| BR-024 | FR-008, FR-009, FR-010, FR-013; US2 scenarios 1, 4, 7, 8, 9 |
| BR-025 | FR-011, FR-012; US2 scenarios 2, 3 |
| BR-038 | FR-014; US2 scenarios 5, 6 |
| BR-043 | FR-002, FR-003, FR-017; US1 scenarios 1, 3; SC-004 |
| BR-045 | FR-016; US4 scenarios 1–3; edge case on downtime longer than 30 minutes (lockout part out of scope, spec 015) |
| QE-05 | NFR-005 (expiry cases; the other ended-session cases are in specs 016, 018, 019) |

## Assumptions

- The Admin session is shared by all tabs of one browser (glossary: "in one browser"). Admin activity in any tab restarts its idle timeout; signing in again in one tab makes the others usable again; closing one tab does not end it.
- An operation the Admin starts counts as Admin activity even when it is then refused for invalid input. Typing in a form does not count.
- The limits are exact: there is no grace period after 30 minutes or 12 hours.
- The idle timeout and the maximum session length are fixed values. Neither the Admin nor the system operator can change them.
- The sign-in prompt opens with both fields empty.
- One message is used for every reason a session ended, for example "Your session has ended. Sign in again to continue."; it does not say why.
- Ending a session by expiry is not logged, because R002 BR-019 does not list it. Signing in again in the sign-in prompt is a successful sign-in and is logged as spec 014 specifies.
- There is no warning or countdown before expiry (R002 assumption).
- The NFR numbers repeat the number of the R002 quality expectation they come from.

## Out of scope

- The sign-in page, the sign-in operation, the generic failure message, the rule that every page and operation requires a session, encrypted connections and double-submission handling: R002 BL-02 (spec 014). The sign-in prompt reuses them.
- The lockout, its message and its survival across a restart (the lockout part of BR-045): R002 BL-03 (spec 015).
- Signing out, the sign-out button, logging sign-outs, the Back button after sign-out, and what the Admin sees when a sign-out is refused: R002 BL-04 (spec 016). This spec covers only the sign-in prompt in another tab after a sign-out.
- Ending other Admin sessions after a password change: R002 BL-06 (spec 018).
- Ending every Admin session at a recovery: R002 BL-07 (spec 019).
- Checking that a product form (add, edit, stock adjustment) keeps its values when a session ends: R001 BL-01, BL-04 and BL-08 (R001 BR-049).
- Any warning or countdown before expiry, and any "keep me signed in" option (BR-022).
- Repeating the refused operation automatically after signing in again (BR-025).
- A list of active sessions, or ending sessions on other devices on request (R002 Out of scope).
- Logging when a session expires (not in R002 BR-019).

## Open questions

- [ ] 1. The product management page is an empty layout in R002 (BL-02), and the only operation the Admin can start on it is sign-out, whose refusal spec 016 may handle differently (016 Open question 1). Which operation should trigger the sign-in prompt in the US2 scenarios, the independent test and SC-002? A) A protected operation that exists only in the automated tests; R001 BL-01 does the end-to-end check with a real form. B) Spec 016 Open question 1 is answered "B", so a refused sign-out opens the prompt and is the trigger. C) Add BL-06 (018) to BL-05's "Depends on" and use the change password form; this needs `/sdd-amend requirements R002`. Recommended: A, because it needs no change elsewhere and R001 BL-01 already owns the form check (R001 BR-049). (affects: US2, SC-002, FR-008; spec 016 US2 scenario 3)
- [ ] 2. Many browsers can restore the previous session when reopened ("continue where you left off"), which would keep the Admin session alive after the browser is closed. Should this be A) accepted, with the session still bounded by the 30-minute idle timeout and the 12-hour maximum, or B) prevented even then? Option A contradicts R002 BR-022 as approved ("after the browser is reopened, the sign-in page is shown") and needs `/sdd-amend requirements R002` (BR-022). Recommended: A, with that amendment, because the system cannot fully control browser restore behavior. Related: a restored tab showing a view without a valid session opens the sign-in prompt at its next operation (FR-008). (affects: FR-006, US3 scenario 1, BR-022)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-17 | Initial version | — |
