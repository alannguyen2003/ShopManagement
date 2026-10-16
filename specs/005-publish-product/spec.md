# Spec: Publish a product

**Feature:** 005-publish-product
**Branch:** feat/005-publish-product
**Source:** R001/BL-05
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-16

## Context

Every newly added product starts in Draft status (001-add-product, BL-01). The Admin needs a way to mark a product as ready by moving it to Active. Publishing also matters for deletion: under the delete rule (007-delete-product, BL-06), an Active product that is still in stock cannot be deleted. That rule is what keeps BG-03 at zero such deletions. This feature adds a separate publish action to the product details of a Draft product. The move is one-way: Draft → Active is the only allowed status change, and an Active product never goes back to Draft (BR-012).

## User stories

### US1 — Publish a Draft product (Priority: P1)

As the Admin, I want to publish a Draft product from its product details, so that it becomes Active and is protected from deletion while it is in stock.

**Why P1:** Without this action no product can ever leave Draft, so Active never occurs and the delete protection of BG-03 never applies.

**Independent test:** Add a product (it is Draft), open its product details, publish it, and check that the product details and the product list both show Active while every other value stays the same.

**Acceptance scenarios:**
1. **Given** the Draft product "Blue Mug" (SKU MUG-01, price 125.000 ₫, stock quantity 1) and a signed-in Admin, **When** the Admin opens its product details and publishes it, **Then** the product details show product status Active, and the name, SKU, price, description and stock quantity are the same as before.
2. **Given** "Blue Mug" has just been published, **When** the Admin looks at its product details, **Then** no publish action is offered.
3. **Given** "Blue Mug" has just been published, **When** the Admin returns to the product list, **Then** the "Blue Mug" row shows product status Active and the row is in the same place in the list as before (its date added did not change).
4. **Given** a Draft product with stock quantity 0, **When** the Admin publishes it, **Then** its product status becomes Active (being out of stock does not block publishing).
5. **Given** the product details of an Active product, **When** the Admin looks at the available actions, **Then** no publish action is offered and no action to return the product to Draft is offered.
6. **Given** no signed-in Admin session, **When** a publish request is made for a Draft product, **Then** the request is refused and the product stays Draft.

### US2 — Refuse publishes that are not allowed (Priority: P1)

As the Admin, I want the system to refuse a publish that no longer makes sense and tell me why, so that a product's status is always correct even when I work in two tabs.

**Why P1:** BR-012 (an Active product never returns to Draft, and only Draft → Active is allowed) must hold from the first release. Otherwise BG-03 cannot be relied on.

**Independent test:** Open the same Draft product in two tabs, publish it in one tab, try again in the other, and check that the second attempt is refused with a reason and the product is still Active.

**Acceptance scenarios:**
1. **Given** the Draft product "Blue Mug" is open in tabs A and B and the Admin publishes it in tab A, **When** the Admin publishes it in tab B, **Then** the system refuses, tab B shows the message that only a Draft product can be published, and tab B then shows product status Active.
2. **Given** a publish request for a product that does not exist, **When** the system handles it, **Then** it shows "product not found" with a way back to the product list, and no product changes.
3. **Given** the Active product "Blue Mug", **When** any request asks to set its product status to Draft, **Then** the request is refused and the product stays Active.
4. **Given** the Draft product "Blue Mug", **When** two publish requests for it arrive at the same moment, **Then** exactly one succeeds, the product is Active, and the other is refused as in scenario 1.
5. **Given** the product details of the Draft product "Blue Mug", **When** the Admin publishes it and the request fails because of a network error, **Then** the Admin is told the product was not published, the product details still show Draft, and publishing again succeeds once the connection is back.
6. **Given** the Admin's session has expired while the product details of a Draft product are open, **When** the Admin publishes it, **Then** the request is refused, the Admin is told the product was not published, and the product stays Draft.

## Edge cases

- The product was published from another tab after the product details were shown: the publish is refused with the reason, and the view updates to show Active (FR-005).
- Two publish requests for the same product arrive at the same moment: the product is published once and the other request is refused (FR-009).
- The Admin clicks publish twice quickly: the action cannot be started again while the first request is in progress, so the Admin does not see a "refused" message caused by their own double click (FR-012).
- The product does not exist, or the product reference is malformed: "product not found" for an unknown product (FR-006); a malformed reference is rejected at the boundary and nothing changes (NFR-001).
- A product that was deleted after its product details were shown: it is treated as not existing, so "product not found" (FR-006). The rule that a deleted product cannot be published (BR-048) is delivered by 007-delete-product (BL-06).
- A Draft product with stock quantity 0, or one with an empty description: it can be published, because the only condition is Draft status (FR-002).
- Network error while publishing: the Admin is told the product was not published and can retry (FR-010).
- The session expires before publishing: the request is refused, the product is unchanged, and the Admin can retry after signing in again (FR-008, FR-010). Sign-in itself is delivered by R002.
- A request that tries to set any status other than Active, or to go from Active back to Draft: refused, and the status is unchanged (FR-007).

## Functional requirements

- **FR-001:** The product details of a Draft product MUST offer a publish action. The product details of an Active product MUST NOT offer it (BR-012, PR-05).
- **FR-002:** When the Admin publishes a product, the system MUST check the product status of the stored product at that moment, not what the screen showed. If the stored product is Draft, the system MUST set its product status to Active (BR-011, BR-012, PR-05). There is no other condition: stock quantity and product information do not affect publishing.
- **FR-003:** Publishing MUST change only the product status. The product information (name, SKU, price, description), stock quantity and date added MUST stay the same.
- **FR-004:** After a successful publish, the system MUST show the updated product details with product status Active, without a full page reload (constraint: single-page application). The product list MUST show the product as Active the next time it is displayed.
- **FR-005:** When the stored product is already Active, the system MUST refuse the publish, tell the Admin that only a Draft product can be published, and keep the product Active. The view MUST then show the current status, Active (BR-012, PR-05).
- **FR-006:** When the product does not exist, the system MUST show "product not found" with a way back to the product list, and MUST NOT change any product (PR-05). Once 007-delete-product (BL-06) exists, a deleted product counts as not existing for this purpose (BR-048).
- **FR-007:** The publish action MUST NOT accept a target status: it only ever moves a product from Draft to Active. The system MUST NOT offer any action that sets a product status to Draft, or to any value other than Draft or Active. Any such request MUST be refused, and the status stays unchanged (BR-011, BR-012).
- **FR-008:** Only the signed-in Admin MUST be able to publish a product. A publish request without a signed-in Admin session MUST be refused, and the product status stays unchanged (BR-015).
- **FR-009:** When several publish requests for the same Draft product arrive at the same moment, the system MUST publish the product exactly once. Every other request MUST be refused as in FR-005.
- **FR-010:** When a publish fails because of a network error or an expired session, the system MUST tell the Admin that the product was not published. The product details MUST keep showing the last known status, and the Admin MUST be able to retry, after signing in again if needed.
- **FR-011:** Before publishing, the system MUST [NEEDS CLARIFICATION: Should publishing ask the Admin to confirm first, given that it cannot be undone and that, once Active and in stock, the product can no longer be deleted? Options: A) ask for confirmation, and the confirmation says the change is final; B) publish at once with no confirmation.]
- **FR-012:** While a publish request is in progress, the publish action MUST NOT be able to start again from the same product details.

## Non-functional requirements

- **NFR-001:** 100% of publish requests with a missing or malformed product reference are rejected at the boundary before any business logic runs, and 0 products change as a result (constitution P6).
- **NFR-002:** 95% of publishes, with 10,000 products stored, are confirmed to the Admin within 1 second of the action, measured in the browser (QE-02).
- **NFR-003:** 100% of publish requests are refused without a signed-in Admin session (QE-03, constitution P5).
- **NFR-004:** 0 refusal or error messages shown to the Admin or returned to the client contain stack traces or internal details (constitution P7).

## Key entities (if any)

- **Product:** something the shop sells and keeps stock of. It has product information (name, SKU, price, description), a stock quantity, a product status and a date added. This feature changes only its product status.
- **Product status:** where a product is in its lifecycle: Draft or Active, exactly one at a time (BR-011). A new product is Draft. Publishing moves it to Active, and it never returns to Draft (BR-012).

## Success criteria

- **SC-001:** In acceptance tests, 100% of publishes of a Draft product leave it Active. Its product information, stock quantity and date added are unchanged.
- **SC-002:** 0 products ever move from Active to Draft, whatever sequence of actions the Admin takes (BR-012).
- **SC-003:** In acceptance tests, 0 publishes succeed for a product that is already Active, does not exist, or is requested without a signed-in Admin session.
- **SC-004:** Every published product carries status Active. This is the status the delete rule of 007-delete-product (BL-06) uses to protect in-stock products, so that 0 Active in-stock products are deleted (BG-03).
- **SC-005:** 95% of publishes are confirmed within 1 second, with 10,000 products stored (NFR-002).

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BG-03 | Context; SC-004; FR-001, FR-002, FR-007 (an Active product stays Active, so the delete rule in 007/BL-06 can protect it) |
| BR-011 | FR-002, FR-007; Key entities (Product status); US2 scenario 3 |
| BR-012 | FR-001, FR-002, FR-005, FR-007; US1 scenarios 2 and 5; US2 scenarios 1 and 3; SC-002 |
| BR-015 | FR-008; US1 scenario 6; US2 scenario 6 |
| QE-02 | NFR-002; SC-005 |
| QE-03 | NFR-003; FR-008 |
| PR-05 exceptions (already Active, not found) | FR-005, FR-006; Edge cases; US2 scenarios 1 and 2 |

## Assumptions

- Admin sign-in, sessions and session expiry come from R002 (Admin sign-in), which is built before this feature. This spec only requires that a publish without a signed-in Admin session is refused.
- Publishing is offered only from the product details, as PR-05 describes, not from the product list.
- Being Draft is the only condition for publishing. A Draft product with stock quantity 0 or an empty description can be published.
- No publish date and no history of status changes is recorded or shown, because the requirements do not ask for them.
- After a refused publish of an already Active product, the view refreshes to show the stored status, so the Admin sees why it was refused.
- Failure handling (FR-010) follows the spirit of BR-049, which covers forms. There are no entered values to keep, so the Admin simply retries the action.
- Blocking a repeated publish while one is in progress (FR-012) is a sensible default. It avoids a confusing "refused" message caused by a double click.
- The message wording "only a Draft product can be published" follows PR-05. The exact text is settled in the plan.

## Out of scope

- The product status a product gets when it is added, which is always Draft (001-add-product, BL-01).
- Showing the product status column in the product list (002-product-list, BL-02) and in the product details (003-product-details, BL-03). This feature relies on both.
- Editing product information (004-update-product, BL-04).
- Adjusting the stock quantity (006-adjust-stock, BL-08).
- Deleting a product and the delete rule that protects Active in-stock products, including refusing to publish a deleted product (007-delete-product, BL-06; BR-004, BR-048).
- Filtering the product list by product status (010-product-filters, BL-10).
- Restoring deleted products (011-restore-products, BL-12).
- Any way back from Active to Draft ("unpublish"), which BR-012 forbids.
- Publishing several products at once (bulk operations are out of scope in R001).
- Admin sign-in (R002).

## Open questions

- [ ] Should publishing ask the Admin to confirm first, given that it cannot be undone and that an Active product in stock can no longer be deleted? A) ask for confirmation, and the confirmation says the change is final; B) publish at once. Recommended: A, because the action cannot be undone and changes what the Admin may later delete. (affects: FR-011, US1)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-16 | Initial version | — |
