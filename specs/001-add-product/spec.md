# Spec: Add a product

**Feature:** 001-add-product
**Branch:** feat/001-add-product
**Source:** R001/BL-01
**Status:** Approved
**Approved:** 2026-10-16
**Revision:** 1
**Created:** 2026-10-16

## Context

The shop has no way to record products yet. The Admin needs to add a product with its product information (name, SKU, price, description) so the shop's products can start to be managed. Every new product starts with a stock quantity of exactly 1 and the product status Draft. The Admin does not enter either value. This is the first item of the R001 backlog. The product list, product details, editing, publishing, stock adjustment and deletion build on it. Every page and operation requires a signed-in Admin (constitution P5). Sign-in is delivered separately by R002 (Admin sign-in), which must exist before this feature can be used.

## User stories

### US1 — Add a product with valid product information (Priority: P1)

As the Admin, I want to enter a new product's name, SKU, price and description and save it, so that the shop's products start to be recorded with a known stock quantity.

**Why P1:** Without adding products, no other product capability (list, details, edit, publish, stock adjustment, delete) has any data to work on.

**Independent test:** Sign in as the Admin, open the add form from the product management page, enter valid product information and save. The success confirmation shows the saved values with stock quantity 1 and product status Draft. Opening the add form again and saving a product with the same SKU is rejected, which proves the first product was stored.

**Acceptance scenarios:**
1. **Given** the signed-in Admin is on the product management page, **When** they choose to add a product, **Then** the add form opens without a full page reload and shows exactly four fields: name, SKU, price and description. It has no stock quantity field and no product status field.
2. **Given** the add form is open, **When** the Admin enters name "Blue Mug", SKU "MUG-01", types the price 125000 (the field shows "125.000"), enters description "Ceramic, 350 ml" and saves, **Then** the product is stored with stock quantity 1 and product status Draft, and a success confirmation shows: name "Blue Mug", SKU "MUG-01", price "125.000 ₫", description "Ceramic, 350 ml", stock quantity 1, product status Draft.
3. **Given** the add form is open, **When** the Admin enters a name, a SKU and price 0, leaves the description empty and saves, **Then** the product is stored with price "0 ₫", no description, stock quantity 1 and product status Draft.
4. **Given** the add form is open with valid values, **When** the Admin presses Save twice in quick succession, **Then** exactly one product is stored and one success confirmation is shown.
5. **Given** the add form is open with values entered, **When** the Admin cancels, **Then** nothing is stored and the product management page is shown.
6. **Given** an add request that contains a stock quantity of 50 or a product status of Active, **When** it reaches the system, **Then** the request is rejected with a message naming the unexpected field, and no product is stored.
7. **Given** there is no signed-in Admin session, **When** someone opens the add form or sends an add request, **Then** the request is refused and no product is stored.

### US2 — Be told exactly what is wrong with invalid input (Priority: P1)

As the Admin, I want every invalid field to be pointed out with its reason, and nothing to be saved, so that I can correct my input and the stored product information is always valid.

**Why P1:** Without field-level validation, invalid or duplicate products could be stored. That would break the product information rules every later feature relies on (constitution P6).

**Independent test:** With one product "MUG-01" stored, submit the add form with each kind of invalid value listed below. Each one is rejected with a message on the right field and no new product is stored. A later valid save with a new SKU succeeds.

**Acceptance scenarios:**
1. **Given** the add form is open, **When** the Admin leaves the name empty (or enters only spaces) and saves, **Then** the save is rejected, nothing is stored, and the name field shows that the name is required.
2. **Given** the add form is open, **When** the Admin enters a name of 201 characters and saves, **Then** the save is rejected and the name field says the name can have at most 200 characters. A name of exactly 200 characters is accepted.
3. **Given** the add form is open, **When** the Admin leaves the SKU empty, or enters a SKU of 51 characters, and saves, **Then** the save is rejected with a message on the SKU field. A SKU of exactly 50 characters is accepted.
4. **Given** a product with SKU "ABC-1" exists, **When** the Admin adds a product with SKU "abc-1 " and saves, **Then** the save is rejected, nothing is stored, and the SKU field says the SKU is already used by another product.
5. **Given** the add form is open, **When** the Admin types 1000000001, or leaves the price empty, and saves, **Then** the save is rejected and the price field says the price must be a whole number of dong from 0 to 1.000.000.000. A price of exactly 1.000.000.000 is accepted.
6. **Given** the add form is open, **When** the Admin enters a description of 2,001 characters and saves, **Then** the save is rejected with a message on the description field. A description of exactly 2,000 characters is accepted.
7. **Given** the add form is open, **When** the Admin leaves the name empty and enters price -1 and saves, **Then** both the name field and the price field show their messages at the same time, and the entered values stay in the form for correction.
8. **Given** an add request with invalid values sent directly to the system, bypassing the form's own checks, **When** it is received, **Then** it is rejected with the same field-level reasons and nothing is stored.
9. **Given** the add form is open, **When** the Admin tries to type "-", "," or a letter in the price field, or pastes "125,5", **Then** the typed character does not appear and the pasted text is rejected with a message on the price field.
10. **Given** an add request sent directly to the system with price -1, 1500.5 or a non-numeric value, **When** it is received, **Then** it is rejected with a message on the price field and nothing is stored.

### US3 — Keep entered values when a save fails (Priority: P2)

As the Admin, I want the add form to keep what I entered when a save fails because of a network error or an expired session, so that I can retry without typing everything again.

**Why P2:** The MVP works without it, but losing a filled-in form to a dropped connection or an expired session wastes the Admin's time and works against BG-01 (under 1 minute per operation).

**Independent test:** Open the add form, enter valid values, cut the network connection and save. The form stays open with the values and an error saying the product was not saved. Restore the connection, save again, and the success confirmation appears with exactly one product stored.

**Acceptance scenarios:**
1. **Given** the add form is filled with valid values and the network connection is lost, **When** the Admin saves, **Then** the form stays open in the same tab with all entered values, an error message says the product was not saved, and no product is stored.
2. **Given** the situation of scenario 1 and the connection is back, **When** the Admin saves again, **Then** the product is stored once and the success confirmation is shown.
3. **Given** the Admin's session has expired while the add form was open, **When** the Admin saves, **Then** the save is refused, no product is stored, the form stays open with the entered values, and a sign-in prompt appears over the form. **When** the Admin signs in through that prompt, **Then** the add form is shown again with all entered values, and pressing Save stores the product once.
4. **Given** a save failed, **When** the Admin reloads the page or closes the tab, **Then** the entered values are not kept. They were never stored anywhere else in the browser.
5. **Given** the Admin saved valid values, the product was stored, but the response was lost and the form shows that the product was not saved, **When** the Admin presses Save again, **Then** no second product is stored, no "SKU already used" message is shown, and the success confirmation for the stored product is shown.

## Edge cases

- Name or SKU made only of spaces: counted as missing (required-field message).
- Name or SKU with leading or trailing spaces: the spaces are trimmed before the checks and before storing.
- SKU that differs from an existing product's SKU only in letter case or in leading/trailing spaces: rejected as already used (BR-030).
- SKU of a deleted product: rejected as already used. Deleted products keep their SKU reserved (BR-045). Deletion itself arrives with 007-delete-product (BL-06).
- Two products with the same SKU added at the same moment from two tabs: exactly one is stored and the other is rejected on the SKU field.
- Two products with the same name but different SKUs: both are accepted. Only the SKU must be unique.
- Boundary values: name 200 / 201 characters, SKU 50 / 51, description 2,000 / 2,001, price 0 and 1,000,000,000 (accepted) and 1,000,000,001 (rejected).
- Price input: letters, signs, commas and decimal points cannot be typed; dots are added automatically every three digits (FR-006a). Pasting "125.000" gives 125.000 ₫; pasting "125,5", "12.50" or "-1" is rejected with a message on the price field.
- Extremely large input (for example a name of 1 million characters): rejected with a field-level message, nothing stored, no internal details shown.
- Vietnamese text with diacritics in name, SKU or description: accepted and shown unchanged.
- An add request carrying fields outside product information (stock quantity, product status, or unknown fields): rejected with a message naming the unexpected field; nothing is stored (FR-009).
- Double click on Save: one product is stored (BR-028).
- Network error or expired session during save: form kept with values (US3).
- Lost response: the product was stored but the Admin saw a failure and presses Save again → the retry is recognized as the same Save action; no second product is stored and the success confirmation for the stored product is shown, not a "SKU already used" error (FR-014).
- Cancel or browser Back from a filled form: the values are discarded without a prompt and nothing is stored.
- Browser Back from the success confirmation: shows the add form empty and never stores the product a second time.
- No signed-in Admin session: the add form is not usable and every add request is refused (BR-015).

## Functional requirements

- **FR-001:** The product management page MUST offer the signed-in Admin a way to open the add form, without a full page reload.
- **FR-002:** The add form MUST contain exactly the product information fields (name, SKU, price, description) and MUST NOT contain a stock quantity field or a product status field (BR-002, BR-014, BR-029).
- **FR-003:** The system MUST store a new product only if every field is valid under FR-004 to FR-008. Otherwise it MUST store nothing (BR-017).
- **FR-004:** The name MUST be required and 1 to 200 characters long (BR-013).
- **FR-005:** The SKU MUST be required and 1 to 50 characters long, and MUST be unique among all stored products, including deleted ones. Uniqueness MUST ignore letter case and leading/trailing spaces (BR-030, BR-045).
- **FR-006:** The price MUST be required and MUST be a whole number of Vietnamese dong (VND) from 0 to 1,000,000,000. Negative, fractional, out-of-range or non-numeric values MUST be rejected (BR-031).
- **FR-006a:** The price field MUST accept only digits as typed input and MUST show them grouped with a dot every three digits while the Admin types (typing 125000 shows "125.000"), matching the display format of BR-050. Pasted text MUST be accepted only if it consists of digits, optionally grouped with correctly placed dots (e.g. "125.000"); any other pasted text MUST be rejected with a message on the price field. The value checked and stored is the whole number without separators.
- **FR-007:** The description MUST be optional and at most 2,000 characters long (BR-032).
- **FR-008:** The system MUST trim leading and trailing spaces from the name and the SKU before checking and storing them. A name or SKU made only of spaces counts as missing.
- **FR-009:** The system MUST store every new product with the starting stock quantity of exactly 1 (BR-001, BR-002, BR-003). An add request that carries any field other than the product information (for example a stock quantity, a product status or an unknown field) MUST be rejected with a message naming the unexpected field, and nothing is stored (constitution P6).
- **FR-010:** The system MUST store every new product with product status Draft. A product has exactly one product status (BR-011, BR-029).
- **FR-011:** The system MUST record the date and time each product is added. The product list (002-product-list, BL-02) orders by it.
- **FR-012:** When a save is rejected, the system MUST show every invalid field at once, each with its reason next to the field, and MUST keep the entered values in the form (BR-017).
- **FR-013:** When a product is stored, the system MUST show a success confirmation with the saved values: name, SKU, price formatted with a dot as thousands separator and the ₫ sign (e.g. "125.000 ₫"), description, stock quantity 1 and product status Draft (BR-050).
- **FR-014:** One Save action MUST create at most one product, even if the Admin submits it twice (BR-028). A retry of the same Save action after a failure in which the product was in fact stored (lost response) MUST be recognized as that same Save action: no second product is stored and the success confirmation for the stored product is shown.
- **FR-015:** When a save fails because of a network error or an expired session, the add form MUST stay open in the same tab with the entered values and a message saying the product was not saved, and the Admin MUST be able to retry. The entered values MUST NOT be stored anywhere else in the browser (BR-049). When the cause is an expired session, a sign-in prompt MUST appear over the add form; after the Admin signs in through it, the add form MUST still hold the entered values so the Admin can save again. A retry after a lost response follows FR-014.
- **FR-016:** When the Admin cancels the add form, the system MUST store nothing and MUST show the product management page.
- **FR-017:** Only the signed-in Admin MUST be able to open the add form and add a product. Any add request without a signed-in Admin session MUST be refused and nothing stored (BR-015). Sign-in itself is provided by R002.
- **FR-018:** The system MUST apply the checks of FR-003 to FR-010 to every add request it receives, including requests that bypass the add form (constitution P6).

## Non-functional requirements

NFR-001 is not used: QE-01 (product list speed) belongs to 002-product-list (BL-02).

- **NFR-002:** With 10,000 products stored, 95% of successful adds are confirmed to the Admin within 1 second of pressing Save, measured in the browser (QE-02).
- **NFR-003:** 100% of add requests and add form openings without a signed-in Admin session are refused, and 0 products are stored by them (QE-03).
- **NFR-004:** Moving between the product management page, the add form and the success confirmation causes 0 full page reloads. The browser Back button returns to the previous view (QE-04).
- **NFR-005:** 100% of invalid add inputs are rejected by the system itself with a field-level message, and 0 invalid values are stored (QE-05).
- **NFR-006:** 0 error messages shown to the Admin contain stack traces, internal identifiers or other internal details (constitution P7).

## Key entities

- **Product:** something the shop sells and keeps stock of. Attributes: product information (name, SKU, price, description), stock quantity (exactly 1 when added), product status (Draft when added), and date added. Its SKU stays reserved while the product is kept, including after it is deleted (BL-06).
- **Product information:** the four descriptive fields of a product: name, SKU, price, description. It excludes stock quantity and product status.

## Success criteria

- **SC-001:** The Admin adds a product (open the add form, enter the product information, save, see the confirmation) in under 1 minute without help (BG-01).
- **SC-002:** 100% of added products are stored with stock quantity 1 and product status Draft (BG-02, BR-001, BR-029).
- **SC-003:** 0 stored products violate the product information rules (name, SKU, price, description), and 0 pairs of stored products share a SKU after case and space trimming (BR-013, BR-030–BR-032).
- **SC-004:** 0 duplicate products are created by a double-clicked Save (BR-028).

## Coverage

| Covers ID | Where in the spec |
|-----------|-------------------|
| BG-01 | SC-001; US3 "Why P2"; FR-001, FR-013 |
| BG-02 | SC-002; FR-009 |
| BR-001 | FR-009; US1 scenario 2; SC-002 |
| BR-002 | FR-002, FR-009; US1 scenario 1 |
| BR-003 | FR-009 (stock quantity stored as exactly 1, inside 0–1,000,000); US1 scenario 6 |
| BR-011 | FR-010 |
| BR-029 | FR-002, FR-010; US1 scenarios 1, 2, 6; SC-002 |
| BR-013 | FR-004; US2 scenarios 1, 2; SC-003 |
| BR-014 | FR-002; Key entities (Product information); US1 scenario 1 |
| BR-030 | FR-005, FR-008; US2 scenarios 3, 4; Edge cases (case/space, concurrent add); SC-003 |
| BR-031 | FR-006, FR-006a; US2 scenarios 5, 9, 10; SC-003 |
| BR-032 | FR-007; US2 scenario 6; US1 scenario 3 |
| BR-050 | FR-013; US1 scenarios 2, 3 |
| BR-045 | FR-005; Edge cases (SKU of a deleted product); Key entities |
| BR-015 | FR-017; US1 scenario 7; NFR-003 |
| BR-017 | FR-003, FR-012; US2 scenarios 1–8 |
| BR-028 | FR-014; US1 scenario 4; SC-004; Open question 4 |
| BR-049 | FR-015; US3 scenarios 1–4; Open question 3 |
| QE-02 | NFR-002 |
| QE-03 | NFR-003 |
| QE-04 | NFR-004; FR-001; US1 scenario 1 |
| QE-05 | NFR-005; FR-018; US2 scenario 8 |

## Assumptions

- A signed-in Admin session exists, provided by R002 (Admin sign-in). This feature does not define sign-in, sign-out, session length or where an unauthenticated Admin is sent; R002 does. This feature cannot be shipped before R002.
- R002 must provide a sign-in prompt that can appear over the current page and return the Admin to that page unchanged after signing in (needed by FR-015 and BR-049). This is a requirement to carry into R002 when it is analysed.
- Until 002-product-list (BL-02) is built, the product management page shows only the way to open the add form.
- The success confirmation is not the product details view (003-product-details, BL-03). It offers two choices: add another product, or return to the product management page.
- Lengths are counted in characters as the Admin sees them; a Vietnamese letter with diacritics counts as 1.
- The SKU is stored with the letter case the Admin typed, after trimming. The description is stored as entered and may contain line breaks.
- The SKU may contain any visible characters, including inner spaces. R001 sets only its length and uniqueness.
- Product names do not have to be unique.
- Cancel, and browser Back from a filled add form, discard the entered values without asking for confirmation.
- Browser Back from the success confirmation shows an empty add form and never re-submits.

## Out of scope

- Viewing the product list: 002-product-list (BL-02).
- Viewing product details, including any link from the confirmation to the product details: 003-product-details (BL-03).
- Editing product information: 004-update-product (BL-04).
- Publishing a product (Draft → Active): 005-publish-product (BL-05).
- Changing the stock quantity after adding: 006-adjust-stock (BL-08).
- Deleting a product: 007-delete-product (BL-06). This spec only makes sure deleted products' SKUs stay reserved once deletion exists.
- Choosing the page size: 008-list-page-size (BL-07). Search: 009-product-search (BL-09). Filters: 010-product-filters (BL-10). Sorting: 012-product-sorting (BL-11).
- Viewing and restoring deleted products: 011-restore-products (BL-12).
- Admin sign-in, sign-out, session expiry, brute-force protection: R002 (Admin sign-in).
- Entering a starting stock quantity other than 1, or choosing the product status when adding (BR-001, BR-002, BR-029).
- Categories, images, bulk add, import/export, stock history (R001 "Out of scope").
- Keeping unsaved form values across page reloads, tabs or sessions (BR-049).

## Open questions

- [x] 1. An add request that carries a stock quantity, a product status or other unknown fields: reject it as invalid with a message, or ignore those values and store the product with stock quantity 1 and Draft? Recommended: reject, so invalid input is refused at the boundary (constitution P6). (affects: FR-009, FR-018, US1 scenario 6)
- [x] 2. Does the price field accept dots as thousands separators ("125.000" = 125,000 ₫, matching the display format of BR-050), or plain digits only ("125.000" rejected)? Recommended: plain digits only, so a typed value can never be misread as a different amount. (affects: FR-006, US2 scenario 5)
- [x] 3. When the session expires while the add form is open, how does the Admin sign in again without losing the entered values, given BR-049 forbids keeping them anywhere else in the browser? Options: A) R002 provides a sign-in prompt over the current page; B) the Admin signs in from another tab, then retries in this one. Recommended: A, decided together with R002. (affects: FR-015, US3 scenario 3)
- [x] 4. If a save actually stored the product but the Admin saw a failure (lost response) and retries, should the system recognize the retry as the same save and show the success confirmation, or reject it because the SKU is now used? Recommended: recognize it as the same save, so the Admin is not told their own product is a duplicate (BR-028). (affects: FR-014, FR-015)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|
| 2026-10-16 | Add request carrying stock quantity, status or unknown fields: reject or ignore? | Reject the whole request with a message naming the unexpected field; nothing stored. Same rule applies to 004-update-product. |
| 2026-10-16 | How does the Admin sign in again after the session expires with the add form open? | A sign-in prompt appears over the form; after signing in, the form still holds the entered values and the Admin saves again. R002 must provide this prompt. |
| 2026-10-16 | Retry after a lost response, when the product was in fact stored? | Recognized as the same Save action: no duplicate, success confirmation shown, no "SKU already used" error. |
| 2026-10-16 | How may the Admin type a price? | Digits only, formatted with a dot every three digits while typing (125000 → "125.000"); pasted text accepted only as digits with correctly placed dots. Same rule applies to 004-update-product. |

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | 2026-10-16 | Initial version | — |
