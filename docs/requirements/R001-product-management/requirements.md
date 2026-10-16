# Requirements: Product management

**Requirements:** R001-product-management
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-16
**Requested by:** alannguyen2003 (shop Admin / product owner)
**Input:** [input.md](input.md)

## Context

The shop Admin needs one place to keep the shop's products and how many of each are in stock. Today the system has no product capability at all. The requester asked for five capabilities: add a product with a starting stock quantity, update product information, delete a product (only when it is in Draft status or its stock quantity is 0), view one product, and view a paged product list (20 per page by default). All of this happens on the product management page, which works as a single-page application. Success means the Admin can keep the product list complete and current without help, and cannot delete a product that is live and still in stock.

## Stakeholders and actors

| Actor / stakeholder | Role | Goals | Interaction with the system |
|---------------------|------|-------|-----------------------------|
| Admin | The only user of the system (constitution P1); manages products and stock | Keep the product list complete and the stock quantities correct; avoid deleting products by mistake | Uses |
| Shop owner / product owner (alannguyen2003) | Requester; same person as the Admin | Get a working product management page | Approves |

## Business goals

- **BG-01:** The Admin can add, view, update and delete any product from the product management page without outside help, and each single operation takes under 1 minute. *(Target is an assumption, to be confirmed.)*
- **BG-02:** Every product in the system has a recorded stock quantity: 100% of products have a whole-number quantity of 0 or more at all times.
- **BG-03:** No product that is out of Draft and still in stock gets deleted: 0 such deletions.

## Scope

### In scope

- Adding a product with its product information and a starting stock quantity.
- Viewing the product list, paged, 20 products per page by default.
- Viewing the details of one product.
- Updating product information, including the stock quantity [NEEDS CLARIFICATION: is stock changed in the update form or by a separate stock adjustment? see Open questions].
- Changing a product's status *(assumption: inferred from the Draft delete condition; not one of the five capabilities requested)* [NEEDS CLARIFICATION: which statuses and transitions exist, and is status set in the add/edit form or by a separate action? see Open questions].
- Deleting a product that is in Draft status or has a stock quantity of 0.
- Choosing a different page size for the product list [NEEDS CLARIFICATION: is page size changeable at all?].

### Out of scope

- Admin sign-in / authentication. The constitution (P5) requires it before any product feature can be used, but the input does not ask for it. It is handled outside this backlog unless decided otherwise (see Open questions and Risks).
- A customer-facing storefront, orders, sales and automatic stock deduction: not requested.
- Searching, filtering and sorting the product list: not requested (see Open questions; could be added as a P2 item).
- Product categories and product images: not requested (see Open questions on product fields).
- Stock movement history, audit trail, low-stock alerts: not requested.
- Bulk operations, import/export: not requested.
- Several user accounts or roles: forbidden by constitution P1.

## Glossary

| Term | Definition | Do not call it |
|------|------------|----------------|
| Admin | The single person who signs in and manages products (constitution P1) | user, manager, operator, staff |
| Product | Something the shop sells and keeps stock of, with its product information, stock quantity and product status | item, article, goods, SKU (as a synonym for the whole product) |
| Product information | The descriptive fields of a product [NEEDS CLARIFICATION: exact field list], excluding stock quantity and product status | product data, product details (for the fields) |
| Stock quantity | Whole number of units of a product the shop has, 0 or more | stock level, inventory count, qty, amount |
| Starting stock quantity | The stock quantity entered when a product is added; at least 1 | default stock, initial inventory |
| Out of stock | A product whose stock quantity is 0 | sold out, empty |
| Product status | Where a product is in its lifecycle; Draft is the only state named by the requester [NEEDS CLARIFICATION: other states] | state, publish state, visibility |
| Draft | A product status meaning the product is not yet in use [NEEDS CLARIFICATION: exact meaning] | unpublished, pending, inactive |
| Product list | The paged list of all products shown on the product management page | catalogue, inventory list, grid |
| Page size | Number of products shown on one page of the product list; 20 by default | page length, limit |
| Product details | The view showing all product information, the stock quantity and the product status of one product | product page, product info view |
| Product management page | The single-page interface where the Admin performs all product operations | dashboard, admin panel, back office |
| Delete (a product) | Take a product out of the system so it no longer appears in the product list or product details [NEEDS CLARIFICATION: permanent or recoverable] | remove, archive, discard |

## Business processes

### PR-01 — Add a product

1. The Admin opens the add form from the product management page.
2. The system shows the form with the starting stock quantity pre-filled with 1.
3. The Admin enters the product information and, optionally, changes the starting stock quantity.
4. The Admin saves.
5. The system checks the input, saves the product with status [NEEDS CLARIFICATION: Draft, or chosen by the Admin?] and shows a success confirmation with the saved values.

**Exceptions:**
- Starting stock quantity is less than 1, not a whole number, or missing → the system rejects the save, saves nothing and shows the reason next to the field.
- A required field is missing or invalid → the system rejects the save, saves nothing and shows the reason next to each invalid field.
- [NEEDS CLARIFICATION: a product with the same identifying value already exists (e.g. same SKU or name) → rejected?]
- The Admin cancels → nothing is saved; the system returns to the product management page.
- The Admin clicks Save twice → at most one product is created (BR-028).
- The save fails (network error or expired session) → the Admin is told the product was not saved [NEEDS CLARIFICATION: is the entered data kept? see Open question 11].

### PR-02 — View the product list

1. The Admin opens the product management page.
2. The system shows the first page of the product list, 20 products per page, newest first, with the total number of products and pages.
3. The Admin moves to another page, or chooses another page size [NEEDS CLARIFICATION].
4. The system shows the requested page.

**Exceptions:**
- No products exist → the system shows an empty-list message and a way to add a product.
- The requested page is beyond the last page (e.g. after a deletion) → the system shows the last existing page.

### PR-03 — View product details

1. The Admin selects a product in the product list.
2. The system shows the product details: all product information, stock quantity and product status.
3. The Admin returns to the product list at the same page.

**Exceptions:**
- The product no longer exists → the system shows a "product not found" message and a way back to the product list.

### PR-04 — Update a product

1. From the product details, the Admin opens the edit form.
2. The system shows the form filled with the current product information and stock quantity.
3. The Admin changes values and saves.
4. The system checks the input, saves the changes and shows the updated product details.

**Exceptions:**
- Invalid value (e.g. negative stock quantity, missing required field) → the system rejects the save, keeps the stored product unchanged and shows the reason next to each invalid field.
- The product was deleted meanwhile → the system shows "product not found"; nothing is saved.
- The Admin cancels → nothing is saved.
- The save fails (network error or expired session) → the Admin is told the change was not saved; the stored product is unchanged [NEEDS CLARIFICATION: see Open question 11].

### PR-05 — Change product status

1. From the product details, the Admin chooses a new product status.
2. The system checks the transition is allowed, saves it and shows the updated product details.

**Exceptions:**
- Transition not allowed [NEEDS CLARIFICATION: which ones] → the system refuses and explains why.

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

- **BR-001:** A product is added with a starting stock quantity of at least 1. *(Source: input "every product will have the quantity at least 1"; interpreted as applying at creation, see Open questions)*
- **BR-002:** The add form pre-fills the starting stock quantity with 1. *(Source: assumption, interpretation of input "with stocks default")*
- **BR-003:** A stock quantity is always a whole number of 0 or more; a negative or fractional quantity is rejected. *(Source: assumption; 0 is implied by input "stock quantity is down to 0")*
- **BR-004:** A product may be deleted only if its product status is Draft or its stock quantity is 0. *(Source: input)*
- **BR-005:** When a deletion is refused under BR-004, the system tells the Admin the reason and the product stays unchanged. *(Source: assumption)*
- **BR-006:** A deletion takes effect only after the Admin explicitly confirms it. *(Source: assumption)*
- **BR-007:** A deleted product no longer appears in the product list and its product details can no longer be opened. [NEEDS CLARIFICATION: permanent deletion or recoverable?] *(Source: input "delete product"; permanence open)*
- **BR-008:** The product list shows 20 products per page by default. *(Source: input)*
- **BR-009:** The Admin can change the page size to [NEEDS CLARIFICATION: is page size changeable, and to which values, e.g. 10/20/50/100?]. *(Source: interpretation of input "default is 20")*
- **BR-010:** The product list is ordered by date added, newest first. *(Source: assumption)*
- **BR-011:** Every product has exactly one product status; a newly added product gets status [NEEDS CLARIFICATION: Draft, or chosen by the Admin?]. *(Source: input mentions "product state is draft"; rest assumption)*
- **BR-012:** Product status changes are limited to [NEEDS CLARIFICATION: list of statuses and allowed transitions; in particular, can a product go back to Draft?]. *(Source: open)*
- **BR-013:** Every product has a name, which is required and is 1 to 200 characters long. *(Source: assumption; length is a default)*
- **BR-014:** Product information consists of [NEEDS CLARIFICATION: field list, which are required, which must be unique (e.g. SKU), price rules]. *(Source: open)*
- **BR-015:** Only the signed-in Admin can view, add, update, change status of or delete products. *(Source: constitution P1, P5)*
- **BR-016:** The Admin changes a product's stock quantity through [NEEDS CLARIFICATION: the update form, or a separate stock adjustment action?]; after creation the quantity may be set to any value allowed by BR-003, including 0. *(Source: open; 0 implied by input delete rule)*
- **BR-017:** When an add or update is rejected, nothing is saved and the Admin sees which field is invalid and why. *(Source: assumption; constitution P6)*
- **BR-018:** Product details show all product information, the stock quantity and the product status. *(Source: input "viewing product information")*
- **BR-019:** Each row of the product list shows at least the product name, stock quantity and product status [NEEDS CLARIFICATION: which other fields]. *(Source: assumption)*
- **BR-020:** When the requested page is beyond the last page, the product list shows the last existing page. *(Source: assumption)*
- **BR-021:** The product list shows the total number of products and of pages. *(Source: assumption)*
- **BR-022:** When no products exist, the product list shows an empty-list message and a way to add a product. *(Source: assumption)*
- **BR-023:** Returning from product details shows the product list at the page the Admin came from. *(Source: assumption)*
- **BR-024:** Delete is offered from both the product list and the product details. *(Source: assumption)*
- **BR-025:** The delete condition of BR-004 is checked against the stored product at the moment of deletion, not against what the screen showed. *(Source: assumption)*
- **BR-026:** When the same product is saved from two places, the last save wins. *(Source: assumption)*
- **BR-027:** After the page size changes, the product list returns to page 1. *(Source: assumption)*
- **BR-028:** One Save action creates at most one product, even if the Admin submits it twice. *(Source: assumption)*

## Quality expectations

- **QE-01:** 95% of product list pages (20 products, 10,000 stored) are shown within 2 seconds of the Admin's action, measured in the browser. *(Volume and time are assumptions.)*
- **QE-02:** 95% of adds, updates, status changes and deletions (10,000 products stored) are confirmed to the Admin within 1 second of the action, measured in the browser. *(Assumption.)*
- **QE-03:** 100% of product operations and product views are refused without a signed-in Admin session. *(Constitution P5.)*
- **QE-04:** Moving between the product list, product details, the add form and the edit form causes 0 full page reloads, and the browser Back button returns to the previous view, including the product list page number. *(Input "SPA"; Back-button behavior is an assumption.)*
- **QE-05:** 100% of invalid inputs are rejected at the boundary with a field-level message, and 0 invalid values are stored. *(Constitution P6.)*

## Constraints

- The product management page must work as a single-page application. *(Source: input "I want it to go the way SPA of frontend"; matches the React + Vite frontend in docs/architecture.md.)*
- Exactly one Admin account; no registration or roles. *(Source: constitution P1.)*
- Every product operation and page requires a signed-in Admin; sign-in must be protected against brute force. *(Source: constitution P5.)*
- All input validated at the boundary. *(Source: constitution P6.)*

## Assumptions

- The Admin is the only actor; there is no customer-facing part, so a product status only matters to the Admin and to the delete rule.
- "At least 1" applies when a product is added; the stock quantity can later go down to 0 (otherwise the "down to 0" delete condition could never apply). Pending confirmation.
- Stock quantity counts whole units; no units of measure, fractions or multiple warehouses.
- The product list is ordered newest first by default (BR-010).
- Deletion always asks for confirmation (BR-006).
- Up to 10,000 products is the expected volume for QE-01 and QE-02.
- The Admin may have two browser tabs open; the last save wins, and an operation on a product deleted meanwhile reports "product not found".
- No history of stock changes is kept in this initiative.
- The BG-01 target (under 1 minute per operation) is a starting point to be confirmed by the requester.
- A chosen page size is not remembered between sessions (BL-07).

## Risks and dependencies

- **Admin authentication does not exist yet.** Constitution P5 requires a signed-in Admin for every product page and endpoint, so no backlog item here can be shipped until sign-in exists. → Decide in Open question 1. Until then, every spec generated from this backlog depends on that capability.
- **Product status lifecycle is undefined.** The delete rule (BR-004) depends on it; if a product can be moved back to Draft at will, any product can be deleted and BG-03 means little. → Open question 2.
- **Conflict in the stock rule:** "every product will have the quantity at least 1" against "stock quantity is down to 0". → Open question 5; the assumption above applies until answered.
- **Permanent deletion loses data for good**, and later features (orders, sales) may need deleted products. → Confirmation step (BR-006); Open question 6.
- **Paging without search** may make products hard to find once there are many. → Open question 7.

## Backlog

### Summary

| ID | Feature | Priority | Size | Depends on | Covers | Spec |
|----|---------|----------|------|------------|--------|------|
| BL-01 | Add a product | P1 | M | Admin sign-in (Q1) | BG-01, BG-02, BR-001, BR-002, BR-003, BR-011, BR-013, BR-014, BR-015, BR-017, BR-028, QE-02, QE-03, QE-04, QE-05 | — |
| BL-02 | Paged product list | P1 | M | BL-01 | BG-01, BR-008, BR-010, BR-015, BR-019, BR-020, BR-021, BR-022, QE-01, QE-03, QE-04 | — |
| BL-03 | View product details | P1 | S | BL-02 | BG-01, BR-014, BR-015, BR-018, BR-023, QE-03, QE-04 | — |
| BL-04 | Update product information and stock quantity | P1 | M | BL-03 | BG-01, BG-02, BR-003, BR-013, BR-014, BR-015, BR-016, BR-017, BR-026, QE-02, QE-03, QE-04, QE-05 | — |
| BL-05 | Change product status | P1 | S | BL-03 | BG-03, BR-011, BR-012, BR-015, QE-02, QE-03 | — |
| BL-06 | Delete a product | P1 | M | BL-03, BL-04, BL-05 | BG-01, BG-03, BR-004, BR-005, BR-006, BR-007, BR-015, BR-024, BR-025, QE-02, QE-03, QE-04 | — |
| BL-07 | Choose product list page size | P2 | S | BL-02 | BR-009, BR-015, BR-027, QE-03, QE-04 | — |

Size: `S` ≤ 1 day, `M` 1–3 days. The `Spec` column holds the reserved `NNN-slug` once `/sdd-ba-spec` creates the spec; `—` until then.

**Every item depends on Admin sign-in** (constitution P5), which is not in this backlog yet; BL-01 lists it explicitly and the others inherit it through BL-01. How sign-in is delivered is Open question 1. P1 items BL-01 to BL-06 form the MVP. BL-05 exists only if Open question 2 keeps status change as a separate action; BL-07 exists only if Open question 8 makes page size changeable.

### BL-01 — Add a product

**Value:** The Admin can record a new product with its starting stock quantity, so the product list starts to reflect what the shop sells.

**Actors:** Admin

**Spec brief:** The signed-in Admin opens the add form on the product management page, enters the product information and a starting stock quantity (pre-filled with 1, must be a whole number of at least 1), and saves. The system validates every field at the boundary, stores the product with its initial product status and shows a success confirmation with the saved values. Invalid input is rejected field by field and nothing is saved. Product fields, uniqueness and initial status are pending clarification (BR-011, BR-014).

**Key scenarios:**
- Admin enters a valid name and other required information, keeps quantity 1, saves → product stored with stock quantity 1 and its initial status; success confirmation shown with the saved values.
- Admin clicks Save twice quickly → only one product is created.
- Admin sets the starting stock quantity to 0, a negative or a fractional number → rejected with a message on the quantity field; nothing saved.
- Admin leaves the name empty → rejected with a message on the name field.
- Request without a signed-in Admin session → refused.

**Not included:** Listing products (BL-02), product details (BL-03), changing status after creation (BL-05).

### BL-02 — Paged product list

**Value:** The Admin sees all products and their stock at a glance, page by page.

**Actors:** Admin

**Spec brief:** The signed-in Admin opens the product management page and sees the product list, 20 products per page, newest first, each row showing at least name, stock quantity and product status, with page navigation and the total count. Navigation stays within the single-page application and the Back button restores the previous page number. With no products, an empty-list message offers a way to add one. A page number beyond the last page shows the last page.

**Key scenarios:**
- 45 products exist → page 1 shows the 20 newest, page 3 shows the last 5; total shows 45.
- No products → empty-list message with a link to the add form.
- Admin requests page 10 when only 3 pages exist → page 3 shown.
- Request without a signed-in Admin session → refused.

**Not included:** Changing the page size (BL-07); search, filter and sort (out of scope, pending Open question 7).

### BL-03 — View product details

**Value:** The Admin can check every piece of information about one product, and reach its edit, status and delete actions.

**Actors:** Admin

**Spec brief:** The signed-in Admin selects a product in the product list and sees its product details: all product information, stock quantity and product status, without a full page reload. A way back returns to the same page of the product list. If the product no longer exists, a "product not found" message with a way back is shown.

**Key scenarios:**
- Admin selects a product on page 2 → details shown; going back shows page 2.
- Admin opens the details of a product deleted meanwhile → "product not found".
- Request without a signed-in Admin session → refused.

**Not included:** Editing (BL-04), changing status (BL-05), deleting (BL-06).

### BL-04 — Update product information and stock quantity

**Value:** The Admin keeps product information and stock quantity correct over time.

**Actors:** Admin

**Spec brief:** From the product details, the signed-in Admin opens the edit form pre-filled with the current product information and stock quantity, changes values and saves. The system validates every field; the stock quantity must be a whole number of 0 or more. On success the updated product details are shown; on failure nothing is saved and each invalid field is explained. Whether stock is edited here or through a separate adjustment action is pending clarification (BR-016); this brief assumes the edit form.

**Key scenarios:**
- Admin changes the name and sets quantity from 5 to 0 → saved; details show the new name and quantity 0.
- Admin enters quantity -1 → rejected on the quantity field; stored product unchanged.
- Admin saves a product deleted meanwhile → "product not found"; nothing saved.
- Request without a signed-in Admin session → refused.

**Not included:** Changing product status (BL-05); stock history (out of scope).

### BL-05 — Change product status

**Value:** The Admin can move a product through its lifecycle, which also determines whether it can be deleted.

**Actors:** Admin

**Spec brief:** *(Assumption: inferred from the Draft delete condition; may merge into BL-01/BL-04 if Open question 2 decides status is a form field.)* From the product details, the signed-in Admin changes the product status. The system allows only the permitted transitions (pending clarification, BR-012) and refuses others with a reason. The new status is shown in the product details and the product list. The statuses, the initial status and whether a product can return to Draft are pending clarification.

**Key scenarios:**
- Admin moves a Draft product to the next allowed status → saved and shown.
- Admin tries a transition that is not allowed → refused with a reason; status unchanged.
- Request without a signed-in Admin session → refused.

**Not included:** The initial status at creation (BL-01); deletion (BL-06).

### BL-06 — Delete a product

**Value:** The Admin can delete Draft products and out-of-stock products, while products that are out of Draft and still in stock are protected.

**Actors:** Admin

**Spec brief:** From the product details or the product list, the signed-in Admin chooses to delete a product. The system allows it only if the product status is Draft or the stock quantity is 0, checked against stored data at the moment of deletion; otherwise it refuses and explains why. An allowed deletion requires explicit confirmation; afterwards the product no longer appears in the product list or details. Permanent versus recoverable deletion is pending clarification (BR-007).

**Key scenarios:**
- Draft product with quantity 12 → confirmation → deleted; no longer in the list.
- Non-Draft product with quantity 0 → confirmation → deleted.
- Non-Draft product with quantity 3 → refused with the reason; product unchanged.
- Admin cancels the confirmation → nothing deleted.
- Request without a signed-in Admin session → refused.

**Not included:** Restoring deleted products (pending Open question 6); bulk deletion (out of scope).

### BL-07 — Choose product list page size

**Value:** The Admin can see more or fewer products per page than the default 20.

**Actors:** Admin

**Spec brief:** Created only if Open question 8 makes page size changeable. On the product list, the signed-in Admin picks a page size from a fixed set of values (pending clarification, BR-009); the list returns to page 1 with the new size (BR-027). The default stays 20. A page size outside the allowed set → the list uses 20 (no error).

**Key scenarios:**
- Admin chooses [an allowed value] → page 1 shows up to that many products.
- A page size outside the allowed set is requested → the list uses 20.

**Not included:** Remembering the choice between sessions (assumption: not remembered).

## Traceability

| Goal / rule / quality | Backlog items |
|-----------------------|---------------|
| BG-01 | BL-01, BL-02, BL-03, BL-04, BL-06 |
| BG-02 | BL-01, BL-04 |
| BG-03 | BL-05, BL-06 |
| BR-001 | BL-01 |
| BR-002 | BL-01 |
| BR-003 | BL-01, BL-04 |
| BR-004 | BL-06 |
| BR-005 | BL-06 |
| BR-006 | BL-06 |
| BR-007 | BL-06 |
| BR-008 | BL-02 |
| BR-009 | BL-07 |
| BR-010 | BL-02 |
| BR-011 | BL-01, BL-05 |
| BR-012 | BL-05 |
| BR-013 | BL-01, BL-04 |
| BR-014 | BL-01, BL-03, BL-04 |
| BR-015 | BL-01, BL-02, BL-03, BL-04, BL-05, BL-06, BL-07 |
| BR-016 | BL-04 |
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
| QE-01 | BL-02 |
| QE-02 | BL-01, BL-04, BL-05, BL-06 |
| QE-03 | BL-01, BL-02, BL-03, BL-04, BL-05, BL-06, BL-07 |
| QE-04 | BL-01, BL-02, BL-03, BL-04, BL-06, BL-07 |
| QE-05 | BL-01, BL-04 |

## Open questions

- [ ] 1. Admin sign-in does not exist, and constitution P5 requires it for every product page and operation. How should it be delivered: as a "BL-00 Admin sign-in" item in this backlog, or as a separate feature/requirements document built first? Also: how is the single Admin account created (seeded from configuration, or a first-run setup), what are the lockout threshold and duration after failed sign-ins, and the session timeout? (affects: BR-015, QE-03, all BL items)
- [ ] 2. Which product statuses exist, which status does a new product get, and which transitions are allowed? In particular, can a product go back to Draft? Is status set in the add/edit form, or by a separate action? (affects: BR-011, BR-012, BR-004, BL-01, BL-05, BL-06; decides whether BL-05 exists and what BL-06 depends on)
- [ ] 3. How does the stock quantity change after a product is added: in the update form, or through a separate stock adjustment action (add/remove units)? (affects: BR-016, BL-04)
- [ ] 4. Which fields make up product information? Which are required, which must be unique (e.g. SKU), and what are the price rules? Are categories or images wanted? (affects: BR-013, BR-014, BR-019, BL-01, BL-02, BL-03, BL-04)
- [ ] 5. Does "at least 1" apply only when a product is added (it can later drop to 0), or at all times? (affects: BR-001, BR-003, BR-004, BR-016)
- [ ] 6. Should deletion be permanent, or should a deleted product be hidden but recoverable? (affects: BR-007, BL-06)
- [ ] 7. Do you want search, filtering or sorting on the product list? (affects: BL-02, scope)
- [ ] 8. Can the Admin change the page size, and to which values? (affects: BR-009, BL-07)
- [ ] 9. Does "stocks default" mean the add form pre-fills the quantity with 1 but lets you change it (at least 1)? Or is it always exactly 1 when a product is added? (affects: BR-002, BL-01)
- [ ] 10. Please confirm the targets and defaults: under 1 minute per operation, up to 10,000 products, list shown within 2 seconds, operations confirmed within 1 second; the list ordered newest first; a page beyond the last showing the last page. (affects: BG-01, QE-01, QE-02, BR-010, BR-020)
- [ ] 11. If a save fails (network error, or the session expired mid-edit), should the form keep what the Admin entered so they can retry after signing in again? (affects: PR-01, PR-04, BL-01, BL-04)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-16 | Initial version | — |
