# ============================================================================
# نام فایل: verify-no-secrets.ps1
# مسئولیت: اسکن تکرارپذیر repository برای secretهای رایج پیش از انتشار.
# وابستگی‌ها و لایه: ابزار کیفیت → source/config/docs؛ هیچ شبکه، APK یا دادهٔ کاربر را تغییر نمی‌دهد.
# نکات تغییر و قیود: placeholderهای مستند deployment مجازند؛ هر مقدار واقعی باید build را ناموفق کند.
# ============================================================================

[CmdletBinding()]
param(
    [string]$RepositoryRoot
)

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
}

$excludedDirectories = @('.git', 'bin', 'obj', 'sources', '.vs', '.gradle', 'node_modules')
$patterns = @(
    'AIza[0-9A-Za-z_-]{20,}',
    'sk-[A-Za-z0-9]{20,}',
    '-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----',
    '(?im)^\s*(?:GEMINI_API_KEY|COACH_PROXY_SESSION_SECRET)\s*=\s*(?!<[^>]+>$)[^\s#]+$'
)

$allowedDocumentationValues = @(
    'GEMINI_API_KEY=<secret supplied by the deployment secret manager>',
    'COACH_PROXY_SESSION_SECRET=<backend-only HMAC secret>'
)

$files = Get-ChildItem -Path $RepositoryRoot -Recurse -File | Where-Object {
    $_.FullName -notmatch ('[\\/](' + ($excludedDirectories -join '|') + ')[\\/]') -and
    $_.Extension -in '.cs', '.csproj', '.xaml', '.xml', '.json', '.yml', '.yaml', '.properties', '.md', '.ps1', '.kt', '.kts'
}

$findings = foreach ($file in $files) {
    $lineNumber = 0
    foreach ($line in Get-Content -LiteralPath $file.FullName) {
        $lineNumber++
        if ($allowedDocumentationValues -contains $line.Trim()) { continue }
        foreach ($pattern in $patterns) {
            if ($line -match $pattern) {
                [pscustomobject]@{ File = $file.FullName; Line = $lineNumber; Pattern = $pattern }
            }
        }
    }
}

if ($findings) {
    $findings | Format-Table -AutoSize | Out-String | Write-Error
    exit 1
}

Write-Output 'Secret scan passed: no credentials or private keys found.'
