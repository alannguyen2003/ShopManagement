# Spec: Update product information

**Feature:** 004-update-product
**Branch:** feat/004-update-product
**Source:** R001/BL-04
**Status:** Draft
**Revision:** 1
**Created:** 2026-10-16

## Context

Product information changes over time. A name has a typo, the shop renames a SKU or changes a price. Without an edit capability the Admin cannot keep the product list correct, which fails business goal BG-01. This feature lets the signed-in Admin open an edit form from the product details, change the product information (name, SKU, price, description) and save it. The same validation rules apply as when a product is added. The edit form never changes the stock quantity or the product status: those change only through a stock adjustment (006-adjust-stock / BL-08) and publishing (005-publish-product / BL-05). The feature builds on the product details view (003-product-details / BL-03). It also depends on Admin sign-in, which is delivered separately by R002.

## User stories

### US1 — Edit and save product information (Priority: P1)

As the Admin, I want to change a product's name, SKU, price and description from its product details so that the product information stays correct.

**Why P1:** This is the core of the feature. Without it, wrong product information can only be fixed by deleting and re-adding the product, and an Active product that is in stock cannot be deleted at all.

**Independent test:** Add a product, open its product details, open the edit form, change the name and price, and save. The updated product details show the new values. The stock quantity and product status are unchanged, and the product list shows the new values.

**Acceptance scenarios:**
1. **Given** a product "Blue Mug", SKU "MUG-01", price 125.000 ₫, description "Ceramic, 350 ml", stock quantity 12, status Active (seeded test data until 005 and 006 exist), **When** the Admin opens the edit form from its product details, **Then** the form shows name "Blue Mug", SKU "MUG-01", price 125000 and description "Ceramic, 350 ml", and it has no stock quantity field and no product status field.
2. **Given** the edit form of "Blue Mug" is open, **When** the Admin changes the name to "Blue Mug Large" and the price to 150000 and saves, **Then** the product details show name "Blue Mug Large" and price "150.000 ₫", the stock quantity is still 12, the status is still Active, and a confirmation says the changes were saved.
3. **Given** the edit form of "Blue Mug" is open with changed values, **When** the Admin cancels, **Then** nothing is saved and the product details show the stored values ("Blue Mug", 125.000 ₫).
4. **Given** a product with description "Ceramic, 350 ml", **When** the Admin clears the description and saves, **Then** the product is saved with no description.
5. **Given** the edit form is open, **When** the Admin saves without changing any value, **Then** the save succeeds and the product details show the same values.
6. **Given** product "Blue Mug" with SKU "MUG-01", **When** the Admin changes its SKU to "mug-01" and saves, **Then** the save succeeds, because the only product with that SKU is the product itself (BR-030).
7. **Given** the Admin moved from the product details to the edit form, **When** the Admin uses the browser Back button, **Then** the product details are shown again without a full page reload.
8. **Given** no signed-in Admin session, **When** someone tries to open the edit form or save product information, **Then** the request is refused and the product is unchanged.

### US2 — Reject invalid product information (Priority: P1)

As the Admin, I want invalid changes rejected field by field so that I never store wrong product information and I know exactly what to fix.

**Why P1:** Constitution P6 requires validation at the boundary. Without it, US1 could store invalid data, which breaks the product list and the SKU rules.

**Independent test:** Open the edit form of an existing product, enter invalid values one at a time (and several together), and save. Each save is rejected, the reason is shown next to each invalid field, and the stored product is unchanged.

**Acceptance scenarios:**
1. **Given** the edit form of "Blue Mug", **When** the Admin clears the name and saves, **Then** the save is rejected, a message next to the name field says a name is required, and the stored product is unchanged.
2. **Given** the edit form, **When** the Admin enters a name of 201 characters, **Then** the save is rejected with a message on the name field; a name of 200 characters is accepted.
3. **Given** another product "Red Plate" with SKU "PL-02", **When** the Admin changes the SKU of "Blue Mug" to " pl-02 " and saves, **Then** the save is rejected with a message on the SKU field saying the SKU is already used, and the stored product is unchanged.
4. **Given** the edit form, **When** the Admin clears the SKU, or enters a SKU of 51 characters, and saves, **Then** the save is rejected with a message on the SKU field.
5. **Given** the edit form, **When** the Admin enters price -1, 1500.5, 1000000001 or a non-numeric value and saves, **Then** the save is rejected with a message on the price field; prices 0 and 1000000000 are accepted.
6. **Given** the edit form, **When** the Admin clears the price and saves, **Then** the save is rejected with a message on the price field saying a price is required.
7. **Given** the edit form, **When** the Admin enters a description of 2,001 characters, **Then** the save is rejected with a message on the description field; 2,000 characters are accepted.
8. **Given** the edit form, **When** the Admin empties the name and enters price -1 and saves, **Then** both fields show their reason at the same time and nothing is saved.
9. **Given** a product with stock quantity 12 (seeded test data until 006 exists) and status Draft, **When** a save request also carries a stock quantity of 50 and status Active, **Then** the save is rejected with a message naming the unexpected fields, nothing is saved, and the stock quantity stays 12 and the status stays Draft.

### US3 — Recover from a failed or outdated save (Priority: P2)

As the Admin, I want a failed save to keep what I typed, and a save on a product that no longer exists to tell me so, so that I do not lose work or change the wrong data.

**Why P2:** US1 and US2 already deliver a usable edit. This story covers network failures, expired sessions and changes made in another tab.

**Independent test:** Open the edit form and change values. Simulate a network failure on save and check that the form still holds the values and an error message is shown. Then let the session expire and save, and check the same. Scenarios 4 and 5 apply once 007-delete-product (BL-06), 005-publish-product (BL-05) and 006-adjust-stock (BL-08) exist; until then they are checked with seeded test data.

**Acceptance scenarios:**
1. **Given** the edit form with the name changed to "Blue Mug Large", **When** the save fails because of a network error, **Then** the Admin is told the change was not saved, the form stays open in the same tab with "Blue Mug Large" still entered, and the stored product is unchanged.
2. **Given** the save failed as in scenario 1, **When** the Admin saves again after the connection returns, **Then** the change is saved and the updated product details are shown.
3. **Given** the edit form with changed values, **When** the save fails because the Admin's session has expired, **Then** the stored product is unchanged, the form stays open with the entered values, and a sign-in prompt appears over the form. **When** the Admin signs in through that prompt, **Then** the edit form is shown again with the entered values, and pressing Save saves them.
4. **Given** the edit form of "Blue Mug" is open (applies once 007-delete-product exists), **When** the product is deleted in another tab and the Admin then saves, **Then** the Admin sees "product not found" with a way back to the product list, and nothing is saved.
5. **Given** the edit form of a product with stock quantity 12 and status Draft is open (applies once 005-publish-product and 006-adjust-stock exist), **When** the stock is adjusted to 20 and the product is published in another tab, and the Admin then saves a new name, **Then** the product has the new name, stock quantity 20 and status Active.
6. **Given** the same product is open in the edit form in two tabs, **When** tab A saves the price 150000 and then tab B saves the price 160000, **Then** the stored price is 160.000 ₫ (last save wins, BR-026).

## Edge cases

- Name made only of spaces: it is treated as missing and rejected (BR-013).
- SKU that differs from another product's SKU only by letter case or leading/trailing spaces: rejected as already used (BR-030).
- SKU changed only in letter case or spacing of the product's own SKU: accepted, because it does not conflict with another product (BR-030).
- SKU equal to the SKU of a deleted product (once deletion exists, 007-delete-product / BL-06): rejected as already used (BR-045).
- Boundary values: name of 1 and 200 characters, SKU of 1 and 50, description of 0 and 2,000, and price 0 and 1,000,000,000 are accepted. Name of 201, SKU of 51, description of 2,001, and price -1, 1,000,000,001 or with a fraction are rejected.
- Several invalid fields at once: every invalid field shows its reason in the same response.
- Save with no changes: succeeds, and the product is unchanged.
- Admin clicks Save twice quickly: the product ends with the entered values, and the second save causes no error and no other change.
- Product deleted in another tab while the edit form is open: "product not found", and nothing is saved (PR-04 exception, BR-048).
- Edit form opened through a direct address for a product that does not exist or is deleted: "product not found" with a way back to the product list.
- Stock adjusted or product published in another tab while the edit form is open: the save does not overwrite the newer stock quantity or status (BR-016, BR-029).
- Same product edited in two tabs: the last save wins, and no warning is shown (BR-026).
- Connection lost during save: the form keeps the entered values and shows an error, and the stored product is unchanged (BR-049).
- Session expired before the edit form is opened: the form is not shown, and the Admin must sign in (R002).
- Session expired during save: the form keeps the entered values, and the save is refused (BR-049, BR-015).
- A save request that carries a stock quantity, a product status or unknown fields: rejected with a message naming the unexpected field; nothing is saved (FR-012, BR-016, BR-029).
- The Admin cancels or navigates away with unsaved changes: [NEEDS CLARIFICATION: whether to confirm before discarding; see Open questions]

## Functional requirements

- **FR-001:** The Admin MUST be able to open the edit form of a product from its product details (PR-04).
- **FR-002:** The edit form MUST be pre-filled with the product's current stored product information: name, SKU, price and description (BR-014).
- **FR-003:** The edit form MUST NOT show a stock quantity field or a product status field (BR-016, BR-029).
- **FR-004:** On save, the system MUST validate every field of the product information before storing anything. It MUST store the changes only when every field is valid (BR-017).
- **FR-005:** The system MUST require a name of 1 to 200 characters (BR-013).
- **FR-006:** The system MUST require a SKU of 1 to 50 characters. The SKU MUST NOT equal the SKU of any other product; the comparison ignores letter case and leading/trailing spaces. The product's own current SKU never counts as a conflict (BR-030).
- **FR-007:** The SKU uniqueness check MUST include deleted products, so a SKU kept by a deleted product cannot be given to another product (BR-045). This applies once deletion exists (007-delete-product / BL-06).
- **FR-008:** The system MUST require a price that is a whole number of Vietnamese dong from 0 to 1,000,000,000. It MUST reject a missing, non-numeric, fractional or out-of-range price (BR-031). The price field follows the same input rule as the add form (001-add-product FR-006a): only digits can be typed, they are shown grouped with a dot every three digits while typing (e.g. "150.000"), and pasted text is accepted only as digits with correctly placed dots.
- **FR-009:** The description MUST be optional, at most 2,000 characters long, and the Admin MUST be able to clear it (BR-032).
- **FR-010:** When a save is rejected, the system MUST keep the stored product unchanged. It MUST show, next to each invalid field, the reason that field is invalid, with all invalid fields reported together (BR-017).
- **FR-011:** A successful save MUST show the updated product details with a confirmation that the changes were saved (PR-04). The price is shown in VND with a dot as thousands separator and the ₫ sign, e.g. 150.000 ₫ (BR-050).
- **FR-012:** Saving product information MUST NOT change the product's stock quantity or product status. A save request that carries any field other than the product information (for example a stock quantity, a product status or an unknown field) MUST be rejected with a message naming the unexpected field, and nothing is saved (constitution P6; same rule as 001-add-product FR-009). It MUST NOT overwrite a stock quantity or status that changed since the edit form was opened (BR-016, BR-029).
- **FR-013:** When the same product is saved from two places, the system MUST keep the product information of the last save, without a conflict warning (BR-026).
- **FR-014:** When the product does not exist or has been deleted, opening the edit form or saving MUST show "product not found" with a way back to the product list. Nothing is saved (PR-04 exception, BR-048).
- **FR-015:** When the Admin cancels the edit form, the system MUST save nothing and show the product details with the stored values (PR-04 exception). [NEEDS CLARIFICATION: confirm before discarding unsaved changes?]
- **FR-016:** When a save fails because of a network error or an expired session, the system MUST tell the Admin the change was not saved. The form MUST stay open in the same tab with the entered values so the Admin can retry, after signing in again if needed. The entered values MUST NOT be stored anywhere else in the browser (BR-049). When the cause is an expired session, a sign-in prompt MUST appear over the edit form; after the Admin signs in through it, the edit form MUST still hold the entered values so the Admin can save again (same as 001-add-product FR-015; R002 provides the prompt).
- **FR-017:** Opening the edit form and saving product information MUST be refused without a signed-in Admin session (BR-015).
- **FR-018:** Moving between the product details and the edit form MUST NOT reload the whole page. The browser Back button from the edit form MUST return to the product details (QE-04).

## Non-functional requirements

- **NFR-002 (QE-02):** With 10,000 products stored, 95% of product information saves are confirmed to the Admin within 1 second of the save action, measured in the browser.
- **NFR-003 (QE-03):** 100% of attempts to open the edit form or save product information without a signed-in Admin session are refused, and 0 products are changed by them.
- **NFR-004 (QE-04):** Moving between the product details and the edit form, in either direction, causes 0 full page reloads. The browser Back button returns to the previous view in 100% of tested navigation paths.
- **NFR-005 (QE-05):** 100% of invalid product information is rejected at the boundary with a field-level message, and 0 invalid values are stored.

## Key entities (if any)

- **Product:** something the shop sells and keeps stock of. It has product information, a stock quantity and a product status. This feature changes only its product information.
- **Product information:** the name, SKU, price and description of a product. It excludes the stock quantity and the product status.
- **Admin:** the single signed-in person who edits products (constitution P1).

## Success criteria

- **SC-001:** The Admin updates the product information of one product, from opening the edit form on the product details to seeing the updated product details, in under 1 minute (BG-01).
- **SC-002:** In acceptance tests, 0 saves of product information change a product's stock quantity or product status.
- **SC-003:** In acceptance tests, 0 saves result in two products whose SKUs are equal when letter case and leading/trailing spaces are ignored.
- **SC-004:** In acceptance tests of a failed save (network error, expired session), 100% of entered values are still in the form afterwards.

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BG-01 | Context; SC-001; US1 |
| BR-029 | FR-003, FR-012; US1 scenario 1; US2 scenario 9; US3 scenario 5 |
| BR-013 | FR-005; US2 scenarios 1–2; Edge cases (name of spaces, boundaries) |
| BR-014 | FR-002; Key entities; US1 scenario 1 |
| BR-030 | FR-006; US1 scenario 6; US2 scenarios 3–4; Edge cases; SC-003 |
| BR-031 | FR-008; US2 scenarios 5–6; Edge cases (boundaries) |
| BR-032 | FR-009; US1 scenario 4; US2 scenario 7 |
| BR-050 | FR-011; US1 scenario 2; US3 scenario 6; Open question 1 |
| BR-015 | FR-017; US1 scenario 8; NFR-003 |
| BR-016 | FR-003, FR-012; US1 scenarios 1–2; US2 scenario 9; US3 scenario 5; SC-002 |
| BR-017 | FR-004, FR-010; US2 scenarios 1–8 |
| BR-026 | FR-013; US3 scenario 6; Edge cases; Out of scope |
| BR-049 | FR-016; US3 scenarios 1–3; SC-004 |
| QE-02 | NFR-002 |
| QE-03 | NFR-003 |
| QE-04 | NFR-004; FR-018; US1 scenario 7 |
| QE-05 | NFR-005; FR-004, FR-010 |
| PR-04 exceptions | Edge cases; FR-014 (deleted meanwhile), FR-015 (cancel), FR-016 (failed save), FR-010 (invalid value) |
| BR-045, BR-048 (not in BL-04 Covers, but constrain updates) | FR-007, FR-014 |

## Assumptions

- The edit form is reached only from the product details, as PR-04 states, and not directly from the product list.
- The validation rules and messages for name, SKU, price and description are the same as in the add form (001-add-product / BL-01), so the Admin sees consistent behavior.
- Leading and trailing spaces are removed from the name and SKU before they are validated and stored. A name or SKU made only of spaces therefore counts as missing.
- Product names do not have to be unique. No rule requires it.
- After a successful save, the browser Back button from the updated product details does not reopen the edit form with the old values. It returns to the view the Admin was on before opening the form.
- When a SKU conflicts with a deleted product, the message is the same "SKU already used" message as for any other product.
- NFR numbers follow the R001 quality expectation numbers. QE-01 (product list speed) does not apply to this feature, so NFR-001 is not used.
- Admin sign-in, including session expiry, is delivered by R002 and is not specified here.

## Out of scope

- Adding a product (001-add-product / BL-01).
- The product list and its navigation (002-product-list / BL-02), page size (008-list-page-size / BL-07), search (009-product-search / BL-09), filters (010-product-filters / BL-10) and sorting (012-product-sorting / BL-11).
- The product details view itself (003-product-details / BL-03). This feature only adds the way to the edit form and the updated view after saving.
- Publishing a product, i.e. changing the product status (005-publish-product / BL-05).
- Changing the stock quantity (006-adjust-stock / BL-08).
- Deleting a product (007-delete-product / BL-06), and viewing or restoring deleted products (011-restore-products / BL-12).
- Admin sign-in, session expiry and re-sign-in (R002).
- Change history or audit trail of product information, bulk editing, product categories and images (out of scope in R001).
- Detecting or merging conflicting edits from two tabs. The last save wins (BR-026).

## Open questions

- [x] 1. Price entry format: BR-050 shows prices with a dot as thousands separator (125.000 ₫). In the price field, may the Admin type "125.000", and is it read as 125,000 ₫? Or are only plain digits ("125000") accepted, with "125.000" rejected? The answer must match the add form (001-add-product). (affects: FR-008, US1 scenario 1, US2 scenario 5)
- [ ] 2. When the Admin cancels or navigates away (including with Back) while the edit form has unsaved changes, should the system ask for confirmation before discarding them, or discard them silently? (affects: FR-015, FR-018, NFR-004)
- [x] 3. When a save fails because the session expired, how does the Admin sign in again without losing the form? BR-049 forbids storing the values elsewhere in the browser. Is it an in-place sign-in that R002 must provide, or does the Admin sign in in another tab and then retry here? (affects: FR-016, US3 scenario 3; dependency on R002)
- [x] 4. A save request that carries a stock quantity, a product status or unknown fields: reject it as invalid with a message, or ignore those values and save the product information? Either way the stock quantity and status never change (FR-012). The answer must match 001-add-product open question 1. Recommended: reject, so invalid input is refused at the boundary (constitution P6). (affects: US2 scenario 9, FR-012, NFR-005)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|
| 2026-10-16 | Save request carrying stock quantity, status or unknown fields: reject or ignore? | Reject the whole request with a message naming the unexpected field; nothing saved (decided with 001-add-product). |
| 2026-10-16 | How may the Admin type a price? | Digits only, formatted with a dot every three digits while typing; pasted text accepted only as digits with correctly placed dots (decided with 001-add-product). |
| 2026-10-16 | How does the Admin sign in again after the session expires with the edit form open? | A sign-in prompt appears over the form; after signing in, the form still holds the entered values (decided with 001-add-product; R002 provides the prompt). |

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-16 | Initial version | — |
