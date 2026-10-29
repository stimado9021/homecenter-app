param([string])

# Patterns to detect
 = @(
    '(?i)AKIA[0-9A-Z]{16}',                                                          # AWS Access Key ID
    '(?<![A-Za-z0-9/+])[A-Za-z0-9/+]{40}(?![A-Za-z0-9/+])',                         # AWS Secret Key (approx)
    'AIza[0-9A-Za-z\-_]{35}',                                                       # Google API Key
    '(?i)ya29\.[0-9A-Za-z\-_]+',                                                    # Google OAuth token
    '(?i)sg\.[0-9a-zA-Z]{22}',                                                      # SendGrid key
    '(?i)sk_(test|live)_[a-zA-Z0-9]{24}',                                           # Stripe key
    '(?i)Bearer [a-zA-Z0-9_\-=\._~+/]+',                                            # Bearer token (simple)
    '(?i)(password|passwd|pwd|secret|key|token|auth)[\s]*[:=][\s]*[^\s]{8,}'       # Generic key/value with long value
)

 = false
 = git diff --cached --name-only --diff-filter=ACMR | ForEach-Object { .Trim() }

if (-not ) {
    exit 0
}

foreach ( in ) {
    if (-not (Test-Path )) {
        continue
    }
    try {
         = Get-Content -Path  -Raw -ErrorAction Stop
        foreach ( in ) {
            if ( -match ) {
                 = [regex]::Matches(, )
                foreach ( in ) {
                    Write-Host "[SECRET DETECTED] Pattern '' matched in file ''
  Value: " -ForegroundColor Red
                     = True
                }
            }
        }
    } catch {
        # Ignore binary files etc.
    }
}

if () {
    Write-Host "
Commit blocked due to potential secrets detected.
Remove the secrets and use environment variables or secret managers.
" -ForegroundColor Red
    exit 1
} else {
    exit 0
}
