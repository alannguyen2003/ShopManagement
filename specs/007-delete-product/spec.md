# Spec: Delete a product

**Feature:** 007-delete-product
**Branch:** feat/007-delete-product
**Source:** R001/BL-06
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-16

## Context

The Admin needs to delete products that are no longer wanted, such as a Draft product added by mistake or a product that is out of stock and will not be restocked. An Active product that is still in stock must never be deleted (R001 BG-03). Deletion must be safe and reversible: a deleted product disappears from the product list and the product details, but it is kept with all its data so it can be restored later (restore is delivered by 011-restore-products / BL-12). This feature builds on the product list (002 / BL-02), the product details (003 / BL-03), publishing (005 / BL-05) and stock adjustment (006 / BL-08), which together make it possible for a product to meet the delete condition. Admin sign-in comes from a separate requirements document, R002, and is not specified here.

## User stories

### US1 — Delete a qualifying product from its product details (Priority: P1)

As the Admin, I want to delete a Draft or out-of-stock product from its product details, after confirming, so that the product list only shows products I still want to manage.

**Why P1:** This is the core of the feature and the only way in R001 to remove an unwanted product (BG-01).

**Independent test:** With only 002 and 003 built plus this story, add a product (it is Draft with stock quantity 1), open its product details, delete it and confirm; check that it is gone from the product list and its totals, that its product details report "product not found", and that the stored product still has its product information, stock quantity and product status.

**Acceptance scenarios:**
1. **Given** a Draft product "Blue Mug" (SKU MUG-01) with stock quantity 12, **When** the Admin chooses Delete in its product details, **Then** the system asks the Admin to confirm and shows the product's name and SKU; nothing is deleted yet.
2. **Given** the confirmation for "Blue Mug" is shown, **When** the Admin confirms, **Then** the product is deleted, the product list is shown with a message that "Blue Mug" was deleted, "Blue Mug" is not in the product list and the total number of products is one lower.
3. **Given** an Active product with stock quantity 0, **When** the Admin chooses Delete and confirms, **Then** the product is deleted.
4. **Given** a Draft product with stock quantity 0, **When** the Admin chooses Delete and confirms, **Then** the product is deleted.
5. **Given** the confirmation is shown, **When** the Admin cancels it, **Then** nothing is deleted and the product details stay shown, unchanged.
6. **Given** "Blue Mug" has been deleted, **When** the Admin opens its product details (e.g. with the browser Back button or a saved link), **Then** the system shows "product not found" and a way back to the product list.
7. **Given** "Blue Mug" (Draft, stock quantity 12, price 125.000 ₫) has been deleted, **When** its stored data is inspected, **Then** its product information, stock quantity 12 and product status Draft are unchanged, and the date and time of deletion are recorded.

### US2 — Protect Active products that are in stock (Priority: P1)

As the Admin, I want the system to refuse to delete an Active product that still has stock, and to tell me why, so that I never lose a product the shop is still selling.

**Why P1:** It enforces the delete rule (BR-004) and business goal BG-03 (0 such deletions); without it, US1 would be unsafe.

**Independent test:** Publish a product (005) so it is Active with stock quantity 1, try to delete it from its product details, and check that it is refused with the reason and stays unchanged; also send a delete request for it directly, without using the page, and check it is refused the same way.

**Acceptance scenarios:**
1. **Given** an Active product with stock quantity 3, **When** the Admin chooses Delete, **Then** the system does not ask for confirmation, refuses the deletion and tells the Admin that only a Draft or out-of-stock product can be deleted, showing the product's current product status (Active) and stock quantity (3); the product is unchanged.
2. **Given** a Draft product with stock quantity 5 whose confirmation is open in one tab, **When** the product is published in another tab and the Admin then confirms in the first tab, **Then** the deletion is refused with the reason, and the product stays Active with stock quantity 5.
3. **Given** an Active product with stock quantity 0 whose confirmation is open in one tab, **When** 4 units are added to its stock in another tab and the Admin then confirms in the first tab, **Then** the deletion is refused with the reason, and the product keeps stock quantity 4.
4. **Given** the product details show an Active product with stock quantity 2, but its stored stock quantity has meanwhile been reduced to 0 in another tab, **When** the Admin chooses Delete, **Then** the system asks for confirmation (the check uses the stored product, not the screen), and confirming deletes it.
5. **Given** an Active product with stock quantity 1, **When** a delete request for it is sent directly, bypassing the product management page, **Then** it is refused and the product is unchanged.

### US3 — Deleted products are gone for every other operation (Priority: P1)

As the Admin, I want a deleted product to behave as gone everywhere, so that I cannot change it by mistake from an old screen or another tab.

**Why P1:** Without it, a deleted product could still be edited, published or adjusted from a stale view, and its data would no longer be "kept unchanged" for restore (BR-044, BR-048).

**Independent test:** Delete a product, then, from views opened before the deletion, try to save its edit form, adjust its stock, publish it and delete it again; check that each reports "product not found" and that the stored product is unchanged.

**Acceptance scenarios:**
1. **Given** the edit form of a product is open in one tab and the product is deleted in another tab, **When** the Admin saves the edit form, **Then** the system reports "product not found" and nothing is saved.
2. **Given** a deleted product, **When** a stock adjustment for it is saved, **Then** the system reports "product not found" and its stock quantity is unchanged.
3. **Given** a deleted Draft product, **When** publishing it is requested, **Then** the system reports "product not found" and its product status stays Draft.
4. **Given** a deleted product, **When** deleting it again is requested, **Then** the system reports "product not found" and nothing changes (including the recorded deletion date).
5. **Given** a deleted product with SKU "MUG-01", **When** the Admin adds a new product, or updates another product, with SKU "MUG-01" (or "mug-01 "), **Then** the save is rejected with a message on the SKU field, because the deleted product's SKU stays reserved.

### US4 — Delete from the product list (Priority: P2)

As the Admin, I want to delete a product directly from its row in the product list, so that I can clean up several products without opening each one.

**Why P2:** Convenience; US1 already lets the Admin delete any qualifying product.

**Independent test:** On page 2 of a product list with 45 products, delete a Draft product from its row and confirm; check that the list stays on page 2, the product is gone and the total shows 44.

**Acceptance scenarios:**
1. **Given** the product list shows page 2 of 45 products, **When** the Admin chooses Delete on the row of a Draft product and confirms, **Then** the list stays on page 2 without that product, with a message naming the deleted product, and the total shows 44.
2. **Given** the row of an Active product with stock quantity 7, **When** the Admin chooses Delete, **Then** the deletion is refused with the reason and the current product status and stock quantity; the list is unchanged.
3. **Given** the last page holds a single product, **When** the Admin deletes it, **Then** the list shows the new last page (BR-020).
4. **Given** only one product exists, **When** the Admin deletes it, **Then** the list shows the empty-list message with a way to add a product (BR-022).
5. **Given** the list shows a product that was deleted meanwhile in another tab, **When** the Admin chooses Delete on its row, **Then** the system reports "product not found" and the list is refreshed without it.
6. **Given** the confirmation is shown from the list, **When** the Admin cancels, **Then** nothing is deleted and the list stays at the same page, search text, filters and sort order.

## Edge cases

- A product that is both Draft and out of stock qualifies (either condition is enough, BR-004).
- A Draft product with the maximum stock quantity (1,000,000) qualifies; stock quantity does not matter for a Draft product.
- An Active product with stock quantity 1 is refused; only exactly 0 qualifies for an Active product.
- The product changes between the moment the Admin chooses Delete and the moment the Admin confirms (published, or stock added, in another tab) → the deletion is refused at confirmation with the reason; the view is refreshed to show the current product status and stock quantity (BR-025).
- The screen shows stale data that would refuse the deletion, but the stored product now qualifies → the stored product decides; the deletion is allowed (BR-025).
- The Admin clicks Confirm twice, or the same delete request arrives twice → the product is deleted once; the second request reports "product not found" and changes nothing; the Admin sees one successful deletion.
- The product was deleted meanwhile (in another tab) → "product not found"; nothing changes; the list or details are refreshed.
- A delete request refers to a product that never existed, or carries a malformed product reference → "product not found" or a rejection at the boundary; nothing changes.
- The connection is lost, or the session has expired, while deleting → the Admin is told the deletion did not complete; the current view stays shown and the Admin can retry (after signing in again, per R002). If the deletion had in fact completed, the retry reports "product not found".
- A request without a signed-in Admin session → refused; nothing changes.
- After deleting from the product details, the Admin presses the browser Back button → the product details report "product not found"; the deleted product's data is never shown again from that view.
- Deleting the last product of the last page → the last existing page is shown (BR-020); deleting the only product → the empty-list message (BR-022).
- When search text (009) or filters (010) are in use, deleting a product removes it from the matching products and their totals; when nothing matches any more, the "no matching products" message of those specs applies.
- 10,000 products stored → the deletion still completes within the NFR-002 target.

## Functional requirements

- **FR-001:** The system MUST offer a Delete action in the product details and on each row of the product list, for every product (BR-024).
- **FR-002:** The system MUST allow a product to be deleted only if its product status is Draft or its stock quantity is 0 (BR-004).
- **FR-003:** The system MUST check the delete condition of FR-002 against the stored product, both when the Admin chooses Delete and again at the moment the confirmed deletion is carried out, never against what the screen showed (BR-025).
- **FR-004:** When the delete condition is not met, the system MUST refuse the deletion without asking for confirmation, MUST tell the Admin that only a Draft or out-of-stock product can be deleted, showing the product's current product status and stock quantity, and MUST leave the product unchanged (BR-005, BG-03).
- **FR-005:** When the delete condition is met, the system MUST ask the Admin to confirm, naming the product (name and SKU), and MUST NOT delete anything until the Admin confirms; cancelling MUST leave the product unchanged and keep the current view (BR-006).
- **FR-006:** When the condition fails at the moment of the confirmed deletion (FR-003), the system MUST refuse it as in FR-004 and show the product's current product status and stock quantity (BR-005, BR-025).
- **FR-007:** Deleting a product MUST keep it with its product information, stock quantity and product status unchanged, so it can be restored later (BR-044), and MUST record the date and time of deletion, used by the deleted products list of 011-restore-products (BL-12).
- **FR-008:** A deleted product MUST NOT appear in the product list, in its totals, or in any search or filter result, and its product details MUST NOT open; a request for them MUST report "product not found" with a way back to the product list (BR-007).
- **FR-009:** A request to update the product information, adjust the stock, publish or delete a deleted product MUST report "product not found" and change nothing (BR-048).
- **FR-010:** A delete request for a product that is already deleted MUST report "product not found" and MUST NOT change its recorded deletion date (BR-048, PR-06 exception). Deleting a product again after it has been restored (011-restore-products, BL-12) is a normal deletion with a new deletion date.
- **FR-011:** A deleted product's SKU MUST keep counting for SKU uniqueness when a product is added (001) or updated (004), so no other product can take that SKU while the deleted product is kept (BR-045, as delivered by BL-01).
- **FR-012:** After a successful deletion, the system MUST show the product list with a message naming the deleted product. After a deletion from the list, the list MUST stay at the same page, page size, search text, filters and sort order; after a deletion from the product details, the list MUST be shown at the page, search text, filters and sort order the Admin came from (BR-023). If that page no longer exists, the last existing page is shown (BR-020); if no products remain, the empty-list message is shown (BR-022).
- **FR-013:** When a delete request cannot be completed because of a network error or an expired session, the system MUST tell the Admin that the deletion did not complete, keep the current view, and let the Admin retry.
- **FR-014:** Only the signed-in Admin MUST be able to delete a product; a delete request without a signed-in Admin session MUST be refused and change nothing (BR-015, constitution P5).
- **FR-015:** The system MUST validate the product reference of a delete request at the boundary and reject a malformed one without deleting anything (constitution P6).

## Non-functional requirements

NFR numbers match the R001 quality expectation numbers (QE-02 → NFR-002, and so on); NFR-001 is not used because QE-01 belongs to the product list (002, 009–012).

- **NFR-002:** 95% of delete outcomes (deleted, refused or "product not found"), with 10,000 products stored, are shown to the Admin within 1 second of choosing Delete or confirming, measured in the browser (QE-02).
- **NFR-003:** 100% of delete requests without a signed-in Admin session are refused, and 0 products are changed by them (QE-03).
- **NFR-004:** Choosing Delete, the confirmation, cancelling, the refusal message and the product list shown after a deletion cause 0 full page reloads; the browser Back button returns to the previous view, including the product list page number, search text, filters and sort order, and a Back step to a deleted product's details shows "product not found" (QE-04).

## Key entities (if any)

- **Product:** something the shop sells and keeps stock of: product information (name, SKU, price, description), stock quantity and product status (Draft or Active). This feature adds whether the product is deleted and, if so, the date and time it was deleted. A deleted product keeps all its other data unchanged and its SKU stays reserved.

## Success criteria

- **SC-001:** 0 Active products with a stock quantity above 0 are deleted, including through direct requests that bypass the product management page and through changes made in another tab between choosing Delete and confirming (BG-03).
- **SC-002:** The Admin deletes a qualifying product, from the product list or the product details, including the confirmation, in under 1 minute without help (BG-01).
- **SC-003:** 100% of deleted products keep their product information, stock quantity and product status exactly as they were before deletion (BR-044).
- **SC-004:** After a deletion, the product appears 0 times in the product list, its totals, search and filter results, and its product details are never shown again until it is restored (BR-007).

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BG-01 | SC-002; US1, US4 |
| BG-03 | SC-001; US2 (scenarios 1–5); FR-004 |
| BR-004 | FR-002; US1 scenarios 3–4; US2 scenario 1; edge cases (Draft + 0, Draft + 1,000,000, Active + 1) |
| BR-005 | FR-004, FR-006; US2 scenarios 1–3; US4 scenario 2 |
| BR-006 | FR-005; US1 scenarios 1, 5; US4 scenario 6 |
| BR-007 | FR-008; US1 scenarios 2, 6; SC-004; edge cases (Back button, search/filters) |
| BR-044 | FR-007; US1 scenario 7; SC-003; Key entities |
| BR-048 | FR-009, FR-010; US3 scenarios 1–4 |
| BR-015 | FR-014; NFR-003; edge cases |
| BR-024 | FR-001; US1 (details), US4 (list) |
| BR-025 | FR-003, FR-006; US2 scenarios 2–4; edge cases |
| QE-02 | NFR-002 |
| QE-03 | NFR-003 |
| QE-04 | NFR-004; FR-012; edge case (Back button after deletion) |
| (supporting, not in Covers) BR-045 | FR-011; US3 scenario 5 — deletion must keep the SKU counted for uniqueness in 001/004 |
| (supporting) BR-020, BR-022, BR-023 | FR-012; US4 scenarios 3–4 |
| (supporting) BR-046 | FR-007 (deletion date recorded for 011) |

## Assumptions

- The Delete action is offered for every product, including those that do not qualify; a non-qualifying product is refused with the reason when the Admin chooses Delete (follows R001 PR-06 step 2 and its first exception), rather than the action being hidden or disabled.
- The confirmation names the product by name and SKU; no extra step (such as typing the name) is needed.
- The refusal message shows the product's current product status and stock quantity so the Admin knows what to change (publish or stock adjustment cannot make an Active product deletable except by bringing its stock to 0 through 006).
- After a refusal caused by a change made elsewhere, the view is refreshed to show the stored product status and stock quantity.
- A successful deletion shows a short message naming the deleted product; there is no "undo" in this feature (restore is 011 / BL-12).
- "Product not found" is the same message for a product that never existed and for a deleted one.
- The date and time of deletion is recorded in this feature because 011 (BL-12, BR-046) shows it and orders the deleted products list by it.
- Admin sign-in, session expiry and signing in again are provided by R002 and are taken as given.
- 001 (BL-01), 002 (BL-02), 003 (BL-03), 004 (BL-04), 005 (BL-05) and 006 (BL-08) are built before this feature; FR-008, FR-009 and FR-011 extend their behavior to deleted products.

## Out of scope

- Viewing the deleted products list and restoring a deleted product: 011-restore-products (BL-12).
- Permanently erasing a deleted product, and bulk deletion: out of scope in R001.
- Undoing a deletion other than through restore (011 / BL-12).
- Recording who deleted a product or why, and any audit trail: out of scope in R001 (single Admin, P1).
- Moving an Active product back to Draft to make it deletable: forbidden by BR-012.
- Adding a product (001 / BL-01), the paged product list (002 / BL-02), product details (003 / BL-03), updating product information (004 / BL-04), publishing (005 / BL-05), stock adjustment (006 / BL-08), page size (008 / BL-07), search (009 / BL-09), filters (010 / BL-10) and sorting (012 / BL-11), except that each must treat deleted products as described in FR-008, FR-009 and FR-011.
- Admin sign-in, account provisioning, lockout and session timeout: R002.

## Open questions

None.

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-16 | Initial version | — |
