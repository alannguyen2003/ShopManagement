# spec-reviewer memory

- Requirements: when constitution P5 (authentication) applies and no sign-in feature exists, require a sign-in backlog item, or an explicit external dependency in the "Depends on" column. Saying "out of scope" is not enough.
- Requirements: the "Depends on" column must include every item needed to create the data a key scenario needs (for example, delete-at-quantity-0 needs the update-stock item).
- Requirements: rules stated only in process steps or exceptions (paging edge cases, re-checks, navigation memory) must also be numbered BRs.
- Requirements: glossary definitions and backlog text must not use the "Do not call it" synonyms of their own glossary.
- Requirements: the first backlog item must not include a view (product details, list) that a later item owns. Check for post-save or after-cancel overlap.
- Requirements: a capability inferred from a passing mention in the input (for example "status change" from "draft") must be labelled an assumption in In scope and in the backlog brief.
- R001 glossary "Do not call it" synonyms keep recurring in specs: "product data", "restock", "ordering", "amount", "reference", "remove". Check every section, including Assumptions and Out of scope.
- Spec: Given states and Independent tests must use only data the item's Depends-on chain can create (e.g. stock ≠ 1, Active, deleted); otherwise mark "seeded test data" or "once NNN exists".
- Spec: specs drafted in parallel must agree on shared decisions (history steps, extra request fields, trimming, discard prompt, invalid list parameters, P7 "no internal details" NFR, NFR numbering); one sibling must not silently answer what another asks.
- Spec: retry-after-network-error scenarios must cover a lost reply (operation saved, reply lost).
- Spec: "at most once"/"never again" rules in a delete spec must not forbid deleting again after a restore.
- Spec: every key scenario in the backlog brief (incl. "request without a signed-in session") must be a Given/When/Then scenario; a story delivering a P1 item's BR must not be P2.
- Spec: check-then-act guards (delete condition vs concurrent publish/adjust) must state check and effect are indivisible.
- Spec: an FR must not hold a MUST owned by a later backlog item; keep it in Out of scope / Edge cases.
- Spec: do not raise [NEEDS CLARIFICATION] on something an approved BR settles just because the item's Covers omits it; cite the BR and flag the Covers gap for /sdd-amend requirements.
- Spec: a duplicate-submit "at most once" rule for anything but adding a product is a new business rule (BR-028 covers add only) and conflicts with BR-035 "both count".
- Spec: a sub-view (dialog, inline form) must say whether it adds a browser-history entry, or QE-04 Back tests can't be written.
