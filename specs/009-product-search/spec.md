# Spec: Search products by name or SKU

**Feature:** 009-product-search
**Branch:** feat/009-product-search
**Source:** R001/BL-09
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-16

## Context

The Admin keeps up to 10,000 products in the product list (002-product-list, BL-02). The list shows 20 products per page, newest first. To find one product today, the Admin has to page through the whole list, and that alone can break the BG-01 target of under 1 minute per operation. This feature lets the Admin type search text on the product list. The list then shows only the products whose name or SKU contains that text, still paged and with totals for the matching products. It builds on the product list (BL-02). Admin sign-in comes from the separate requirements document R002 and is not specified here.

## User stories

### US1 — Find products by name or SKU (Priority: P1)

As the Admin, I want to type part of a product's name or SKU and see only the matching products, so that I find a product quickly without paging through the whole product list.

**Why P1:** This is the whole value of the feature. Without it the Admin can only page through the list.

**Independent test:** Store products "Blue Mug" (SKU MUG-01) and "Red Plate" (SKU PL-02) plus 30 products whose name contains "tea". Search for "mug", "pl-0" and "tea". Each time, check which products are shown and check the totals. This needs only the product list (BL-02).

**Acceptance scenarios:**
1. **Given** products "Blue Mug" (SKU MUG-01) and "Red Plate" (SKU PL-02) exist, **When** the Admin searches for "mug", **Then** the product list shows only "Blue Mug" and the totals show 1 product and 1 page.
2. **Given** the same products, **When** the Admin searches for "pl-0", **Then** the product list shows only "Red Plate" (matched on its SKU).
3. **Given** "Blue Mug" exists, **When** the Admin searches for "  MUG  " (upper case, with leading and trailing spaces), **Then** "Blue Mug" is shown.
4. **Given** a product "Đèn bàn" exists, **When** the Admin searches for "đèn", **Then** "Đèn bàn" is shown (letter case is ignored for all letters, including Vietnamese letters).
5. **Given** 30 products contain "tea" in their name or SKU and 15 others do not, **When** the Admin searches for "tea", **Then** page 1 shows the 20 newest matching products, and the totals show 30 products and 2 pages.
6. **Given** the Admin is on page 2 of the "tea" results, **When** the Admin changes the search text to "mug", **Then** the product list shows page 1 of the "mug" results.
7. **Given** the Admin is on page 3 of the product list without search text, **When** the Admin searches for "tea", **Then** page 1 of the matching products is shown.
8. **Given** no signed-in Admin session, **When** a search of the product list is requested, **Then** it is refused and no product data is returned.

### US2 — No matches and clearing the search (Priority: P1)

As the Admin, I want to be told clearly when nothing matches and to clear the search in one action, so that I can always get back to the full product list.

**Why P1:** Without a way to clear the search and a clear "no matching products" message, the Admin can get stuck on an empty-looking list and think products are missing.

**Independent test:** With some products stored, search for "zzz", check the "no matching products" message, use its clear action and check that the full product list is back on page 1.

**Acceptance scenarios:**
1. **Given** products exist but none has "zzz" in its name or SKU, **When** the Admin searches for "zzz", **Then** a "no matching products" message is shown with a way to clear the search text.
2. **Given** the "no matching products" message is shown, **When** the Admin uses the clear action, **Then** the search text is empty and the product list shows all products from page 1.
3. **Given** the Admin is on page 2 of the "tea" results, **When** the Admin clears the search text, **Then** the product list shows all products, page 1, newest first.
4. **Given** search text with only spaces, **When** the search is applied, **Then** every product matches, as with empty search text.
5. **Given** 100 characters of search text, **When** the search is applied, **Then** it is accepted.
6. **Given** a search request with 101 characters of search text, **When** it reaches the system, **Then** it is rejected at the boundary, a message is shown on the search text field and the product list shown stays unchanged.

### US3 — Keep the search while moving around (Priority: P2)

As the Admin, I want my search text and page to be kept when I open a product and come back, or when I press the browser Back button, so that I do not have to type the search again.

**Why P2:** Searching works without this, but redoing a search after every product the Admin opens slows down routine work.

**Independent test:** Search for "tea", go to page 2 and open a product's details. Return to the list, then press Back. Check the search text and the page each time.

**Acceptance scenarios:**
1. **Given** the Admin searched for "tea" and is on page 2, **When** the Admin opens a product's details and returns to the product list, **Then** the list shows page 2 of the "tea" results with "tea" in the search field.
2. **Given** the Admin searched for "mug" and then for "tea", **When** the Admin presses the browser Back button, **Then** the product list shows the "mug" results with "mug" in the search field.
3. **Given** the Admin searches, clears the search and pages through the results, **When** any of these actions happens, **Then** no full page reload occurs.

## Edge cases

- **No products exist at all and search text is entered:** the product list shows the empty-list message with a way to add a product (BR-022, from 002-product-list), not the "no matching products" message.
- **Requested page is beyond the last page of matching products** (e.g. page 5 when only 2 pages match): the last existing page of matching products is shown (BR-020, applied to matching products per BR-041).
- **Search text contains characters with a special meaning elsewhere**, such as `%`, `_`, `*`, `?`, quotes or backslash: each one is matched literally as plain text. For example, "%" matches only products whose name or SKU contains "%", and "_" matches nothing when no name or SKU contains "_".
- **Spaces inside the search text** (e.g. "blue mug"): only leading and trailing spaces are ignored, so inner spaces must match. "blue mug" matches "Blue Mug" but not "Blue  Mug" (two spaces) or "Mug Blue".
- **Search text over 100 characters** (e.g. typed into a modified address or pasted): rejected at the boundary with a message on the search text field (BR-037).
- **Accented and unaccented Vietnamese text** (e.g. "ca phe" against "Cà phê"): see FR-016 [NEEDS CLARIFICATION].
- **A product is deleted while the Admin is looking at search results:** the next time the results are shown, the product no longer matches (BR-007, delivered by 007-delete-product, BL-06). The totals shrink, and if the current page no longer exists the last existing page is shown.
- **A matching product is renamed in another tab:** the results reflect the stored data the next time the list is shown. There is no live update.
- **The session expires while searching:** the request is refused and no product data is shown. The sign-in flow from R002 applies.
- **Searching fails because of a network error:** an error message is shown and the search text stays in the field so the Admin can retry (assumption).
- **Very large result set** (e.g. every one of 10,000 products contains "a"): results are paged as usual with the current page size, and the totals show the full count.

## Functional requirements

- **FR-001:** The Admin MUST be able to enter search text in a search field on the product list. The field MUST show the search text currently applied. (BR-036)
- **FR-002:** The system MUST show only products whose name or SKU contains the search text, ignoring letter case. Letter case covers all letters, including Vietnamese letters with marks (e.g. "Đ"/"đ"). (BR-036)
- **FR-003:** The system MUST ignore leading and trailing spaces of the search text. Spaces inside the text MUST be kept and must match. (BR-036)
- **FR-004:** The system MUST treat every character of the search text literally. No character acts as a wildcard, pattern or command. (BR-036; constitution P6)
- **FR-005:** Empty search text, or search text made only of spaces, MUST match every product. (BR-037)
- **FR-006:** The search field MUST NOT accept more than 100 characters. Any search request whose text is longer than 100 characters after leading and trailing spaces are removed MUST be rejected at the boundary, with a message on the search text field, and the product list shown stays unchanged. (BR-037, QE-05)
- **FR-007:** Matching products MUST be shown paged, with the product list's current page size (20 by default; the page size chosen under 008-list-page-size, BL-07, once it exists). They MUST be in the product list's order (newest first per BR-010, unless a sort order from 012-product-sorting, BL-11, is chosen). (BR-041)
- **FR-008:** While search text is applied, the totals MUST show the number of matching products and the number of pages of matching products. (BR-041)
- **FR-009:** Applying new search text, changing it or clearing it MUST return the product list to page 1. (BR-042)
- **FR-010:** When products exist but none match, the product list MUST show a "no matching products" message with a way to clear the search text. Once filters exist (010-product-filters, BL-10), the same action also clears the filters. (BR-043)
- **FR-011:** The Admin MUST be able to clear the search text with one action whenever search text is applied. The product list then shows all products from page 1. (BR-042, BR-043)
- **FR-012:** Search text MUST combine with the filters (BL-10) and the sort order (BL-11) when those exist. A product is shown only if it matches the search text and every filter. Paging and totals apply to those products only. (BR-041)
- **FR-013:** When the requested page is beyond the last page of matching products, the system MUST show the last existing page of matching products. (BR-041 with BR-020)
- **FR-014:** Only products in the product list MUST be searched. Deleted products never match. (BR-007, delivered by 007-delete-product, BL-06)
- **FR-015:** Returning from product details MUST show the product list with the same search text and page. Each applied search MUST be a step in the browser history, so the Back button restores the previous search text and page. Searching, clearing and paging MUST NOT cause a full page reload. (QE-04, BR-023)
- **FR-016:** Accents MUST [NEEDS CLARIFICATION: Should matching also ignore Vietnamese accents (diacritics), so that "ca phe" finds "Cà phê"? BR-036 only says letter case and leading/trailing spaces are ignored. A) Accents matter, so "ca phe" does not find "Cà phê" (the literal reading of BR-036). B) Accents are ignored. Choosing B changes BR-036 through /sdd-amend requirements.]
- **FR-017:** The search MUST be applied [NEEDS CLARIFICATION: When is the search text applied? A) When the Admin presses Enter or a search button. B) Automatically, shortly after the Admin stops typing. This affects how many steps the Back button records (FR-015) and when QE-01 timing starts.]
- **FR-018:** Only the signed-in Admin MUST be able to search the product list. A request without a signed-in Admin session MUST be refused and MUST return no product data. (BR-015)

## Non-functional requirements

- **NFR-001 (QE-01):** 95% of product list pages showing search results (20 products per page, 10,000 products stored, with search text, with or without filters and sort order) are shown within 2 seconds of the Admin's action, measured in the browser.
- **NFR-002 (QE-03):** 100% of product list requests with search text are refused without a signed-in Admin session, and 0 product data is returned in those responses.
- **NFR-003 (QE-04):** Searching, clearing the search, paging through results and moving between the results and product details cause 0 full page reloads. In 100% of cases the browser Back button returns to the previous search text and page.
- **NFR-004 (QE-05):** 100% of invalid search requests (search text over 100 characters) are rejected at the boundary with a message on the search text field. 0 characters of search text are interpreted as a pattern or command.

## Key entities

- **Product:** something the shop sells. Search matches only its **name** and **SKU**. The other product information (price, description), the stock quantity and the product status are not searched. Matching products are ordered by date added unless a sort order applies.
- **Search text:** text the Admin types to narrow the product list to products whose name or SKU contains it. It holds 0 to 100 characters after leading and trailing spaces are removed. It is part of the product list's current view, together with the page number, page size, filters and sort order.
- **Product list:** the paged list of all products on the product management page. When search text is applied, it shows only the matching products, with totals for them.

## Success criteria

- **SC-001:** With 10,000 products stored, the Admin finds a known product by typing part of its name or SKU and opens its product details in under 1 minute. (BG-01)
- **SC-002:** On a reference set of products, a search shows 100% of the products whose name or SKU contains the search text (ignoring letter case and leading/trailing spaces) and 0 products that do not.
- **SC-003:** In 100% of tested cases, the Admin gets back to the full product list from any search result with one clear action.

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BG-01 | Context; SC-001; US1 |
| BR-015 | FR-018; US1 scenario 8; NFR-002 |
| BR-036 | FR-001, FR-002, FR-003, FR-004; US1 scenarios 1–4; Edge cases (special characters, inner spaces) |
| BR-037 | FR-005, FR-006; US2 scenarios 4–6; Edge case (over 100 characters) |
| BR-041 | FR-007, FR-008, FR-012, FR-013; US1 scenario 5; Edge cases (page beyond last, large result set) |
| BR-042 | FR-009, FR-011; US1 scenarios 6–7; US2 scenario 3 |
| BR-043 | FR-010, FR-011; US2 scenarios 1–2; Edge case (no products at all) |
| QE-01 | NFR-001 |
| QE-03 | NFR-002; FR-018 |
| QE-04 | NFR-003; FR-015; US3 scenarios 1–3 |
| QE-05 | NFR-004; FR-006; US2 scenario 6 |

## Assumptions

- Search depends on the paged product list (002-product-list, BL-02). It works alone, without page size choice (BL-07), filters (BL-10) or sorting (BL-11). When those exist, they combine as in FR-012.
- Admin sign-in, session handling and what the Admin sees when the session has expired are delivered by R002 (Admin sign-in). This spec only requires that searches are refused without a signed-in Admin session.
- Characters with a special meaning in other search tools (`%`, `_`, `*`, `?`) are matched literally. Neither the requirements nor the requester mention wildcard search.
- The 100-character limit (BR-037) is counted after leading and trailing spaces are removed.
- If the search fails because of a network error, the search text stays in the field with an error message so the Admin can retry, in line with BR-049 for forms.
- Search text is kept in the product list's current view, so reloading the page shows the same search. It is not remembered between sessions.
- Matched text is not highlighted in the results.

## Out of scope

- Searching the description, price, stock quantity or product status (BL-09 "Not included").
- Filtering by product status or stock: 010-product-filters (BL-10).
- Choosing a sort order: 012-product-sorting (BL-11).
- Choosing the page size: 008-list-page-size (BL-07).
- The product list itself: paging, default page size, newest-first order, empty-list message, totals and page-beyond-last behavior. These belong to 002-product-list (BL-02); this spec only applies them to matching products.
- Product details and the way back to the list: 003-product-details (BL-03).
- Deleting products and hiding deleted ones: 007-delete-product (BL-06).
- Searching the deleted products list: not part of 011-restore-products (BL-12) or of this item.
- Fuzzy matching, typo tolerance, suggestions or autocomplete, search history, and remembering the search between sessions.
- Admin sign-in: R002.

## Open questions

- [ ] Should search matching also ignore Vietnamese accents (diacritics), so that "ca phe" finds "Cà phê"? A) No, accents matter (the literal reading of BR-036). B) Yes, accents are ignored (needs /sdd-amend requirements on BR-036). (affects: FR-002, FR-016, SC-002)
- [ ] When is the search text applied? A) When the Admin presses Enter or a search button. B) Automatically, shortly after the Admin stops typing. (affects: FR-015, FR-017, NFR-001, NFR-003)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-16 | Initial version | — |
