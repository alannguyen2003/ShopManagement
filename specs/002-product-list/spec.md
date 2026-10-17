# Spec: Paged product list

**Feature:** 002-product-list
**Branch:** feat/002-product-list
**Source:** R001/BL-02
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-16

## Context

Once the Admin can add products (001-add-product, BL-01), they need to see every product and its stock in one place. Today the product management page has no product list. This feature gives the signed-in Admin a paged product list: 20 products per page, newest first. Each row shows the name, SKU, price, stock quantity and product status, with the total number of products and pages. The Admin moves between pages without full page reloads, and the browser Back button returns to the previous page. It is the base view that product details, publishing, stock adjustment, deletion, page size, search, filters and sorting build on.

## User stories

### US1 — See the first page of the product list (Priority: P1)

As the Admin, I want to open the product management page and see the newest products with their key product information, stock quantity and product status, so that I know at a glance what the shop has in stock.

**Why P1:** This is the entry point of the product management page. Without it, the Admin cannot see any stored product.

**Independent test:** With 45 products stored, sign in as the Admin and open the product management page. Page 1 shows the 20 most recently added products, newest first, with name, SKU, price, stock quantity and product status, and the totals show 45 products and 3 pages. With no products stored, an empty-list message and a way to add a product are shown.

**Acceptance scenarios:**
1. **Given** 45 products added one after another, **When** the Admin opens the product management page, **Then** the product list shows page 1 with the 20 most recently added products, newest first, and shows a total of 45 products and 3 pages (BR-008, BR-010, BR-021).
2. **Given** a product with name "Blue Mug", SKU "MUG-01", price 125000 VND, stock quantity 1, product status Draft and a description, **When** the product list is shown, **Then** its row shows "Blue Mug", "MUG-01", "125.000 ₫", "1" and "Draft", and does not show the description (BR-019, BR-050).
3. **Given** products with price 0 and price 1,000,000,000 VND, **When** the product list is shown, **Then** their prices are shown as "0 ₫" and "1.000.000.000 ₫" (BR-050).
4. **Given** the Admin has just added a product "Green Tea" through the add form, **When** the product list is shown, **Then** "Green Tea" is the first row of page 1 (BR-010).
5. **Given** 5 products are stored, **When** the Admin opens the product management page, **Then** all 5 are shown on page 1, the totals show 5 products and 1 page, and no other page can be reached.
6. **Given** no products are stored, **When** the Admin opens the product management page, **Then** an empty-list message is shown with a way to add a product, and choosing it opens the add form (BR-022).
7. **Given** no signed-in Admin session, **When** the product list is requested, **Then** the request is refused and no product information is shown or returned (BR-015).

### US2 — Move between pages (Priority: P1)

As the Admin, I want to move from page to page of the product list and use the browser Back button, so that I can reach every product without losing my place.

**Why P1:** With more than 20 products, any product after the first 20 can only be reached by moving between pages.

**Independent test:** With 45 products stored, move from page 1 to page 2 and then page 3. Each page shows the right products with no full page reload. Pressing Back returns to page 2, then to page 1.

**Acceptance scenarios:**
1. **Given** 45 products and the Admin is on page 1, **When** the Admin moves to page 2, **Then** the 21st to 40th most recently added products are shown, newest first, without a full page reload (QE-04).
2. **Given** 45 products and the Admin is on page 2, **When** the Admin moves to page 3, **Then** the 5 oldest products are shown and the list shows that page 3 of 3 is the current page.
3. **Given** the Admin moved from page 1 to page 2 to page 3, **When** the Admin presses the browser Back button, **Then** page 2 is shown, and pressing Back again shows page 1 (QE-04).
4. **Given** the Admin is on page 1, **When** the page navigation is shown, **Then** moving to a previous page is not offered. **Given** the Admin is on the last page, **Then** moving to a next page is not offered.
5. **Given** exactly 20 products, **When** the list is shown, **Then** the totals show 1 page. **Given** 21 products, **Then** the totals show 2 pages and page 2 shows only the oldest product.
6. **Given** 45 products and the Admin is on page 2, **When** the Admin moves to the add form and then returns to the product list with the Back button, **Then** page 2 is shown without a full page reload (QE-04).

### US3 — Page beyond the last page (Priority: P2)

As the Admin, I want a page number that no longer exists to take me to the last existing page, so that I never see a blank list when products exist.

**Why P2:** This only happens when a page number is reached directly (an old address, a bookmark, or a list that shrank). It is a safety net, not the main flow.

**Independent test:** With 45 products stored, request page 10. Page 3 is shown and is shown as the current page.

**Acceptance scenarios:**
1. **Given** 45 products (3 pages), **When** page 10 is requested, **Then** page 3 is shown with the 5 oldest products, and the list shows that page 3 of 3 is the current page (BR-020).
2. **Given** no products are stored, **When** page 4 is requested, **Then** the empty-list message with a way to add a product is shown (BR-022).
3. **Given** 45 products, **When** page 0, a negative page number or a page number that is not a whole number is requested, **Then** page 1 is shown (assumption).

## Edge cases

- **Exactly at a page boundary:** 20 products give 1 page; 21 products give 2 pages. 10,000 products give 500 pages, and the last page holds the 20 oldest.
- **Products added at the same moment:** the order stays the same every time the list is shown, so while nothing changes each product appears on exactly one page and none is skipped (FR-003).
- **A product is added in another tab while the Admin is on page 2:** the page already shown does not change by itself. The next page the Admin opens reflects the stored products at that moment, so a product may move to the next page (FR-014).
- **The list shrinks while the Admin is on a later page** (e.g. after a deletion, once 007-delete-product exists): the requested page is beyond the last, so the last existing page is shown (BR-020).
- **Very long values:** a 200-character name or a 50-character SKU is shown in full in its row (assumption).
- **Largest values:** price 1,000,000,000 is shown as "1.000.000.000 ₫". Stock quantity 1,000,000 is shown as "1.000.000" (assumption).
- **Out of stock product (stock quantity 0):** shown like any other product with stock quantity "0"; no special highlighting (assumption).
- **Invalid page number** (0, negative, not a whole number, not a number): page 1 is shown (FR-010).
- **A page size is requested before 008-list-page-size exists:** it is ignored and 20 products per page are shown (FR-002).
- **Loading a page fails (network error):** the Admin sees an error message and a way to try again. No product information is presented as current if it could not be loaded (FR-015).
- **The session expires while the Admin is on the list:** the next page request is refused and no product information is returned. The Admin is then directed to sign in as defined by R002 (Admin sign-in) (FR-012).
- **No signed-in Admin session at all:** the product list and its data are refused (FR-012, NFR-003).

## Functional requirements

- **FR-001:** The system MUST show the product list as the main view of the product management page, so that the signed-in Admin can see the stored products without outside help (BG-01).
- **FR-002:** The system MUST show 20 products per page of the product list (BR-008). In this feature the page size cannot be changed, and any page size in a request MUST be ignored.
- **FR-003:** The system MUST order the product list by date added, newest first (BR-010). Products with the same date added MUST always appear in the same order, so that while the stored products do not change, each product appears on exactly one page.
- **FR-004:** Each row of the product list MUST show the product's name, SKU, price, stock quantity and product status, and MUST NOT show the description (BR-019).
- **FR-005:** The system MUST show each price as a whole number of VND with a dot as thousands separator and the ₫ sign, e.g. "125.000 ₫" and "0 ₫" (BR-050).
- **FR-006:** The system MUST show the stock quantity as a whole number and the product status as "Draft" or "Active", using these glossary terms.
- **FR-007:** The product list MUST show the total number of products, the total number of pages and the current page number (BR-021).
- **FR-008:** The Admin MUST be able to move to the other pages of the product list [NEEDS CLARIFICATION: which page navigation should be offered? Recommended: first, previous, next and last, plus "Page X of Y". A typed page number is not included.]. Moving to a page before page 1 or after the last page MUST NOT be offered.
- **FR-009:** When the requested page number is beyond the last page, the system MUST show the last existing page and show it as the current page (BR-020).
- **FR-010:** When the requested page number is below 1, not a whole number or not a number, the system MUST show page 1.
- **FR-011:** When no products exist, the system MUST show an empty-list message and a way to add a product that opens the add form (BR-022). In that case it MUST NOT show page navigation.
- **FR-012:** Only the signed-in Admin MUST be able to view the product list. A request without a signed-in Admin session MUST be refused, and no product information MUST be returned or shown (BR-015).
- **FR-013:** Moving between pages of the product list, and between the product list and the add form, MUST NOT cause a full page reload. The browser Back button MUST return to the previously shown view, including the product list page number (QE-04).
- **FR-014:** Each page of the product list MUST reflect the products stored at the moment that page is requested.
- **FR-015:** When a page of the product list cannot be loaded, the system MUST tell the Admin and offer a way to try again. It MUST NOT present product information that could not be loaded as current.

## Non-functional requirements

NFR numbers follow the QE numbers of R001. QE-02 does not apply to this feature, so NFR-002 is not used.

- **NFR-001:** (QE-01) 95% of product list pages are shown within 2 seconds of the Admin's action (opening the product management page or moving to another page), measured in the browser, with 20 products per page and 10,000 products stored. Search, filters and sort order are measured in 009-product-search, 010-product-filters and 012-product-sorting.
- **NFR-003:** (QE-03) 100% of product list requests without a signed-in Admin session are refused, and 0 of them return any product information.
- **NFR-004:** (QE-04) Moving between product list pages, and between the product list and the add form, causes 0 full page reloads. In 100% of cases the browser Back button returns to the previously shown view with the same page number.

## Key entities (if any)

- **Product:** something the shop sells and keeps stock of. In this feature, the list shows its product information (name, SKU, price; the description is not shown), its stock quantity and its product status (Draft or Active). The date it was added sets its place in the list.
- **Product list:** the paged list of all products on the product management page. It is ordered newest first and shows the total number of products and pages.
- **Page size:** the number of products on one page of the product list; always 20 in this feature.

## Success criteria

- **SC-001:** (BG-01) With 45 products stored, the Admin, without outside help, finds the stock quantity and product status of any named product in under 1 minute from opening the product management page.
- **SC-002:** With 45 products stored, every product appears on exactly one of the 3 pages: 0 duplicates and 0 missing products.
- **SC-003:** In 100% of checks, the totals shown match the number of stored products and the number of pages that follow from 20 per page.
- **SC-004:** With 10,000 products stored, 95% of product list pages are shown within 2 seconds (NFR-001).

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BG-01 | FR-001, SC-001 |
| BR-008 | FR-002; US1 scenario 1 |
| BR-010 | FR-003; US1 scenarios 1, 4; US2 scenario 1 |
| BR-015 | FR-012; US1 scenario 7; edge cases (session expires, no session) |
| BR-019 | FR-004; US1 scenario 2 |
| BR-050 | FR-005; US1 scenarios 2, 3; edge case (largest values) |
| BR-020 | FR-009; US3 scenario 1; edge case (list shrinks) |
| BR-021 | FR-007; US1 scenarios 1, 5; US2 scenario 5; SC-003 |
| BR-022 | FR-011; US1 scenario 6; US3 scenario 2 |
| QE-01 | NFR-001; SC-004 |
| QE-03 | NFR-003; FR-012 |
| QE-04 | NFR-004; FR-013; US2 scenarios 1, 3, 6 |

## Assumptions

- Admin sign-in and session handling come from R002 (Admin sign-in), which is built before this feature. What the Admin sees after a refusal (e.g. being directed to sign in) is defined there.
- The product list is the view the Admin sees first on the product management page.
- An invalid page number (below 1, not a whole number, not a number) shows page 1. This is consistent with BR-009, where an invalid page size falls back to the default rather than giving an error.
- The page number is part of the view, so reloading the view or reopening it from the browser history shows the same page.
- The list does not refresh by itself. It shows what was stored when the page was requested.
- The stock quantity is shown with the same thousands separator as the price (e.g. 1.000.000), for consistency with BR-050.
- Long names and SKUs are shown in full in their row. Out-of-stock products get no special highlighting.
- The date added is used for ordering but is not shown in the row (BR-019 lists the row fields).
- When no products exist, the empty-list message replaces the rows, totals and page navigation.

## Out of scope

- Changing the page size (10, 20, 50, 100): 008-list-page-size (BL-07).
- Searching by name or SKU: 009-product-search (BL-09).
- Filtering by product status or stock: 010-product-filters (BL-10).
- Choosing a sort order: 012-product-sorting (BL-11). This feature only applies the default order of BR-010.
- Selecting a product to open its product details, and returning to the same page, search text, filters and sort order (BR-023): 003-product-details (BL-03).
- The add form itself: 001-add-product (BL-01). This feature only offers the way to it.
- Updating product information: 004-update-product (BL-04).
- Publishing a product: 005-publish-product (BL-05).
- Adjusting stock: 006-adjust-stock (BL-08).
- Deleting a product, including the delete action on the product list (BR-024) and leaving deleted products out of the list and totals (BR-007): 007-delete-product (BL-06).
- The deleted products list and restoring: 011-restore-products (BL-12).
- Admin sign-in, session timeout and lockout: R002 (Admin sign-in).
- Highlighting low or zero stock, export, bulk actions: out of scope in R001.

## Open questions

- [ ] Which page navigation should the product list offer? Recommended: first, previous, next and last, plus "Page X of Y". A typed page number is not included. (affects: FR-008, US2)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-16 | Initial version | — |
