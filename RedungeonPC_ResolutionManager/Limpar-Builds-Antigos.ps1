param([switch]$Aplicar)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent)).TrimEnd('\')
if (!(Test-Path -LiteralPath (Join-Path $root 'JOGO REDUNGEON/PC/RedungeonPC.exe')) -or
    !(Test-Path -LiteralPath (Join-Path $root 'JOGO REDUNGEON/Android/Redungeon.apk'))) {
    throw 'A entrega atual deve existir antes da limpeza.'
}
# Explicit generated paths only. No source, .git, settings or player saves.
$relative = @(
 'RedungeonPC_ResolutionManager/artifacts/focus-build',
 'RedungeonPC_ResolutionManager/artifacts/pc-controls-win-x64',
 'RedungeonPC_ResolutionManager/artifacts/qa-ui',
 'RedungeonPC_ResolutionManager/artifacts/ui-tests',
 'RedungeonPC_ResolutionManager/bin',
 'RedungeonPC_ResolutionManager/obj',
 'RedungeonPC_ResolutionManager/Content/bin',
 'RedungeonPC_ResolutionManager/Content/obj',
 'RedungeonPC_ResolutionManager/Redungeon-Android.apk',
 'RedungeonPC_ResolutionManager/Redungeon-Android-controles.apk',
 'RedungeonPC_ResolutionManager/Redungeon-PC-Controles.zip',
 'Redungeon-Android.apk',
 'RedungeonPC_ResolutionManager.zip'
)
$targets = foreach ($relativePath in $relative) {
    $full = [IO.Path]::GetFullPath((Join-Path $root $relativePath))
    if (!$full.StartsWith($root + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Path outside workspace' }
    if (Test-Path -LiteralPath $full) {
        $item = Get-Item -LiteralPath $full
        if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Link not allowed: $full" }
        if ($item.PSIsContainer) {
            if (Get-ChildItem -LiteralPath $full -Recurse -Attributes ReparsePoint) { throw "Nested link: $full" }
            $bytes = (Get-ChildItem -LiteralPath $full -File -Recurse | Measure-Object Length -Sum).Sum
        } else { $bytes = $item.Length }
        [pscustomobject]@{ Path = $full; Bytes = $bytes }
    }
}
$targets | Select-Object Path, @{Name='MiB';Expression={[math]::Round($_.Bytes / 1MB, 1)}} | Format-Table -AutoSize
Write-Host ('Total: {0:N1} MiB' -f (($targets | Measure-Object Bytes -Sum).Sum / 1MB))
if (!$Aplicar) { Write-Host 'Somente inventário. Execute novamente com -Aplicar para remover.'; return }
foreach ($target in $targets) { Remove-Item -LiteralPath $target.Path -Recurse -Force }
Write-Host 'Limpeza concluída. Código, save e entrega atual preservados.'
