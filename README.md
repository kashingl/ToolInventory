# ToolInventory

ToolInventory is currently in bootstrap phase. This repository includes a security baseline so new code can be added safely.

## Security baseline

1. Vulnerability reporting process: see [SECURITY.md](SECURITY.md).
2. Automated checks in CI: CodeQL, secret scanning (Gitleaks), dependency review, and SBOM generation.
3. Dependency updates: Dependabot for GitHub Actions.
4. Secret hygiene: `.gitignore` blocks common local secret files.

## Secure development rules

1. Never commit secrets, tokens, or production credentials.
2. Use least privilege for tokens, services, and CI permissions.
3. Validate and sanitize all external input.
4. Log security-relevant events without sensitive payloads.

## Required repository settings (GitHub)

Configure branch protection on `main`:

1. Require pull requests before merging.
2. Require at least one approving review.
3. Require all status checks to pass.
4. Block force pushes and branch deletion.
5. Require signed commits (recommended).