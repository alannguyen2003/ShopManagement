# Requirements: Product management

**Requirements:** R001-product-management
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-16
**Requested by:** alannguyen2003 (shop Admin / product owner)
**Input:** [input.md](input.md)

## Context

The shop Admin needs one place to keep the shop's products and how many of each are in stock. Today the system has no product capability at all. The requester asked for five capabilities: add a product with a starting stock quantity, update product information, delete a product (only when it is in Draft status or its stock quantity is 0), view one product, and view a paged product list (20 per page by default). All of this happens on the product management page, which works as a single-page application. Success means the Admin can keep the product list complete and current without help, and cannot delete an Active product that is still in stock.

## Stakeholders and actors

| Actor / stakeholder | Role | Goals | Interaction with the system |
|---------------------|------|-------|-----------------------------|
| Admin | The only user of the system (constitution P1); manages products and stock | Keep the product list complete and the stock quantities correct; avoid deleting products by mistake | Uses |
| Shop owner / product owner (alannguyen2003) | Requester; same person as the Admin | Get a working product management page | Approves |

## Business goals

- **BG-01:** The Admin can add, view, update and delete any product from the product management page without outside help, and each single operation takes under 1 minute. *(Target confirmed in clarification 2026-10-16.)*
- **BG-02:** Every product in the system has a recorded stock quantity: 100% of products have a whole-number quantity of 0 or more at all times.
- **BG-03:** No Active product that is still in stock gets deleted: 0 such deletions.

## Scope

### In scope

- Adding a product with its product information; its stock quantity always starts at 1.
- Viewing the product list, paged, 20 products per page by default.
- Viewing the details of one product.
- Updating product information (name, SKU, price, description).
- Adjusting a product's stock quantity through a separate stock adjustment (add or remove units).
- Publishing a product: moving it from Draft to Active through a separate action (the Draft delete condition needs a way out of Draft).
- Deleting a product that is in Draft status or has a stock quantity of 0; a deleted product is hidden but kept.
- Viewing the deleted products and restoring one.
- Choosing the page size of the product list: 10, 20, 50 or 100.
- Searching the product list by name or SKU, filtering it by product status and by stock, and sorting it by column.

### Out of scope

- Admin sign-in / authentication. The constitution (P5) requires it before any product feature can be used; it is delivered by a separate requirements document, R002 (Admin sign-in), which this backlog depends on.
- A customer-facing storefront, orders, sales and automatic stock deduction: not requested.
- Product categories and product images: not requested; product information is limited to name, SKU, price and description.
- Stock movement history, audit trail, low-stock alerts: not requested.
- Bulk operations, import/export: not requested.
- Permanently erasing a deleted product: not requested; deleted products are kept.
- Several user accounts or roles: forbidden by constitution P1.

## Glossary

| Term | Definition | Do not call it |
|------|------------|----------------|
| Admin | The single person who signs in and manages products (constitution P1) | user, manager, operator, staff |
| Product | Something the shop sells and keeps stock of, with its product information, stock quantity and product status | item, article, goods, SKU (as a synonym for the whole product) |
| Product information | The descriptive fields of a product: name, SKU, price and description; excludes stock quantity and product status | product data, product details (for the fields) |
| SKU | The shop's unique code for a product (stock keeping unit) | code, reference, barcode, ID |
| Price | The selling price of one unit of a product, a whole number of Vietnamese dong (VND) | cost, amount, rate |
| Stock quantity | Whole number of units of a product the shop has, 0 or more | stock level, inventory count, qty, amount |
| Starting stock quantity | The stock quantity a product gets when it is added: always exactly 1, not entered by the Admin | default stock, initial inventory |
| Out of stock | A product whose stock quantity is 0 | sold out, empty |
| Stock adjustment | An action that adds a number of units to, or removes a number of units from, a product's stock quantity | stock update, restock, stock edit |
| Product status | Where a product is in its lifecycle: Draft or Active | state, publish state, visibility |
| Draft | The product status of a newly added product that has not been published yet | unpublished, pending, inactive |
| Active | The product status of a published product; a product never goes back from Active to Draft | live, published, enabled |
| Publish (a product) | Move a product from Draft to Active | activate, release, go live |
| Product list | The paged list of all products shown on the product management page | catalogue, inventory list, grid |
| Page size | Number of products shown on one page of the product list; 20 by default | page length, limit |
| Search text | Text the Admin types to narrow the product list to products whose name or SKU contains it | query, keyword, lookup |
| Filter | A choice that narrows the product list to products with a given product status or stock (in stock / out of stock) | facet, criteria |
| Sort order | The column the product list is ordered by, and the direction (ascending or descending) | ordering, ranking |
| Product details | The view showing all product information, the stock quantity and the product status of one product | product page, product info view |
| Product management page | The single-page interface where the Admin performs all product operations | dashboard, admin panel, back office |
| Delete (a product) | Mark a product as deleted: it no longer appears in the product list or product details, but it is kept and can be restored | remove, archive, discard, erase |
| Deleted products list | The paged list of deleted products, from which the Admin can restore one | trash, recycle bin, archive |
| Restore (a product) | Bring a deleted product back, unchanged, into the product list | undelete, recover, reactivate |

## Business processes

### PR-01 — Add a product

1. The Admin opens the add form from the product management page.
2. The system shows the form with the product information fields; it has no stock quantity field.
3. The Admin enters the product information.
4. The Admin saves.
5. The system checks the input, saves the product with stock quantity 1 and status Draft, and shows a success confirmation with the saved values.

**Exceptions:**
- A required field is missing or invalid → the system rejects the save, saves nothing and shows the reason next to each invalid field.
- The SKU is already used by another product → the system rejects the save, saves nothing and shows the reason next to the SKU field (BR-030).
- The Admin cancels → nothing is saved; the system returns to the product management page.
- The Admin clicks Save twice → at most one product is created (BR-028).
- The save fails (network error or expired session) → the Admin is told the product was not saved; the form stays open with the entered values so the Admin can retry (BR-049).

### PR-02 — View the product list

1. The Admin opens the product management page.
2. The system shows the first page of the product list, 20 products per page, newest first, with the total number of products and pages.
3. The Admin moves to another page, chooses another page size (10, 20, 50 or 100), enters search text, sets filters or changes the sort order.
4. The system shows the requested page of the matching products; a change of search text, filter or sort order goes back to page 1.

**Exceptions:**
- No products exist → the system shows an empty-list message and a way to add a product.
- Products exist but none match the search text and filters → the system shows a "no matching products" message and a way to clear the search text and filters.
- The requested page is beyond the last page (e.g. after a deletion) → the system shows the last existing page.

### PR-03 — View product details

1. The Admin selects a product in the product list.
2. The system shows the product details: all product information, stock quantity and product status.
3. The Admin returns to the product list at the same page, with the same search text, filters and sort order.

**Exceptions:**
- The product no longer exists → the system shows a "product not found" message and a way back to the product list.

### PR-04 — Update a product

1. From the product details, the Admin opens the edit form.
2. The system shows the form filled with the current product information; it has no stock quantity or status field.
3. The Admin changes values and saves.
4. The system checks the input, saves the changes and shows the updated product details.

**Exceptions:**
- Invalid value (e.g. missing required field, price below 0, SKU already used by another product) → the system rejects the save, keeps the stored product unchanged and shows the reason next to each invalid field.
- The product was deleted meanwhile → the system shows "product not found"; nothing is saved.
- The Admin cancels → nothing is saved.
- The save fails (network error or expired session) → the Admin is told the change was not saved; the stored product is unchanged and the form stays open with the entered values so the Admin can retry (BR-049).

### PR-05 — Publish a product

1. From the product details of a Draft product, the Admin chooses to publish it.
2. The system checks the product is still Draft, sets its status to Active and shows the updated product details.

**Exceptions:**
- The product is already Active (e.g. published from another tab) → the system refuses, explains that only a Draft product can be published, and the product stays Active.
- The product no longer exists → "product not found".

### PR-07 — Adjust stock

1. From the product details, the Admin chooses to adjust the stock.
2. The Admin chooses add or remove and enters a number of units (a whole number from 1 to 1,000,000).
3. The Admin saves.
4. The system applies the adjustment to the stored stock quantity and shows the updated product details.

**Exceptions:**
- The number of units is missing, 0, negative, above 1,000,000 or not a whole number → the system rejects it, the stock quantity is unchanged and the reason is shown next to the field.
- Removing the units would bring the stock quantity below 0 → the system refuses, tells the Admin the current stock quantity, and the stock quantity is unchanged.
- Adding the units would bring the stock quantity above 1,000,000 → the system refuses, tells the Admin the current stock quantity and the limit, and the stock quantity is unchanged.
- The product no longer exists → "product not found".
- The Admin cancels → nothing changes.

### PR-08 — Restore a deleted product

1. The Admin opens the deleted products list from the product management page.
2. The system shows the deleted products, 20 per page, most recently deleted first.
3. The Admin chooses to restore one.
4. The system brings the product back with its product information, stock quantity and product status unchanged, and it appears in the product list again.

**Exceptions:**
- No deleted products exist → the system shows an empty-list message.
- The product was already restored (e.g. from another tab) → "product not found" in the deleted products list; nothing changes.

### PR-06 — Delete a product

1. From the product details or the product list, the Admin chooses to delete a product.
2. The system checks that the product is in Draft status or out of stock.
3. The system asks the Admin to confirm.
4. The Admin confirms.
5. The system deletes the product and shows the product list.

**Exceptions:**
- The product is not in Draft status and its stock quantity is above 0 → the system refuses the deletion and tells the Admin it can only delete a Draft or out-of-stock product.
- The Admin does not confirm → nothing is deleted.
- The product changed between step 2 and step 4 so that it no longer qualifies → the system refuses the deletion (the rule is checked against stored data at the moment of deletion).
- The product no longer exists → "product not found".
- The Admin confirms twice → the product is deleted once; the second request reports "product not found".

## Business rules

- **BR-001:** A newly added product always gets a stock quantity of exactly 1. *(Source: input "stocks default … at least 1"; clarification 2026-10-16)*
- **BR-002:** The Admin does not enter a stock quantity when adding a product; the add form has no stock quantity field. *(Source: clarification 2026-10-16)*
- **BR-003:** After a product is added, its stock quantity is a whole number from 0 to 1,000,000; a negative, fractional or larger quantity is never stored. *(Source: input "stock quantity is down to 0"; clarification 2026-10-16)*
- **BR-004:** A product may be deleted only if its product status is Draft or its stock quantity is 0. *(Source: input)*
- **BR-005:** When a deletion is refused under BR-004, the system tells the Admin the reason and the product stays unchanged. *(Source: assumption)*
- **BR-006:** A deletion takes effect only after the Admin explicitly confirms it. *(Source: assumption)*
- **BR-007:** A deleted product no longer appears in the product list (including search, filters and totals) and its product details can no longer be opened. *(Source: input "delete product"; clarification 2026-10-16)*
- **BR-008:** The product list shows 20 products per page by default. *(Source: input)*
- **BR-009:** The Admin can change the page size of the product list to 10, 20, 50 or 100; any other requested value falls back to 20. *(Source: input "default is 20"; clarification 2026-10-16)*
- **BR-010:** The product list is ordered by date added, newest first. *(Source: clarification 2026-10-16)*
- **BR-011:** Every product has exactly one product status, Draft or Active. *(Source: clarification 2026-10-16)*
- **BR-012:** The only allowed status change is Draft → Active (publish); an Active product never returns to Draft. *(Source: clarification 2026-10-16)*
- **BR-029:** A newly added product always gets status Draft; the add and edit forms do not set the status. *(Source: clarification 2026-10-16)*
- **BR-013:** Every product has a name, which is required and is 1 to 200 characters long. *(Source: clarification 2026-10-16)*
- **BR-014:** Product information consists of exactly these fields: name, SKU, price and description. *(Source: clarification 2026-10-16)*
- **BR-030:** Every product has a SKU, which is required, 1 to 50 characters long and unique among all products; uniqueness ignores letter case and leading/trailing spaces. *(Source: clarification 2026-10-16; case and space handling is an assumption)*
- **BR-031:** Every product has a price, which is required and is a whole number of Vietnamese dong (VND) from 0 to 1,000,000,000; a fractional or out-of-range price is rejected. *(Source: clarification 2026-10-16)*
- **BR-050:** The shop uses a single currency, VND; prices are shown with a dot as thousands separator and the ₫ sign (e.g. 125.000 ₫). *(Source: clarification 2026-10-16; display format is an assumption)*
- **BR-032:** A product's description is optional and at most 2,000 characters long. *(Source: clarification 2026-10-16)*
- **BR-015:** Only the signed-in Admin can view, add, update, change status of or delete products. *(Source: constitution P1, P5)*
- **BR-016:** After a product is added, its stock quantity changes only through a stock adjustment; the edit form does not show or change the stock quantity. *(Source: clarification 2026-10-16)*
- **BR-033:** A stock adjustment adds or removes a whole number of units from 1 to 1,000,000. *(Source: clarification 2026-10-16)*
- **BR-034:** A stock adjustment that would bring the stock quantity below 0 or above 1,000,000 is refused and the stock quantity is unchanged. *(Source: clarification 2026-10-16, follows from BR-003)*
- **BR-035:** A stock adjustment is applied to the stored stock quantity at the moment it is saved, so two adjustments made close together both count. *(Source: assumption)*
- **BR-036:** A product matches the search text when its name or SKU contains that text, ignoring letter case and leading/trailing spaces. *(Source: clarification 2026-10-16; matching rule is an assumption)*
- **BR-037:** Search text is at most 100 characters; empty search text matches every product. *(Source: assumption)*
- **BR-038:** The Admin can filter the product list by product status: all, Draft or Active. *(Source: clarification 2026-10-16)*
- **BR-039:** The Admin can filter the product list by stock: all, in stock (stock quantity 1 or more) or out of stock (stock quantity 0). *(Source: clarification 2026-10-16; the two stock values are an assumption)*
- **BR-040:** The Admin can sort the product list by name, SKU, price, stock quantity or date added, ascending or descending; products with equal values are ordered by date added, newest first. Without a chosen sort order, BR-010 applies. *(Source: clarification 2026-10-16; columns and tie rule are assumptions)*
- **BR-041:** Search text, filters and sort order can be used together; paging and the totals of BR-021 apply to the matching products only. *(Source: assumption)*
- **BR-042:** Changing the search text, a filter or the sort order returns the product list to page 1. *(Source: assumption)*
- **BR-044:** A deleted product is kept with its product information, stock quantity and product status, and can be restored. *(Source: clarification 2026-10-16)*
- **BR-045:** A deleted product's SKU stays reserved: no other product can be added or updated with that SKU while the deleted product is kept. *(Source: assumption, so that a restore never conflicts)*
- **BR-046:** The deleted products list shows each deleted product's name, SKU and deletion date, 20 per page, most recently deleted first. *(Source: assumption)*
- **BR-047:** Restoring a deleted product brings it back unchanged (product information, stock quantity, product status) and needs no confirmation. *(Source: clarification 2026-10-16; no confirmation is an assumption)*
- **BR-048:** A deleted product cannot be edited, adjusted, published or deleted again; such requests report "product not found". *(Source: assumption)*
- **BR-049:** When saving the add form, the edit form or a stock adjustment fails (network error or expired session), the form stays open in the same tab with the entered values and an error message, so the Admin can retry, after signing in again if needed; the values are not stored anywhere else in the browser. *(Source: clarification 2026-10-16)*
- **BR-043:** When products exist but none match the search text and filters, the product list shows a "no matching products" message and a way to clear the search text and filters. *(Source: assumption)*
- **BR-017:** When an add or update is rejected, nothing is saved and the Admin sees which field is invalid and why. *(Source: assumption; constitution P6)*
- **BR-018:** Product details show all product information, the stock quantity and the product status. *(Source: input "viewing product information")*
- **BR-019:** Each row of the product list shows the product's name, SKU, price, stock quantity and product status; the description appears only in the product details. *(Source: assumption, based on the fields fixed by clarification 2026-10-16)*
- **BR-020:** When the requested page is beyond the last page, the product list shows the last existing page. *(Source: clarification 2026-10-16)*
- **BR-021:** The product list shows the total number of products and of pages. *(Source: assumption)*
- **BR-022:** When no products exist, the product list shows an empty-list message and a way to add a product. *(Source: assumption)*
- **BR-023:** Returning from product details shows the product list at the page, search text, filters and sort order the Admin came from. *(Source: assumption)*
- **BR-024:** Delete is offered from both the product list and the product details. *(Source: assumption)*
- **BR-025:** The delete condition of BR-004 is checked against the stored product at the moment of deletion, not against what the screen showed. *(Source: assumption)*
- **BR-026:** When the same product is saved from two places, the last save wins. *(Source: assumption)*
- **BR-027:** After the page size changes, the product list returns to page 1. *(Source: assumption)*
- **BR-028:** One Save action creates at most one product, even if the Admin submits it twice. *(Source: assumption)*

## Quality expectations

- **QE-01:** 95% of product list pages (20 products, 10,000 stored, with or without search text, filters and sort order) are shown within 2 seconds of the Admin's action, measured in the browser. *(Confirmed in clarification 2026-10-16.)*
- **QE-02:** 95% of adds, updates, stock adjustments, publishes, deletions and restores (10,000 products stored) are confirmed to the Admin within 1 second of the action, measured in the browser. *(Confirmed in clarification 2026-10-16.)*
- **QE-03:** 100% of product operations and product views are refused without a signed-in Admin session. *(Constitution P5.)*
- **QE-04:** Moving between the product list, product details, the add form and the edit form causes 0 full page reloads, and the browser Back button returns to the previous view, including the product list page number, search text, filters and sort order. *(Input "SPA"; Back-button behavior is an assumption.)*
- **QE-05:** 100% of invalid inputs are rejected at the boundary with a field-level message, and 0 invalid values are stored. *(Constitution P6.)*

## Constraints

- The product management page must work as a single-page application. *(Source: input "I want it to go the way SPA of frontend"; matches the React + Vite frontend in docs/architecture.md.)*
- Exactly one Admin account; no registration or roles. *(Source: constitution P1.)*
- Every product operation and page requires a signed-in Admin; sign-in must be protected against brute force. *(Source: constitution P5.)*
- All input validated at the boundary. *(Source: constitution P6.)*

## Assumptions

- The Admin is the only actor; there is no customer-facing part, so a product status only matters to the Admin and to the delete rule.
- Stock quantity counts whole units; no units of measure, fractions or multiple warehouses.
- Deletion always asks for confirmation (BR-006).
- Up to 10,000 products is the expected volume for QE-01 and QE-02 (confirmed in clarification 2026-10-16).
- The Admin may have two browser tabs open; the last save wins, and an operation on a product deleted meanwhile reports "product not found".
- No history of stock changes is kept in this initiative, and a stock adjustment does not record a reason.
- A chosen page size is not remembered between sessions (BL-07).

## Risks and dependencies

- **Admin sign-in does not exist yet.** Constitution P5 requires a signed-in Admin for every product page and operation, so no backlog item here can be shipped until sign-in exists. → Delivered by R002 (Admin sign-in), analysed with `/sdd-ba` and built before BL-01.
- **Deleted products accumulate** and keep their SKUs reserved (BR-045), so a SKU cannot be reused without first restoring and changing the old product. → Acceptable at the expected volume; permanent erasing can be added later through `/sdd-amend requirements`.

## Backlog

### Summary

| ID | Feature | Priority | Size | Depends on | Covers | Spec |
|----|---------|----------|------|------------|--------|------|
| BL-01 | Add a product | P1 | M | R002 (Admin sign-in) | BG-01, BG-02, BR-001, BR-002, BR-003, BR-011, BR-029, BR-013, BR-014, BR-030, BR-031, BR-032, BR-050, BR-045, BR-015, BR-017, BR-028, BR-049, QE-02, QE-03, QE-04, QE-05 | — |
| BL-02 | Paged product list | P1 | M | BL-01 | BG-01, BR-008, BR-010, BR-015, BR-019, BR-050, BR-020, BR-021, BR-022, QE-01, QE-03, QE-04 | — |
| BL-03 | View product details | P1 | S | BL-02 | BG-01, BR-014, BR-015, BR-018, BR-050, BR-023, QE-03, QE-04 | — |
| BL-04 | Update product information | P1 | M | BL-03 | BG-01, BR-029, BR-013, BR-014, BR-030, BR-031, BR-032, BR-050, BR-015, BR-016, BR-017, BR-026, BR-049, QE-02, QE-03, QE-04, QE-05 | — |
| BL-05 | Publish a product | P1 | S | BL-03 | BG-03, BR-011, BR-012, BR-015, QE-02, QE-03 | — |
| BL-08 | Adjust stock | P1 | S | BL-03 | BG-01, BG-02, BR-003, BR-015, BR-016, BR-033, BR-034, BR-035, BR-049, QE-02, QE-03, QE-04, QE-05 | — |
| BL-06 | Delete a product | P1 | M | BL-03, BL-05, BL-08 | BG-01, BG-03, BR-004, BR-005, BR-006, BR-007, BR-044, BR-048, BR-015, BR-024, BR-025, QE-02, QE-03, QE-04 | — |
| BL-07 | Choose product list page size | P2 | S | BL-02 | BR-009, BR-015, BR-027, QE-03, QE-04 | — |
| BL-09 | Search products by name or SKU | P2 | S | BL-02 | BG-01, BR-015, BR-036, BR-037, BR-041, BR-042, BR-043, QE-01, QE-03, QE-04, QE-05 | — |
| BL-10 | Filter products by status and stock | P2 | S | BL-02, BL-05, BL-08 | BR-015, BR-038, BR-039, BR-041, BR-042, BR-043, QE-01, QE-03, QE-04, QE-05 | — |
| BL-11 | Sort the product list | P3 | S | BL-02 | BR-010, BR-015, BR-040, BR-041, BR-042, QE-01, QE-03, QE-04, QE-05 | — |
| BL-12 | View and restore deleted products | P2 | M | BL-06 | BR-015, BR-044, BR-045, BR-046, BR-047, QE-02, QE-03, QE-04 | — |

Size: `S` ≤ 1 day, `M` 1–3 days. The `Spec` column holds the reserved `NNN-slug` once `/sdd-ba-spec` creates the spec; `—` until then.

**Every item depends on Admin sign-in** (constitution P5), delivered by the separate requirements document R002; BL-01 lists it explicitly and the others inherit it through BL-01. P1 items BL-01 to BL-06 and BL-08 form the MVP (BL-08 was added during clarification and is listed before BL-06, which depends on it). BL-09 to BL-12 were added during clarification; their priorities (search, filter and restore P2, sort P3) are an assumption. Each one alone works with the others not yet built; once several exist they combine (BR-041).

### BL-01 — Add a product

**Value:** The Admin can record a new product, so the product list starts to reflect what the shop sells.

**Actors:** Admin

**Spec brief:** The signed-in Admin opens the add form on the product management page, enters the product information (no stock quantity field) and saves. The system validates every field at the boundary, stores the product with stock quantity 1 and status Draft and shows a success confirmation with the saved values. Product information is name, SKU (unique), price and description (BR-013, BR-014, BR-030–BR-032). Invalid input is rejected field by field and nothing is saved.

**Key scenarios:**
- Admin enters valid product information and saves → product stored with stock quantity 1 and status Draft; success confirmation shown with the saved values.
- Admin clicks Save twice quickly → only one product is created.
- A request that includes a stock quantity (e.g. 50) → the value is ignored or rejected; the product is never stored with a stock quantity other than 1.
- Admin leaves the name empty → rejected with a message on the name field.
- Admin enters SKU "abc-1 " when "ABC-1" already exists → rejected with a message on the SKU field.
- Admin enters price -1, 1500.5 or 1,000,000,001 → rejected with a message on the price field.
- Request without a signed-in Admin session → refused.

**Not included:** Listing products (BL-02), product details (BL-03), publishing (BL-05).

### BL-02 — Paged product list

**Value:** The Admin sees all products and their stock at a glance, page by page.

**Actors:** Admin

**Spec brief:** The signed-in Admin opens the product management page and sees the product list, 20 products per page, newest first, each row showing name, SKU, price, stock quantity and product status, with page navigation and the total count. Navigation stays within the single-page application and the Back button restores the previous page number. With no products, an empty-list message offers a way to add one. A page number beyond the last page shows the last page.

**Key scenarios:**
- 45 products exist → page 1 shows the 20 newest, page 3 shows the last 5; total shows 45.
- No products → empty-list message with a link to the add form.
- Admin requests page 10 when only 3 pages exist → page 3 shown.
- Request without a signed-in Admin session → refused.

**Not included:** Changing the page size (BL-07); search (BL-09), filters (BL-10) and choosing a sort order (BL-11).

### BL-03 — View product details

**Value:** The Admin can check every piece of information about one product, and reach its edit, stock adjustment, publish and delete actions.

**Actors:** Admin

**Spec brief:** The signed-in Admin selects a product in the product list and sees its product details: all product information, stock quantity and product status, without a full page reload. A way back returns to the same page of the product list. If the product no longer exists, a "product not found" message with a way back is shown.

**Key scenarios:**
- Admin selects a product on page 2 → details shown; going back shows page 2.
- Admin opens the details of a product deleted meanwhile → "product not found".
- Request without a signed-in Admin session → refused.

**Not included:** Editing (BL-04), publishing (BL-05), deleting (BL-06), adjusting stock (BL-08).

### BL-04 — Update product information

**Value:** The Admin keeps product information correct over time.

**Actors:** Admin

**Spec brief:** From the product details, the signed-in Admin opens the edit form pre-filled with the current product information (name, SKU, price, description), changes values and saves. The form has no stock quantity or status field (BR-016, BR-029). The system validates every field at the boundary; on success the updated product details are shown; on failure nothing is saved and each invalid field is explained.

**Key scenarios:**
- Admin changes the name and price → saved; details show the new values; stock quantity and status unchanged.
- Admin enters price -1 → rejected on the price field; stored product unchanged.
- A request that includes a stock quantity or status → those values are ignored or rejected; stock quantity and status unchanged.
- Admin changes the SKU to one used by another product → rejected on the SKU field; stored product unchanged.
- Admin saves a product deleted meanwhile → "product not found"; nothing saved.
- Request without a signed-in Admin session → refused.

**Not included:** Publishing (BL-05); stock quantity changes (BL-08).

### BL-05 — Publish a product

**Value:** The Admin marks a product as ready (Active), which also protects it from deletion while it is in stock.

**Actors:** Admin

**Spec brief:** From the product details of a Draft product, the signed-in Admin publishes it with a separate action; the system checks the stored product is still Draft, sets it to Active and shows the updated status in the product details and the product list. Draft → Active is the only allowed change; an Active product offers no publish action and can never return to Draft (BR-012).

**Key scenarios:**
- Admin publishes a Draft product → status Active, shown in details and list.
- Publish requested for a product that is already Active → refused with a reason; status stays Active.
- Request without a signed-in Admin session → refused.

**Not included:** The initial status at creation (BL-01, always Draft); deletion (BL-06); any way back to Draft.

### BL-08 — Adjust stock

**Value:** The Admin keeps stock quantities matching what is on the shelf, including bringing a product down to 0 so it can be deleted.

**Actors:** Admin

**Spec brief:** From the product details, the signed-in Admin adjusts the stock: chooses add or remove and enters a whole number of units from 1 to 1,000,000. The system validates the input at the boundary, applies the adjustment to the stored stock quantity at that moment (BR-035), and shows the new stock quantity. An adjustment that would take the stock below 0 or above 1,000,000 is refused with the current stock quantity shown; nothing changes. No reason or history is recorded.

**Key scenarios:**
- Stock 1, Admin adds 24 → stock 25.
- Stock 25, Admin removes 25 → stock 0 (product is now out of stock).
- Stock 3, Admin removes 5 → refused; message shows current stock 3; stock stays 3.
- Stock 999,990, Admin adds 20 → refused; message shows current stock and the 1,000,000 limit; stock unchanged.
- Admin enters 0, -2 or 1.5 units → rejected on the units field; stock unchanged.
- Two adjustments (add 5, remove 2) saved close together from two tabs on stock 10 → stock 13.
- Request without a signed-in Admin session → refused.

**Not included:** Editing product information (BL-04); stock history, reasons, low-stock alerts (out of scope).

### BL-06 — Delete a product

**Value:** The Admin can delete Draft products and out-of-stock products, while Active products that are still in stock are protected.

**Actors:** Admin

**Spec brief:** From the product details or the product list, the signed-in Admin chooses to delete a product. The system allows it only if the product status is Draft or the stock quantity is 0, checked against stored data at the moment of deletion; otherwise it refuses and explains why. An allowed deletion requires explicit confirmation; afterwards the product no longer appears in the product list or details, but it is kept with all its data and its SKU stays reserved (BR-044, BR-045). Any later edit, stock adjustment, publish or delete of it reports "product not found" (BR-048).

**Key scenarios:**
- Draft product with quantity 12 → confirmation → deleted; no longer in the list.
- Active product with quantity 0 → confirmation → deleted.
- Active product with quantity 3 → refused with the reason; product unchanged.
- Admin cancels the confirmation → nothing deleted.
- After deleting product SKU "MUG-01", adding a new product with SKU "MUG-01" → rejected on the SKU field.
- Request without a signed-in Admin session → refused.

**Not included:** Viewing and restoring deleted products (BL-12); permanent erasing and bulk deletion (out of scope).

### BL-07 — Choose product list page size

**Value:** The Admin can see more or fewer products per page than the default 20.

**Actors:** Admin

**Spec brief:** On the product list, the signed-in Admin picks a page size of 10, 20, 50 or 100 (BR-009); the list returns to page 1 with the new size (BR-027) and the totals update. The default stays 20. A page size outside the allowed set → the list uses 20 (no error). The page size survives going to product details and back, and the Back button.

**Key scenarios:**
- 120 products, Admin chooses 50 → page 1 shows 50 products; totals show 3 pages.
- A page size outside the allowed set (e.g. 7 or 1000) is requested → the list uses 20.
- Request without a signed-in Admin session → refused.

**Not included:** Remembering the choice between sessions (assumption: not remembered).

### BL-09 — Search products by name or SKU

**Value:** The Admin finds a product quickly without paging through the whole list.

**Actors:** Admin

**Spec brief:** On the product list, the signed-in Admin types search text (up to 100 characters). The list shows only products whose name or SKU contains the text, ignoring letter case and leading/trailing spaces, still paged, with totals for the matching products, starting at page 1. Clearing the text shows all products again. When nothing matches, a "no matching products" message offers a way to clear the search. The search text survives going to product details and back, and the Back button.

**Key scenarios:**
- Products "Blue Mug" (SKU MUG-01) and "Red Plate" (SKU PL-02) → search "mug" shows only Blue Mug; search "pl-0" shows only Red Plate.
- 30 products contain "tea" → page 1 shows 20, totals show 30 and 2 pages.
- Search "zzz" → "no matching products" with a way to clear the search.
- Search text of 101 characters → rejected at the boundary; the list is unchanged.
- Request without a signed-in Admin session → refused.

**Not included:** Searching the description; filters (BL-10); choosing a sort order (BL-11).

### BL-10 — Filter products by status and stock

**Value:** The Admin sees at a glance which products are still Draft, or which are out of stock and may need restocking or deleting.

**Actors:** Admin

**Spec brief:** On the product list, the signed-in Admin picks a status filter (all, Draft, Active) and a stock filter (all, in stock, out of stock). The list shows only matching products, paged, with totals for the matching products, starting at page 1. Filters combine with each other and with search text and sort order when those exist. Values outside the allowed set are rejected at the boundary. When nothing matches, a "no matching products" message offers a way to clear the filters. The filters survive going to product details and back, and the Back button.

**Key scenarios:**
- 5 Draft and 8 Active products → status filter Draft shows the 5 Draft products.
- Stock filter out of stock → only products with stock quantity 0.
- Status Active + out of stock → only Active products with stock quantity 0.
- An unknown filter value is requested → rejected at the boundary.
- Request without a signed-in Admin session → refused.

**Not included:** Filtering by price range or date; search (BL-09); choosing a sort order (BL-11).

### BL-11 — Sort the product list

**Value:** The Admin orders the list the way that suits the task, e.g. lowest stock first or by name.

**Actors:** Admin

**Spec brief:** On the product list, the signed-in Admin sorts by name, SKU, price, stock quantity or date added, ascending or descending. Equal values are ordered by date added, newest first. Without a chosen sort order the list is newest first (BR-010). Changing the sort order returns to page 1; it combines with search text and filters when those exist, and survives going to product details and back. Unknown sort columns or directions are rejected at the boundary.

**Key scenarios:**
- Sort by stock quantity ascending → products with stock 0 first.
- Sort by name descending → Z to A; two products with the same name → newer first.
- Sort by price on page 3, then change to sort by name → page 1 shown.
- An unknown sort column is requested → rejected at the boundary.
- Request without a signed-in Admin session → refused.

**Not included:** Sorting by more than one chosen column; remembering the sort order between sessions.

### BL-12 — View and restore deleted products

**Value:** A product deleted by mistake can be brought back with all its data.

**Actors:** Admin

**Spec brief:** From the product management page, the signed-in Admin opens the deleted products list: name, SKU and deletion date, 20 per page, most recently deleted first. Choosing restore brings the product back unchanged (product information, stock quantity, product status) without a confirmation step; it reappears in the product list and leaves the deleted products list. Navigation stays within the single-page application.

**Key scenarios:**
- Admin deletes Draft product "Blue Mug" and restores it → it is back in the product list as Draft with the same stock quantity.
- Restored Active product with stock 0 → back in the product list as Active with stock 0.
- No deleted products → empty-list message.
- Restore requested for a product already restored → "product not found"; nothing changes.
- Request without a signed-in Admin session → refused.

**Not included:** Searching or filtering the deleted products list; permanently erasing a deleted product (out of scope).

## Traceability

| Goal / rule / quality | Backlog items |
|-----------------------|---------------|
| BG-01 | BL-01, BL-02, BL-03, BL-04, BL-06, BL-08, BL-09 |
| BG-02 | BL-01, BL-08 |
| BG-03 | BL-05, BL-06 |
| BR-001 | BL-01 |
| BR-002 | BL-01 |
| BR-003 | BL-08 |
| BR-004 | BL-06 |
| BR-005 | BL-06 |
| BR-006 | BL-06 |
| BR-007 | BL-06 |
| BR-008 | BL-02 |
| BR-009 | BL-07 |
| BR-010 | BL-02, BL-11 |
| BR-011 | BL-01, BL-05 |
| BR-012 | BL-05 |
| BR-013 | BL-01, BL-04 |
| BR-014 | BL-01, BL-03, BL-04 |
| BR-015 | BL-01, BL-02, BL-03, BL-04, BL-05, BL-06, BL-07, BL-08, BL-09, BL-10, BL-11, BL-12 |
| BR-016 | BL-04, BL-08 |
| BR-017 | BL-01, BL-04 |
| BR-018 | BL-03 |
| BR-019 | BL-02 |
| BR-020 | BL-02 |
| BR-021 | BL-02 |
| BR-022 | BL-02 |
| BR-023 | BL-03 |
| BR-024 | BL-06 |
| BR-025 | BL-06 |
| BR-026 | BL-04 |
| BR-027 | BL-07 |
| BR-028 | BL-01 |
| BR-029 | BL-01, BL-04 |
| BR-030 | BL-01, BL-04 |
| BR-031 | BL-01, BL-04 |
| BR-032 | BL-01, BL-04 |
| BR-033 | BL-08 |
| BR-034 | BL-08 |
| BR-035 | BL-08 |
| BR-036 | BL-09 |
| BR-037 | BL-09 |
| BR-038 | BL-10 |
| BR-039 | BL-10 |
| BR-040 | BL-11 |
| BR-041 | BL-09, BL-10, BL-11 |
| BR-042 | BL-09, BL-10, BL-11 |
| BR-043 | BL-09, BL-10 |
| BR-044 | BL-06, BL-12 |
| BR-045 | BL-01, BL-12 |
| BR-046 | BL-12 |
| BR-047 | BL-12 |
| BR-048 | BL-06 |
| BR-049 | BL-01, BL-04, BL-08 |
| BR-050 | BL-01, BL-02, BL-03, BL-04 |
| QE-01 | BL-02, BL-09, BL-10, BL-11 |
| QE-02 | BL-01, BL-04, BL-05, BL-06, BL-08, BL-12 |
| QE-03 | BL-01, BL-02, BL-03, BL-04, BL-05, BL-06, BL-07, BL-08, BL-09, BL-10, BL-11, BL-12 |
| QE-04 | BL-01, BL-02, BL-03, BL-04, BL-06, BL-07, BL-08, BL-09, BL-10, BL-11, BL-12 |
| QE-05 | BL-01, BL-04, BL-08, BL-09, BL-10, BL-11 |

## Open questions

- [x] 1. Admin sign-in does not exist, and constitution P5 requires it for every product page and operation. How should it be delivered: as a "BL-00 Admin sign-in" item in this backlog, or as a separate feature/requirements document built first? Also: how is the single Admin account created (seeded from configuration, or a first-run setup), what are the lockout threshold and duration after failed sign-ins, and the session timeout? (affects: BR-015, QE-03, all BL items)
- [x] 2. Which product statuses exist, which status does a new product get, and which transitions are allowed? In particular, can a product go back to Draft? Is status set in the add/edit form, or by a separate action? (affects: BR-011, BR-012, BR-004, BL-01, BL-05, BL-06; decides whether BL-05 exists and what BL-06 depends on)
- [x] 3. How does the stock quantity change after a product is added: in the update form, or through a separate stock adjustment action (add/remove units)? (affects: BR-016, BL-04)
- [x] 4. Which fields make up product information? Which are required, which must be unique (e.g. SKU), and what are the price rules? Are categories or images wanted? (affects: BR-013, BR-014, BR-019, BL-01, BL-02, BL-03, BL-04)
- [x] 5. Does "at least 1" apply only when a product is added (it can later drop to 0), or at all times? (affects: BR-001, BR-003, BR-004, BR-016)
- [x] 6. Should deletion be permanent, or should a deleted product be hidden but recoverable? (affects: BR-007, BL-06)
- [x] 7. Do you want search, filtering or sorting on the product list? (affects: BL-02, scope)
- [x] 8. Can the Admin change the page size, and to which values? (affects: BR-009, BL-07)
- [x] 9. Does "stocks default" mean the add form pre-fills the quantity with 1 but lets you change it (at least 1)? Or is it always exactly 1 when a product is added? (affects: BR-002, BL-01)
- [x] 10. Please confirm the targets and defaults: under 1 minute per operation, up to 10,000 products, list shown within 2 seconds, operations confirmed within 1 second; the list ordered newest first; a page beyond the last showing the last page. (affects: BG-01, QE-01, QE-02, BR-010, BR-020)
- [x] 11. If a save fails (network error, or the session expired mid-edit), should the form keep what the Admin entered so they can retry after signing in again? (affects: PR-01, PR-04, BL-01, BL-04)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|
| 2026-10-16 | How is Admin sign-in (required by P5) delivered? | A separate requirements document, R002 (Admin sign-in), built before BL-01; account provisioning, lockout and session timeout are decided there. |
| 2026-10-16 | Which product statuses exist and which transitions are allowed? | Two statuses, Draft and Active. New products start as Draft; a separate publish action moves Draft → Active; no return to Draft. |
| 2026-10-16 | Which fields make up product information? | Name (required, 1–200), SKU (required, unique, up to 50), price (required, ≥ 0, 2 decimals), description (optional, up to 2,000). No categories or images. |
| 2026-10-16 | How do "at least 1" and "down to 0" fit together, and what does "stocks default" mean? | A new product always starts with stock quantity exactly 1 (the add form has no quantity field); afterwards stock can be any whole number ≥ 0. Also answers question 9. |
| 2026-10-16 | How does the stock quantity change after a product is added? | Only through a separate stock adjustment (add or remove N ≥ 1 units, never below 0); the edit form does not touch stock. New backlog item BL-08 (P1, S); BL-04 narrowed to product information. |
| 2026-10-16 | Do you want search, filtering or sorting on the product list? | All three: search by name or SKU (BL-09), filter by status and stock (BL-10), sort by column (BL-11). |
| 2026-10-16 | Should deletion be permanent or recoverable? | Recoverable: a deleted product is hidden but kept, and can be restored from a deleted products list (new item BL-12). Its SKU stays reserved (assumption). |
| 2026-10-16 | Can the Admin change the page size? | Yes: 10, 20, 50 or 100, default 20; any other value falls back to 20 (BL-07 kept). |
| 2026-10-16 | What happens to entered values when a save fails? | The form stays open in the same tab with the values and an error, so the Admin can retry; nothing is stored elsewhere in the browser. |
| 2026-10-16 | Which currency are prices in? | VND only; price is a whole number of dong, 0 or more (replaces the earlier "2 decimals"); shown like 125.000 ₫. |
| 2026-10-16 | What upper limits apply to price and stock? | Price 0 – 1,000,000,000 ₫; stock quantity 0 – 1,000,000; one stock adjustment moves 1 – 1,000,000 units. |
| 2026-10-16 | Are the targets and defaults acceptable? | Yes, all accepted: under 1 minute per operation; up to 10,000 products; list within 2 s and actions within 1 s (95th percentile); newest first by default; a page past the last shows the last page. |

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-16 | Initial version | — |
