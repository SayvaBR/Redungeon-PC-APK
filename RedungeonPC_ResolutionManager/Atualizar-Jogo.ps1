param([switch]$IncluirAndroid)
$ErrorActionPreference = 'Stop'
$source = $PSScriptRoot
$delivery = Join-Path (Split-Path $source -Parent) 'JOGO REDUNGEON'
# MGCB cannot resolve HLSL includes under an accented Windows username.
# This is disposable compiler workspace, never a second distribution.
$stage = 'C:\Dev\RedungeonBuild'
$stageFull = [IO.Path]::GetFullPath($stage)
if ($stageFull -ne 'C:\Dev\RedungeonBuild') { throw 'Unexpected staging path' }
New-Item -ItemType Directory -Force $stageFull, $delivery | Out-Null
robocopy $source $stageFull /E /XD .git .vs bin obj artifacts /XF *.zip *.apk *.log build_log.txt /NFL /NDL /NJH /NJS /NP
if ($LASTEXITCODE -ge 8) { throw 'Source copy failed' }
Push-Location $stageFull
try {
    dotnet run --project Tests/RedungeonPC.InputContractTests -c Debug
    if ($LASTEXITCODE) { throw 'Input tests failed' }
    dotnet publish RedungeonPC.csproj -c Release -r win-x64 --self-contained true -o (Join-Path $delivery 'PC')
    if ($LASTEXITCODE) { throw 'PC publish failed' }
    if ($IncluirAndroid) {
        dotnet build Android/Redungeon.Android.csproj -c Release '-p:AndroidSdkDirectory=C:/Program Files (x86)/Android/android-sdk' '-p:JavaSdkDirectory=C:/Program Files/Microsoft/jdk-17.0.20.101-hotspot'
        if ($LASTEXITCODE) { throw 'Android build failed' }
        $apk = Join-Path $stageFull 'Android/bin/Release/net10.0-android/android-arm64/com.redungeon.personal-Signed.apk'
        if (!(Test-Path -LiteralPath $apk)) { throw 'Signed APK missing' }
        New-Item -ItemType Directory -Force (Join-Path $delivery 'Android') | Out-Null
        Copy-Item -LiteralPath $apk -Destination (Join-Path $delivery 'Android/Redungeon.apk') -Force
    }
    Copy-Item -LiteralPath (Join-Path $source 'docs/entrega-atual.md') -Destination (Join-Path $delivery 'LEIA-ME.md') -Force
    Copy-Item -LiteralPath (Join-Path $source 'docs/visao-game-dev.md') -Destination (Join-Path $delivery 'VISAO-GAME-DEV.md') -Force
    Copy-Item -LiteralPath (Join-Path $source 'docs/validacao-1.0.7.md') -Destination (Join-Path $delivery 'VALIDACAO.md') -Force
    Copy-Item -LiteralPath (Join-Path $source 'docs/wolf-test-checklist.md') -Destination (Join-Path $delivery 'wolf-test-checklist.md') -Force
    Write-Host "Entrega atualizada em $delivery"
}
finally { Pop-Location }
# Keep failed builds for diagnosis; successful builds leave only the delivery.
if (([IO.Path]::GetFullPath($stageFull)) -eq 'C:\Dev\RedungeonBuild') {
    Remove-Item -LiteralPath $stageFull -Recurse -Force
}
