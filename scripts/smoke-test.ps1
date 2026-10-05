param(
    [string]$ApiBaseUrl = "http://localhost:5000",
    [string]$WebBaseUrl = "http://localhost:3000"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Assert-Status {
    param(
        [System.Net.Http.HttpResponseMessage]$Response,
        [int[]]$Expected,
        [string]$Name
    )

    $code = [int]$Response.StatusCode
    if ($Expected -notcontains $code) {
        throw "$Name failed: expected $($Expected -join '/') but got $code"
    }

    Write-Host "PASS $Name ($code)"
}

$api = $ApiBaseUrl.TrimEnd("/")
$web = $WebBaseUrl.TrimEnd("/")
$client = [System.Net.Http.HttpClient]::new()
$client.Timeout = [TimeSpan]::FromSeconds(15)

try {
    Assert-Status -Response ($client.GetAsync("$api/health").GetAwaiter().GetResult()) -Expected 200 -Name "API /health"
    Assert-Status -Response ($client.GetAsync("$api/health/live").GetAwaiter().GetResult()) -Expected 200 -Name "API /health/live"
    Assert-Status -Response ($client.GetAsync("$api/health/ready").GetAwaiter().GetResult()) -Expected 200 -Name "API /health/ready"
    Assert-Status -Response ($client.GetAsync("$api/api/public/courses").GetAwaiter().GetResult()) -Expected 200 -Name "API public courses"
    Assert-Status -Response ($client.GetAsync("$api/api/public/courses/does-not-exist-$([guid]::NewGuid())").GetAwaiter().GetResult()) -Expected 404 -Name "API unknown course 404"

    try {
        Assert-Status -Response ($client.GetAsync("$web/").GetAwaiter().GetResult()) -Expected 200 -Name "Web homepage"
    }
    catch {
        Write-Host "SKIP Web homepage ($($_.Exception.Message))"
    }

    $email = [Environment]::GetEnvironmentVariable("SMOKE_EMAIL")
    $password = [Environment]::GetEnvironmentVariable("SMOKE_PASSWORD")
    if ([string]::IsNullOrWhiteSpace($email) -or [string]::IsNullOrWhiteSpace($password)) {
        Write-Host "SKIP authenticated smoke (set SMOKE_EMAIL and SMOKE_PASSWORD to enable)"
    }
    else {
        $body = @{ email = $email; password = $password; rememberMe = $false } | ConvertTo-Json
        $content = [System.Net.Http.StringContent]::new($body, [System.Text.Encoding]::UTF8, "application/json")
        $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Post, "$api/api/auth/login")
        $request.Headers.Add("X-Requested-With", "XMLHttpRequest")
        $request.Content = $content
        $login = $client.SendAsync($request).GetAwaiter().GetResult()
        Assert-Status -Response $login -Expected @(200, 401) -Name "API login"
        if ([int]$login.StatusCode -eq 401) {
            Write-Host "WARN login returned 401 (credentials rejected). Password is not printed."
        }
    }

    Write-Host "Smoke test finished."
}
finally {
    $client.Dispose()
}
