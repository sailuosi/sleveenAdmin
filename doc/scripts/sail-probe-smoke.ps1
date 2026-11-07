# Sail 产品缺口接线烟雾探针（需已登录 Cookie/Token 或本机匿名放开的开发环境）
# 用法: .\sail-probe-smoke.ps1 -BaseUrl http://localhost:5xxx
param(
  [string]$BaseUrl = "http://localhost:5000",
  [hashtable]$Headers = @{}
)

$ErrorActionPreference = "Stop"
function Hit([string]$Method, [string]$Path) {
  $uri = "$BaseUrl$Path"
  Write-Host "→ $Method $uri"
  try {
    $r = Invoke-RestMethod -Method $Method -Uri $uri -Headers $Headers
    Write-Host ($r | ConvertTo-Json -Depth 6 -Compress)
    return $r
  } catch {
    Write-Warning $_.Exception.Message
    return $null
  }
}

Write-Host "=== ProbeOperationHandlers ==="
Hit GET "/api/Wf2Xml/ProbeOperationHandlers"

Write-Host "=== WfBizForm/Probe ==="
Hit GET "/api/WfBizForm/Probe"

Write-Host "Done. 对照 doc/sail/联调清单-产品缺口.md 继续人工场景。"
