# Coding preferences

Apply these preferences alongside AGENTS.md and .editorconfig.

- Use enums for values with a defined set of outcomes, such as statuses and
  diagnostic severities. Convert to strings at external boundaries when needed;
  preserve readable enum names in JSON reports.
- Prefer one operation per line and explicit intermediate variables.
- Expand conditional assignments into separate statements and clear branches
  rather than using ternary expressions or compact, clever expressions.
- Use LINQ and lambdas only when necessary or when they clearly improve
  readability. Straightforward loops are appropriate otherwise.
- Separate meaningful steps with vertical whitespace. Keep related statements
  together and avoid dense blocks of unrelated operations.
- Preserve manual edits and established local style when making changes.
- Reuse the existing command-line package for parsing and generated help.
  Keep application-specific validation and execution in the application.
- Keep methods small and focused on one responsibility. Evaluate validation
  rules once and return the result with its diagnostic.
