# Spec: Choose product list page size

**Feature:** 008-list-page-size
**Branch:** feat/008-list-page-size
**Source:** R001/BL-07
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-16

## Context

The product list on the product management page shows 20 products per page (BR-008, delivered by 002-product-list / BL-02). With up to 10,000 products, the Admin sometimes wants to scan more products at once, and sometimes wants a shorter page. This feature lets the signed-in Admin choose a page size of 10, 20, 50 or 100 (BR-009). After a change, the list goes back to page 1 (BR-027). The chosen page size stays in place while the Admin moves around the product management page. Requests for any other page size quietly fall back to 20.

## User stories

### US1 — Choose a page size (Priority: P1)

As the Admin, I want to choose how many products the product list shows per page (10, 20, 50 or 100) so that I can see more or fewer products at once than the default 20.

**Why P1:** This is the whole value of the backlog item. Without it the list stays fixed at 20.

**Independent test:** With 120 products stored and the Admin signed in, open the product list, choose 50, and check that page 1 shows 50 products and the totals show 120 products and 3 pages. No other feature of this spec is needed.

**Acceptance scenarios:**
1. **Given** 120 products exist and the product list shows page 1 with page size 20, **When** the Admin chooses page size 50, **Then** page 1 shows the 50 newest products, the totals show 120 products and 3 pages, and the page size choice shows 50.
2. **Given** 120 products exist, **When** the Admin opens the product list without choosing a page size, **Then** the page size choice shows 20 and the totals show 6 pages (BR-008, BR-009).
3. **Given** 45 products exist and the Admin is on page 3 with page size 20, **When** the Admin chooses page size 10, **Then** page 1 is shown with 10 products and the totals show 45 products and 5 pages (BR-027).
4. **Given** 120 products exist and the Admin chose page size 50, **When** the Admin moves to page 3, **Then** page 3 shows the last 20 products and the page size is still 50.
5. **Given** 30 products exist, **When** the Admin chooses page size 100, **Then** a single page shows all 30 products and the totals show 1 page.
6. **Given** 120 products exist, **When** a product list page is requested with page size 7, 1000, 0, -20, 20.5, "abc" or no value, **Then** the list is shown with page size 20, no error message is shown, and the page size choice shows 20 (BR-009).
7. **Given** no signed-in Admin session, **When** a product list page is requested with any page size, **Then** the request is refused and no product data is returned (BR-015).

### US2 — Keep the page size while navigating (Priority: P2)

As the Admin, I want the page size I chose to stay in place when I page through the list, open a product and come back, or use the browser Back button, so that I do not have to choose it again every time.

**Why P2:** US1 already gives a usable page size choice. Keeping it during navigation saves repeated actions but is not needed to prove the choice works.

**Independent test:** With 120 products, choose page size 50, go to page 2, open product details, return to the list. Check that page 2 with page size 50 is shown. Then use the browser Back button and check that the previous page size and page are restored, with no full page reload at any step.

**Acceptance scenarios:**
1. **Given** the Admin chose page size 50 and is on page 2, **When** the Admin opens the product details of a product and returns to the product list, **Then** page 2 with page size 50 is shown (BR-023 extended to page size).
2. **Given** the Admin was on page 2 with page size 20 and then chose page size 100, **When** the Admin uses the browser Back button, **Then** the product list shows page 2 with page size 20, with no full page reload (QE-04).
3. **Given** the Admin used the browser Back button as in scenario 2, **When** the Admin uses the browser Forward button, **Then** page 1 with page size 100 is shown.
4. **Given** the Admin chose page size 50 and search (009-product-search / BL-09), filters (010-product-filters / BL-10) or sorting (012-product-sorting / BL-11) are available, **When** the Admin changes the search text, a filter or the sort order, **Then** the list returns to page 1 (BR-042) and the page size stays 50, applied to the matching products only (BR-041).
5. **Given** the Admin chose page size 50, **When** the Admin reloads the product management page while viewing the product list, **Then** the same page and page size 50 are shown (assumption, see Assumptions).
6. **Given** the Admin chose page size 50 in an earlier session, **When** the Admin signs in again and opens the product management page, **Then** the product list uses page size 20.

## Edge cases

- **No products:** The product list shows the empty-list message from 002-product-list (BR-022). The page size choice stays available, and choosing a size still shows the empty-list message.
- **Fewer products than the page size:** One page holds all products, and the totals show 1 page.
- **Exact multiples:** 100 products with page size 50 gives 2 pages. 101 products with page size 50 gives 3 pages, and page 3 holds 1 product.
- **The Admin picks the page size already in use:** Nothing changes. The Admin stays on the current page.
- **The Admin is on a high page number and picks a larger size:** The list goes to page 1 (BR-027). The "page beyond the last page" case cannot happen through this change.
- **Unsupported page size, for example from an edited link or a direct request** (any value other than 10, 20, 50, 100, including missing, zero, negative, fractional, non-numeric or very large): The page size becomes 20, no error is shown, and at most 20 products are returned (BR-009).
- **Products added or deleted in another tab** while the Admin pages with a non-default page size: The totals are recalculated with the page size in use. A page beyond the new last page shows the last existing page (BR-020, 002-product-list).
- **Session expired when the Admin changes the page size:** The request is refused (BR-015). Signing in again is handled by R002 (Admin sign-in).
- **Network failure while loading the list after a page size change:** The same behaviour as any failed product list load in 002-product-list (assumption).
- **Fallback through Back:** If the Admin uses Back to reach a view that was requested with an unsupported page size, the list again uses 20.

## Functional requirements

- **FR-001:** The product list MUST offer the Admin a page size choice of exactly these values: 10, 20, 50 and 100 (BR-009).
- **FR-002:** When the Admin has not chosen a page size in the current session, the product list MUST use page size 20, and the page size choice MUST show 20 (BR-008, BR-009).
- **FR-003:** When the Admin chooses a page size, the system MUST show page 1 of the product list with at most that many products, in the order in use (BR-027).
- **FR-004:** The product list totals MUST be calculated with the page size in use: the total number of products, and the number of pages equal to that total divided by the page size, rounded up (BR-021 as delivered by 002-product-list).
- **FR-005:** The system MUST check every requested page size at the boundary. Any value other than 10, 20, 50 or 100 (missing, zero, negative, fractional, non-numeric or out of the set) MUST be replaced by 20 before the list is produced, without an error message (BR-009, constitution P6).
- **FR-006:** The product list MUST show which page size is in use, including 20 after a fallback under FR-005.
- **FR-007:** The page size in use MUST stay the same when the Admin moves to another page of the product list (BR-009).
- **FR-008:** Returning from product details to the product list MUST restore the page size together with the page number (and the search text, filters and sort order where those exist) (BR-023, extended to page size by BL-07).
- **FR-009:** The browser Back and Forward buttons MUST restore the page size and page number of the previous or next product list view (BL-07 brief, QE-04). [NEEDS CLARIFICATION: is each page size change its own step in browser history (A), or does it replace the current step (B)? This must be decided once for page number, page size, search text, filters and sort order (002, 008, 009, 010, 012). US2 scenarios 2–3 and NFR-004 assume A.]
- **FR-010:** Changing the page size MUST NOT cause a full page reload (QE-04).
- **FR-011:** When search text, filters or sort order exist (BL-09, BL-10, BL-11), changing them MUST keep the page size in use. The page size and totals MUST apply to the matching products only (BR-041, BR-042).
- **FR-012:** Choosing the page size already in use MUST leave the product list unchanged, on the current page.
- **FR-013:** The product list, with any page size, MUST be refused to anyone without a signed-in Admin session (BR-015, constitution P5).
- **FR-014:** A chosen page size MUST NOT be remembered between sessions. After the Admin signs in again, the product list uses 20 (R001 assumption).
- **FR-015:** Within one session, when the Admin returns to the product list through the product management page's own navigation (not Back, not the return from product details), the product list MUST use [NEEDS CLARIFICATION: the last chosen page size, or 20 again?].

## Non-functional requirements

- **NFR-001:** (from QE-01, extended to larger page sizes) 95% of product list pages with page size 50 or 100, with 10,000 products stored, are shown within [NEEDS CLARIFICATION: does the 2-second target of QE-01, defined for 20 products per page, also apply to pages of 50 and 100 products?] seconds of the Admin's action, measured in the browser.
- **NFR-003:** (QE-03) 100% of product list requests, whatever the requested page size, are refused without a signed-in Admin session.
- **NFR-004:** (QE-04) Changing the page size, paging, and moving between the product list and product details cause 0 full page reloads. In 100% of tested navigations, the browser Back button returns to the previous product list view with the same page size and page number.
- **NFR-006:** (BR-009, constitution P6) 100% of requests with an unsupported page size are served with page size 20, and 0 requests return more than 100 products in one page.

## Key entities

- **Product list:** The paged list of all products on the product management page (defined by 002-product-list). In this feature it gains a page size choice. Its totals (number of products and pages) depend on the page size.
- **Page size:** The number of products shown on one page of the product list. It is one of 10, 20, 50 or 100, and 20 by default. It belongs to the current product list view together with the page number (and search text, filters and sort order where those exist).
- **Product:** An entry in the product list. This feature does not change products.

## Success criteria

- **SC-001:** The Admin changes the page size in at most 2 actions (open the choice, pick a value) and sees page 1 with the new size.
- **SC-002:** In acceptance testing, 100% of unsupported page size values (7, 1000, 0, -20, 20.5, "abc", missing) result in a list of at most 20 products and no error message.
- **SC-003:** In acceptance testing, 100% of returns from product details and Back-button navigations show the same page size and page number as before.

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BR-009 | FR-001, FR-002, FR-005, FR-006, FR-007; NFR-006; US1 scenarios 1, 2, 6; SC-002 |
| BR-015 | FR-013; NFR-003; US1 scenario 7; edge case "Session expired" |
| BR-027 | FR-003; US1 scenario 3; edge case "high page number"; FR-012 (no-op on the same size, assumption) |
| QE-03 | NFR-003; FR-013 |
| QE-04 | NFR-004; FR-008, FR-009, FR-010; US2 scenarios 1–3; SC-003 |

## Assumptions

- Reloading the product management page while viewing the product list keeps the current view (page number and page size). This matches the Back-button behaviour of QE-04. A new visit after signing in again starts at 20.
- Changing the search text, a filter or the sort order keeps the chosen page size. BR-042 only resets the page number.
- Choosing the page size already in use is not a change, so the list does not go back to page 1.
- The page size choice stays visible when the list is empty or holds fewer products than the smallest size.
- A failed list load after a page size change behaves like any failed product list load defined in 002-product-list.
- Admin sign-in, session expiry and re-sign-in are delivered by R002 (Admin sign-in), which is built before this feature. This spec only requires that the list is refused without a signed-in Admin session.
- NFR numbers follow the R001 quality expectation numbers they derive from (NFR-003 from QE-03, NFR-004 from QE-04). NFR-006 is an extra rule from BR-009 and constitution P6.

## Out of scope

- The product list itself: default page size 20 (BR-008), newest-first order, row content, totals, page navigation, the empty-list message and the "page beyond the last page" rule. These belong to 002-product-list (BL-02).
- Product details and the return to the product list. These belong to 003-product-details (BL-03). This spec only adds the page size to the state that is restored.
- Search text: 009-product-search (BL-09). Filters: 010-product-filters (BL-10). Sorting: 012-product-sorting (BL-11).
- The deleted products list. It keeps a fixed 20 per page (BR-046, 011-restore-products / BL-12), and the page size choice does not apply to it.
- Remembering the page size between sessions (R001 assumption: not remembered).
- Page sizes other than 10, 20, 50 and 100, a free-entry page size, and an "all products" option.
- Admin sign-in, account provisioning, lockout and session timeout (R002).

## Open questions

- [ ] 1. Does the 2-second target of QE-01 (95% of list pages, 10,000 products stored), defined for 20 products per page, also apply to pages of 50 and 100 products? Recommended: yes, the same 2 seconds for every page size. (affects: NFR-001)
- [ ] 2. Within one session, if the Admin leaves the product list (for example, to the deleted products list or the add form) and comes back through the page's own navigation rather than Back, should the list keep the last chosen page size or return to 20? Recommended: keep the last chosen page size until sign-out. (affects: FR-015, US2)
- [ ] 3. Is each page size change its own step in browser history, so that Back returns to the previous page size (A), or does a page size change replace the current step, so that Back only moves between views (B)? Decide once for page number, page size, search text, filters and sort order (002, 008, 009, 010, 012). Recommended: A, which 002 and 009 already assume. (affects: FR-009, US2 scenarios 2–3, NFR-004)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-16 | Initial version | — |
