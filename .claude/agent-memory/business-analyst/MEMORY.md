# business-analyst memory

- The only actor is the Admin (constitution P1). There is no customer, storefront or order domain, so product status matters only to the Admin and the delete rule.
- As of R001, the project has no sign-in feature. Every product backlog item depends on Admin authentication (P5), which lives outside R001 unless Q1 decides otherwise.
- R001 vocabulary: Product, Product information, Stock quantity, Starting stock quantity, Out of stock, Product status, Draft, Product list, Page size, Product details, Product management page.
- The requester (alannguyen2003) writes short one-paragraph inputs without field lists or lifecycles. Always ask for entity fields, status transitions and delete semantics.
- The scaffold's `/health` endpoint is `AllowAnonymous`, which is a possible P5 conflict to watch in reviews.
