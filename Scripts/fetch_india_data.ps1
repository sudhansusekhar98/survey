$ErrorActionPreference = "Stop"

# Fetch Indian states
Write-Host "Fetching Indian states..."
$statesBody = @{ country = "India" } | ConvertTo-Json
$statesResp = Invoke-RestMethod -Uri "https://countriesnow.space/api/v0.1/countries/states" -Method POST -Body $statesBody -ContentType "application/json"

$states = $statesResp.data.states | Sort-Object name
Write-Host "Found $($states.Count) states"

$result = @{}

foreach ($state in $states) {
    Write-Host "Fetching cities for $($state.name)..."
    try {
        $citiesBody = @{ country = "India"; state = $state.name } | ConvertTo-Json
        $citiesResp = Invoke-RestMethod -Uri "https://countriesnow.space/api/v0.1/countries/state/cities" -Method POST -Body $citiesBody -ContentType "application/json"
        
        $cities = @($citiesResp.data | Sort-Object)
        $result[$state.name] = @{
            state_code = $state.state_code
            cities = $cities
        }
        Write-Host "  -> $($cities.Count) cities"
        Start-Sleep -Milliseconds 200
    } catch {
        Write-Host "  -> ERROR: $($_.Exception.Message)"
        $result[$state.name] = @{
            state_code = $state.state_code
            cities = @()
        }
    }
}

# Save to JSON
$outputPath = "d:\VL Access\Survey\CODES\Survey\survey\wwwroot\data\india_states_cities.json"
$outputDir = Split-Path $outputPath
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

$result | ConvertTo-Json -Depth 5 | Set-Content -Path $outputPath -Encoding UTF8
Write-Host "`nSaved to $outputPath"
Write-Host "Total states: $($result.Keys.Count)"
