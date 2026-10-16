# Spec: Adjust stock

**Feature:** 006-adjust-stock
**Branch:** feat/006-adjust-stock
**Source:** R001/BL-08
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-16

## Context

Every Product starts with a Stock quantity of exactly 1 (001-add-product, BL-01), and the edit form does not touch the Stock quantity (004-update-product, BL-04). The Admin therefore has no way to bring the Stock quantity in line with what is on the shelf: they cannot record new deliveries, and they cannot bring an Active product down to 0 so that it can be deleted (007-delete-product, BL-06). This feature adds the Stock adjustment: from the Product details, the Admin adds a number of units to the Stock quantity or removes a number of units from it. The Stock quantity always stays a whole number from 0 to 1,000,000. Adjustments made close together all count.

## User stories

### US1 — Add units to the stock (Priority: P1)

As the Admin, I want to add a number of units to a product's Stock quantity so that the recorded stock matches what was delivered to the shop.

**Why P1:** Without it, every product stays at the Starting stock quantity of 1 forever, and BG-02 cannot be met in any useful way.

**Independent test:** Add a product (Stock quantity 1), open its Product details, add 24 units and check that the Product details and the Product list both show a Stock quantity of 25, with the Product information and Product status unchanged.

**Acceptance scenarios:**
1. **Given** a Draft product with Stock quantity 1, **When** the Admin chooses add, enters 24 units and saves, **Then** the Product details show Stock quantity 25 with a success confirmation, and the name, SKU, price, description and Product status are unchanged.
2. **Given** an Active product that is Out of stock (Stock quantity 0), **When** the Admin adds 10 units, **Then** the Stock quantity is 10.
3. **Given** a product with Stock quantity 0, **When** the Admin adds 1,000,000 units, **Then** the Stock quantity is 1,000,000.
4. **Given** a product with Stock quantity 999,990, **When** the Admin adds 20 units, **Then** the adjustment is refused, the message tells the Admin that the current Stock quantity is 999,990 and that the limit is 1,000,000, and the Stock quantity stays 999,990.
5. **Given** the adjustment is open, **When** the Admin enters 0, -2, 1.5, 1,000,001, a value that is not a number, or leaves the number of units empty, and saves, **Then** the adjustment is rejected with a message next to the number of units field and the Stock quantity is unchanged.
6. **Given** the adjustment is open with add and 5 entered, **When** the Admin cancels, **Then** nothing changes and the Product details show the unchanged Stock quantity.
7. **Given** the Product details show Stock quantity 10 and the stored Stock quantity has meanwhile become 15 (5 units added from another tab), **When** the Admin adds 2 units, **Then** the Stock quantity becomes 17 and the Product details show 17.
8. **Given** an Admin whose Stock quantity is 25 after an adjustment, **When** the Admin returns to the Product list, **Then** the product's row shows Stock quantity 25.
9. **Given** no signed-in Admin session, **When** a Stock adjustment is requested for any product, **Then** it is refused and the Stock quantity is unchanged.

### US2 — Remove units from the stock (Priority: P1)

As the Admin, I want to remove a number of units from a product's Stock quantity so that the recorded stock matches the shelf after units are sold, lost or damaged, and so that I can bring an Active product to 0 before I delete it.

**Why P1:** Bringing an Active product to 0 is the only way it can ever be deleted (BR-004, BL-06). Without removal, the recorded stock can only go up.

**Independent test:** On a product with Stock quantity 25, remove 25 units and check that the Product details show Stock quantity 0. Then try to remove 1 more unit and check that the adjustment is refused and the Stock quantity stays 0.

**Acceptance scenarios:**
1. **Given** a product with Stock quantity 25, **When** the Admin chooses remove, enters 25 units and saves, **Then** the Stock quantity is 0 (the product is Out of stock), and its Product status is unchanged.
2. **Given** an Active product with Stock quantity 3, **When** the Admin removes 3 units, **Then** the Stock quantity is 0 and the Product status stays Active.
3. **Given** a product with Stock quantity 3, **When** the Admin removes 5 units, **Then** the adjustment is refused, the message tells the Admin the current Stock quantity is 3, and the Stock quantity stays 3.
4. **Given** the Product details show Stock quantity 5 and the stored Stock quantity has meanwhile become 2, **When** the Admin removes 4 units, **Then** the adjustment is refused, the message tells the Admin the current Stock quantity is 2, the Product details now show 2, and the Stock quantity stays 2.
5. **Given** a product with Stock quantity 10 open in two tabs, **When** the Admin adds 5 units in one tab and removes 2 units in the other, saved close together, **Then** the Stock quantity is 13.
6. **Given** the adjustment is open, **When** the Admin enters a number of units without choosing add or remove and saves, **Then** the adjustment is rejected with a message asking the Admin to choose add or remove, and the Stock quantity is unchanged.
7. **Given** a product that was deleted from another tab, **When** the Admin saves a Stock adjustment for it, **Then** the system shows "product not found" with a way back to the Product list, and nothing changes.

### US3 — Retry after a failed save (Priority: P2)

As the Admin, I want the adjustment I entered to stay on screen when saving fails so that I can retry without typing it again.

**Why P2:** The core adjustment (US1, US2) works without this. It protects the Admin from re-entering values after a network error or an expired session.

**Independent test:** Open an adjustment, enter remove and 4, cut the network and save. Check that the form is still open with remove and 4 and an error message. Restore the network, save again and check that the Stock quantity changed by exactly 4.

**Acceptance scenarios:**
1. **Given** the adjustment is open with remove and 4 entered, **When** saving fails because of a network error, **Then** the adjustment stays open in the same tab with remove and 4 still entered and an error message, and the Admin can save again.
2. **Given** the adjustment is open with add and 12 entered and the Admin's session has expired, **When** the Admin saves, **Then** the adjustment is not applied, the adjustment stays open with add and 12 still entered and a message that the Admin must sign in again, and after signing in again the Admin can save it.
3. **Given** a save failed and the entered values are on screen, **When** the Admin closes the tab, **Then** the entered values are not kept anywhere in the browser.

## Edge cases

- The removal would take the Stock quantity below 0 → refused; the message shows the current stored Stock quantity; nothing changes (BR-034).
- The addition would take the Stock quantity above 1,000,000 → refused; the message shows the current stored Stock quantity and the 1,000,000 limit; nothing changes (BR-034).
- The number of units is at the limits (1 and 1,000,000) → accepted when the result stays from 0 to 1,000,000.
- The number of units is 0, negative, fractional, above 1,000,000, empty, contains letters, or contains thousands separators (e.g. "1.000" or "1,000") → rejected next to the number of units field; nothing changes.
- The add/remove choice is missing, or a request carries any other value → rejected at the boundary; nothing changes.
- The Product details show an old Stock quantity because another tab changed it → the adjustment is applied to the stored Stock quantity (BR-035); a refusal shows the stored value, not the old one.
- Two adjustments are saved close together (two tabs) → both count (BR-035); neither one is lost.
- A Stock adjustment and a product information update (BL-04) are saved close together → both take effect; the update never undoes the adjustment (BR-016).
- The product was deleted, or never existed → "product not found"; nothing changes (PR-07; the rule for deleted products is BR-048, specified in 007-delete-product).
- The Admin clicks Save twice, or retries after a network error when the first save may have been applied → [NEEDS CLARIFICATION: see FR-014].
- The session expired while the adjustment was open → refused; the entered values stay on screen (BR-049).
- The Admin presses the browser Back button after a successful adjustment → the previous view is shown; the adjustment is not applied again.
- A request without a signed-in Admin session → refused; nothing changes (BR-015).

## Functional requirements

- **FR-001:** The Product details MUST offer a Stock adjustment action for every product shown, whether its Product status is Draft or Active.
- **FR-002:** In a Stock adjustment, the Admin MUST choose add or remove and enter a number of units. Neither add nor remove is preselected, and a save without a choice MUST be rejected with a message on the choice.
- **FR-003:** The system MUST accept a number of units only if it is a whole number from 1 to 1,000,000. Any other value (missing, 0, negative, fractional, above 1,000,000, not a number) MUST be rejected with a message next to the number of units field, and the Stock quantity MUST stay unchanged (BR-033).
- **FR-004:** The system MUST reject any add/remove value other than add or remove, at the boundary, before anything changes.
- **FR-005:** The system MUST apply a Stock adjustment to the stored Stock quantity at the moment it is saved, not to the value the screen showed, so that two adjustments saved close together both count (BR-035).
- **FR-006:** The system MUST refuse a removal that would bring the Stock quantity below 0. The message MUST tell the Admin the current stored Stock quantity, and the Stock quantity MUST stay unchanged (BR-034, BR-003).
- **FR-007:** The system MUST refuse an addition that would bring the Stock quantity above 1,000,000. The message MUST tell the Admin the current stored Stock quantity and the 1,000,000 limit, and the Stock quantity MUST stay unchanged (BR-034, BR-003).
- **FR-008:** The system MUST NOT store a Stock quantity that is negative, fractional or above 1,000,000 (BR-003).
- **FR-009:** After a successful Stock adjustment, the system MUST show the updated Product details with the new stored Stock quantity and a success confirmation. Later views of the Product list MUST show the new Stock quantity.
- **FR-010:** After a refused adjustment (FR-006, FR-007), the Product details MUST show the current stored Stock quantity.
- **FR-011:** A Stock adjustment MUST change only the Stock quantity. The Product information and Product status MUST stay unchanged. After a product is added, the Stock adjustment is the only way its Stock quantity changes (BR-016).
- **FR-012:** If the product does not exist or has been deleted, the system MUST show "product not found" with a way back to the Product list and change nothing.
- **FR-013:** If the Admin cancels a Stock adjustment, the system MUST change nothing and return to the Product details.
- **FR-014:** When one Save action of a Stock adjustment is submitted twice (double click, or a retry after a lost response), the system MUST [NEEDS CLARIFICATION: apply it at most once, or apply each submission? R001 limits duplicate submissions only for adding a product (BR-028), and BR-035 says adjustments saved close together all count. "At most once" is a new business rule that would need `/sdd-amend requirements R001` (a new BR, and BR-035 reworded to "two separate Save actions both count").]
- **FR-015:** When saving a Stock adjustment fails because of a network error or an expired session, the adjustment MUST stay open in the same tab with the entered add/remove choice and number of units and an error message, so that the Admin can retry, after signing in again if needed. The entered values MUST NOT be stored anywhere else in the browser (BR-049).
- **FR-016:** Only a signed-in Admin MUST be able to make a Stock adjustment. A request without a signed-in Admin session MUST be refused and change nothing (BR-015).
- **FR-017:** The system MUST NOT record a reason, note or history entry for a Stock adjustment.
- **FR-018:** Opening, saving and cancelling a Stock adjustment MUST happen within the Product management page without a full page reload. The browser Back button MUST return to the previous view and MUST NOT apply an adjustment again.

## Non-functional requirements

NFR numbers match the R001 QE numbers. QE-01 is not covered by this item, so NFR-001 is not used.

- **NFR-002:** With 10,000 products stored, 95% of Stock adjustments (successful or refused) are confirmed to the Admin within 1 second of saving, measured in the browser (QE-02).
- **NFR-003:** 100% of Stock adjustment requests without a signed-in Admin session are refused, and 0 of them change a Stock quantity (QE-03).
- **NFR-004:** Opening, saving and cancelling a Stock adjustment cause 0 full page reloads. The browser Back button returns to the previous view in 100% of cases and applies 0 adjustments again (QE-04).
- **NFR-005:** 100% of invalid Stock adjustment inputs (number of units, add/remove choice, unknown product) are rejected at the boundary with a field-level message, and 0 invalid Stock quantities are stored (QE-05).
- **NFR-006:** 0 error messages shown to the Admin for a Stock adjustment contain stack traces or internal details (constitution P7).

## Key entities

- **Product:** something the shop sells and keeps stock of. In this feature only its Stock quantity changes; its Product information (name, SKU, price, description) and Product status (Draft or Active) are read but never changed.
- **Stock adjustment:** an action that adds a number of units to, or removes a number of units from, one Product's Stock quantity. Attributes: the Product, the add/remove choice, the number of units (1 to 1,000,000). It is applied and then not kept (no history).

## Success criteria

- **SC-001:** The Admin completes a Stock adjustment, from the Product details to the confirmed new Stock quantity, in under 1 minute without help (BG-01).
- **SC-002:** After any sequence of Stock adjustments, including refused adjustments and adjustments saved close together from two tabs, 100% of products have a whole-number Stock quantity from 0 to 1,000,000, and 0 saved adjustments are lost (BG-02, BR-035).
- **SC-003:** 0 Stock quantities change because of a cancelled, refused or invalid adjustment, or because of a press of the browser Back button.

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BG-01 | SC-001; US1, US2 |
| BG-02 | SC-002; FR-005, FR-006, FR-007, FR-008 |
| BR-003 | FR-006, FR-007, FR-008; US1 scenario 4, US2 scenario 3 |
| BR-015 | FR-016; US1 scenario 9; NFR-003 |
| BR-016 | FR-011; US1 scenario 1; edge case "adjustment and update close together" |
| BR-033 | FR-003; US1 scenarios 3 and 5 |
| BR-034 | FR-006, FR-007; US1 scenario 4, US2 scenarios 3 and 4 |
| BR-035 | FR-005; US1 scenario 7, US2 scenario 5; SC-002 |
| BR-049 | FR-015; US3 scenarios 1–3 |
| QE-02 | NFR-002 |
| QE-03 | NFR-003 |
| QE-04 | NFR-004; FR-018 |
| QE-05 | NFR-005; FR-003, FR-004 |
| PR-07 exceptions | Edge cases; FR-003, FR-006, FR-007, FR-012, FR-013 |

## Assumptions

- Admin sign-in, session expiry and signing in again are delivered by R002 (Admin sign-in), which is not yet written. This spec assumes a signed-in Admin session exists and does not specify how it is obtained.
- The Product details view (003-product-details, BL-03) exists. The Stock adjustment is started from there only.
- A Stock adjustment needs no confirmation step before it is applied. PR-07 has none, and a mistaken adjustment can be undone with the opposite adjustment.
- Neither add nor remove is preselected, so that the Admin never applies the wrong direction by accident.
- The number of units accepts digits only; leading and trailing spaces are ignored. Thousands separators are rejected rather than interpreted, because "1.000" could mean 1 or 1,000 (BR-050 uses a dot as thousands separator for prices).
- Stock adjustments are allowed for Draft and Active products alike; R001 does not restrict them by Product status.
- After a refused adjustment, the Product details refresh to the stored Stock quantity so the Admin sees the real value.
- No history, reason or author is recorded for an adjustment (R001 assumptions).

## Out of scope

- The Starting stock quantity of 1 when a product is added: 001-add-product (BL-01).
- Showing the Stock quantity in the Product list: 002-product-list (BL-02).
- The Product details view itself: 003-product-details (BL-03).
- Editing Product information, and keeping the Stock quantity out of the edit form: 004-update-product (BL-04).
- Publishing a product: 005-publish-product (BL-05).
- Deleting a product, including the rule that a deleted product cannot be adjusted (BR-048): 007-delete-product (BL-06). This spec only reports "product not found".
- Filtering the Product list by stock (in stock / out of stock): 010-product-filters (BL-10).
- Sorting the Product list by Stock quantity: 012-product-sorting (BL-11).
- Starting a Stock adjustment from the Product list, setting the Stock quantity to an exact value, and adjusting several products at once: not requested.
- Stock movement history, adjustment reasons, low-stock alerts, automatic stock deduction from sales, and several warehouses: out of scope in R001.
- Admin sign-in: R002.

## Open questions

- [ ] Should one Save apply a Stock adjustment at most once, even when the Admin double-clicks Save or retries after a network error in which the first save actually reached the system? If yes, after a lost response should the Admin be told "the adjustment may already have been applied" rather than "not saved"? Recommended: yes, apply at most once (same principle as BR-028 for adding a product), because a duplicate adjustment silently corrupts the Stock quantity. This is a new business rule: R001 has no such rule for stock adjustments, and BR-035 ("adjustments made close together both count") would need rewording, so choosing "yes" requires `/sdd-amend requirements R001` before this spec is approved. (affects: FR-014, FR-015, US3, SC-002)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-16 | Initial version | — |
