$ErrorActionPreference = "Stop"
$baseUrl = "http://localhost:5016"

try {
    # Step 1: Get login page
    $response = Invoke-WebRequest -Uri $baseUrl -UseBasicParsing -SessionVariable session
    Write-Host "GET login page: Status $($response.StatusCode)"
    
    # Step 2: Extract anti-forgery token
    $match = [regex]::Match($response.Content, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
    if ($match.Success) {
        $token = $match.Groups[1].Value
        Write-Host "Anti-forgery token found: Yes (length $($token.Length))"
    } else {
        Write-Host "Anti-forgery token: NOT FOUND"
        Write-Host "Page content snippet:"
        Write-Host $response.Content.Substring(0, [Math]::Min(1000, $response.Content.Length))
        exit 1
    }
    
    # Step 3: Attempt login
    $body = @{
        LoginId = "admin"
        LoginPassword = "@dmin"
        __RequestVerificationToken = $token
    }
    
    $loginResp = Invoke-WebRequest -Uri "$baseUrl/UserLogin/Index" -Method POST -Body $body -UseBasicParsing -WebSession $session -MaximumRedirection 5
    Write-Host "POST login: Status $($loginResp.StatusCode)"
    Write-Host "Final URL: $($loginResp.BaseResponse.ResponseUri)"
    
    if ($loginResp.Content -match "Login failed") {
        Write-Host "RESULT: Login FAILED - credentials rejected"
    } elseif ($loginResp.Content -match "Dashboard|dashboard") {
        Write-Host "RESULT: Login SUCCESS - reached dashboard"
    } elseif ($loginResp.Content -match "Access denied") {
        Write-Host "RESULT: Account LOCKED"
    } else {
        Write-Host "RESULT: Unknown page - first 500 chars:"
        Write-Host $loginResp.Content.Substring(0, [Math]::Min(500, $loginResp.Content.Length))
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        Write-Host "Response Status: $($_.Exception.Response.StatusCode)"
    }
}
