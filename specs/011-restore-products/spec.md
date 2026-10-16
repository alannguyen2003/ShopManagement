# Spec: View and restore deleted products

**Feature:** 011-restore-products
**Branch:** feat/011-restore-products
**Source:** R001/BL-12
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-16

## Context

Deleting a product hides it but keeps it, with its product information, stock quantity and product status (BR-044, delivered by 007-delete-product / BL-06). Without a way to see and restore deleted products, the Admin cannot undo a deletion made by mistake, and the product's SKU stays reserved for good (BR-045). This feature gives the Admin a deleted products list on the product management page and a restore action that brings a deleted product back into the product list unchanged. It is the second half of making deletion safe and recoverable.

## User stories

### US1 — See the deleted products (Priority: P1)

As the Admin, I want to open a list of the products I have deleted so that I can find a product I deleted by mistake.

**Why P1:** Restoring (US2) is impossible without first seeing which products are deleted.

**Independent test:** Delete three products (through 007-delete-product or with prepared data), open the deleted products list from the product management page and check that exactly those three appear, with name, SKU and deletion date, most recently deleted first.

**Acceptance scenarios:**
1. **Given** 5 products that are not deleted and 3 deleted products "Blue Mug" (SKU MUG-01, deleted at 10:00), "Red Plate" (SKU PL-02, deleted at 11:00) and "Tea Pot" (SKU TP-03, deleted at 12:00) on the same day, **When** the Admin opens the deleted products list from the product management page, **Then** it shows exactly 3 rows in the order Tea Pot, Red Plate, Blue Mug, each showing name, SKU and deletion date, and none of the 5 other products.
2. **Given** no deleted products exist, **When** the Admin opens the deleted products list, **Then** an empty-list message is shown and no rows.
3. **Given** a deleted product, **When** the Admin looks at its row, **Then** the row shows only name, SKU and deletion date and offers a restore action; it offers no way to open the product details, edit, adjust the stock, publish or delete it.
4. **Given** the Admin is on page 2 of the product list, **When** the Admin opens the deleted products list and then presses the browser Back button, **Then** the product list is shown again at page 2 with the same search text, filters and sort order, and no full page reload happened.
5. **Given** no signed-in Admin session, **When** the deleted products list is requested, **Then** the request is refused and no deleted product data is returned.

### US2 — Restore a deleted product (Priority: P1)

As the Admin, I want to restore a deleted product so that a product deleted by mistake is back in the product list with all its data.

**Why P1:** This is the value of the backlog item: undoing a deletion without re-entering anything.

**Independent test:** Delete a product, restore it from the deleted products list, and check that it appears in the product list with the same product information, stock quantity and product status as before the deletion, and no longer appears in the deleted products list.

**Acceptance scenarios:**
1. **Given** a Draft product "Blue Mug" (SKU MUG-01, price 125.000 ₫, description "Ceramic, 350 ml", stock quantity 12) that was deleted, **When** the Admin chooses restore on its row, **Then** the product leaves the deleted products list, a confirmation naming "Blue Mug" is shown, and the product list shows "Blue Mug" again as Draft with stock quantity 12, SKU MUG-01, price 125.000 ₫ and the same description.
2. **Given** an Active product with stock quantity 0 that was deleted, **When** the Admin restores it, **Then** it is back in the product list as Active with stock quantity 0.
3. **Given** a deleted product, **When** the Admin chooses restore, **Then** the product is restored immediately, without a confirmation step (BR-047).
4. **Given** a product was added on 1 October, deleted, and restored on 16 October, **When** the Admin views the product list in its default order (newest first), **Then** the product is placed by its original date added (1 October), not by the restore date.
5. **Given** a deleted product was restored from another browser tab, **When** the Admin chooses restore on it in this tab, **Then** the system reports "product not found", nothing changes, and the deleted products list is refreshed without that product.
6. **Given** a restored product, **When** the Admin opens its product details, edits it, adjusts its stock, publishes it (if Draft) or deletes it, **Then** each operation behaves as for any product that was never deleted.
7. **Given** a deleted product with SKU MUG-01, **When** it is restored, **Then** it keeps SKU MUG-01 and no two products in the product list share that SKU (BR-045).
8. **Given** the Admin chooses restore and the request fails (network error or expired session), **When** the failure is reported, **Then** the Admin is told the product was not restored, the product stays in the deleted products list, and the Admin can retry, after signing in again if needed.
9. **Given** no signed-in Admin session, **When** a restore is requested, **Then** it is refused and the product stays deleted.

### US3 — Page through many deleted products (Priority: P2)

As the Admin, I want the deleted products list to be paged so that I can find an older deletion when many products have been deleted.

**Why P2:** With fewer than 21 deleted products, US1 and US2 already work on a single page; paging matters only as deletions accumulate.

**Independent test:** Prepare 45 deleted products with distinct deletion moments, open the deleted products list and move between pages 1, 2 and 3, checking order, page contents and totals.

**Acceptance scenarios:**
1. **Given** 45 deleted products, **When** the Admin opens the deleted products list, **Then** page 1 shows the 20 most recently deleted, and the total shows 45 deleted products and 3 pages.
2. **Given** 45 deleted products, **When** the Admin moves to page 3, **Then** the 5 least recently deleted products are shown.
3. **Given** 45 deleted products, **When** page 10 is requested, **Then** page 3 (the last existing page) is shown.
4. **Given** 41 deleted products and the Admin is on page 3, which shows 1 product, **When** the Admin restores that product, **Then** page 2 (now the last page) is shown and the totals show 40 deleted products and 2 pages.
5. **Given** the Admin moved from page 1 to page 2 of the deleted products list, **When** the Admin presses the browser Back button, **Then** page 1 of the deleted products list is shown, without a full page reload.
6. **Given** a page number that is not a whole number of 1 or more (e.g. "abc", 0, -1) is requested, **When** the deleted products list is shown, **Then** page 1 is shown.

## Edge cases

- No deleted products → empty-list message (FR-006).
- The same restore is submitted twice (double click, or two tabs at once) → the product is restored once; the other request reports "product not found" (FR-011).
- A restore is requested for a product that is not deleted (already restored, or never deleted) or that never existed → "product not found"; nothing changes (FR-010).
- A product is deleted from another tab while the deleted products list is open → it appears after the list is next loaded; the list is not updated live (assumption).
- Restoring the only product on the last page → the last existing page is shown (FR-005).
- Two products deleted at the same moment → ordered as described in FR-003.
- The SKU of the deleted product used by another product → cannot happen: BR-045 keeps it reserved on add (001) and update (004) (FR-012).
- Session expires while the deleted products list is open, then the Admin chooses restore → refused, the Admin is told the product was not restored and can retry after signing in again (FR-014).
- Network error during restore → same as above; if the first attempt actually succeeded, the retry reports "product not found" and the refreshed list no longer shows the product.
- A product that was restored and deleted again → it appears in the deleted products list once, with the new deletion date.
- Very large data: 10,000 stored products, many of them deleted → paging keeps each page at 20 rows (NFR-001, NFR-002).

## Functional requirements

- **FR-001:** The product management page MUST offer the Admin a way to open the deleted products list, without a full page reload (PR-08, QE-04).
- **FR-002:** The deleted products list MUST show only deleted products, each row showing the product's name, SKU and deletion date (BR-046).
- **FR-003:** The deleted products list MUST show 20 deleted products per page, ordered by deletion date, most recently deleted first; products deleted at the same moment MUST be ordered by date added, newest first (BR-046; tie rule is an assumption).
- **FR-004:** The deleted products list MUST show the total number of deleted products and of pages, and a way to move between pages.
- **FR-005:** When the requested page is beyond the last page of the deleted products list, the system MUST show the last existing page.
- **FR-006:** When no deleted products exist, the deleted products list MUST show an empty-list message (PR-08).
- **FR-007:** Each row of the deleted products list MUST offer a restore action, and MUST NOT offer opening the product details, editing, adjusting the stock, publishing or deleting the deleted product (BR-048).
- **FR-008:** Restoring MUST take effect at once, without a confirmation step (BR-047).
- **FR-009:** Restoring MUST bring the product back with its product information (name, SKU, price, description), stock quantity, product status and date added exactly as they were when it was deleted (BR-044, BR-047).
- **FR-010:** The system MUST check, against the stored product at the moment of the restore, that the product is deleted; if it is not deleted (already restored or never deleted) or does not exist, the system MUST report "product not found" and change nothing (PR-08).
- **FR-011:** One restore action MUST restore a product at most once; a repeated request for the same product MUST report "product not found".
- **FR-012:** A restored product MUST keep its SKU. BR-045, enforced when a product is added (001-add-product FR-005) and updated (004-update-product FR-007), guarantees that no other product holds that SKU, so a restore never results in two products with the same SKU (BR-045).
- **FR-013:** After a restore, the product MUST no longer appear in the deleted products list and MUST appear in the product list, counted in its totals and matched by its search text, filters and sort order like any other product; all product operations MUST be available for it again (BR-007 and BR-048 no longer apply).
- **FR-014:** After a successful restore, the system MUST show a confirmation naming the restored product and MUST show the refreshed deleted products list on the same page, or on the last existing page if that page no longer exists.
- **FR-015:** When a restore fails (network error or expired session), the system MUST tell the Admin the product was not restored, keep it in the deleted products list and let the Admin retry, after signing in again if needed.
- **FR-016:** Only the signed-in Admin MUST be able to view the deleted products list or restore a product; any request without a signed-in Admin session MUST be refused and return no product data (BR-015).
- **FR-017:** The page number of the deleted products list MUST be validated at the boundary; a value that is not a whole number of 1 or more MUST show page 1.

## Non-functional requirements

- **NFR-001:** 95% of deleted products list pages (20 rows, 10,000 products stored) are shown within 2 seconds of the Admin's action, measured in the browser. [NEEDS CLARIFICATION: R001 sets no target for the deleted products list (QE-01 covers the product list only); should QE-01's 2-second target apply here?]
- **NFR-002:** 95% of restores (10,000 products stored) are confirmed to the Admin within 1 second of the action, measured in the browser (QE-02).
- **NFR-003:** 100% of deleted products list views and restores are refused without a signed-in Admin session, and a refused request reveals no product data (QE-03).
- **NFR-004:** Moving between the product list and the deleted products list, and between pages of the deleted products list, causes 0 full page reloads; the browser Back button returns to the previous view, including the deleted products list page number and the product list's page number, search text, filters and sort order (QE-04).
- **NFR-005:** 0 error messages shown to the Admin contain stack traces or internal details (constitution P7).

## Key entities (if any)

- **Product:** something the shop sells and keeps stock of: product information (name, SKU, price, description), stock quantity, product status (Draft or Active) and date added. When deleted it is kept, with a deletion date, and hidden from the product list; restoring clears the deleted state and leaves everything else unchanged.
- **Deleted products list:** the paged list of deleted products (name, SKU, deletion date), 20 per page, most recently deleted first, from which the Admin can restore one.

## Success criteria

- **SC-001:** The Admin can find a deleted product among the 20 most recent deletions and restore it in under 1 minute, without outside help.
- **SC-002:** 100% of restored products have the same product information, stock quantity, product status and date added as at the moment they were deleted.
- **SC-003:** 0 products appear in both the product list and the deleted products list at the same time, and 0 restores produce two products with the same SKU.

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BR-015 | FR-016; US1 scenario 5; US2 scenario 9; NFR-003 |
| BR-044 | FR-009; US2 scenarios 1, 2; SC-002; Context |
| BR-045 | FR-012; US2 scenario 7; SC-003; Edge cases |
| BR-046 | FR-002, FR-003; US1 scenario 1; US3 scenarios 1, 2 |
| BR-047 | FR-008, FR-009; US2 scenarios 1, 2, 3 |
| QE-02 | NFR-002 |
| QE-03 | NFR-003; FR-016 |
| QE-04 | NFR-004; FR-001; US1 scenario 4; US3 scenario 5 |
| PR-08 exceptions (empty list, already restored) | FR-006, FR-010; US1 scenario 2; US2 scenario 5; Edge cases |
| BR-048 (context, owned by BL-06) | FR-007, FR-013; US1 scenario 3; US2 scenario 6 |

## Assumptions

- Admin sign-in and the signed-in Admin session come from R002 (Admin sign-in), built before this feature; this spec does not specify sign-in.
- 007-delete-product (BL-06) records the moment each product is deleted; this feature only reads it.
- The deletion date is shown as date and time in the Admin's browser local time, so several deletions on the same day can be told apart.
- The deleted products list has a fixed page size of 20; the page-size choice of 008-list-page-size (BL-07) applies only to the product list.
- The deleted products list is not updated live; changes made in another tab appear when the list is next loaded.
- A restored product keeps its original date added, so it reappears in the product list at its original position, not at the top.
- A product can be deleted and restored any number of times.
- An invalid page number shows page 1 rather than an error (FR-017), matching the forgiving page handling of the product list.

## Out of scope

- Deleting a product, the delete rule and the confirmation step (007-delete-product, BL-06).
- The product list itself, its totals and empty-list message (002-product-list, BL-02).
- Product details (003-product-details, BL-03); viewing the product details of a deleted product is not offered.
- Editing, publishing or adjusting the stock of a deleted product (004-update-product BL-04, 005-publish-product BL-05, 006-adjust-stock BL-08; refused for deleted products under BR-048).
- Choosing the page size (008-list-page-size, BL-07), searching (009-product-search, BL-09), filtering (010-product-filters, BL-10) and sorting (012-product-sorting, BL-11) — none of these apply to the deleted products list.
- Enforcing SKU reservation when adding or updating a product (001-add-product BL-01, 004-update-product BL-04).
- Restoring several products at once; permanently erasing a deleted product (R001 out of scope).
- Admin sign-in, lockout and session handling (R002).

## Open questions

- [ ] Should QE-01's target (95% of pages within 2 seconds, 10,000 products stored) also apply to the deleted products list? (affects: NFR-001)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-16 | Initial version | — |
