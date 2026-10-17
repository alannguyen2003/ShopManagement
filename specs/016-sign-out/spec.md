# Spec: Sign out

**Feature:** 016-sign-out
**Branch:** feat/016-sign-out
**Source:** R002/BL-04
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-17

## Context

Once the Admin has signed in (014-admin-sign-in, R002 BL-02), without this feature an Admin session ends only by expiry or by closing the browser (017-session-expiry, R002 BL-05). On a shared computer, the Admin needs to end the session on purpose, so that nobody who uses the same browser afterwards can reach product data. This feature adds a sign-out to every page except the sign-in page. Signing out ends the Admin session at once. The ended session can never be used again, and Back, Forward or reload in that tab show no protected content. Sign-out was not in the input. R002 added it as an assumption for the shared-computer case, and it is part of the R002 MVP that must be built before R001 BL-01.

## User stories

### US1 — Sign out (Priority: P1)

As the Admin, I want to sign out from the page I am on so that nobody else using this browser can act as me.

**Why P1:** It is the only way the Admin can end an Admin session on purpose. R002 places it in the MVP that must exist before any product feature.

**Independent test:** Sign in, open the product management page and choose sign out. The sign-in page is shown, and a request made afterwards with the old Admin session is refused. This works without any other R002 feature beyond 013-admin-account-setup and 014-admin-sign-in.

**Acceptance scenarios:**
1. **Given** a signed-in Admin on the product management page, **When** the Admin chooses sign out, **Then** the Admin session ends at once with no confirmation step, and the sign-in page is shown (BR-026, BR-027).
2. **Given** the sign-in page is shown (no Admin session, including right after a sign-out), **When** the Admin looks at it, **Then** it offers no sign-out (BR-026).
3. **Given** the Admin has just signed out, **When** a request is made with the ended session to the product management page, **Then** the sign-in page is shown (BR-023, BR-014).
4. **Given** the Admin has just signed out, **When** a request is made with the ended session to a protected operation, **Then** it is refused and returns no data or internal detail (BR-023, BR-014).
5. **Given** the Admin has signed out, **When** the Admin signs in again on the sign-in page, **Then** a new Admin session starts, the product management page is shown (BR-011), and the ended session is still refused (BR-023).
6. **Given** the Admin is signed in on browser A and on browser B, **When** the Admin signs out on browser A, **Then** the Admin session on browser B stays active, and its next action succeeds (BR-015).
7. **Given** the Admin signs out, **When** the log is read, **Then** it holds exactly one sign-out entry with its time and kind. The entry contains no sign-in name, password, password hash or session secret (BR-019, QE-03).

### US2 — Nothing usable is left after sign-out (Priority: P1)

As the Admin, I want the browser to show and allow nothing protected after I sign out so that the next person at this computer cannot see or change product data.

**Why P1:** Sign-out has no value on a shared computer if the Back button or a second tab still exposes protected content (constitution P5).

**Independent test:** Sign in, open the product management page in two tabs and sign out in one. Going Back in that tab shows the sign-in page, and the other tab's next action is refused.

**Acceptance scenarios:**
1. **Given** the Admin has signed out from the product management page, **When** the Admin goes Back in the browser, **Then** the sign-in page is shown, not the product management page or any other protected content (BR-039).
2. **Given** the Admin has signed out and gone Back, **When** the Admin goes Forward, **Then** no protected content is shown either (BR-039).
3. **Given** two tabs of the same browser show the product management page, **When** the Admin signs out in the first tab and a protected operation (for example the health check) is then requested from the second tab, **Then** it is refused because the Admin session has ended (BR-027, BR-023). A sign-out chosen in the second tab is the case of FR-008. Which Admin-started operation the empty layout offers for this check is shared with spec 017 Open question 1.
4. **Given** two tabs of the same browser and the Admin has signed out in the first, **When** the second tab is reloaded, **Then** the sign-in page is shown (BR-023, BR-014).
5. **Given** the Admin signed out and the system was then restarted, **When** a request is made with the ended session, **Then** it is still refused (BR-023).

## Edge cases

- **Sign-out chosen twice (double click), including both submissions arriving at the same time:** the Admin session ends once, the sign-in page is shown once with no error or prompt, and exactly one sign-out is logged. The repeated submission has no visible effect (BR-023, FR-009).
- **Sign-out chosen after the Admin session has already ended** (expired, or signed out in another tab): the sign-out operation is refused like any operation without a session (BR-014). What the Admin sees is FR-008 [NEEDS CLARIFICATION].
- **Sign-out does not reach the system** (connectivity lost): see FR-011 [NEEDS CLARIFICATION].
- **Sign-out with an open form holding unsaved values:** there is no warning. The values are discarded and the sign-in page is shown (R002 assumption "Signing out with an open form does not warn about unsaved values"). The values are not kept anywhere in the browser (R001 BR-049).
- **An operation started just before sign-out in another tab of the same browser:** if it reached the system before the sign-out, it is processed normally. If it arrived after, it is refused (BR-023).
- **Sign-out during a lockout** (015-sign-in-lockout): a lockout refuses sign-ins only. It does not end an active Admin session, so sign-out works. After that, signing in again is refused until the lockout ends (BR-016).
- **Signed in on several browsers:** sign-out ends only the Admin session of the browser where it was chosen (BR-015).
- **Sign-out operation called with no session at all** (for example a direct request): refused, with no data or internal detail (BR-014, QE-02).
- **Sign-out sent over an unencrypted connection:** refused, as for every operation (BR-034, delivered by 014-admin-sign-in).
- **Sign-in prompt open over the current view** (017-session-expiry): the Admin session has already ended, so the sign-out case above applies.

## Functional requirements

- **FR-001:** The system MUST offer sign-out on every page except the sign-in page. This includes the product management page and any view or form opened on it (BR-026).
- **FR-002:** The Admin MUST be able to sign out with a single action and no confirmation step (BR-027).
- **FR-003:** The system MUST end the current Admin session when sign-out is chosen, and MUST then show the sign-in page with empty fields (BR-027).
- **FR-004:** The system MUST treat a session ended by sign-out as no session, for good: a page requested with it shows the sign-in page, and an operation requested with it is refused with no data or internal detail. This holds after a system restart too (BR-023, BR-014).
- **FR-005:** Sign-out MUST end only the Admin session of the browser where it was chosen. Admin sessions in other browsers stay active (BR-015).
- **FR-006:** Other tabs of the same browser MUST be refused at their next action after the sign-out (BR-027). The prompt to sign in again in those tabs is delivered by 017-session-expiry (BR-024).
- **FR-007:** After sign-out, going Back or Forward in the browser MUST NOT show product data or any other protected content. The sign-in page MUST be shown instead (BR-039).
- **FR-008:** The sign-out operation MUST itself require a signed-in Admin session and MUST be refused without one, returning no data (BR-014, QE-02). When a sign-out is refused because the Admin session had already ended before it was chosen (expiry, sign-out in another tab), the Admin MUST see [NEEDS CLARIFICATION: the sign-in page directly with no error, or the sign-in prompt over the current view that BR-024 gives for other refused operations? See Open question 1].
- **FR-009:** The system MUST log exactly one sign-out entry per Admin session ended by sign-out, with its time and kind, even when the sign-out is submitted twice or both submissions arrive at the same time. The entry MUST NOT contain the sign-in name, a password, a password hash or a session secret (BR-019, QE-03).
- **FR-010:** Responses to sign-out, and refusals of requests made with an ended session, MUST NOT contain a password, a password hash, a session secret, a stack trace or other internal detail (QE-03).
- **FR-011:** When a sign-out cannot reach the system (for example, connectivity is lost) or fails unexpectedly, with no internal detail shown, the system MUST [NEEDS CLARIFICATION: tell the Admin the sign-out did not complete and keep the current view so it can be retried, or show the sign-in page anyway while the Admin session stays valid until it expires or the browser is closed? See Open question 2].

## Non-functional requirements

- **NFR-002:** 100% of pages and operations this feature adds, including the sign-out operation, refuse access without a signed-in Admin session. This is checked automatically for each one (QE-02).
- **NFR-003:** 0 occurrences of a password, a password hash or a session secret in the sign-out log entry, the sign-out response or the refusals of ended sessions, and 0 occurrences of a sign-in name in the sign-out log entry. This is checked automatically (QE-03).
- **NFR-005:** 100% of requests made with a session ended by sign-out are refused. This is checked automatically for the product management page, a protected operation and the sign-out operation itself, before and after a system restart (QE-05).

## Key entities

- **Admin:** the single person who signs in and signs out (constitution P1).
- **Admin session:** the time during which the Admin is signed in, in one browser. Sign-out ends it. Tabs of the same browser share it, and each browser has its own (BR-015).
- **Ended session:** an Admin session that is no longer valid. Here it has been ended by sign-out. It is refused on every later request (BR-023).

## Success criteria

- **SC-001:** From any page except the sign-in page, the Admin signs out with exactly one action, and the sign-in page is shown.
- **SC-002:** After sign-out, 0 protected pages or product data can be seen through Back, Forward or reload in the tab where the Admin signed out, and 100% of next actions or reloads in other tabs of the same browser are refused or show the sign-in page (BR-027, BR-039).
- **SC-003:** 0 requests made with a session ended by sign-out are accepted.
- **SC-004:** Each sign-out produces exactly 1 log entry, and that entry contains no sign-in name, password, password hash or session secret.

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BR-019 | FR-009; US1 scenario 7; SC-004; Assumptions (only completed sign-outs are logged) |
| BR-023 | FR-004, FR-006; US1 scenarios 3, 4, 5; US2 scenarios 3, 4, 5; edge cases (double click, operation just before sign-out) |
| BR-026 | FR-001; US1 scenarios 1, 2; SC-001 |
| BR-027 | FR-002, FR-003, FR-006; US1 scenario 1; US2 scenario 3 |
| BR-039 | FR-007; US2 scenarios 1, 2; SC-002 |
| QE-02 | NFR-002; FR-008; edge case "sign-out operation called with no session at all" |
| QE-03 | NFR-003; FR-009, FR-010; US1 scenario 7 |
| QE-05 | NFR-005; FR-004; SC-003 |

## Assumptions

- Sign-out was not in the input. R002 includes it as an assumption for shared computers (R002 BL-04 spec brief).
- Signing out with an open form gives no warning about unsaved values (R002 assumption).
- After sign-out, the sign-in page shows empty fields and no special "signed out" message.
- Other tabs of the same browser do not have to change on their own when the Admin signs out. They are refused at their next action (BR-027).
- An operation that reached the system before the sign-out is processed normally.
- Only completed sign-outs are logged as sign-out events. A sign-out refused for lack of a valid session is not a separate event, because BR-019 lists only sign-outs.
- Sign-out is a lockout-independent operation: a lockout (BR-016) refuses sign-ins only and does not prevent sign-out.
- Like the sign-in page (QE-06), the sign-out control can be reached and used with the keyboard alone and has a visible label "Sign out".
- NFR numbers match the R002 QE numbers they come from. QE-01, QE-04 and QE-06–QE-08 belong to other backlog items, so NFR-001, NFR-004 and NFR-006–NFR-008 are absent here.

## Out of scope

- The sign-in prompt over the current view in other tabs, or after any refused operation: R002 BL-05 (017-session-expiry).
- Ending an Admin session through the idle timeout, the maximum session length or closing the browser (BR-020–BR-022): R002 BL-05 (017-session-expiry).
- The sign-in page, protection of every page and operation, the encrypted-connection rule (BR-034) and the product management page layout: R002 BL-02 (014-admin-sign-in).
- Lockout after failed sign-in attempts: R002 BL-03 (015-sign-in-lockout).
- Ending other Admin sessions through a password change: R002 BL-06 (018-change-password). Ending all Admin sessions at recovery: R002 BL-07 (019-password-recovery).
- Signing out other browsers on request, or a list of active Admin sessions: out of scope in R002.
- A warning about unsaved form values when signing out: not requested.
- Checking that product forms behave correctly when a session ends: R001 BL-01, BL-04 and BL-08 (R001 BR-049).
- Logging any event other than sign-out: R002 BL-02, BL-03, BL-06 and BL-07.

## Open questions

- [ ] 1. When a sign-out is refused because the Admin session had already ended before it was chosen (expiry, sign-out in another tab), what does the Admin see? A) The sign-in page directly, with no error and no prompt. B) The sign-in prompt over the current view, as BR-024 gives for other refused operations. Recommended: A, because the Admin wanted to end the session, which has already happened, and offering to sign in so as to sign out is confusing. Spec 017 relies on this answer. (affects: FR-008, edge cases; spec 017 FR-008)
- [ ] 2. When a sign-out cannot reach the system (connectivity lost), what happens? A) The Admin is told the sign-out did not complete and stays on the current view to retry. B) The sign-in page is shown anyway, while the Admin session stays valid until it expires or the browser is closed. Recommended: A, because B makes a shared computer look signed out while the Admin session is still usable. (affects: FR-011, edge cases)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-17 | Initial version | — |
