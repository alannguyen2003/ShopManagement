# business-analyst memory

- The only actor is the Admin (constitution P1). There is no customer, storefront or order domain, so product status matters only to the Admin and the delete rule.
- As of R001, the project has no sign-in feature. Every product backlog item depends on Admin authentication (P5), which lives outside R001 unless Q1 decides otherwise.
- R001 vocabulary: Product, Product information, Stock quantity, Starting stock quantity, Out of stock, Product status, Draft, Product list, Page size, Product details, Product management page.
- The requester (alannguyen2003) writes short one-paragraph inputs without field lists or lifecycles. Always ask for entity fields, status transitions and delete semantics.
- The scaffold's `/health` endpoint is `AllowAnonymous`, which is a possible P5 conflict to watch in reviews.
- R001 spec numbers do not follow BL order: 001=BL-01, 002=BL-02, 003=BL-03, 004=BL-04, 005=BL-05, 006=BL-08, 007=BL-06, 008=BL-07, 009=BL-09, 010=BL-10, 011=BL-12, 012=BL-11.
- The shop works in Vietnamese (VND, names with diacritics such as "Đèn bàn"): text matching and ordering rules must say how accents and differently-entered accented letters are handled.
- R002 (Admin sign-in) is not written yet; R001 specs treat sign-in as external. BR-049 (keep form values, retry after signing in again, nothing stored elsewhere in the browser) only works if R002 lets the Admin sign in again without leaving the page — raise it when R002 is analysed.
- Cross-cutting R001 decisions to answer once for all sibling specs: reject vs ignore extra request fields (001/004), price input format with dot separators (001/004), lost-response retry (001/006), browser-history step per list-state change (002/008/009/010/012), Vietnamese accents in search/sort (009/012), invalid list parameters fall back vs reject (008 vs 009/010/012).
- BR-045 (deleted SKU stays reserved on add and update) applies to BL-04 although its Covers omits it; BR-048 is owned by BL-06/007; 007 records the deletion date that 011 (BR-046) shows.
- NFR numbering in specs: use one scheme per batch — sequential "NFR-001 (QE-01)" with the source QE in brackets, not "NFR number = QE number".
