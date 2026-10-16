# Spec: View product details

**Feature:** 003-product-details
**Branch:** feat/003-product-details
**Source:** R001/BL-03
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-16

## Context

The product list (002-product-list, BL-02) shows only a summary of each product: name, SKU, price, stock quantity and product status. The description is not shown there (BR-019), and there is no single place where the Admin can check one product completely. This feature adds product details. The Admin selects a product in the product list, sees all its product information, its stock quantity and its product status, and goes back to the product list where they left it. Later features attach their edit (BL-04), publish (BL-05), stock adjustment (BL-08) and delete (BL-06) actions to the product details, so this view is part of the MVP (requirements R001, PR-03).

## User stories

### US1 — See one product's details (Priority: P1)

As the Admin, I want to open one product from the product list and see all its product information, its stock quantity and its product status, so that I can check everything about that product in one place.

**Why P1:** This is the core of BL-03. The edit, publish, stock adjustment and delete features all start from the product details.

**Independent test:** With products already stored, sign in, open the product list, select one product. The product details show its name, SKU, price, description, stock quantity and product status, matching the stored values. A way back leads to the product list.

**Acceptance scenarios:**
1. **Given** a signed-in Admin and a stored product "Blue Mug", SKU "MUG-01", price 125000 VND, description "Ceramic, 350 ml", stock quantity 1, product status Draft, **When** the Admin selects "Blue Mug" in the product list, **Then** the product details show name "Blue Mug", SKU "MUG-01", price "125.000 ₫", description "Ceramic, 350 ml", stock quantity 1 and product status "Draft", without a full page reload.
2. **Given** a stored Active product with stock quantity 0 and price 0, **When** the Admin opens its product details, **Then** they show product status "Active", stock quantity 0 and price "0 ₫".
3. **Given** a stored product with no description, **When** the Admin opens its product details, **Then** the description area shows a "No description" indication instead of being blank.
4. **Given** a stored product with a 200-character name and a 2,000-character description that contains line breaks and the text `<b>sale</b>`, **When** the Admin opens its product details, **Then** the name and description are shown in full with no truncation, the line breaks are kept, and `<b>sale</b>` appears as literal text.
5. **Given** the Admin has the product list open in one tab and the product "Blue Mug" is deleted in another tab, **When** the Admin selects "Blue Mug" in the first tab, **Then** a "product not found" message is shown with a way back to the product list, and no product information is shown.
6. **Given** a product details address that refers to a product that never existed, or a malformed product identifier, **When** the Admin opens it, **Then** a "product not found" message is shown with a way back to the product list, with no internal details.
7. **Given** no signed-in Admin session, **When** the product details of any product, or that product's data, are requested, **Then** the request is refused and no product information is revealed.
8. **Given** the Admin is viewing a product's details, **When** the Admin reloads the browser, **Then** the same product's details are shown again, using the stored values at that moment.

### US2 — Return to the product list where I left it (Priority: P2)

As the Admin, I want to go back from the product details to the same page of the product list, with the same settings, so that I can go on checking products without finding my place again.

**Why P2:** US1 already lets the Admin view a product and go back to the product list. This story keeps the Admin's place in the list (BR-023, QE-04). It is required for BL-03 to be complete, but the product details are usable without it.

**Independent test:** With more than 20 products stored, go to page 2 of the product list, open a product, then use the way back, and separately the browser Back button. Both show page 2.

**Acceptance scenarios:**
1. **Given** 45 stored products and the Admin on page 2 of the product list, **When** the Admin opens a product's details and then uses the way back to the product list, **Then** page 2 of the product list is shown, without a full page reload.
2. **Given** 45 stored products and the Admin on page 2 of the product list, **When** the Admin opens a product's details and then presses the browser Back button, **Then** page 2 of the product list is shown, without a full page reload.
3. **Given** the product list shows a chosen page size, search text, filters or sort order (when 008-list-page-size/BL-07, 009-product-search/BL-09, 010-product-filters/BL-10 or 012-product-sorting/BL-11 exist), **When** the Admin opens a product's details and goes back, **Then** the product list shows the same page with the same page size, search text, filters and sort order.
4. **Given** the Admin opened product details from page 3 of the product list, and products were then deleted in another tab so that only 2 pages remain, **When** the Admin goes back to the product list, **Then** the last existing page (page 2) is shown (BR-020, as defined in 002-product-list).
5. **Given** the Admin opened the product details directly from their address, with no earlier product list view in this tab, **When** the Admin uses the way back to the product list, **Then** page 1 of the product list is shown with the default settings.

## Edge cases

- The product was deleted, from another tab, between the list being shown and the product being selected: "product not found" with a way back. No product information is shown (BR-007, PR-03).
- The product identifier never existed, or is malformed (wrong format, empty, too long): it is rejected at the boundary and the Admin sees the same "product not found" message. The response contains no internal details (constitution P6, P7).
- The product changes in another tab (for example, it is edited, published or has its stock adjusted) while its details are open: the details keep showing the values loaded when they were opened. Reopening or reloading the details shows the current stored values. There is no live refresh.
- The session has expired when the details are opened or reloaded: the request is refused and no product information is shown. What the Admin then sees and where they land after signing in again are defined by R002 (Admin sign-in).
- Loading the details fails (for example, a network error): the Admin sees a message that the product details could not be loaded, with a way to retry and a way back to the product list. Partial or stale product information is never shown as if it were current.
- Boundary values are shown correctly: price 0 → "0 ₫", price 1,000,000,000 → "1.000.000.000 ₫", stock quantity 0 and 1,000,000. Name (200), SKU (50) and description (2,000) at their maximum lengths are shown in full.
- The description is empty: a "No description" indication is shown.
- The description or name contains markup-like or script-like text: it is shown as literal text and never interpreted.
- The Admin double-clicks a product in the list: one product details view opens, and the browser Back button returns to the product list, not to a second copy of the same details.
- The Admin goes back to a list page that no longer exists: the last existing page is shown (BR-020).

## Functional requirements

- **FR-001:** The Admin MUST be able to open the product details of any product shown in the product list by selecting it (PR-03).
- **FR-002:** The product details MUST show all product information (name, SKU, price and description) plus the stock quantity and the product status of the selected product (BR-014, BR-018).
- **FR-003:** The values shown in the product details MUST be the stored values at the moment the details are opened or reloaded.
- **FR-004:** The price MUST be shown in VND with a dot as thousands separator and the ₫ sign, for example "125.000 ₫" and "0 ₫" (BR-050).
- **FR-005:** The stock quantity MUST be shown as a whole number, including 0. It MUST use the same format as in the product list (002-product-list).
- **FR-006:** The product status MUST be shown as "Draft" or "Active" (BR-011).
- **FR-007:** When a product has no description, the product details MUST show a "No description" indication instead of an empty area.
- **FR-008:** Name, SKU and description MUST be shown in full, without truncation, exactly as stored and as plain text: line breaks are kept and markup is never interpreted.
- **FR-009:** The product details MUST offer a way back to the product list.
- **FR-010:** Going back to the product list, whether by the way back of FR-009 or by the browser Back button, MUST show the product list at the page number the Admin came from. It MUST also keep the page size, search text, filters and sort order the Admin came from, when those exist (BR-023).
- **FR-011:** When the page the Admin came from no longer exists, going back MUST show the last existing page of the product list (BR-020, as defined in 002-product-list).
- **FR-012:** When there is no earlier product list view to return to (the details were opened directly from their address), the way back MUST show page 1 of the product list with the default settings.
- **FR-013:** When the requested product does not exist, because it never existed or because it was deleted (BR-007), the system MUST show a "product not found" message with a way back to the product list. It MUST NOT show any product information.
- **FR-014:** The system MUST validate the product identifier of a product details request at the boundary. A malformed identifier MUST produce the same "product not found" outcome as FR-013 (constitution P6).
- **FR-015:** Only the signed-in Admin MAY view product details or request one product's data. A request without a signed-in Admin session MUST be refused without revealing any product information (BR-015).
- **FR-016:** The product details of a product MUST have their own address within the product management page, so that reloading the browser or opening that address shows the same product's details (QE-04).
- **FR-017:** When loading the product details fails, the system MUST tell the Admin that the details could not be loaded and MUST offer a way to retry and a way back to the product list.

## Non-functional requirements

- **NFR-003:** 100% of product details views and requests for one product's data are refused without a signed-in Admin session, and 0 product information fields are revealed in the refusal (QE-03, constitution P5).
- **NFR-004:** Moving from the product list to the product details and back causes 0 full page reloads. The browser Back button from the product details returns to the product list with the same page number, and with the same page size, search text, filters and sort order when those exist, in 100% of cases (QE-04).
- **NFR-006:** 95% of product details are shown within [NEEDS CLARIFICATION: what is the target time for showing product details? R001 sets 2 s for product list pages (QE-01) and 1 s for changes (QE-02), but none for viewing one product. Recommended: 1 s] of the Admin selecting the product, with 10,000 products stored, measured in the browser.
- **NFR-007:** 100% of "product not found", refused and failed responses contain no stack traces or internal details (constitution P7).

## Key entities (if any)

- **Product:** something the shop sells and keeps stock of. It has product information (name, SKU, price, description), a stock quantity and a product status (Draft or Active). A deleted product is kept but cannot be shown in the product details (BR-007, BR-044).
- **Product list:** the paged list of all products on the product management page. The Admin's position in it (page number, and page size, search text, filters and sort order when those exist) is what going back from the product details restores.

## Success criteria

- **SC-001:** Starting from any page of the product list, the Admin opens the product details of a product on that page and returns to the same page in under 1 minute, in 100% of acceptance test runs (BG-01).
- **SC-002:** For a test set covering the boundary values (price 0 and 1,000,000,000; stock quantity 0 and 1,000,000; maximum-length name, SKU and description; empty description; both product statuses), 100% of the values shown in the product details match the stored values.
- **SC-003:** In 100% of tests without a signed-in Admin session, no product information is shown or returned.

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BG-01 | SC-001; US1 (scenario 1); US2 (scenarios 1–2) |
| BR-014 | FR-002; US1 scenario 1 |
| BR-015 | FR-015; US1 scenario 7; NFR-003; SC-003 |
| BR-018 | FR-002, FR-003; US1 scenarios 1–2; SC-002 |
| BR-050 | FR-004; US1 scenarios 1–2; Edge cases (price boundaries) |
| BR-023 | FR-010, FR-011, FR-012; US2 scenarios 1–5 |
| QE-03 | NFR-003; FR-015; US1 scenario 7 |
| QE-04 | NFR-004; FR-016; US1 scenario 1; US2 scenarios 1–3 |
| PR-03 exception (product no longer exists) | FR-013; US1 scenario 5; Edge cases |

## Assumptions

- Admin sign-in and sessions are delivered by R002 (Admin sign-in), which is not written yet. This spec only requires that product details are refused without a signed-in Admin session. The sign-in page, session expiry handling and return after sign-in are defined by R002.
- The product details show exactly the fields BR-018 lists. The date added and the deletion state are not shown.
- The Admin selects a product by its row or name in the product list. The exact control is a detail for the plan, consistent with 002-product-list.
- The product details have no live refresh. Changes made in another tab appear when the details are reopened or reloaded (consistent with "last save wins", BR-026).
- The way back falls back to page 1 with the default settings when there is no earlier product list view in the tab (FR-012).
- An empty description shows a "No description" indication. The exact wording is a detail for the plan.
- Text is shown as plain text and never interpreted as markup. This is the safe default for content typed by the Admin.
- When loading fails, the Admin can retry and go back. R001 sets this behavior only for forms (BR-049); this spec applies the same spirit to viewing.
- NFR numbers match the R001 quality expectations they come from (NFR-003 = QE-03, NFR-004 = QE-04). NFRs specific to this spec start at NFR-006.

## Out of scope

- Showing the product list, its paging and its empty-list message: 002-product-list (BL-02).
- Editing product information, and the edit entry point on the product details: 004-update-product (BL-04).
- Publishing a product, and the publish entry point on the product details: 005-publish-product (BL-05).
- Adjusting stock, and the stock adjustment entry point on the product details: 006-adjust-stock (BL-08).
- Deleting a product, and the delete entry point on the product details: 007-delete-product (BL-06).
- Choosing the page size: 008-list-page-size (BL-07). This spec only keeps it when going back.
- Search text: 009-product-search (BL-09). This spec only keeps it when going back.
- Filters: 010-product-filters (BL-10). This spec only keeps them when going back.
- Sort order: 012-product-sorting (BL-11). This spec only keeps it when going back.
- Viewing deleted products or restoring them: 011-restore-products (BL-12).
- Admin sign-in, sessions and lockout: R002 (Admin sign-in).
- Stock movement history, date added, audit information, product images and categories: out of scope in R001.

## Open questions

- [ ] What is the target time for showing product details? R001 sets 2 s for product list pages (QE-01) and 1 s for changes (QE-02), but none for viewing one product. Options: A) 1 s, the same as QE-02; B) 2 s, the same as QE-01; C) no target, drop NFR-006 because R001 sets none. Recommended: A. A target is a new quality expectation: record it here as a spec-level decision, or add it to R001 through `/sdd-amend requirements`. (affects: NFR-006)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-16 | Initial version | — |
