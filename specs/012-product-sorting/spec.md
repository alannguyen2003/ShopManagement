# Spec: Sort the product list

**Feature:** 012-product-sorting
**Branch:** feat/012-product-sorting
**Source:** R001/BL-11
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-16

## Context

The product list (002-product-list, BL-02) always shows products newest first (BR-010). To check which products are running low, or to find a product by name or SKU, the Admin currently has to page through the whole list. This feature lets the Admin choose a sort order for the product list: one column (name, SKU, price, stock quantity or date added) and a direction (ascending or descending). The order applies to all products, not only the current page. It works together with search text and filters, and it is kept when the Admin goes to product details and back. Every request needs a signed-in Admin session. Sign-in is delivered by R002 (Admin sign-in) and is not specified here.

## User stories

### US1 — Sort the product list by a column (Priority: P1)

As the Admin, I want to order the product list by name, SKU, price, stock quantity or date added, ascending or descending, so that the products I care about right now (e.g. lowest stock first) are on the first page.

**Why P1:** This is the whole value of the backlog item. Without it, nothing else in this spec can be used.

**Independent test:** Store products with known names, SKUs, prices, stock quantities and dates added, across more than one page. Choose each column and direction in turn and check the order across all pages, the tie rule and the return to page 1. Only the product list from 002-product-list is needed.

**Acceptance scenarios:**
1. **Given** no sort order has been chosen, **When** the Admin opens the product list, **Then** products are ordered by date added, newest first, and the list shows that this is the sort order in effect (BR-010, BR-040).
2. **Given** 25 products with stock quantities from 0 to 24, **When** the Admin sorts by stock quantity ascending, **Then** page 1 shows the 20 products with stock quantities 0 to 19 in that order, and page 2 shows stock quantities 20 to 24 (BR-040, BR-041).
3. **Given** products named "apple tea", "Blue Mug" and "Zebra Cup", **When** the Admin sorts by name descending, **Then** the order is "Zebra Cup", "Blue Mug", "apple tea" (letter case does not affect the order).
4. **Given** two products named "Blue Mug", one added on 2026-10-01 and one added on 2026-10-10, **When** the Admin sorts by name ascending or descending, **Then** the one added on 2026-10-10 comes first in both cases (BR-040).
5. **Given** products priced 9.000 ₫, 125.000 ₫ and 1.000.000 ₫, **When** the Admin sorts by price ascending, **Then** they appear as 9.000 ₫, 125.000 ₫, 1.000.000 ₫ (ordered by value, not as text).
6. **Given** products with SKUs "MUG-10", "mug-02" and "PL-01", **When** the Admin sorts by SKU ascending, **Then** the order is "mug-02", "MUG-10", "PL-01".
7. **Given** the list is sorted by date added, **When** the Admin chooses ascending, **Then** the oldest product is first.
8. **Given** the Admin is on page 3 of the list sorted by price ascending, **When** the Admin changes to name ascending, or to price descending, **Then** page 1 of the new order is shown (BR-042).
9. **Given** a list request with an unknown sort column (e.g. "colour") or an unknown direction (e.g. "sideways"), **When** it reaches the system, **Then** it is rejected with a message that names the invalid value and lists the allowed values, and no products are returned in an unknown order (QE-05).
10. **Given** no signed-in Admin session, **When** a sorted product list is requested, **Then** the request is refused and no product data is returned (BR-015, QE-03).

### US2 — Keep the sort order while navigating (Priority: P2)

As the Admin, I want the product list to keep my chosen sort order when I open a product and come back, or use the browser Back button, so that I do not have to choose it again after every check.

**Why P2:** US1 already works without this, but having to choose the order again after every product details view would make sorting tiresome to use.

**Independent test:** With 002-product-list and 003-product-details in place, choose a sort order and a page, open product details, return, then use the browser Back button. Check that the sort order and page are restored and that no full page reload happens.

**Acceptance scenarios:**
1. **Given** the list is sorted by stock quantity ascending and the Admin is on page 2, **When** the Admin opens a product's details and returns to the list, **Then** the list shows page 2 sorted by stock quantity ascending (QE-04).
2. **Given** the Admin sorted by name ascending and then changed to price descending, **When** the Admin presses the browser Back button, **Then** the list is again sorted by name ascending at the page shown before the change, without a full page reload (QE-04).
3. **Given** a sort order is in effect, **When** the Admin changes it, **Then** the product list updates without a full page reload (QE-04).

### US3 — Sort together with the other product list choices (Priority: P3)

As the Admin, I want the sort order to work together with the page size, search text and filters, so that I can, for example, see out-of-stock Active products by name.

**Why P3:** It only adds value once 008-list-page-size (BL-07), 009-product-search (BL-09) or 010-product-filters (BL-10) exist. Each scenario below applies as soon as the related feature exists.

**Independent test:** With at least one of 008, 009 or 010 in place, combine it with a sort order. Check that only the matching products are sorted, that the totals are unchanged by sorting, and that changing the sort order keeps the other choices.

**Acceptance scenarios:**
1. **Given** the search text "mug" matches 30 of 100 products, **When** the Admin sorts by price ascending, **Then** only those 30 products are shown, ordered by price, and the totals show 30 products and 2 pages (BR-041).
2. **Given** the filters Active and out of stock are set, **When** the Admin sorts by name ascending, **Then** only Active products with stock quantity 0 are shown, ordered by name (BR-041).
3. **Given** the page size is 50, the search text is "tea" and the Admin is on page 2, **When** the Admin changes the sort order, **Then** page 1 is shown with page size 50 and search text "tea" unchanged (BR-042).
4. **Given** a sort order is in effect, **When** the Admin changes the search text or a filter, **Then** the sort order is kept and page 1 is shown (BR-042).

## Edge cases

- No products exist → the empty-list message of 002-product-list is shown. Choosing a sort order changes nothing else.
- Products exist but none match the search text and filters → the "no matching products" message is shown (BR-043, owned by 009/010). The sort order stays in effect.
- Exactly one product, or all products with the same value in the sort column → the order falls back to date added, newest first (BR-040).
- Products added at the same moment with equal values in the sort column → the order is still fixed. The same request always gives the same order, and each product appears on exactly one page (FR-005).
- A product is changed (e.g. its price is updated or its stock is adjusted) in another tab while the Admin is on page 2 of a sorted list → the next page the Admin requests reflects the stored data at that moment. The list does not refresh by itself, so a product may move between pages.
- A product is deleted in another tab → it no longer appears in any sorted page or in the totals (BR-007, owned by 007-delete-product).
- The current page no longer exists (e.g. after a deletion) → the last existing page is shown (BR-020, owned by 002-product-list).
- Names with different letter case ("apple" vs "Apple") → the order ignores letter case (FR-004).
- Names containing numbers ("Mug 10", "Mug 2") → compared character by character, so "Mug 10" comes before "Mug 2" (assumption).
- Names with Vietnamese accented letters → [NEEDS CLARIFICATION: see FR-004].
- A request with a sort column but no direction → ascending is used. A request with a direction but no sort column → rejected as invalid (FR-008).
- A request with an unknown sort column or direction, or with a column that is not sortable (product status, description) → rejected at the boundary with a message (FR-008).
- 10,000 products sorted by any column → the page is still shown within the NFR-001 time.
- The session expires before a sort order change → the request is refused (NFR-003). Signing in again is handled by R002.
- Lost connection while changing the sort order → the Admin is told the list could not be loaded. The previously shown list and sort order stay on screen.

## Functional requirements

- **FR-001:** The Admin MUST be able to choose exactly one sort column for the product list from this set: name, SKU, price, stock quantity, date added (BR-040). [NEEDS CLARIFICATION: date added is not a column of the product list rows (BR-019). Should the Admin choose "date added" (and the other columns) from a separate sort choice, with no new column, or should a date-added column be added to the rows, which changes BR-019 and needs `/sdd-amend requirements R001`?]
- **FR-002:** The Admin MUST be able to choose the direction, ascending or descending, for the chosen sort column (BR-040).
- **FR-003:** When no sort order has been chosen, the system MUST order the product list by date added, newest first (BR-010, BR-040).
- **FR-004:** The system MUST order each column as follows (BR-040):
  - Price and stock quantity by numeric value.
  - Date added by the moment the product was added.
  - Name and SKU alphabetically, ignoring letter case and leading or trailing spaces. [NEEDS CLARIFICATION: how should Vietnamese accented letters (e.g. "Đ", "Ă", "Ê") be ordered in names: in Vietnamese alphabet order (a, ă, â, b, c, d, đ, e, ê…), or treated as their base letter (đ as d, ă as a)?]
- **FR-005:** When two products have equal values in the sort column, the system MUST order them by date added, newest first (BR-040). The overall order MUST be fixed: the same request always gives the same order, and each matching product appears on exactly one page.
- **FR-006:** The system MUST apply the sort order to all products that match the current search text and filters before splitting them into pages, not only to the products on the current page (BR-040, BR-041).
- **FR-007:** The product list MUST show which sort column and direction are in effect, including the default of FR-003.
- **FR-008:** The system MUST validate the requested sort column and direction at the boundary (QE-05):
  - It MUST reject an unknown column, an unknown direction, a direction without a column, or a column outside FR-001 (e.g. product status or description). The rejection includes a message that names the invalid value and lists the allowed values, and no product list in an unknown order is returned.
  - A column given without a direction MUST be sorted ascending.
- **FR-009:** Changing the sort column or the direction MUST show page 1 of the product list (BR-042). Choosing the sort order that is already in effect MUST leave the list and the page unchanged.
- **FR-010:** Changing the sort order MUST keep the current page size, search text and filters (BR-041). Changing the search text or a filter MUST keep the current sort order (BR-041, BR-042).
- **FR-011:** Sorting MUST NOT change which products match or the totals of products and pages. Paging and totals apply to the matching products only (BR-041).
- **FR-012:** Returning from product details to the product list MUST restore the sort order the Admin came from, together with the page, search text and filters (QE-04; extends the return behavior of BR-023 in 003-product-details).
- **FR-013:** The browser Back button MUST return to the previous product list view, including its sort order and page number. Changing the sort order MUST NOT cause a full page reload (QE-04).
- **FR-014:** The system MUST serve a sorted product list only to the signed-in Admin. Requests without a signed-in Admin session MUST be refused without returning product data (BR-015).

## Non-functional requirements

- **NFR-001:** With 10,000 products stored, 95% of product list pages (20 products) are shown within 2 seconds of the Admin's action, measured in the browser. This holds for every sort column and direction, with or without search text and filters (QE-01).
- **NFR-003:** 100% of product list requests, sorted or not, are refused without a signed-in Admin session (QE-03).
- **NFR-004:** Changing the sort order, going to product details and back, and using the browser Back button cause 0 full page reloads. Back restores the previous sort order and page number in 100% of cases (QE-04).
- **NFR-005:** 100% of requests with an invalid sort column or direction (as defined in FR-008) are rejected at the boundary with a message naming the invalid value. 0 such requests produce a product list (QE-05).

(NFR-002 is not used: NFR numbers match the QE numbers of R001, and QE-02 is not covered by this item.)

## Key entities (if any)

- **Product:** something the shop sells and keeps stock of. The attributes used by this feature are name, SKU, price, stock quantity and date added. Product status and description are shown elsewhere but are not sort columns.
- **Product list:** the paged list of all (non-deleted) products shown on the product management page. Its view is defined by page number, page size, search text, filters and sort order.
- **Sort order:** the column the product list is ordered by (name, SKU, price, stock quantity or date added) and the direction (ascending or descending). The default is date added, newest first.

## Success criteria

- **SC-001:** From the product list, the Admin can bring the products with the lowest stock quantity to the top of page 1 in at most 2 actions and under 10 seconds.
- **SC-002:** In an acceptance test with 10,000 products, every page of every sort column and direction follows FR-004 and FR-005. Across all pages, each product appears exactly once.
- **SC-003:** In acceptance tests, 100% of navigations from product details back to the list, and of browser Back presses, restore the sort order and page the Admin came from.

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BR-010 | FR-003; US1 scenario 1 |
| BR-015 | FR-014; US1 scenario 10; NFR-003 |
| BR-040 | FR-001, FR-002, FR-004, FR-005, FR-006; US1 scenarios 1–7 |
| BR-041 | FR-006, FR-010, FR-011; US3 scenarios 1–3 |
| BR-042 | FR-009, FR-010; US1 scenario 8; US3 scenarios 3–4 |
| QE-01 | NFR-001 |
| QE-03 | NFR-003; FR-014; US1 scenario 10 |
| QE-04 | NFR-004; FR-012, FR-013; US2 scenarios 1–3 |
| QE-05 | NFR-005; FR-008; US1 scenario 9 |

## Assumptions

- Admin sign-in exists before this feature is built. It is delivered by R002 (Admin sign-in), which also decides the session timeout and what happens after the session expires.
- When the Admin chooses a column that is not currently sorted, it is sorted ascending first. Choosing the same column again switches the direction.
- Name and SKU ordering ignores letter case and leading or trailing spaces, consistent with SKU uniqueness (BR-030) and search (BR-036).
- Numbers inside names and SKUs are compared character by character, not by value ("Mug 10" before "Mug 2").
- A sort column given without a direction is sorted ascending. A direction without a column is invalid (FR-008).
- Each page is computed from the data stored when it is requested. An open list does not refresh by itself when another tab changes a product.
- The sort order is not remembered between sessions (BL-11 "Not included").

## Out of scope

- The paged product list itself: row content, totals, the empty-list message and showing the last page when the page is beyond the last (002-product-list, BL-02).
- Returning from product details to the list at the same page, search text and filters (003-product-details, BL-03). This spec only adds the sort order to what is restored.
- Choosing the page size (008-list-page-size, BL-07).
- Search by name or SKU and the "no matching products" message (009-product-search, BL-09).
- Filters by product status and stock (010-product-filters, BL-10).
- Hiding deleted products from the list (007-delete-product, BL-06), and the ordering of the deleted products list (011-restore-products, BL-12).
- Sorting by product status or description. Sorting by more than one chosen column. Remembering the sort order between sessions.
- Admin sign-in, account setup, lockout and session timeout (R002).

## Open questions

- [ ] Date added is not a column of the product list rows (BR-019). Should the Admin choose "date added" (and the other columns) from a separate sort choice, with no new column, or should a date-added column be added to the rows, which changes BR-019 and needs `/sdd-amend requirements R001`? (affects: FR-001, FR-007, US1 scenario 7)
- [ ] How should Vietnamese accented letters be ordered when sorting by name: in Vietnamese alphabet order (a, ă, â, b, c, d, đ, e, ê…), or treated as their base letter (đ as d, ă as a)? (affects: FR-004, SC-002)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-16 | Initial version | — |
