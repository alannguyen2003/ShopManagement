# Spec: Filter products by status and stock

**Feature:** 010-product-filters
**Branch:** feat/010-product-filters
**Source:** R001/BL-10
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-16

## Context

The product list (002/BL-02) shows every product, newest first, page by page. As the shop grows, the Admin has to page through the whole list to see which products are still Draft and need publishing, or which are out of stock and may need restocking or deleting. Out-of-stock and Draft products are also the only ones the Admin can delete (BR-004), so the Admin needs to find them quickly. This feature adds two filters to the product list, one on product status and one on stock. Each filter narrows the list to the matching products, still paged and with totals for the matching products only.

## User stories

### US1 — Filter by product status (Priority: P1)

As the Admin, I want to narrow the product list to Draft products or to Active products so that I can see at a glance which products still need publishing.

**Why P1:** This half of the feature's value covers Draft products that are waiting to be published or can be deleted.

**Independent test:** Store 5 Draft and 8 Active products. On the product list, choose each status filter value and check that the rows, the total number of products and the number of pages match.

**Acceptance scenarios:**
1. **Given** 5 Draft and 8 Active products exist, **When** the Admin sets the status filter to Draft, **Then** the product list shows exactly the 5 Draft products, newest first, and the totals show 5 products and 1 page.
2. **Given** 5 Draft and 8 Active products exist, **When** the Admin sets the status filter to Active, **Then** the product list shows exactly the 8 Active products and the totals show 8 products.
3. **Given** the status filter is Draft, **When** the Admin sets it back to all, **Then** the product list shows all 13 products and the totals show 13 products.
4. **Given** 30 Draft products exist and the Admin is on page 2 of the unfiltered list, **When** the Admin sets the status filter to Draft, **Then** page 1 of the matching products is shown (20 products) and the totals show 30 products and 2 pages.
5. **Given** only Active products exist, **When** the Admin sets the status filter to Draft, **Then** the product list shows a "no matching products" message and a way to clear the filters.
6. **Given** no signed-in Admin session, **When** a filtered product list is requested, **Then** the request is refused and no product data is returned.

### US2 — Filter by stock (Priority: P1)

As the Admin, I want to narrow the product list to products that are in stock or to products that are out of stock so that I can find what needs restocking or can be deleted.

**Why P1:** This half of the feature's value covers out-of-stock products, which need restocking or can be deleted.

**Independent test:** Store products with stock quantities 0, 0, 1 and 40. On the product list, choose each stock filter value and check the rows and totals.

**Acceptance scenarios:**
1. **Given** products with stock quantities 0, 0, 1 and 40 exist, **When** the Admin sets the stock filter to out of stock, **Then** only the 2 products with stock quantity 0 are shown and the totals show 2 products.
2. **Given** the same products, **When** the Admin sets the stock filter to in stock, **Then** only the products with stock quantity 1 and 40 are shown and the totals show 2 products.
3. **Given** the stock filter is out of stock, **When** the Admin sets it back to all, **Then** all 4 products are shown.
4. **Given** every product has stock quantity 1 or more, **When** the Admin sets the stock filter to out of stock, **Then** the product list shows a "no matching products" message and a way to clear the filters.

### US3 — Combine filters with each other, with search text and with sort order (Priority: P2)

As the Admin, I want to use both filters together, along with search text and sort order when those exist, so that I can find a specific group of products, such as Active products that are out of stock.

**Why P2:** Each filter is useful alone. Combining them refines the result but is not needed for a first usable version.

**Independent test:** Store products that cover every combination of Draft/Active and stock 0/1 or more. Set both filters and check that only products matching both are listed and counted.

**Acceptance scenarios:**
1. **Given** these products exist: Draft with stock 0, Draft with stock 5, Active with stock 0 (two products) and Active with stock 7, **When** the Admin sets the status filter to Active and the stock filter to out of stock, **Then** only the 2 Active products with stock quantity 0 are shown and the totals show 2 products.
2. **Given** search text exists (009/BL-09) and products "Blue Mug" (Draft) and "Blue Plate" (Active) exist, **When** the Admin searches "blue" and sets the status filter to Active, **Then** only "Blue Plate" is shown and the totals count only it.
3. **Given** sort order exists (012/BL-11), **When** the Admin sets the stock filter to in stock and sorts by stock quantity ascending, **Then** only in-stock products are shown, in ascending stock quantity.
4. **Given** search text "zzz" and the status filter Draft match no product while products exist, **When** the Admin uses the way to clear offered with the "no matching products" message, **Then** the search text is emptied, both filters return to all, and page 1 of all products is shown.
5. **Given** the Admin is on page 3 of in-stock products, **When** the Admin also sets the status filter to Draft, **Then** page 1 of the products matching both filters is shown.

### US4 — Keep filters while navigating (Priority: P2)

As the Admin, I want the filters I set to still apply when I come back from product details or use the browser Back button so that I don't have to set them again after every product I check.

**Why P2:** Without this story filtering still works, but repeated product checks become slow.

**Independent test:** Set filters and go to page 2. Open a product, return to the list and check that the filters and page are restored without a full page reload. Then use the browser Back button and check again.

**Acceptance scenarios:**
1. **Given** the status filter is Draft, the stock filter is out of stock and the Admin is on page 2, **When** the Admin opens a product's details and returns to the product list, **Then** page 2 of the same filtered list is shown with both filters still set.
2. **Given** the same state, **When** the Admin opens a product's details and presses the browser Back button, **Then** the product list returns with the same filters and page, without a full page reload.
3. **Given** the stock filter is out of stock, **When** the Admin opens one of the listed products, adds stock to it (006/BL-08) and returns to the list, **Then** that product no longer appears and the totals are one lower.

## Edge cases

- **No products at all:** whatever the filters are, the list shows the empty-list message with a way to add a product (BR-022), not the "no matching products" message.
- **Products exist, none match:** the list shows a "no matching products" message and a way to clear the search text and filters (BR-043).
- **Unknown filter value** (for example status "Archived", stock "low", or a garbled value): rejected at the boundary with a message that names the invalid filter, and no product data is returned (QE-05). What the Admin sees when such a value reaches the list some other way, for example from an edited page address, is open: see FR-014.
- **A product changes while a filtered list is on screen** (published, stock adjusted or deleted in another tab): the next time the list loads it reflects the stored data, so the product appears in or disappears from the filtered list. No error is shown.
- **Requested page is beyond the last page of matching products** (for example a product left the filter while the Admin was on the last page): the last existing page of matching products is shown (BR-020). If nothing matches any more, the "no matching products" message is shown.
- **Deleted products:** never appear in any filtered list or in its totals, even when they match the filters (BR-007, 007/BL-06). For example, a deleted Draft product with stock 0 does not appear under Draft or out of stock.
- **Filters changed several times quickly:** the list ends up showing the result for the last filter values chosen. An earlier, slower response never replaces it.
- **Connection lost or session expired while changing a filter:** the Admin sees an error message and a way to retry. No partial or wrong list is shown as if it matched the chosen filters. Signing in again is handled by R002.
- **Large volume:** with 10,000 products stored and all of them matching (both filters on all), paging and totals still work and meet NFR-001.

## Functional requirements

- **FR-001:** The product list MUST offer a status filter with exactly three values: all, Draft and Active. The default is all. (BR-038)
- **FR-002:** With the status filter at Draft, the product list MUST show only products whose product status is Draft. At Active it MUST show only Active products. At all, product status MUST NOT restrict the list. (BR-038)
- **FR-003:** The product list MUST offer a stock filter with exactly three values: all, in stock and out of stock. The default is all. (BR-039)
- **FR-004:** With the stock filter at in stock, the product list MUST show only products with a stock quantity of 1 or more. At out of stock it MUST show only products with a stock quantity of 0. At all, stock quantity MUST NOT restrict the list. (BR-039)
- **FR-005:** When both filters are set, a product MUST appear only if it matches both. (BR-041)
- **FR-006:** When search text (009/BL-09) or a sort order (012/BL-11) is in use, the filters MUST combine with them. The list then shows only products that match the search text and every filter, in the chosen sort order. Without a chosen sort order, the matching products are ordered by date added, newest first (BR-010). (BR-041)
- **FR-007:** Paging, the page size and the totals (number of products and number of pages) MUST apply only to the products that match the current filters, plus the search text if one is entered. (BR-041, BR-021)
- **FR-008:** Changing the value of either filter MUST return the product list to page 1. (BR-042)
- **FR-009:** When the requested page is beyond the last page of matching products, the product list MUST show the last existing page of matching products. (BR-020)
- **FR-010:** When products exist but none match the current filters and search text, the product list MUST show a "no matching products" message and a way to clear the search text and filters. (BR-043)
- **FR-011:** Clearing MUST set both filters to all, empty the search text when search exists (009/BL-09) and show page 1. (BR-043, BR-042)
- **FR-012:** When no products exist at all, the product list MUST show the empty-list message of BR-022 whatever the filter values.
- **FR-013:** The product list MUST show which value of each filter is currently applied.
- **FR-014:** Filter values MUST be validated at the boundary. A status or stock filter value outside the allowed set MUST be rejected with a message that names the invalid filter, and no product data is returned (QE-05). A filter that is not given at all means all. [NEEDS CLARIFICATION: When an invalid filter value reaches the product list from the page address (an edited address, an old bookmark or a Back step), should the Admin see an error message with a way to clear the filters, or should the list silently fall back to "all", the way an invalid page size falls back to 20 (BR-009)?]
- **FR-015:** Filters MUST be evaluated against the stored products at the moment the list is requested, so publishes (005/BL-05), stock adjustments (006/BL-08) and deletions (007/BL-06) made since are reflected.
- **FR-016:** A deleted product MUST NOT appear in any filtered product list or be counted in its totals. (BR-007, delivered by 007/BL-06)
- **FR-017:** Only the signed-in Admin MUST be able to request a filtered product list. A request without a signed-in Admin session MUST be refused and return no product data. (BR-015)
- **FR-018:** Changing a filter, clearing the filters and returning from product details MUST update the product list without a full page reload. (QE-04)
- **FR-019:** Returning from product details MUST show the product list with the same filters, page, search text and sort order the Admin came from. The browser Back button MUST return to the previous view, with its filters. (BR-023, QE-04) [NEEDS CLARIFICATION: Should each filter change be its own step in browser history, so that Back undoes the last filter change, or should Back only move between views (product list ↔ product details)? This should be the same for page number, page size, search text and sort order in 002, 008, 009 and 012.]

## Non-functional requirements

- **NFR-001 (QE-01):** 95% of product list pages are shown within 2 seconds of the Admin's action, measured in the browser. This holds for 20 products per page with 10,000 products stored, for any combination of status filter and stock filter, with or without search text and sort order.
- **NFR-002 (QE-03):** 100% of filtered product list requests are refused without a signed-in Admin session, and 0 of them return product data.
- **NFR-003 (QE-04):** Setting a filter, clearing the filters and moving between the filtered product list and product details cause 0 full page reloads. In 100% of tested cases, returning to the list restores its filters, page, search text and sort order.
- **NFR-004 (QE-05):** 100% of filter values outside the allowed sets are rejected at the boundary with a message naming the invalid filter, and 0 such requests return product data.

## Key entities

- **Product:** something the shop sells and keeps stock of. This feature uses only its product status (Draft or Active) and its stock quantity (whole number, 0 or more) to decide whether it matches a filter. Deleted products are never matched.
- **Filter:** a choice that narrows the product list. There are two: the status filter (all, Draft, Active) and the stock filter (all, in stock, out of stock). Both apply to the product list together with search text, sort order, page and page size.
- **Product list:** the paged list of products on the product management page. When filters are set, it holds only the matching products, and its totals count only those.

## Success criteria

- **SC-001:** From the product management page, the Admin can show all out-of-stock products, or all Draft products, with at most 2 actions and in under 10 seconds.
- **SC-002:** For each of the 9 combinations of status filter and stock filter, the listed products and the totals exactly match the stored, non-deleted products that meet both filters (0 discrepancies in acceptance tests).
- **SC-003:** After opening product details and returning, the filters, page, search text and sort order are as the Admin left them in 100% of acceptance tests.

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BR-015 | FR-017; US1 scenario 6; NFR-002 |
| BR-038 | FR-001, FR-002; US1 scenarios 1–5 |
| BR-039 | FR-003, FR-004; US2 scenarios 1–4 |
| BR-041 | FR-005, FR-006, FR-007; US3 scenarios 1–3 |
| BR-042 | FR-008, FR-011; US1 scenario 4; US3 scenario 5 |
| BR-043 | FR-010, FR-011; US1 scenario 5; US2 scenario 4; US3 scenario 4; Edge cases |
| QE-01 | NFR-001 |
| QE-03 | NFR-002; FR-017 |
| QE-04 | NFR-003; FR-018, FR-019; US4 scenarios 1–2 |
| QE-05 | NFR-004; FR-014; Edge cases (unknown filter value) |

## Assumptions

- Admin sign-in, account provisioning, lockout and session timeout are delivered by the separate requirements document R002 (Admin sign-in), which is not yet written. This spec assumes a signed-in Admin session and does not specify sign-in.
- When the Admin newly opens the product management page, both filters are at all. Filters are not remembered between sessions, in the same way as page size (BL-07).
- A filter that is not given in a request means all. A value that is given but is not in the allowed set, including an empty value, is invalid (FR-014).
- Changing a filter keeps the chosen page size (008/BL-07). Only the page number returns to 1.
- The filter values are labelled with the glossary terms: All, Draft, Active, In stock, Out of stock.
- Filters apply only to the product list, not to the deleted products list (011/BL-12).
- Before 005/BL-05 and 006/BL-08 are built, every product is Draft with stock quantity 1. Tests for this feature therefore set up stored products with mixed product status and stock quantity directly.

## Out of scope

- The paged product list itself: rows, default page size, default order, empty-list message and totals (002/BL-02).
- Product details and the way back to the list (003/BL-03). This spec only requires that the filters are kept.
- Publishing a product (005/BL-05), adjusting stock (006/BL-08) and deleting a product (007/BL-06). This spec only reflects their results.
- Choosing the page size (008/BL-07).
- Search text and its matching rules (009/BL-09). This spec only requires that the filters combine with it and that clearing empties it.
- The deleted products list and restoring (011/BL-12). It has no filters.
- Choosing a sort order (012/BL-11). This spec only requires that the filters combine with it.
- Filtering by price range, date added, low-stock threshold or any other field. Showing counts per filter value. Remembering filters between sessions.
- Admin sign-in (R002).

## Open questions

- [ ] When an invalid filter value reaches the product list from the page address, should the Admin see an error with a way to clear the filters, or should the list silently fall back to "all" as page size does (BR-009)? Options: A) show an error message and a way to clear the filters; B) silently use "all". Recommended: A, because BL-10 says unknown values are "rejected" and silent fallback would hide a wrong filter. (affects: FR-014, NFR-004)
- [ ] Should each filter change be its own step in browser history, so Back undoes it, or should Back only move between views? Options: A) each filter change is a history step; B) only view changes are history steps, and filter changes replace the current step. Recommended: decide once for page number, page size, search text, filters and sort order (002, 008, 009, 010, 012). (affects: FR-019, NFR-003, SC-003)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-16 | Initial version | — |
