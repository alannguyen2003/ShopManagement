# spec-reviewer memory

- Requirements: when constitution P5 (authentication) applies and no sign-in feature exists, require a sign-in backlog item, or an explicit external dependency in the "Depends on" column. Saying "out of scope" is not enough.
- Requirements: the "Depends on" column must include every item needed to create the data a key scenario needs (for example, delete-at-quantity-0 needs the update-stock item).
- Requirements: rules stated only in process steps or exceptions (paging edge cases, re-checks, navigation memory) must also be numbered BRs.
- Requirements: glossary definitions and backlog text must not use the "Do not call it" synonyms of their own glossary.
- Requirements: the first backlog item must not include a view (product details, list) that a later item owns. Check for post-save or after-cancel overlap.
- Requirements: a capability inferred from a passing mention in the input (for example "status change" from "draft") must be labelled an assumption in In scope and in the backlog brief.
