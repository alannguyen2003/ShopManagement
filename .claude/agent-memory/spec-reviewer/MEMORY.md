# spec-reviewer memory

- Requirements: when constitution P5 (authentication) applies and no sign-in feature exists, require a sign-in backlog item, or an explicit external dependency in the "Depends on" column. Saying "out of scope" is not enough.
- Requirements: the "Depends on" column must include every item needed to create the data a key scenario needs (for example, delete-at-quantity-0 needs the update-stock item).
- Requirements: rules stated only in process steps or exceptions (paging edge cases, re-checks, navigation memory) must also be numbered BRs.
- Requirements: glossary definitions and backlog text must not use the "Do not call it" synonyms of their own glossary.
- Requirements: the first backlog item must not include a view (product details, list) that a later item owns. Check for post-save or after-cancel overlap.
- Requirements: a capability inferred from a passing mention in the input (for example "status change" from "draft") must be labelled an assumption in In scope and in the backlog brief.
- Requirements: when document A must be built before document B, A's key scenarios must not rely on B's features. Check for build-order cycles across documents.
- Requirements: build-order statements must agree across Constraints, Risks and the Backlog note.
- Requirements: lockout rules must cover simultaneous attempts, attempts during a lockout, and duplicate submissions (including a double-submitted password change counting as a failed attempt).
- Requirements: when P5 applies, check the existing code (Program.cs) for every endpoint that answers without a session (health check, OpenAPI document), not only the one already known.
- Requirements: the transport-encryption rule must cover every request that carries a session, not only requests that carry a password.
- Requirements: when a rule from another requirements document is restated, point to it instead of copying it, so one rule is not owned by two documents.
