# Sail 联调核心链：登录 → Probe → Biz* Start→Approve → Leave H1/H2 → UAT G/A/B/C → Extra/R2/体验包烟
# 用法: .\sail-uat-core.ps1 -BaseUrl http://localhost:5000
param(
  [string]$BaseUrl = "http://localhost:5000",
  [string]$UserName = "admin",
  [string]$Password = "123456"
)

$ErrorActionPreference = "Stop"
$script:FailCount = 0

function Get-CaptchaCode([string]$img) {
  $b64 = ($img -split ',', 2)[1]
  $svg = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($b64))
  if ($svg -match '>([A-Z0-9]{4})<') { return $Matches[1] }
  throw "cannot parse captcha"
}

function Login {
  $cap = Invoke-RestMethod -Method GET -Uri "$BaseUrl/api/auth/captcha"
  $p = $cap.data
  $code = Get-CaptchaCode $p.captchaImage
  $body = [Text.Encoding]::UTF8.GetBytes((@{
        userName = $UserName; password = $Password
        captchaId = $p.captchaId; captchaCode = $code
      } | ConvertTo-Json))
  $login = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/auth/login" `
    -ContentType 'application/json; charset=utf-8' -Body $body
  return $login.data.token
}

function HitJson([hashtable]$Headers, [string]$Method, [string]$Path, [string]$JsonBody = $null) {
  $uri = "$BaseUrl$Path"
  Write-Host "→ $Method $Path"
  $params = @{ Method = $Method; Uri = $uri; Headers = $Headers; TimeoutSec = 60 }
  if ($JsonBody) {
    $params.ContentType = 'application/json; charset=utf-8'
    $params.Body = [Text.Encoding]::UTF8.GetBytes($JsonBody)
  }
  return Invoke-RestMethod @params
}

function Assert-Ok([string]$label, [bool]$ok, [string]$detail = "") {
  if ($ok) { Write-Host "OK $label $detail" }
  else { Write-Host "FAIL $label $detail"; $script:FailCount++ }
}

Write-Host "=== login ==="
$token = Login
$h = @{ Authorization = "Bearer $token" }

Write-Host "=== P1 ProbeOperationHandlers ==="
$p1 = HitJson $h GET "/api/Wf2Xml/ProbeOperationHandlers"
Write-Host ($p1.extraData | ConvertTo-Json -Compress)

Write-Host "=== P2 WfBizForm/Probe ==="
$p2 = HitJson $h GET "/api/WfBizForm/Probe"
Write-Host ($p2.extraData | ConvertTo-Json -Compress)

Write-Host "=== init-db (UAT seeds) ==="
$null = Invoke-RestMethod -Method POST -Uri "$BaseUrl/api/sail/Probe/init-db" -TimeoutSec 60

Write-Host "=== Biz* Start→Approve (seed hello) ==="
$bizPk = "uat-core-" + [guid]::NewGuid().ToString("N").Substring(0, 8)
$start = HitJson $h POST "/api/Wf2Xml/StartProcess" (@{
    ProcessId = "sail-seed-hello"; Version = "1"
    BizChannel = "UAT"; BizPK = $bizPk
    UserId = $UserName; UserName = $UserName
    NextActivityPerformers = @{ UserTask_1 = @(@{ UserId = $UserName; UserName = $UserName }) }
  } | ConvertTo-Json -Depth 6)
$pi = $start.extraData.processInstanceId
Write-Host "start status=$($start.status) pi=$pi"
$wb = HitJson $h POST "/api/Wf2Xml/GetApprovalWorkbench" (@{ ProcessInstanceId = $pi } | ConvertTo-Json)
$tid = $wb.entity.task.taskId
$aid = $wb.entity.task.activityInstanceId
$ap = HitJson $h POST "/api/Wf2Xml/ButtonsOperation" (@{
    ButtonId = "Approve"
    Runner = @{
      TaskId = $tid; ActivityInstanceId = $aid
      ProcessId = "sail-seed-hello"; Version = "1"
      BizChannel = "UAT"; BizPK = $bizPk
      Conditions = @{ comment = "agree"; signPicUrl = "https://example.com/s.png" }
      NextActivityPerformers = @{ EndEvent_1 = @() }
    }
  } | ConvertTo-Json -Depth 8)
Write-Host "approve status=$($ap.status) msg=$($ap.message)"
$wb2 = HitJson $h POST "/api/Wf2Xml/GetApprovalWorkbench" (@{ ProcessInstanceId = $pi } | ConvertTo-Json)
$opJson = $wb2.entity.activityOpinions | ConvertTo-Json -Compress
Write-Host "state=$($wb2.entity.processInstance.processState) opinions=$opJson"
# ActivityOpinions 已按 ActivityInstanceId 解析（底层变量名 __sail.act.comment.|sign.）
Assert-Ok "Biz* opinions by actId" ($opJson -match 'activityInstanceId' -and $opJson -match 'agree') $opJson

Write-Host "=== H1/H2 Leave Create→Approve→Status=2 ==="
$lv = HitJson $h POST "/api/WfLeave/Create" (@{
    ProcessId = "sail-seed-hello"; Version = "1"
    Days = 1; Reason = "UAT-H2"
    StartDate = (Get-Date).Date.ToString("yyyy-MM-ddTHH:mm:ss")
    EndDate = (Get-Date).Date.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss")
  } | ConvertTo-Json)
$leaveId = $lv.entity.id
$leavePi = $lv.entity.processInstanceId
Assert-Ok "H1 Leave Create" ($lv.status -eq 1 -and $leavePi) "status=$($lv.status) pi=$leavePi"
$ltasks = HitJson $h POST "/api/Wf2Xml/QueryReadyTasks" '{"PageIndex":1,"PageSize":50}'
$lhit = @($ltasks.entity | Where-Object { $_.processInstanceId -eq $leavePi })[0]
$lap = HitJson $h POST "/api/Wf2Xml/ButtonsOperation" (@{
    ButtonId = "Approve"
    Runner = @{
      TaskId = $lhit.taskId; ActivityInstanceId = $lhit.activityInstanceId
      ProcessId = "sail-seed-hello"; Version = "1"
      BizChannel = "SailLeave"; BizPK = $leaveId
      Conditions = @{ comment = "同意" }
      NextActivityPerformers = @{ EndEvent_1 = @() }
    }
  } | ConvertTo-Json -Depth 8)
Write-Host "leave approve status=$($lap.status) msg=$($lap.message)"
$lg = HitJson $h GET "/api/WfLeave/Get/$leaveId"
Assert-Ok "H2 Leave Status=通过(2)" ($lg.entity.status -eq 2) "status=$($lg.entity.status)"

Write-Host "=== G: UAT-G Start→Approve A→SendBack from B ==="
$gPk = "uat-g-" + [guid]::NewGuid().ToString("N").Substring(0, 8)
$gst = HitJson $h POST "/api/Wf2Xml/StartProcess" (@{
    ProcessId = "sail-uat-g"; Version = "1"
    BizChannel = "UAT"; BizPK = $gPk
    NextActivityPerformers = @{ UserTask_A = @(@{ UserId = $UserName; UserName = $UserName }) }
  } | ConvertTo-Json -Depth 6)
$gPi = $gst.extraData.processInstanceId
Write-Host "G start status=$($gst.status) pi=$gPi"
if ($gst.status -eq 1 -and $gPi) {
  $gwb = HitJson $h POST "/api/Wf2Xml/GetApprovalWorkbench" (@{ ProcessInstanceId = $gPi } | ConvertTo-Json)
  $gap = HitJson $h POST "/api/Wf2Xml/ButtonsOperation" (@{
      ButtonId = "Approve"
      Runner = @{
        TaskId = $gwb.entity.task.taskId
        ActivityInstanceId = $gwb.entity.task.activityInstanceId
        ProcessId = "sail-uat-g"; Version = "1"
        BizChannel = "UAT"; BizPK = $gPk
        Conditions = @{ comment = "ok-a" }
        NextActivityPerformers = @{ UserTask_B = @(@{ UserId = $UserName; UserName = $UserName }) }
      }
    } | ConvertTo-Json -Depth 8)
  Write-Host "G approveA status=$($gap.status)"
  $gwb2 = HitJson $h POST "/api/Wf2Xml/GetApprovalWorkbench" (@{ ProcessInstanceId = $gPi } | ConvertTo-Json)
  $gsb = HitJson $h POST "/api/Wf2Xml/ButtonsOperation" (@{
      ButtonId = "SendBack"
      Runner = @{
        TaskId = $gwb2.entity.task.taskId
        ActivityInstanceId = $gwb2.entity.task.activityInstanceId
        ProcessId = "sail-uat-g"; Version = "1"
        BizChannel = "UAT"; BizPK = $gPk
        NextActivityPerformers = @{ UserTask_A = @(@{ UserId = $UserName; UserName = $UserName }) }
      }
    } | ConvertTo-Json -Depth 8)
  Write-Host "G sendBack status=$($gsb.status) msg=$($gsb.message) reason=$($gsb.extraData.reasonCode)"
  Assert-Ok "G SendBack" ($gsb.status -eq 1) "status=$($gsb.status)"
}

Write-Host "=== A: UAT-A empty performers → AssigneeEmpty ==="
$aPk = "uat-a-" + [guid]::NewGuid().ToString("N").Substring(0, 8)
$ast = HitJson $h POST "/api/Wf2Xml/StartProcess" (@{
    ProcessId = "sail-uat-a"; Version = "1"
    BizChannel = "UAT"; BizPK = $aPk
  } | ConvertTo-Json -Depth 6)
$aReason = "$($ast.message) $($ast.extraData.reasonCode)"
Write-Host "A start status=$($ast.status) detail=$aReason"
Assert-Ok "A1 empty/pick fail" ($ast.status -ne 1 -or $aReason -match 'AssigneeEmpty|Performer|ApproverPick|下一跳') $aReason
# 带办理人时可看 NodePolicy 超时/空人配置
$ast2 = HitJson $h POST "/api/Wf2Xml/StartProcess" (@{
    ProcessId = "sail-uat-a"; Version = "1"
    BizChannel = "UAT"; BizPK = ($aPk + "-ok")
    NextActivityPerformers = @{ UserTask_1 = @(@{ UserId = $UserName; UserName = $UserName }) }
  } | ConvertTo-Json -Depth 6)
if ($ast2.status -eq 1) {
  $aTasks = HitJson $h POST "/api/Wf2Xml/QueryReadyTasks" '{"PageIndex":1,"PageSize":50}'
  $aHit = @($aTasks.entity | Where-Object { $_.processInstanceId -eq $ast2.extraData.processInstanceId })[0]
  $awb = HitJson $h POST "/api/Wf2Xml/GetApprovalWorkbench" (@{ TaskId = $aHit.taskId } | ConvertTo-Json)
  $np = $awb.entity.nodePolicy | ConvertTo-Json -Compress
  Write-Host "A nodePolicy=$np"
  Assert-Ok "A policy timeout/empty" (
    $awb.entity.nodePolicy.timeoutHandlerEnable -eq $true -or
    $awb.entity.nodePolicy.assignEmptyHandlerType -eq 2
  ) $np
}

Write-Host "=== C: UAT-C fieldModes (by TaskId) ==="
$cPk = "uat-c-" + [guid]::NewGuid().ToString("N").Substring(0, 8)
$cst = HitJson $h POST "/api/Wf2Xml/StartProcess" (@{
    ProcessId = "sail-uat-c"; Version = "1"
    BizChannel = "UAT"; BizPK = $cPk
    NextActivityPerformers = @{ UserTask_1 = @(@{ UserId = $UserName; UserName = $UserName }) }
  } | ConvertTo-Json -Depth 6)
if ($cst.status -eq 1) {
  $cTasks = HitJson $h POST "/api/Wf2Xml/QueryReadyTasks" '{"PageIndex":1,"PageSize":50}'
  $cHit = @($cTasks.entity | Where-Object { $_.processInstanceId -eq $cst.extraData.processInstanceId })[0]
  $cwb = HitJson $h POST "/api/Wf2Xml/GetApprovalWorkbench" (@{ TaskId = $cHit.taskId } | ConvertTo-Json)
  $fm = $cwb.entity.fieldModes | ConvertTo-Json -Compress
  Write-Host "fieldModes=$fm signEnable=$($cwb.entity.nodePolicy.signEnable)"
  Assert-Ok "C fieldModes" ($fm -match 'editable' -and $fm -match 'readonly' -and $fm -match 'hidden' -and $fm -match 'masked') $fm
  Assert-Ok "R2 signEnable" ($cwb.entity.nodePolicy.signEnable -eq $true) "signEnable=$($cwb.entity.nodePolicy.signEnable)"
}

Write-Host "=== B: UAT-B Terminate/Discard (by TaskId) ==="
$bPk = "uat-b-" + [guid]::NewGuid().ToString("N").Substring(0, 8)
$bst = HitJson $h POST "/api/Wf2Xml/StartProcess" (@{
    ProcessId = "sail-uat-b"; Version = "1"
    BizChannel = "UAT"; BizPK = $bPk
    NextActivityPerformers = @{ UserTask_1 = @(@{ UserId = $UserName; UserName = $UserName }) }
  } | ConvertTo-Json -Depth 6)
if ($bst.status -eq 1) {
  $bTasks = HitJson $h POST "/api/Wf2Xml/QueryReadyTasks" '{"PageIndex":1,"PageSize":50}'
  $bHit = @($bTasks.entity | Where-Object { $_.processInstanceId -eq $bst.extraData.processInstanceId })[0]
  $bwb = HitJson $h POST "/api/Wf2Xml/GetApprovalWorkbench" (@{ TaskId = $bHit.taskId } | ConvertTo-Json)
  $btns = @($bwb.entity.buttonsSetting | ForEach-Object { "$($_.id):$($_.enable)" }) -join ','
  Write-Host "buttons=$btns"
  Assert-Ok "B Terminate/Discard" ($btns -match 'Terminate:True' -and $btns -match 'Discard:True') $btns
  $term = HitJson $h POST "/api/Wf2Xml/ButtonsOperation" (@{
      ButtonId = "Terminate"
      Runner = @{
        TaskId = $bHit.taskId; ActivityInstanceId = $bHit.activityInstanceId
        ProcessInstanceId = $bst.extraData.processInstanceId
        ProcessId = "sail-uat-b"; Version = "1"
        BizChannel = "UAT"; BizPK = $bPk
        Conditions = @{ comment = "uat-terminate" }
      }
    } | ConvertTo-Json -Depth 8)
  Write-Host "Terminate status=$($term.status) msg=$($term.message)"
  Assert-Ok "B Terminate exec" ($term.status -eq 1) "status=$($term.status)"
}

Write-Host "=== Extra E1 WithdrawNotAllowed ==="
$e1 = HitJson $h POST "/api/Wf2Xml/ButtonsOperation" (@{
    ButtonId = "Withdraw"
    Runner = @{
      ProcessId = "sail-seed-hello"; Version = "1"
      BizChannel = "UAT"; BizPK = "e1-probe"
      TaskId = [guid]::NewGuid().ToString()
    }
  } | ConvertTo-Json -Depth 6)
$e1r = "$($e1.extraData.reasonCode) $($e1.message)"
Write-Host "E1=$e1r"
Assert-Ok "E1 Withdraw gate" ($e1r -match 'WithdrawNotAllowed|NotAllowed|撤回') $e1r

Write-Host "=== Experience: phrases / category sort ==="
$ph = HitJson $h GET "/api/WfApprovalPhrase/MyList"
Assert-Ok "R3 phrases" ($null -ne $ph) "count=$(@($ph.entity).Count)"
$cats = HitJson $h GET "/api/WfCategory/List"
$flat = @($cats.entity)
if ($flat.Count -ge 2) {
  $ids = @($flat[0].id, $flat[1].id)
  $sr = HitJson $h POST "/api/WfCategory/UpdateSortBatch" (@{ ids = $ids } | ConvertTo-Json)
  Assert-Ok "category sort" ($sr.status -eq 1 -or "$($sr.message)" -match 'sorted') "$($sr.message)"
} else {
  Write-Host "SKIP category sort (need >=2 cats)"
}

Write-Host "=== Done. fails=$script:FailCount ==="
if ($script:FailCount -gt 0) { exit 1 }
