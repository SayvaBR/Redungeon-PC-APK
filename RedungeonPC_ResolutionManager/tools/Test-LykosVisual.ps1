param([string]$Stage='C:\Dev\RedungeonBuild', [string[]]$Scenarios=@('lykos-shop','lykos-selector','lykos-purchase','lykos-all-unlocked','lykos-serpent','lykos-ice-mimic','lykos-web','lykos-transform','lykos-wolf-fall','lykos-moon','lykos-expiry','lykos-level2','lykos-death-impact','lykos-death-fall','lykos-death-zap','lykos-death-fire','lykos-death-crush','lykos-death-bolt','lykos-return','lykos-revive','frost'))
$ErrorActionPreference='Stop'
$output=Join-Path $Stage 'qa'
New-Item -ItemType Directory -Force $output | Out-Null
foreach($scenario in $Scenarios) {
    $capture=if($scenario -eq 'lykos-purchase'){240}elseif($scenario -eq 'lykos-transform'){84}elseif($scenario -eq 'lykos-expiry'){530}elseif($scenario -eq 'lykos-death-zap'){96}elseif($scenario -eq 'lykos-death-fire'){100}elseif($scenario.StartsWith('lykos-death-')){108}else{180}
    if($scenario -in @('lykos-return','lykos-revive')) { $capture=430 }
    $env:REDUNGEON_QA_VISUAL=Join-Path $output "$scenario.png"
    $env:REDUNGEON_QA_SCREEN=$scenario
    $env:REDUNGEON_QA_CAPTURE_FRAME="$capture"
    $env:REDUNGEON_QA_PORTRAIT=if($scenario -eq 'frost'){'1'}else{'0'}
    $exe=Join-Path $Stage 'bin/Debug/net9.0/RedungeonPC.exe'
    $args="--qa-record $output\$scenario.jsonl --qa-max-frames 900"
    $p=Start-Process -FilePath $exe -ArgumentList $args -WorkingDirectory (Split-Path $exe) -WindowStyle Hidden -PassThru -RedirectStandardOutput "$output\$scenario.stdout.log" -RedirectStandardError "$output\$scenario.stderr.log"
    $deadline=[DateTime]::UtcNow.AddSeconds(45)
    while(!$p.WaitForExit(250)) {
        if((Get-Content "$output\$scenario.stdout.log" -Raw -ErrorAction SilentlyContinue) -match 'ERRO CAPTURADO') {
            $p.Kill(); Get-Content "$output\$scenario.stdout.log" -Tail 20; throw "QA exception: $scenario"
        }
        if([DateTime]::UtcNow -gt $deadline) { $p.Kill(); throw "QA timeout: $scenario" }
    }
    if($p.ExitCode -ne 0 -or !(Test-Path -LiteralPath $env:REDUNGEON_QA_VISUAL)) { Get-Content "$output\$scenario.stderr.log" -Tail 18; throw "QA failed: $scenario exit=$($p.ExitCode)" }
    Write-Host "PASS $scenario (capture $capture)"
}
Remove-Item Env:REDUNGEON_QA_VISUAL,Env:REDUNGEON_QA_SCREEN,Env:REDUNGEON_QA_CAPTURE_FRAME,Env:REDUNGEON_QA_PORTRAIT
