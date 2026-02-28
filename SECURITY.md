# Security Policy

## Reporting a vulnerability

If you discover a security issue, do not open a public issue with exploit details.
Report privately to the maintainers and include:

- affected component (`api`, `console-ui`, `maui`, `docker`)
- reproduction steps
- potential impact

## Scope

Security-sensitive areas include:

- environment variables and secrets handling
- uploaded audio handling and file lifecycle
- dependency supply chain (Python, NuGet, Docker base images)
- network exposure and API authentication (if added)

## Best practices in this repo

- no secrets committed to source control
- machine-local artifacts ignored in `.gitignore`
- CI validates builds across core components
- dependencies should be updated regularly
